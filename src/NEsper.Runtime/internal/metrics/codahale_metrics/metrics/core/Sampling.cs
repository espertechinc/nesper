///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    /// An object which samples values.
    /// </summary>
    public interface Sampling
    {
        /// <summary>
        /// Returns a snapshot of the values.
        /// </summary>
        /// <returns>a snapshot of the values</returns>
        Snapshot Snapshot { get; }
    }
} // end of namespace