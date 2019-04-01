///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Factory for data caches for use caching database query results and method invocation results.
    /// </summary>
    public class DataCacheFactory {
        /// <summary>
        /// Creates a cache implementation for the strategy as defined by the cache descriptor.
        /// </summary>
        /// <param name="cacheDesc">cache descriptor</param>
        /// <param name="epStatementAgentInstanceHandle">statement handle for timer invocations</param>
        /// <param name="schedulingService">scheduling service for time-based caches</param>
        /// <param name="scheduleBucket">for ordered timer invokation</param>
        /// <param name="statementContext">statement context</param>
        /// <param name="streamNum">stream number</param>
        /// <returns>data cache implementation</returns>
        public DataCache GetDataCache(ConfigurationDataCache cacheDesc,
                                      StatementContext statementContext,
                                      EPStatementAgentInstanceHandle epStatementAgentInstanceHandle,
                                      SchedulingService schedulingService,
                                      ScheduleBucket scheduleBucket,
                                      int streamNum) {
            if (cacheDesc == null) {
                return new DataCacheNullImpl();
            }
    
            if (cacheDesc is ConfigurationLRUCache) {
                ConfigurationLRUCache lruCache = (ConfigurationLRUCache) cacheDesc;
                return new DataCacheLRUImpl(lruCache.Size);
            }
    
            if (cacheDesc is ConfigurationExpiryTimeCache) {
                ConfigurationExpiryTimeCache expCache = (ConfigurationExpiryTimeCache) cacheDesc;
                return MakeTimeCache(expCache, statementContext, epStatementAgentInstanceHandle, schedulingService, scheduleBucket, streamNum);
            }
    
            throw new IllegalStateException("Cache implementation class not configured");
        }
    
        protected DataCache MakeTimeCache(ConfigurationExpiryTimeCache expCache, StatementContext statementContext, EPStatementAgentInstanceHandle epStatementAgentInstanceHandle, SchedulingService schedulingService, ScheduleBucket scheduleBucket, int streamNum) {
            return new DataCacheExpiringImpl(expCache.MaxAgeSeconds, expCache.PurgeIntervalSeconds, expCache.CacheReferenceType,
                    schedulingService, scheduleBucket.AllocateSlot(), epStatementAgentInstanceHandle, statementContext.TimeAbacus);
        }
    }
} // end of namespace
