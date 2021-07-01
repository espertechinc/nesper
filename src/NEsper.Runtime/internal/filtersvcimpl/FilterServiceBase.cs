///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Implementation of the filter service interface.
    ///     Does not allow the same filter callback to be added more then once.
    /// </summary>
    public abstract class FilterServiceBase : FilterServiceSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FilterServiceBase));

        private readonly int stageId;
        private readonly EventTypeIndex eventTypeIndex;
        private readonly CopyOnWriteArraySet<FilterServiceListener> filterServiceListeners;
        private readonly EventTypeIndexBuilder indexBuilder;

        private readonly FilterServiceGranularLockFactory lockFactory;
        private readonly AtomicLong numEventsEvaluated = new AtomicLong();
        private long filtersVersion = 1;

        protected FilterServiceBase(
            FilterServiceGranularLockFactory lockFactory,
            int stageId)
        {
            this.lockFactory = lockFactory;
            this.stageId = stageId;
            eventTypeIndex = new EventTypeIndex(lockFactory);
            indexBuilder = new EventTypeIndexBuilder(eventTypeIndex);
            filterServiceListeners = new CopyOnWriteArraySet<FilterServiceListener>();
        }

        public long FiltersVersion => filtersVersion;

        public void Destroy()
        {
            Log.Debug("Destroying filter service");
            eventTypeIndex.Destroy();
            indexBuilder.Destroy();
        }

        //@JmxGetter(name = "NumEventsEvaluated", description = "Number of events evaluated (main)")
        public long NumEventsEvaluated => numEventsEvaluated.Get();

        //@JmxOperation(description = "Reset number of events evaluated")
        public void ResetStats()
        {
            numEventsEvaluated.Set(0);
        }

        public void AddFilterServiceListener(FilterServiceListener filterServiceListener)
        {
            filterServiceListeners.Add(filterServiceListener);
        }

        public void RemoveFilterServiceListener(FilterServiceListener filterServiceListener)
        {
            filterServiceListeners.Remove(filterServiceListener);
        }

        //@JmxGetter(name = "NumFiltersApprox", description = "Number of filters managed (approximately)")
        public int FilterCountApprox => eventTypeIndex.FilterCountApprox;

        //@JmxGetter(name = "NumEventTypes", description = "Number of event types considered")
        public int CountTypes => eventTypeIndex.Count;

        protected void AddInternal(
            EventType eventType,
            FilterValueSetParam[][] valueSet,
            FilterHandle filterCallback)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterAdd(eventType, valueSet, filterCallback);
            }

            indexBuilder.Add(eventType, valueSet, filterCallback, lockFactory);
            filtersVersion++;

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AFilterAdd();
            }
        }

        protected void RemoveInternal(
            FilterHandle filterCallback,
            EventType eventType,
            FilterValueSetParam[][] valueSet)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterRemove(filterCallback, eventType, valueSet);
            }

            indexBuilder.Remove(filterCallback, eventType, valueSet);
            filtersVersion++;

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AFilterRemove();
            }
        }

        protected long EvaluateInternal(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx)
        {
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilter(theEvent);
            }

            var version = filtersVersion;
            numEventsEvaluated.IncrementAndGet();

            // Finds all matching filters and return their callbacks.
            RetryableMatchEvent(theEvent, matches, ctx);

            if (AuditPath.isAuditEnabled && !filterServiceListeners.IsEmpty()) {
                foreach (var listener in filterServiceListeners) {
                    listener.Filtering(theEvent, matches, null);
                }
            }

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AFilter(matches);
            }

            return version;
        }

        protected long EvaluateInternal(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            int statementId,
            ExprEvaluatorContext ctx)
        {
            var version = filtersVersion;
            numEventsEvaluated.IncrementAndGet();

            var allMatches = new ArrayDeque<FilterHandle>();

            // Finds all matching filters
            RetryableMatchEvent(theEvent, allMatches, ctx);

            // Add statement matches to collection passed
            foreach (var match in allMatches) {
                if (match.StatementId == statementId) {
                    matches.Add(match);
                }
            }

            if (AuditPath.isAuditEnabled && !filterServiceListeners.IsEmpty()) {
                foreach (var listener in filterServiceListeners) {
                    listener.Filtering(theEvent, matches, statementId);
                }
            }

            return version;
        }

        protected IDictionary<EventTypeIdPair, IDictionary<int, IList<FilterItem[]>>> GetInternal(ISet<int> statementIds)
        {
            return indexBuilder.Get(statementIds);
        }

        public void Init(Supplier<ICollection<EventType>> availableTypes)
        {
            // no initialization required
        }

        protected void RemoveTypeInternal(EventType type)
        {
            eventTypeIndex.RemoveType(type);
        }

        private void RetryableMatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx)
        {
            // Install lock backoff exception handler that retries the evaluation.
            try {
                eventTypeIndex.MatchEvent(theEvent, matches, ctx);
            }
            catch (FilterLockBackoffException) {
                // retry on lock back-off
                // lock-backoff may occur when stateful evaluations take place such as boolean expressions that are subqueries
                // statements that contain subqueries in pattern filter expression can themselves modify filters, leading to a theoretically possible deadlock
                int delaySpin = 10; // 10 cycles
                while (true) {
                    try {
                        // yield
                        try {
                            Thread.Sleep(0);
                        }
                        catch (ThreadInterruptedException) {
                            Thread.CurrentThread.Interrupt();
                        }

                        // delay
                        Thread.SpinWait(delaySpin);
                        if (delaySpin < 10000000) {
                            delaySpin = delaySpin * 2;
                        }

                        // evaluate
                        matches.Clear();
                        eventTypeIndex.MatchEvent(theEvent, matches, ctx);
                        break;
                    }
                    catch (FilterLockBackoffException) {
                        // retried
                    }
                }
            }
        }

        public abstract long Evaluate(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            ExprEvaluatorContext ctx);

        public abstract long Evaluate(
            EventBean theEvent,
            ICollection<FilterHandle> matches,
            int statementId,
            ExprEvaluatorContext ctx);

        public abstract void Add(
            EventType eventType,
            FilterValueSetParam[][] valueSet,
            FilterHandle callback);

        public abstract void Remove(
            FilterHandle callback,
            EventType eventType,
            FilterValueSetParam[][] valueSet);

        public abstract void RemoveType(EventType type);
        public abstract void AcquireWriteLock();
        public abstract void ReleaseWriteLock();
        public abstract IDictionary<EventTypeIdPair, IDictionary<int, IList<FilterItem[]>>> Get(ISet<int> statementId);
    }
} // end of namespace