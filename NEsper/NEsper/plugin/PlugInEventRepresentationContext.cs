///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.events;

namespace com.espertech.esper.plugin
{
    /// <summary>Context for use in <see cref="PlugInEventRepresentation"/> to initialize an implementation. </summary>
    public class PlugInEventRepresentationContext
    {
        private readonly EventAdapterService eventAdapterService;
        private readonly Uri eventRepresentationRootURI;
        private readonly Object representationInitializer;

        /// <summary>Ctor. </summary>
        /// <param name="eventAdapterService">for creating further event types or wrapping event objects</param>
        /// <param name="eventRepresentationRootURI">URI of the event representation</param>
        /// <param name="representationInitializer">initializer objects</param>
        public PlugInEventRepresentationContext(EventAdapterService eventAdapterService,
                                                Uri eventRepresentationRootURI,
                                                Object representationInitializer)
        {
            this.eventAdapterService = eventAdapterService;
            this.eventRepresentationRootURI = eventRepresentationRootURI;
            this.representationInitializer = representationInitializer;
        }

        /// <summary>Ctor. </summary>
        /// <returns>URI of event representation instance</returns>
        public Uri EventRepresentationRootURI
        {
            get { return eventRepresentationRootURI; }
        }

        /// <summary>Returns optional configuration for the event representation, or null if none supplied. An String XML document if the configuration was read from an XML file. </summary>
        /// <returns>configuration, or null if none supplied</returns>
        public object RepresentationInitializer
        {
            get { return representationInitializer; }
        }

        /// <summary>Returns the service for for creating further event types or wrapping event objects. </summary>
        /// <returns>event adapter service</returns>
        public EventAdapterService EventAdapterService
        {
            get { return eventAdapterService; }
        }
    }
}
