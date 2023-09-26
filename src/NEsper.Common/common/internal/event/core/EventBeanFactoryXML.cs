///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Xml;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventBeanFactoryXML : EventBeanFactory
    {
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly EventType type;

        public EventBeanFactoryXML(
            EventType type,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.type = type;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public Type UnderlyingType => type.UnderlyingType;

        public EventBean Wrap(object underlying)
        {
            return eventBeanTypedEventFactory.AdapterForTypedDOM((XmlNode)underlying, type);
        }
    }
} // end of namespace