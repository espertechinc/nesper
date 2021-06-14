///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Concurrent;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    /// A registry for health checks.
    /// </summary>
    public class HealthCheckRegistry
    {
        private readonly IDictionary<string, HealthCheck> healthChecks = new ConcurrentDictionary<string, HealthCheck>();

        /// <summary>
        /// Registers an application <seealso cref="HealthCheck" />.
        /// </summary>
        /// <param name="healthCheck">the <seealso cref="HealthCheck" /> instance</param>
        public void Register(HealthCheck healthCheck)
        {
            healthChecks.PutIfAbsent(healthCheck.Name, healthCheck);
        }

        /// <summary>
        /// Unregisters the application <seealso cref="HealthCheck" /> with the given name.
        /// </summary>
        /// <param name="name">the name of the <seealso cref="HealthCheck" /> instance</param>
        public void Unregister(string name)
        {
            healthChecks.Remove(name);
        }

        /// <summary>
        /// Unregisters the given <seealso cref="HealthCheck" />.
        /// </summary>
        /// <param name="healthCheck">a <seealso cref="HealthCheck" /></param>
        public void Unregister(HealthCheck healthCheck)
        {
            Unregister(healthCheck.Name);
        }

        /// <summary>
        /// Runs the registered health checks and returns a map of the results.
        /// </summary>
        /// <returns>a map of the health check results</returns>
        public IDictionary<string, HealthCheck.Result> RunHealthChecks()
        {
            IDictionary<string, HealthCheck.Result> results = new SortedDictionary<string, HealthCheck.Result>();
            foreach (var entry in healthChecks)
            {
                HealthCheck.Result result = entry.Value.Execute();
                results.Put(entry.Key, result);
            }
            return new ReadOnlyDictionary<string, HealthCheck.Result>(results);
        }
    }
} // end of namespace