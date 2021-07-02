///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
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
        private readonly BaseXMLEventType _baseXmlEventType;
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly EPRuntimeEventProcessWrapped _runtimeEventSender;
        private readonly ThreadingCommon _threadingService;
        private readonly bool _validateRootElement;

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
            this._runtimeEventSender = runtimeEventSender;
            this._baseXmlEventType = baseXMLEventType;
            _validateRootElement = baseXMLEventType.ConfigurationEventTypeXMLDOM.IsEventSenderValidatesRoot;
            this._eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this._threadingService = threadingService;
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
            if (node is XmlDocument) {
                namedNode = ((XmlDocument) node).DocumentElement;
            }
            else if (node is XmlElement) {
                namedNode = (XmlElement) node;
            }
            else {
                throw new EPException(
                    "Unexpected event object type '" +
                    node.GetType().TypeSafeName() +
                    "' encountered, please supply a XmlDocument or XmlElement node");
            }

            if (_validateRootElement) {
                var getNodeName = namedNode.Name;
                if (getNodeName != _baseXmlEventType.RootElementName) {
                    throw new EPException(
                        "Unexpected root element name '" +
                        getNodeName +
                        "' encountered, expected a root element name of '" +
                        _baseXmlEventType.RootElementName +
                        "'");
                }
            }

            var theEvent = _eventBeanTypedEventFactory.AdapterForTypedDOM(namedNode, _baseXmlEventType);
            if (isRoute) {
                _runtimeEventSender.RouteEventBean(theEvent);
            }
            else {
                if (_threadingService.IsInboundThreading) {
                    _threadingService.SubmitInbound(theEvent, _runtimeEventSender);
                }
                else {
                    _runtimeEventSender.ProcessWrappedEvent(theEvent);
                }
            }
        }
    }
} // end of namespace