///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Plug-in event representation that can dynamically create event types and event
    /// instances based on information available elsewhere.
    /// <para>
    /// A plug-in event representation can be useful when your application has existing
    /// types that carry event metadata and event property values and your application
    /// does not want to (or cannot) extract or transform such event metadata and event
    /// data into one of the built-in event representations (objects, DataMap or XML DOM).
    /// </para>
    /// <para>
    /// Further use of a plug-in event representation is to provide a faster or short-cut
    /// access path to event data. For example, the access to XML event data through a StAX
    /// Streaming API for XML (SAX) is known to be very efficient.
    /// </para>
    /// <para>
    /// Further, a plug-in event representation can provide network lookup and general
    /// abstraction of event typing and event sourcing.
    /// </para>
    /// <para>
    /// Before use, an implementation of this interface must be registered via configuration.
    /// Upon engine initialization, the engine invokes the <see cref="Init"/> method passing
    /// configuration information.
    /// </para>
    /// <para>
    /// When a plug-in event type name is registered via configuration (runtime or configuration
    /// time), the engine first asks the implementation whether the type is accepted via
    /// <see cref="AcceptsType"/>. If accepted, the engine follows with a call to <see cref="GetTypeHandler"/>
    /// for creating and handling the type.
    /// </para>
    /// <para>
    /// An implementation can participate in dynamic resolution of new (unseen) event type
    /// names if the application configures the URI of the event representation, or a child URI
    /// (parameters possible) via <see cref="ConfigurationOperations.PlugInEventTypeResolutionURIs"/>.
    /// </para>
    /// <para>
    /// Last, see <see cref="EPRuntime.GetEventSender(Uri[])"/>. An
    /// event sender allows dynamic reflection on an incoming event object. At the time such
    /// an event sender is obtained and a matching URI specified, the
    /// <see cref="AcceptsEventBeanResolution"/> method indicates that the event representation can
    /// or cannot inspect events, and the <see cref="PlugInEventBeanFactory"/> returned is used by
    /// the event sender to wrap event objects for processing. 
    /// </para>
    /// </summary>
    public interface PlugInEventRepresentation
    {
        /// <summary>
        /// Initializes the event representation.
        /// </summary>
        /// <param name="eventRepresentationContext">URI and optional configuration information</param>
        void Init(PlugInEventRepresentationContext eventRepresentationContext);

        /// <summary>
        /// Returns true to indicate that the event representation can handle the requested event type.
        /// <para/>
        /// Called when a new plug-in event type and name is registered and the its resolution URI
        /// matches or is a child URI of the event representation URI.
        /// <para/>
        /// Also called when a new EPL statement is created with an unseen event type name and the
        /// URIs for resolution have been configured.
        /// </summary>
        /// <param name="acceptTypeContext">provides the URI specified for resolving the type, and configuration INFO.</param>
        /// <returns>
        /// true to accept the type, false such that another event representation may handle the type request
        /// </returns>
        bool AcceptsType(PlugInEventTypeHandlerContext acceptTypeContext);

        /// <summary>
        /// Returns the event type handler that provides the event type and, upon request, event sender,
        /// for this type.
        /// </summary>
        /// <param name="eventTypeContext">provides the URI specified for resolving the type, and configuration INFO.</param>
        /// <returns>provides event type and event sender</returns>
        PlugInEventTypeHandler GetTypeHandler(PlugInEventTypeHandlerContext eventTypeContext);

        /// <summary>
        /// For use with <see cref="EPRuntime.GetEventSender(Uri[])"/>, returns 
        /// true if the event representation intends to provide event wrappers for event objects passed in.
        /// </summary>
        /// <param name="acceptBeanContext">provides the URI specified for resolving the event object reflection</param>
        /// <returns>
        /// true to accept the requested URI, false such that another event representation may handle the request
        /// </returns>
        bool AcceptsEventBeanResolution(PlugInEventBeanReflectorContext acceptBeanContext);

        /// <summary>
        /// For use with <see cref="EPRuntime.GetEventSender(Uri[])"/>, returns
        /// the factory that can inspect event objects and provide an event <see cref="EventBean"/> wrapper.
        /// </summary>
        /// <param name="eventBeanContext">provides the URI specified for resolving the event object reflection</param>
        /// <returns>
        /// true to accept the requested URI, false such that another event representation may handle the request
        /// </returns>
        PlugInEventBeanFactory GetEventBeanFactory(PlugInEventBeanReflectorContext eventBeanContext);
    }
}
