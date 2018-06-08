///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.core.thread;
using com.espertech.esper.epl.core;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.avro;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.xml;
using com.espertech.esper.plugin;
using com.espertech.esper.util;

namespace com.espertech.esper.events
{
    using DataMap = IDictionary<string, object>;
    using TypeMap = IDictionary<string, Type>;

    /// <summary>
    /// Implementation for resolving event name to event type.
    /// <para/>
    /// The implementation assigned a unique identifier to each event type. For Class-based event types, only 
    /// one EventType instance and one event type id exists for the same class.
    /// <para/>
    /// Event type names must be unique, that is an name must resolve to a single event type.
    /// <para/>
    /// Each event type can have multiple names defined for it. For example, expressions such as "select * from A"
    /// and "select * from B" in which A and B are names for the same class X the select clauses each fireStatementStopped
    /// for events of type X. In summary, names A and B point to the same underlying event type and therefore event type id.
    /// </summary>
    public class EventAdapterServiceImpl : EventAdapterService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BeanEventAdapter _beanEventAdapter;

        private readonly ICache<Type, BeanEventType> _beanEventTypeCache =
            new Cache3D<Type, BeanEventType>();

        private readonly EventTypeIdGenerator _eventTypeIdGenerator;
        private readonly EventAdapterServiceAnonymousTypeCache _anonymousTypeCache;

        private readonly EventAdapterAvroHandler _avroHandler;
        private readonly EngineImportService _engineImportService;

        private readonly IDictionary<String, PlugInEventTypeHandler> _nameToHandlerMap;
        private readonly IDictionary<String, EventType> _nameToTypeMap;

        private readonly ICollection<String> _namespaces;
        private readonly IDictionary<Uri, PlugInEventRepresentation> _plugInRepresentations;

        private readonly ILockable _syncLock;
        private readonly IDictionary<Type, BeanEventType> _typesPerBean;
        private readonly IDictionary<String, EventType> _xmldomRootElementNames;
        private readonly IDictionary<String, EventType> _xelementRootElementNames;

        private readonly IContainer _container;

        /// <summary>Ctor. </summary>
        public EventAdapterServiceImpl(
            IContainer container,
            EventTypeIdGenerator eventTypeIdGenerator,
            int anonymousTypeCacheSize,
            EventAdapterAvroHandler avroHandler,
            EngineImportService engineImportService)
        {
            _eventTypeIdGenerator = eventTypeIdGenerator;
            _avroHandler = avroHandler;
            _engineImportService = engineImportService;
            _container = container;

            _syncLock = _container.LockManager().CreateLock(GetType());

            _nameToTypeMap = new Dictionary<String, EventType>();
            _xmldomRootElementNames = new Dictionary<String, EventType>();
            _xelementRootElementNames = new Dictionary<String, EventType>();
            _namespaces = new LinkedHashSet<String>();
            _nameToHandlerMap = new Dictionary<String, PlugInEventTypeHandler>();

            // Share the mapping of class to type with the type creation for thread safety
            _typesPerBean = new ConcurrentDictionary<Type, BeanEventType>();

            _beanEventAdapter = new BeanEventAdapter(
                _container, _typesPerBean, this, eventTypeIdGenerator);
            _plugInRepresentations = new Dictionary<Uri, PlugInEventRepresentation>();
            _anonymousTypeCache = new EventAdapterServiceAnonymousTypeCache(anonymousTypeCacheSize);
        }

        /// <summary>Sets the default property resolution style. </summary>
        /// <value>is the default style</value>
        public PropertyResolutionStyle DefaultPropertyResolutionStyle
        {
            get => _beanEventAdapter.DefaultPropertyResolutionStyle;
            set => _beanEventAdapter.DefaultPropertyResolutionStyle = value;
        }

        public AccessorStyleEnum DefaultAccessorStyle
        {
            set => _beanEventAdapter.DefaultAccessorStyle = value;
        }

        public IDictionary<string, EventType> DeclaredEventTypes => new Dictionary<String, EventType>(_nameToTypeMap);

        public EngineImportService EngineImportService => _engineImportService;

        public ICollection<WriteablePropertyDescriptor> GetWriteableProperties(EventType eventType, bool allowAnyType)
        {
            return EventAdapterServiceHelper.GetWriteableProperties(eventType, allowAnyType);
        }

        public EventBeanManufacturer GetManufacturer(
            EventType eventType,
            IList<WriteablePropertyDescriptor> properties,
            EngineImportService engineImportService,
            bool allowAnyType)
        {
            return EventAdapterServiceHelper.GetManufacturer(
                this, eventType, properties, engineImportService, allowAnyType, _avroHandler);
        }

        public void AddTypeByName(
            String name,
            EventType eventType)
        {
            using (_syncLock.Acquire())
            {
                if (_nameToTypeMap.ContainsKey(name))
                {
                    throw new EventAdapterException("Event type by name '" + name + "' already exists");
                }
                _nameToTypeMap.Put(name, eventType);
            }
        }

        public void AddEventRepresentation(Uri eventRepURI, PlugInEventRepresentation pluginEventRep)
        {
            if (_plugInRepresentations.ContainsKey(eventRepURI))
            {
                throw new EventAdapterException(
                    "Plug-in event representation URI by name " + eventRepURI +
                    " already exists");
            }
            _plugInRepresentations.Put(eventRepURI, pluginEventRep);
        }

