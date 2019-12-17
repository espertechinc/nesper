///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.historical.datacache
{
    /// <summary>
    /// Factory for data caches for use caching database query results and method invocation results.
    /// </summary>
    public class HistoricalDataCacheFactory
    {
        /// <summary>
        /// Creates a cache implementation for the strategy as defined by the cache descriptor.
        /// </summary>
        /// <param name="cacheDesc">cache descriptor</param>
        /// <param name="agentInstanceContext">agent instance context</param>
        /// <param name="streamNum">stream number</param>
        /// <param name="scheduleCallbackId">callback id</param>
        /// <returns>data cache implementation</returns>
        public HistoricalDataCache GetDataCache(
            ConfigurationCommonCache cacheDesc,
            AgentInstanceContext agentInstanceContext,
            int streamNum,
            int scheduleCallbackId)
        {
            if (cacheDesc == null) {
                return new HistoricalDataCacheNullImpl();
            }

            if (cacheDesc is ConfigurationCommonCacheLRU) {
                ConfigurationCommonCacheLRU lruCache = (ConfigurationCommonCacheLRU) cacheDesc;
                return new HistoricalDataCacheLRUImpl(lruCache.Size);
            }

            if (cacheDesc is ConfigurationCommonCacheExpiryTime) {
                ConfigurationCommonCacheExpiryTime expCache = (ConfigurationCommonCacheExpiryTime) cacheDesc;
                return MakeTimeCache(expCache, agentInstanceContext, streamNum, scheduleCallbackId);
            }

            throw new IllegalStateException("Cache implementation class not configured");
        }

        protected HistoricalDataCache MakeTimeCache(
            ConfigurationCommonCacheExpiryTime expCache,
            AgentInstanceContext agentInstanceContext,
            int streamNum,
            int scheduleCallbackId)
        {
            return new HistoricalDataCacheExpiringImpl(
                expCache.MaxAgeSeconds,
                expCache.PurgeIntervalSeconds,
                expCache.CacheReferenceType,
                agentInstanceContext,
                agentInstanceContext.ScheduleBucket.AllocateSlot());
        }
    }
} // end of namespace