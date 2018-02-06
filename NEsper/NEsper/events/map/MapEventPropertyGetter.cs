///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;

namespace com.espertech.esper.events.map
{
    /// <summary>Property getter for Map-underlying events.</summary>
    public interface MapEventPropertyGetter : EventPropertyGetterSPI
    {
        /// <summary>
        ///     Returns a property of an event.
        /// </summary>
        /// <param name="map">to interrogate</param>
        /// <exception cref="PropertyAccessException">for property access errors</exception>
        /// <returns>property value</returns>
        object GetMap(IDictionary<string, object> map);

        /// <summary>
        ///     Exists-function for properties in a map-type event.
        /// </summary>
        /// <param name="map">to interrogate</param>
        /// <returns>indicator</returns>
        bool IsMapExistsProperty(IDictionary<string, object> map);
    }
} // end of namespace