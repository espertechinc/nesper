///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.datacache
{
    /// <summary>
    ///     Query result data cache implementation that uses a least-recently-used algorithm
    ///     to store and evict query results.
    /// </summary>
    public class HistoricalDataCacheLRUImpl : HistoricalDataCache
    {
        private static readonly float HASH_TABLE_LOAD_FACTOR = 0.75f;
        private readonly LinkedHashMap<object, EventTable[]> cache;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="cacheSize">is the maximum cache size</param>
        public HistoricalDataCacheLRUImpl(int cacheSize)
        {
            CacheSize = cacheSize;
            var hashTableCapacity = (int) Math.Ceiling(cacheSize / HASH_TABLE_LOAD_FACTOR) + 1;
            cache = new LinkedHashMap<object, EventTable[]>();
            cache.RemoveEldest += entry => cache.Count > cacheSize;
        }

        /// <summary>
        ///     Returns the maximum cache size.
        /// </summary>
        /// <value>maximum cache size</value>
        public int CacheSize { get; }

        public void Destroy()
        {
        }

        /// <summary>
        ///     Retrieves an entry from the cache.
        ///     The retrieved entry becomes the MRU (most recently used) entry.
        /// </summary>
        /// <param name="methodParams">the key whose associated value is to be returned.</param>
        /// <returns>the value associated to this key, or null if no value with this key exists in the cache.</returns>
        public EventTable[] GetCached(object methodParams)
        {
            lock (this) {
                var key = methodParams;
                return cache.Get(key);
            }
        }

        /// <summary>
        ///     Adds an entry to this cache.
        ///     If the cache is full, the LRU (least recently used) entry is dropped.
        /// </summary>
        /// <param name="methodParams">the keys with which the specified value is to be associated.</param>
        /// <param name="rows">a value to be associated with the specified key.</param>
        public void Put(
            object methodParams,
            EventTable[] rows)
        {
            lock (this) {
                var key = methodParams;
                cache.Put(key, rows);
            }
        }

        public bool IsActive => true;
    }
}