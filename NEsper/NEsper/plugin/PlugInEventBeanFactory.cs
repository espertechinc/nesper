///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;

namespace com.espertech.esper.plugin
{
    /// <summary>
    /// Factory for creating <see cref="EventBean"/> event object wrapper for a plug-in event representation.
    /// <para/>
    /// Implementations typically reflect on the event object to be processed and decides on the proper
    /// <see cref="com.espertech.esper.client.EventType"/> to assign. If the implementation finds that it cannot
    /// handle the event object, it should return null. Returning null gives another instance of this
    /// class as specified by the list of URI to handle the event object.
    /// <para>
    /// Returns an event wrapper for the event object specific to the plug-in event representation or
    /// using one of the built-in types, or null if the event object is unknown and cannot be handled.
    /// </para>
    /// </summary>
    /// <param name="theEvent">is the event object to reflect upon and wrap</param>
    /// <param name="resolutionURI">is the URI used originally for obtaining the event sender</param>
    /// <returns>
    /// wrapped event object, or null if the event is of unknown type or content
    /// </returns>
    public delegate EventBean PlugInEventBeanFactory(Object theEvent, Uri resolutionURI);
}
