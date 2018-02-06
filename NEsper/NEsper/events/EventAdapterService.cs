///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.collection;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.core.thread;
using com.espertech.esper.epl.core;
using com.espertech.esper.events.avro;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.xml;
using com.espertech.esper.plugin;
using com.espertech.esper.util;

using DataMap = System.Collections.Generic.IDictionary<string, object>;
using TypeMap = System.Collections.Generic.IDictionary<string, System.Type>;

namespace com.espertech.esper.events
{
    /// <summary>
    /// Interface for a service to resolve event names to event type.
    /// </summary>
    public interface EventAdapterService : EventBeanService
    {
        /// <summary>
        /// Returns descriptors for all writable properties.
        /// </summary>
        /// <param name="eventType">to reflect on</param>
        /// <param name="allowAnyType">if set to <c>true</c> [allow any type].</param>
        /// <returns>list of writable properties</returns>
        ICollection<WriteablePropertyDescriptor> GetWriteableProperties(EventType eventType, bool allowAnyType);

        /// <summary>
        /// Returns a factory for creating and populating event object instances for the
        /// given type.
        /// </summary>
        /// <param name="eventType">to create underlying objects for</param>
        /// <param name="properties">to write</param>
        /// <param name="engineImportService">The engine import service.</param>
        /// <param name="allowAnyType">if set to <c>true</c> [allow any type].</param>
        /// <returns>factory</returns>
        /// <throws>EventBeanManufactureException if a factory cannot be created for the type</throws>
        EventBeanManufacturer GetManufacturer(
            EventType eventType,
            IList<WriteablePropertyDescriptor> properties,
            EngineImportService engineImportService,
            bool allowAnyType);

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        /// <summary>
        /// Creates a thin adaper for an event object given an event type.
        /// </summary>
        /// <param name="bean">event object</param>
        /// <param name="eventType">event type</param>
        /// <returns>
        /// event
        /// </returns>
        EventBean AdapterForTypedObject(Object bean, EventType eventType);
#endif

        /// <summary>
        /// Adds an event type to the registery available for use, and originating outside
        /// as a non-adapter.
        /// </summary>
        /// <param name="name">to add an event type under</param>
        /// <param name="eventType">the type to add</param>
        /// <throws>EventAdapterException if the name is already in used by another type</throws>
        void AddTypeByName(String name, EventType eventType);

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        /// <summary>
        /// Return the event type for a given event name, or null if none is registered for
        /// that name.
        /// </summary>
        /// <param name="eventTypeName">is the event type name to return type for</param>
        /// <returns>
        /// event type for named event, or null if unknown/unnamed type
        /// </returns>
        EventType GetEventTypeByName(String eventTypeName);
#endif

        /// <summary>
        /// Return all known event types.
        /// </summary>
        /// <returns>
        /// event types
        /// </returns>
        ICollection<EventType> AllTypes { get; }

        /// <summary>
        /// Add an event type with the given name and a given set of properties, wherein properties may itself 
        /// be Maps, nested and strongly-typed.
        /// <para/> 
        /// If the name already exists with the same event property information, returns the existing EventType instance. 
        /// <para/> 
        /// If the name already exists with different event property information, throws an exception. 
        /// <para/> 
        /// If the name does not already exists, adds the name and constructs a new <seealso cref="MapEventType"/>.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="propertyTypes">is the names and types of event properties</param>
        /// <param name="optionalConfig">an optional set of Map event type names that are supertypes to the type</param>
        /// <param name="isPreconfiguredStatic">if set to <c>true</c> [is preconfigured static].</param>
        /// <param name="isPreconfigured">if set to <c>true</c> [is preconfigured].</param>
        /// <param name="isConfigured">if the type is application-configured</param>
        /// <param name="namedWindow">if the type is from a named window</param>
        /// <param name="insertInto">if inserting into a stream</param>
        /// <returns>event type is the type added</returns>
        /// <throws>EventAdapterException if name already exists and doesn't match property type info</throws>
        EventType AddNestableMapType(
            String eventTypeName,
            IDictionary<String, Object> propertyTypes,
            ConfigurationEventTypeMap optionalConfig,
            bool isPreconfiguredStatic,
            bool isPreconfigured,
            bool isConfigured,
            bool namedWindow,
            bool insertInto);

