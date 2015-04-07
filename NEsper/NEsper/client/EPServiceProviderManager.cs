///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Factory for instances of <seealso cref="EPServiceProviderSPI"/>.
    /// </summary>
    public sealed class EPServiceProviderManager
    {
        private static readonly ILockable LockObj;
        private static readonly IDictionary<String, EPServiceProviderSPI> Runtimes;

        static EPServiceProviderManager()
        {
            LockObj = LockManager.CreateLock(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            Runtimes = new ConcurrentDictionary<string, EPServiceProviderSPI>();
        }

        /// <summary>
        /// Returns a collection of known provider URIs.
        /// <para/>
        /// Returns a the value "default" for the default provider.
        /// <para/>
        /// Returns URIs for all engine instances including destroyed instances.
        /// </summary>
        public static ICollection<string> ProviderURIs
        {
            get { return new List<string>(Runtimes.Keys); }
        }


        /// <summary> 
        /// Returns the default EPServiceProvider.
        /// The URI value for the service returned is "default".
        /// </summary>
        /// <returns> default instance of the service.
        /// </returns>

        public static EPServiceProvider GetDefaultProvider()
        {
            return GetProvider(EPServiceProviderConstants.DEFAULT_ENGINE_URI, new Configuration());
        }

        /// <summary> 
        /// Returns the default EPServiceProvider.
        /// The URI value for the service returned is "default".
        /// </summary>
        /// <param name="configuration">is the configuration for the service
        /// </param>
        /// <returns> default instance of the service.
        /// </returns>
        /// <throws>  ConfigurationException to indicate a configuration problem </throws>

        public static EPServiceProvider GetDefaultProvider(Configuration configuration)
        {
            return GetProvider(EPServiceProviderConstants.DEFAULT_ENGINE_URI, configuration);
        }

        /// <summary>
        /// Returns an EPServiceProvider for a given provider URI.
        /// Use the URI of "default" or null to return the default service provider.
        /// </summary>
        /// <param name="providerURI">the provider URI</param>
        /// <returns>EPServiceProvider for the given provider URI.</returns>
        public static EPServiceProvider GetProvider(String providerURI)
        {
            return GetProvider(providerURI, new Configuration());
        }

        /// <summary>
        /// Returns an EPServiceProvider for a given provider URI.
        /// Use the URI of "default" or null to return the default service provider.
        /// </summary>
        /// <param name="providerURI">the provider URI.  If null provided it assumes "default".</param>
        /// <param name="configuration">is the configuration for the service</param>
        /// <returns>EPServiceProvider for the given provider URI.</returns>
        /// <throws>ConfigurationException to indicate a configuration problem</throws>
        public static EPServiceProvider GetProvider(String providerURI, Configuration configuration)
        {
            using (LockObj.Acquire())
            {
                if (String.IsNullOrEmpty(providerURI))
                {
                    providerURI = EPServiceProviderConstants.DEFAULT_ENGINE_URI;
                }

                if (Runtimes.ContainsKey(providerURI))
                {
                    var provider = Runtimes[providerURI];
                    if (provider.IsDestroyed)
                    {
                        provider = new EPServiceProviderImpl(configuration, providerURI, Runtimes);
                        Runtimes[providerURI] = provider;
                    }
                    else
                    {
                        provider.SetConfiguration(configuration);
                    }

                    return provider;
                }

                // New runtime
                EPServiceProviderImpl runtime = new EPServiceProviderImpl(configuration, providerURI, Runtimes);
                Runtimes[providerURI] = runtime;
                runtime.PostInitialize();

                return runtime;
            }
        }

        /// <summary>
        /// Gets the existing provider.
        /// </summary>
        /// <param name="providerURI">the provider URI. If null provided it assumes "default".</param>
        /// <returns>
        /// Returns an existing provider. Returns null if the provider for the given URI has not been initialized
        /// or the provider for the given URI is in destroyed state.
        /// </returns>
        public static EPServiceProvider GetExistingProvider(String providerURI)
        {
            var providerURINonNull = providerURI ?? EPServiceProviderConstants.DEFAULT_ENGINE_URI;
            var provider = Runtimes.Get(providerURINonNull);
            if (provider == null || provider.IsDestroyed)
            {
                return null;
            }
            return provider;
        }

        /// <summary>
        /// Clears references to the provider.
        /// </summary>
        /// <param name="providerURI"></param>

        public static void PurgeProvider(String providerURI)
        {
            using (LockObj.Acquire())
            {
                if (String.IsNullOrEmpty(providerURI))
                {
                    providerURI = EPServiceProviderConstants.DEFAULT_ENGINE_URI;
                }

                Runtimes.Remove(providerURI);
            }
        }

        /// <summary>
        /// Clears references to the default provider.
        /// </summary>

        public static void PurgeDefaultProvider()
        {
            PurgeProvider(null);
        }

        /// <summary>
        /// Purges all providers.
        /// </summary>
        public static void PurgeAllProviders()
        {
            using (LockObj.Acquire())
            {
                Runtimes.Clear();
            }
        }
    }
}