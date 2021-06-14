///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    /// Service for managing event types.
    /// </summary>
    public interface EPEventTypeService
    {
        /// <summary>
        /// Returns the event type for a preconfigured event type.
        /// </summary>
        /// <param name="eventTypeName">event type name of a preconfigured event type</param>
        /// <returns>event type or null if not found</returns>
        EventType GetEventTypePreconfigured(string eventTypeName);

        /// <summary>
        /// Returns the event type as defined by a given deployment.
        /// </summary>
        /// <param name="deploymentId">deployment id of the deployment</param>
        /// <param name="eventTypeName">event type name of a preconfigured event type</param>
        /// <returns>event type or null if not found</returns>
        EventType GetEventType(string deploymentId, string eventTypeName);
        
        /// <summary>
        /// Returns the event type that has bus-visibility or null in case that the event type is not found
        /// </summary>
        /// <param name="eventTypeName">the event type name</param>
        /// <returns>event type or null</returns>
        EventType GetBusEventType(string eventTypeName);
    }
} // end of namespace