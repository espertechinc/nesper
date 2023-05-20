///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.threadlocal;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.kernel.statement;
using com.espertech.esper.runtime.@internal.kernel.thread;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;
using com.espertech.esper.runtime.@internal.subscriber;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    /// <summary>
    ///     Implements tracking of statement listeners and subscribers for a given statement
    ///     such as to efficiently dispatch in situations where 0, 1 or more listeners
    ///     are attached and/or 0 or 1 subscriber (such as iteration-only statement).
    /// </summary>
    public class StatementResultServiceImpl : StatementResultService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(StatementResultServiceImpl));

        private readonly EPServicesContext _epServicesContext;
        private readonly bool _outboundThreading;

        private readonly StatementInformationalsRuntime _statementInformationals;

        // Part of the statement context
        private EPStatementSPI _epStatement;

        private bool _forClauseDelivery;
        private ExprEvaluator _groupDeliveryExpressions;
        private EPRuntimeSPI _runtime;
        private string[] _selectClauseColumnNames;

        // For natural delivery derived out of select-clause expressions
        private Type[] _selectClauseTypes;

        /// <summary>
        ///     Buffer for holding dispatchable events.
        /// </summary>
        protected IThreadLocal<StatementDispatchTLEntry> StatementDispatchTl;

        // Listeners and subscribers and derived information
        private StatementMetricHandle _statementMetricHandle;

        private ISet<object> _statementOutputHooks;
        private ResultDeliveryStrategy _statementResultNaturalStrategy;

        public StatementResultServiceImpl(
            StatementInformationalsRuntime statementInformationals,
            EPServicesContext epServicesContext)
        {
            StatementDispatchTl = new SystemThreadLocal<StatementDispatchTLEntry>(() => new StatementDispatchTLEntry());
            _statementInformationals = statementInformationals;
            _epServicesContext = epServicesContext;
            _outboundThreading = epServicesContext.ThreadingService.IsOutboundThreading;
            IsMakeSynthetic = statementInformationals.IsAlwaysSynthesizeOutputEvents;
        }

        public int StatementId => _epStatement.StatementContext.StatementId;

        public EPStatementListenerSet StatementListenerSet { get; private set; }

        public IThreadLocal<StatementDispatchTLEntry> DispatchTL => StatementDispatchTl;

        public bool IsMakeSynthetic { get; private set; }

        public bool IsMakeNatural { get; private set; }

        public string StatementName => _epStatement.Name;

        // Called by OutputProcessView
        public void Indicate(
            UniformPair<EventBean[]> results,
            StatementDispatchTLEntry dispatchTLEntry)
        {
            if (results != null) {
                if (_statementMetricHandle.IsEnabled) {
                    var numIStream = results.First?.Length ?? 0;
                    var numRStream = results.Second?.Length ?? 0;
                    _epServicesContext.MetricReportingService.AccountOutput(_statementMetricHandle, numIStream, numRStream, _epStatement, _runtime);
                }

                var lastResults = dispatchTLEntry.Results;
                if (results.First != null && results.First.Length != 0) {
                    lastResults.Add(results);
                }
                else if (results.Second != null && results.Second.Length != 0) {
                    lastResults.Add(results);
                }
            }
        }

        public void Execute(StatementDispatchTLEntry dispatchTLEntry)
        {
            var dispatches = dispatchTLEntry.Results;

            var events = EventBeanUtility.FlattenList(dispatches);

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QStatementResultExecute(
                    events, _epStatement.DeploymentId, _epStatement.StatementId, _epStatement.Name, Thread.CurrentThread.ManagedThreadId);
                InstrumentationHelper.Get().AStatementResultExecute();
            }

            if (_outboundThreading) {
                _epServicesContext.ThreadingService.SubmitOutbound(new OutboundUnitRunnable(events, this));
            }
            else {
                ProcessDispatch(events);
            }

            dispatches.Clear();
        }

        public void SetContext(
            EPStatementSPI epStatement,
            EPRuntimeSPI runtime)
        {
            _epStatement = epStatement;
            _runtime = runtime;
            _statementMetricHandle = epStatement.StatementContext.EpStatementHandle.MetricsHandle;
        }

        public void SetSelectClause(
            Type[] selectClauseTypes,
            string[] selectClauseColumnNames,
            bool forClauseDelivery,
            ExprEvaluator groupDeliveryExpressions)
        {
            _selectClauseTypes = selectClauseTypes;
            _selectClauseColumnNames = selectClauseColumnNames;
            _forClauseDelivery = forClauseDelivery;
            _groupDeliveryExpressions = groupDeliveryExpressions;
        }

        public void SetUpdateListeners(
            EPStatementListenerSet listenerSet,
            bool isRecovery)
        {
            // indicate that listeners were updated for potential persistence of listener set, once the statement context is known
            if (_epStatement != null) {
                if (!isRecovery) {
                    var stmtCtx = _epStatement.StatementContext;
                    _epServicesContext.EpServicesHA.ListenerRecoveryService.Put(
                        stmtCtx.StatementId, 
                        stmtCtx.StatementName, 
                        listenerSet.Listeners);
                }
            }

            StatementListenerSet = listenerSet;

            IsMakeNatural = StatementListenerSet.Subscriber != null;
            IsMakeSynthetic = StatementListenerSet.Listeners.Length != 0 
                              || _statementInformationals.IsAlwaysSynthesizeOutputEvents;

            if (StatementListenerSet.Subscriber == null) {
                _statementResultNaturalStrategy = null;
                IsMakeNatural = false;
                return;
            }

            try {
                _statementResultNaturalStrategy = ResultDeliveryStrategyFactory.Create(
                    _epStatement,
                    StatementListenerSet.Subscriber, 
                    StatementListenerSet.SubscriberMethodName,
                    _selectClauseTypes, 
                    _selectClauseColumnNames, 
                    _runtime.URI, 
                    _runtime.ServicesContext.ImportServiceRuntime);
                IsMakeNatural = true;
            }
            catch (ResultDeliveryStrategyInvalidException ex) {
                throw new EPSubscriberException(ex.Message, ex);
            }
        }

        /// <summary>
        ///     Indicate an outbound result.
        /// </summary>
        /// <param name="events">to indicate</param>
        public void ProcessDispatch(UniformPair<EventBean[]> events)
        {
            // Plain all-events delivery
            if (!_forClauseDelivery) {
                DispatchInternal(events);
                return;
            }

            // Discrete delivery
            if (_groupDeliveryExpressions == null) {
                var todeliver = new UniformPair<EventBean[]>(null, null);
                if (events != null) {
                    if (events.First != null) {
                        foreach (var theEvent in events.First) {
                            todeliver.First = new[] {theEvent};
                            DispatchInternal(todeliver);
                        }

                        todeliver.First = null;
                    }

                    if (events.Second != null) {
                        foreach (var theEvent in events.Second) {
                            todeliver.Second = new[] {theEvent};
                            DispatchInternal(todeliver);
                        }

                        todeliver.Second = null;
                    }
                }

                return;
            }

            // Grouped delivery
            IDictionary<object, UniformPair<EventBean[]>> groups;
            try {
                groups = GetGroupedResults(events);
            }
            catch (Exception ex) {
                Log.Error($"Unexpected exception evaluating grouped-delivery expressions: {ex.Message}, delivering ungrouped", ex);
                DispatchInternal(events);
                return;
            }

            // Deliver each group separately
            foreach (var group in groups) {
                DispatchInternal(group.Value);
            }
        }

        public void ClearDeliveriesRemoveStream(EventBean[] removedEvents)
        {
            var entry = DispatchTL.Value;
            if (entry == null) {
                return;
            }

            entry.Results.RemoveWhere(
                pair => {
                    if (pair.Second != null) {
                        foreach (var removedEvent in removedEvents) {
                            if (pair.Second.Any(dispatchEvent => removedEvent == dispatchEvent)) {
                                return true;
                            }
                        }
                    }

                    return false;
                },
                pair => {
                });

            if (!entry.Results.IsEmpty()) {
                return;
            }

            entry.IsDispatchWaiting = false;
            _epServicesContext.DispatchService.RemoveAll(_epStatement.DispatchChildView);
        }

        private IDictionary<object, UniformPair<EventBean[]>> GetGroupedResults(UniformPair<EventBean[]> events)
        {
            if (events == null) {
                return new EmptyDictionary<object, UniformPair<EventBean[]>>();
            }

            IDictionary<object, UniformPair<EventBean[]>> groups = new LinkedHashMap<object, UniformPair<EventBean[]>>();
            var eventsPerStream = new EventBean[1];
            GetGroupedResults(groups, events.First, true, eventsPerStream);
            GetGroupedResults(groups, events.Second, false, eventsPerStream);
            return groups;
        }

        private void GetGroupedResults(
            IDictionary<object, UniformPair<EventBean[]>> groups,
            EventBean[] events,
            bool insertStream,
            EventBean[] eventsPerStream)
        {
            if (events == null) {
                return;
            }

            foreach (var theEvent in events) {
                var evalEvent = theEvent;
                if (evalEvent is NaturalEventBean) {
                    evalEvent = ((NaturalEventBean) evalEvent).OptionalSynthetic;
                }

                eventsPerStream[0] = evalEvent;
                var key = _groupDeliveryExpressions.Evaluate(eventsPerStream, true, _epStatement.StatementContext);

                var groupEntry = groups.Get(key);
                if (groupEntry == null) {
                    if (insertStream) {
                        groupEntry = new UniformPair<EventBean[]>(new[] {theEvent}, null);
                    }
                    else {
                        groupEntry = new UniformPair<EventBean[]>(null, new[] {theEvent});
                    }

                    groups.Put(key, groupEntry);
                }
                else {
                    if (insertStream) {
                        if (groupEntry.First == null) {
                            groupEntry.First = new[] {theEvent};
                        }
                        else {
                            groupEntry.First = EventBeanUtility.AddToArray(groupEntry.First, theEvent);
                        }
                    }
                    else {
                        if (groupEntry.Second == null) {
                            groupEntry.Second = new[] {theEvent};
                        }
                        else {
                            groupEntry.Second = EventBeanUtility.AddToArray(groupEntry.Second, theEvent);
                        }
                    }
                }
            }
        }

        private void DispatchInternal(UniformPair<EventBean[]> events)
        {
            _statementResultNaturalStrategy?.Execute(events);

            var newEventArr = events?.First;
            var oldEventArr = events?.Second;
            var updateEventArgs = new UpdateEventArgs(_runtime, _epStatement, newEventArr, oldEventArr);

            foreach (var listener in StatementListenerSet.Listeners) {
                try {
                    listener.Update(_epStatement, updateEventArgs);
                }
                catch (Exception ex) {
                    var message = string.Format(
                        "Unexpected exception invoking listener update method on listener class '{0}' : {1} : {2}",
                        listener.GetType().Name,
                        ex.GetType().Name,
                        ex.Message);
                    Log.Error(message, ex);
                }
            }
        }
    }
} // end of namespace