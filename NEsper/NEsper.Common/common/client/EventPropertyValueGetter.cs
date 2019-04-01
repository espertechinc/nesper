///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client
{
	/// <summary>
	/// Get property values from an event instance for a given event property.
	/// Instances that implement this interface are usually bound to a particular <seealso cref="EventType" /> and cannot
	/// be used to access <seealso cref="EventBean" /> instances of a different type.
	/// </summary>
	public interface EventPropertyValueGetter {

	    /// <summary>
	    /// Return the value for the property in the event object specified when the instance was obtained.
	    /// Useful for fast access to event properties. Throws a PropertyAccessException if the getter instance
	    /// doesn't match the EventType it was obtained from, and to indicate other property access problems.
	    /// </summary>
	    /// <param name="eventBean">is the event to get the value of a property from</param>
	    /// <returns>value of property in event</returns>
	    /// <throws>PropertyAccessException to indicate that property access failed</throws>
	    object Get(EventBean eventBean) ;
	}
} // end of namespace
