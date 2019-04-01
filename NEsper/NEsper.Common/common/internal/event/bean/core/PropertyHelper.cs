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
using com.espertech.esper.common.magic;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

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
        /// <param name="beanEventTypeFactory">bean facory</param>
        /// <returns>property getter</returns>
        public static EventPropertyGetterSPI GetGetter(
            MethodInfo method, EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return new ReflectionPropMethodGetter(method, eventBeanTypedEventFactory, beanEventTypeFactory);
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
            return GetPropertiesForClasses(propertyOrigClasses);
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

        private static void GetImplementedInterfaceParents(Type clazz, ISet<Type> classesResult)
        {
            var interfaces = clazz.GetInterfaces();

            for (var i = 0; i < interfaces.Length; i++) {
                classesResult.Add(interfaces[i]);
                GetImplementedInterfaceParents(interfaces[i], classesResult);
            }
        }

        private static ISet<WriteablePropertyDescriptor> GetWritablePropertiesForClasses(ISet<Type> propertyClasses)
        {
            ISet<WriteablePropertyDescriptor> result = new HashSet<WriteablePropertyDescriptor>();

            foreach (var clazz in propertyClasses) {
                AddIntrospectPropertiesWritable(clazz, result);
            }

            return result;
        }

        private static IList<PropertyStem> GetPropertiesForClasses(ISet<Type> propertyClasses)
        {
            IList<PropertyStem> result = new List<PropertyStem>();

            foreach (var clazz in propertyClasses) {
                AddIntrospectProperties(clazz, result);
                AddMappedProperties(clazz, result);
            }

            RemoveDuplicateProperties(result);
            RemovePlatformProperties(result);

            return result;
        }

        /// <summary>
        ///     Remove Java language specific properties from the given list of property descriptors.
        /// </summary>
        /// <param name="properties">is the list of property descriptors</param>
        public static void RemovePlatformProperties(IList<PropertyStem> properties)
        {
            IList<PropertyStem> toRemove = new List<PropertyStem>();

            // add removed entries to separate list
            foreach (var desc in properties) {
                if (desc.PropertyName.Equals("class") ||
                    desc.PropertyName.Equals("getClass") ||
                    desc.PropertyName.Equals("toString") ||
                    desc.PropertyName.Equals("hashCode")) {
                    toRemove.Add(desc);
                }
            }

            // remove
            foreach (var desc in toRemove) {
                properties.Remove(desc);
            }
        }

        /// <summary>
        ///     Removed duplicate properties using the property name to find unique properties.
        /// </summary>
        /// <param name="properties">is a list of property descriptors</param>
        protected internal static void RemoveDuplicateProperties(IList<PropertyStem> properties)
        {
            var set = new LinkedHashMap<string, PropertyStem>();
            IList<PropertyStem> toRemove = new List<PropertyStem>();

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

        /// <summary>
        ///     Adds to the given list of property descriptors the properties of the given class
        ///     using the Introspector to introspect properties. This also finds array and indexed properties.
        /// </summary>
        /// <param name="clazz">to introspect</param>
        /// <param name="result">is the list to add to</param>
        protected internal static void AddIntrospectProperties(Type clazz, IList<PropertyStem> result)
        {
            var magic = MagicType.GetCachedType(clazz);


            PropertyDescriptor[] properties = Introspect(clazz);
            for (var i = 0; i < properties.Length; i++) {
                PropertyDescriptor property = properties[i];
                string propertyName = property.Name;
                MethodInfo readMethod = property.ReadMethod;

                var type = EventPropertyType.SIMPLE;
                if (property is IndexedPropertyDescriptor) {
                    readMethod = ((IndexedPropertyDescriptor) property).IndexedReadMethod;
                    type = EventPropertyType.INDEXED;
                }

                if (readMethod == null) {
                    continue;
                }

                result.Add(new PropertyStem(propertyName, readMethod, type));
            }
        }

        private static void AddIntrospectPropertiesWritable(Type clazz, ISet<WriteablePropertyDescriptor> result)
        {
            var magic = MagicType.GetCachedType(clazz);

            foreach (var magicProperty in magic.GetAllProperties(true).Where(p => p.CanWrite)) {
                result.Add(
                    new WriteablePropertyDescriptor(
                        magicProperty.Name,
                        magicProperty.PropertyType,
                        magicProperty.SetMethod));
            }
        }

        /// <summary>
        ///     Adds to the given list of property descriptors the mapped properties, ie.
        ///     properties that have a getter method taking a single String value as a parameter.
        /// </summary>
        /// <param name="clazz">to introspect</param>
        /// <param name="result">is the list to add to</param>
        protected internal static void AddMappedProperties(Type clazz, IList<PropertyStem> result)
        {
            ISet<string> uniquePropertyNames = new HashSet<string>();
            var methods = clazz.GetMethods();

            for (var i = 0; i < methods.Length; i++) {
                var methodName = methods[i].Name;
                if (!methodName.StartsWith("get")) {
                    continue;
                }

                var inferredName = methodName.Substring(3, methodName.Length);
                if (inferredName.Length == 0) {
                    continue;
                }

                Type[] parameterTypes = methods[i].ParameterTypes;
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

                result.Add(new PropertyStem(inferredName, methods[i], EventPropertyType.MAPPED));
                uniquePropertyNames.Add(inferredName);
            }
        }

#if false
/// <summary>
/// Using the Java Introspector class the method returns the property descriptors obtained through introspection.
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
            return GetGetterSetterMethodName(propertyName, "get");
        }

        public static string GetSetterMethodName(string propertyName)
        {
            return GetGetterSetterMethodName(propertyName, "set");
        }

        public static string GetIsMethodName(string propertyName)
        {
            return GetGetterSetterMethodName(propertyName, "is");
        }

        private static string GetGetterSetterMethodName(string propertyName, string operation)
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