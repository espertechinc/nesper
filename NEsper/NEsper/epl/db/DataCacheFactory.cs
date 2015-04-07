///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.schedule;


namespace com.espertech.esper.epl.db
{
    /// <summary>Factory for data caches for use caching database query results and method invocation results. </summary>
    public class DataCacheFactory
    {
        /// <summary>Creates a cache implementation for the strategy as defined by the cache descriptor. </summary>
        /// <param name="cacheDesc">cache descriptor</param>
        /// <param name="epStatementAgentInstanceHandle">statement handle for timer invocations</param>
        /// <param name="schedulingService">scheduling service for time-based caches</param>
        /// <param name="scheduleBucket">for ordered timer invokation</param>
        /// <returns>data cache implementation</returns>
        public static DataCache GetDataCache(ConfigurationDataCache cacheDesc,
                                             EPStatementAgentInstanceHandle epStatementAgentInstanceHandle,
                                             SchedulingService schedulingService,
                                             ScheduleBucket scheduleBucket)
        {
            if (cacheDesc == null)
            {
                return new DataCacheNullImpl();
            }
    
            if (cacheDesc is ConfigurationLRUCache)
            {
                ConfigurationLRUCache lruCache = (ConfigurationLRUCache) cacheDesc;
                return new DataCacheLRUImpl(lruCache.Size);
            }
    
            if (cacheDesc is ConfigurationExpiryTimeCache)
            {
                ConfigurationExpiryTimeCache expCache = (ConfigurationExpiryTimeCache) cacheDesc;
                return new DataCacheExpiringImpl(expCache.MaxAgeSeconds, expCache.PurgeIntervalSeconds, expCache.CacheReferenceType,
                        schedulingService, scheduleBucket.AllocateSlot(), epStatementAgentInstanceHandle);
            }
    
            throw new IllegalStateException("Cache implementation class not configured");
        }
    }
}
