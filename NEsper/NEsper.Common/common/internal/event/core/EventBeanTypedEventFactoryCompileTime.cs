///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Xml;
using com.espertech.esper.common.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventBeanTypedEventFactoryCompileTime : EventBeanTypedEventFactory
    {
        public static readonly EventBeanTypedEventFactoryCompileTime INSTANCE =
            new EventBeanTypedEventFactoryCompileTime();

        private EventBeanTypedEventFactoryCompileTime()
        {
        }

        public MappedEventBean AdapterForTypedMap(IDictionary<string, object> value, EventType eventType)
        {
            throw new UnsupportedOperationException();
        }

        public ObjectArrayBackedEventBean AdapterForTypedObjectArray(object[] value, EventType eventType)
        {
            throw new UnsupportedOperationException();
        }

        public EventBean AdapterForTypedBean(object value, EventType eventType)
        {
            throw new UnsupportedOperationException();
        }

        public EventBean AdapterForTypedDOM(XmlNode value, EventType eventType)
        {
            throw new UnsupportedOperationException();
        }

        public EventBean AdapterForTypedAvro(object value, EventType eventType)
        {
            throw new UnsupportedOperationException();
        }

        public EventBean AdapterForTypedWrapper(
            EventBean decoratedUnderlying, IDictionary<string, object> map, EventType wrapperEventType)
        {
            throw new UnsupportedOperationException();
        }

        private IllegalStateException GetUnsupported()
        {
            return new IllegalStateException("Event bean generation not supported at compile time");
        }
    }
} // end of namespace