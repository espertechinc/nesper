///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    /// An object which can produce statistical summaries.
    /// </summary>
    public interface Summarizable
    {
        /// <summary>
        /// Returns the largest recorded value.
        /// </summary>
        /// <value>the largest recorded value</value>
        double Max { get; }

        /// <summary>
        /// Returns the smallest recorded value.
        /// </summary>
        /// <value>the smallest recorded value</value>
        double Min { get; }

        /// <summary>
        /// Returns the arithmetic mean of all recorded values.
        /// </summary>
        /// <value>the arithmetic mean of all recorded values</value>
        double Mean { get; }

        /// <summary>
        /// Returns the standard deviation of all recorded values.
        /// </summary>
        /// <value>the standard deviation of all recorded values</value>
        double StdDev { get; }

        /// <summary>
        /// Returns the sum of all recorded values.
        /// </summary>
        /// <value>the sum of all recorded values</value>
        double Sum { get; }
    }
} // end of namespace