///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.datacache
{
    /// <summary>
    ///     For use in iteration over historical joins, a <seealso cref="HistoricalDataCache" /> implementation
    ///     that serves to hold EventBean rows generated during a join evaluation
    ///     involving historical streams stable for the same cache lookup keys.
    /// </summary>
    public class HistoricalDataCacheClearableMap : HistoricalDataCache
    {
        private readonly IDictionary<object, EventTable[]> cache;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public HistoricalDataCacheClearableMap()
        {
            cache = new Dictionary<object, EventTable[]>().WithNullKeySupport();
        }

        public EventTable[] GetCached(object methodParams)
        {
            var key = methodParams;
            return cache.Get(key);
        }

        public void Put(
            object methodParams,
            EventTable[] rows)
        {
            var key = methodParams;
            cache.Put(key, rows);
        }

        public bool IsActive => false;

        public void Destroy()
        {
        }

        /// <summary>
        ///     Clears the cache.
        /// </summary>
        public void Clear()
        {
            cache.Clear();
        }
    }
} // end of namespace