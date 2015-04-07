///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client
{
    /// <summary>Expiring cache settings. </summary>
    [Serializable]
    public class ConfigurationExpiryTimeCache : ConfigurationDataCache
    {
        private ConfigurationCacheReferenceType cacheReferenceType;
        private double maxAgeSeconds;
        private double purgeIntervalSeconds;

        /// <summary>Ctor. </summary>
        /// <param name="maxAgeSeconds">is the maximum age in seconds</param>
        /// <param name="purgeIntervalSeconds">is the purge interval</param>
        /// <param name="cacheReferenceType">the reference type may allow garbage collection to remove entries fromcache unless HARD reference type indicates otherwise </param>
        public ConfigurationExpiryTimeCache(double maxAgeSeconds, double purgeIntervalSeconds, ConfigurationCacheReferenceType cacheReferenceType)
        {
            this.maxAgeSeconds = maxAgeSeconds;
            this.purgeIntervalSeconds = purgeIntervalSeconds;
            this.cacheReferenceType = cacheReferenceType;
        }


        /// <summary>
        /// Gets the type of the cache reference.
        /// </summary>
        /// <value>The type of the cache reference.</value>
        public ConfigurationCacheReferenceType CacheReferenceType
        {
            get { return cacheReferenceType; }
        }

        /// <summary>
        /// Gets the max age in seconds.
        /// </summary>
        /// <value>The max age seconds.</value>
        public double MaxAgeSeconds
        {
            get { return maxAgeSeconds; }
        }

        /// <summary>
        /// Gets the purge interval in seconds.
        /// </summary>
        /// <value>The purge interval seconds.</value>
        public double PurgeIntervalSeconds
        {
            get { return purgeIntervalSeconds; }
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override String ToString()
        {
            return "ExpiryTimeCacheDesc maxAgeSeconds=" + maxAgeSeconds + " purgeIntervalSeconds=" + purgeIntervalSeconds;
        }
    }
}
