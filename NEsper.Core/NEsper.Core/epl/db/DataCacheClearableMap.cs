///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// For use in iteration over historical joins, a <seealso cref="DataCache" /> implementation
    /// that serves to hold EventBean rows generated during a join evaluation
    /// involving historical streams stable for the same cache lookup keys.
    /// </summary>
    public class DataCacheClearableMap : DataCache
    {
        private readonly IDictionary<Object, EventTable[]> _cache;

        /// <summary>Ctor.</summary>
        public DataCacheClearableMap()
        {
            _cache = new Dictionary<Object, EventTable[]>();
        }

        public EventTable[] GetCached(Object[] methodParams, int numLookupKeys)
        {
            var key = DataCacheUtil.GetLookupKey(methodParams, numLookupKeys);
            return _cache.Get(key);
        }

        public void PutCached(Object[] methodParams, int numLookupKeys, EventTable[] rows)
        {
            var key = DataCacheUtil.GetLookupKey(methodParams, numLookupKeys);
            _cache.Put(key, rows);
        }

        public bool IsActive
        {
            get { return false; }
        }

        /// <summary>Clears the cache.</summary>
        public void Clear()
        {
            _cache.Clear();
        }

        public void Dispose()
        {
        }
    }
} // end of namespace