        /// <summary>
        /// Add an event type with the given name and the given underlying event type, as well as the additional given properties.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="underlyingEventType">is the event type for the event type that this wrapper wraps</param>
        /// <param name="propertyTypes">is the names and types of any additional properties</param>
        /// <param name="isNamedWindow">if the type is from a named window</param>
        /// <param name="isInsertInto">if inserting into a stream</param>
        /// <returns>eventType is the type added</returns>
        /// <throws>EventAdapterException if name already exists and doesn't match this type's info</throws>
        EventType AddWrapperType(
            String eventTypeName,
            EventType underlyingEventType,
            DataMap propertyTypes,
            bool isNamedWindow,
            bool isInsertInto);

        /// <summary>
        /// Creates a new anonymous EventType instance for an event type that contains a map
        /// of name value pairs. The method accepts a Map that contains the property names
        /// as keys and Class objects as the values. The Class instances represent the
        /// property types.
        /// <para />
        /// New instances are created Statement by this method on every invocation. Clients
        /// to this method need to cache the returned EventType instance to reuse EventType's
        /// for same-typed events.
        /// <para />
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="propertyTypes">is a map of String to Class objects</param>
        /// <param name="isTransient">isTransient transient types are not available by event type id lookup and recovery, they are always re-created on-the-fly</param>
        /// <returns>
        /// EventType implementation for map field names and value types
        /// </returns>
        EventType CreateAnonymousMapType(string typeName, DataMap propertyTypes, bool isTransient);

        /// <summary>
        /// Creata a wrapper around an event and some additional properties
        /// </summary>
        /// <param name="theEvent">is the wrapped event</param>
        /// <param name="properties">are the additional properties</param>
        /// <param name="eventType">os the type metadata for any wrappers of this type</param>
        /// <returns>wrapper event bean</returns>
        EventBean AdapterForTypedWrapper(
            EventBean theEvent,
            IDictionary<String, Object> properties,
            EventType eventType);

        /// <summary>
        /// Add an event type with the given name and fully-qualified class name.
        /// <para/> If the name already exists with the same class name, returns the existing EventType instance.
        /// <para/> If the name already exists with different class name, throws an exception.
        /// <para/> If the name does not already exists, adds the name and constructs a new <seealso cref="BeanEventType"/>. 
        /// <para/> Takes into account all event-type-auto-package names supplied and attempts to resolve the class name via the packages if the direct resolution failed.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="fullyQualClassName">is the fully qualified class name</param>
        /// <param name="considerAutoName">whether auto-name by namespaces should be considered</param>
        /// <param name="isPreconfiguredStatic">if set to <c>true</c> [is preconfigured static].</param>
        /// <param name="isPreconfigured">if set to <c>true</c> [is preconfigured].</param>
        /// <param name="isConfigured">if set to <c>true</c> [is configured].</param>
        /// <returns>event type is the type added</returns>
        /// <throws>EventAdapterException if name already exists and doesn't match class names</throws>
        EventType AddBeanType(
            String eventTypeName,
            String fullyQualClassName,
            bool considerAutoName,
            bool isPreconfiguredStatic,
            bool isPreconfigured,
            bool isConfigured);

        /// <summary>
        /// Add an event type with the given name and class.
        /// <para/> If the name already exists with the same Class, returns the existing EventType instance.
        /// <para/> If the name already exists with different Class name, throws an exception.
        /// <para/> If the name does not already exists, adds the name and constructs a new <seealso cref="BeanEventType"/>.
        /// </summary>
        /// <param name="eventTypeName">is the name for the event type</param>
        /// <param name="clazz">is the fully class</param>
        /// <param name="isPreconfiguredStatic">if set to <c>true</c> [is preconfigured static].</param>
        /// <param name="isPreconfigured">if set to <c>true</c> [is preconfigured].</param>
        /// <param name="isConfigured">if the class is application-configured</param>
        /// <returns>event type is the type added</returns>
        /// <throws>EventAdapterException if name already exists and doesn't match class names</throws>
        EventType AddBeanType(
            String eventTypeName,
            Type clazz,
            bool isPreconfiguredStatic,
            bool isPreconfigured,
            bool isConfigured);

