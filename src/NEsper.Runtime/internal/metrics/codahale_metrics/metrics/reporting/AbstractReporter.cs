///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.reporting
{
    /// <summary>
    /// The base class for all metric reporters.
    /// </summary>
    public abstract class AbstractReporter
    {
        /// <summary>
        /// Creates a new <seealso cref="AbstractReporter" /> instance.
        /// </summary>
        /// <param name="registry">the <seealso cref="MetricsRegistry" /> containing the metrics this reporter will report
        /// </param>
        protected AbstractReporter(MetricsRegistry registry)
        {
            MetricsRegistry = registry;
        }

        /// <summary>
        /// Stops the reporter and closes any internal resources.
        /// </summary>
        public virtual void Shutdown()
        {
            // nothing to do here
        }

        /// <summary>
        /// Returns the reporter's <seealso cref="MetricsRegistry" />.
        /// </summary>
        /// <value>the reporter's <seealso cref="MetricsRegistry" />
        /// </value>
        protected MetricsRegistry MetricsRegistry { get; }
    }
} // end of namespace