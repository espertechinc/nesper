///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client
{
    /// <summary>
    /// Get property values from an event instance for a given indexed event property by passing the array index.
    /// Instances that implement this interface are usually bound to a particular 
    /// <see cref="com.espertech.esper.client.EventType" /> and cannot be used to access 
    /// <see cref="EventBean" /> instances of a different type.
    /// </summary>
    public interface EventPropertyGetterIndexed
    {
        /// <summary>
        /// Return the value for the property in the event object specified when the instance was obtained. 
        /// Useful for fast access to event properties. Throws a PropertyAccessException if the getter instance 
        /// doesn't match the EventType it was obtained from, and to indicate other property access problems.
        /// </summary>
        /// <param name="eventBean">is the event to get the value of a property from</param>
        /// <param name="index">the index value</param>
        /// <returns>value of indexed property in event</returns>
        /// <throws>PropertyAccessException to indicate that property access failed</throws>
        Object Get(EventBean eventBean, int index);
    }

    public class ProxyEventPropertyGetterIndexed : EventPropertyGetterIndexed
    {
        public Func<EventBean, int, Object> ProcGet { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyEventPropertyGetterIndexed"/> class.
        /// </summary>
        public ProxyEventPropertyGetterIndexed()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyEventPropertyGetterIndexed"/> class.
        /// </summary>
        /// <param name="procGet">The method get delegate.</param>
        public ProxyEventPropertyGetterIndexed(Func<EventBean, int, object> procGet)
        {
            ProcGet = procGet;
        }

        /// <summary>
        /// Return the value for the property in the event object specified when the instance was obtained.
        /// Useful for fast access to event properties. Throws a PropertyAccessException if the getter instance
        /// doesn't match the EventType it was obtained from, and to indicate other property access problems.
        /// </summary>
        /// <param name="eventBean">is the event to get the value of a property from</param>
        /// <param name="index">the index value</param>
        /// <returns>value of indexed property in event</returns>
        /// <throws>PropertyAccessException to indicate that property access failed</throws>
        public Object Get(EventBean eventBean, int index)
        {
            return ProcGet.Invoke(eventBean, index);
        }
    }

}
