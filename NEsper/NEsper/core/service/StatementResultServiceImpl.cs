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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.thread;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.metric;
using com.espertech.esper.events;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Implements tracking of statement listeners and subscribers for a given statement
    /// such as to efficiently dispatch in situations where 0, 1 or more listeners are attached 
    /// and/or 0 or 1 subscriber (such as iteration-only statement).
    /// </summary>
    public class StatementResultServiceImpl : StatementResultService
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly bool _isDebugEnabled;

        private readonly String _statementName;
        private readonly StatementLifecycleSvc _statementLifecycleSvc;
        private readonly MetricReportingService _metricReportingService;
        private readonly ThreadingService _threadingService;

        // Part of the statement context
        private EPStatementSPI _epStatement;
        private EPServiceProviderSPI _epServiceProvider;
        private bool _isInsertInto;
        private bool _isPattern;
        private bool _isDistinct;
        private bool _isForClause;
        private StatementMetricHandle _statementMetricHandle;

        private bool _forClauseDelivery;
        private ExprEvaluator[] _groupDeliveryExpressions;
        private ExprEvaluatorContext _exprEvaluatorContext;

        // For natural delivery derived out of select-clause expressions
        private Type[] _selectClauseTypes;
        private String[] _selectClauseColumnNames;

        // Listeners and subscribers and derived information
        private EPStatementListenerSet _statementListenerSet;
        private bool _isMakeNatural;
        private bool _isMakeSynthetic;
        private ResultDeliveryStrategy _statementResultNaturalStrategy;

        private readonly ICollection<StatementResultListener> _statementOutputHooks;

        /// <summary>Buffer for holding dispatchable events. </summary>
        private IThreadLocal<LinkedList<UniformPair<EventBean[]>>> _lastResults;

        private IThreadLocalManager _threadLocalManager;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="statementLifecycleSvc">handles persistence for statements</param>
        /// <param name="metricReportingService">for metrics reporting</param>
        /// <param name="threadingService">for outbound threading</param>
        /// <param name="threadLocalManager">The thread local manager.</param>
        public StatementResultServiceImpl(
            String statementName,
            StatementLifecycleSvc statementLifecycleSvc,
            MetricReportingServiceSPI metricReportingService,
            ThreadingService threadingService,
            IThreadLocalManager threadLocalManager)
        {
            Log.Debug(".ctor");

            _threadLocalManager = threadLocalManager;
            _lastResults =  threadLocalManager.Create(() => new LinkedList<UniformPair<EventBean[]>>());
            _isDebugEnabled = ExecutionPathDebugLog.IsEnabled && Log.IsDebugEnabled;
            _statementName = statementName;
            _statementLifecycleSvc = statementLifecycleSvc;
            _metricReportingService = metricReportingService;
            _statementOutputHooks = metricReportingService != null ? metricReportingService.StatementOutputHooks : new List<StatementResultListener>();
            _threadingService = threadingService;
        }

        /// <summary>
        /// Gets the name of the statement.
        /// </summary>
        /// <value>The name of the statement.</value>
        public string StatementName
        {
            get { return _statementName; }
        }

        /// <summary>
        /// For initialization of the service to provide statement context.
        /// </summary>
        /// <param name="epStatement">the statement</param>
        /// <param name="epServiceProvider">the engine instance</param>
        /// <param name="isInsertInto">true if this is insert into</param>
        /// <param name="isPattern">true if this is a pattern statement</param>
        /// <param name="isDistinct">true if using distinct</param>
        /// <param name="isForClause">if set to <c>true</c> [is for clause].</param>
        /// <param name="statementMetricHandle">handle for metrics reporting</param>
        public void SetContext(
            EPStatementSPI epStatement,
            EPServiceProviderSPI epServiceProvider,
            bool isInsertInto,
            bool isPattern,
            bool isDistinct,
            bool isForClause,
            StatementMetricHandle statementMetricHandle)
        {
            _epStatement = epStatement;
            _epServiceProvider = epServiceProvider;
            _isInsertInto = isInsertInto;
            _isPattern = isPattern;
            _isDistinct = isDistinct;
            _isForClause = isForClause;
            _isMakeSynthetic = isInsertInto || isPattern || isDistinct || isForClause;
            _statementMetricHandle = statementMetricHandle;
        }

        /// <summary>
        /// For initialize of the service providing select clause column types and names.
        /// </summary>
        /// <param name="selectClauseTypes">types of columns in the select clause</param>
        /// <param name="selectClauseColumnNames">column names</param>
        /// <param name="forClauseDelivery">if set to <c>true</c> [for clause delivery].</param>
        /// <param name="groupDeliveryExpressions">The group delivery expressions.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        public void SetSelectClause(
            Type[] selectClauseTypes,
            String[] selectClauseColumnNames,
            bool forClauseDelivery,
            ExprEvaluator[] groupDeliveryExpressions,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if ((selectClauseTypes == null) || (selectClauseTypes.Length == 0))
            {
                throw new ArgumentException("Invalid null or zero-element list of select clause expression types");
            }
            if ((selectClauseColumnNames == null) || (selectClauseColumnNames.Length == 0))
            {
                throw new ArgumentException("Invalid null or zero-element list of select clause column names");
            }
            _selectClauseTypes = selectClauseTypes;
            _selectClauseColumnNames = selectClauseColumnNames;
            _forClauseDelivery = forClauseDelivery;
            _exprEvaluatorContext = exprEvaluatorContext;
            _groupDeliveryExpressions = groupDeliveryExpressions;
        }

        public int StatementId
        {
            get { return _epStatement.StatementId; }
        }

        public bool IsMakeSynthetic
        {
            get { return _isMakeSynthetic; }
        }

        public bool IsMakeNatural
        {
            get { return _isMakeNatural; }
        }

        public EPStatementListenerSet StatementListenerSet
        {
            get { return _statementListenerSet; }
        }

        public void SetUpdateListeners(EPStatementListenerSet updateListeners, bool isRecovery)
        {
            // indicate that listeners were updated for potential persistence of listener set, once the statement context is known
            if (_epStatement != null)
            {
                _statementLifecycleSvc.UpdatedListeners(_epStatement, updateListeners, isRecovery);
            }

            _statementListenerSet = updateListeners;

            _isMakeNatural = _statementListenerSet.Subscriber != null;
            _isMakeSynthetic = _statementListenerSet.HasEventConsumers
                    || _isPattern || _isInsertInto || _isDistinct | _isForClause;

            if (_statementListenerSet.Subscriber == null)
            {
                _statementResultNaturalStrategy = null;
                _isMakeNatural = false;
                return;
            }

            _statementResultNaturalStrategy = ResultDeliveryStrategyFactory.Create(
                _epStatement,
                _statementListenerSet.Subscriber,
                _selectClauseTypes,
                _selectClauseColumnNames,
                _epServiceProvider.URI,
                _epServiceProvider.EngineImportService
                );
            _isMakeNatural = true;
        }

