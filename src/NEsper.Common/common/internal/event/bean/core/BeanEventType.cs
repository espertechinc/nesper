///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.client.util;
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

using PropertyInfo = com.espertech.esper.common.@internal.@event.bean.introspect.PropertyInfo;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    ///     Implementation of the EventType interface for handling classes.
    /// </summary>
    public class BeanEventType : EventTypeSPI,
        NativeEventType
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly BeanEventTypeFactory _beanEventTypeFactory;

        private readonly IContainer _container;

        private readonly IDictionary<string, EventPropertyGetterSPI> _propertyGetterCache =
            new Dictionary<string, EventPropertyGetterSPI>(4);

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
            Stem = stem;
            Metadata = metadata;
            _beanEventTypeFactory = beanEventTypeFactory;
            SuperTypes = superTypes;
            DeepSuperTypesCollection = deepSuperTypes;

            var desc = EventTypeUtility.ValidatedDetermineTimestampProps(
                this,
                startTimestampPropertyName,
                endTimestampPropertyName,
                superTypes);
            StartTimestampPropertyName = desc.Start;
            EndTimestampPropertyName = desc.End;
        }

        /// <summary>
        ///     Returns the factory methods name, or null if none defined.
        /// </summary>
        /// <returns>factory methods name</returns>
        public string FactoryMethodName => Stem.OptionalLegacyDef?.FactoryMethod;

        /// <summary>
        ///     Returns the property resolution style.
        /// </summary>
        /// <returns>property resolution style</returns>
        public PropertyResolutionStyle PropertyResolutionStyle => Stem.PropertyResolutionStyle;

        public BeanEventTypeStem Stem { get; }

        public void SetMetadataId(
            long publicId,
            long protectedId)
        {
            Metadata = Metadata.WithIds(publicId, protectedId);
        }

        public string StartTimestampPropertyName { get; }

        public string EndTimestampPropertyName { get; }

        public string Name => Metadata.Name;

        public EventPropertyDescriptor GetPropertyDescriptor(string propertyName)
        {
            return Stem.PropertyDescriptorMap.Get(propertyName);
        }

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

        public Type UnderlyingType => Stem.Clazz;

        public EventPropertyGetterSPI GetGetterSPI(string propertyName)
        {
            if (_propertyGetterCache.TryGetValue(propertyName, out var cachedGetter))
            { 
                return cachedGetter;
            }
            
            EventPropertyGetterSPI getter = SimplePropertyGetter(propertyName);
            if (getter != null) {
                return getter;
            }

            var prop = PropertyParser.ParseAndWalkLaxToSimple(propertyName);
            if (prop is SimpleProperty) {
                // unescaped lookup
                return SimplePropertyGetter(prop.PropertyNameAtomic);
            }

            getter = prop.GetGetter(this, _beanEventTypeFactory.EventBeanTypedEventFactory, _beanEventTypeFactory);
            _propertyGetterCache[propertyName] = getter;

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
            var desc = Stem.PropertyDescriptorMap.Get(propertyName);
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
            var desc = Stem.PropertyDescriptorMap.Get(indexedPropertyName);
            if (desc == null || !desc.IsIndexed) {
                return null;
            }

            var indexedProperty = new IndexedProperty(indexedPropertyName);
            return (EventPropertyGetterIndexedSPI) indexedProperty.GetGetter(
                this,
                _beanEventTypeFactory.EventBeanTypedEventFactory,
                _beanEventTypeFactory);
        }

        public string[] PropertyNames => Stem.PropertyNames;

        public IList<EventType> SuperTypes { get; }

        public IEnumerable<EventType> DeepSuperTypes => DeepSuperTypesCollection;

        public EventTypeMetadata Metadata { get; private set; }

        public IList<EventPropertyDescriptor> PropertyDescriptors => Stem.PropertyDescriptors;

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

            return EventBeanUtility.CreateNativeFragmentType(
                type,
                _beanEventTypeFactory,
                Stem.IsPublicFields);
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

        public ICollection<EventType> DeepSuperTypesCollection { get; }

        public EventBeanCopyMethodForge GetCopyMethodForge(string[] properties)
        {
            var copyMethodName = Stem.OptionalLegacyDef?.CopyMethod;
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
                method = Stem.Clazz.GetMethod(copyMethodName);
            }
            catch (Exception e)
                when (e is AmbiguousMatchException || e is ArgumentNullException) {
                Log.Error(
                    "Configured copy-method for class '" +
                    Stem.Clazz.Name +
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
                    Stem.Clazz.Name +
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

        public ExprValidationException EqualsCompareType(EventType eventType)
        {
            if (!Equals(this, eventType)) {
                return new ExprValidationException("Bean event types mismatch");
            }

            return null;
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
                        Stem.Clazz,
                        methodName,
                        new[] {typeof(string), typeof(object)},
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

                return new BeanEventPropertyWriterMapProp(Stem.Clazz, setterMethod, mapProp.Key);
            }

            if (property is IndexedProperty) {
                var indexedProp = (IndexedProperty) property;
                var methodName = PropertyHelper.GetSetterMethodName(indexedProp.PropertyNameAtomic);
                MethodInfo setterMethod;
                try {
                    setterMethod = MethodResolver.ResolveMethod(
                        Stem.Clazz,
                        methodName,
                        new[] {typeof(int), typeof(object)},
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

                return new BeanEventPropertyWriterIndexedProp(Stem.Clazz, setterMethod, indexedProp.Index);
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

        /// <summary>
        ///     Looks up and returns a cached simple property's descriptor.
        /// </summary>
        /// <param name="propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public PropertyStem GetSimpleProperty(string propertyName)
        {
            var simpleProp = GetSimplePropertyInfo(propertyName);
            return simpleProp?.Descriptor;
        }

        /// <summary>
        ///     Looks up and returns a cached mapped property's descriptor.
        /// </summary>
        /// <param name="propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public PropertyStem GetMappedProperty(string propertyName)
        {
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_SENSITIVE)) {
                return Stem.MappedPropertyDescriptors.Get(propertyName);
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_INSENSITIVE)) {
                var propertyInfos = Stem.MappedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                return propertyInfos?[0].Descriptor;
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE)) {
                var propertyInfos = Stem.MappedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
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
        ///     Looks up and returns a cached indexed property's descriptor.
        /// </summary>
        /// <param name="propertyName">to look up</param>
        /// <returns>property descriptor</returns>
        public PropertyStem GetIndexedProperty(string propertyName)
        {
            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_SENSITIVE)) {
                return Stem.IndexedPropertyDescriptors.Get(propertyName);
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_INSENSITIVE)) {
                var propertyInfos = Stem.IndexedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                return propertyInfos?[0].Descriptor;
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE)) {
                var propertyInfos = Stem.IndexedSmartPropertyTable.Get(propertyName.ToLowerInvariant());
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

        public override string ToString()
        {
            return "BeanEventType" +
                   " name=" +
                   Name +
                   " clazz=" +
                   Stem.Clazz.TypeSafeName();
        }

        private PropertyInfo GetSimplePropertyInfo(string propertyName)
        {
            PropertyInfo propertyInfo;
            IList<PropertyInfo> simplePropertyInfoList;

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_SENSITIVE)) {
                return Stem.SimpleProperties.Get(propertyName);
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_INSENSITIVE)) {
                propertyInfo = Stem.SimpleProperties.Get(propertyName);
                if (propertyInfo != null) {
                    return propertyInfo;
                }

                simplePropertyInfoList = Stem.SimpleSmartPropertyTable.Get(propertyName.ToLowerInvariant());
                return simplePropertyInfoList?[0];
            }

            if (PropertyResolutionStyle.Equals(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE)) {
                propertyInfo = Stem.SimpleProperties.Get(propertyName);
                if (propertyInfo != null) {
                    return propertyInfo;
                }

                simplePropertyInfoList = Stem.SimpleSmartPropertyTable.Get(propertyName.ToLowerInvariant());
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

        private void InitializeWriters()
        {
            var writables = PropertyHelper.GetWritableProperties(Stem.Clazz);
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
                        new BeanEventPropertyWriter(
                            Stem.Clazz,
                            writable.WriteMember)));
            }

            _writerMap = writers;
            _writeablePropertyDescriptors = desc;
        }
        
        private Type SimplePropertyType(string propertyName) {
            var simpleProp = GetSimplePropertyInfo(propertyName);
            if ((simpleProp != null) && (simpleProp.Clazz != null)) {
                return simpleProp.Clazz;
            }
            return null;
        }

        private EventPropertyGetterSPI SimplePropertyGetter(string propertyName) {
            var simpleProp = GetSimplePropertyInfo(propertyName);
            if ((simpleProp != null) && (simpleProp.GetterFactory != null)) {
                var getter = simpleProp.GetterFactory.Make(_beanEventTypeFactory.EventBeanTypedEventFactory, _beanEventTypeFactory);
                _propertyGetterCache.Put(propertyName, getter);
                return getter;
            }
            return null;
        }

        private FragmentEventType SimplePropertyFragmentType(string propertyName) {
            var simpleProp = GetSimplePropertyInfo(propertyName);
            if ((simpleProp != null) && (simpleProp.Clazz != null)) {
                var type = simpleProp.Descriptor.ReturnType;
                return EventBeanUtility.CreateNativeFragmentType(type, _beanEventTypeFactory, Stem.IsPublicFields);
            }
            return null;
        }

        private BeanEventPropertyWriter SimplePropertyWriter(string propertyName)
        {
            return _writerMap.TryGetValue(propertyName, out var pair) ? pair.Second : null;
        }
    }
} // end of namespace