///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Interface for composite events in which a property is itself an event.
    /// <para>
    /// For use with patterns in which pattern tags are properties in a result event and property values
    /// are the event itself that is matching in a pattern.
    /// </para>
    /// </summary>
	public interface TaggedCompositeEventBean
	{
	    /// <summary>Returns the event for the tag.</summary>
	    /// <param name="property">is the tag name</param>
	    /// <returns>event</returns>
	    EventBean GetEventBean(String property);
	}
} // End of namespace
