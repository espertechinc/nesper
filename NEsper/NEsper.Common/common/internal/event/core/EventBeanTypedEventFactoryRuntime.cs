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
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventBeanTypedEventFactoryRuntime : EventBeanTypedEventFactory
    {
        private readonly EventTypeAvroHandler eventTypeAvroHandler;

        public EventBeanTypedEventFactoryRuntime(EventTypeAvroHandler eventTypeAvroHandler)
        {
            this.eventTypeAvroHandler = eventTypeAvroHandler;
        }

        public MappedEventBean AdapterForTypedMap(
            IDictionary<string, object> value,
            EventType eventType)
        {
            return new MapEventBean(value, eventType);
        }

        public ObjectArrayBackedEventBean AdapterForTypedObjectArray(
            object[] value,
            EventType eventType)
        {
            return new ObjectArrayEventBean(value, eventType);
        }

        public EventBean AdapterForTypedObject(
            object value,
            EventType eventType)
        {
            return new BeanEventBean(value, eventType);
        }

        public EventBean AdapterForTypedDOM(
            XmlNode value,
            EventType eventType)
        {
            return new XMLEventBean(value, eventType);
        }

        public EventBean AdapterForTypedAvro(
            object avroGenericDataDotRecord,
            EventType eventType)
        {
            return eventTypeAvroHandler.AdapterForTypeAvro(avroGenericDataDotRecord, eventType);
        }

        public EventBean AdapterForTypedWrapper(
            EventBean decoratedUnderlying,
            IDictionary<string, object> map,
            EventType wrapperEventType)
        {
            if (decoratedUnderlying is DecoratingEventBean) {
                DecoratingEventBean wrapper = (DecoratingEventBean) decoratedUnderlying;
                if (!wrapper.DecoratingProperties.IsEmpty()) {
                    if (map.IsEmpty()) {
                        map = new Dictionary<string, object>();
                    }

                    map.PutAll(wrapper.DecoratingProperties);
                }

                return new WrapperEventBean(wrapper.UnderlyingEvent, map, wrapperEventType);
            }
            else {
                return new WrapperEventBean(decoratedUnderlying, map, wrapperEventType);
            }
        }

        public EventBean AdapterForTypedJson(
            object underlying,
            EventType eventType)
        {
            return new JsonEventBean(underlying, eventType);
        }
    }
} // end of namespace