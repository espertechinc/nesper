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
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.@event.eventtyperepo;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventBeanServiceImpl : EventBeanService
    {
        private readonly EventTypeRepositoryImpl eventTypeRepositoryPreconfigured;
        private readonly PathRegistry<string, EventType> pathEventTypes;
        private readonly EventBeanTypedEventFactory typedEventFactory;

        public EventBeanServiceImpl(
            EventTypeRepositoryImpl eventTypeRepositoryPreconfigured,
            PathRegistry<string, EventType> pathEventTypes,
            EventBeanTypedEventFactory typedEventFactory)
        {
            this.eventTypeRepositoryPreconfigured = eventTypeRepositoryPreconfigured;
            this.pathEventTypes = pathEventTypes;
            this.typedEventFactory = typedEventFactory;
        }

        public EventBean AdapterForMap(
            IDictionary<string, object> theEvent,
            string eventTypeName)
        {
            var eventType = FindType(eventTypeName);
            return typedEventFactory.AdapterForTypedMap(theEvent, eventType);
        }

        public EventBean AdapterForAvro(
            object avroGenericDataDotRecord,
            string eventTypeName)
        {
            var eventType = FindType(eventTypeName);
            return typedEventFactory.AdapterForTypedAvro(avroGenericDataDotRecord, eventType);
        }

        public EventBean AdapterForObjectArray(
            object[] theEvent,
            string eventTypeName)
        {
            var eventType = FindType(eventTypeName);
            return typedEventFactory.AdapterForTypedObjectArray(theEvent, eventType);
        }

        public EventBean AdapterForTypedAvro(
            object avroGenericDataDotRecord,
            EventType eventType)
        {
            return typedEventFactory.AdapterForTypedAvro(avroGenericDataDotRecord, eventType);
        }

        public EventBean AdapterForTypedMap(
            IDictionary<string, object> properties,
            EventType eventType)
        {
            return typedEventFactory.AdapterForTypedMap(properties, eventType);
        }

        public EventBean AdapterForTypedObjectArray(
            object[] props,
            EventType eventType)
        {
            return typedEventFactory.AdapterForTypedObjectArray(props, eventType);
        }

        public EventBean AdapterForTypedDOM(
            XmlNode node,
            EventType eventType)
        {
            return typedEventFactory.AdapterForTypedDOM(node, eventType);
        }

        public EventBean AdapterForBean(
            object theEvent,
            string eventTypeName)
        {
            var eventType = FindType(eventTypeName);
            return typedEventFactory.AdapterForTypedObject(theEvent, eventType);
        }

        public EventBean AdapterForDOM(
            XmlNode node,
            string eventTypeName)
        {
            var eventType = FindType(eventTypeName);
            return typedEventFactory.AdapterForTypedDOM(node, eventType);
        }

        public EventType GetExistsTypeByName(string eventTypeName)
        {
            return FindTypeMayNull(eventTypeName);
        }

        public EventBean AdapterForTypedObject(
            object bean,
            EventType eventType)
        {
            return typedEventFactory.AdapterForTypedObject(bean, eventType);
        }

        private EventType FindType(string eventTypeName)
        {
            var eventType = FindTypeMayNull(eventTypeName);
            if (eventType == null) {
                throw new EPException("Failed to find event type '" + eventTypeName + "'");
            }

            return eventType;
        }

        private EventType FindTypeMayNull(string eventTypeName)
        {
            var eventType = eventTypeRepositoryPreconfigured.GetTypeByName(eventTypeName);
            if (eventType != null) {
                return eventType;
            }

            try {
                eventType = pathEventTypes.GetAnyModuleExpectSingle(eventTypeName, null).First;
            }
            catch (PathException ex) {
                throw new EPException("Failed to obtain event type '" + eventTypeName + "': " + ex.Message, ex);
            }

            return eventType;
        }
    }
} // end of namespace