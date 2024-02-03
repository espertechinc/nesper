///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.metric
{
    /// <summary>
    /// Base metric event.
    /// </summary>
    public abstract class MetricEvent
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="runtimeUri">the engine URI</param>
        protected MetricEvent(string runtimeUri)
        {
            RuntimeURI = runtimeUri;
        }

        /// <summary>
        /// Returns the runtime URI.
        /// </summary>
        /// <value>The runtime URI.</value>
        /// <returns>uri</returns>
        public string RuntimeURI { get; private set; }
    }
}