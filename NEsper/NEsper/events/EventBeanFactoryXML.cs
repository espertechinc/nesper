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

using com.espertech.esper.client;

namespace com.espertech.esper.events
{
    public class EventBeanFactoryXML : EventBeanFactory
    {
        private readonly EventType _type;
        private readonly EventAdapterService _eventAdapterService;
    
        public EventBeanFactoryXML(EventType type, EventAdapterService eventAdapterService) 
        {
            _type = type;
            _eventAdapterService = eventAdapterService;
        }
    
        public EventBean Wrap(Object underlying)
        {
            if (underlying is XmlNode) {
                return _eventAdapterService.AdapterForTypedDOM((XmlNode) underlying, _type);
            }

            return _eventAdapterService.AdapterForTypedDOM((XElement) underlying, _type);
        }
    }
}
