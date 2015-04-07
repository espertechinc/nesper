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
    /// Holds configuration information for data caches for use in method invocations in the from-clause.
    /// </summary>
	[Serializable]
    public class ConfigurationMethodRef
	{
        /// <summary>
        /// Return a method invocation result data cache descriptor.
        /// </summary>
        /// <returns>cache descriptor</returns>
        public ConfigurationDataCache DataCacheDesc { get; private set; }

        /// <summary>
        /// Configures a LRU cache of the given size for the method invocation.
        /// </summary>
        /// <param name="size">is the maximum number of entries before method invocation results are evicted</param>
	    public void SetLRUCache(int size)
	    {
            DataCacheDesc = new ConfigurationLRUCache(size);
	    }

        /// <summary>
        /// Configures an expiry-time cache of the given maximum age in seconds and purge interval in seconds.
        /// <para>
        /// Specifies the cache reference type to be weak references. Weak reference cache entries become
        /// eligible for garbage collection and are removed from cache when the garbage collection requires so.
        /// </para>
        /// </summary>
        /// <param name="maxAgeSeconds">is the maximum number of seconds before a method invocation result is considered stale (also known as time-to-live)</param>
        /// <param name="purgeIntervalSeconds">is the interval at which the engine purges stale data from the cache</param>
	    public void SetExpiryTimeCache(double maxAgeSeconds, double purgeIntervalSeconds)
	    {
            DataCacheDesc = new ConfigurationExpiryTimeCache(maxAgeSeconds, purgeIntervalSeconds, ConfigurationCacheReferenceTypeHelper.GetDefault());
	    }

        /// <summary>
        /// Configures an expiry-time cache of the given maximum age in seconds and purge interval in seconds. Also allows
        /// setting the reference type indicating whether garbage collection may remove entries from cache.
        /// </summary>
        /// <param name="maxAgeSeconds">is the maximum number of seconds before a method invocation result is considered stale (also known as time-to-live)</param>
        /// <param name="purgeIntervalSeconds">is the interval at which the engine purges stale data from the cache</param>
        /// <param name="configurationCacheReferenceType">specifies the reference type to use</param>
	    public void SetExpiryTimeCache(double maxAgeSeconds, double purgeIntervalSeconds, ConfigurationCacheReferenceType configurationCacheReferenceType)
	    {
            DataCacheDesc = new ConfigurationExpiryTimeCache(maxAgeSeconds, purgeIntervalSeconds, configurationCacheReferenceType);
	    }
	}
} // End of namespace
