///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.metric
{
    /// <summary>
    /// Base metric event.
    /// </summary>
    public abstract class MetricEvent
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="engineURI">the engine URI</param>
        protected MetricEvent(String engineURI)
        {
            EngineURI = engineURI;
        }

        /// <summary>
        /// Returns the engine URI.
        /// </summary>
        /// <value>The engine URI.</value>
        /// <returns>uri</returns>
        public string EngineURI { get; private set; }
    }
}
