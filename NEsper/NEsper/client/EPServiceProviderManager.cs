///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;

[assembly:InternalsVisibleTo("NEsper.Tests")]

namespace com.espertech.esper.client
{
    /// <summary>
    /// Factory for instances of <seealso cref="EPServiceProvider" />.
    /// </summary>
    public sealed class EPServiceProviderManager
    {
        private static readonly ILockable LockObj;
        private static readonly IDictionary<string, EPServiceProviderSPI> Runtimes;

        static EPServiceProviderManager()
        {
            LockObj = new MonitorLock(60000);
            Runtimes = new ConcurrentDictionary<string, EPServiceProviderSPI>();
        }

        /// <summary>
        /// Returns the default EPServiceProvider. The URI value for the service returned is "default".
        /// </summary>
        /// <returns></returns>
        public static EPServiceProvider GetDefaultProvider()
        {
            return GetDefaultProvider(ContainerExtensions.CreateDefaultContainer());
        }

        /// <summary>
        /// Returns the default EPServiceProvider. The URI value for the service returned is "default".
        /// </summary>
        /// <returns>default instance of the service.</returns>
        public static EPServiceProvider GetDefaultProvider(IContainer container)
        {
            return GetProvider(
                container,
                EPServiceProviderConstants.DEFAULT_ENGINE_URI,
                new Configuration(container));
        }

        /// <summary>
        /// Returns the default EPServiceProvider. The URI value for the service returned is "default".
        /// </summary>
        /// <param name="container">The service provider.</param>
        /// <param name="configuration">is the configuration for the service</param>
        /// <returns>
        /// default instance of the service.
        /// </returns>
        /// <exception cref="ConfigurationException">to indicate a configuration problem</exception>
        public static EPServiceProvider GetDefaultProvider(
            IContainer container,
            Configuration configuration)
        {
            return GetProvider(
                container,
                EPServiceProviderConstants.DEFAULT_ENGINE_URI, 
                configuration);
        }

        public static EPServiceProvider GetDefaultProvider(
            Configuration configuration)
        {
            var container = configuration.Container;
            if (container == null) {
                container = ContainerExtensions.CreateDefaultContainer();
            }

            return GetProvider(
                container,
                EPServiceProviderConstants.DEFAULT_ENGINE_URI,
                configuration);
        }

        /// <summary>
        /// Returns an EPServiceProvider for a given provider URI.
        /// <para>
        /// Use the URI of "default" or null to return the default service provider.
        /// </para>
        /// </summary>
        /// <param name="container">The service provider.</param>
        /// <param name="providerURI">- the provider URI</param>
        /// <returns>
        /// EPServiceProvider for the given provider URI.
        /// </returns>
        public static EPServiceProvider GetProvider(
            IContainer container, 
            string providerURI)
        {
            return GetProvider(container, providerURI, new Configuration(container));
        }

        public static EPServiceProvider GetProvider(
            string providerURI)
        {
            return GetProvider(
                ContainerExtensions.CreateDefaultContainer(),
                providerURI);
        }

        public static EPServiceProvider GetProvider(
            string providerURI,
            Configuration configuration)
        {
            var container = configuration.Container;
            if (container == null) {
                container = ContainerExtensions.CreateDefaultContainer();
            }

            return GetProvider(
                container,
                providerURI,
                configuration);
        }

        /// <summary>
        /// Returns an EPServiceProvider for a given provider URI.
        /// Use the URI of "default" or null to return the default service provider.
        /// </summary>
        /// <param name="container">The service provider.</param>
        /// <param name="providerURI">- the provider URI. If null provided it assumes "default".</param>
        /// <param name="configuration">is the configuration for the service</param>
        /// <returns>
        /// EPServiceProvider for the given provider URI.
        /// </returns>
        /// <exception cref="ConfigurationException">to indicate a configuration problem</exception>
        public static EPServiceProvider GetProvider(
            IContainer container, 
            string providerURI, 
            Configuration configuration)
        {
            using (LockObj.Acquire())
            {
                var providerURINonNull = providerURI ?? EPServiceProviderConstants.DEFAULT_ENGINE_URI;

                if (Runtimes.ContainsKey(providerURINonNull))
                {
                    var provider = Runtimes.Get(providerURINonNull);
                    if (provider.IsDestroyed)
                    {
                        provider = GetProviderInternal(container, configuration, providerURINonNull);
                        Runtimes.Put(providerURINonNull, provider);
                    }
                    else
                    {
                        provider.SetConfiguration(configuration);
                    }
                    return provider;
                }

                // New runtime
                var runtime = GetProviderInternal(container, configuration, providerURINonNull);
                Runtimes.Put(providerURINonNull, runtime);
                runtime.PostInitialize();

                return runtime;
            }
        }

        /// <summary>
        /// Returns an existing provider. Returns null if the provider for the given URI has not been initialized
        /// or the provider for the given URI is in destroyed state.
        /// </summary>
        /// <param name="providerURI">- the provider URI. If null provided it assumes "default".</param>
        /// <returns>EPServiceProvider for the given provider URI.</returns>
        public static EPServiceProvider GetExistingProvider(string providerURI)
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
        /// Returns a list of known provider URIs.
        /// <para>
        /// Returns a the value "default" for the default provider.
        /// </para>
        /// <para>
        /// Returns URIs for all engine instances including destroyed instances.
        /// </para>
        /// </summary>
        /// <value>array of URI strings</value>
        public static string[] ProviderURIs
        {
            get { return Runtimes.Keys.ToArray(); }
        }

        private static EPServiceProviderSPI GetProviderInternal(
            IContainer container,
            Configuration configuration, 
            string providerURINonNull)
        {
            return new EPServiceProviderImpl(
                container, configuration, providerURINonNull, Runtimes);
        }

        /// <summary>
        /// Clears references to the provider.
        /// </summary>
        /// <param name="providerURI"></param>

        public static void PurgeProvider(string providerURI)
        {
            using (LockObj.Acquire())
            {
                if (string.IsNullOrEmpty(providerURI))
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
} // end of namespace
