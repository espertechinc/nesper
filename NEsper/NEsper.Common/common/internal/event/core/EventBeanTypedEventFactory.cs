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

namespace com.espertech.esper.common.@internal.@event.core
{
    public interface EventBeanTypedEventFactory
    {
        MappedEventBean AdapterForTypedMap(IDictionary<string, object> value, EventType eventType);

        ObjectArrayBackedEventBean AdapterForTypedObjectArray(object[] value, EventType eventType);

        EventBean AdapterForTypedBean(object value, EventType eventType);

        EventBean AdapterForTypedDOM(XmlNode value, EventType eventType);

        EventBean AdapterForTypedAvro(object avroGenericDataDotRecord, EventType eventType);

        EventBean AdapterForTypedWrapper(
            EventBean decoratedUnderlying, IDictionary<string, object> map, EventType wrapperEventType);
    }

    public class EventBeanTypedEventFactoryConstants
    {
        public const string ADAPTERFORTYPEDMAP = "adapterForTypedMap";
        public const string ADAPTERFORTYPEDOBJECTARRAY = "adapterForTypedObjectArray";
        public const string ADAPTERFORTYPEDBEAN = "adapterForTypedBean";
        public const string ADAPTERFORTYPEDDOM = "adapterForTypedDOM";
        public const string ADAPTERFORTYPEDAVRO = "adapterForTypedAvro";
        public const string ADAPTERFORTYPEDWRAPPER = "adapterForTypedWrapper";
    }
} // end of namespace