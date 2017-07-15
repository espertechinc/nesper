///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Query result data _cache implementation that uses a least-recently-used algorithm
    /// to store and evict query results.
    /// </summary>
    public class DataCacheLRUImpl : DataCache
    {
        private static readonly float HASH_TABLE_LOAD_FACTOR = 0.75f;
        private readonly int _cacheSize;
        private readonly LinkedHashMap<Object, EventTable[]> _cache;
        private readonly object _iLock = new object();
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="cacheSize">is the maximum _cache size</param>
        public DataCacheLRUImpl(int cacheSize) {
            _cacheSize = cacheSize;
            int hashTableCapacity = (int)Math.Ceiling(cacheSize / HASH_TABLE_LOAD_FACTOR) + 1;
            _cache = new LinkedHashMap<Object, EventTable[]>(); // hashTableCapacity
            _cache.ShuffleOnAccess = true;
            _cache.RemoveEldest += delegate { return _cache.Count > _cacheSize; };
        }
    
        /// <summary>
        /// Retrieves an entry from the _cache.
        /// The retrieved entry becomes the MRU (most recently used) entry.
        /// </summary>
        /// <param name="methodParams">the key whose associated value is to be returned.</param>
        /// <returns>
        /// the value associated to this key, or null if no value with this key exists in the _cache.
        /// </returns>
        public EventTable[] GetCached(Object[] methodParams, int numInputParameters) {
            var key = DataCacheUtil.GetLookupKey(methodParams, numInputParameters);
            return _cache.Get(key);
        }
    
        /// <summary>
        /// Adds an entry to this _cache.
        /// If the _cache is full, the LRU (least recently used) entry is dropped.
        /// </summary>
        /// <param name="methodParams">the keys with which the specified value is to be associated.</param>
        /// <param name="rows">a value to be associated with the specified key.</param>
        public void PutCached(Object[] methodParams, int numLookupKeys, EventTable[] rows)
        {
            lock (_iLock)
            {
                var key = DataCacheUtil.GetLookupKey(methodParams, numLookupKeys);
                _cache.Put(key, rows);
            }
        }

        /// <summary>
        /// Returns the maximum _cache size.
        /// </summary>
        /// <value>maximum _cache size</value>
        public int CacheSize
        {
            get { return _cacheSize; }
        }

        public bool IsActive
        {
            get { return true; }
        }

        public void Dispose()
        {
        }
    }
} // end of namespace
