///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics
{
    /// <summary>
    /// A manager class for health checks.
    /// </summary>
    public class HealthChecks
    {
        private static readonly HealthCheckRegistry DEFAULT_REGISTRY = new HealthCheckRegistry();

        private HealthChecks()
        {
            /* unused */
        }

        /// <summary>
        /// Registers an application <seealso cref="HealthCheck" /> with a given name.
        /// </summary>
        /// <param name="healthCheck">the <seealso cref="HealthCheck" /> instance</param>
        public static void Register(HealthCheck healthCheck)
        {
            DEFAULT_REGISTRY.Register(healthCheck);
        }

        /// <summary>
        /// Runs the registered health checks and returns a map of the results.
        /// </summary>
        /// <returns>a map of the health check results</returns>
        public static IDictionary<string, HealthCheck.Result> RunHealthChecks()
        {
            return DEFAULT_REGISTRY.RunHealthChecks();
        }

        /// <summary>
        /// Returns the (static) default registry.
        /// </summary>
        /// <returns>the registry</returns>
        public static HealthCheckRegistry DefaultRegistry()
        {
            return DEFAULT_REGISTRY;
        }
    }
} // end of namespace