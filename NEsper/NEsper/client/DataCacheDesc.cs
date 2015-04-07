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
    /// <summary>
    ///  Marker for different cache settings.
    /// </summary>
    
    public interface DataCacheDesc
    {
    }

    /// <summary>
    /// LRU cache settings.
    /// </summary>
    [Serializable]
    public class LRUCacheDesc : DataCacheDesc
    {
        /// <summary> Returns the maximum cache size.</summary>
        /// <returns> max cache size
        /// </returns>
        public int Size { get; private set; }

        /// <summary> Ctor.</summary>
        /// <param name="size">is the maximum cache size
        /// </param>
        public LRUCacheDesc(int size)
        {
            Size = size;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            return "LRUCacheDesc size=" + Size;
        }
    }

    /// <summary>
    ///  Expiring cache settings.
    /// </summary>
    
    [Serializable]
    public class ExpiryTimeCacheDesc : DataCacheDesc
    {
        /// <summary> Returns the maximum age in seconds.</summary>
        /// <returns> number of seconds
        /// </returns>
        public double MaxAgeSeconds { get; private set; }

        /// <summary> Returns the purge interval length.</summary>
        /// <returns> purge interval in seconds
        /// </returns>
        public double PurgeIntervalSeconds { get; private set; }

        /// <summary>
        /// Returns the enumeration whether hard, soft or weak reference type are used
        /// to control whether the garbage collection can remove entries from cache.
        /// </summary>
        /// <value>The type of the cache reference.</value>
        public ConfigurationCacheReferenceType ConfigurationCacheReferenceType { get; private set; }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="maxAgeSeconds">is the maximum age in seconds</param>
        /// <param name="purgeIntervalSeconds">is the purge interval</param>
        /// <param name="configurationCacheReferenceType">cacheReferenceType the reference type may allow garbage collection to remove entries from
        /// cache unless HARD reference type indicates otherwise</param>
        public ExpiryTimeCacheDesc(double maxAgeSeconds, double purgeIntervalSeconds, ConfigurationCacheReferenceType configurationCacheReferenceType)
        {
            MaxAgeSeconds = maxAgeSeconds;
            PurgeIntervalSeconds = purgeIntervalSeconds;
            ConfigurationCacheReferenceType = configurationCacheReferenceType;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            return "ExpiryTimeCacheDesc maxAgeSeconds=" + MaxAgeSeconds + " purgeIntervalSeconds=" + PurgeIntervalSeconds;
        }
    }
}
