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
    /// Interface for <seealso cref="Metric" /> instances that can be stopped.
    /// </summary>
    public interface Stoppable
    {
        /// <summary>
        /// Stop the instance.
        /// </summary>
        void Stop();
    }
} // end of namespace