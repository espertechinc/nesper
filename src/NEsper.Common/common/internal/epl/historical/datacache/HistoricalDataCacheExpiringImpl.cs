///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.datacache
{
    /// <summary>
    ///     Implements an expiry-time cache that evicts data when data becomes stale
    ///     after a given number of seconds.
    ///     <para />
    ///     The cache reference type indicates which backing Map is used: Weak type uses the WeakHashMap,
    ///     Soft type uses the apache commons ReferenceMap, and Hard type simply uses a HashMap.
    /// </summary>
    public class HistoricalDataCacheExpiringImpl : HistoricalDataCache,
        ScheduleHandleCallback
    {
        private const string NAME_AUDITPROVIDER_SCHEDULE = "historical data-cache";
        
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly IDictionary<object, Item> _cache;
        private readonly long _scheduleSlot;
        private bool _isScheduled;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="maxAgeSec">is the maximum age in seconds</param>
        /// <param name="purgeIntervalSec">is the purge interval in seconds</param>
        /// <param name="cacheReferenceType">indicates whether hard, soft or weak references are used in the cache</param>
        /// <param name="agentInstanceContext">agent instance context</param>
        /// <param name="scheduleSlot">slot for scheduling callbacks for this cache</param>
        public HistoricalDataCacheExpiringImpl(
            double maxAgeSec,
            double purgeIntervalSec,
            CacheReferenceType cacheReferenceType,
            AgentInstanceContext agentInstanceContext,
            long scheduleSlot)
        {
            MaxAgeSec = maxAgeSec;
            PurgeIntervalSec = purgeIntervalSec;
            _agentInstanceContext = agentInstanceContext;
            _scheduleSlot = scheduleSlot;

            if (cacheReferenceType == CacheReferenceType.HARD) {
                _cache = new Dictionary<object, Item>()
                    .WithNullKeySupport();
            }
            else if (cacheReferenceType == CacheReferenceType.SOFT) {
                _cache = new ReferenceMap<object, Item>(
                        ReferenceType.SOFT,
                        ReferenceType.SOFT)
                    .WithNullKeySupport();
            }
            else {
                _cache = new WeakDictionary<object, Item>()
                    .WithNullKeySupport();
            }
        }

        /// <summary>
        ///     Returns the maximum age in milliseconds.
        /// </summary>
        /// <value>millisecon max age</value>
        protected double MaxAgeSec { get; }

        public double PurgeIntervalSec { get; }

        /// <summary>
        ///     Returns the current cache size.
        /// </summary>
        /// <value>cache size</value>
        protected long Size => _cache.Count;

        public EventTable[] GetCached(object methodParams)
        {
            var key = methodParams;
            if (!_cache.TryGetValue(key, out var item)) {
                return null;
            }

            var now = _agentInstanceContext.SchedulingService.Time;
            var maxAgeMSec =
                _agentInstanceContext.ImportServiceRuntime.TimeAbacus.DeltaForSecondsDouble(MaxAgeSec);
            if (now - item.Time > maxAgeMSec) {
                _cache.Remove(key);
                return null;
            }

            return item.Data;
        }

        public void Put(
            object methodParams,
            EventTable[] rows)
        {
            var schedulingService = _agentInstanceContext.SchedulingService;
            var timeAbacus = _agentInstanceContext.ImportServiceRuntime.TimeAbacus;

            var key = methodParams;
            var now = schedulingService.Time;
            var item = new Item(rows, now);
            _cache.Put(key, item);

            if (!_isScheduled) {
                var callback = new EPStatementHandleCallbackSchedule(
                    _agentInstanceContext.EpStatementAgentInstanceHandle,
                    this);
                var timeDelta = timeAbacus.DeltaForSecondsDouble(PurgeIntervalSec);
                _agentInstanceContext.AuditProvider.ScheduleAdd(
                    timeDelta,
                    _agentInstanceContext,
                    callback,
                    ScheduleObjectType.historicaldatacache,
                    NAME_AUDITPROVIDER_SCHEDULE);
                schedulingService.Add(timeDelta, callback, _scheduleSlot);
                _isScheduled = true;
            }
        }

        public bool IsActive => true;

        public void Destroy()
        {
        }

        public void ScheduledTrigger()
        {
            _agentInstanceContext.InstrumentationProvider.QHistoricalScheduledEval();

            // purge expired
            _agentInstanceContext.AuditProvider.ScheduleFire(
                _agentInstanceContext,
                ScheduleObjectType.historicaldatacache,
                NAME_AUDITPROVIDER_SCHEDULE);
            var now = _agentInstanceContext.SchedulingService.Time;
            var maxAgeMSec = _agentInstanceContext.ImportServiceRuntime.TimeAbacus.DeltaForSecondsDouble(MaxAgeSec);

            _cache
                .Where(entry => now - entry.Value.Time > maxAgeMSec)
                .ToList()
                .ForEach(entry => _cache.Remove(entry.Key));

            _isScheduled = false;

            _agentInstanceContext.InstrumentationProvider.AHistoricalScheduledEval();
        }

        private class Item
        {
            public Item(
                EventTable[] data,
                long time)
            {
                Data = data;
                Time = time;
            }

            public EventTable[] Data { get; }

            public long Time { get; }
        }
    }
} // end of namespace