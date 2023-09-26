///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using com.espertech.esper.common.@internal.@event.bean.getter;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.magic;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    /// <summary>
    ///     This class offers utility methods around introspection.
    /// </summary>
    public class PropertyHelper
    {
        /// <summary>
        ///     Return getter for the given method.
        /// </summary>
        /// <param name="method">to return getter for</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        /// <returns>property getter</returns>
        public static EventPropertyGetterSPI GetGetter(
            MethodInfo method,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return new ReflectionPropMethodGetter(method, eventBeanTypedEventFactory, beanEventTypeFactory);
        }

        /// <summary>
        ///     Return getter for the given property.
        /// </summary>
        /// <param name="property">property to resolve</param>
        /// <param name="eventBeanTypedEventFactory">factory for event beans and event types</param>
        /// <param name="beanEventTypeFactory">bean factory</param>
        public static EventPropertyGetterSPI GetGetter(
            PropertyInfo property,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return new ReflectionPropMethodGetter(property, eventBeanTypedEventFactory, beanEventTypeFactory);
        }

        /// <summary>
        ///     Introspects the given class and returns event property descriptors for each property found
        ///     in the class itself, it's superclasses and all interfaces this class and the superclasses implements.
        /// </summary>
        /// <param name="clazz">is the Class to introspect</param>
        /// <returns>list of properties</returns>
        public static IList<PropertyStem> GetProperties(Type clazz)
        {
            // Determine all interfaces implemented and the interface's parent interfaces if any
            ISet<Type> propertyOrigClasses = new HashSet<Type>();
            GetImplementedInterfaceParents(clazz, propertyOrigClasses);

            // Add class itself
            propertyOrigClasses.Add(clazz);

            // Get the set of property names for all classes
            return GetPropertiesForTypes(propertyOrigClasses);
        }

        /// <summary>
        ///     Introspects the given class and returns event property descriptors for each writable property found
        ///     in the class itself, it's superclasses and all interfaces this class and the superclasses implements.
        /// </summary>
        /// <param name="clazz">is the Class to introspect</param>
        /// <returns>list of properties</returns>
        public static ISet<WriteablePropertyDescriptor> GetWritableProperties(Type clazz)
        {
            // Determine all interfaces implemented and the interface's parent interfaces if any
            ISet<Type> propertyOrigClasses = new HashSet<Type>();
            GetImplementedInterfaceParents(clazz, propertyOrigClasses);

            // Add class itself
            propertyOrigClasses.Add(clazz);

            // Get the set of property names for all classes
            return GetWritablePropertiesForClasses(propertyOrigClasses);
        }

        private static void GetImplementedInterfaceParents(
            Type clazz,
            ICollection<Type> classesResult)
        {
            var interfaces = clazz.GetInterfaces();

            for (var i = 0; i < interfaces.Length; i++) {
                classesResult.Add(interfaces[i]);
                GetImplementedInterfaceParents(interfaces[i], classesResult);
            }
        }

        private static ISet<WriteablePropertyDescriptor> GetWritablePropertiesForClasses(
            IEnumerable<Type> propertyClasses)
        {
            ISet<WriteablePropertyDescriptor> result = new HashSet<WriteablePropertyDescriptor>();

            foreach (var clazz in propertyClasses) {
                AddIntrospectPropertiesWritable(clazz, result);
            }

            return result;
        }

        private static IList<PropertyStem> GetPropertiesForTypes(IEnumerable<Type> propertyClasses)
        {
            var result = new List<PropertyStem>();
            foreach (var type in propertyClasses) {
                var magicType = MagicType.GetCachedType(type);

                AddIntrospectProperties(magicType, result);
                // MagicType captures properties that are true 'native' properties
                // - and properties that are exposed via getters.  As such, the
                // - AddMappedProperties call is no needed, but left here commented
                // - for future revisions.
                //
                // AddMappedProperties(magicType, result);
            }

            RemoveDuplicateProperties(result);
            RemovePlatformProperties(result);

            return result;
        }

        public static PropertyStem Merge(
            PropertyStem baseStem,
            PropertyStem mergeStem)
        {
            if (baseStem == null) {
                return mergeStem;
            }

            if (baseStem.DeclaringType != mergeStem.DeclaringType) {
                throw new ArgumentException("cannot merge property stems from different declaring types");
            }

            // Below are codified rules for how to merge stems when a class defines two
            // different ways to retrieve a property.  For example, the class could define
            // a property and a getter method.  In this case, which should take precedence?
            // 
            // These rules need to be captured somewhere.  Currently, there is no good answer
            // for where these rules are codified.  We will capture them here for now and move
            // them out so that these rules can be in the development space elsewhere.

            var accessorProp = baseStem.AccessorProp;
            var accessorField = baseStem.AccessorField;
            var accessorMethod = baseStem.ReadMethod;
            var stemPropType = baseStem.PropertyType | mergeStem.PropertyType;
            var stemPropName = baseStem.PropertyName;

            if (mergeStem.AccessorProp != null) {
                if (accessorProp != null) {
                    throw new ArgumentException($"base stem already defines {nameof(baseStem.AccessorProp)}");
                }

                accessorProp = mergeStem.AccessorProp;
            }

            if (mergeStem.AccessorField != null) {
                if (accessorField != null) {
                    throw new ArgumentException($"base stem already defines {nameof(baseStem.AccessorField)}");
                }

                accessorField = mergeStem.AccessorField;
            }


            if (mergeStem.ReadMethod != null) {
                if (accessorMethod != null) {
                    throw new ArgumentException($"base stem already defines {nameof(baseStem.ReadMethod)}");
                }

                accessorMethod = mergeStem.ReadMethod;
            }

            return new PropertyStem(stemPropName, accessorMethod, accessorField, accessorProp, stemPropType);
        }

        public static void AddIntrospectProperties(
            MagicType magicType,
            IList<PropertyStem> result)
        {
            var propertyStemTable = new Dictionary<string, PropertyStem>();
            var properties = magicType.GetAllProperties(true).ToList();

            foreach (var propertyInfo in properties) {
                propertyStemTable.TryGetValue(
                    propertyInfo.Name,
                    out var propertyStem);

                if (propertyInfo.Member is PropertyInfo propertyInfoMember) {
                    propertyStemTable[propertyInfo.Name] =
                        Merge(
                            propertyStem,
                            new PropertyStem(
                                propertyInfo.Name,
                                propertyInfoMember,
                                propertyInfo.EventPropertyType));
                }
                else if (propertyInfo.Member is MethodInfo) {
                    propertyStemTable[propertyInfo.Name] =
                        Merge(
                            propertyStem,
                            new PropertyStem(
                                propertyInfo.Name,
                                propertyInfo.GetMethod,
                                propertyInfo.EventPropertyType));
                }
                else if (propertyInfo.Member is FieldInfo fieldInfoMember) {
                    propertyStemTable[propertyInfo.Name] =
                        Merge(
                            propertyStem,
                            new PropertyStem(
                                propertyInfo.Name,
                                fieldInfoMember,
                                propertyInfo.EventPropertyType));
                }
            }

            result.AddAll(propertyStemTable.Values);
        }

        /// <summary>
        ///     Remove language or platform specific properties from the given list of property descriptors.
        /// </summary>
        /// <param name="properties">is the list of property descriptors</param>
        public static void RemovePlatformProperties(IList<PropertyStem> properties)
        {
            IList<PropertyStem> toRemove = properties
                .Where(d => d.DeclaringType == typeof(object))
                .ToList();

            // remove
            foreach (var desc in toRemove) {
                properties.Remove(desc);
            }
        }

        /// <summary>
        ///     Removed duplicate properties using the property name to find unique properties.
        /// </summary>
        /// <param name="properties">is a list of property descriptors</param>
        public static void RemoveDuplicateProperties(IList<PropertyStem> properties)
        {
            var set = new LinkedHashMap<string, PropertyStem>();
            var toRemove = new List<PropertyStem>();

            // add duplicates to separate list
            foreach (var desc in properties) {
                if (set.ContainsKey(desc.PropertyName)) {
                    toRemove.Add(desc);
                    continue;
                }

                set.Put(desc.PropertyName, desc);
            }

            // remove duplicates
            foreach (var desc in toRemove) {
                properties.Remove(desc);
            }
        }

        private static void AddIntrospectPropertiesWritable(
            Type clazz,
            ISet<WriteablePropertyDescriptor> result)
        {
            var magic = MagicType.GetCachedType(clazz);

            foreach (var magicProperty in magic.GetAllProperties(true).Where(p => p.CanWrite)) {
                result.Add(
                    new WriteablePropertyDescriptor(
                        magicProperty.Name,
                        magicProperty.PropertyType,
                        magicProperty.Member,
                        false));
            }
        }

        /// <summary>
        ///     Adds to the given list of property descriptors the mapped properties, ie.
        ///     properties that have a getter method taking a single String value as a parameter.
        /// </summary>
        /// <param name="magicType">type to introspect</param>
        /// <param name="result">is the list to add to</param>
        public static void AddMappedProperties(
            MagicType magicType,
            IList<PropertyStem> result)
        {
            var clazz = magicType.TargetType;
            ISet<string> uniquePropertyNames = new HashSet<string>();
            var methods = clazz.GetMethods();

            for (var i = 0; i < methods.Length; i++) {
                var methodName = methods[i].Name;
                if (!methodName.StartsWith("Get")) {
                    continue;
                }

                var inferredName = methodName.Substring(3);
                if (inferredName.Length == 0) {
                    continue;
                }

                var parameterTypes = methods[i].GetParameterTypes();
                if (parameterTypes.Length != 1) {
                    continue;
                }

                if (parameterTypes[0] != typeof(string)) {
                    continue;
                }

                string newInferredName = null;
                // Leave uppercase inferred names such as URL
                if (inferredName.Length >= 2) {
                    if (char.IsUpper(inferredName[0]) && char.IsUpper(inferredName[1])) {
                        newInferredName = inferredName;
                    }
                }

                // camelCase the inferred name
                if (newInferredName == null) {
                    newInferredName = char.ToString(char.ToLower(inferredName[0]));
                    if (inferredName.Length > 1) {
                        newInferredName += inferredName.Substring(1);
                    }
                }

                inferredName = newInferredName;

                // if the property inferred name already exists, don't supply it
                if (uniquePropertyNames.Contains(inferredName)) {
                    continue;
                }

                result.Add(new PropertyStem(inferredName, methods[i], PropertyType.MAPPED));
                uniquePropertyNames.Add(inferredName);
            }
        }

#if false
/// <summary>
/// Using the Introspector class the method returns the property descriptors obtained through introspection.
/// </summary>
/// <param name="clazz">to introspect</param>
/// <returns>array of property descriptors</returns>
        protected internal static PropertyDescriptor[] Introspect(Type clazz) {
	        BeanInfo beanInfo;

	        try {
	            beanInfo = Introspector.GetBeanInfo(clazz);
	        } catch (IntrospectionException e) {
	            return new PropertyDescriptor[0];
	        }

	        return beanInfo.PropertyDescriptors;
	    }
#endif

        public static string GetGetterMethodName(string propertyName)
        {
            return GetGetterSetterMethodName(propertyName, "Get");
        }

        public static string GetSetterMethodName(string propertyName)
        {
            return GetGetterSetterMethodName(propertyName, "Set");
        }

        public static string GetIsMethodName(string propertyName)
        {
            return GetGetterSetterMethodName(propertyName, "Is");
        }

        public static string GetPropertyName(string fieldName)
        {
            var writer = new StringWriter();
            writer.Write(char.ToUpperInvariant(fieldName[0]));
            writer.Write(fieldName.Substring(1));
            return writer.ToString();
        }

        private static string GetGetterSetterMethodName(
            string propertyName,
            string operation)
        {
            var writer = new StringWriter();
            writer.Write(operation);
            writer.Write(char.ToUpperInvariant(propertyName[0]));
            writer.Write(propertyName.Substring(1));
            return writer.ToString();
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(PropertyHelper));
    }
} // end of namespace