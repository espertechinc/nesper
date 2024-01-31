///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.statement.thread;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.xml
{
    /// <summary>
    ///     Event sender for XML DOM-backed events.
    ///     <para>
    ///         Allows sending only event objects of type Node or Document, does check the root name of the XML document
    ///         which must match the event type root name as configured. Any other event object generates an error.
    ///     </para>
    /// </summary>
    public class EventSenderXMLDOM : EventSender
    {
        private readonly BaseXMLEventType baseXMLEventType;
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly EPRuntimeEventProcessWrapped runtimeEventSender;
        private readonly ThreadingCommon threadingService;
        private readonly bool validateRootElement;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="runtimeEventSender">for processing events</param>
        /// <param name="baseXMLEventType">the event type</param>
        /// <param name="threadingService">for inbound threading</param>
        /// <param name="eventBeanTypedEventFactory">for event bean creation</param>
        public EventSenderXMLDOM(
            EPRuntimeEventProcessWrapped runtimeEventSender,
            BaseXMLEventType baseXMLEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ThreadingCommon threadingService)
        {
            this.runtimeEventSender = runtimeEventSender;
            this.baseXMLEventType = baseXMLEventType;
            validateRootElement = baseXMLEventType.ConfigurationEventTypeXMLDOM.IsEventSenderValidatesRoot;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.threadingService = threadingService;
        }

        public void SendEvent(object theEvent)
        {
            SendEvent(theEvent, false);
        }

        public void RouteEvent(object theEvent)
        {
            SendEvent(theEvent, true);
        }

        private void SendEvent(
            object node,
            bool isRoute)
        {
            XmlNode namedNode;
            if (node is XmlDocument document) {
                namedNode = document.DocumentElement;
            }
            else if (node is XmlElement element) {
                namedNode = element;
            }
            else {
                throw new EPException(
                    "Unexpected event object type '" +
                    node.GetType().CleanName() +
                    "' encountered, please supply a XmlDocument or XmlElement node");
            }

            if (validateRootElement) {
                var getNodeName = namedNode.Name;
                if (getNodeName != baseXMLEventType.RootElementName) {
                    throw new EPException(
                        "Unexpected root element name '" +
                        getNodeName +
                        "' encountered, expected a root element name of '" +
                        baseXMLEventType.RootElementName +
                        "'");
                }
            }

            var theEvent = eventBeanTypedEventFactory.AdapterForTypedDOM(namedNode, baseXMLEventType);
            if (isRoute) {
                runtimeEventSender.RouteEventBean(theEvent);
            }
            else {
                if (threadingService.IsInboundThreading) {
                    threadingService.SubmitInbound(theEvent, runtimeEventSender);
                }
                else {
                    runtimeEventSender.ProcessWrappedEvent(theEvent);
                }
            }
        }
    }
} // end of namespace