///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Implements an expiry-time cache that evicts data when data becomes stale
    /// after a given number of seconds.
    /// <para>
    /// The cache reference type indicates which backing Map is used: Weak type uses the WeakHashMap,
    /// Soft type uses the apache commons ReferenceMap, and Hard type simply uses a HashMap.
    /// </para>
    /// </summary>
    public class DataCacheExpiringImpl
        : DataCache
        , ScheduleHandleCallback
    {
        private readonly double _maxAgeSec;
        private readonly double _purgeIntervalSec;
        private readonly SchedulingService _schedulingService;
        private readonly long _scheduleSlot;
        private readonly IDictionary<Object, Item> _cache;
        private readonly EPStatementAgentInstanceHandle _epStatementAgentInstanceHandle;
        private readonly TimeAbacus _timeAbacus;

        private bool _isScheduled;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="maxAgeSec">is the maximum age in seconds</param>
        /// <param name="purgeIntervalSec">is the purge interval in seconds</param>
        /// <param name="cacheReferenceType">indicates whether hard, soft or weak references are used in the cache</param>
        /// <param name="schedulingService">is a service for call backs at a scheduled time, for purging</param>
        /// <param name="scheduleSlot">slot for scheduling callbacks for this cache</param>
        /// <param name="epStatementAgentInstanceHandle">is the statements-own handle for use in registering callbacks with services</param>
        /// <param name="timeAbacus">time abacus</param>
        public DataCacheExpiringImpl(
            double maxAgeSec,
            double purgeIntervalSec,
            ConfigurationCacheReferenceType cacheReferenceType,
            SchedulingService schedulingService,
            long scheduleSlot,
            EPStatementAgentInstanceHandle epStatementAgentInstanceHandle,
            TimeAbacus timeAbacus)
        {
            _maxAgeSec = maxAgeSec;
            _purgeIntervalSec = purgeIntervalSec;
            _schedulingService = schedulingService;
            _scheduleSlot = scheduleSlot;
            _timeAbacus = timeAbacus;

            if (cacheReferenceType == ConfigurationCacheReferenceType.HARD)
            {
                _cache = new Dictionary<Object, Item>();
            }
            else if (cacheReferenceType == ConfigurationCacheReferenceType.SOFT)
            {
                _cache = new ReferenceMap<Object, Item>(ReferenceType.SOFT, ReferenceType.SOFT);
            }
            else
            {
                _cache = new WeakDictionary<Object, Item>();
            }

            _epStatementAgentInstanceHandle = epStatementAgentInstanceHandle;
        }

        public EventTable[] GetCached(Object[] methodParams, int numLookupKeys)
        {
            var key = DataCacheUtil.GetLookupKey(methodParams, numLookupKeys);
            var item = _cache.Get(key);
            if (item == null)
            {
                return null;
            }

            var now = _schedulingService.Time;
            long maxAgeMSec = _timeAbacus.DeltaForSecondsDouble(_maxAgeSec);
            if ((now - item.Time) > maxAgeMSec)
            {
                _cache.Remove(key);
                return null;
            }

            return item.Data;
        }

        public void PutCached(Object[] methodParams, int numLookupKeys, EventTable[] rows)
        {
            var key = DataCacheUtil.GetLookupKey(methodParams, numLookupKeys);
            var now = _schedulingService.Time;
            var item = new Item(rows, now);
            _cache.Put(key, item);

            if (!_isScheduled)
            {
                var callback = new EPStatementHandleCallback(_epStatementAgentInstanceHandle, this);
                _schedulingService.Add(_timeAbacus.DeltaForSecondsDouble(_purgeIntervalSec), callback, _scheduleSlot);
                _isScheduled = true;
            }
        }

        /// <summary>
        /// Returns the maximum age in milliseconds.
        /// </summary>
        /// <value>millisecon max age</value>
        protected double MaxAgeSec
        {
            get { return _maxAgeSec; }
        }

        public double PurgeIntervalSec
        {
            get { return _purgeIntervalSec; }
        }

        public bool IsActive
        {
            get { return true; }
        }

        /// <summary>
        /// Returns the current cache size.
        /// </summary>
        /// <value>cache size</value>
        protected long Count
        {
            get { return _cache.Count; }
        }

        public void ScheduledTrigger(EngineLevelExtensionServicesContext engineLevelExtensionServicesContext)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QHistoricalScheduledEval();
            }
            // purge expired
            var now = _schedulingService.Time;
            var maxAgeMSec = _timeAbacus.DeltaForSecondsDouble(_maxAgeSec);

            _cache
                .Where(entry => ((now - entry.Value.Time) > maxAgeMSec))
                .Select(entry => entry.Key)
                .ToList()
                .ForEach(itemKey => _cache.Remove(itemKey));

            _isScheduled = false;
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AHistoricalScheduledEval();
            }
        }

        public void Dispose()
        {
        }

        private class Item
        {
            public Item(EventTable[] data, long time)
            {
                Data = data;
                Time = time;
            }

            public EventTable[] Data { get; private set; }
            public long Time { get; private set; }
        }
    }
} // end of namespace
