///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.epl.join.pollindex;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.script;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Implements a poller viewable that uses a polling strategy, a cache and
    /// some input parameters extracted from event streams to perform the polling.
    /// </summary>

    public class DatabasePollingViewable : HistoricalEventViewable
    {
        private readonly int _myStreamNumber;
        private readonly PollExecStrategy _pollExecStrategy;
        private readonly IList<String> _inputParameters;
        private readonly DataCache _dataCache;
        private readonly EventType _eventType;

        private readonly IThreadLocal<DataCache> _dataCacheThreadLocal;

        private ExprEvaluator[] _evaluators;
        private ICollection<int> _subordinateStreams;
        private ExprEvaluatorContext _exprEvaluatorContext;
        private StatementContext _statementContext;

        private static readonly EventBean[][] NULL_ROWS;

        static DatabasePollingViewable()
        {
            NULL_ROWS = new EventBean[1][];
            NULL_ROWS[0] = new EventBean[1];
        }



        private class IteratorIndexingStrategy : PollResultIndexingStrategy
        {
            public EventTable[] Index(IList<EventBean> pollResult, bool isActiveCache, StatementContext statementContext)
            {
                return new EventTable[]
                {
                    new UnindexedEventTableList(pollResult, -1)
                };
            }

            public String ToQueryPlan()
            {
                return GetType().Name + " unindexed";
            }
        }

        private static readonly PollResultIndexingStrategy ITERATOR_INDEXING_STRATEGY = new IteratorIndexingStrategy();

        /// <summary>
        /// Returns the a set of stream numbers of all streams that provide
        /// property values in any of the parameter expressions to the stream.
        /// </summary>
        /// <value></value>
        /// <returns>set of stream numbers</returns>
        public ICollection<int> RequiredStreams
        {
            get { return _subordinateStreams; }
        }

        /// <summary>
        /// Returns true if the parameters expressions to the historical require
        /// other stream's data, or false if there are no parameters or all
        /// parameter expressions are only contants and variables without
        /// properties of other stream events.
        /// </summary>
        /// <value></value>
        /// <returns>indicator whether properties are required for parameter evaluation</returns>
        public bool HasRequiredStreams
        {
            get { return _subordinateStreams.IsNotEmpty(); }
        }

        /// <summary>
        /// Historical views are expected to provide a thread-local data cache
        /// for use in keeping row (<seealso cref="EventBean" /> references) returned during
        /// iteration stable, since the concept of a primary key does not exist.
        /// </summary>
        /// <value></value>
        /// <returns>thread-local cache, can be null for any thread to indicate no caching</returns>
        public IThreadLocal<DataCache> DataCacheThreadLocal
        {
            get { return _dataCacheThreadLocal; }
        }

        /// <summary>
        /// Returns the optional data cache.
        /// </summary>
        /// <value>
        /// The optional data cache.
        /// </value>
        public DataCache OptionalDataCache
        {
            get { return _dataCache; }
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="myStreamNumber">is the stream number of the view</param>
        /// <param name="inputParameters">are the event property names providing input parameter keys</param>
        /// <param name="pollExecStrategy">is the strategy to use for retrieving results</param>
        /// <param name="dataCache">is looked up before using the strategy</param>
        /// <param name="eventType">is the type of events generated by the view</param>
        /// <param name="threadLocalManager">The thread local manager.</param>

        public DatabasePollingViewable(
            int myStreamNumber,
            IList<String> inputParameters,
            PollExecStrategy pollExecStrategy,
            DataCache dataCache,
            EventType eventType,
            IThreadLocalManager threadLocalManager)
        {
            _dataCacheThreadLocal = threadLocalManager.Create<DataCache>(() => null);
            _myStreamNumber = myStreamNumber;
            _inputParameters = inputParameters;
            _pollExecStrategy = pollExecStrategy;
            _dataCache = dataCache;
            _eventType = eventType;
        }

        /// <summary>
        /// Stops the view
        /// </summary>
        public virtual void Stop()
        {
            _pollExecStrategy.Dispose();
            _dataCache.Dispose();
        }

        /// <summary>
        /// Validate the view.
        /// </summary>
        /// <param name="engineImportService">The engine import service.</param>
        /// <param name="streamTypeService">supplies the types of streams against which to validate</param>
        /// <param name="timeProvider">for providing current time</param>
        /// <param name="variableService">for access to variables</param>
        /// <param name="tableService"></param>
        /// <param name="scriptingService">The scripting service.</param>
        /// <param name="exprEvaluatorContext">The expression evaluator context.</param>
        /// <param name="configSnapshot">The config snapshot.</param>
        /// <param name="schedulingService">The scheduling service.</param>
        /// <param name="engineURI">The engine URI.</param>
        /// <param name="sqlParameters">The SQL parameters.</param>
        /// <param name="eventAdapterService">The event adapter service.</param>
        /// <param name="statementContext">The statement context</param>
        /// <throws>  ExprValidationException is thrown to indicate an exception in validating the view </throws>
        public void Validate(EngineImportService engineImportService, StreamTypeService streamTypeService, TimeProvider timeProvider, VariableService variableService, TableService tableService, ScriptingService scriptingService, ExprEvaluatorContext exprEvaluatorContext, ConfigurationInformation configSnapshot, SchedulingService schedulingService, string engineURI, IDictionary<int, IList<ExprNode>> sqlParameters, EventAdapterService eventAdapterService, StatementContext statementContext)
        {
            _statementContext = statementContext;
            _evaluators = new ExprEvaluator[_inputParameters.Count];
            _subordinateStreams = new SortedSet<int>();
            _exprEvaluatorContext = exprEvaluatorContext;

            var count = 0;
            var validationContext = new ExprValidationContext(
                statementContext.Container,
                streamTypeService, engineImportService,
                statementContext.StatementExtensionServicesContext, null,
                timeProvider, variableService, tableService,
                exprEvaluatorContext, eventAdapterService,
                statementContext.StatementName,
                statementContext.StatementId,
                statementContext.Annotations, null,
                scriptingService,
                false, false, true, false, null, false);

            foreach (var inputParam in _inputParameters)
            {
                var raw = FindSQLExpressionNode(_myStreamNumber, count, sqlParameters);
                if (raw == null)
                {
                    throw new ExprValidationException(
                        "Internal error find expression for historical stream parameter " + count + " stream " +
                        _myStreamNumber);
                }
                var evaluator = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.DATABASEPOLL, raw, validationContext);
                _evaluators[count++] = evaluator.ExprEvaluator;

                var visitor = new ExprNodeIdentifierCollectVisitor();
                visitor.Visit(evaluator);
                foreach (var identNode in visitor.ExprProperties)
                {
                    if (identNode.StreamId == _myStreamNumber)
                    {
                        throw new ExprValidationException("Invalid expression '" + inputParam +
                                                          "' resolves to the historical data itself");
                    }
                    _subordinateStreams.Add(identNode.StreamId);
                }
            }
        }

        /// <summary>
        /// Poll for stored historical or reference data using events per stream and
        /// returing for each event-per-stream row a separate list with events
        /// representing the poll result.
        /// </summary>
        /// <param name="lookupEventsPerStream">is the events per stream where the
        /// first dimension is a number of rows (often 1 depending on windows used) and
        /// the second dimension is the number of streams participating in a join.</param>
        /// <param name="indexingStrategy">the strategy to use for converting poll results into a indexed table for fast lookup</param>
        /// <param name="exprEvaluatorContext">The expression evaluator context.</param>
        /// <returns>
        /// array of lists with one list for each event-per-stream row
        /// </returns>
        public EventTable[][] Poll(EventBean[][] lookupEventsPerStream, PollResultIndexingStrategy indexingStrategy, ExprEvaluatorContext exprEvaluatorContext)
        {
            var localDataCache = _dataCacheThreadLocal.GetOrCreate();
            var strategyStarted = false;

            var resultPerInputRow = new EventTable[lookupEventsPerStream.Length][];

            // Get input parameters for each row
            for (var row = 0; row < lookupEventsPerStream.Length; row++)
            {
                var lookupValues = new Object[_inputParameters.Count];

                // Build lookup keys
                for (var valueNum = 0; valueNum < _inputParameters.Count; valueNum++)
                {
                    var eventsPerStream = lookupEventsPerStream[row];
                    var lookupValue = _evaluators[valueNum].Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                    lookupValues[valueNum] = lookupValue;
                }

                EventTable[] result = null;

                // try the threadlocal iteration cache, if set
                if (localDataCache != null)
                {
                    result = localDataCache.GetCached(lookupValues, lookupValues.Length);
                }

                // try the connection cache
                if (result == null)
                {
                    var multi = _dataCache.GetCached(lookupValues, lookupValues.Length);
                    if (multi != null)
                    {
                        result = multi;
                        if (localDataCache != null)
                        {
                            localDataCache.PutCached(lookupValues, lookupValues.Length, result);
                        }
                    }
                }

                // use the result from cache
                if (result != null)
                // found in cache
                {
                    resultPerInputRow[row] = result;
                }
                // not found in cache, get from actual polling (db query)
                else
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
                        _dataCache.PutCached(lookupValues, lookupValues.Length, indexTable);

                        if (localDataCache != null)
                        {
                            localDataCache.PutCached(lookupValues, lookupValues.Length, indexTable);
                        }
                    }
                    catch (EPException)
                    {
                        if (strategyStarted)
                        {
                            _pollExecStrategy.Done();
                        }

                        throw;
                    }
                }
            }


            if (strategyStarted)
            {
                _pollExecStrategy.Done();
            }

            return resultPerInputRow;
        }

        /// <summary>
        /// Add a view to the viewable object.
        /// </summary>
        /// <param name="view">to add</param>
        /// <returns>view to add</returns>
        public virtual View AddView(View view)
        {
            view.Parent = this;
            return view;
        }

        /// <summary>
        /// Returns all added views.
        /// </summary>
        /// <returns>list of added views</returns>
        public View[] Views
        {
            get { return EmptyList; }
        }

        private static readonly View[] EmptyList = { };

        /// <summary>
        /// Remove a view.
        /// </summary>
        /// <param name="view">to remove</param>
        /// <returns>
        /// true to indicate that the view to be removed existed within this view, false if the view to
        /// remove could not be found
        /// </returns>
        public virtual bool RemoveView(View view)
        {
            throw new NotSupportedException("Subviews not supported");
        }

        /// <summary>
        /// Test is there are any views to the Viewable.
        /// </summary>
        /// <value></value>
        /// <returns> true indicating there are child views, false indicating there are no child views
        /// </returns>
        public virtual bool HasViews
        {
            get { return false; }
        }

        /// <summary>
        /// Provides metadata information about the type of object the event collection contains.
        /// </summary>
        /// <value></value>
        /// <returns> metadata for the objects in the collection
        /// </returns>
        public virtual EventType EventType
        {
            get { return _eventType; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<EventBean> GetEnumerator()
        {
            var result = Poll(NULL_ROWS, ITERATOR_INDEXING_STRATEGY, _exprEvaluatorContext);

            foreach (var tableList in result)
            {
                foreach (var table in tableList)
                {
                    foreach (var e in table)
                    {
                        yield return e;
                    }
                }
            }
        }

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public void RemoveAllViews()
        {
            throw new UnsupportedOperationException("Subviews not supported");
        }

        private static ExprNode FindSQLExpressionNode(int myStreamNumber, int count, IDictionary<int, IList<ExprNode>> sqlParameters)
        {
            if ((sqlParameters == null) || (sqlParameters.IsEmpty()))
            {
                return null;
            }
            var paramList = sqlParameters.Get(myStreamNumber);
            if ((paramList == null) || (paramList.IsEmpty()) || (paramList.Count < (count + 1)))
            {
                return null;
            }
            return paramList[count];
        }
    }
}
