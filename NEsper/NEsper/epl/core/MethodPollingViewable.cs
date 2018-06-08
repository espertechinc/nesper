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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
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
        private static readonly EventBean[][] NULL_ROWS;
        private static readonly PollResultIndexingStrategy ITERATOR_INDEXING_STRATEGY = new ProxyPollResultIndexingStrategy
        {
            ProcIndex = (pollResult, isActiveCache, statementContext)  => new EventTable[]{ new UnindexedEventTableList(pollResult, -1) },
            ProcToQueryPlan = () => typeof (MethodPollingViewable).Name + " unindexed"
        };
    
        static MethodPollingViewable()
        {
            NULL_ROWS = new EventBean[1][];
            NULL_ROWS[0] = new EventBean[1];
        }
    
        private readonly MethodStreamSpec _methodStreamSpec;
        private readonly DataCache _dataCache;
        private readonly EventType _eventType;
        private readonly IThreadLocal<DataCache> _dataCacheThreadLocal;
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly MethodPollingViewableMeta _metadata;
        private PollExecStrategy _pollExecStrategy;
        private SortedSet<int> _requiredStreams;
        private ExprEvaluator[] _validatedExprNodes;
        private StatementContext _statementContext;
    
        public MethodPollingViewable(
                MethodStreamSpec methodStreamSpec,
                DataCache dataCache,
                EventType eventType,
                ExprEvaluatorContext exprEvaluatorContext,
                MethodPollingViewableMeta metadata,
                IThreadLocalManager threadLocalManager)
        {
            _methodStreamSpec = methodStreamSpec;
            _dataCacheThreadLocal = threadLocalManager.Create<DataCache>(() => null);
            _dataCache = dataCache;
            _eventType = eventType;
            _exprEvaluatorContext = exprEvaluatorContext;
            _metadata = metadata;
        }
    
        public void Stop() {
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

        public void Validate(
            EngineImportService engineImportService,
            StreamTypeService streamTypeService,
            TimeProvider timeProvider,
            VariableService variableService,
            TableService tableService,
            ScriptingService scriptingService,
            ExprEvaluatorContext exprEvaluatorContext,
            ConfigurationInformation configSnapshot,
            SchedulingService schedulingService,
            string engineURI,
            IDictionary<int, IList<ExprNode>> sqlParameters,
            EventAdapterService eventAdapterService,
            StatementContext statementContext)
        {
            _statementContext = statementContext;
    
            // validate and visit
            var validationContext = new ExprValidationContext(
                statementContext.Container,
                streamTypeService,
                engineImportService, 
                statementContext.StatementExtensionServicesContext, null, 
                timeProvider,
                variableService, 
                tableService, 
                exprEvaluatorContext, 
                eventAdapterService,
                statementContext.StatementName, 
                statementContext.StatementId, 
                statementContext.Annotations, null, 
                statementContext.ScriptingService, 
                false, false, true, false, null, false);
            var visitor = new ExprNodeIdentifierVisitor(true);
            var validatedInputParameters = new List<ExprNode>();
            foreach (var exprNode in _methodStreamSpec.Expressions) {
                var validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.METHODINVJOIN, exprNode, validationContext);
                validatedInputParameters.Add(validated);
                validated.Accept(visitor);
            }
    
            // determine required streams
            _requiredStreams = new SortedSet<int>();
            foreach (var identifier in visitor.ExprProperties) {
                _requiredStreams.Add(identifier.First);
            }
    
            // class-based evaluation
            if (_metadata.MethodProviderClass != null) {
                // resolve actual method to use
                var handler = new ProxyExprNodeUtilResolveExceptionHandler {
                    ProcHandle = e => {
                        if (_methodStreamSpec.Expressions.Count == 0) {
                            return new ExprValidationException("Method footprint does not match the number or type of expression parameters, expecting no parameters in method: " + e.Message, e);
                        }
                        var resultTypes = ExprNodeUtility.GetExprResultTypes(validatedInputParameters);
                        return new ExprValidationException(
                            string.Format("Method footprint does not match the number or type of expression parameters, expecting a method where parameters are typed '{0}': {1}", TypeHelper.GetParameterAsString(resultTypes), e.Message), e);
                    }
                };
                var desc = ExprNodeUtility.ResolveMethodAllowWildcardAndStream(
                        _metadata.MethodProviderClass.FullName, _metadata.IsStaticMethod ? null : _metadata.MethodProviderClass,
                        _methodStreamSpec.MethodName, validatedInputParameters, engineImportService, eventAdapterService, statementContext.StatementId,
                        false, null, handler, _methodStreamSpec.MethodName, tableService, statementContext.EngineURI);
                _validatedExprNodes = desc.ChildEvals;
    
                // Construct polling strategy as a method invocation
                var invocationTarget = _metadata.InvocationTarget;
                var strategy = _metadata.Strategy;
                var variableReader = _metadata.VariableReader;
                var variableName = _metadata.VariableName;
                var methodFastClass = desc.FastMethod;
                if (_metadata.EventTypeEventBeanArray != null) {
                    _pollExecStrategy = new MethodPollingExecStrategyEventBeans(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                } else if (_metadata.OptionalMapType != null) {
                    if (desc.FastMethod.ReturnType.IsArray) {
                        _pollExecStrategy = new MethodPollingExecStrategyMapArray(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    } else if (_metadata.IsCollection) {
                        _pollExecStrategy = new MethodPollingExecStrategyMapCollection(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    } else if (_metadata.IsEnumerator) {
                        _pollExecStrategy = new MethodPollingExecStrategyMapIterator(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    } else {
                        _pollExecStrategy = new MethodPollingExecStrategyMapPlain(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    }
                } else if (_metadata.OptionalOaType != null) {
                    if (desc.FastMethod.ReturnType == typeof(Object[][])) {
                        _pollExecStrategy = new MethodPollingExecStrategyOAArray(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    } else if (_metadata.IsCollection) {
                        _pollExecStrategy = new MethodPollingExecStrategyOACollection(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    } else if (_metadata.IsEnumerator) {
                        _pollExecStrategy = new MethodPollingExecStrategyOAIterator(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    } else {
                        _pollExecStrategy = new MethodPollingExecStrategyOAPlain(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    }
                } else {
                    if (desc.FastMethod.ReturnType.IsArray) {
                        _pollExecStrategy = new MethodPollingExecStrategyPONOArray(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    } else if (_metadata.IsCollection) {
                        _pollExecStrategy = new MethodPollingExecStrategyPONOCollection(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    } else if (_metadata.IsEnumerator) {
                        _pollExecStrategy = new MethodPollingExecStrategyPONOIterator(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    } else {
                        _pollExecStrategy = new MethodPollingExecStrategyPONOPlain(eventAdapterService, methodFastClass, _eventType, invocationTarget, strategy, variableReader, variableName, variableService);
                    }
                }
            } else {
                // script-based evaluation
                _pollExecStrategy = new MethodPollingExecStrategyScript(_metadata.ScriptExpression, _metadata.EventTypeEventBeanArray);
                _validatedExprNodes = ExprNodeUtility.GetEvaluators(validatedInputParameters);
            }
        }
    
        public EventTable[][] Poll(EventBean[][] lookupEventsPerStream, PollResultIndexingStrategy indexingStrategy, ExprEvaluatorContext exprEvaluatorContext)
        {
            var localDataCache = _dataCacheThreadLocal.GetOrCreate();
            var strategyStarted = false;
    
            var resultPerInputRow = new EventTable[lookupEventsPerStream.Length][];
    
            // Get input parameters for each row
            for (var row = 0; row < lookupEventsPerStream.Length; row++)
            {
                var methodParams = new Object[_validatedExprNodes.Length];
                var evaluateParams = new EvaluateParams(lookupEventsPerStream[row], true, exprEvaluatorContext);
    
                // Build lookup keys
                for (var valueNum = 0; valueNum < _validatedExprNodes.Length; valueNum++)
                {
                    var parameterValue = _validatedExprNodes[valueNum].Evaluate(evaluateParams);
                    methodParams[valueNum] = parameterValue;
                }
    
                EventTable[] result = null;
    
                // try the threadlocal iteration cache, if set
                if (localDataCache != null) {
                    result = localDataCache.GetCached(methodParams, _methodStreamSpec.Expressions.Count);
                }
    
                // try the connection cache
                if (result == null) {
                    result = _dataCache.GetCached(methodParams, _methodStreamSpec.Expressions.Count);
                    if ((result != null) && (localDataCache != null)) {
                        localDataCache.PutCached(methodParams, _methodStreamSpec.Expressions.Count, result);
                    }
                }
    
                if (result != null) {
                    // found in cache
                    resultPerInputRow[row] = result;
                } else {
                    // not found in cache, get from actual polling (db query)
                    try {
                        if (!strategyStarted) {
                            _pollExecStrategy.Start();
                            strategyStarted = true;
                        }
    
                        // Poll using the polling execution strategy and lookup values
                        var pollResult = _pollExecStrategy.Poll(methodParams, exprEvaluatorContext);
    
                        // index the result, if required, using an indexing strategy
                        var indexTable = indexingStrategy.Index(pollResult, _dataCache.IsActive, _statementContext);
    
                        // assign to row
                        resultPerInputRow[row] = indexTable;
    
                        // save in cache
                        _dataCache.PutCached(methodParams, _methodStreamSpec.Expressions.Count, indexTable);
    
                        if (localDataCache != null) {
                            localDataCache.PutCached(methodParams, _methodStreamSpec.Expressions.Count, indexTable);
                        }
                    } catch (EPException)
                    {
                        if (strategyStarted) {
                            _pollExecStrategy.Done();
                        }
                        throw;
                    }
                }
            }
    
            if (strategyStarted) {
                _pollExecStrategy.Done();
            }
    
            return resultPerInputRow;
        }
    
        public View AddView(View view) {
            view.Parent = this;
            return view;
        }

        public View[] Views
        {
            get { return ViewSupport.EMPTY_VIEW_ARRAY; }
        }

        public bool RemoveView(View view) {
            throw new UnsupportedOperationException("Subviews not supported");
        }
    
        public void RemoveAllViews() {
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
            var result = Poll(NULL_ROWS, ITERATOR_INDEXING_STRATEGY, _exprEvaluatorContext);
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
