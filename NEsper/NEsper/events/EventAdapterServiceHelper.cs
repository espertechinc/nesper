///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.core;
using com.espertech.esper.events.avro;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.xml;
using com.espertech.esper.util;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Helper for writeable events.
    /// </summary>
    public class EventAdapterServiceHelper
    {
        public static string GetMessageExpecting(string eventTypeName, EventType existingType, string typeOfEventType)
        {
            var message = "Event type named '" + eventTypeName + "' has not been defined or is not a " + typeOfEventType +
                          " event type";
            if (existingType != null)
            {
                message += ", the name '" + eventTypeName + "' refers to a " + existingType.UnderlyingType.GetCleanName() + " event type";
            }
            else
            {
                message += ", the name '" + eventTypeName + "' has not been defined as an event type";
            }
            return message;
        }

        public static EventBeanFactory GetFactoryForType(EventType type, EventAdapterService eventAdapterService)
        {
            if (type is WrapperEventType)
            {
                var wrapperType = (WrapperEventType) type;
                if (wrapperType.UnderlyingEventType is BeanEventType)
                {
                    return new EventBeanFactoryBeanWrapped(
                        wrapperType.UnderlyingEventType, wrapperType, eventAdapterService);
                }
            }
            if (type is BeanEventType)
            {
                return new EventBeanFactoryBean(type, eventAdapterService);
            }
            if (type is MapEventType)
            {
                return new EventBeanFactoryMap(type, eventAdapterService);
            }
            if (type is ObjectArrayEventType)
            {
                return new EventBeanFactoryObjectArray(type, eventAdapterService);
            }
            if (type is BaseXMLEventType)
            {
                return new EventBeanFactoryXML(type, eventAdapterService);
            }
            if (type is AvroSchemaEventType)
            {
                return eventAdapterService.EventAdapterAvroHandler.GetEventBeanFactory(type, eventAdapterService);
            }
            throw new ArgumentException(
                "Cannot create event bean factory for event type '" + type.Name + "': " + type.GetType().FullName +
                " is not a recognized event type or supported wrap event type");
        }

        /// <summary>
        /// Returns descriptors for all writable properties.
        /// </summary>
        /// <param name="eventType">to reflect on</param>
        /// <param name="allowAnyType">whether any type property can be populated</param>
        /// <returns>list of writable properties</returns>
        public static ICollection<WriteablePropertyDescriptor> GetWriteableProperties(EventType eventType, bool allowAnyType)
        {
            if (!(eventType is EventTypeSPI))
            {
                return null;
            }
            if (eventType is BeanEventType)
            {
                var beanEventType = (BeanEventType) eventType;
                return PropertyHelper.GetWritableProperties(beanEventType.UnderlyingType);
            }
            var typeSPI = (EventTypeSPI) eventType;
            if (!allowAnyType && !AllowPropulate(typeSPI))
            {
                return null;
            }
            if (eventType is BaseNestableEventType)
            {
                IDictionary<string, Object> mapdef = ((BaseNestableEventType) eventType).Types;
                ISet<WriteablePropertyDescriptor> writables = new LinkedHashSet<WriteablePropertyDescriptor>();
                foreach (var types in mapdef)
                {
                    if (types.Value is Type)
                    {
                        writables.Add(new WriteablePropertyDescriptor(types.Key, (Type) types.Value, null));
                    }
                    if (types.Value is string)
                    {
                        var typeName = types.Value.ToString();
                        Type clazz = TypeHelper.GetPrimitiveTypeForName(typeName);
                        if (clazz != null)
                        {
                            writables.Add(new WriteablePropertyDescriptor(types.Key, clazz, null));
                        }
                    }
                }
                return writables;
            }
            else if (eventType is AvroSchemaEventType)
            {
                var writables = new LinkedHashSet<WriteablePropertyDescriptor>();
                var desc = typeSPI.WriteableProperties;
                foreach (var prop in desc)
                {
                    writables.Add(new WriteablePropertyDescriptor(prop.PropertyName, prop.PropertyType, null));
                }
                return writables;
            }
            else
            {
                return null;
            }
        }

        private static bool AllowPropulate(EventTypeSPI typeSPI)
        {
            if (!typeSPI.Metadata.IsApplicationConfigured &&
                typeSPI.Metadata.TypeClass != TypeClass.ANONYMOUS &&
                typeSPI.Metadata.TypeClass != TypeClass.TABLE)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Return an adapter for the given type of event using the pre-validated object.
        /// </summary>
        /// <param name="theEvent">value object</param>
        /// <param name="eventType">type of event</param>
        /// <param name="eventAdapterService">service for instances</param>
        /// <returns>event adapter</returns>
        public static EventBean AdapterForType(
            Object theEvent,
            EventType eventType,
            EventAdapterService eventAdapterService)
        {
            if (theEvent == null)
            {
                return null;
            }
            if (eventType is BeanEventType)
            {
                return eventAdapterService.AdapterForTypedObject(theEvent, (BeanEventType)eventType);
            }
            else if (eventType is MapEventType)
            {
                return eventAdapterService.AdapterForTypedMap((IDictionary<string, object>) theEvent, eventType);
            }
            else if (eventType is ObjectArrayEventType)
            {
                return eventAdapterService.AdapterForTypedObjectArray((Object[]) theEvent, eventType);
            }
            else if (eventType is BaseConfigurableEventType)
            {
                return eventAdapterService.AdapterForTypedDOM((XmlNode) theEvent, eventType);
            }
            else if (eventType is AvroSchemaEventType)
            {
                return eventAdapterService.AdapterForTypedAvro(theEvent, eventType);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a factory for creating and populating event object instances for the given type.
        /// </summary>
        /// <param name="eventAdapterService">fatory for event</param>
        /// <param name="eventType">to create underlying objects for</param>
        /// <param name="properties">to write</param>
        /// <param name="engineImportService">for resolving methods</param>
        /// <param name="allowAnyType">whether any type property can be populated</param>
        /// <param name="avroHandler">avro handler</param>
        /// <exception cref="EventBeanManufactureException">if a factory cannot be created for the type</exception>
        /// <returns>factory</returns>
        public static EventBeanManufacturer GetManufacturer(
            EventAdapterService eventAdapterService,
            EventType eventType,
            IList<WriteablePropertyDescriptor> properties,
            EngineImportService engineImportService,
            bool allowAnyType,
            EventAdapterAvroHandler avroHandler)
        {
            if (!(eventType is EventTypeSPI))
            {
                return null;
            }
            if (eventType is BeanEventType)
            {
                var beanEventType = (BeanEventType) eventType;
                return new EventBeanManufacturerBean(
                    beanEventType, eventAdapterService, properties, engineImportService);
            }
            var typeSPI = (EventTypeSPI) eventType;
            if (!allowAnyType && !AllowPropulate(typeSPI))
            {
                return null;
            }
            if (eventType is MapEventType)
            {
                var mapEventType = (MapEventType) eventType;
                return new EventBeanManufacturerMap(mapEventType, eventAdapterService, properties);
            }
            if (eventType is ObjectArrayEventType)
            {
                var objectArrayEventType = (ObjectArrayEventType) eventType;
                return new EventBeanManufacturerObjectArray(objectArrayEventType, eventAdapterService, properties);
            }
            if (eventType is AvroSchemaEventType)
            {
                var avroSchemaEventType = (AvroSchemaEventType) eventType;
                return avroHandler.GetEventBeanManufacturer(avroSchemaEventType, eventAdapterService, properties);
            }
            return null;
        }

        public static EventBean[] TypeCast(
            IList<EventBean> events,
            EventType targetType,
            EventAdapterService eventAdapterService)
        {
            var convertedArray = new EventBean[events.Count];
            var count = 0;
            foreach (var theEvent in events)
            {
                EventBean converted;
                if (theEvent is DecoratingEventBean)
                {
                    var wrapper = (DecoratingEventBean) theEvent;
                    if (targetType is MapEventType)
                    {
                        var props = new Dictionary<string, Object>();
                        props.PutAll(wrapper.DecoratingProperties);
                        foreach (var propDesc in wrapper.UnderlyingEvent.EventType.PropertyDescriptors)
                        {
                            props.Put(propDesc.PropertyName, wrapper.UnderlyingEvent.Get(propDesc.PropertyName));
                        }
                        converted = eventAdapterService.AdapterForTypedMap(props, targetType);
                    }
                    else
                    {
                        converted = eventAdapterService.AdapterForTypedWrapper(
                            wrapper.UnderlyingEvent, wrapper.DecoratingProperties, targetType);
                    }
                }
                else if ((theEvent.EventType is MapEventType) && (targetType is MapEventType))
                {
                    var mapEvent = (MappedEventBean) theEvent;
                    converted = eventAdapterService.AdapterForTypedMap(mapEvent.Properties, targetType);
                }
                else if ((theEvent.EventType is MapEventType) && (targetType is WrapperEventType))
                {
                    converted = eventAdapterService.AdapterForTypedWrapper(theEvent, Collections.EmptyDataMap, targetType);
                }
                else if ((theEvent.EventType is BeanEventType) && (targetType is BeanEventType))
                {
                    converted = eventAdapterService.AdapterForTypedObject(theEvent.Underlying, targetType);
                }
                else if (theEvent.EventType is ObjectArrayEventType && targetType is ObjectArrayEventType)
                {
                    var convertedObjectArray = ObjectArrayEventType.ConvertEvent(
                        theEvent, (ObjectArrayEventType) targetType);
                    converted = eventAdapterService.AdapterForTypedObjectArray(convertedObjectArray, targetType);
                }
                else if (theEvent.EventType is AvroSchemaEventType && targetType is AvroSchemaEventType)
                {
                    Object convertedGenericRecord = eventAdapterService.EventAdapterAvroHandler.ConvertEvent(
                        theEvent, (AvroSchemaEventType) targetType);
                    converted = eventAdapterService.AdapterForTypedAvro(convertedGenericRecord, targetType);
                }
                else
                {
                    throw new EPException("Unknown event type " + theEvent.EventType);
                }
                convertedArray[count] = converted;
                count++;
            }
            return convertedArray;
        }

        public static EventBeanSPI GetShellForType(EventType eventType)
        {
            if (eventType is BeanEventType)
            {
                return new BeanEventBean(null, eventType);
            }
            if (eventType is ObjectArrayEventType)
            {
                return new ObjectArrayEventBean(null, eventType);
            }
            if (eventType is MapEventType)
            {
                return new MapEventBean(null, eventType);
            }
            if (eventType is BaseXMLEventType)
            {
                return new XMLEventBean(null, eventType);
            }
            throw new EventAdapterException("Event type '" + eventType.Name + "' is not an engine-native event type");
        }

        public static EventBeanAdapterFactory GetAdapterFactoryForType(EventType eventType)
        {
            if (eventType is BeanEventType)
            {
                return new EventBeanAdapterFactoryBean(eventType);
            }
            if (eventType is ObjectArrayEventType)
            {
                return new EventBeanAdapterFactoryObjectArray(eventType);
            }
            if (eventType is MapEventType)
            {
                return new EventBeanAdapterFactoryMap(eventType);
            }
            if (eventType is BaseXMLEventType)
            {
                return new EventBeanAdapterFactoryXml(eventType);
            }
            throw new EventAdapterException("Event type '" + eventType.Name + "' is not an engine-native event type");
        }

        public class EventBeanAdapterFactoryBean : EventBeanAdapterFactory
        {
            private readonly EventType _eventType;

            public EventBeanAdapterFactoryBean(EventType eventType)
            {
                _eventType = eventType;
            }

            public EventBean MakeAdapter(Object underlying)
            {
                return new BeanEventBean(underlying, _eventType);
            }
        }

        public class EventBeanAdapterFactoryMap : EventBeanAdapterFactory
        {
            private readonly EventType _eventType;

            public EventBeanAdapterFactoryMap(EventType eventType)
            {
                _eventType = eventType;
            }

            public EventBean MakeAdapter(Object underlying)
            {
                return new MapEventBean((IDictionary<string, Object>) underlying, _eventType);
            }
        }

        public class EventBeanAdapterFactoryObjectArray : EventBeanAdapterFactory
        {
            private readonly EventType _eventType;

            public EventBeanAdapterFactoryObjectArray(EventType eventType)
            {
                _eventType = eventType;
            }

            public EventBean MakeAdapter(Object underlying)
            {
                return new ObjectArrayEventBean((Object[]) underlying, _eventType);
            }
        }

        public class EventBeanAdapterFactoryXml : EventBeanAdapterFactory
        {
            private readonly EventType _eventType;

            public EventBeanAdapterFactoryXml(EventType eventType)
            {
                _eventType = eventType;
            }

            public EventBean MakeAdapter(Object underlying)
            {
                return new XMLEventBean((XmlNode) underlying, _eventType);
            }
        }
    }
} // end of namespace