#if NET45
        //[MethodImplOptions.AggressiveInlining]
#endif

        // Called by OutputProcessView
        public void Indicate(UniformPair<EventBean[]> results)
        {
            if (results != null)
            {
                if ((MetricReportingPath.IsMetricsEnabled) && (_statementMetricHandle.IsEnabled))
                {
                    int numIStream = (results.First != null) ? results.First.Length : 0;
                    int numRStream = (results.Second != null) ? results.Second.Length : 0;
                    _metricReportingService.AccountOutput(_statementMetricHandle, numIStream, numRStream);
                }

                var lastResults = _lastResults.GetOrCreate();

                if ((results.First != null) && (results.First.Length != 0))
                {
                    lastResults.AddLast(results);
                }
                else if ((results.Second != null) && (results.Second.Length != 0))
                {
                    lastResults.AddLast(results);
                }
            }
        }

        public void Execute()
        {
            var dispatches = _lastResults.GetOrCreate();
            var events = EventBeanUtility.FlattenList(dispatches);

            if (_isDebugEnabled)
            {
                ViewSupport.DumpUpdateParams(".execute", events);
            }

            if ((ThreadingOption.IsThreadingEnabledValue) && (_threadingService.IsOutboundThreading))
            {
                _threadingService.SubmitOutbound(new OutboundUnitRunnable(events, this).Run);
            }
            else
            {
                ProcessDispatch(events);
            }

            dispatches.Clear();
        }

        /// <summary>Indicate an outbound result. </summary>
        /// <param name="events">to indicate</param>
        public void ProcessDispatch(UniformPair<EventBean[]> events)
        {
            // Plain all-events delivery
            if (!_forClauseDelivery)
            {
                DispatchInternal(events);
                return;
            }

            // Discrete delivery
            if ((_groupDeliveryExpressions == null) || (_groupDeliveryExpressions.Length == 0))
            {
                var todeliver = new UniformPair<EventBean[]>(null, null);

                if (events != null)
                {
                    if (events.First != null)
                    {
                        foreach (EventBean theEvent in events.First)
                        {
                            todeliver.First = new EventBean[] { theEvent };
                            DispatchInternal(todeliver);
                        }
                        todeliver.First = null;
                    }
                    if (events.Second != null)
                    {
                        foreach (EventBean theEvent in events.Second)
                        {
                            todeliver.Second = new EventBean[] { theEvent };
                            DispatchInternal(todeliver);
                        }
                        todeliver.Second = null;
                    }
                }

                return;
            }

            // Grouped delivery
            IDictionary<Object, UniformPair<EventBean[]>> groups;
            try
            {
                groups = GetGroupedResults(events);
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected exception evaluating grouped-delivery expressions: " + ex.Message + ", delivering ungrouped", ex);
                DispatchInternal(events);
                return;
            }

            // Deliver each group separately
            foreach (var group in groups)
            {
                DispatchInternal(group.Value);
            }
        }

        private IDictionary<Object, UniformPair<EventBean[]>> GetGroupedResults(UniformPair<EventBean[]> events)
        {
            if (events == null)
            {
                return new Dictionary<Object, UniformPair<EventBean[]>>();
            }

            var groups = new LinkedHashMap<Object, UniformPair<EventBean[]>>();
            var eventsPerStream = new EventBean[1];
            GetGroupedResults(groups, events.First, true, eventsPerStream);
            GetGroupedResults(groups, events.Second, false, eventsPerStream);
            return groups;
        }

        private void GetGroupedResults(IDictionary<Object, UniformPair<EventBean[]>> groups, IEnumerable<EventBean> events, bool insertStream, EventBean[] eventsPerStream)
        {
            if (events == null)
            {
                return;
            }

            foreach (EventBean theEvent in events)
            {
                EventBean evalEvent = theEvent;
                if (evalEvent is NaturalEventBean)
                {
                    evalEvent = ((NaturalEventBean)evalEvent).OptionalSynthetic;
                }

                Object key;
                eventsPerStream[0] = evalEvent;
                if (_groupDeliveryExpressions.Length == 1)
                {
                    key = _groupDeliveryExpressions[0].Evaluate(new EvaluateParams(eventsPerStream, true, _exprEvaluatorContext));
                }
                else
                {
                    var keys = new Object[_groupDeliveryExpressions.Length];
                    for (int i = 0; i < _groupDeliveryExpressions.Length; i++)
                    {
                        keys[i] = _groupDeliveryExpressions[i].Evaluate(new EvaluateParams(eventsPerStream, true, _exprEvaluatorContext));
                    }
                    key = new MultiKeyUntyped(keys);
                }

                UniformPair<EventBean[]> groupEntry = groups.Get(key);
                if (groupEntry == null)
                {
                    groupEntry = insertStream
                                     ? new UniformPair<EventBean[]>(new[] { theEvent }, null)
                                     : new UniformPair<EventBean[]>(null, new[] { theEvent });
                    groups.Put(key, groupEntry);
                }
                else
                {
                    if (insertStream)
                    {
                        groupEntry.First = groupEntry.First == null
                                               ? new[] { theEvent }
                                               : EventBeanUtility.AddToArray(groupEntry.First, theEvent);
                    }
                    else
                    {
                        groupEntry.Second = groupEntry.Second == null
                                                ? new[] { theEvent }
                                                : EventBeanUtility.AddToArray(groupEntry.Second, theEvent);
                    }
                }
            }
        }

        private void DispatchInternal(UniformPair<EventBean[]> events)
        {
            if (_statementResultNaturalStrategy != null)
            {
                _statementResultNaturalStrategy.Execute(events);
            }

            EventBean[] newEventArr = events != null ? events.First : null;
            EventBean[] oldEventArr = events != null ? events.Second : null;

            var eventHandlerList = _statementListenerSet.Events;
            if (eventHandlerList.Count != 0)
            {
                var ev = new UpdateEventArgs(_epServiceProvider, _epStatement, newEventArr, oldEventArr);
                var eventList = eventHandlerList.ToArray();
                if (eventList != null)
                {
                    var eventListLength = eventList.Length;
                    for (int ii = 0; ii < eventListLength; ii++)
                    {
                        var eventHandler = eventList[ii];
                        try {
                            eventHandler.Invoke(this, ev);
                        }
                        catch (Exception e) {
                            Log.Error("Unexpected exception invoking event handler", e);
                        }
                    }
                }
            }

            if ((AuditPath.IsAuditEnabled) && (_statementOutputHooks.IsNotEmpty()))
            {
                foreach (StatementResultListener listener in _statementOutputHooks)
                {
                    listener.Update(newEventArr, oldEventArr, _epStatement.Name, _epStatement, _epServiceProvider);
                }
            }
        }

        /// <summary>
        /// Dispatches when the statement is stopped any remaining results.
        /// </summary>
        public void DispatchOnStop()
        {
            var dispatches = _lastResults.GetOrCreate();
            if (dispatches.IsEmpty())
            {
                return;
            }

            Execute();

            _lastResults = _threadLocalManager.Create(
                () => new LinkedList<UniformPair<EventBean[]>>());
        }
    }
}
