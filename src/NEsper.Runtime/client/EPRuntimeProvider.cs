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
using System.Linq;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.@internal.kernel.service;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    /// Factory for instances of <seealso cref="EPRuntime" />.
    /// </summary>
    public sealed class EPRuntimeProvider
    {
        /// <summary>
        /// A default provider to provide backwards compatibility (static methods).
        /// </summary>
        private static readonly EPRuntimeProvider DefaultProvider = new EPRuntimeProvider();

        /// <summary>
        /// For the default runtime instance the URI value is "default".
        /// </summary>
        public const string DEFAULT_RUNTIME_URI = "default";

        /// <summary>
        /// Set of runtimes associated with this provider.
        /// </summary>
        private readonly IDictionary<string, EPRuntimeSPI> _runtimes = new ConcurrentDictionary<string, EPRuntimeSPI>();
        
#region BACKWARD_COMPATIBILITY
        /// <summary>
        /// Returns the runtime for the default URI. The URI value for the runtime returned is "default".
        /// </summary>
        /// <returns>default runtime</returns>
        public static EPRuntime GetDefaultRuntime()
        {
            return DefaultProvider.GetRuntimeInstance(DEFAULT_RUNTIME_URI, new Configuration());
        }
        
        /// <summary>
        /// Returns the default runtime. The URI value for the runtime returned is "default".
        /// </summary>4
        /// <param name="configuration">is the configuration for the runtime</param>
        /// <returns>default instance of the runtime.</returns>
        /// <throws>ConfigurationException to indicate a configuration problem</throws>
        public static EPRuntime GetDefaultRuntime(Configuration configuration)
        {
            return DefaultProvider.GetRuntimeInstance(DEFAULT_RUNTIME_URI, configuration);
        }
        
        /// <summary>
        /// Backward compatible. Returns a runtime for a given runtime URI.
        /// <para>Use the URI of "default" or null to return the default runtime.</para>
        /// </summary>
        /// <param name="uri">the URI</param>
        /// <returns>runtime for the given URI.</returns>
        public static EPRuntime GetRuntime(string uri)
        {
            return DefaultProvider.GetRuntimeInstance(uri);
        }

        /// <summary>
        /// Backwards compatible. Returns a runtime for a given URI.
        /// Use the URI of "default" or null to return the default runtime.
        /// </summary>
        /// <param name="uri">the runtime URI. If null provided it assumes "default".</param>
        /// <param name="configuration">is the configuration for the runtime</param>
        /// <returns>Runtime for the given URI.</returns>
        /// <throws>ConfigurationException to indicate a configuration problem</throws>
        public static EPRuntime GetRuntime(
            string uri,
            Configuration configuration)
        {
            return DefaultProvider.GetRuntimeInstance(uri, configuration);
        }
#endregion

        /// <summary>
        /// Returns the runtime for the default URI. The URI value for the runtime returned is "default".
        /// </summary>
        /// <returns>default runtime</returns>
        public EPRuntime GetDefaultRuntimeInstance()
        {
            return GetRuntimeInstance(DEFAULT_RUNTIME_URI, new Configuration());
        }

        /// <summary>
        /// Returns the default runtime. The URI value for the runtime returned is "default".
        /// </summary>4
        /// <param name="configuration">is the configuration for the runtime</param>
        /// <returns>default instance of the runtime.</returns>
        /// <throws>ConfigurationException to indicate a configuration problem</throws>
        public EPRuntime GetDefaultRuntimeInstance(Configuration configuration)
        {
            return GetRuntimeInstance(DEFAULT_RUNTIME_URI, configuration);
        }

        /// <summary>
        /// Returns a runtime for a given runtime URI.
        /// <para>Use the URI of "default" or null to return the default runtime.</para>
        /// </summary>
        /// <param name="uri">the URI</param>
        /// <returns>runtime for the given URI.</returns>
        public EPRuntime GetRuntimeInstance(string uri)
        {
            return GetRuntimeInstance(uri, new Configuration());
        }
        /// <summary>
        /// Returns a runtime for a given URI.
        /// Use the URI of "default" or null to return the default runtime.
        /// </summary>
        /// <param name="uri">the runtime URI. If null provided it assumes "default".</param>
        /// <param name="configuration">is the configuration for the runtime</param>
        /// <returns>Runtime for the given URI.</returns>
        /// <throws>ConfigurationException to indicate a configuration problem</throws>
        public EPRuntime GetRuntimeInstance(string uri, Configuration configuration)
        {
            var runtimeURINonNull = uri ?? DEFAULT_RUNTIME_URI;

            if (_runtimes.ContainsKey(runtimeURINonNull))
            {
                var runtimeSpi = _runtimes.Get(runtimeURINonNull);
                if (runtimeSpi.IsDestroyed)
                {
                    runtimeSpi = GetRuntimeInternal(configuration, runtimeURINonNull);
                    //runtimes.Put(runtimeURINonNull, runtimeSpi);
                }
                else
                {
                    runtimeSpi.SetConfiguration(configuration);
                }
                return runtimeSpi;
            }

            // New runtime
            var runtime = GetRuntimeInternal(configuration, runtimeURINonNull);
            //runtimes.Put(runtimeURINonNull, runtime);
            runtime.PostInitialize();

            return runtime;
        }

        /// <summary>
        /// Returns an existing runtime. Returns null if the runtime for the given URI has not been initialized
        /// or the runtime for the given URI is in destroyed state.
        /// </summary>
        /// <param name="uri">the URI. If null provided it assumes "default".</param>
        /// <returns>Runtime for the given URI.</returns>
        public EPRuntime GetExistingRuntime(string uri)
        {
            var runtimeURINonNull = uri ?? DEFAULT_RUNTIME_URI;
            var runtime = _runtimes.Get(runtimeURINonNull);
            if (runtime == null || runtime.IsDestroyed)
            {
                return null;
            }
            return runtime;
        }

        /// <summary>
        /// Returns a list of known URIs.
        /// <para />Returns a the value "default" for the default runtime.
        /// </summary>
        /// <value>array of URI strings</value>
        public string[] RuntimeURIs => _runtimes.Keys.ToArray();

        /// <summary>
        /// Returns an indicator whether a runtime for the given URI is allocated (true) or is not allocated (false)
        /// </summary>
        /// <param name="uri">runtime uri</param>
        /// <returns>indicator</returns>
        public bool HasRuntime(string uri)
        {
            return _runtimes.ContainsKey(uri);
        }

        private EPRuntimeSPI GetRuntimeInternal(Configuration configuration, string runtimeURINonNull)
        {
            var runtime = new EPRuntimeImpl(configuration, runtimeURINonNull, _runtimes);
            // Add an event handler for destruction
            runtime.Destroyed += (sender, args) => {
                _runtimes.Remove(runtimeURINonNull);
            };
            // Add to the runtimes
            _runtimes.Put(runtimeURINonNull, runtime);
            // Return the runtime
            return runtime;
        }
    }
} // end of namespace