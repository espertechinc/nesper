///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;

using PropertyInfo = System.Reflection.PropertyInfo;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    /// Implementation of the EventType interface for handling JavaBean-type classes.
    /// </summary>
    public class BeanEventType : EventTypeSPI,
        NativeEventType
    {
        private readonly IContainer _container;
        private readonly BeanEventTypeStem _stem;
        private EventTypeMetadata _metadata;
        private readonly BeanEventTypeFactory _beanEventTypeFactory;
        private readonly EventType[] _superTypes;
        private readonly ICollection<EventType> _deepSuperTypes;
        private readonly string _startTimestampPropertyName;
        private readonly string _endTimestampPropertyName;

        private readonly IDictionary<string, EventPropertyGetterSPI> _propertyGetterCache =
            new Dictionary<string, EventPropertyGetterSPI>();

        private EventPropertyDescriptor[] _writeablePropertyDescriptors;
        private IDictionary<string, Pair<EventPropertyDescriptor, BeanEventPropertyWriter>> _writerMap;

        public BeanEventType(
            IContainer container,
            BeanEventTypeStem stem,
            EventTypeMetadata metadata,
            BeanEventTypeFactory beanEventTypeFactory,
            EventType[] superTypes,
            ICollection<EventType> deepSuperTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName)
        {
            _container = container;
            _stem = stem;
            _metadata = metadata;
            _beanEventTypeFactory = beanEventTypeFactory;
            _superTypes = superTypes;
            _deepSuperTypes = deepSuperTypes;
            var desc = EventTypeUtility.ValidatedDetermineTimestampProps(
                this,
                startTimestampPropertyName,
                endTimestampPropertyName,
                superTypes);
            _startTimestampPropertyName = desc.Start;
            _endTimestampPropertyName = desc.End;
        }

        public void SetMetadataId(
            long publicId,
            long protectedId)
        {
            _metadata = _metadata.WithIds(publicId, protectedId);
        }

        public string StartTimestampPropertyName => _startTimestampPropertyName;
        public string EndTimestampPropertyName => _endTimestampPropertyName;
        public string Name => _metadata.Name;

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            return _stem.PropertyDescriptorMap.Get(propertyName);
        }

        /// <summary>
        /// Returns the factory methods name, or null if none defined.
        /// </summary>
        /// <value>factory methods name</value>
        public string FactoryMethodName => _stem.OptionalLegacyDef?.FactoryMethod;

        public Type GetPropertyType(string propertyName)
        {
            var type = SimplePropertyType(propertyName);
            if (type != null) {
                return type;
            }

            var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (prop is SimpleProperty) {
                // unescaped lookup
                return SimplePropertyType(prop.PropertyNameAtomic);
            }

            return prop.GetPropertyType(this, _beanEventTypeFactory);
        }

        public bool IsProperty(string propertyName)
        {
            if (GetPropertyType(propertyName) == null) {
                return false;
            }

            return true;
        }

        public Type UnderlyingType => _stem.Clazz;

        public EventPropertyGetterSPI GetGetterSPI(string propertyName)
        {
            var cachedGetter = _propertyGetterCache.Get(propertyName);
            if (cachedGetter != null) {
                return cachedGetter;
            }

            var getter = SimplePropertyGetter(propertyName);
            if (getter != null) {
                return getter;
            }

            var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (prop is SimpleProperty) {
                // unescpaped lookup
                return SimplePropertyGetter(prop.PropertyNameAtomic);
            }

            getter = prop.GetGetter(this, _beanEventTypeFactory.EventBeanTypedEventFactory, _beanEventTypeFactory);
            _propertyGetterCache.Put(propertyName, getter);
            return getter;
        }

        public EventPropertyGetter GetGetter(string propertyName)
        {
            return GetGetterSPI(propertyName);
        }

        public EventPropertyGetterMapped GetGetterMapped(string mappedPropertyName)
        {
            return GetGetterMappedSPI(mappedPropertyName);
        }

        public EventPropertyGetterMappedSPI GetGetterMappedSPI(string propertyName)
        {
            var desc = _stem.PropertyDescriptorMap.Get(propertyName);
            if (desc == null || !desc.IsMapped) {
                return null;
            }

            var mappedProperty = new MappedProperty(propertyName);
            return (EventPropertyGetterMappedSPI) mappedProperty.GetGetter(
                this,
                _beanEventTypeFactory.EventBeanTypedEventFactory,
                _beanEventTypeFactory);
        }

        public EventPropertyGetterIndexed GetGetterIndexed(string indexedPropertyName)
        {
            return GetGetterIndexedSPI(indexedPropertyName);
        }

        public EventPropertyGetterIndexedSPI GetGetterIndexedSPI(string indexedPropertyName)
        {
            var desc = _stem.PropertyDescriptorMap.Get(indexedPropertyName);
            if (desc == null || !desc.IsIndexed) {
                return null;
            }

            var indexedProperty = new IndexedProperty(indexedPropertyName);
            return (EventPropertyGetterIndexedSPI) indexedProperty.GetGetter(
                this,
                _beanEventTypeFactory.EventBeanTypedEventFactory,
                _beanEventTypeFactory);
        }

        /// <summary>
        /// Looks up and returns a cached simple property's descriptor.
        /// </summary>
        /// <param name = "propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public PropertyStem GetSimpleProperty(string propertyName)
        {
            var simpleProp = GetSimplePropertyInfo(propertyName);
            if (simpleProp != null) {
                return simpleProp.Descriptor;
            }

            return null;
        }

        /// <summary>
        /// Looks up and returns a cached mapped property's descriptor.
        /// </summary>
        /// <param name = "propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public PropertyStem GetMappedProperty(string propertyName)
        {
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_SENSITIVE)) {
                return _stem.MappedPropertyDescriptors.Get(propertyName);
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_INSENSITIVE)) {
                var propertyInfos = _stem.MappedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                return propertyInfos?[0].Descriptor;
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE)) {
                var propertyInfos = _stem.MappedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                if (propertyInfos != null) {
                    if (propertyInfos.Count != 1) {
                        throw new EPException(
                            "Unable to determine which property to use for \"" +
                            propertyName +
                            "\" because more than one property matched");
                    }

                    return propertyInfos[0].Descriptor;
                }
            }

            return null;
        }

        /// <summary>
        /// Looks up and returns a cached indexed property's descriptor.
        /// </summary>
        /// <param name = "propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public PropertyStem GetIndexedProperty(string propertyName)
        {
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_SENSITIVE)) {
                return _stem.IndexedPropertyDescriptors.Get(propertyName);
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_INSENSITIVE)) {
                var propertyInfos = _stem.IndexedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                return propertyInfos?[0].Descriptor;
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE)) {
                var propertyInfos = _stem.IndexedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                if (propertyInfos != null) {
                    if (propertyInfos.Count != 1) {
                        throw new EPException(
                            "Unable to determine which property to use for \"" +
                            propertyName +
                            "\" because more than one property matched");
                    }

                    return propertyInfos[0].Descriptor;
                }
            }

            return null;
        }

        public string[] PropertyNames => _stem.PropertyNames;
        public IList<EventType> SuperTypes => _superTypes;
        public IEnumerable<EventType> DeepSuperTypes => _deepSuperTypes;

        public override string ToString()
        {
            return "BeanEventType" + " name=" + Name + " clazz=" + _stem.Clazz.CleanName();
        }

        private introspect.PropertyInfo GetSimplePropertyInfo(string propertyName)
        {
            introspect.PropertyInfo propertyInfo;
            IList<introspect.PropertyInfo> simplePropertyInfoList;
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_SENSITIVE)) {
                return _stem.SimpleProperties.Get(propertyName);
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_INSENSITIVE)) {
                propertyInfo = _stem.SimpleProperties.Get(propertyName);
                if (propertyInfo != null) {
                    return propertyInfo;
                }

                simplePropertyInfoList = _stem.SimpleSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                return simplePropertyInfoList?[0];
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE)) {
                propertyInfo = _stem.SimpleProperties.Get(propertyName);
                if (propertyInfo != null) {
                    return propertyInfo;
                }

                simplePropertyInfoList = _stem.SimpleSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                if (simplePropertyInfoList != null) {
                    if (simplePropertyInfoList.Count != 1) {
                        throw new EPException(
                            "Unable to determine which property to use for \"" +
                            propertyName +
                            "\" because more than one property matched");
                    }

                    return simplePropertyInfoList[0];
                }
            }

            return null;
        }

        public EventTypeMetadata Metadata => _metadata;
        public IList<EventPropertyDescriptor> PropertyDescriptors => _stem.PropertyDescriptors;

        public FragmentEventType GetFragmentType(string propertyExpression)
        {
            var fragmentEventType = SimplePropertyFragmentType(propertyExpression);
            if (fragmentEventType != null) {
                return fragmentEventType;
            }

            var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyExpression);
            if (prop is SimpleProperty) {
                // unescaped lookup
                return SimplePropertyFragmentType(prop.PropertyNameAtomic);
            }

            var type = prop.GetPropertyType(this, _beanEventTypeFactory);
            if (type == null) {
                return null;
            }

            return EventBeanUtility.CreateNativeFragmentType(type, _beanEventTypeFactory, _stem.IsPublicFields);
        }

        public EventPropertyWriterSPI GetWriter(string propertyName)
        {
            if (_writeablePropertyDescriptors == null) {
                InitializeWriters();
            }

            var writer = SimplePropertyWriter(propertyName);
            if (writer != null) {
                return writer;
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is SimpleProperty) {
                // unescaped lookup
                return SimplePropertyWriter(property.PropertyNameAtomic);
            }

            if (property is MappedProperty mapProp) {
                var methodName = PropertyHelper.GetSetterMethodName(mapProp.PropertyNameAtomic);
                MethodInfo setterMethod;
                try {
                    setterMethod = MethodResolver.ResolveMethod(
                        _stem.Clazz,
                        methodName,
                        new Type[] { typeof(string), typeof(object) },
                        true,
                        new bool[2],
                        new bool[2]);
                }
                catch (MethodResolverNoSuchMethodException) {
                    Log.Info(
                        "Failed to find mapped property setter method '" +
                        methodName +
                        "' for writing to property '" +
                        propertyName +
                        "' taking {String, Object} as parameters");
                    return null;
                }

                return new BeanEventPropertyWriterMapProp(_stem.Clazz, setterMethod, mapProp.Key);
            }

            if (property is IndexedProperty indexedProp) {
                var methodName = PropertyHelper.GetSetterMethodName(indexedProp.PropertyNameAtomic);
                MethodInfo setterMethod;
                try {
                    setterMethod = MethodResolver.ResolveMethod(
                        _stem.Clazz,
                        methodName,
                        new Type[] { typeof(int), typeof(object) },
                        true,
                        new bool[2],
                        new bool[2]);
                }
                catch (MethodResolverNoSuchMethodException) {
                    Log.Info(
                        "Failed to find indexed property setter method '" +
                        methodName +
                        "' for writing to property '" +
                        propertyName +
                        "' taking {int, Object} as parameters");
                    return null;
                }

                return new BeanEventPropertyWriterIndexedProp(_stem.Clazz, setterMethod, indexedProp.Index);
            }

            return null;
        }

        public EventPropertyDescriptor GetWritableProperty(string propertyName)
        {
            if (_writeablePropertyDescriptors == null) {
                InitializeWriters();
            }

            var pair = _writerMap.Get(propertyName);
            if (pair != null) {
                return pair.First;
            }

            var property = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (property is MappedProperty mapProp) {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null) {
                    return null;
                }

                return new EventPropertyDescriptor(
                    mapProp.PropertyNameAtomic,
                    typeof(object),
                    false,
                    true,
                    false,
                    true,
                    false);
            }

            if (property is IndexedProperty indexedProp) {
                EventPropertyWriter writer = GetWriter(propertyName);
                if (writer == null) {
                    return null;
                }

                return new EventPropertyDescriptor(
                    indexedProp.PropertyNameAtomic,
                    typeof(object),
                    true,
                    false,
                    true,
                    false,
                    false);
            }

            return null;
        }

        public EventPropertyDescriptor[] WriteableProperties {
            get {
                if (_writeablePropertyDescriptors == null) {
                    InitializeWriters();
                }

                return _writeablePropertyDescriptors;
            }
        }

        public ICollection<EventType> DeepSuperTypesCollection => _deepSuperTypes;

        public EventBeanCopyMethodForge GetCopyMethodForge(string[] properties)
        {
            var copyMethodName = _stem.OptionalLegacyDef?.CopyMethod;
            if (copyMethodName == null) {
                var objectCopier = _container.Resolve<IObjectCopier>();
                if (objectCopier.IsSupported(Stem.Clazz))
                {
                    return new BeanEventBeanObjectCopyMethodForge(
                        this, objectCopier);
                }

                return null;
            }

            MethodInfo method = null;
            try {
                method = _stem.Clazz.GetMethod(copyMethodName);
            }
            catch (Exception e)
                when (e is AmbiguousMatchException || e is ArgumentNullException) {
                Log.Error(
                    "Configured copy-method for class '" +
                    _stem.Clazz.CleanName() +
                    " not found by name '" +
                    copyMethodName +
                    "': " +
                    e.Message);
            }

            if (method == null) {
                if (Stem.Clazz.IsSerializable) {
                    return new BeanEventBeanObjectCopyMethodForge(
                        this,
                        _container.Resolve<SerializableObjectCopier>());
                }

                throw new EPException(
                    "Configured copy-method for class '" +
                    _stem.Clazz.CleanName() +
                    " not found by name '" +
                    copyMethodName +
                    "' and class does not implement Serializable");
            }

            return new BeanEventBeanConfiguredCopyMethodForge(this, method);
        }

        public EventBeanWriter GetWriter(string[] properties)
        {
            if (_writeablePropertyDescriptors == null) {
                InitializeWriters();
            }

            var writers = new BeanEventPropertyWriter[properties.Length];
            for (var i = 0; i < properties.Length; i++) {
                var pair = _writerMap.Get(properties[i]);
                if (pair != null) {
                    writers[i] = pair.Second;
                }
                else {
                    writers[i] = (BeanEventPropertyWriter) GetWriter(properties[i]);
                }
            }

            return new BeanEventBeanWriter(writers);
        }

        public BeanEventTypeStem Stem => _stem;

        public ExprValidationException EqualsCompareType(EventType eventType)
        {
            if (this != eventType) {
                return new ExprValidationException("Bean event types mismatch");
            }

            return null;
        }

        private void InitializeWriters()
        {
            var writables = PropertyHelper.GetWritableProperties(_stem.Clazz);
            var desc = new EventPropertyDescriptor[writables.Count];
            IDictionary<string, Pair<EventPropertyDescriptor, BeanEventPropertyWriter>> writers =
                new Dictionary<string, Pair<EventPropertyDescriptor, BeanEventPropertyWriter>>();
            var count = 0;
            foreach (var writable in writables) {
                var propertyDesc = new EventPropertyDescriptor(
                    writable.PropertyName,
                    writable.PropertyType,
                    false,
                    false,
                    false,
                    false,
                    false);
                desc[count++] = propertyDesc;
                writers.Put(
                    writable.PropertyName,
                    new Pair<EventPropertyDescriptor, BeanEventPropertyWriter>(
                        propertyDesc,
                        new BeanEventPropertyWriter(_stem.Clazz, writable.WriteMember)));
            }

            _writerMap = writers;
            _writeablePropertyDescriptors = desc;
        }

        private Type SimplePropertyType(string propertyName)
        {
            var simpleProp = GetSimplePropertyInfo(propertyName);
            if (simpleProp != null) {
                return simpleProp.Clazz;
            }

            return null;
        }

        private EventPropertyGetterSPI SimplePropertyGetter(string propertyName)
        {
            var simpleProp = GetSimplePropertyInfo(propertyName);
            if (simpleProp != null && simpleProp.GetterFactory != null) {
                var getter = simpleProp.GetterFactory.Make(
                    _beanEventTypeFactory.EventBeanTypedEventFactory,
                    _beanEventTypeFactory);
                _propertyGetterCache.Put(propertyName, getter);
                return getter;
            }

            return null;
        }

        private FragmentEventType SimplePropertyFragmentType(string propertyName)
        {
            var simpleProp = GetSimplePropertyInfo(propertyName);
            if (simpleProp != null) {
                Type type = simpleProp.Descriptor.ReturnType;
                return EventBeanUtility.CreateNativeFragmentType(type, _beanEventTypeFactory, _stem.IsPublicFields);
            }

            return null;
        }

        private BeanEventPropertyWriter SimplePropertyWriter(string propertyName)
        {
            var pair = _writerMap.Get(propertyName);
            if (pair != null) {
                return pair.Second;
            }

            return null;
        }

        public PropertyResolutionStyle PropertyResolutionStyle => _stem.PropertyResolutionStyle;

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace