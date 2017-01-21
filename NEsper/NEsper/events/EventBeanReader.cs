///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Interface for reading all event properties of an event.
    /// </summary>
    public interface EventBeanReader
    {
        /// <summary>
        /// Returns all event properties in the exact order they appear as properties.
        /// </summary>
        /// <param name="theEvent">to read</param>
        /// <returns>
        /// property values
        /// </returns>
        Object[] Read(EventBean theEvent);
    }
}
