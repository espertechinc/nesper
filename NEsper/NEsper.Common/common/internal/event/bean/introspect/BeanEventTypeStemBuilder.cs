///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.getter;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
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
            this._optionalConfig = optionalConfig;

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

            var propertyDescriptors = new EventPropertyDescriptor[properties.Count];
            IDictionary<string, EventPropertyDescriptor> propertyDescriptorMap =
                new Dictionary<string, EventPropertyDescriptor>();
            var propertyNames = new string[properties.Count];
            IDictionary<string, PropertyInfo> simpleProperties = new Dictionary<string, PropertyInfo>();
            IDictionary<string, PropertyStem> mappedPropertyDescriptors = new Dictionary<string, PropertyStem>();
            IDictionary<string, PropertyStem> indexedPropertyDescriptors = new Dictionary<string, PropertyStem>();

            IDictionary<string, IList<PropertyInfo>> simpleSmartPropertyTable = null;
            IDictionary<string, IList<PropertyInfo>> mappedSmartPropertyTable = null;
            IDictionary<string, IList<PropertyInfo>> indexedSmartPropertyTable = null;
            if (_smartResolutionStyle) {
                simpleSmartPropertyTable = new Dictionary<string, IList<PropertyInfo>>();
                mappedSmartPropertyTable = new Dictionary<string, IList<PropertyInfo>>();
                indexedSmartPropertyTable = new Dictionary<string, IList<PropertyInfo>>();
            }

            var count = 0;
            foreach (var desc in properties) {
                var propertyName = desc.PropertyName;
                Type underlyingType;
                Type componentType;
                bool isRequiresIndex;
                bool isRequiresMapkey;
                bool isIndexed;
                bool isMapped;
                bool isFragment;

                if (desc.PropertyType == EventPropertyType.SIMPLE) {
                    EventPropertyGetterSPIFactory getter;
                    Type type;
                    if (desc.ReadMethod != null) {
                        getter = new ReflectionPropMethodGetterFactory(desc.ReadMethod);
                        type = desc.ReadMethod.ReturnType;
                    }
                    else if (desc.AccessorProp != null) {
                        getter = new ReflectionPropMethodGetterFactory(desc.AccessorProp);
                        type = desc.AccessorProp.PropertyType;
                    }
                    else if (desc.AccessorField != null) {
                        getter = new ReflectionPropFieldGetterFactory(desc.AccessorField);
                        type = desc.AccessorField.FieldType;
                    }
                    else {
                        // Ignore property
                        continue;
                    }

                    underlyingType = type;
                    componentType = null;
                    isRequiresIndex = false;
                    isRequiresMapkey = false;
                    isIndexed = false;
                    isMapped = false;
                    if (type.IsImplementsInterface(typeof(IDictionary<object, object>))) {
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

                    simpleProperties.Put(propertyName, new PropertyInfo(type, getter, desc));

                    // Recognize that there may be properties with overlapping case-insensitive names
                    if (_smartResolutionStyle) {
                        // Find the property in the smart property table
                        var smartPropertyName = propertyName.ToLowerInvariant();
                        var propertyInfoList = simpleSmartPropertyTable.Get(smartPropertyName);
                        if (propertyInfoList == null) {
                            propertyInfoList = new List<PropertyInfo>();
                            simpleSmartPropertyTable.Put(smartPropertyName, propertyInfoList);
                        }

                        // Enter the property into the smart property list
                        var propertyInfo = new PropertyInfo(type, getter, desc);
                        propertyInfoList.Add(propertyInfo);
                    }
                }
                else if (desc.PropertyType == EventPropertyType.MAPPED) {
                    mappedPropertyDescriptors.Put(propertyName, desc);

                    underlyingType = desc.ReturnType;
                    componentType = typeof(object);
                    isRequiresIndex = false;
                    isRequiresMapkey = desc.ReadMethod.GetParameters().Length > 0;
                    isIndexed = false;
                    isMapped = true;
                    isFragment = false;

                    // Recognize that there may be properties with overlapping case-insentitive names
                    if (_smartResolutionStyle) {
                        // Find the property in the smart property table
                        var smartPropertyName = propertyName.ToLowerInvariant();
                        var propertyInfoList = mappedSmartPropertyTable.Get(smartPropertyName);
                        if (propertyInfoList == null) {
                            propertyInfoList = new List<PropertyInfo>();
                            mappedSmartPropertyTable.Put(smartPropertyName, propertyInfoList);
                        }

                        // Enter the property into the smart property list
                        var propertyInfo = new PropertyInfo(desc.ReturnType, null, desc);
                        propertyInfoList.Add(propertyInfo);
                    }
                }
                else if (desc.PropertyType == EventPropertyType.INDEXED) {
                    indexedPropertyDescriptors.Put(propertyName, desc);

                    underlyingType = desc.ReturnType;
                    componentType = null;
                    isRequiresIndex = desc.ReadMethod.GetParameters().Length > 0;
                    isRequiresMapkey = false;
                    isIndexed = true;
                    isMapped = false;
                    isFragment = desc.ReturnType.IsFragmentableType();

                    if (_smartResolutionStyle) {
                        // Find the property in the smart property table
                        var smartPropertyName = propertyName.ToLowerInvariant();
                        var propertyInfoList = indexedSmartPropertyTable.Get(smartPropertyName);
                        if (propertyInfoList == null) {
                            propertyInfoList = new List<PropertyInfo>();
                            indexedSmartPropertyTable.Put(smartPropertyName, propertyInfoList);
                        }

                        // Enter the property into the smart property list
                        var propertyInfo = new PropertyInfo(desc.ReturnType, null, desc);
                        propertyInfoList.Add(propertyInfo);
                    }
                }
                else {
                    continue;
                }

                propertyNames[count] = desc.PropertyName;
                var descriptor = new EventPropertyDescriptor(
                    desc.PropertyName,
                    underlyingType,
                    componentType,
                    isRequiresIndex,
                    isRequiresMapkey,
                    isIndexed,
                    isMapped,
                    isFragment);
                propertyDescriptors[count++] = descriptor;
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
                propertyNames,
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