///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
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
using com.espertech.esper.compat.threading;
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

        private readonly EPServicesContext epServicesContext;
        private readonly bool outboundThreading;

        private readonly StatementInformationalsRuntime statementInformationals;

        // Part of the statement context
        private EPStatementSPI epStatement;

        private bool forClauseDelivery;
        private ExprEvaluator groupDeliveryExpressions;
        private EPRuntimeSPI runtime;
        private string[] selectClauseColumnNames;

        // For natural delivery derived out of select-clause expressions
        private Type[] selectClauseTypes;

        /// <summary>
        ///     Buffer for holding dispatchable events.
        /// </summary>
        protected IThreadLocal<StatementDispatchTLEntry> statementDispatchTL;

        // Listeners and subscribers and derived information
        private StatementMetricHandle statementMetricHandle;

        private ISet<object> statementOutputHooks;
        private ResultDeliveryStrategy statementResultNaturalStrategy;

        public StatementResultServiceImpl(
            StatementInformationalsRuntime statementInformationals,
            EPServicesContext epServicesContext)
        {
            statementDispatchTL = new FastThreadLocal<StatementDispatchTLEntry>(() => new StatementDispatchTLEntry());
            this.statementInformationals = statementInformationals;
            this.epServicesContext = epServicesContext;
            outboundThreading = epServicesContext.ThreadingService.IsOutboundThreading;
            IsMakeSynthetic = statementInformationals.IsAlwaysSynthesizeOutputEvents;
        }

        public int StatementId => epStatement.StatementContext.StatementId;

        public EPStatementEventHandlerSet StatementEventHandlerSet { get; private set; }

        public IThreadLocal<StatementDispatchTLEntry> DispatchTL => statementDispatchTL;

        public bool IsMakeSynthetic { get; private set; }

        public bool IsMakeNatural { get; private set; }

        public string StatementName => epStatement.Name;

        // Called by OutputProcessView
        public void Indicate(
            UniformPair<EventBean[]> results,
            StatementDispatchTLEntry dispatchTLEntry)
        {
            if (results != null) {
                if (statementMetricHandle.IsEnabled) {
                    var numIStream = results.First?.Length ?? 0;
                    var numRStream = results.Second?.Length ?? 0;
                    epServicesContext.MetricReportingService.AccountOutput(statementMetricHandle, numIStream, numRStream, epStatement, runtime);
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
                    events, epStatement.DeploymentId, epStatement.StatementId, epStatement.Name, Thread.CurrentThread.ManagedThreadId);
                InstrumentationHelper.Get().AStatementResultExecute();
            }

            if (outboundThreading) {
                epServicesContext.ThreadingService.SubmitOutbound(new OutboundUnitRunnable(events, this));
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
            this.epStatement = epStatement;
            this.runtime = runtime;
            statementMetricHandle = epStatement.StatementContext.EpStatementHandle.MetricsHandle;
        }

        public void SetSelectClause(
            Type[] selectClauseTypes,
            string[] selectClauseColumnNames,
            bool forClauseDelivery,
            ExprEvaluator groupDeliveryExpressions)
        {
            this.selectClauseTypes = selectClauseTypes;
            this.selectClauseColumnNames = selectClauseColumnNames;
            this.forClauseDelivery = forClauseDelivery;
            this.groupDeliveryExpressions = groupDeliveryExpressions;
        }

        public void SetUpdateEventHandlers(
            EPStatementEventHandlerSet eventHandlers,
            bool isRecovery)
        {
            // indicate that listeners were updated for potential persistence of listener set, once the statement context is known
            if (epStatement != null) {
                if (!isRecovery) {
                    var stmtCtx = epStatement.StatementContext;
                    epServicesContext.EpServicesHA.ListenerRecoveryService.Put(
                        stmtCtx.StatementId, 
                        stmtCtx.StatementName, 
                        eventHandlers.EventHandlers);
                }
            }

            StatementEventHandlerSet = eventHandlers;

            IsMakeNatural = StatementEventHandlerSet.Subscriber != null;
            IsMakeSynthetic = StatementEventHandlerSet.EventHandlers.Length != 0 
                              || statementInformationals.IsAlwaysSynthesizeOutputEvents;

            if (StatementEventHandlerSet.Subscriber == null) {
                statementResultNaturalStrategy = null;
                IsMakeNatural = false;
                return;
            }

            try {
                statementResultNaturalStrategy = ResultDeliveryStrategyFactory.Create(
                    epStatement,
                    StatementEventHandlerSet.Subscriber, 
                    StatementEventHandlerSet.SubscriberMethodName,
                    selectClauseTypes, 
                    selectClauseColumnNames, 
                    runtime.URI, 
                    runtime.ServicesContext.ImportServiceRuntime);
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
            if (!forClauseDelivery) {
                DispatchInternal(events);
                return;
            }

            // Discrete delivery
            if (groupDeliveryExpressions == null) {
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
                var key = groupDeliveryExpressions.Evaluate(eventsPerStream, true, epStatement.StatementContext);

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
            statementResultNaturalStrategy?.Execute(events);

            var newEventArr = events?.First;
            var oldEventArr = events?.Second;
            var updateEventArgs = new UpdateEventArgs(runtime, epStatement, newEventArr, oldEventArr);

            foreach (var listener in StatementEventHandlerSet.EventHandlers) {
                try {
                    listener.Invoke(epStatement, updateEventArgs);
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