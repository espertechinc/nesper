///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.plugin
{
    /// <summary>
    /// Context for use in <seealso cref="PlugInEventRepresentation" /> to provide information 
    /// to help decide whether an event representation can handle the requested event type.
    /// </summary>
    public class PlugInEventTypeHandlerContext
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventTypeResolutionURI">the URI specified for resolving the event type, may be a child URI of the event representation URI and may carry additional parameters</param>
        /// <param name="typeInitializer">optional configuration for the type, or null if none supplied</param>
        /// <param name="eventTypeName">the name of the event</param>
        /// <param name="eventTypeId">The event type id.</param>
        public PlugInEventTypeHandlerContext(Uri eventTypeResolutionURI, Object typeInitializer, String eventTypeName, int eventTypeId)
        {
            EventTypeResolutionURI = eventTypeResolutionURI;
            TypeInitializer = typeInitializer;
            EventTypeName = eventTypeName;
            EventTypeId = eventTypeId;
        }

        /// <summary>
        /// Gets or sets the event type id.
        /// </summary>
        /// <value>The event type id.</value>
        public int EventTypeId { get; private set; }

        /// <summary>Returns the URI specified for resolving the event type, may be a child URI of the event representation URI and may carry additional parameters </summary>
        /// <value>URI</value>
        public Uri EventTypeResolutionURI { get; private set; }

        /// <summary>Returns optional configuration for the type, or null if none supplied. An String XML document if the configuration was read from an XML file. </summary>
        /// <value>configuration, or null if none supplied</value>
        public object TypeInitializer { get; private set; }

        /// <summary>Returns the name assigned to the event type. </summary>
        /// <value>name</value>
        public string EventTypeName { get; private set; }
    }
}
