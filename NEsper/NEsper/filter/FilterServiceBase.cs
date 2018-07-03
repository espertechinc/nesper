///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Implementation of the filter service interface. Does not allow the same filter callback
    /// to be added more then once.
    /// </summary>
    public abstract class FilterServiceBase : FilterServiceSPI
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly FilterServiceGranularLockFactory _lockFactory;
        private readonly EventTypeIndexBuilder _indexBuilder;
        private readonly EventTypeIndex _eventTypeIndex;
        private long _numEventsEvaluated = 0;
        private long _filtersVersion = 1;
        private readonly CopyOnWriteArraySet<FilterServiceListener> _filterServiceListeners;

        /// <summary>Constructor. </summary>
        protected FilterServiceBase(
            ILockManager lockManager,
            FilterServiceGranularLockFactory lockFactory,
            bool allowIsolation)
        {
            _lockFactory = lockFactory;
            _eventTypeIndex = new EventTypeIndex(lockFactory);
            _indexBuilder = new EventTypeIndexBuilder(lockManager, _eventTypeIndex, allowIsolation);
            _filterServiceListeners = new CopyOnWriteArraySet<FilterServiceListener>();
        }

        public bool IsSupportsTakeApply
        {
            get { return _indexBuilder.IsSupportsTakeApply; }
        }

        public long FiltersVersion
        {
            get { return _filtersVersion; }
        }

        public void Dispose()
        {
            Log.Debug("Destroying filter service");
            _eventTypeIndex.Dispose();
            _indexBuilder.Destroy();
        }

        protected FilterServiceEntry AddInternal(FilterValueSet filterValueSet, FilterHandle filterCallback)
        {
            var entry = _indexBuilder.Add(filterValueSet, filterCallback, _lockFactory);
            _filtersVersion++;
            return entry;
        }

        protected void RemoveInternal(FilterHandle filterCallback, FilterServiceEntry filterServiceEntry)
        {
            _indexBuilder.Remove(filterCallback, filterServiceEntry);
            _filtersVersion++;
        }

        protected long EvaluateInternal(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QFilter(theEvent); }

            long version = _filtersVersion;
            Interlocked.Increment(ref _numEventsEvaluated);
            //_numEventsEvaluated.IncrementAndGet();

            // Finds all matching filters and return their callbacks.
            RetryableMatchEvent(theEvent, matches);

            if ((AuditPath.IsAuditEnabled) && (_filterServiceListeners.IsNotEmpty()))
            {
                foreach (FilterServiceListener listener in _filterServiceListeners)
                {
                    listener.Filtering(theEvent, matches, null);
                }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AFilter(matches); }

            return version;
        }

        protected long EvaluateInternal(EventBean theEvent, ICollection<FilterHandle> matches, int statementId)
        {
            long version = _filtersVersion;
            Interlocked.Increment(ref _numEventsEvaluated);
            //_numEventsEvaluated.IncrementAndGet();

            ArrayDeque<FilterHandle> allMatches = new ArrayDeque<FilterHandle>();

            // Finds all matching filters
            RetryableMatchEvent(theEvent, allMatches);

            // Add statement matches to collection passed
            foreach (FilterHandle match in allMatches)
            {
                if (match.StatementId == statementId)
                {
                    matches.Add(match);
                }
            }

            if ((AuditPath.IsAuditEnabled) && (_filterServiceListeners.IsNotEmpty()))
            {
                foreach (FilterServiceListener listener in _filterServiceListeners)
                {
                    listener.Filtering(theEvent, matches, statementId);
                }
            }

            return version;
        }

        public long NumEventsEvaluated
        {
            get { return Interlocked.Read(ref _numEventsEvaluated); }
        }

        public void ResetStats()
        {
            Interlocked.Exchange(ref _numEventsEvaluated, 0);
        }

        public void AddFilterServiceListener(FilterServiceListener filterServiceListener)
        {
            _filterServiceListeners.Add(filterServiceListener);
        }

        public void RemoveFilterServiceListener(FilterServiceListener filterServiceListener)
        {
            _filterServiceListeners.Remove(filterServiceListener);
        }

        protected FilterSet TakeInternal(ICollection<int> statementIds)
        {
            _filtersVersion++;
            return _indexBuilder.Take(statementIds);
        }

        protected void ApplyInternal(FilterSet filterSet)
        {
            _filtersVersion++;
            _indexBuilder.Apply(filterSet, _lockFactory);
        }

        //@JmxGetter(name="NumFiltersApprox", description = "Number of filters managed (approximately)")
        public int FilterCountApprox
        {
            get { return _eventTypeIndex.FilterCountApprox; }
        }


        public int CountTypes
        {
            get { return _eventTypeIndex.Count; }
        }

        public void Init()
        {
            // no initialization required
        }

        protected void RemoveTypeInternal(EventType type)
        {
            _eventTypeIndex.RemoveType(type);
        }

        private void RetryableMatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            // Install lock backoff exception handler that retries the evaluation.
            try
            {
                _eventTypeIndex.MatchEvent(theEvent, matches);
            }
            catch (FilterLockBackoffException)
            {
                // retry on lock back-off
                // lock-backoff may occur when stateful evaluations take place such as bool expressions that are subqueries
                // statements that contain subqueries in pattern filter expression can themselves modify filters, leading to a theoretically possible deadlock
                long delayNs = 10;
                while (true)
                {
                    try
                    {
                        // yield
                        try
                        {
                            Thread.Sleep(0);
                        }
                        catch (ThreadInterruptedException)
                        {
                            Thread.CurrentThread.Interrupt();
                        }

                        // delay
                        MicroThread.SleepNano(delayNs);
                        if (delayNs < 1000000000)
                        {
                            delayNs = delayNs * 2;
                        }

                        // evaluate
                        matches.Clear();
                        _eventTypeIndex.MatchEvent(theEvent, matches);
                        break;
                    }
                    catch (FilterLockBackoffException)
                    {
                        // retried
                    }
                }
            }
        }

        public abstract long Evaluate(EventBean theEvent, ICollection<FilterHandle> matches, int statementId);
        public abstract long Evaluate(EventBean theEvent, ICollection<FilterHandle> matches);
        public abstract FilterServiceEntry Add(FilterValueSet filterValueSet, FilterHandle callback);
        public abstract void Remove(FilterHandle callback, FilterServiceEntry filterServiceEntry);
        public abstract void RemoveType(EventType type);
        public abstract FilterSet Take(ICollection<int> statementId);
        public abstract void Apply(FilterSet filterSet);
        public abstract ILockable WriteLock { get; }
    }
}