        public EventType AddPlugInEventType(string eventTypeName, IList<Uri> resolutionURIs, object initializer)
        {
            if (_nameToTypeMap.ContainsKey(eventTypeName))
            {
                throw new EventAdapterException(
                    "Event type named '" + eventTypeName +
                    "' has already been declared");
            }

            PlugInEventRepresentation handlingFactory = null;
            Uri handledEventTypeURI = null;

            if ((resolutionURIs == null) || (resolutionURIs.Count == 0))
            {
                throw new EventAdapterException(
                    "Event type named '" + eventTypeName + "' could not be created as" +
                    " no resolution URIs for dynamic resolution of event type names through a plug-in event representation have been defined");
            }

            foreach (var eventTypeURI in resolutionURIs)
            {
                // Determine a list of event representations that may handle this type
                var allFactories = new Dictionary<Uri, PlugInEventRepresentation>(_plugInRepresentations);
                var factories = URIUtil.FilterSort(eventTypeURI, allFactories);

                if (factories.IsEmpty())
                {
                    continue;
                }

                // Ask each in turn to accept the type (the process of resolving the type)
                foreach (var entry in factories)
                {
                    var factory = entry.Value;
                    var context = new PlugInEventTypeHandlerContext(
                        eventTypeURI, initializer, eventTypeName, _eventTypeIdGenerator.GetTypeId(eventTypeName));
                    if (factory.AcceptsType(context))
                    {
                        handlingFactory = factory;
                        handledEventTypeURI = eventTypeURI;
                        break;
                    }
                }

                if (handlingFactory != null)
                {
                    break;
                }
            }

            if (handlingFactory == null)
            {
                throw new EventAdapterException(
                    "Event type named '" + eventTypeName +
                    "' could not be created as none of the " +
                    "registered plug-in event representations accepts any of the resolution URIs '" +
                    resolutionURIs.Render() +
                    "' and initializer");
            }

            var typeContext = new PlugInEventTypeHandlerContext(
                handledEventTypeURI,
                initializer,
                eventTypeName,
                _eventTypeIdGenerator.GetTypeId(eventTypeName));
            var handler = handlingFactory.GetTypeHandler(typeContext);
            if (handler == null)
            {
                throw new EventAdapterException(
                    "Event type named '" + eventTypeName +
                    "' could not be created as no handler was returned");
            }

            var eventType = handler.EventType;
            _nameToTypeMap.Put(eventTypeName, eventType);
            _nameToHandlerMap.Put(eventTypeName, handler);

            return eventType;
        }

        public EventSender GetStaticTypeEventSender(
            EPRuntimeEventSender runtimeEventSender,
            String eventTypeName,
            ThreadingService threadingService,
            ILockManager lockManager)
        {
            var eventType = _nameToTypeMap.Get(eventTypeName);
            if (eventType == null)
            {
                throw new EventTypeException("Event type named '" + eventTypeName + "' could not be found");
            }

            // handle built-in types
            if (eventType is BeanEventType)
            {
                return new EventSenderBean(
                    runtimeEventSender, (BeanEventType) eventType, this, threadingService, lockManager);
            }
            else if (eventType is MapEventType)
            {
                return new EventSenderMap(runtimeEventSender, (MapEventType) eventType, this, threadingService);
            }
            else if (eventType is ObjectArrayEventType)
            {
                return new EventSenderObjectArray(
                    runtimeEventSender, (ObjectArrayEventType) eventType, this, threadingService);
            }
            else if (eventType is BaseXMLEventType)
            {
                return new EventSenderXMLDOM(runtimeEventSender, (BaseXMLEventType) eventType, this, threadingService);
            }
            else if (eventType is AvroSchemaEventType)
            {
                return new EventSenderAvro(runtimeEventSender, eventType, this, threadingService);
            }

            var handlers = _nameToHandlerMap.Get(eventTypeName);
            if (handlers != null)
            {
                return handlers.GetSender(runtimeEventSender);
            }
            throw new EventTypeException(
                "An event sender for event type named '" + eventTypeName +
                "' could not be created as the type is internal");
        }

        public void UpdateMapEventType(
            String mapeventTypeName,
            IDictionary<String, Object> typeMap)
        {
            var type = _nameToTypeMap.Get(mapeventTypeName);
            if (type == null)
            {
                throw new EventAdapterException("Event type named '" + mapeventTypeName + "' has not been declared");
            }
            if (!(type is MapEventType))
            {
                throw new EventAdapterException("Event type by name '" + mapeventTypeName + "' is not a Map event type");
            }

            var mapEventType = (MapEventType) type;
            mapEventType.AddAdditionalProperties(typeMap, this);
        }

        public void UpdateObjectArrayEventType(String objectArrayEventTypeName, DataMap typeMap)
        {
            var type = _nameToTypeMap.Get(objectArrayEventTypeName);
            if (type == null)
            {
                throw new EventAdapterException(
                    "Event type named '" + objectArrayEventTypeName + "' has not been declared");
            }

            var objectArrayEventType = type as ObjectArrayEventType;
            if (objectArrayEventType == null)
            {
                throw new EventAdapterException(
                    "Event type by name '" + objectArrayEventTypeName + "' is not an Object-array event type");
            }

            objectArrayEventType.AddAdditionalProperties(typeMap, this);
        }

        public EventSender GetDynamicTypeEventSender(
            EPRuntimeEventSender epRuntime,
            Uri[] uri,
            ThreadingService threadingService)
        {
            var handlingFactories = new List<EventSenderURIDesc>();
            foreach (var resolutionURI in uri)
            {
                // Determine a list of event representations that may handle this type
                var allFactories = new Dictionary<Uri, PlugInEventRepresentation>(_plugInRepresentations);
                var factories = URIUtil.FilterSort(resolutionURI, allFactories);

                if (factories.IsEmpty())
                {
                    continue;
                }

                // Ask each in turn to accept the type (the process of resolving the type)
                foreach (var entry in factories)
                {
                    var factory = entry.Value;
                    var context = new PlugInEventBeanReflectorContext(resolutionURI);
                    if (factory.AcceptsEventBeanResolution(context))
                    {
                        var beanFactory = factory.GetEventBeanFactory(context);
                        if (beanFactory == null)
                        {
                            Log.Warn("Plug-in event representation returned a null bean factory, ignoring entry");
                            continue;
                        }
                        var desc = new EventSenderURIDesc(beanFactory, resolutionURI, entry.Key);
                        handlingFactories.Add(desc);
                    }
                }
            }

            if (handlingFactories.IsEmpty())
            {
                throw new EventTypeException(
                    "Event sender for resolution URIs '" + uri.Render() +
                    "' did not return at least one event representation's event factory");
            }

            return new EventSenderImpl(handlingFactories, epRuntime, threadingService);
        }

