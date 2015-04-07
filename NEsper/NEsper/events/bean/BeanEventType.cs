///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.events.property;
using com.espertech.esper.util;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Implementation of the EventType interface for handling object-type classes.
    /// </summary>
    public class BeanEventType : EventTypeSPI, NativeEventType
    {
        private readonly EventAdapterService _eventAdapterService;
        private IDictionary<String, SimplePropertyInfo> _simpleProperties;
        private IDictionary<String, InternalEventPropDescriptor> _mappedPropertyDescriptors;
        private IDictionary<String, InternalEventPropDescriptor> _indexedPropertyDescriptors;
        private ICollection<EventType> _deepSuperTypes;

        private IDictionary<String, IList<SimplePropertyInfo>> _simpleSmartPropertyTable;
        private IDictionary<String, IList<SimplePropertyInfo>> _indexedSmartPropertyTable;
        private IDictionary<String, IList<SimplePropertyInfo>> _mappedSmartPropertyTable;

        private readonly IDictionary<String, EventPropertyGetter> _propertyGetterCache;
        private IList<EventPropertyDescriptor> _propertyDescriptors;
        private EventPropertyDescriptor[] _writeablePropertyDescriptors;
        private IDictionary<String, Pair<EventPropertyDescriptor, BeanEventPropertyWriter>> _writerMap;
        private IDictionary<String, EventPropertyDescriptor> _propertyDescriptorMap;
        private readonly String _copyMethodName;

        /// <summary>
        /// Constructor takes a class as an argument.
        /// </summary>
        /// <param name="metadata">event type metadata</param>
        /// <param name="eventTypeId">The event type id.</param>
        /// <param name="clazz">is the class of an object</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="optionalLegacyDef">optional configuration supplying legacy event type information</param>
        public BeanEventType(EventTypeMetadata metadata,
                             int eventTypeId,
                             Type clazz,
                             EventAdapterService eventAdapterService,
                             ConfigurationEventTypeLegacy optionalLegacyDef)
        {
            Metadata = metadata;
            UnderlyingType = clazz;
            _eventAdapterService = eventAdapterService;
            OptionalLegacyDef = optionalLegacyDef;
            EventTypeId = eventTypeId;
            if (optionalLegacyDef != null)
            {
                FactoryMethodName = optionalLegacyDef.FactoryMethod;
                _copyMethodName = optionalLegacyDef.CopyMethod;
                PropertyResolutionStyle = optionalLegacyDef.PropertyResolutionStyle;
            }
            else
            {
                PropertyResolutionStyle = eventAdapterService.BeanEventTypeFactory.DefaultPropertyResolutionStyle;
            }
            _propertyGetterCache = new Dictionary<String, EventPropertyGetter>();

            Initialize(false);

            EventTypeUtility.TimestampPropertyDesc desc = EventTypeUtility.ValidatedDetermineTimestampProps(
                this, 
                optionalLegacyDef == null ? null : optionalLegacyDef.StartTimestampPropertyName, 
                optionalLegacyDef == null ? null : optionalLegacyDef.EndTimestampPropertyName, 
                SuperTypes);
            StartTimestampPropertyName = desc.Start;
            EndTimestampPropertyName = desc.End;
        }

        public string StartTimestampPropertyName { get; private set; }

        public string EndTimestampPropertyName { get; private set; }

        public ConfigurationEventTypeLegacy OptionalLegacyDef { get; private set; }

        public string Name
        {
            get { return Metadata.PublicName; }
        }

        public EventPropertyDescriptor GetPropertyDescriptor(String propertyName)
        {
            return _propertyDescriptorMap.Get(propertyName);
        }

        public int EventTypeId { get; private set; }

        /// <summary>Returns the factory methods name, or null if none defined. </summary>
        /// <value>factory methods name</value>
        public string FactoryMethodName { get; private set; }

        public Type GetPropertyType(String propertyName)
        {
            var simpleProp = GetSimplePropertyInfo(propertyName);
            if ((simpleProp != null) && (simpleProp.PropertyType != null))
            {
                return simpleProp.PropertyType;
            }

            var prop = PropertyParser.ParseAndWalk(propertyName, false);
            if (prop is SimpleProperty)
            {
                // there is no such property since it wasn't in simplePropertyTypes
                return null;
            }
            return prop.GetPropertyType(this, _eventAdapterService);
        }

        public bool IsProperty(String propertyName)
        {
            return (GetPropertyType(propertyName) != null);
        }

        public Type UnderlyingType { get; private set; }

        /// <summary>Returns the property resolution style. </summary>
        /// <value>property resolution style</value>
        public PropertyResolutionStyle PropertyResolutionStyle { get; private set; }

        public EventPropertyGetter GetGetter(String propertyName)
        {
            EventPropertyGetter cachedGetter = _propertyGetterCache.Get(propertyName);
            if (cachedGetter != null)
            {
                return cachedGetter;
            }

            SimplePropertyInfo simpleProp = GetSimplePropertyInfo(propertyName);
            if ((simpleProp != null) && (simpleProp.Getter != null))
            {
                EventPropertyGetter propertyGetter = simpleProp.Getter;
                _propertyGetterCache.Put(propertyName, propertyGetter);
                return propertyGetter;
            }

            Property prop = PropertyParser.ParseAndWalk(propertyName, false);
            if (prop is SimpleProperty)
            {
                // there is no such property since it wasn't in simplePropertyGetters
                return null;
            }

            EventPropertyGetter getter = prop.GetGetter(this, _eventAdapterService);
            _propertyGetterCache.Put(propertyName, getter);
            return getter;
        }

        public EventPropertyGetterMapped GetGetterMapped(String mappedPropertyName)
        {
            EventPropertyDescriptor desc = _propertyDescriptorMap.Get(mappedPropertyName);
            if (desc == null || !desc.IsMapped)
            {
                return null;
            }
            var mappedProperty = new MappedProperty(mappedPropertyName);
            return (EventPropertyGetterMapped)mappedProperty.GetGetter(this, _eventAdapterService);
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedPropertyName)
        {
            EventPropertyDescriptor desc = _propertyDescriptorMap.Get(indexedPropertyName);
            if (desc == null || !desc.IsIndexed)
            {
                return null;
            }
            var indexedProperty = new IndexedProperty(indexedPropertyName);
            return (EventPropertyGetterIndexed)indexedProperty.GetGetter(this, _eventAdapterService);
        }

        /// <summary>Looks up and returns a cached simple property's descriptor. </summary>
        /// <param name="propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public InternalEventPropDescriptor GetSimpleProperty(String propertyName)
        {
            var simpleProp = GetSimplePropertyInfo(propertyName);
            return simpleProp != null ? simpleProp.Descriptor : null;
        }

        /// <summary>Looks up and returns a cached mapped property's descriptor. </summary>
        /// <param name="propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public InternalEventPropDescriptor GetMappedProperty(String propertyName)
        {
            switch (PropertyResolutionStyle)
            {
                case PropertyResolutionStyle.CASE_SENSITIVE:
                    return _mappedPropertyDescriptors.Get(propertyName);
                case PropertyResolutionStyle.CASE_INSENSITIVE:
                    {
                        var propertyInfos = _mappedSmartPropertyTable.Get(propertyName.ToLower());
                        return propertyInfos != null
                                   ? propertyInfos[0].Descriptor
                                   : null;
                    }
                case PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE:
                    {
                        var propertyInfos = _mappedSmartPropertyTable.Get(propertyName.ToLower());
                        if (propertyInfos != null)
                        {
                            if (propertyInfos.Count != 1)
                            {
                                throw new EPException("Unable to determine which property to use for \"" + propertyName + "\" because more than one property matched");
                            }

                            return propertyInfos[0].Descriptor;
                        }
                    }
                    break;
            }
            return null;
        }

        /// <summary>Looks up and returns a cached indexed property's descriptor. </summary>
        /// <param name="propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public InternalEventPropDescriptor GetIndexedProperty(String propertyName)
        {
            switch (PropertyResolutionStyle)
            {
                case PropertyResolutionStyle.CASE_SENSITIVE:
                    return _indexedPropertyDescriptors.Get(propertyName);
                case PropertyResolutionStyle.CASE_INSENSITIVE:
                    {
                        var propertyInfos = _indexedSmartPropertyTable.Get(propertyName.ToLower());
                        return propertyInfos != null
                                   ? propertyInfos[0].Descriptor
                                   : null;
                    }
                case PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE:
                    {
                        var propertyInfos = _indexedSmartPropertyTable.Get(propertyName.ToLower());
                        if (propertyInfos != null)
                        {
                            if (propertyInfos.Count != 1)
                            {
                                throw new EPException("Unable to determine which property to use for \"" + propertyName + "\" because more than one property matched");
                            }

                            return propertyInfos[0].Descriptor;
                        }
                    }
                    break;
            }
            return null;
        }

        public string[] PropertyNames { get; private set; }

        public EventType[] SuperTypes { get; private set; }

        public EventType[] DeepSuperTypes { get; private set; }

        /// <summary>Returns the fast class reference, if code generation is used for this type, else null. </summary>
        /// <value>fast class, or null if no code generation</value>
        public FastClass FastClass { get; private set; }

        public override String ToString()
        {
            return "BeanEventType" +
                   " name=" + Name +
                   " clazz=" + UnderlyingType.Name;
        }

        private void Initialize(bool isConfigured)
        {
            var propertyListBuilder = PropertyListBuilderFactory.CreateBuilder(OptionalLegacyDef);
            var properties = propertyListBuilder.AssessProperties(UnderlyingType);

            _propertyDescriptors = new EventPropertyDescriptor[properties.Count];
            _propertyDescriptorMap = new Dictionary<String, EventPropertyDescriptor>();
            PropertyNames = new String[properties.Count];
            _simpleProperties = new Dictionary<String, SimplePropertyInfo>();
            _mappedPropertyDescriptors = new Dictionary<String, InternalEventPropDescriptor>();
            _indexedPropertyDescriptors = new Dictionary<String, InternalEventPropDescriptor>();

            if (UsesSmartResolutionStyle)
            {
                _simpleSmartPropertyTable = new Dictionary<String, IList<SimplePropertyInfo>>();
                _mappedSmartPropertyTable = new Dictionary<String, IList<SimplePropertyInfo>>();
                _indexedSmartPropertyTable = new Dictionary<String, IList<SimplePropertyInfo>>();
            }

            if ((OptionalLegacyDef == null) ||
                (OptionalLegacyDef.CodeGeneration != CodeGenerationEnum.DISABLED))
            {
                // get CGLib fast class
                FastClass = null;
                try
                {
                    FastClass = FastClass.Create(UnderlyingType);
                }
                catch (Exception ex)
                {
                    Log.Warn(".initialize Unable to obtain CGLib fast class and/or method implementation for class " +
                            UnderlyingType.Name + ", error msg is " + ex.Message, ex);
                    FastClass = null;
                }
            }

            int count = 0;
            foreach (InternalEventPropDescriptor desc in properties)
            {
                String propertyName = desc.PropertyName;
                Type underlyingType;
                Type componentType;
                bool isRequiresIndex;
                bool isRequiresMapkey;
                bool isIndexed;
                bool isMapped;
                bool isFragment;

                if (desc.PropertyType.GetValueOrDefault() == EventPropertyType.SIMPLE)
                {
                    EventPropertyGetter getter;
                    Type type;
                    if (desc.ReadMethod != null)
                    {
                        getter = PropertyHelper.GetGetter(desc.PropertyName, desc.ReadMethod, FastClass, _eventAdapterService);
                        type = desc.ReadMethod.ReturnType;
                    }
                    else
                    {
                        if (desc.AccessorField == null)
                        {
                            // Ignore property
                            continue;
                        }
                        getter = new ReflectionPropFieldGetter(desc.AccessorField, _eventAdapterService);
                        type = desc.AccessorField.FieldType;
                    }

                    underlyingType = type;
                    componentType = null;
                    isRequiresIndex = false;
                    isRequiresMapkey = false;
                    isIndexed = false;
                    isMapped = false;

                    if (type.IsGenericDictionary())
                    {
                        isMapped = true;
                        // We do not yet allow to fragment maps entries.
                        // Class genericType = TypeHelper.GetGenericReturnTypeMap(desc.ReadMethod, desc.AccessorField);
                        isFragment = false;

                        if (desc.ReadMethod != null)
                        {
                            componentType = TypeHelper.GetGenericReturnTypeMap(desc.ReadMethod, false);
                        }
                        else if (desc.AccessorField != null)
                        {
                            componentType = TypeHelper.GetGenericFieldTypeMap(desc.AccessorField, false);
                        }
                        else
                        {
                            componentType = typeof (object);
                        }
                    }
                    else if (type.IsArray)
                    {
                        isIndexed = true;
                        isFragment = type.GetElementType().IsFragmentableType();
                        componentType = type.GetElementType();
                    }
                    else if (type.IsImplementsInterface(typeof(IEnumerable)))
                    {
                        isIndexed = true;
                        Type genericType = TypeHelper.GetGenericReturnType(desc.ReadMethod, desc.AccessorField, true);
                        isFragment = genericType.IsFragmentableType();
                        if (genericType != null)
                        {
                            componentType = genericType;
                        }
                        else
                        {
                            componentType = typeof(Object);
                        }
                    }
                    else
                    {
                        isMapped = false;
                        isFragment = type.IsFragmentableType();
                    }
                    _simpleProperties.Put(propertyName, new SimplePropertyInfo(type, getter, desc));

                    // Recognize that there may be properties with overlapping case-insentitive names
                    if (UsesSmartResolutionStyle)
                    {
                        // Find the property in the smart property table
                        var smartPropertyName = propertyName.ToLower();
                        var propertyInfoList = _simpleSmartPropertyTable.Get(smartPropertyName);
                        if (propertyInfoList == null)
                        {
                            propertyInfoList = new List<SimplePropertyInfo>();
                            _simpleSmartPropertyTable.Put(smartPropertyName, propertyInfoList);
                        }

                        // Enter the property into the smart property list
                        var propertyInfo = new SimplePropertyInfo(type, getter, desc);
                        propertyInfoList.Add(propertyInfo);
                    }
                }
                else if (desc.PropertyType.GetValueOrDefault() == EventPropertyType.MAPPED)
                {
                    _mappedPropertyDescriptors.Put(propertyName, desc);

                    underlyingType = desc.ReturnType;
                    componentType = typeof (object);
                    isRequiresIndex = false;
                    isRequiresMapkey = desc.ReadMethod.GetParameterTypes().Length > 0;
                    isIndexed = false;
                    isMapped = true;
                    isFragment = false;

                    // Recognize that there may be properties with overlapping case-insentitive names
                    if (UsesSmartResolutionStyle)
                    {
                        // Find the property in the smart property table
                        var smartPropertyName = propertyName.ToLower();
                        var propertyInfoList = _mappedSmartPropertyTable.Get(smartPropertyName);
                        if (propertyInfoList == null)
                        {
                            propertyInfoList = new List<SimplePropertyInfo>();
                            _mappedSmartPropertyTable.Put(smartPropertyName, propertyInfoList);
                        }

                        // Enter the property into the smart property list
                        var propertyInfo = new SimplePropertyInfo(desc.ReturnType, null, desc);
                        propertyInfoList.Add(propertyInfo);
                    }
                }
                else if (desc.PropertyType.GetValueOrDefault() == EventPropertyType.INDEXED)
                {
                    _indexedPropertyDescriptors.Put(propertyName, desc);

                    underlyingType = desc.ReturnType;
                    componentType = null;
                    isRequiresIndex = desc.ReadMethod.GetParameterTypes().Length > 0;
                    isRequiresMapkey = false;
                    isIndexed = true;
                    isMapped = false;
                    isFragment = desc.ReturnType.IsFragmentableType();

                    if (UsesSmartResolutionStyle)
                    {
                        // Find the property in the smart property table
                        String smartPropertyName = propertyName.ToLower();
                        IList<SimplePropertyInfo> propertyInfoList = _indexedSmartPropertyTable.Get(smartPropertyName);
                        if (propertyInfoList == null)
                        {
                            propertyInfoList = new List<SimplePropertyInfo>();
                            _indexedSmartPropertyTable.Put(smartPropertyName, propertyInfoList);
                        }

                        // Enter the property into the smart property list
                        var propertyInfo = new SimplePropertyInfo(desc.ReturnType, null, desc);
                        propertyInfoList.Add(propertyInfo);
                    }
                }
                else
                {
                    continue;
                }

                PropertyNames[count] = desc.PropertyName;
                var descriptor = new EventPropertyDescriptor(desc.PropertyName,
                    underlyingType, componentType, isRequiresIndex, isRequiresMapkey, isIndexed, isMapped, isFragment);
                _propertyDescriptors[count++] = descriptor;
                _propertyDescriptorMap.Put(descriptor.PropertyName, descriptor);
            }

            // Determine event type super types
            SuperTypes = GetBaseTypes(UnderlyingType, _eventAdapterService.BeanEventTypeFactory);
            if (SuperTypes != null && SuperTypes.Length == 0)
            {
                SuperTypes = null;
            }

            if (Metadata != null && Metadata.TypeClass == TypeClass.NAMED_WINDOW)
            {
                SuperTypes = null;
            }

            // Determine deep supertypes
            // Get base types (superclasses and interfaces), deep get of all in the tree
            ICollection<Type> supers = new HashSet<Type>();
            GetSuper(UnderlyingType, supers);
            RemoveLibraryInterfaces(supers);    // Remove CLR library base types

            // Cache the supertypes of this event type for later use
            _deepSuperTypes = new HashSet<EventType>();
            foreach (Type superClass in supers)
            {
                EventType superType = _eventAdapterService.BeanEventTypeFactory.CreateBeanType(superClass.FullName, superClass, false, false, isConfigured);
                _deepSuperTypes.Add(superType);
            }

            DeepSuperTypes = _deepSuperTypes.ToArray();
        }

        private static EventType[] GetBaseTypes(Type clazz, BeanEventTypeFactory beanEventTypeFactory)
        {
            var superclasses = new List<Type>();

            // add superclass
            var superClass = clazz.BaseType;
            if (superClass != null)
            {
                superclasses.Add(superClass);
            }

            // add interfaces
            Type[] interfaces = clazz.GetInterfaces();
            superclasses.AddAll(interfaces);

            // Build event types, ignoring language types
            var superTypes = new List<EventType>();
            foreach (var superclass in superclasses)
            {
                if (!superclass.Name.StartsWith("System"))
                {
                    EventType superType = beanEventTypeFactory.CreateBeanType(superclass.FullName, superclass, false, false, false);
                    superTypes.Add(superType);
                }
            }

            return superTypes.ToArray();
        }

        /// <summary>Add the given class's implemented interfaces and superclasses to the result set of classes. </summary>
        /// <param name="clazz">to introspect</param>
        /// <param name="result">to add classes to</param>
        internal static void GetSuper(Type clazz, ICollection<Type> result)
        {
            GetBaseInterfaces(clazz, result);
            GetBaseClasses(clazz, result);
        }

        private static void GetBaseInterfaces(Type clazz, ICollection<Type> result)
        {
            var interfaces = clazz.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                result.Add(@interface);
                GetBaseInterfaces(@interface, result);
            }
        }

        private static void GetBaseClasses(Type clazz, ICollection<Type> result)
        {
            Type superClass = clazz.BaseType;
            if (superClass == null)
            {
                return;
            }

            result.Add(superClass);
            GetSuper(superClass, result);
        }

        private static void RemoveLibraryInterfaces(ICollection<Type> types)
        {
            var coreAssembly = typeof(Object).Assembly;
            var coreTypes = types
                .Where(type => type.Assembly == coreAssembly)
                .ToArray();

            foreach (Type type in coreTypes)
            {
                types.Remove(type);
            }
        }

        private bool UsesSmartResolutionStyle
        {
            get
            {
                return (PropertyResolutionStyle == PropertyResolutionStyle.CASE_INSENSITIVE) ||
                       (PropertyResolutionStyle == PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE);
            }
        }

        private SimplePropertyInfo GetSimplePropertyInfo(String propertyName)
        {
            SimplePropertyInfo propertyInfo;
            IList<SimplePropertyInfo> simplePropertyInfoList;

            switch (PropertyResolutionStyle)
            {
                case PropertyResolutionStyle.CASE_SENSITIVE:
                    return _simpleProperties.Get(propertyName);
                case PropertyResolutionStyle.CASE_INSENSITIVE:
                    propertyInfo = _simpleProperties.Get(propertyName);
                    if (propertyInfo != null)
                    {
                        return propertyInfo;
                    }
                    simplePropertyInfoList = _simpleSmartPropertyTable.Get(propertyName.ToLower());
                    return
                        simplePropertyInfoList != null
                            ? simplePropertyInfoList[0]
                            : null;
                case PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE:
                    propertyInfo = _simpleProperties.Get(propertyName);
                    if (propertyInfo != null)
                    {
                        return propertyInfo;
                    }
                    simplePropertyInfoList = _simpleSmartPropertyTable.Get(propertyName.ToLower());
                    if (simplePropertyInfoList != null)
                    {
                        if (simplePropertyInfoList.Count != 1)
                        {
                            throw new EPException("Unable to determine which property to use for \"" + propertyName + "\" because more than one property matched");
                        }

                        return simplePropertyInfoList[0];
                    }
                    break;
            }

            return null;
        }

        /// <summary>Descriptor caching the getter, class and property info. </summary>
        public class SimplePropertyInfo
        {
            /// <summary>Ctor. </summary>
            /// <param name="clazz">is the class</param>
            /// <param name="getter">is the getter</param>
            /// <param name="descriptor">is the property info</param>
            public SimplePropertyInfo(Type clazz, EventPropertyGetter getter, InternalEventPropDescriptor descriptor)
            {
                PropertyType = clazz;
                Getter = getter;
                Descriptor = descriptor;
            }

            /// <summary>Returns the return type. </summary>
            /// <value>return type</value>
            public Type PropertyType { get; private set; }

            /// <summary>Returns the getter. </summary>
            /// <value>getter</value>
            public EventPropertyGetter Getter { get; private set; }

            /// <summary>Returns the property info. </summary>
            /// <value>property info</value>
            public InternalEventPropDescriptor Descriptor { get; private set; }
        }

        public EventTypeMetadata Metadata { get; private set; }

        public IList<EventPropertyDescriptor> PropertyDescriptors
        {
            get { return _propertyDescriptors; }
        }

        public FragmentEventType GetFragmentType(String propertyExpression)
        {
            SimplePropertyInfo simpleProp = GetSimplePropertyInfo(propertyExpression);
            if ((simpleProp != null) && (simpleProp.PropertyType != null))
            {
                GenericPropertyDesc genericProp = simpleProp.Descriptor.GetReturnTypeGeneric();
                return EventBeanUtility.CreateNativeFragmentType(genericProp.PropertyType, genericProp.GenericType, _eventAdapterService);
            }

            Property prop = PropertyParser.ParseAndWalk(propertyExpression, false);
            if (prop is SimpleProperty)
            {
                // there is no such property since it wasn't in simplePropertyTypes
                return null;
            }

            GenericPropertyDesc genericPropertyDesc = prop.GetPropertyTypeGeneric(this, _eventAdapterService);
            if (genericPropertyDesc == null)
            {
                return null;
            }
            return EventBeanUtility.CreateNativeFragmentType(genericPropertyDesc.PropertyType, genericPropertyDesc.GenericType, _eventAdapterService);
        }

        public EventPropertyWriter GetWriter(String propertyName)
        {
            if (_writeablePropertyDescriptors == null)
            {
                InitializeWriters();
            }

            var pair = _writerMap.Get(propertyName);
            if (pair != null)
            {
                return pair.Second;
            }

            var property = PropertyParser.ParseAndWalk(propertyName, false);
            if (property is MappedProperty)
            {
                var mapProp = (MappedProperty)property;
                var methodName = string.Format("Set{0}", mapProp.PropertyNameAtomic);
                var methodInfo = UnderlyingType.GetMethod(
                    methodName, BindingFlags.Public | BindingFlags.Instance, null,
                    new Type[] {typeof (string), typeof (object)}, null);
                if (methodInfo == null)
                {
                    Log.Info("Failed to find mapped property '" + mapProp.PropertyNameAtomic +
                             "' for writing to property '" + propertyName + "'");
                    return null;
                }
                
                var fastMethod = FastClass.GetMethod(methodInfo);
                return new BeanEventPropertyWriterMapProp(UnderlyingType, fastMethod, mapProp.Key);
            }

            if (property is IndexedProperty)
            {
                var indexedProp = (IndexedProperty)property;
                var methodName = string.Format("Set{0}", indexedProp.PropertyNameAtomic);
                var methodInfo = UnderlyingType.GetMethod(
                    methodName, BindingFlags.Public | BindingFlags.Instance, null,
                    new Type[] {typeof (int), typeof (object)}, null);
                if (methodInfo == null)
                {
                    Log.Info("Failed to find mapped property '" + indexedProp.PropertyNameAtomic +
                             "' for writing to property '" + propertyName + "'");
                    return null;
                }

                var fastMethod = FastClass.GetMethod(methodInfo);
                return new BeanEventPropertyWriterIndexedProp(UnderlyingType, fastMethod, indexedProp.Index);
            }

            return null;
        }

        public EventPropertyDescriptor GetWritableProperty(String propertyName)
        {
            if (_writeablePropertyDescriptors == null)
            {
                InitializeWriters();
            }

            var pair = _writerMap.Get(propertyName);
            if (pair != null)
            {
                return pair.First;
            }

            var property = PropertyParser.ParseAndWalk(propertyName, false);
            if (property is MappedProperty)
            {
                var writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }
                var mapProp = (MappedProperty)property;
                return new EventPropertyDescriptor(mapProp.PropertyNameAtomic, typeof(Object), null, false, true, false, true, false);
            }

            if (property is IndexedProperty)
            {
                var writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }
                var indexedProp = (IndexedProperty)property;
                return new EventPropertyDescriptor(indexedProp.PropertyNameAtomic, typeof(Object), null, true, false, true, false, false);
            }

            return null;
        }

        public EventPropertyDescriptor[] WriteableProperties
        {
            get
            {
                if (_writeablePropertyDescriptors == null)
                {
                    InitializeWriters();
                }

                return _writeablePropertyDescriptors;
            }
        }

        public EventBeanReader GetReader()
        {
            return new BeanEventBeanReader(this);
        }

        public EventBeanCopyMethod GetCopyMethod(String[] properties)
        {
            if (_copyMethodName == null)
            {
                if (UnderlyingType.IsSerializable)
                {
                    return new BeanEventBeanSerializableCopyMethod(this, _eventAdapterService);
                }

                if (UnderlyingType.IsInterface)
                {
                    return new BeanEventBeanSerializableCopyMethod(this, _eventAdapterService);
                }

                return null;
            }
            MethodInfo method = null;
            try
            {
                method = UnderlyingType.GetMethod(_copyMethodName);
            }
            catch (AmbiguousMatchException e)
            {
                Log.Error("Configured copy-method for class '" + UnderlyingType.Name + " not found by name '" + _copyMethodName + "': " + e.Message);
            }

            if (method == null)
            {
                if (UnderlyingType.IsSerializable)
                {
                    return new BeanEventBeanSerializableCopyMethod(this, _eventAdapterService);
                }

                throw new EPException("Configured copy-method for class '" + UnderlyingType.Name + " not found by name '" + _copyMethodName + "' and type is not Serializable");
            }

            return new BeanEventBeanConfiguredCopyMethod(this, _eventAdapterService, FastClass.GetMethod(method));
        }

        public EventBeanWriter GetWriter(String[] properties)
        {
            if (_writeablePropertyDescriptors == null)
            {
                InitializeWriters();
            }

            var writers = new BeanEventPropertyWriter[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                Pair<EventPropertyDescriptor, BeanEventPropertyWriter> pair = _writerMap.Get(properties[i]);
                if (pair != null)
                {
                    writers[i] = pair.Second;
                }
                else
                {
                    writers[i] = (BeanEventPropertyWriter)GetWriter(properties[i]);
                }

            }
            return new BeanEventBeanWriter(writers);
        }

        public bool EqualsCompareType(EventType eventType)
        {
            return this == eventType;
        }

        private void InitializeWriters()
        {
            var writables = PropertyHelper.GetWritableProperties(FastClass.TargetType);
            var desc = new EventPropertyDescriptor[writables.Count];
            var writers = new Dictionary<String, Pair<EventPropertyDescriptor, BeanEventPropertyWriter>>();

            int count = 0;
            foreach (WriteablePropertyDescriptor writable in writables)
            {
                var propertyDesc = new EventPropertyDescriptor(writable.PropertyName, writable.PropertyType, null, false, false, false, false, false);
                desc[count++] = propertyDesc;

                FastMethod fastMethod = FastClass.GetMethod(writable.WriteMethod);
                writers.Put(writable.PropertyName, new Pair<EventPropertyDescriptor, BeanEventPropertyWriter>(propertyDesc, new BeanEventPropertyWriter(UnderlyingType, fastMethod)));
            }

            _writerMap = writers;
            _writeablePropertyDescriptors = desc;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
