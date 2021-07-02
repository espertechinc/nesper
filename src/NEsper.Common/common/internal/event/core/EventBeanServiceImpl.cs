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
        private readonly EventTypeRepositoryImpl _eventTypeRepositoryPreconfigured;
        private readonly PathRegistry<string, EventType> _pathEventTypes;
        private readonly EventBeanTypedEventFactory _typedEventFactory;

        public EventBeanServiceImpl(
            EventTypeRepositoryImpl eventTypeRepositoryPreconfigured,
            PathRegistry<string, EventType> pathEventTypes,
            EventBeanTypedEventFactory typedEventFactory)
        {
            this._eventTypeRepositoryPreconfigured = eventTypeRepositoryPreconfigured;
            this._pathEventTypes = pathEventTypes;
            this._typedEventFactory = typedEventFactory;
        }

        public EventBean AdapterForMap(
            IDictionary<string, object> theEvent,
            string eventTypeName)
        {
            var eventType = FindType(eventTypeName);
            return _typedEventFactory.AdapterForTypedMap(theEvent, eventType);
        }
        
        public EventBean AdapterForBean(
            object theEvent,
            string eventTypeName)
        {
            var eventType = FindType(eventTypeName);
            return _typedEventFactory.AdapterForTypedObject(theEvent, eventType);
        }

        public EventBean AdapterForAvro(
            object avroGenericDataDotRecord,
            string eventTypeName)
        {
            var eventType = FindType(eventTypeName);
            return _typedEventFactory.AdapterForTypedAvro(avroGenericDataDotRecord, eventType);
        }

        public EventBean AdapterForObjectArray(
            object[] theEvent,
            string eventTypeName)
        {
            var eventType = FindType(eventTypeName);
            return _typedEventFactory.AdapterForTypedObjectArray(theEvent, eventType);
        }
        
        public EventBean AdapterForDOM(
            XmlNode node,
            string eventTypeName)
        {
            var eventType = FindType(eventTypeName);
            return _typedEventFactory.AdapterForTypedDOM(node, eventType);
        }
        
        
        public EventType GetExistsTypeByName(string eventTypeName)
        {
            return FindTypeMayNull(eventTypeName);
        }

        public EventBean AdapterForTypedObject(
            object bean,
            EventType eventType)
        {
            return _typedEventFactory.AdapterForTypedObject(bean, eventType);
        }
        
        public EventBean AdapterForTypedAvro(
            object avroGenericDataDotRecord,
            EventType eventType)
        {
            return _typedEventFactory.AdapterForTypedAvro(avroGenericDataDotRecord, eventType);
        }

        public MappedEventBean AdapterForTypedMap(
            IDictionary<string, object> properties,
            EventType eventType)
        {
            return _typedEventFactory.AdapterForTypedMap(properties, eventType);
        }

        public ObjectArrayBackedEventBean AdapterForTypedObjectArray(
            object[] props,
            EventType eventType)
        {
            return _typedEventFactory.AdapterForTypedObjectArray(props, eventType);
        }

        public EventBean AdapterForTypedDOM(
            XmlNode node,
            EventType eventType)
        {
            return _typedEventFactory.AdapterForTypedDOM(node, eventType);
        }

        public EventBean AdapterForTypedWrapper(
            EventBean decoratedUnderlying,
            IDictionary<string, object> map,
            EventType wrapperEventType)
        {
            return _typedEventFactory.AdapterForTypedWrapper(decoratedUnderlying, map, wrapperEventType);
        }

        public EventBean AdapterForTypedJson(
            object underlying,
            EventType eventType)
        {
            return _typedEventFactory.AdapterForTypedJson(underlying, eventType);
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
            var eventType = _eventTypeRepositoryPreconfigured.GetTypeByName(eventTypeName);
            if (eventType != null) {
                return eventType;
            }

            try {
                eventType = _pathEventTypes.GetAnyModuleExpectSingle(eventTypeName, null).First;
            }
            catch (PathException ex) {
                throw new EPException("Failed to obtain event type '" + eventTypeName + "': " + ex.Message, ex);
            }

            return eventType;
        }
    }
} // end of namespace