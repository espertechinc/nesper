///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Factory for instances of <seealso cref="EPServiceProvider" />.
    /// </summary>
    public sealed class EPServiceProviderManager {
        private static IDictionary<string, EPServiceProviderSPI> runtimes = new ConcurrentDictionary<string, EPServiceProviderSPI>();
    
        /// <summary>
        /// Returns the default EPServiceProvider. The URI value for the service returned is "default".
        /// </summary>
        /// <returns>default instance of the service.</returns>
        public static EPServiceProvider GetDefaultProvider() {
            return GetProvider(EPServiceProviderSPI.DEFAULT_ENGINE_URI, new Configuration());
        }
    
        /// <summary>
        /// Returns the default EPServiceProvider. The URI value for the service returned is "default".
        /// </summary>
        /// <param name="configuration">is the configuration for the service</param>
        /// <exception cref="ConfigurationException">to indicate a configuration problem</exception>
        /// <returns>default instance of the service.</returns>
        public static EPServiceProvider GetDefaultProvider(Configuration configuration) {
            return GetProvider(EPServiceProviderSPI.DEFAULT_ENGINE_URI, configuration);
        }
    
        /// <summary>
        /// Returns an EPServiceProvider for a given provider URI.
        /// <para>
        /// Use the URI of "default" or null to return the default service provider.
        /// </para>
        /// </summary>
        /// <param name="providerURI">- the provider URI</param>
        /// <returns>EPServiceProvider for the given provider URI.</returns>
        public static EPServiceProvider GetProvider(string providerURI) {
            return GetProvider(providerURI, new Configuration());
        }
    
        /// <summary>
        /// Returns an EPServiceProvider for a given provider URI.
        /// Use the URI of "default" or null to return the default service provider.
        /// </summary>
        /// <param name="providerURI">- the provider URI. If null provided it assumes "default".</param>
        /// <param name="configuration">is the configuration for the service</param>
        /// <exception cref="ConfigurationException">to indicate a configuration problem</exception>
        /// <returns>EPServiceProvider for the given provider URI.</returns>
        public static EPServiceProvider GetProvider(string providerURI, Configuration configuration) {
            string providerURINonNull = (providerURI == null) ? EPServiceProviderSPI.DEFAULT_ENGINE_URI : providerURI;
    
            if (runtimes.ContainsKey(providerURINonNull)) {
                EPServiceProviderSPI provider = runtimes.Get(providerURINonNull);
                if (provider.IsDestroyed) {
                    provider = GetProviderInternal(configuration, providerURINonNull);
                    runtimes.Put(providerURINonNull, provider);
                } else {
                    provider.Configuration = configuration;
                }
                return provider;
            }
    
            // New runtime
            EPServiceProviderSPI runtime = GetProviderInternal(configuration, providerURINonNull);
            runtimes.Put(providerURINonNull, runtime);
            runtime.PostInitialize();
    
            return runtime;
        }
    
        /// <summary>
        /// Returns an existing provider. Returns null if the provider for the given URI has not been initialized
        /// or the provider for the given URI is in destroyed state.
        /// </summary>
        /// <param name="providerURI">- the provider URI. If null provided it assumes "default".</param>
        /// <returns>EPServiceProvider for the given provider URI.</returns>
        public static EPServiceProvider GetExistingProvider(string providerURI) {
            string providerURINonNull = (providerURI == null) ? EPServiceProviderSPI.DEFAULT_ENGINE_URI : providerURI;
            EPServiceProviderSPI provider = runtimes.Get(providerURINonNull);
            if (provider == null || provider.IsDestroyed) {
                return null;
            }
            return provider;
        }
    
        /// <summary>
        /// Returns a list of known provider URIs.
        /// <para>
        /// Returns a the value "default" for the default provider.
        /// </para>
        /// <para>
        /// Returns URIs for all engine instances including destroyed instances.
        /// </para>
        /// </summary>
        /// <returns>array of URI strings</returns>
        public static string[] GetProviderURIs() {
            ISet<string> uriSet = runtimes.KeySet();
            return UriSet.ToArray(new string[uriSet.Count]);
        }
    
        private static EPServiceProviderSPI GetProviderInternal(Configuration configuration, string providerURINonNull) {
            return new EPServiceProviderImpl(configuration, providerURINonNull, runtimes);
        }
    }
} // end of namespace
