///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.client.configuration.common
{
    /// <summary>
    ///     Holds configuration information for data caches for use in method invocations in the from-clause.
    /// </summary>
    [Serializable]
    public class ConfigurationCommonMethodRef
    {
        /// <summary>
        ///     Return a method invocation result data cache descriptor.
        /// </summary>
        /// <returns>cache descriptor</returns>
        public ConfigurationCommonCache DataCacheDesc { get; set; }

        /// <summary>
        ///     Configures a LRU cache of the given size for the method invocation.
        /// </summary>
        /// <param name="size">is the maximum number of entries before method invocation results are evicted</param>
        public void SetLRUCache(int size)
        {
            DataCacheDesc = new ConfigurationCommonCacheLRU(size);
        }

        /// <summary>
        ///     Configures an expiry-time cache of the given maximum age in seconds and purge interval in seconds.
        ///     <para />
        ///     Specifies the cache reference type to be weak references. Weak reference cache entries become
        ///     eligible for garbage collection and are removed from cache when the garbage collection requires so.
        /// </summary>
        /// <param name="maxAgeSeconds">
        ///     is the maximum number of seconds before a method invocation result is considered stale
        ///     (also known as time-to-live)
        /// </param>
        /// <param name="purgeIntervalSeconds">is the interval at which the runtimepurges stale data from the cache</param>
        public void SetExpiryTimeCache(
            double maxAgeSeconds,
            double purgeIntervalSeconds)
        {
            DataCacheDesc = new ConfigurationCommonCacheExpiryTime(
                maxAgeSeconds,
                purgeIntervalSeconds,
                CacheReferenceType.DEFAULT);
        }

        /// <summary>
        ///     Configures an expiry-time cache of the given maximum age in seconds and purge interval in seconds. Also allows
        ///     setting the reference type indicating whether garbage collection may remove entries from cache.
        /// </summary>
        /// <param name="maxAgeSeconds">
        ///     is the maximum number of seconds before a method invocation result is considered stale
        ///     (also known as time-to-live)
        /// </param>
        /// <param name="purgeIntervalSeconds">is the interval at which the runtimepurges stale data from the cache</param>
        /// <param name="cacheReferenceType">specifies the reference type to use</param>
        public void SetExpiryTimeCache(
            double maxAgeSeconds,
            double purgeIntervalSeconds,
            CacheReferenceType cacheReferenceType)
        {
            DataCacheDesc = new ConfigurationCommonCacheExpiryTime(
                maxAgeSeconds,
                purgeIntervalSeconds,
                cacheReferenceType);
        }
    }
} // end of namespace