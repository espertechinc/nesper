///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats
{
    /// <summary>
    /// A statistically representative sample of a data stream.
    /// </summary>
    public interface Sample
    {
        /// <summary>
        /// Clears all recorded values.
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns the number of values recorded.
        /// </summary>
        /// <value>the number of values recorded</value>
        int Count { get; }

        /// <summary>
        /// Adds a new recorded value to the sample.
        /// </summary>
        /// <param name="value">a new recorded value</param>
        void Update(long value);

        /// <summary>
        /// Returns a snapshot of the sample's values.
        /// </summary>
        /// <returns>a snapshot of the sample's values</returns>
        Snapshot Snapshot { get; }
    }
} // end of namespace