        EventType AddBeanTypeByName(String eventTypeName, Type clazz, bool isNamedWindow);

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        /// <summary>
        /// Wrap the native event returning an <seealso cref="EventBean"/>.
        /// </summary>
        /// <param name="theEvent">to be wrapped</param>
        /// <returns>
        /// event bean wrapping native underlying event
        /// </returns>
        EventBean AdapterForObject(Object theEvent);
#endif

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        /// <summary>
        /// Wrap the Map-type event returning an <seealso cref="EventBean"/> using the event type name to identify the EventType that the event should carry.
        /// </summary>
        /// <param name="theEvent">to be wrapped</param>
        /// <param name="eventTypeName">name for the event type of the event</param>
        /// <returns>
        /// event bean wrapping native underlying event
        /// </returns>
        /// <throws>EventAdapterException if the name has not been declared, or the event cannot be wrapped using thatname's event type </throws>
        EventBean AdapterForMap(DataMap theEvent, String eventTypeName);
#endif

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        EventBean AdapterForObjectArray(Object[] theEvent, String eventTypeName);
#endif

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        /// <summary>
        /// Create an event map bean from a set of event properties (name and value
        /// objectes) stored in a Map.
        /// </summary>
        /// <param name="properties">is key-value pairs for the event properties</param>
        /// <param name="eventType">is the type metadata for any maps of that type</param>
        /// <returns>EventBean instance</returns>
        EventBean AdapterForTypedMap(DataMap properties, EventType eventType);
#endif

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        /// <summary>
        /// Returns an adapter for the LINQ element that exposes it's data as event
        /// properties for use in statements.
        /// </summary>
        /// <param name="element">is the element to wrap</param>
        /// <returns>
        /// event wrapper for document
        /// </returns>
        EventBean AdapterForDOM(XElement element);
#endif

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        /// <summary>
        /// Returns an adapter for the XML DOM document that exposes it's data as event
        /// properties for use in statements.
        /// </summary>
        /// <param name="node">is the node to wrap</param>
        /// <returns>event wrapper for document</returns>
        EventBean AdapterForDOM(XmlNode node);
#endif

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        /// <summary>
        /// Returns an adapter for the XML DOM document that exposes it's data as event
        /// properties for use in statements.
        /// </summary>
        /// <param name="node">is the node to wrap</param>
        /// <param name="eventType">the event type associated with the node</param>
        /// <returns>event wrapper for document</returns>
        EventBean AdapterForTypedDOM(XmlNode node, EventType eventType);
#endif

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        /// <summary>
        /// Returns an adapter for the XML LINQ object that exposes it's data as event
        /// properties for use in statements.
        /// </summary>
        /// <param name="node">is the node to wrap</param>
        /// <param name="eventType">the event type associated with the node</param>
        /// <returns>
        /// event wrapper for document
        /// </returns>
        EventBean AdapterForTypedDOM(XObject node, EventType eventType);
#endif

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        /// <summary>
        /// Returns an adapter for an event underlying object when the event type is known.
        /// </summary>
        /// <param name="theEvent">underlying</param>
        /// <param name="eventType">type</param>
        /// <returns>event wrapper for object</returns>
        EventBean AdapterForType(Object theEvent, EventType eventType);
#endif

        /// <summary>
        /// Create a new anonymous event type with the given underlying event type, as well as the additional given properties.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="underlyingEventType">is the event type for the event type that this wrapper wraps</param>
        /// <param name="propertyTypes">is the names and types of any additional properties</param>
        /// <returns>eventType is the type createdStatement</returns>
        /// <throws>EventAdapterException if name already exists and doesn't match this type's info</throws>
        EventType CreateAnonymousWrapperType(String typeName, EventType underlyingEventType, DataMap propertyTypes);

