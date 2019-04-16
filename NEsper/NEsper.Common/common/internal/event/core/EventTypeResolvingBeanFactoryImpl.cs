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
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.xml;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventTypeResolvingBeanFactoryImpl : EventTypeResolvingBeanFactory
    {
        private readonly EventTypeAvroHandler avroHandler;
        private readonly EventTypeRepository eventTypeRepository;

        public EventTypeResolvingBeanFactoryImpl(
            EventTypeRepository eventTypeRepository,
            EventTypeAvroHandler avroHandler)
        {
            this.eventTypeRepository = eventTypeRepository;
            this.avroHandler = avroHandler;
        }

        public EventBean AdapterForObjectArray(
            object[] theEvent,
            string eventTypeName)
        {
            var type = eventTypeRepository.GetTypeByName(eventTypeName);
            EventTypeUtility.ValidateTypeObjectArray(eventTypeName, type);
            return new ObjectArrayEventBean(theEvent, type);
        }

        public EventBean AdapterForBean(
            object data,
            string eventTypeName)
        {
            var type = eventTypeRepository.GetTypeByName(eventTypeName);
            EventTypeUtility.ValidateTypeBean(eventTypeName, type);
            return new BeanEventBean(data, type);
        }

        public EventBean AdapterForMap(
            IDictionary<string, object> map,
            string eventTypeName)
        {
            var type = eventTypeRepository.GetTypeByName(eventTypeName);
            EventTypeUtility.ValidateTypeMap(eventTypeName, type);
            return new MapEventBean(map, type);
        }

        public EventBean AdapterForXMLDOM(
            XmlNode node,
            string eventTypeName)
        {
            var type = eventTypeRepository.GetTypeByName(eventTypeName);
            EventTypeUtility.ValidateTypeXMLDOM(eventTypeName, type);
            var namedNode = GetXMLNodeFromDocument(node);
            return new XMLEventBean(namedNode, type);
        }

        public EventBean AdapterForAvro(
            object avroGenericDataDotRecord,
            string eventTypeName)
        {
            var type = eventTypeRepository.GetTypeByName(eventTypeName);
            EventTypeUtility.ValidateTypeAvro(eventTypeName, type);
            return avroHandler.AdapterForTypeAvro(avroGenericDataDotRecord, type);
        }

        public static XmlNode GetXMLNodeFromDocument(XmlNode node)
        {
            var resultNode = node;
            if (node is XmlDocument document) {
                resultNode = document.DocumentElement;
            }
            else if (!(node is XmlElement)) {
                throw new EPException("Unexpected DOM node of type '" + node.GetType() + "' encountered, please supply a Document or Element node");
            }

            return resultNode;
        }
    }
} // end of namespace