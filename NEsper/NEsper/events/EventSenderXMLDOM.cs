///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.core.thread;
using com.espertech.esper.events.xml;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Event sender for XML DOM-backed events.
    /// <para />
    /// Allows sending only event objects of type Node or Document, does check the root name 
    /// of the XML document which must match the event type root name as configured. Any other 
    /// event object generates an error.
    /// </summary>
    public class EventSenderXMLDOM : EventSender
    {
        private readonly EPRuntimeEventSender _runtimeEventSender;
        private readonly BaseXMLEventType _baseXmlEventType;
        private readonly bool _validateRootElement;
        private readonly EventAdapterService _eventAdapterService;
        private readonly ThreadingService _threadingService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="runtimeEventSender">for processing events</param>
        /// <param name="baseXMLEventType">the event type</param>
        /// <param name="eventAdapterService">for event bean creation</param>
        /// <param name="threadingService">for inbound threading</param>
        public EventSenderXMLDOM(EPRuntimeEventSender runtimeEventSender, BaseXMLEventType baseXMLEventType, EventAdapterService eventAdapterService, ThreadingService threadingService)
        {
            _runtimeEventSender = runtimeEventSender;
            _baseXmlEventType = baseXMLEventType;
            _validateRootElement = baseXMLEventType.ConfigurationEventTypeXMLDOM.IsEventSenderValidatesRoot;
            _eventAdapterService = eventAdapterService;
            _threadingService = threadingService;        
        }
    
        public void SendEvent(Object node)
        {
            SendEvent(node, false);
        }
    
        public void Route(Object node)
        {
            SendEvent(node, true);
        }
    
        private void SendEvent(Object node, bool isRoute)
        {
            XmlNode namedNode;
            if (node is XmlDocument)
            {
                namedNode = ((XmlDocument) node).DocumentElement;
            }
            else if (node is XmlElement)
            {
                namedNode = (XmlElement) node;
            }
            else
            {
                throw new EPException("Unexpected event object type '" + node.GetType().FullName + "' encountered, please supply a System.Xml.XmlDocument or Element node");
            }
    
            if (_validateRootElement)
            {
                var theNodeName = namedNode.LocalName;
                if (theNodeName == null)
                {
                    theNodeName = namedNode.Name;
                }
    
                if (!theNodeName.Equals(_baseXmlEventType.RootElementName))
                {
                    throw new EPException("Unexpected root element name '" + theNodeName + "' encountered, expected a root element name of '" + _baseXmlEventType.RootElementName + "'");
                }
            }
    
            EventBean theEvent = _eventAdapterService.AdapterForTypedDOM(namedNode, _baseXmlEventType);
            if (isRoute)
            {
                _runtimeEventSender.RouteEventBean(theEvent);            
            }
            else
            {
                if ((ThreadingOption.IsThreadingEnabled) && (_threadingService.IsInboundThreading))
                {
                    _threadingService.SubmitInbound(new InboundUnitSendWrapped(theEvent, _runtimeEventSender).Run);
                }
                else
                {
                    _runtimeEventSender.ProcessWrappedEvent(theEvent);
                }
            }
        }
    }
}
