///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.core
{
    public interface EventBeanTypedEventFactory
    {
        MappedEventBean AdapterForTypedMap(
            IDictionary<string, object> value,
            EventType eventType);

        ObjectArrayBackedEventBean AdapterForTypedObjectArray(
            object[] value,
            EventType eventType);

        EventBean AdapterForTypedObject(
            object value,
            EventType eventType);

        EventBean AdapterForTypedDOM(
            XmlNode value,
            EventType eventType);

        EventBean AdapterForTypedAvro(
            object avroGenericDataDotRecord,
            EventType eventType);

        EventBean AdapterForTypedWrapper(
            EventBean decoratedUnderlying,
            IDictionary<string, object> map,
            EventType wrapperEventType);

        EventBean AdapterForTypedJson(
            object underlying,
            EventType eventType);
    }

    public static class EventBeanTypedEventFactoryExtensions
    {
        public static EventBean AdapterForGivenType(
            this EventBeanTypedEventFactory factory,
            object value,
            EventType eventType)
        {
            if (eventType is BeanEventType) {
                return factory.AdapterForTypedObject(value, eventType);
            }
            else if (eventType is MapEventType) {
                return factory.AdapterForTypedMap((IDictionary<string, object>)value, eventType);
            }
            else if (eventType is ObjectArrayEventType) {
                return factory.AdapterForTypedObjectArray((object[])value, eventType);
            }
            else if (eventType is JsonEventType) {
                return factory.AdapterForTypedJson(value, eventType);
            }
            else if (eventType is AvroSchemaEventType) {
                return factory.AdapterForTypedAvro(value, eventType);
            }
            else if (eventType is BaseXMLEventType) {
                return factory.AdapterForTypedDOM((XmlNode)value, eventType);
            }
            else if (eventType is WrapperEventType) {
                throw new UnsupportedOperationException(
                    "EventBean allocation for wrapper event types without the decorated event type is not supported");
            }

            throw new UnsupportedOperationException(
                "Event type " +
                eventType.Name +
                " of type " +
                eventType.GetType().CleanName() +
                " is not a recognized type");
        }
    }

    public class EventBeanTypedEventFactoryConstants
    {
        public const string ADAPTERFORTYPEDMAP = "AdapterForTypedMap";
        public const string ADAPTERFORTYPEDOBJECTARRAY = "AdapterForTypedObjectArray";
        public const string ADAPTERFORTYPEDBEAN = "AdapterForTypedObject";
        public const string ADAPTERFORTYPEDDOM = "AdapterForTypedDOM";
        public const string ADAPTERFORTYPEDAVRO = "AdapterForTypedAvro";
        public const string ADAPTERFORTYPEDWRAPPER = "AdapterForTypedWrapper";
        public const string ADAPTERFORTYPEDJSON = "AdapterForTypedJson";
    }
} // end of namespace