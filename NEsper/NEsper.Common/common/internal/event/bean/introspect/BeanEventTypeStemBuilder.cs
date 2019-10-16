///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.getter;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
    public class BeanEventTypeStemBuilder
    {
        private readonly ConfigurationCommonEventTypeBean _optionalConfig;
        private readonly PropertyResolutionStyle _propertyResolutionStyle;
        private readonly bool _smartResolutionStyle;

        public BeanEventTypeStemBuilder(
            ConfigurationCommonEventTypeBean optionalConfig,
            PropertyResolutionStyle defaultPropertyResolutionStyle)
        {
            _optionalConfig = optionalConfig;

            if (optionalConfig != null) {
                _propertyResolutionStyle = optionalConfig.PropertyResolutionStyle;
            }
            else {
                _propertyResolutionStyle = defaultPropertyResolutionStyle;
            }

            _smartResolutionStyle = _propertyResolutionStyle.Equals(PropertyResolutionStyle.CASE_INSENSITIVE) ||
                                   _propertyResolutionStyle.Equals(PropertyResolutionStyle.DISTINCT_CASE_INSENSITIVE);
        }

        public BeanEventTypeStem Make(Type clazz)
        {
            EventTypeUtility.ValidateEventBeanClassVisibility(clazz);

            var propertyListBuilder = PropertyListBuilderFactory.CreateBuilder(_optionalConfig);
            var properties = propertyListBuilder.AssessProperties(clazz);

            var propertyDescriptors = new List<EventPropertyDescriptor>();
            var propertyDescriptorMap = new Dictionary<string, EventPropertyDescriptor>();
            var propertyNames = new SortedSet<string>();
            var simpleProperties = new Dictionary<string, PropertyInfo>();
            var mappedPropertyDescriptors = new Dictionary<string, PropertyStem>();
            var indexedPropertyDescriptors = new Dictionary<string, PropertyStem>();

            IDictionary<string, IList<PropertyInfo>> simpleSmartPropertyTable = null;
            IDictionary<string, IList<PropertyInfo>> mappedSmartPropertyTable = null;
            IDictionary<string, IList<PropertyInfo>> indexedSmartPropertyTable = null;
            if (_smartResolutionStyle) {
                simpleSmartPropertyTable = new Dictionary<string, IList<PropertyInfo>>();
                mappedSmartPropertyTable = new Dictionary<string, IList<PropertyInfo>>();
                indexedSmartPropertyTable = new Dictionary<string, IList<PropertyInfo>>();
            }

            foreach (var desc in properties) {
                var propertyName = desc.PropertyName;
                Type underlyingType = null;
                Type componentType = null;
                bool isRequiresIndex = false;
                bool isRequiresMapkey = false;
                bool isIndexed = false;
                bool isMapped = false;
                bool isFragment = false;
                bool isSimple = false;

                if (desc.PropertyType.IsUndefined()) {
                    continue;
                }

                EventPropertyGetterSPIFactory getter = null;

                // SIMPLE

                if (desc.PropertyType.IsSimple())
                {
                    if (desc.ReadMethod != null) {
                        getter = new ReflectionPropMethodGetterFactory(desc.ReadMethod);
                        underlyingType = desc.ReadMethod.ReturnType;
                    }
                    else if (desc.AccessorProp != null) {
                        getter = new ReflectionPropPropertyGetterFactory(desc.AccessorProp);
                        underlyingType = desc.AccessorProp.PropertyType;
                    }
                    else if (desc.AccessorField != null) {
                        getter = new ReflectionPropFieldGetterFactory(desc.AccessorField);
                        underlyingType = desc.AccessorField.FieldType;
                    }
                    else {
                        throw new IllegalStateException($"invalid property descriptor: {desc}");
                    }

                    isSimple = true;
                    isFragment = underlyingType.IsFragmentableType();

#if false
                    if (TypeHelper.IsImplementsInterface(type, typeof(IDictionary<object, object>))) {
                        isMapped = true;
                        // We do not yet allow to fragment maps entries.
                        // Class genericType = TypeHelper.getGenericReturnTypeMap(desc.getReadMethod(), desc.getAccessorField());
                        isFragment = false;

                        if (desc.ReadMethod != null) {
                            componentType = TypeHelper.GetGenericReturnTypeMap(desc.ReadMethod, false);
                        }
                        else if (desc.AccessorProp != null) {
                            componentType = TypeHelper.GetGenericPropertyTypeMap(desc.AccessorProp, false);
                        }
                        else if (desc.AccessorField != null) {
                            componentType = TypeHelper.GetGenericFieldTypeMap(desc.AccessorField, false);
                        }
                        else {
                            componentType = typeof(object);
                        }
                    }
                    else if (type.IsArray) {
                        isIndexed = true;
                        isFragment = type.GetElementType().IsFragmentableType();
                        componentType = type.GetElementType();
                    }
                    else if (type.IsGenericEnumerable()) {
                        isIndexed = true;
                        var genericType = TypeHelper.GetGenericReturnType(
                            desc.ReadMethod,
                            desc.AccessorField,
                            desc.AccessorProp,
                            true);
                        isFragment = genericType.IsFragmentableType();
                        if (genericType != null) {
                            componentType = genericType;
                        }
                        else {
                            componentType = typeof(object);
                        }
                    }
                    else {
                        isMapped = false;
                        isFragment = type.IsFragmentableType();
                    }
#endif

                    simpleProperties.Put(propertyName, new PropertyInfo(underlyingType, getter, desc));
                }

                // MAPPED

                if (desc.PropertyType.IsMapped()) {
                    // Local function: CheckComponentType
                    void CheckComponentType()
                    {
                        if (underlyingType.IsGenericDictionary()) {
                            componentType = underlyingType.GetDictionaryValueType();
                        }
                    }


                    underlyingType = desc.ReturnType;
                    componentType = typeof(object);

                    if (desc.ReadMethod != null) {
                        isRequiresMapkey = desc.ReadMethod.GetParameters().Length > 0;
                        componentType = desc.ReadMethod.ReturnType;
                    }
                    else if (desc.AccessorProp != null) {
                        isRequiresMapkey = false; // not required, you can "get" the property
                        CheckComponentType();
                    }
                    else if (desc.AccessorField != null) {
                        isRequiresMapkey = false; // not required, you can "get" the property
                        CheckComponentType();
                    }
                    else {
                        throw new IllegalStateException($"invalid property descriptor: {desc}");
                    }

                    isMapped = true;
                    isFragment = false;

                    mappedPropertyDescriptors.Put(propertyName, desc);
                }

                // INDEXED

                if (desc.PropertyType.IsIndexed()) {
                    // Local function: CheckFragmentation
                    void CheckFragmentation()
                    {
                        if (underlyingType.IsArray) {
                            isFragment = underlyingType.GetElementType().IsFragmentableType();
                            componentType = underlyingType.GetElementType();
                        }
                        else if (underlyingType.IsGenericEnumerable()) {
                            var genericType = TypeHelper.GetGenericReturnType(
                                desc.ReadMethod,
                                desc.AccessorField,
                                desc.AccessorProp,
                                true);
                            isFragment = genericType.IsFragmentableType();
                            componentType = genericType != null ? genericType : typeof(object);
                        }
                        else {
                            isFragment = false;
                            componentType = typeof(object);
                        }
                    }

                    underlyingType = desc.ReturnType;

                    if (desc.ReadMethod != null) {
                        var rmParameters = desc.ReadMethod.GetParameters() ?? new ParameterInfo[0];
                        isRequiresIndex = rmParameters.Length > 0;
                        if (!isRequiresIndex) {
                            //componentType = TypeHelper.GetGenericReturnTypeMap(desc.ReadMethod, false);
                            CheckFragmentation();
                        }
                    }
                    else if (desc.AccessorProp != null) {
                        isRequiresIndex = false; // not required, you can "get" the index
                        //componentType = TypeHelper.GetGenericPropertyTypeMap(desc.AccessorProp, false);
                        CheckFragmentation();
                    }
                    else if (desc.AccessorField != null) {
                        isRequiresIndex = false; // not required, you can "get" the index
                        //componentType = TypeHelper.GetGenericFieldTypeMap(desc.AccessorField, false);
                        CheckFragmentation();
                    }
                    else {
                        throw new IllegalStateException($"invalid property descriptor: {desc}");
                    }

                    //componentType = null;
                    isIndexed = true;
                    indexedPropertyDescriptors.Put(propertyName, desc);
                }

                // ----------------------------------------------------------------------------------------------
                // SMART-INDEXING: Recognize that there may be properties with overlapping case-insensitive names
                // ----------------------------------------------------------------------------------------------

                if (_smartResolutionStyle) {
                    // SIMPLE

                    if (isSimple)
                    {
                        // Find the property in the smart property table
                        var smartPropertyName = propertyName.ToLowerInvariant();
                        var propertyInfoList = simpleSmartPropertyTable.Get(smartPropertyName);
                        if (propertyInfoList == null)
                        {
                            propertyInfoList = new List<PropertyInfo>();
                            simpleSmartPropertyTable.Put(smartPropertyName, propertyInfoList);
                        }

                        // Enter the property into the smart property list
                        var propertyInfo = new PropertyInfo(underlyingType, getter, desc);
                        propertyInfoList.Add(propertyInfo);
                    }

                    // MAPPED

                    if (isMapped) {
                        // Find the property in the smart property table
                        var smartPropertyName = propertyName.ToLowerInvariant();
                        var propertyInfoList = mappedSmartPropertyTable.Get(smartPropertyName);
                        if (propertyInfoList == null) {
                            propertyInfoList = new List<PropertyInfo>();
                            mappedSmartPropertyTable.Put(smartPropertyName, propertyInfoList);
                        }

                        // Enter the property into the smart property list
                        var propertyInfo = new PropertyInfo(underlyingType, null, desc);
                        propertyInfoList.Add(propertyInfo);
                    }

                    // INDEXED

                    if (isIndexed) {
                        // Find the property in the smart property table
                        var smartPropertyName = propertyName.ToLowerInvariant();
                        var propertyInfoList = indexedSmartPropertyTable.Get(smartPropertyName);
                        if (propertyInfoList == null) {
                            propertyInfoList = new List<PropertyInfo>();
                            indexedSmartPropertyTable.Put(smartPropertyName, propertyInfoList);
                        }

                        // Enter the property into the smart property list
                        var propertyInfo = new PropertyInfo(underlyingType, null, desc);
                        propertyInfoList.Add(propertyInfo);
                    }
                }

                // ----------------------------------------------------------------------------------------------
                // STANDARD-INDEXING
                // ----------------------------------------------------------------------------------------------

                propertyNames.Add(desc.PropertyName);

                var descriptor = new EventPropertyDescriptor(
                    desc.PropertyName,
                    underlyingType,
                    componentType,
                    isRequiresIndex,
                    isRequiresMapkey,
                    isIndexed,
                    isMapped,
                    isFragment);

                propertyDescriptors.Add(descriptor);
                propertyDescriptorMap.Put(descriptor.PropertyName, descriptor);
            }

            // Determine event type super types
            var superTypes = GetSuperTypes(clazz);
            if (superTypes != null && superTypes.Length == 0) {
                superTypes = null;
            }

            // Determine deep supertypes
            // Get base types (superclasses and interfaces), deep get of all in the tree
            ISet<Type> deepSuperTypes = new HashSet<Type>();
            GetSuper(clazz, deepSuperTypes);
            RemovePlatformInterfaces(deepSuperTypes);

            return new BeanEventTypeStem(
                clazz,
                _optionalConfig,
                propertyNames.ToArray(),
                simpleProperties,
                mappedPropertyDescriptors,
                indexedPropertyDescriptors,
                superTypes,
                deepSuperTypes,
                _propertyResolutionStyle,
                simpleSmartPropertyTable,
                indexedSmartPropertyTable,
                mappedSmartPropertyTable,
                propertyDescriptors,
                propertyDescriptorMap);
        }

        private static Type[] GetSuperTypes(Type clazz)
        {
            IList<Type> superclasses = new List<Type>();

            // add superclass
            var superClass = clazz.BaseType;
            if (superClass != null) {
                superclasses.Add(superClass);
            }

            // add interfaces
            var interfaces = clazz.GetInterfaces();
            superclasses.AddAll(interfaces);

            // Build super types, ignoring platformtypes
            IList<Type> superTypes = new List<Type>();
            foreach (var superclass in superclasses) {
                if (superclass.Namespace != "System") {
                    superTypes.Add(superclass);
                }
            }

            return superTypes.ToArray();
        }

        /// <summary>
        ///     Add the given class's implemented interfaces and superclasses to the result set of classes.
        /// </summary>
        /// <param name="clazz">to introspect</param>
        /// <param name="result">to add classes to</param>
        protected internal static void GetSuper(
            Type clazz,
            ISet<Type> result)
        {
            GetSuperInterfaces(clazz, result);
            GetSuperClasses(clazz, result);
        }

        private static void GetSuperInterfaces(
            Type clazz,
            ISet<Type> result)
        {
            var interfaces = clazz.GetInterfaces();

            for (var i = 0; i < interfaces.Length; i++) {
                result.Add(interfaces[i]);
                GetSuperInterfaces(interfaces[i], result);
            }
        }

        private static void GetSuperClasses(
            Type clazz,
            ISet<Type> result)
        {
            var superClass = clazz.BaseType;
            if (superClass == null) {
                return;
            }

            result.Add(superClass);
            GetSuper(superClass, result);
        }

        private static void RemovePlatformInterfaces(ISet<Type> classes)
        {
            foreach (var clazz in classes.ToArray()) {
                if (clazz.Namespace == "System") {
                    classes.Remove(clazz);
                }
            }
        }
    }
} // end of namespace