        public BeanEventTypeFactory BeanEventTypeFactory => _beanEventAdapter;

        public EventType AddBeanType(
            String eventTypeName,
            Type clazz,
            bool isPreconfiguredStatic,
            bool isPreconfigured,
            bool isConfigured)
        {
            using (_syncLock.Acquire())
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".addBeanType Adding " + eventTypeName + " for type " + clazz.FullName);
                }

                var existingType = _nameToTypeMap.Get(eventTypeName);
                if (existingType != null)
                {
                    if (existingType.UnderlyingType.Equals(clazz))
                    {
                        return existingType;
                    }

                    throw new EventAdapterException(
                        "Event type named '" + eventTypeName +
                        "' has already been declared with differing underlying type information:" +
                        existingType.UnderlyingType.Name +
                        " versus " + clazz.Name);
                }

                EventType eventType = _beanEventAdapter.CreateBeanType(
                    eventTypeName, clazz, isPreconfiguredStatic,
                    isPreconfigured, isConfigured);
                _nameToTypeMap.Put(eventTypeName, eventType);

                return eventType;
            }
        }

        public EventType AddBeanTypeByName(
            String eventTypeName,
            Type clazz,
            bool isNamedWindow)
        {
            using (_syncLock.Acquire())
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".addBeanTypeNamedWindow Adding " + eventTypeName + " for type " + clazz.FullName);
                }

                var existingType = _nameToTypeMap.Get(eventTypeName);
                TypeClass typeClass;
                if (existingType != null)
                {
                    if (existingType is BeanEventType &&
                        existingType.UnderlyingType == clazz &&
                        existingType.Name.Equals(eventTypeName))
                    {
                        typeClass = ((BeanEventType) existingType).Metadata.TypeClass;
                        if (isNamedWindow)
                        {
                            if (typeClass == TypeClass.NAMED_WINDOW)
                            {
                                return existingType;
                            }
                        }
                        else
                        {
                            if (typeClass == TypeClass.STREAM)
                            {
                                return existingType;
                            }
                        }
                    }
                    throw new EventAdapterException(
                        "An event type named '" + eventTypeName +
                        "' has already been declared");
                }

                typeClass = isNamedWindow
                    ? TypeClass.NAMED_WINDOW
                    : TypeClass.STREAM;
                var beanEventType =
                    new BeanEventType(
                        _container,
                        EventTypeMetadata.CreateBeanType(eventTypeName, clazz, false, false, false, typeClass),
                        _eventTypeIdGenerator.GetTypeId(eventTypeName), clazz, this,
                        _beanEventAdapter.GetClassToLegacyConfigs(clazz.AssemblyQualifiedName));
                _nameToTypeMap.Put(eventTypeName, beanEventType);

                return beanEventType;
            }
        }

        /// <summary>Create an event bean given an event of object id. </summary>
        /// <param name="theEvent">is the event class</param>
        /// <returns>event</returns>
        public EventBean AdapterForObject(Object theEvent)
        {
            var type = theEvent.GetType();
            BeanEventType eventType;
            if (!_beanEventTypeCache.TryGet(type, out eventType))
            {
                eventType = _beanEventTypeCache.Put(
                    type,
                    _typesPerBean.Get(type) ??
                    _beanEventAdapter.CreateBeanType(type.FullName, type, false, false, false));
            }

            return new BeanEventBean(theEvent, eventType);
        }

        /// <summary>
        /// Add an event type for the given type name.
        /// </summary>
        /// <param name="eventTypeName">is the name</param>
        /// <param name="fullyQualClassName">is the type name</param>
        /// <param name="considerAutoName">whether auto-name by namespaces should be considered</param>
        /// <param name="isPreconfiguredStatic">if set to <c>true</c> [is preconfigured static].</param>
        /// <param name="isPreconfigured">if set to <c>true</c> [is preconfigured].</param>
        /// <param name="isConfigured">if set to <c>true</c> [is configured].</param>
        /// <returns>event type</returns>
        /// <throws>EventAdapterException if the Class name cannot resolve or other error occured</throws>
        public EventType AddBeanType(
            String eventTypeName,
            String fullyQualClassName,
            bool considerAutoName,
            bool isPreconfiguredStatic,
            bool isPreconfigured,
            bool isConfigured)
        {
            using (_syncLock.Acquire())
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(".addBeanType Adding " + eventTypeName + " for type " + fullyQualClassName);
                }

                var existingType = _nameToTypeMap.Get(eventTypeName);
                if (existingType != null)
                {
                    if ((fullyQualClassName == existingType.UnderlyingType.AssemblyQualifiedName) ||
                        (fullyQualClassName == existingType.UnderlyingType.FullName) ||
                        (fullyQualClassName == existingType.UnderlyingType.Name))
                    {
                        if (Log.IsDebugEnabled)
                        {
                            Log.Debug(".addBeanType Returning existing type for " + eventTypeName);
                        }
                        return existingType;
                    }

                    throw new EventAdapterException(
                        "Event type named '" + eventTypeName +
                        "' has already been declared with differing underlying type information: Class " +
                        existingType.UnderlyingType.FullName +
                        " versus " + fullyQualClassName);
                }

                // Try to resolve as a fully-qualified class name first
                Type clazz = null;
                try
                {
                    clazz = _engineImportService.GetClassForNameProvider().ClassForName(fullyQualClassName);
                }
                catch (TypeLoadException ex)
                {
                    if (!considerAutoName)
                    {
                        throw new EventAdapterException(
                            "Event type or class named '" + fullyQualClassName + "' was not found", ex);
                    }

                    // Attempt to resolve from auto-name packages
                    foreach (var @namespace in _namespaces)
                    {
                        var generatedClassName = @namespace + "." + fullyQualClassName;
                        try
                        {
                            var resolvedClass = _engineImportService.GetClassForNameProvider().ClassForName(generatedClassName);
                            if (clazz != null)
                            {
                                throw new EventAdapterException(
                                    "Failed to resolve name '" + eventTypeName +
                                    "', the class was ambigously found both in " +
                                    "namespace '" + clazz.Namespace + "' and in " +
                                    "namespace '" + resolvedClass.Namespace + "'", ex);
                            }
                            clazz = resolvedClass;
                        }
                        catch (TypeLoadException)
                        {
                            // expected, class may not exists in all packages
                        }
                    }
                    if (clazz == null)
                    {
                        throw new EventAdapterException(
                            "Event type or class named '" + fullyQualClassName + "' was not found", ex);
                    }
                }

                EventType eventType = _beanEventAdapter.CreateBeanType(
                    eventTypeName, clazz, isPreconfiguredStatic,
                    isPreconfigured, isConfigured);
                _nameToTypeMap.Put(eventTypeName, eventType);

                return eventType;
            }
        }

        public EventType AddNestableMapType(
            String eventTypeName,
            IDictionary<String, Object> propertyTypes,
            ConfigurationEventTypeMap optionalConfig,
            bool isPreconfiguredStatic,
            bool isPreconfigured,
            bool isConfigured,
            bool namedWindow,
            bool insertInto)
        {
            using (_syncLock.Acquire())
            {
                var mapSuperTypes = EventTypeUtility.GetSuperTypesDepthFirst(
                    optionalConfig, EventUnderlyingType.MAP, _nameToTypeMap);
                var metadata = EventTypeMetadata.CreateNonPonoApplicationType(
                    ApplicationType.MAP,
                    eventTypeName, isPreconfiguredStatic,
                    isPreconfigured, isConfigured,
                    namedWindow, insertInto);

                var typeId = _eventTypeIdGenerator.GetTypeId(eventTypeName);
                var newEventType = new MapEventType(
                    metadata, eventTypeName, typeId, this, propertyTypes,
                    mapSuperTypes.First, mapSuperTypes.Second, optionalConfig);

                var existingType = _nameToTypeMap.Get(eventTypeName);
                if (existingType != null)
                {
                    // The existing type must be the same as the type createdStatement
                    if (!newEventType.EqualsCompareType(existingType))
                    {
                        var message = newEventType.GetEqualsMessage(existingType);
                        throw new EventAdapterException(
                            "Event type named '" + eventTypeName +
                            "' has already been declared with differing column name or type information: " +
                            message);
                    }

                    // Since it's the same, return the existing type
                    return existingType;
                }

                _nameToTypeMap.Put(eventTypeName, newEventType);

                return newEventType;
            }
        }

        public EventType AddNestableObjectArrayType(
            String eventTypeName,
            IDictionary<String, Object> propertyTypes,
            ConfigurationEventTypeObjectArray optionalConfig,
            bool isPreconfiguredStatic,
            bool isPreconfigured,
            bool isConfigured,
            bool namedWindow,
            bool insertInto,
            bool table,
            string tableName)
        {
            using (_syncLock.Acquire())
            {
                if (optionalConfig != null && optionalConfig.SuperTypes.Count > 1)
                {
                    throw new EventAdapterException(ConfigurationEventTypeObjectArray.SINGLE_SUPERTYPE_MSG);
                }

                var mapSuperTypes = EventTypeUtility.GetSuperTypesDepthFirst(
                    optionalConfig, EventUnderlyingType.OBJECTARRAY, _nameToTypeMap);

                EventTypeMetadata metadata;
                if (table)
                {
                    metadata = EventTypeMetadata.CreateTable(tableName);
                }
                else
                {
                    metadata = EventTypeMetadata.CreateNonPonoApplicationType(
                        ApplicationType.OBJECTARR,
                        eventTypeName,
                        isPreconfiguredStatic,
                        isPreconfigured,
                        isConfigured,
                        namedWindow,
                        insertInto);
                }


                var typeId = _eventTypeIdGenerator.GetTypeId(eventTypeName);
                var newEventType = new ObjectArrayEventType(
                    metadata, eventTypeName, typeId, this,
                    propertyTypes, optionalConfig,
                    mapSuperTypes.First,
                    mapSuperTypes.Second);

                var existingType = _nameToTypeMap.Get(eventTypeName);
                if (existingType != null)
                {
                    // The existing type must be the same as the type createdStatement
                    if (!newEventType.EqualsCompareType(existingType))
                    {
                        var message = newEventType.GetEqualsMessage(existingType);
                        throw new EventAdapterException(
                            "Event type named '" + eventTypeName +
                            "' has already been declared with differing column name or type information: " +
                            message);
                    }

                    // Since it's the same, return the existing type
                    return existingType;
                }

                _nameToTypeMap.Put(eventTypeName, newEventType);

                return newEventType;
            }
        }

        public EventBean AdapterForMap(IDictionary<String, Object> theEvent, String eventTypeName)
        {
            var existingType = _nameToTypeMap.Get(eventTypeName);
            if (!(existingType is MapEventType))
            {
                throw new EPException(EventAdapterServiceHelper.GetMessageExpecting(eventTypeName, existingType, "Map"));
            }

            return AdapterForTypedMap(theEvent, existingType);
        }

        public EventBean AdapterForObjectArray(Object[] theEvent, String eventTypeName)
        {
            var existingType = _nameToTypeMap.Get(eventTypeName);
            if (!(existingType is ObjectArrayEventType))
            {
                throw new EPException(
                    EventAdapterServiceHelper.GetMessageExpecting(eventTypeName, existingType, "Object-array"));
            }

            return AdapterForTypedObjectArray(theEvent, existingType);
        }

        public EventBean AdapterForDOM(XElement element)
        {
            var rootElementName = element.Name.LocalName;
            var eventType = _xelementRootElementNames.Get(rootElementName);
            if (eventType == null)
            {
                throw new EventAdapterException(
                    "DOM event root element name '" + rootElementName +
                    "' has not been configured");
            }

            return new XEventBean(element, eventType);
        }

        public EventBean AdapterForDOM(XmlNode node)
        {
            XmlNode namedNode;
            if (node is XmlDocument)
            {
                namedNode = ((XmlDocument) node).DocumentElement;
            }
            else if (node is XmlElement)
            {
                namedNode = node;
            }
            else
            {
                throw new EPException(
                    "Unexpected DOM node of type '" + node.GetType().Name +
                    "' encountered, please supply a XmlDocument or XmlElement node");
            }

            var rootElementName = namedNode.LocalName;
            if (rootElementName == null)
            {
                rootElementName = namedNode.Name;
            }

            var eventType = _xmldomRootElementNames.Get(rootElementName);
            if (eventType == null)
            {
                throw new EventAdapterException(
                    "DOM event root element name '" + rootElementName +
                    "' has not been configured");
            }

            return new XMLEventBean(namedNode, eventType);
        }

        public EventBean AdapterForTypedDOM(XmlNode node, EventType eventType)
        {
            return new XMLEventBean(node, eventType);
        }

        public EventBean AdapterForTypedDOM(XObject node, EventType eventType)
        {
            return new XEventBean(node, eventType);
        }

        public EventBean AdapterForType(Object theEvent, EventType eventType)
        {
            return EventAdapterServiceHelper.AdapterForType(theEvent, eventType, this);
        }

        public EventBean AdapterForTypedMap(IDictionary<String, Object> properties, EventType eventType)
        {
            return new MapEventBean(properties, eventType);
        }

        public EventBean AdapterForTypedObjectArray(Object[] properties, EventType eventType)
        {
            return new ObjectArrayEventBean(properties, eventType);
        }

        public EventType AddWrapperType(
            String eventTypeName,
            EventType underlyingEventType,
            IDictionary<String, Object> propertyTypes,
            bool isNamedWindow,
            bool isInsertInto)
        {
            using (_syncLock.Acquire())
            {
                // If we are wrapping an underlying type that is itself a wrapper, then this is a special case
                if (underlyingEventType is WrapperEventType)
                {
                    var underlyingWrapperType = (WrapperEventType) underlyingEventType;

                    // the underlying type becomes the type already wrapped
                    // properties are a superset of the wrapped properties and the additional properties
                    underlyingEventType = underlyingWrapperType.UnderlyingEventType;
                    IDictionary<String, Object> propertiesSuperset = new Dictionary<String, Object>();
                    propertiesSuperset.PutAll(underlyingWrapperType.UnderlyingMapType.Types);
                    propertiesSuperset.PutAll(propertyTypes);
                    propertyTypes = propertiesSuperset;
                }

                var isPropertyAgnostic = false;
                if (underlyingEventType is EventTypeSPI)
                {
                    isPropertyAgnostic = ((EventTypeSPI) underlyingEventType).Metadata.IsPropertyAgnostic;
                }

                var metadata = EventTypeMetadata.CreateWrapper(
                    eventTypeName, isNamedWindow, isInsertInto,
                    isPropertyAgnostic);
                var typeId = _eventTypeIdGenerator.GetTypeId(eventTypeName);
                var newEventType = new WrapperEventType(
                    metadata, eventTypeName, typeId,
                    underlyingEventType, propertyTypes, this);

                var existingType = _nameToTypeMap.Get(eventTypeName);
                if (existingType != null)
                {
                    // The existing type must be the same as the type created
                    if (!newEventType.EqualsCompareType(existingType))
                    {
                        // It is possible that the wrapped event type is compatible: a child type of the desired type
                        var message = IsCompatibleWrapper(existingType, underlyingEventType, propertyTypes);
                        if (message == null)
                        {
                            return existingType;
                        }

                        throw new EventAdapterException(
                            "Event type named '" + eventTypeName +
                            "' has already been declared with differing column name or type information: " +
                            message);
                    }

                    // Since it's the same, return the existing type
                    return existingType;
                }

                _nameToTypeMap.Put(eventTypeName, newEventType);

                return newEventType;
            }
        }

        public EventType CreateAnonymousMapType(string typeName, DataMap propertyTypes, bool isTransient)
        {
            var assignedTypeName = EventAdapterServiceConstants.ANONYMOUS_TYPE_NAME_PREFIX + typeName;
            var metadata = EventTypeMetadata.CreateAnonymous(assignedTypeName, ApplicationType.MAP);
            var mapEventType = new MapEventType(
                metadata, assignedTypeName, _eventTypeIdGenerator.GetTypeId(assignedTypeName), this, propertyTypes, null,
                null, null);
            return _anonymousTypeCache.AddReturnExistingAnonymousType(mapEventType);
        }

        public EventType CreateAnonymousObjectArrayType(String typeName, IDictionary<String, Object> propertyTypes)
        {
            var assignedTypeName = EventAdapterServiceConstants.ANONYMOUS_TYPE_NAME_PREFIX + typeName;
            var metadata = EventTypeMetadata.CreateAnonymous(assignedTypeName, ApplicationType.OBJECTARR);
            var oaEventType = new ObjectArrayEventType(
                metadata, assignedTypeName, _eventTypeIdGenerator.GetTypeId(assignedTypeName), this, propertyTypes, null,
                null, null);
            return _anonymousTypeCache.AddReturnExistingAnonymousType(oaEventType);
        }

        public EventType CreateAnonymousAvroType(
            String typeName,
            IDictionary<String, Object> properties,
            Attribute[] annotations,
            String statementName,
            String engineURI)
        {
            var assignedTypeName = EventAdapterServiceConstants.ANONYMOUS_TYPE_NAME_PREFIX + typeName;
            var metadata = EventTypeMetadata.CreateAnonymous(assignedTypeName, ApplicationType.AVRO);
            var typeId = _eventTypeIdGenerator.GetTypeId(assignedTypeName);
            var newEventType = _avroHandler.NewEventTypeFromNormalized(
                metadata, assignedTypeName, typeId, this, properties, annotations, null, null, null, statementName,
                engineURI);
            return _anonymousTypeCache.AddReturnExistingAnonymousType(newEventType);
        }

        public EventType CreateSemiAnonymousMapType(
            String typeName,
            IDictionary<String, Pair<EventType, String>> taggedEventTypes,
            IDictionary<String, Pair<EventType, String>> arrayEventTypes,
            bool isUsedByChildViews)
        {
            IDictionary<String, Object> mapProperties = new LinkedHashMap<String, Object>();
            foreach (var entry in taggedEventTypes)
            {
                mapProperties.Put(entry.Key, entry.Value.First);
            }
            foreach (var entry in arrayEventTypes)
            {
                mapProperties.Put(
                    entry.Key, new[]
                    {
                        entry.Value.First
                    });
            }
            return CreateAnonymousMapType(typeName, mapProperties, true);
        }

        public EventType CreateAnonymousWrapperType(
            String typeName,
            EventType underlyingEventType,
            IDictionary<String, Object> propertyTypes)
        {
            var assignedTypeName = EventAdapterServiceConstants.ANONYMOUS_TYPE_NAME_PREFIX + typeName;
            var metadata = EventTypeMetadata.CreateAnonymous(assignedTypeName, ApplicationType.WRAPPER);

            // If we are wrapping an underlying type that is itself a wrapper, then this is a special case: unwrap
            if (underlyingEventType is WrapperEventType)
            {
                var underlyingWrapperType = (WrapperEventType) underlyingEventType;

                // the underlying type becomes the type already wrapped
                // properties are a superset of the wrapped properties and the additional properties
                underlyingEventType = underlyingWrapperType.UnderlyingEventType;
                IDictionary<String, Object> propertiesSuperset = new Dictionary<String, Object>();
                propertiesSuperset.PutAll(underlyingWrapperType.UnderlyingMapType.Types);
                propertiesSuperset.PutAll(propertyTypes);
                propertyTypes = propertiesSuperset;
            }

            var wrapperEventType = new WrapperEventType(
                metadata, assignedTypeName, _eventTypeIdGenerator.GetTypeId(assignedTypeName), underlyingEventType,
                propertyTypes, this);
            return _anonymousTypeCache.AddReturnExistingAnonymousType(wrapperEventType);
        }

        public EventBean AdapterForTypedWrapper(
            EventBean theEvent,
            IDictionary<String, Object> properties,
            EventType eventType)
        {
            if (theEvent is DecoratingEventBean)
            {
                var wrapper = (DecoratingEventBean) theEvent;
                properties.PutAll(wrapper.DecoratingProperties);
                return new WrapperEventBean(wrapper.UnderlyingEvent, properties, eventType);
            }

            return new WrapperEventBean(theEvent, properties, eventType);
        }

        public void AddAutoNamePackage(String @namespace)
        {
            _namespaces.Add(@namespace);
        }

        public EventType CreateAnonymousBeanType(String eventTypeName, Type clazz)
        {
            var beanEventType = new BeanEventType(
                _container,
                EventTypeMetadata.CreateBeanType(eventTypeName, clazz, false, false, false, TypeClass.ANONYMOUS),
                -1, clazz, this,
                _beanEventAdapter.GetClassToLegacyConfigs(clazz.AssemblyQualifiedName));
            return _anonymousTypeCache.AddReturnExistingAnonymousType(beanEventType);
        }

        private Pair<EventType[], ISet<EventType>> GetSuperTypesDepthFirst(
            ICollection<String> superTypesSet,
            bool expectMapType)
        {
            if (superTypesSet == null || superTypesSet.IsEmpty())
            {
                return new Pair<EventType[], ISet<EventType>>(null, null);
            }

            var superTypes = new EventType[superTypesSet.Count];
            var deepSuperTypes = new LinkedHashSet<EventType>();

            var count = 0;
            foreach (var superName in superTypesSet)
            {
                var type = _nameToTypeMap.Get(superName);
                if (type == null)
                {
                    throw new EventAdapterException("Supertype by name '" + superName + "' could not be found");
                }
                if (expectMapType)
                {
                    if (!(type is MapEventType))
                    {
                        throw new EventAdapterException(
                            "Supertype by name '" + superName +
                            "' is not a Map, expected a Map event type as a supertype");
                    }
                }
                else
                {
                    if (!(type is ObjectArrayEventType))
                    {
                        throw new EventAdapterException(
                            "Supertype by name '" + superName +
                            "' is not an Object-array type, expected a Object-array event type as a supertype");
                    }
                }
                superTypes[count++] = type;
                deepSuperTypes.Add(type);
                AddRecursiveSupertypes(deepSuperTypes, type);
            }

            var superTypesListDepthFirst = new List<EventType>(deepSuperTypes);
            superTypesListDepthFirst.Reverse();

            return new Pair<EventType[], ISet<EventType>>(
                superTypes, new LinkedHashSet<EventType>(superTypesListDepthFirst));
        }

        public bool RemoveType(String name)
        {
            var eventType = _nameToTypeMap.Delete(name);
            if (eventType == null)
            {
                return false;
            }

            if (eventType is BaseXMLEventType)
            {
                var baseXML = (BaseXMLEventType) eventType;
                _xmldomRootElementNames.Remove(baseXML.RootElementName);
                _xelementRootElementNames.Remove(baseXML.RootElementName);
            }

            _nameToHandlerMap.Remove(name);
            return true;
        }

        /// <summary>Set the legacy class type information. </summary>
        /// <value>is the legacy class configs</value>
        public IDictionary<string, ConfigurationEventTypeLegacy> TypeLegacyConfigs
        {
            set => _beanEventAdapter.TypeToLegacyConfigs = value;
            get => _beanEventAdapter.TypeToLegacyConfigs;
        }

        public ConfigurationEventTypeLegacy GetTypeLegacyConfigs(String className)
        {
            return _beanEventAdapter.GetClassToLegacyConfigs(className);
        }

        public ICollection<EventType> AllTypes => _nameToTypeMap.Values.ToArray();

        public EventType GetEventTypeByName(String eventTypeName)
        {
            if (eventTypeName == null)
            {
                throw new IllegalStateException("Null event type name parameter");
            }
            return _nameToTypeMap.Get(eventTypeName);
        }

        /// <summary>
        /// Add a configured XML DOM event type.
        /// </summary>
        /// <param name="eventTypeName">is the name name of the event type</param>
        /// <param name="configurationEventTypeXMLDOM">configures the event type schema and namespace and XPathproperty information.</param>
        /// <param name="optionalSchemaModel">The optional schema model.</param>
        /// <param name="isPreconfiguredStatic">if set to <c>true</c> [is preconfigured static].</param>
        /// <returns></returns>
        public EventType AddXMLDOMType(
            String eventTypeName,
            ConfigurationEventTypeXMLDOM configurationEventTypeXMLDOM,
            SchemaModel optionalSchemaModel,
            bool isPreconfiguredStatic)
        {
            using (_syncLock.Acquire())
            {
                return AddXMLDOMType(
                    eventTypeName, configurationEventTypeXMLDOM, optionalSchemaModel,
                    isPreconfiguredStatic, false);
            }
        }

        public EventType ReplaceXMLEventType(
            String xmlEventTypeName,
            ConfigurationEventTypeXMLDOM config,
            SchemaModel schemaModel)
        {
            return AddXMLDOMType(xmlEventTypeName, config, schemaModel, false, true);
        }

        /// <summary>
        /// Add a configured XML DOM event type.
        /// </summary>
        /// <param name="eventTypeName">is the name name of the event type</param>
        /// <param name="configurationEventTypeXMLDOM">configures the event type schema and namespace and XPathproperty information.</param>
        /// <param name="optionalSchemaModel">The optional schema model.</param>
        /// <param name="isPreconfiguredStatic">if set to <c>true</c> [is preconfigured static].</param>
        /// <param name="allowOverrideExisting">if set to <c>true</c> [allow override existing].</param>
        /// <returns></returns>
        private EventType AddXMLDOMType(
            String eventTypeName,
            ConfigurationEventTypeXMLDOM configurationEventTypeXMLDOM,
            SchemaModel optionalSchemaModel,
            bool isPreconfiguredStatic,
            bool allowOverrideExisting)
        {
            using (_syncLock.Acquire())
            {
                if (configurationEventTypeXMLDOM.RootElementName == null)
                {
                    throw new EventAdapterException("Required root element name has not been supplied");
                }

                if (!allowOverrideExisting)
                {
                    var existingType = _nameToTypeMap.Get(eventTypeName);
                    if (existingType != null)
                    {
                        var message = "Event type named '" + eventTypeName +
                                      "' has already been declared with differing column name or type information";
                        if (!(existingType is BaseXMLEventType))
                        {
                            throw new EventAdapterException(message);
                        }
                        var config =
                            ((BaseXMLEventType) existingType).ConfigurationEventTypeXMLDOM;
                        if (!config.Equals(configurationEventTypeXMLDOM))
                        {
                            throw new EventAdapterException(message);
                        }

                        return existingType;
                    }
                }

                var metadata = EventTypeMetadata.CreateXMLType(
                    eventTypeName, isPreconfiguredStatic,
                    configurationEventTypeXMLDOM.SchemaResource ==
                    null &&
                    configurationEventTypeXMLDOM.SchemaText ==
                    null);
                EventType type;
                if ((configurationEventTypeXMLDOM.SchemaResource == null) &&
                    (configurationEventTypeXMLDOM.SchemaText == null))
                {
                    type = new SimpleXMLEventType(
                        metadata,
                        _eventTypeIdGenerator.GetTypeId(eventTypeName),
                        configurationEventTypeXMLDOM,
                        this, _container.LockManager());
                }
                else
                {
                    if (optionalSchemaModel == null)
                    {
                        throw new EPException("Schema model has not been provided");
                    }
                    type = new SchemaXMLEventType(
                        metadata, 
                        _eventTypeIdGenerator.GetTypeId(eventTypeName),
                        configurationEventTypeXMLDOM, 
                        optionalSchemaModel, 
                        this, _container.LockManager());
                }

                EventType xelementType = new SimpleXElementType(
                    metadata,
                    _eventTypeIdGenerator.GetTypeId(eventTypeName),
                    configurationEventTypeXMLDOM,
                    this, _container.LockManager());

                _nameToTypeMap.Put(eventTypeName, type);
                _xmldomRootElementNames.Put(configurationEventTypeXMLDOM.RootElementName, type);
                _xelementRootElementNames.Put(configurationEventTypeXMLDOM.RootElementName, xelementType);

                return type;
            }
        }

        /// <summary>Returns true if the wrapper type is compatible with an existing wrapper type, for the reason that the underlying event is a subtype of the existing underlying wrapper's type. </summary>
        /// <param name="existingType">is the existing wrapper type</param>
        /// <param name="underlyingType">is the proposed new wrapper type's underlying type</param>
        /// <param name="propertyTypes">is the additional properties</param>
        /// <returns>true for compatible, or false if not</returns>
        public static String IsCompatibleWrapper(
            EventType existingType,
            EventType underlyingType,
            IDictionary<String, Object> propertyTypes)
        {
            if (!(existingType is WrapperEventType))
            {
                return "Type '" + existingType.Name + "' is not compatible";
            }
            var existingWrapper = (WrapperEventType) existingType;

            var message = MapEventType.IsDeepEqualsProperties(
                existingType.Name,
                existingWrapper.UnderlyingMapType.Types,
                propertyTypes);
            if (message != null)
            {
                return message;
            }
            var existingUnderlyingType = existingWrapper.UnderlyingEventType;

            // If one of the supertypes of the underlying type is the existing underlying type, we are compatible
            if (underlyingType.SuperTypes == null)
            {
                return "Type '" + existingType.Name + "' is not compatible";
            }

            if (underlyingType.DeepSuperTypes.Any(superUnderlying => superUnderlying == existingUnderlyingType))
            {
                return null;
            }
            return "Type '" + existingType.Name + "' is not compatible";
        }

        public EventBean AdapterForTypedObject(Object bean, EventType eventType)
        {
            return new BeanEventBean(bean, eventType);
        }

        private Pair<EventType[], ICollection<EventType>> GetMapSuperTypes(ConfigurationEventTypeMap optionalConfig)
        {
            if (optionalConfig == null || optionalConfig.SuperTypes == null || optionalConfig.SuperTypes.IsEmpty())
            {
                return new Pair<EventType[], ICollection<EventType>>(null, null);
            }

            var superTypes = new EventType[optionalConfig.SuperTypes.Count];
            ICollection<EventType> deepSuperTypes = new LinkedHashSet<EventType>();

            var count = 0;
            foreach (var superName in optionalConfig.SuperTypes)
            {
                var type = _nameToTypeMap.Get(superName);
                if (type == null)
                {
                    throw new EventAdapterException("Map supertype by name '" + superName + "' could not be found");
                }
                if (!(type is MapEventType))
                {
                    throw new EventAdapterException(
                        "Supertype by name '" + superName +
                        "' is not a Map, expected a Map event type as a supertype");
                }
                superTypes[count++] = type;
                deepSuperTypes.Add(type);
                AddRecursiveSupertypes(deepSuperTypes, type);
            }
            return new Pair<EventType[], ICollection<EventType>>(superTypes, deepSuperTypes);
        }

        private static void AddRecursiveSupertypes(
            ICollection<EventType> superTypes,
            EventType child)
        {
            if (child.SuperTypes != null)
            {
                for (var i = 0; i < child.SuperTypes.Length; i++)
                {
                    superTypes.Add(child.SuperTypes[i]);
                    AddRecursiveSupertypes(superTypes, child.SuperTypes[i]);
                }
            }
        }

        public EventBean[] TypeCast(IList<EventBean> events, EventType targetType)
        {
            return EventAdapterServiceHelper.TypeCast(events, targetType, this);
        }

        public EventBeanSPI GetShellForType(EventType eventType)
        {
            return EventAdapterServiceHelper.GetShellForType(eventType);
        }

        public EventBeanAdapterFactory GetAdapterFactoryForType(EventType eventType)
        {
            return EventAdapterServiceHelper.GetAdapterFactoryForType(eventType);
        }

        public EventType AddAvroType(
            String eventTypeName,
            ConfigurationEventTypeAvro avro,
            bool isPreconfiguredStatic,
            bool isPreconfigured,
            bool isConfigured,
            bool isNamedWindow,
            bool isInsertInto)
        {
            var metadata = EventTypeMetadata.CreateNonPonoApplicationType(
                ApplicationType.AVRO, eventTypeName, isPreconfiguredStatic, isPreconfigured, isConfigured, isNamedWindow,
                isInsertInto);

            var typeId = _eventTypeIdGenerator.GetTypeId(eventTypeName);
            var avroSuperTypes = EventTypeUtility.GetSuperTypesDepthFirst(
                avro, EventUnderlyingType.AVRO, _nameToTypeMap);
            var newEventType = _avroHandler.NewEventTypeFromSchema(
                metadata, eventTypeName, typeId, this, avro, avroSuperTypes.First, avroSuperTypes.Second);

            var existingType = _nameToTypeMap.Get(eventTypeName);
            if (existingType != null)
            {
                _avroHandler.ValidateExistingType(existingType, newEventType);
                return existingType;
            }

            _nameToTypeMap.Put(eventTypeName, newEventType);

            return newEventType;
        }

        public EventType AddAvroType(
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
            String engineURI)
        {
            var metadata = EventTypeMetadata.CreateNonPonoApplicationType(
                ApplicationType.AVRO, eventTypeName, isPreconfiguredStatic, isPreconfigured, isConfigured, isNamedWindow,
                isInsertInto);

            var typeId = _eventTypeIdGenerator.GetTypeId(eventTypeName);
            var avroSuperTypes = EventTypeUtility.GetSuperTypesDepthFirst(
                config, EventUnderlyingType.AVRO, _nameToTypeMap);
            var newEventType = _avroHandler.NewEventTypeFromNormalized(
                metadata, eventTypeName, typeId, this, types, annotations, config, avroSuperTypes.First,
                avroSuperTypes.Second, statementName, engineURI);

            var existingType = _nameToTypeMap.Get(eventTypeName);
            if (existingType != null)
            {
                _avroHandler.ValidateExistingType(existingType, newEventType);
                return existingType;
            }

            _nameToTypeMap.Put(eventTypeName, newEventType);

            return newEventType;
        }

        public EventBean AdapterForAvro(Object avroGenericDataDotRecord, String eventTypeName)
        {
            EventType existingType = _nameToTypeMap.Get(eventTypeName);
            if (!(existingType is AvroSchemaEventType))
            {
                throw new EPException(
                    EventAdapterServiceHelper.GetMessageExpecting(eventTypeName, existingType, "Avro"));
            }
            return _avroHandler.AdapterForTypeAvro(avroGenericDataDotRecord, existingType);
        }

        public EventBean AdapterForTypedAvro(Object avroGenericDataDotRecord, EventType eventType)
        {
            return _avroHandler.AdapterForTypeAvro(avroGenericDataDotRecord, eventType);
        }

        public EventAdapterAvroHandler EventAdapterAvroHandler => _avroHandler;

        public TypeWidenerCustomizer GetTypeWidenerCustomizer(EventType resultEventType)
        {
            return resultEventType is AvroSchemaEventType
                ? _avroHandler.GetTypeWidenerCustomizer(resultEventType)
                : null;
        }
    }
}