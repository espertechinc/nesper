///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.@join.pollindex;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.script;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// Polling-data provider that calls a static method on a class and passed parameters, and wraps the
	/// results as events.
	/// </summary>
	public class MethodPollingViewable : HistoricalEventViewable
	{
	    private readonly bool _isStaticMethod;
	    private readonly Type _methodProviderClass;
	    private readonly MethodStreamSpec _methodStreamSpec;
	    private readonly PollExecStrategy _pollExecStrategy;
	    private readonly IList<ExprNode> _inputParameters;
	    private readonly DataCache _dataCache;
	    private readonly EventType _eventType;
        private readonly IThreadLocal<DataCache> _dataCacheThreadLocal = ThreadLocalManager.Create<DataCache>(() => null);
	    private readonly ExprEvaluatorContext _exprEvaluatorContext;

	    private SortedSet<int> _requiredStreams;
	    private ExprEvaluator[] _validatedExprNodes;
	    private StatementContext _statementContext;

	    private static readonly EventBean[][] NULL_ROWS;
	    static MethodPollingViewable()
        {
	        NULL_ROWS = new EventBean[1][];
	        NULL_ROWS[0] = new EventBean[1];
	    }

	    private static readonly PollResultIndexingStrategy iteratorIndexingStrategy = new ProxyPollResultIndexingStrategy()
	    {
	        ProcIndex = (pollResult, isActiveCache, statementContext) =>
                new EventTable[] { new UnindexedEventTableList(pollResult, -1) },
	        ProcToQueryPlan = () =>
                typeof(MethodPollingViewable).Name + " unindexed"
	    };

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="methodStreamSpec">defines class and method names</param>
	    /// <param name="myStreamNumber">is the stream number</param>
	    /// <param name="inputParameters">the input parameter expressions</param>
	    /// <param name="pollExecStrategy">the execution strategy</param>
	    /// <param name="dataCache">the cache to use</param>
	    /// <param name="eventType">the type of event returned</param>
	    /// <param name="exprEvaluatorContext">expression evaluation context</param>
	    public MethodPollingViewable(
	        bool isStaticMethod,
	        Type methodProviderClass,
	        MethodStreamSpec methodStreamSpec,
	        int myStreamNumber,
	        IList<ExprNode> inputParameters,
	        PollExecStrategy pollExecStrategy,
	        DataCache dataCache,
	        EventType eventType,
	        ExprEvaluatorContext exprEvaluatorContext)
	    {
	        _isStaticMethod = isStaticMethod;
	        _methodProviderClass = methodProviderClass;
	        _methodStreamSpec = methodStreamSpec;
	        _inputParameters = inputParameters;
	        _pollExecStrategy = pollExecStrategy;
	        _dataCache = dataCache;
	        _eventType = eventType;
	        _exprEvaluatorContext = exprEvaluatorContext;
	    }

	    public void Stop()
	    {
	        _pollExecStrategy.Dispose();
	        _dataCache.Dispose();
	    }

	    public IThreadLocal<DataCache> DataCacheThreadLocal
	    {
	        get { return _dataCacheThreadLocal; }
	    }

	    public DataCache OptionalDataCache
	    {
	        get { return _dataCache; }
	    }

	    public void Validate(EngineImportService engineImportService, StreamTypeService streamTypeService, TimeProvider timeProvider, VariableService variableService, TableService tableService, ScriptingService scriptingService, ExprEvaluatorContext exprEvaluatorContext, ConfigurationInformation configSnapshot, SchedulingService schedulingService, string engineURI, IDictionary<int, IList<ExprNode>> sqlParameters, EventAdapterService eventAdapterService, StatementContext statementContext)
        {
	        _statementContext = statementContext;

	        // validate and visit
	        var validationContext = new ExprValidationContext(
	            streamTypeService, engineImportService,
                statementContext.StatementExtensionServicesContext, null, 
                timeProvider, variableService, tableService,
	            exprEvaluatorContext, eventAdapterService, 
                statementContext.StatementName, 
                statementContext.StatementId,
	            statementContext.Annotations, null, 
                scriptingService, false, false, true, false, null, false);
	        var visitor = new ExprNodeIdentifierVisitor(true);
	        IList<ExprNode> validatedInputParameters = new List<ExprNode>();
	        foreach (var exprNode in _inputParameters) {
	            var validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.METHODINVJOIN, exprNode, validationContext);
	            validatedInputParameters.Add(validated);
	            validated.Accept(visitor);
	        }

	        // determine required streams
	        _requiredStreams = new SortedSet<int>();
	        foreach (var identifier in visitor.ExprProperties)
	        {
	            _requiredStreams.Add(identifier.First);
	        }

	        ExprNodeUtilResolveExceptionHandler handler = new ProxyExprNodeUtilResolveExceptionHandler
	        {
	            ProcHandle = (e) =>  {
	                if (_inputParameters.Count == 0)
	                {
	                    return new ExprValidationException("Method footprint does not match the number or type of expression parameters, expecting no parameters in method: " + e.Message);
	                }
	                var resultTypes = ExprNodeUtility.GetExprResultTypes(validatedInputParameters);
	                return new ExprValidationException("Method footprint does not match the number or type of expression parameters, expecting a method where parameters are typed '" +
	                        TypeHelper.GetParameterAsString(resultTypes) + "': " + e.Message);
	            },
	        };

	        var desc = ExprNodeUtility.ResolveMethodAllowWildcardAndStream(
	            _methodProviderClass.FullName, _isStaticMethod ? null : _methodProviderClass,
	            _methodStreamSpec.MethodName, validatedInputParameters, engineImportService, eventAdapterService,
	            statementContext.StatementId,
	            false, null, handler, _methodStreamSpec.MethodName, tableService);
	        _validatedExprNodes = desc.ChildEvals;
	    }

	    public EventTable[][] Poll(EventBean[][] lookupEventsPerStream, PollResultIndexingStrategy indexingStrategy, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        DataCache localDataCache = _dataCacheThreadLocal.GetOrCreate();
	        var strategyStarted = false;

	        var resultPerInputRow = new EventTable[lookupEventsPerStream.Length][];

	        // Get input parameters for each row
	        for (var row = 0; row < lookupEventsPerStream.Length; row++)
	        {
	            var lookupValues = new object[_inputParameters.Count];
                var evaluateParams = new EvaluateParams(lookupEventsPerStream[row], true, exprEvaluatorContext);

	            // Build lookup keys
	            for (var valueNum = 0; valueNum < _inputParameters.Count; valueNum++)
	            {
	                var parameterValue = _validatedExprNodes[valueNum].Evaluate(evaluateParams);
	                lookupValues[valueNum] = parameterValue;
	            }

	            EventTable[] result = null;

	            // try the threadlocal iteration cache, if set
	            if (localDataCache != null)
	            {
	                result = localDataCache.GetCached(lookupValues);
	            }

	            // try the connection cache
	            if (result == null)
	            {
	                result = _dataCache.GetCached(lookupValues);
	                if ((result != null) && (localDataCache != null))
	                {
	                    localDataCache.PutCached(lookupValues, result);
	                }
	            }

	            if (result != null)     // found in cache
	            {
	                resultPerInputRow[row] = result;
	            }
	            else        // not found in cache, get from actual polling (db query)
	            {
	                try
	                {
	                    if (!strategyStarted)
	                    {
	                        _pollExecStrategy.Start();
	                        strategyStarted = true;
	                    }

	                    // Poll using the polling execution strategy and lookup values
	                    var pollResult = _pollExecStrategy.Poll(lookupValues, exprEvaluatorContext);

	                    // index the result, if required, using an indexing strategy
	                    var indexTable = indexingStrategy.Index(pollResult, _dataCache.IsActive, _statementContext);

	                    // assign to row
	                    resultPerInputRow[row] = indexTable;

	                    // save in cache
	                    _dataCache.PutCached(lookupValues, indexTable);

	                    if (localDataCache != null)
	                    {
                            localDataCache.PutCached(lookupValues, indexTable);
	                    }
	                }
	                catch (EPException ex)
	                {
	                    if (strategyStarted)
	                    {
	                        _pollExecStrategy.Done();
	                    }
	                    throw ex;
	                }
	            }
	        }

	        if (strategyStarted)
	        {
	            _pollExecStrategy.Done();
	        }

	        return resultPerInputRow;
	    }

	    public View AddView(View view)
	    {
	        view.Parent = this;
	        return view;
	    }

	    public View[] Views
	    {
	        get { return ViewSupport.EMPTY_VIEW_ARRAY; }
	    }

	    public bool RemoveView(View view)
	    {
	        throw new UnsupportedOperationException("Subviews not supported");
	    }

	    public void RemoveAllViews()
	    {
	        throw new UnsupportedOperationException("Subviews not supported");
	    }

	    public bool HasViews
	    {
	        get { return false; }
	    }

	    public EventType EventType
	    {
	        get { return _eventType; }
	    }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

	    public IEnumerator<EventBean> GetEnumerator()
	    {
	        var result = Poll(NULL_ROWS, iteratorIndexingStrategy, _exprEvaluatorContext);
            foreach (EventTable[] eventTableList in result)
            {
                foreach (EventTable eventTable in eventTableList)
                {
                    foreach (EventBean eventBean in eventTable)
                    {
                        yield return eventBean;
                    }
                }
            }
        }

	    public ICollection<int> RequiredStreams
	    {
	        get { return _requiredStreams; }
	    }

	    public bool HasRequiredStreams
	    {
	        get { return !_requiredStreams.IsEmpty(); }
	    }
	}
} // end of namespace