        /// <summary>
        /// Adds an XML DOM event type.
        /// </summary>
        /// <param name="eventTypeName">is the name to add the type for</param>
        /// <param name="configurationEventTypeXMLDOM">is the XML DOM config info</param>
        /// <param name="optionalSchemaModel">is the object model of the schema, or null in none provided</param>
        /// <param name="isPreconfiguredStatic">if set to <c>true</c> [is preconfigured static].</param>
        /// <returns>event type</returns>
        EventType AddXMLDOMType(
            String eventTypeName,
            ConfigurationEventTypeXMLDOM configurationEventTypeXMLDOM,
            SchemaModel optionalSchemaModel,
            bool isPreconfiguredStatic);

        /// <summary>
        /// Gets or sets the configured legacy class information.
        /// </summary>
        IDictionary<string, ConfigurationEventTypeLegacy> TypeLegacyConfigs { get; set; }

        /// <summary>
        /// Returns the configured legacy class information or null if none defined.
        /// </summary>
        /// <param name="className">is the fully-qualified class name</param>
        /// <returns></returns>
        ConfigurationEventTypeLegacy GetTypeLegacyConfigs(String className);

        /// <summary>
        /// Gets or sets the resolution style for case-sentitivity.
        /// </summary>
        /// <value>for resolving properties.</value>
        PropertyResolutionStyle DefaultPropertyResolutionStyle { get; set; }

        /// <summary>
        /// Adds a namespace within which event types reside.
        /// </summary>
        /// <param name="namespace">is the namespace within which event types reside</param>
        void AddAutoNamePackage(String @namespace);

        /// <summary>
        /// Returns a subset of the functionality of the service specific to creating event types.
        /// </summary>
        /// <value>bean event type factory</value>
        BeanEventTypeFactory BeanEventTypeFactory { get; }

        /// <summary>
        /// Add a plug-in event representation.
        /// </summary>
        /// <param name="eventRepURI">URI is the unique identifier for the event representation</param>
        /// <param name="pluginEventRep">is the instance</param>
        void AddEventRepresentation(Uri eventRepURI, PlugInEventRepresentation pluginEventRep);

        /// <summary>
        /// Adds a plug-in event type.
        /// </summary>
        /// <param name="name">is the name of the event type</param>
        /// <param name="resolutionURIs">is the URIs of plug-in event representations, or child URIs of such</param>
        /// <param name="initializer">is configs for the type</param>
        /// <returns>type</returns>
        EventType AddPlugInEventType(string name, IList<Uri> resolutionURIs, object initializer);

        /// <summary>
        /// Returns an event sender for a specific type, only generating events of that type.
        /// </summary>
        /// <param name="runtimeEventSender">the runtime handle for sending the wrapped type</param>
        /// <param name="eventTypeName">is the name of the event type to return the sender for</param>
        /// <param name="threadingService">threading service</param>
        /// <param name="lockManager">The lock manager.</param>
        EventSender GetStaticTypeEventSender(
            EPRuntimeEventSender runtimeEventSender,
            String eventTypeName,
            ThreadingService threadingService,
            ILockManager lockManager);

        /// <summary>
        /// Returns an event sender that dynamically decides what the event type for a given object is.
        /// </summary>
        /// <param name="runtimeEventSender">the runtime handle for sending the wrapped type</param>
        /// <param name="uri">is for plug-in event representations to provide implementations, if accepted, to make a wrapped event</param>
        /// <param name="threadingService">threading service</param>
        /// <returns>
        /// event sender that is dynamic, multi-type based on multiple event bean factories provided byplug-in event representations
        /// </returns>
        EventSender GetDynamicTypeEventSender(
            EPRuntimeEventSender runtimeEventSender,
            Uri[] uri,
            ThreadingService threadingService);

        /// <summary>
        /// Update a given Map  event type.
        /// </summary>
        /// <param name="mapEventTypeName">name to Update</param>
        /// <param name="typeMap">additional properties to add, nesting allowed</param>
        /// <throws>EventAdapterException when the type is not found or is not a IDictionary</throws>
        void UpdateMapEventType(String mapEventTypeName, DataMap typeMap);

