///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.events.property;
using com.espertech.esper.util;

using XLR8.CGLib;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    ///     Implementation of the EventType interface for handling JavaBean-type classes.
    /// </summary>
    public class BeanEventType
        : EventTypeSPI
        , NativeEventType
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Type _clazz;
        private readonly string _copyMethodName;
        private readonly string _endTimestampPropertyName;
        private readonly EventAdapterService _eventAdapterService;
        private readonly int _eventTypeId;
        private readonly string _factoryMethodName;
        private readonly EventTypeMetadata _metadata;
        private readonly ConfigurationEventTypeLegacy _optionalLegacyDef;
        private readonly IDictionary<string, EventPropertyGetter> _propertyGetterCache;
        private readonly PropertyResolutionStyle _propertyResolutionStyle;
        private readonly string _startTimestampPropertyName;
        private ISet<EventType> _deepSuperTypes;
        private FastClass _fastClass;
        private IDictionary<string, InternalEventPropDescriptor> _indexedPropertyDescriptors;
        private IDictionary<string, IList<SimplePropertyInfo>> _indexedSmartPropertyTable;
        private IDictionary<string, InternalEventPropDescriptor> _mappedPropertyDescriptors;
        private IDictionary<string, IList<SimplePropertyInfo>> _mappedSmartPropertyTable;
        private IDictionary<string, EventPropertyDescriptor> _propertyDescriptorMap;
        private EventPropertyDescriptor[] _propertyDescriptors;
        private string[] _propertyNames;
        private IDictionary<string, SimplePropertyInfo> _simpleProperties;
        private IDictionary<string, IList<SimplePropertyInfo>> _simpleSmartPropertyTable;
        private EventType[] _superTypes;
        private EventPropertyDescriptor[] _writeablePropertyDescriptors;
        private IDictionary<string, Pair<EventPropertyDescriptor, BeanEventPropertyWriter>> _writerMap;

        /// <summary>
        ///     Constructor takes a class as an argument.
        /// </summary>
        /// <param name="clazz">is the class of a java bean or other POJO</param>
        /// <param name="optionalLegacyDef">optional configuration supplying legacy event type information</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <param name="metadata">event type metadata</param>
        /// <param name="eventTypeId">type id</param>
        public BeanEventType(
            EventTypeMetadata metadata,
            int eventTypeId,
            Type clazz,
            EventAdapterService eventAdapterService,
            ConfigurationEventTypeLegacy optionalLegacyDef)
        {
            _metadata = metadata;
            _clazz = clazz;
            _eventAdapterService = eventAdapterService;
            _optionalLegacyDef = optionalLegacyDef;
            _eventTypeId = eventTypeId;
            if (optionalLegacyDef != null)
            {
                _factoryMethodName = optionalLegacyDef.FactoryMethod;
                _copyMethodName = optionalLegacyDef.CopyMethod;
                _propertyResolutionStyle = optionalLegacyDef.PropertyResolutionStyle;
            }
            else
            {
                _propertyResolutionStyle = eventAdapterService.BeanEventTypeFactory.DefaultPropertyResolutionStyle;
            }
            _propertyGetterCache = new Dictionary<string, EventPropertyGetter>();

            Initialize(false, eventAdapterService.EngineImportService);

            EventTypeUtility.TimestampPropertyDesc desc = EventTypeUtility.ValidatedDetermineTimestampProps(
                this, 
                optionalLegacyDef == null ? null : optionalLegacyDef.StartTimestampPropertyName,
                optionalLegacyDef == null ? null : optionalLegacyDef.EndTimestampPropertyName, _superTypes);
            _startTimestampPropertyName = desc.Start;
            _endTimestampPropertyName = desc.End;
        }

        private bool UsesSmartResolutionStyle
        {
            get
            {
                return (PropertyResolutionStyle == PropertyResolutionStyle.CASE_INSENSITIVE) ||
                       (PropertyResolutionStyle == PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE);
            }
        }

        public ConfigurationEventTypeLegacy OptionalLegacyDef
        {
            get { return _optionalLegacyDef; }
        }

        /// <summary>
        ///     Returns the factory methods name, or null if none defined.
        /// </summary>
        /// <value>factory methods name</value>
        public string FactoryMethodName
        {
            get { return _factoryMethodName; }
        }

        /// <summary>
        ///     Returns the property resolution style.
        /// </summary>
        /// <value>property resolution style</value>
        public PropertyResolutionStyle PropertyResolutionStyle
        {
            get { return _propertyResolutionStyle; }
        }

        public EventType[] DeepSuperTypes
        {
            get { return _deepSuperTypes.ToArray(); }
        }

        /// <summary>
        ///     Returns the fast class reference, if code generation is used for this type, else null.
        /// </summary>
        /// <value>fast class, or null if no code generation</value>
        public FastClass FastClass
        {
            get { return _fastClass; }
        }

        public string StartTimestampPropertyName
        {
            get { return _startTimestampPropertyName; }
        }

        public string EndTimestampPropertyName
        {
            get { return _endTimestampPropertyName; }
        }

        public string Name
        {
            get { return _metadata.PublicName; }
        }

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            return _propertyDescriptorMap.Get(propertyName);
        }

        public int EventTypeId
        {
            get { return _eventTypeId; }
        }

        public Type GetPropertyType(string propertyName)
        {
            SimplePropertyInfo simpleProp = GetSimplePropertyInfo(propertyName);
            if ((simpleProp != null) && (simpleProp.Clazz != null))
            {
                return simpleProp.Clazz;
            }

            Property prop = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (prop is SimpleProperty)
            {
                // there is no such property since it wasn't in simplePropertyTypes
                return null;
            }
            return prop.GetPropertyType(this, _eventAdapterService);
        }

        public bool IsProperty(string propertyName)
        {
            if (GetPropertyType(propertyName) == null)
            {
                return false;
            }
            return true;
        }

        public Type UnderlyingType
        {
            get { return _clazz; }
        }

        public EventPropertyGetter GetGetter(string propertyName)
        {
            EventPropertyGetter cachedGetter = _propertyGetterCache.Get(propertyName);
            if (cachedGetter != null)
            {
                return cachedGetter;
            }

            SimplePropertyInfo simpleProp = GetSimplePropertyInfo(propertyName);
            if ((simpleProp != null) && (simpleProp.Getter != null))
            {
                EventPropertyGetter getterX = simpleProp.Getter;
                _propertyGetterCache.Put(propertyName, getterX);
                return getterX;
            }

            Property prop = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (prop is SimpleProperty)
            {
                // there is no such property since it wasn't in simplePropertyGetters
                return null;
            }

            EventPropertyGetter getter = prop.GetGetter(this, _eventAdapterService);
            _propertyGetterCache.Put(propertyName, getter);
            return getter;
        }

        public EventPropertyGetterMapped GetGetterMapped(string mappedPropertyName)
        {
            EventPropertyDescriptor desc = _propertyDescriptorMap.Get(mappedPropertyName);
            if (desc == null || !desc.IsMapped)
            {
                return null;
            }
            var mappedProperty = new MappedProperty(mappedPropertyName);
            return (EventPropertyGetterMapped) mappedProperty.GetGetter(this, _eventAdapterService);
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedPropertyName)
        {
            EventPropertyDescriptor desc = _propertyDescriptorMap.Get(indexedPropertyName);
            if (desc == null || !desc.IsIndexed)
            {
                return null;
            }
            var indexedProperty = new IndexedProperty(indexedPropertyName);
            return (EventPropertyGetterIndexed) indexedProperty.GetGetter(this, _eventAdapterService);
        }

        public string[] PropertyNames
        {
            get { return _propertyNames; }
        }

        public EventType[] SuperTypes
        {
            get { return _superTypes; }
        }

        public EventTypeMetadata Metadata
        {
            get { return _metadata; }
        }

        public IList<EventPropertyDescriptor> PropertyDescriptors
        {
            get { return _propertyDescriptors; }
        }

        public FragmentEventType GetFragmentType(string propertyExpression)
        {
            SimplePropertyInfo simpleProp = GetSimplePropertyInfo(propertyExpression);
            if ((simpleProp != null) && (simpleProp.Clazz != null))
            {
                GenericPropertyDesc genericProp = simpleProp.Descriptor.GetReturnTypeGeneric();
                return EventBeanUtility.CreateNativeFragmentType(
                    genericProp.GenericType, genericProp.Generic, _eventAdapterService);
            }

            Property prop = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
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
            return EventBeanUtility.CreateNativeFragmentType(
                genericPropertyDesc.GenericType, genericPropertyDesc.Generic, _eventAdapterService);
        }

        public EventPropertyDescriptor GetWritableProperty(string propertyName)
        {
            if (_writeablePropertyDescriptors == null)
            {
                InitializeWriters();
            }
            Pair<EventPropertyDescriptor, BeanEventPropertyWriter> pair = _writerMap.Get(propertyName);
            if (pair != null)
            {
                return pair.First;
            }

            Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty)
            {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }
                var mapProp = (MappedProperty) property;
                return new EventPropertyDescriptor(
                    mapProp.PropertyNameAtomic, typeof (Object), null, false, true, false, true, false);
            }
            if (property is IndexedProperty)
            {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null)
                {
                    return null;
                }
                var indexedProp = (IndexedProperty) property;
                return new EventPropertyDescriptor(
                    indexedProp.PropertyNameAtomic, typeof (Object), null, true, false, true, false, false);
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

        public EventBeanReader Reader
        {
            get { return new BeanEventBeanReader(this); }
        }

        public EventBeanCopyMethod GetCopyMethod(String[] properties)
        {
            if (_copyMethodName == null)
            {
                if (_clazz.IsSerializable)
                {
                    return new BeanEventBeanSerializableCopyMethod(this, _eventAdapterService);
                }

                if (_clazz.IsInterface)
                {
                    return new BeanEventBeanSerializableCopyMethod(this, _eventAdapterService);
                }

                return null;
            }
            MethodInfo method = null;
            try
            {
                method = _clazz.GetMethod(_copyMethodName);
            }
            catch (AmbiguousMatchException e)
            {
                Log.Error("Configured copy-method for class '" + UnderlyingType.Name + " not found by name '" + _copyMethodName + "': " + e.Message);
            }

            if (method == null)
            {
                if (_clazz.IsSerializable)
                {
                    return new BeanEventBeanSerializableCopyMethod(this, _eventAdapterService);
                }

                throw new EPException("Configured copy-method for class '" + _clazz.Name + " not found by name '" + _copyMethodName + "' and type is not Serializable");
            }

            return new BeanEventBeanConfiguredCopyMethod(this, _eventAdapterService, FastClass.GetMethod(method));
        }

        public EventBeanWriter GetWriter(string[] properties)
        {
            if (_writeablePropertyDescriptors == null)
            {
                InitializeWriters();
            }

            var writers = new BeanEventPropertyWriter[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                var pair = _writerMap.Get(properties[i]);
                if (pair != null)
                {
                    writers[i] = pair.Second;
                }
                else
                {
                    writers[i] = (BeanEventPropertyWriter) GetWriter(properties[i]);
                }
            }
            return new BeanEventBeanWriter(writers);
        }

        public bool EqualsCompareType(EventType eventType)
        {
            return this == eventType;
        }

        private static EventType[] GetSuperTypes(Type clazz, BeanEventTypeFactory beanEventTypeFactory)
        {
            var superclasses = new List<Type>();

            // add superclass
            Type superClass = clazz.BaseType;
            if (superClass != null)
            {
                superclasses.Add(superClass);
            }

            // add interfaces
            Type[] interfaces = clazz.GetInterfaces();
            superclasses.AddAll(interfaces);

            // Build event types, ignoring java language types
            var superTypes = new List<EventType>();
            foreach (Type superclass in superclasses)
            {
                if (!superclass.Name.StartsWith("java"))
                {
                    EventType superType = beanEventTypeFactory.CreateBeanType(
                        superclass.Name, superclass, false, false, false);
                    superTypes.Add(superType);
                }
            }

            return superTypes.ToArray();
        }

        /// <summary>
        ///     Add the given class's implemented interfaces and superclasses to the result set of classes.
        /// </summary>
        /// <param name="clazz">to introspect</param>
        /// <param name="result">to add classes to</param>
        internal static void GetBase(Type clazz, ISet<Type> result)
        {
            GetSuperInterfaces(clazz, result);
            GetSuperClasses(clazz, result);
        }

        private static void GetSuperInterfaces(Type clazz, ISet<Type> result)
        {
            Type[] interfaces = clazz.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++)
            {
                result.Add(interfaces[i]);
                GetSuperInterfaces(interfaces[i], result);
            }
        }

        private static void GetSuperClasses(Type clazz, ISet<Type> result)
        {
            Type superClass = clazz.BaseType;
            if (superClass == null)
            {
                return;
            }

            result.Add(superClass);
            GetBase(superClass, result);
        }

        private static void RemoveLibInterfaces(ICollection<Type> types)
        {
            Assembly coreAssembly = typeof (Object).Assembly;
            Type[] coreTypes = types
                .Where(type => type.Assembly == coreAssembly)
                .ToArray();

            foreach (Type type in coreTypes)
            {
                types.Remove(type);
            }
        }

        /// <summary>
        ///     Looks up and returns a cached simple property's descriptor.
        /// </summary>
        /// <param name="propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public InternalEventPropDescriptor GetSimpleProperty(string propertyName)
        {
            SimplePropertyInfo simpleProp = GetSimplePropertyInfo(propertyName);
            if (simpleProp != null)
            {
                return simpleProp.Descriptor;
            }
            return null;
        }

        /// <summary>
        ///     Looks up and returns a cached mapped property's descriptor.
        /// </summary>
        /// <param name="propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public InternalEventPropDescriptor GetMappedProperty(string propertyName)
        {
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_SENSITIVE))
            {
                return _mappedPropertyDescriptors.Get(propertyName);
            }
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_INSENSITIVE))
            {
                IList<SimplePropertyInfo> propertyInfos = _mappedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                return propertyInfos != null
                    ? propertyInfos[0].Descriptor
                    : null;
            }
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE))
            {
                IList<SimplePropertyInfo> propertyInfos = _mappedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                if (propertyInfos != null)
                {
                    if (propertyInfos.Count != 1)
                    {
                        throw new EPException(
                            "Unable to determine which property to use for \"" + propertyName +
                            "\" because more than one property matched");
                    }

                    return propertyInfos[0].Descriptor;
                }
            }
            return null;
        }

        /// <summary>
        ///     Looks up and returns a cached indexed property's descriptor.
        /// </summary>
        /// <param name="propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public InternalEventPropDescriptor GetIndexedProperty(string propertyName)
        {
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_SENSITIVE))
            {
                return _indexedPropertyDescriptors.Get(propertyName);
            }
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_INSENSITIVE))
            {
                IList<SimplePropertyInfo> propertyInfos = _indexedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                return propertyInfos != null
                    ? propertyInfos[0].Descriptor
                    : null;
            }
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE))
            {
                IList<SimplePropertyInfo> propertyInfos = _indexedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                if (propertyInfos != null)
                {
                    if (propertyInfos.Count != 1)
                    {
                        throw new EPException(
                            "Unable to determine which property to use for \"" + propertyName +
                            "\" because more than one property matched");
                    }

                    return propertyInfos[0].Descriptor;
                }
            }
            return null;
        }

        public override String ToString()
        {
            return "BeanEventType" +
                   " name=" + Name +
                   " clazz=" + _clazz.Name;
        }

        private void Initialize(bool isConfigured, EngineImportService engineImportService)
        {
            PropertyListBuilder propertyListBuilder = PropertyListBuilderFactory.CreateBuilder(_optionalLegacyDef);
            IList<InternalEventPropDescriptor> properties = propertyListBuilder.AssessProperties(_clazz);

            _propertyDescriptors = new EventPropertyDescriptor[properties.Count];
            _propertyDescriptorMap = new Dictionary<string, EventPropertyDescriptor>();
            _propertyNames = new string[properties.Count];
            _simpleProperties = new Dictionary<string, SimplePropertyInfo>();
            _mappedPropertyDescriptors = new Dictionary<string, InternalEventPropDescriptor>();
            _indexedPropertyDescriptors = new Dictionary<string, InternalEventPropDescriptor>();

            if (UsesSmartResolutionStyle)
            {
                _simpleSmartPropertyTable = new Dictionary<string, IList<SimplePropertyInfo>>();
                _mappedSmartPropertyTable = new Dictionary<string, IList<SimplePropertyInfo>>();
                _indexedSmartPropertyTable = new Dictionary<string, IList<SimplePropertyInfo>>();
            }

            if ((_optionalLegacyDef == null) ||
                (_optionalLegacyDef.CodeGeneration != CodeGenerationEnum.DISABLED))
            {
                // get CGLib fast class using current thread class loader
                _fastClass = null;
                try
                {
                    _fastClass = FastClass.Create(engineImportService.GetFastClassClassLoader(_clazz), _clazz);
                }
                catch (Exception exWithThreadClassLoader)
                {
                    // get CGLib fast class based on given class (for OSGI support)
                    try
                    {
                        _fastClass = FastClass.Create(_clazz);
                    }
                    catch (Exception exWithoutThreadClassLoader)
                    {
                        Log.Warn(
                            ".initialize Unable to obtain CGLib fast class and/or method implementation for class " +
                            _clazz.Name + ", error msg is " + exWithThreadClassLoader.Message, exWithThreadClassLoader);
                        Log.Warn(
                            ".initialize Not using the provided class loader, the error msg is: " +
                            exWithoutThreadClassLoader.Message, exWithoutThreadClassLoader);
                        _fastClass = null;
                    }
                }
            }

            int count = 0;
            foreach (InternalEventPropDescriptor desc in properties)
            {
                string propertyName = desc.PropertyName;
                Type underlyingType;
                Type componentType;
                bool isRequiresIndex;
                bool isRequiresMapkey;
                bool isIndexed;
                bool isMapped;
                bool isFragment;

                if (desc.PropertyType.Equals(EventPropertyType.SIMPLE))
                {
                    EventPropertyGetter getter;
                    Type type;
                    if (desc.ReadMethod != null)
                    {
                        getter = PropertyHelper.GetGetter(desc.ReadMethod, _fastClass, _eventAdapterService);
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
                        // Type genericType = TypeHelper.GetGenericReturnTypeMap(desc.ReadMethod, desc.AccessorField);
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
                        isFragment = TypeHelper.IsFragmentableType(type.GetElementType());
                        componentType = type.GetElementType();
                    }
                    else if (type.IsImplementsInterface(typeof (IEnumerable)))
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
                            componentType = typeof (Object);
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
                        string smartPropertyName = propertyName.ToLowerInvariant();
                        IList<SimplePropertyInfo> propertyInfoList = _simpleSmartPropertyTable.Get(smartPropertyName);
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
                else if (desc.PropertyType.Equals(EventPropertyType.MAPPED))
                {
                    _mappedPropertyDescriptors.Put(propertyName, desc);

                    underlyingType = desc.ReturnType;
                    componentType = typeof (Object);
                    isRequiresIndex = false;
                    isRequiresMapkey = desc.ReadMethod.GetParameterTypes().Length > 0;
                    isIndexed = false;
                    isMapped = true;
                    isFragment = false;

                    // Recognize that there may be properties with overlapping case-insentitive names
                    if (UsesSmartResolutionStyle)
                    {
                        // Find the property in the smart property table
                        string smartPropertyName = propertyName.ToLowerInvariant();
                        IList<SimplePropertyInfo> propertyInfoList = _mappedSmartPropertyTable.Get(smartPropertyName);
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
                else if (desc.PropertyType.Equals(EventPropertyType.INDEXED))
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
                        string smartPropertyName = propertyName.ToLowerInvariant();
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

                _propertyNames[count] = desc.PropertyName;
                var descriptor = new EventPropertyDescriptor(
                    desc.PropertyName,
                    underlyingType, componentType, isRequiresIndex, isRequiresMapkey, isIndexed, isMapped, isFragment);
                _propertyDescriptors[count++] = descriptor;
                _propertyDescriptorMap.Put(descriptor.PropertyName, descriptor);
            }

            // Determine event type super types
            _superTypes = GetSuperTypes(_clazz, _eventAdapterService.BeanEventTypeFactory);
            if (_superTypes != null && _superTypes.Length == 0)
            {
                _superTypes = null;
            }
            if (_metadata != null && _metadata.TypeClass == TypeClass.NAMED_WINDOW)
            {
                _superTypes = null;
            }

            // Determine deep supertypes
            // Get Java super types (superclasses and interfaces), deep get of all in the tree
            var supers = new HashSet<Type>();
            GetBase(_clazz, supers);
            RemoveLibInterfaces(supers); // Remove "java." super types

            // Cache the supertypes of this event type for later use
            _deepSuperTypes = new HashSet<EventType>();
            foreach (Type superClass in supers)
            {
                EventType superType = _eventAdapterService.BeanEventTypeFactory.CreateBeanType(
                    superClass.Name, superClass, false, false, isConfigured);
                _deepSuperTypes.Add(superType);
            }
        }

        private SimplePropertyInfo GetSimplePropertyInfo(string propertyName)
        {
            SimplePropertyInfo propertyInfo;
            IList<SimplePropertyInfo> simplePropertyInfoList;

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_SENSITIVE))
            {
                return _simpleProperties.Get(propertyName);
            }
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_INSENSITIVE))
            {
                propertyInfo = _simpleProperties.Get(propertyName);
                if (propertyInfo != null)
                {
                    return propertyInfo;
                }

                simplePropertyInfoList = _simpleSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                return
                    simplePropertyInfoList != null
                        ? simplePropertyInfoList[0]
                        : null;
            }
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE))
            {
                propertyInfo = _simpleProperties.Get(propertyName);
                if (propertyInfo != null)
                {
                    return propertyInfo;
                }

                simplePropertyInfoList = _simpleSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                if (simplePropertyInfoList != null)
                {
                    if (simplePropertyInfoList.Count != 1)
                    {
                        throw new EPException(
                            "Unable to determine which property to use for \"" + propertyName +
                            "\" because more than one property matched");
                    }

                    return simplePropertyInfoList[0];
                }
            }

            return null;
        }

        public EventPropertyWriter GetWriter(string propertyName)
        {
            if (_writeablePropertyDescriptors == null)
            {
                InitializeWriters();
            }
            Pair<EventPropertyDescriptor, BeanEventPropertyWriter> pair = _writerMap.Get(propertyName);
            if (pair != null)
            {
                return pair.Second;
            }

            Property property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty)
            {
                var mapProp = (MappedProperty) property;
                var methodName = PropertyHelper.GetSetterMethodName(mapProp.PropertyNameAtomic);
                MethodInfo setterMethod;
                try
                {
                    setterMethod = MethodResolver.ResolveMethod(
                        _clazz, methodName, new Type[] { typeof (string), typeof (object) }, true, new bool[2], new bool[2]);
                }
                catch (EngineNoSuchMethodException e)
                {
                    Log.Info(
                        "Failed to find mapped property setter method '" + methodName + "' for writing to property '" +
                        propertyName + "' taking {string, Object} as parameters");
                    return null;
                }
                if (setterMethod == null)
                {
                    return null;
                }
                FastMethod fastMethod = _fastClass.GetMethod(setterMethod);
                return new BeanEventPropertyWriterMapProp(_clazz, fastMethod, mapProp.Key);
            }

            if (property is IndexedProperty)
            {
                var indexedProp = (IndexedProperty) property;
                var methodName = PropertyHelper.GetSetterMethodName(indexedProp.PropertyNameAtomic);
                MethodInfo setterMethod;
                try
                {
                    // setterMethod = UnderlyingType.GetMethod(
                    //    methodName, BindingFlags.Public | BindingFlags.Instance, null,
                    //    new Type[] { typeof(int), typeof(object) }, null);

                    setterMethod = MethodResolver.ResolveMethod(
                        _clazz, methodName, new Type[] { typeof (int), typeof (object) }, true, new bool[2], new bool[2]);
                }
                catch (EngineNoSuchMethodException)
                {
                    Log.Info(
                        "Failed to find indexed property setter method '" + methodName + "' for writing to property '" +
                        propertyName + "' taking {int, Object} as parameters");
                    return null;
                }
                if (setterMethod == null)
                {
                    return null;
                }
                FastMethod fastMethod = _fastClass.GetMethod(setterMethod);
                return new BeanEventPropertyWriterIndexedProp(_clazz, fastMethod, indexedProp.Index);
            }

            return null;
        }

        private void InitializeWriters()
        {
            var writables = PropertyHelper.GetWritableProperties(FastClass.TargetType);
            var desc = new EventPropertyDescriptor[writables.Count];
            var writers = new Dictionary<String, Pair<EventPropertyDescriptor, BeanEventPropertyWriter>>();

            int count = 0;
            foreach (WriteablePropertyDescriptor writable in writables)
            {
                var propertyDesc = new EventPropertyDescriptor(
                    writable.PropertyName, writable.PropertyType, null, false, false, false, false, false);
                desc[count++] = propertyDesc;

                FastMethod fastMethod = FastClass.GetMethod(writable.WriteMethod);
                writers.Put(
                    writable.PropertyName,
                    new Pair<EventPropertyDescriptor, BeanEventPropertyWriter>(
                        propertyDesc, new BeanEventPropertyWriter(_clazz, fastMethod)));
            }

            _writerMap = writers;
            _writeablePropertyDescriptors = desc;
        }

        /// <summary>
        ///     Descriptor caching the getter, class and property INFO.
        /// </summary>
        public class SimplePropertyInfo
        {
            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="clazz">is the class</param>
            /// <param name="getter">is the getter</param>
            /// <param name="descriptor">is the property INFO</param>
            public SimplePropertyInfo(Type clazz, EventPropertyGetter getter, InternalEventPropDescriptor descriptor)
            {
                Clazz = clazz;
                Getter = getter;
                Descriptor = descriptor;
            }

            /// <summary>
            ///     Returns the return type.
            /// </summary>
            /// <value>return type</value>
            public Type Clazz { get; private set; }

            /// <summary>
            ///     Returns the getter.
            /// </summary>
            /// <value>getter</value>
            public EventPropertyGetter Getter { get; private set; }

            /// <summary>
            ///     Returns the property INFO.
            /// </summary>
            /// <value>property INFO</value>
            public InternalEventPropDescriptor Descriptor { get; private set; }
        }
    }
} // end of namespace