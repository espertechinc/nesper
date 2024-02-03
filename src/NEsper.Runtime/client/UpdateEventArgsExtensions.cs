///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.render;
using com.espertech.esper.common.@internal.@event.render;

namespace com.espertech.esper.runtime.client
{
    public static class UpdateEventArgsExtensions
    {
        /// <summary>
        /// Converts the event bean into a contract event.
        /// </summary>
        /// <param name="eventBean">The event bean.</param>
        /// <returns></returns>
        public static XElement ToXElement(this EventBean eventBean)
        {
            if (eventBean.Underlying is XElement xElement)
            {
                return xElement;
            }

            var renderingOptions = new XMLRenderingOptions {IsDefaultAsAttribute = false, PreventLooping = true};
            var eventRenderer = new XMLRendererImpl(eventBean.EventType, renderingOptions);
            var rendered = eventRenderer.Render("eventBean", eventBean);
            return XElement.Parse(rendered);
        }
        
        /// <summary>
        /// Converts the array of even beans into an XElement.
        /// </summary>
        /// <param name="eventBeans">The event beans.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <returns></returns>
        public static XElement ToXElement(this EventBean[] eventBeans, string elementName)
        {
            if (eventBeans != null)
            {
                return new XElement(
                    elementName,
                    eventBeans.Select(ToXElement).Cast<object>().ToArray());
            }

            return new XElement(elementName);
        }
        
        /// <summary>
        /// Converts the Update events args into an XElement.
        /// </summary>
        /// <param name="updateEventArgs">The <see cref="UpdateEventArgs"/> instance containing the event data.</param>
        /// <param name="elementName">Name of the element.</param>
        /// <returns></returns>
        public static XElement ToXElement(this UpdateEventArgs updateEventArgs, string elementName)
        {
            return new XElement(
                elementName,
                new XElement("statement", updateEventArgs.Statement.Name),
                ToXElement(updateEventArgs.NewEvents, "new"),
                ToXElement(updateEventArgs.OldEvents, "old"));
        }

        /// <summary>
        /// Converts the Update events args into an XElement.
        /// </summary>
        /// <param name="updateEventArgs">The <see cref="UpdateEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        public static XElement ToXElement(this UpdateEventArgs updateEventArgs)
        {
            return ToXElement(updateEventArgs, "Update");
        }
        
        /// <summary>
        /// Converts the element to Update event args.
        /// </summary>
        /// <param name="eventElement">The event element.</param>
        /// <param name="eventBeanFactory">The event bean factory.</param>
        /// <returns></returns>
        public static UpdateEventArgs ToUpdateEventArgs(this XElement eventElement, Func<XElement, EventBean> eventBeanFactory)
        {
            if (eventElement == null) {
                throw new ArgumentNullException("eventElement");
            }

            var oldElement = eventElement.Element(XName.Get("old"));
            if (oldElement == null) {
                throw new InvalidDataException("missing required element 'old'");
            }

            var newElement = eventElement.Element(XName.Get("new"));
            if (newElement == null) {
                throw new InvalidDataException("missing required element 'new'");
            }

            // find the xelements that represent the old and new event beans
            // and convert them back into an eventBean form
            var oldEvents = oldElement
                .Elements()
                .Select(eventBeanFactory.Invoke)
                .ToArray();
            var newEvents = newElement
                .Elements()
                .Select(eventBeanFactory.Invoke)
                .ToArray();

            // construct event args
            var updateEventArgs = new UpdateEventArgs(
                null,
                null,
                newEvents,
                oldEvents);

            return updateEventArgs;
        }
    }
}