        /// <summary>Casts event type of a list of events to either Wrapper or Map type. </summary>
        /// <param name="events">to cast</param>
        /// <param name="targetType">target type</param>
        /// <returns>type casted event array</returns>
        EventBean[] TypeCast(IList<EventBean> events, EventType targetType);

        /// <summary>
        /// Removes an event type by a given name indicating by the return value whether the type was found or not. 
        /// <para/>
        /// Does not uncache an existing class loaded by a JVM. Does remove XML root element names. Does not handle
        /// value-add event types.
        /// </summary>
        /// <param name="eventTypeName">to remove</param>
        /// <returns>
        /// true if found and removed, false if not found
        /// </returns>
        bool RemoveType(String eventTypeName);

        /// <summary>
        /// Creates an anonymous map that has no name, however in a fail-over scenario events of this type may be recoverable 
        /// and therefore the type is only semi-anonymous, identified by the tags and event type names used.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="taggedEventTypes">simple type per property name</param>
        /// <param name="arrayEventTypes">array type per property name</param>
        /// <param name="isUsedByChildViews">if the type is going to be in used by child views</param>
        /// <returns>event type</returns>
        EventType CreateSemiAnonymousMapType(
            String typeName,
            IDictionary<String, Pair<EventType, String>> taggedEventTypes,
            IDictionary<String, Pair<EventType, String>> arrayEventTypes,
            bool isUsedByChildViews);

        EventType ReplaceXMLEventType(
            String xmlEventTypeName,
            ConfigurationEventTypeXMLDOM config,
            SchemaModel schemaModel);

        AccessorStyleEnum DefaultAccessorStyle { set; }

        IDictionary<string, EventType> DeclaredEventTypes { get; }

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        EventBean AdapterForTypedObjectArray(Object[] props, EventType resultEventType);
#endif

        EventType CreateAnonymousObjectArrayType(String typeName, IDictionary<String, Object> propertyTypes);
        EventType CreateAnonymousAvroType(String typeName, IDictionary<String, Object> properties, Attribute[] annotations, String statementName, String engineURI);

        EventType AddNestableObjectArrayType(
            String eventTypeName,
            IDictionary<String, Object> propertyTypes,
            ConfigurationEventTypeObjectArray typeConfig,
            bool isPreconfiguredStatic,
            bool isPreconfigured,
            bool isConfigured,
            bool namedWindow,
            bool insertInto,
            bool table,
            string tableName);

        void UpdateObjectArrayEventType(String objectArrayEventTypeName, IDictionary<String, Object> typeMap);
        EventBeanSPI GetShellForType(EventType eventType);
        EventBeanAdapterFactory GetAdapterFactoryForType(EventType eventType);
        EventType CreateAnonymousBeanType(String schemaName, Type clazz);

        EventType AddAvroType(
            String eventTypeName,
            ConfigurationEventTypeAvro avro,
            bool isPreconfiguredStatic,
            bool isPreconfigured,
            bool isConfigured,
            bool isNamedWindow,
            bool isInsertInto);

        EventType AddAvroType(
            String eventTypeName,
            IDictionary<String, Object> types,
            bool isPreconfiguredStatic,
            bool isPreconfigured,
            bool isConfigured,
            bool isNamedWindow,
            bool isInsertInto,
            Attribute[] annotations,
            ConfigurationEventTypeAvro config,
            String statementName,
            String engineURI);

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        EventBean AdapterForAvro(Object avroGenericDataDotRecord, String eventTypeName);
#endif

        EventAdapterAvroHandler EventAdapterAvroHandler { get; }

#if DUPLICATE_IN_EVENT_BEAN_SERVICE
        EventBean AdapterForTypedAvro(Object avroGenericDataDotRecord, EventType eventType);
#endif

        TypeWidenerCustomizer GetTypeWidenerCustomizer(EventType resultEventType);

        EngineImportService EngineImportService { get; }
    }

    public class EventAdapterServiceConstants
    {
        public readonly static String ANONYMOUS_TYPE_NAME_PREFIX = "anonymous_";
    }
}
