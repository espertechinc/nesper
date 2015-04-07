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

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Implements an expiry-time cache that evicts data when data becomes stale after a given
    /// number of seconds.
    /// <para/>
    /// The cache reference type indicates which backing Map is used: Weak type uses the WeakHashMap,
    /// Soft type uses the apache commons ReferenceMap, and Hard type simply uses a HashMap.
    /// </summary>
    public class DataCacheExpiringImpl : DataCache, ScheduleHandleCallback
    {
        private readonly IDictionary<Object, Item> _cache;
        private readonly EPStatementAgentInstanceHandle _epStatementAgentInstanceHandle;
        private readonly SchedulingService _schedulingService;
        private readonly ScheduleSlot _scheduleSlot;
    
        private bool _isScheduled;
    
        /// <summary>Ctor. </summary>
        /// <param name="maxAgeSec">is the maximum age in seconds</param>
        /// <param name="purgeIntervalSec">is the purge interval in seconds</param>
        /// <param name="cacheReferenceType">indicates whether hard, soft or weak references are used in the cache</param>
        /// <param name="schedulingService">is a service for call backs at a scheduled time, for purging</param>
        /// <param name="scheduleSlot">slot for scheduling callbacks for this cache</param>
        /// <param name="epStatementAgentInstanceHandle">is the statements-own handle for use in registering callbacks with services</param>
        public DataCacheExpiringImpl(double maxAgeSec,
                                     double purgeIntervalSec,
                                     ConfigurationCacheReferenceType cacheReferenceType,
                                     SchedulingService schedulingService,
                                     ScheduleSlot scheduleSlot,
                                     EPStatementAgentInstanceHandle epStatementAgentInstanceHandle)
        {
            MaxAgeMSec = (long) maxAgeSec * 1000;
            PurgeIntervalMSec = (long) purgeIntervalSec * 1000;
            _schedulingService = schedulingService;
            _scheduleSlot = scheduleSlot;
    
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

        /// <summary>Returns the maximum age in milliseconds. </summary>
        /// <value>millisecon max age</value>
        public long MaxAgeMSec { get; private set; }

        /// <summary>Returns the purge interval in milliseconds. </summary>
        /// <value>millisecond purge interval</value>
        public long PurgeIntervalMSec { get; private set; }

        /// <summary>Returns the current cache size. </summary>
        /// <value>cache size</value>
        public long Count
        {
            get { return _cache.Count; }
        }

        public bool IsActive
        {
            get { return true; }
        }

        public EventTable[] GetCached(object[] lookupKeys)
        {
            var key = DataCacheUtil.GetLookupKey(lookupKeys);
            var item = _cache.Get(key);
            if (item == null)
            {
                return null;
            }
    
            var now = _schedulingService.Time;
            if ((now - item.Time) > MaxAgeMSec)
            {
                _cache.Remove(key);
                return null;
            }
    
            return item.Data;
        }
    
        public void PutCached(object[] lookupKeys, EventTable[] rows)
        {
            var key = DataCacheUtil.GetLookupKey(lookupKeys);
            var now = _schedulingService.Time;
            var item = new Item(rows, now);
            _cache.Put(key, item);
    
            if (!_isScheduled)
            {
                var callback = new EPStatementHandleCallback(_epStatementAgentInstanceHandle, this);
                _schedulingService.Add(PurgeIntervalMSec, callback, _scheduleSlot);
                _isScheduled = true;
            }
        }

        public void ScheduledTrigger(ExtensionServicesContext extensionServicesContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHistoricalScheduledEval();}
            // purge expired
            var now = _schedulingService.Time;

            _cache.Where(entry => MaxAgeMSec < (now - entry.Value.Time))
                .Select(entry => entry.Key)
                .ToList()                .ForEach(key => _cache.Remove(key));
            _isScheduled = false;
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHistoricalScheduledEval();}
        }
    
        private class Item
        {
            public EventTable[] Data { get; private set; }
            public long Time { get; private set; }

            public Item(EventTable[] data, long time)
            {
                Data = data;
                Time = time;
            }
        }
    }
}
