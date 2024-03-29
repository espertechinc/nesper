///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client
{
    /// <summary>
    /// Get property values from an event instance for a given mapped event property by passing 
    /// the map string key. Instances that implement this interface are usually bound to a particular 
    /// <see cref="EventType" /> and cannot be used to access <see cref="EventBean" />
    /// instances of a different type.
    /// </summary>
    public interface EventPropertyGetterMapped
    {
        /// <summary>
        /// Return the value for the property in the event object specified when the instance was obtained.
        /// Useful for fast access to event properties. Throws a PropertyAccessException if the getter instance
        /// doesn't match the EventType it was obtained from, and to indicate other property access problems.
        /// </summary>
        /// <param name="eventBean">is the event to get the value of a property from</param>
        /// <param name="mapKey">the map key value</param>
        /// <returns>value of property in event</returns>
        /// <throws>com.espertech.esper.client.PropertyAccessException to indicate that property access failed</throws>
        object Get(
            EventBean eventBean,
            string mapKey);
    }

    public class ProxyEventPropertyGetterMapped : EventPropertyGetterMapped
    {
        public Func<EventBean, string, object> ProcGet { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyEventPropertyGetterMapped"/> class.
        /// </summary>
        public ProxyEventPropertyGetterMapped()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyEventPropertyGetter"/> class.
        /// </summary>
        /// <param name="procGet">The method get delegate.</param>
        public ProxyEventPropertyGetterMapped(Func<EventBean, string, object> procGet)
        {
            ProcGet = procGet;
        }


        /// <summary>
        /// Return the value for the property in the event object specified when the instance was obtained.
        /// Useful for fast access to event properties. Throws a PropertyAccessException if the getter instance
        /// doesn't match the EventType it was obtained from, and to indicate other property access problems.
        /// </summary>
        /// <param name="eventBean">is the event to get the value of a property from</param>
        /// <param name="mapKey">the map key value</param>
        /// <returns>value of property in event</returns>
        /// <throws>com.espertech.esper.client.PropertyAccessException to indicate that property access failed</throws>
        public object Get(
            EventBean eventBean,
            string mapKey)
        {
            return ProcGet.Invoke(eventBean, mapKey);
        }
    }
}