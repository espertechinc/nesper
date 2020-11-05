///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.render
{
    /// <summary>
    /// Provider for rendering services of <seealso cref="EventBean" /> events.
    /// </summary>
    public interface EPEventTypeRuntime
    {
        /// <summary>
        /// Returns a render for the JSON format, valid only for the given event type and its subtypes.
        /// </summary>
        /// <param name="eventType">to return renderer for</param>
        /// <param name="options">rendering options</param>
        /// <returns>JSON format renderer</returns>
        JSONEventRenderer GetJSONRenderer(
            EventType eventType,
            JSONRenderingOptions options);

        /// <summary>
        /// Returns a render for the JSON format, valid only for the given event type and its subtypes.
        /// </summary>
        /// <param name="eventType">to return renderer for</param>
        /// <returns>JSON format renderer</returns>
        JSONEventRenderer GetJSONRenderer(EventType eventType);

        /// <summary>
        /// Quick-access method to render a given event in the JSON format.
        /// <para />Use the #getJSONRenderer to obtain a renderer instance that allows repeated rendering of the same type of event.
        /// For performance reasons obtaining a dedicated renderer instance is the preferred method compared to repeated rendering via this method.
        /// </summary>
        /// <param name="title">the JSON root title</param>
        /// <param name="theEvent">the event to render</param>
        /// <returns>JSON formatted text</returns>
        string RenderJSON(
            string title,
            EventBean theEvent);

        /// <summary>
        /// Quick-access method to render a given event in the JSON format.
        /// <para />Use the #getJSONRenderer to obtain a renderer instance that allows repeated rendering of the same type of event.
        /// For performance reasons obtaining a dedicated renderer instance is the preferred method compared to repeated rendering via this method.
        /// </summary>
        /// <param name="title">the JSON root title</param>
        /// <param name="theEvent">the event to render</param>
        /// <param name="options">are JSON rendering options</param>
        /// <returns>JSON formatted text</returns>
        string RenderJSON(
            string title,
            EventBean theEvent,
            JSONRenderingOptions options);

        /// <summary>
        /// Returns a render for the XML format, valid only for the given event type and its subtypes.
        /// </summary>
        /// <param name="eventType">to return renderer for</param>
        /// <returns>XML format renderer</returns>
        XMLEventRenderer GetXMLRenderer(EventType eventType);

        /// <summary>
        /// Returns a render for the XML format, valid only for the given event type and its subtypes.
        /// </summary>
        /// <param name="eventType">to return renderer for</param>
        /// <param name="options">rendering options</param>
        /// <returns>XML format renderer</returns>
        XMLEventRenderer GetXMLRenderer(
            EventType eventType,
            XMLRenderingOptions options);

        /// <summary>
        /// Quick-access method to render a given event in the XML format.
        /// <para />Use the #getXMLRenderer to obtain a renderer instance that allows repeated rendering of the same type of event.
        /// For performance reasons obtaining a dedicated renderer instance is the preferred method compared to repeated rendering via this method.
        /// </summary>
        /// <param name="rootElementName">the root element name that may also include namespace information</param>
        /// <param name="theEvent">the event to render</param>
        /// <returns>XML formatted text</returns>
        string RenderXML(
            string rootElementName,
            EventBean theEvent);

        /// <summary>
        /// Quick-access method to render a given event in the XML format.
        /// <para />Use the #getXMLRenderer to obtain a renderer instance that allows repeated rendering of the same type of event.
        /// For performance reasons obtaining a dedicated renderer instance is the preferred method compared to repeated rendering via this method.
        /// </summary>
        /// <param name="rootElementName">the root element name that may also include namespace information</param>
        /// <param name="theEvent">the event to render</param>
        /// <param name="options">are XML rendering options</param>
        /// <returns>XML formatted text</returns>
        string RenderXML(
            string rootElementName,
            EventBean theEvent,
            XMLRenderingOptions options);
    }
} // end of namespace