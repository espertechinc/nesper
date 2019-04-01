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
    ///     Expiring cache settings.
    /// </summary>
    [Serializable]
    public class ConfigurationCommonCacheExpiryTime : ConfigurationCommonCache
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="maxAgeSeconds">is the maximum age in seconds</param>
        /// <param name="purgeIntervalSeconds">is the purge interval</param>
        /// <param name="cacheReferenceType">
        ///     the reference type may allow garbage collection to remove entries fromcache unless HARD reference type indicates
        ///     otherwise
        /// </param>
        public ConfigurationCommonCacheExpiryTime(
            double maxAgeSeconds, double purgeIntervalSeconds, CacheReferenceType cacheReferenceType)
        {
            MaxAgeSeconds = maxAgeSeconds;
            PurgeIntervalSeconds = purgeIntervalSeconds;
            CacheReferenceType = cacheReferenceType;
        }

        /// <summary>
        ///     Returns the maximum age in seconds.
        /// </summary>
        /// <returns>number of seconds</returns>
        public double MaxAgeSeconds { get; }

        /// <summary>
        ///     Returns the purge interval length.
        /// </summary>
        /// <returns>purge interval in seconds</returns>
        public double PurgeIntervalSeconds { get; }

        /// <summary>
        ///     Returns the enumeration whether hard, soft or weak reference type are used
        ///     to control whether the garbage collection can remove entries from cache.
        /// </summary>
        /// <returns>reference type</returns>
        public CacheReferenceType CacheReferenceType { get; }

        public override string ToString()
        {
            return "ExpiryTimeCacheDesc maxAgeSeconds=" + MaxAgeSeconds + " purgeIntervalSeconds=" +
                   PurgeIntervalSeconds;
        }
    }
} // end of namespace