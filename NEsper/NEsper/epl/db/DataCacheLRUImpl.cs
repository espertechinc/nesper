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
	/// <summary> Query result data cache implementation that uses a least-recently-used algorithm
	/// to store and evict query results.
	/// </summary>

    public class DataCacheLRUImpl : DataCache
    {
        private const float HashTableLoadFactor = 0.75f;

        private readonly int _cacheSize;
        private readonly LinkedHashMap<Object, EventTable[]> _cache;

        /// <summary> Ctor.</summary>
        /// <param name="cacheSize">is the maximum cache size
        /// </param>

        public DataCacheLRUImpl(int cacheSize)
        {
            _cacheSize = cacheSize;
            int hashTableCapacity = (int)Math.Ceiling(cacheSize / HashTableLoadFactor) + 1;
            _cache = new LinkedHashMap<Object, EventTable[]>(); // hashTableCapacity
            _cache.ShuffleOnAccess = true;
            _cache.RemoveEldest += delegate { return _cache.Count > _cacheSize; };
        }

	    /// <summary> Retrieves an entry from the cache.
	    /// The retrieved entry becomes the MRU (most recently used) entry.
	    /// </summary>
	    /// <param name="lookupKeys">the key whose associated value is to be returned.
	    /// </param>
	    /// <returns> the value associated to this key, or null if no value with this key exists in the cache.
	    /// </returns>
	    public EventTable[] GetCached(object[] lookupKeys)
        {
            var key = DataCacheUtil.GetLookupKey(lookupKeys);
            return _cache.Get(key);
        }

	    /// <summary>
	    /// Adds an entry to this cache.
	    /// If the cache is full, the LRU (least recently used) entry is dropped.
	    /// </summary>
	    /// <param name="keys">The keys.</param>
	    /// <param name="value">a value to be associated with the specified key.</param>
	    public void PutCached(object[] keys, EventTable[] value)
        {
            lock (this)
            {
                var key = DataCacheUtil.GetLookupKey(keys);
                _cache[key] = value;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
	    public void Dispose()
	    {
	    }

	    /// <summary> Returns the maximum cache size.</summary>
        /// <returns> maximum cache size
        /// </returns>
        public int CacheSize
        {
            get { return _cacheSize; }
        }

        public bool IsActive
        {
            get { return true; }
        }
    }
}
