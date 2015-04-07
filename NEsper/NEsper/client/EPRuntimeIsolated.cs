///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;
using System.Xml.Linq;
using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esper.client
{
    /// <summary>
    /// Runtime interface for the isolated service provider, for controlling event
    /// visibility and scheduling for the statements contained within the isolated service.
    /// </summary>
    public interface EPRuntimeIsolated
    {
        /// <summary>
        /// Send an event represented by a plain object to the event stream processing
        /// runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// UpdateListener code, to avoid the possibility of a stack overflow due to nested calls to
        /// sendEvent.
        /// </summary>
        /// <param name="object">is the event to sent to the runtime</param>
        /// <throws>com.espertech.esper.client.EPException is thrown when the processing of the event lead to an error</throws>
        void SendEvent(Object @object);

        /// <summary>
        /// Send a map containing event property values to the event stream processing
        /// runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// UpdateListener code. to avoid the possibility of a stack overflow due to nested calls to
        /// sendEvent.
        /// </summary>
        /// <param name="map">map that contains event property values. Keys are expected to be of type String while value scan be of any type. Keys and values should match those declared via Configuration for the given eventTypeName.</param>
        /// <param name="eventTypeName">the name for the Map event type that was previously configured</param>
        /// <throws>com.espertech.esper.client.EPException - when the processing of the event leads to an error</throws>
        void SendEvent(DataMap map, String eventTypeName);

        /// <summary>Send an object array containing event property values to the event stream processing runtime. &lt;p&gt; Use the route method for sending events into the runtime from within UpdateListener code. to avoid the possibility of a stack overflow due to nested calls to sendEvent.  </summary>
        /// <param name="objectarray">array that contains event property values. Your application must ensure that property valuesmatch the exact same order that the property names and types have been declared, and that the array length matches the number of properties declared. </param>
        /// <param name="objectArrayEventTypeName">the name for the Object-array event type that was previously configured</param>
        /// <throws>EPException - when the processing of the event leads to an error</throws>
        void SendEvent(Object[] objectarray, String objectArrayEventTypeName);

        /// <summary>
        /// Send an event represented by a LINQ element to the event stream processing runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// event handler code. to avoid the possibility of a stack overflow due to nested calls to
        /// SendEvent.
        /// </summary>
        /// <param name="element">The element.</param>
        void SendEvent(XElement element);

        /// <summary>
        /// Send an event represented by a DOM node to the event stream processing runtime.
        /// <para/>
        /// Use the route method for sending events into the runtime from within
        /// UpdateListener code. to avoid the possibility of a stack overflow due to nested calls to
        /// sendEvent.
        /// </summary>
        /// <param name="node">is the DOM node as an event</param>
        /// <throws>com.espertech.esper.client.EPException is thrown when the processing of the event lead to an error</throws>
        void SendEvent(XmlNode node);
    
        /// <summary>
        /// Returns current engine time.
        /// <para/>
        /// If time is provided externally via timer events, the function returns current
        /// time as externally provided.
        /// </summary>
        /// <returns>
        /// current engine time
        /// </returns>
        long CurrentTime { get; }

        /// <summary>
        /// Returns the time at which the next schedule execution is expected, returns null if no schedule execution is
        /// outstanding.
        /// </summary>
        long? NextScheduledTime { get; }

        /// <summary>
        /// Returns a facility to process event objects that are of a known type.
        /// <para>
        /// Given an event type name this method returns a sender that allows to send in event objects of that type. The 
        /// event objects send in via the event sender are expected to match the event type, thus the event sender does 
        /// not inspect the event object other then perform basic checking.
        /// </para>
        /// <para>
        /// For events backed by a class, the sender ensures that the object send in matches in class, or implements or 
        /// extends the class underlying the event type for the given event type name.
        /// </para>
        /// <para>
        /// For events backed by a Object[] (Object-array events), the sender does not perform any checking other then checking 
        /// that the event object indeed is an array of object.
        /// </para>
        /// <para>
        /// For events backed by a DataMap (Map events), the sender does not perform any checking other then checking that the 
        /// event object indeed implements Map. 
        /// </para>
        /// <para>
        /// For events backed by a XmlNode (XML DOM events), the sender checks that the root element name indeed does match the 
        /// root element name for the event type name. 
        /// </para>
        /// </summary>
        /// <param name="eventTypeName">is the name of the event type</param>
        /// <returns>sender for fast-access processing of event objects of known type (and content)</returns>
        /// <throws>EventTypeException thrown to indicate that the name does not exist</throws>
        EventSender GetEventSender(String eventTypeName) ;

        /// <summary>
        /// For use with plug-in event representations, returns a facility to process event objects that are of one of a number 
        /// of types that one or more of the registered plug-in event representation extensions can reflect upon and provide an event for. 
        /// </summary>
        /// <param name="uris">
        /// is the URIs that specify which plug-in event representations may process an event object.
        /// <para>
        /// URIs do not need to match event representation URIs exactly, a child (hierarchical) match is enough for an event representation to participate.
        /// </para>
        /// <para>
        /// The order of URIs is relevant as each event representation's factory is asked in turn to process the event, until the first factory processes the event.
        /// </para>
        /// </param>
        /// <returns>sender for processing of event objects of one of the plug-in event representations</returns>
        /// <throws>EventTypeException thrown to indicate that the URI list was invalid</throws>
        EventSender GetEventSender(Uri[] uris) ;
    }
}
