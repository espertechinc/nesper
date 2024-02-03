///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    /// Modifier that dictates whether an event type allows or does not allow sending events in using one of the send-event
    /// methods.
    /// </summary>
    public enum EventTypeBusModifier
    {
        /// <summary>
        /// Allow sending in events of this type using the send-event API on event service.
        /// </summary>
        BUS,

        /// <summary>
        /// Disallow sending in events of this type using the send-event API on event service.
        /// </summary>
        NONBUS
    }
} // end of namespace