///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.magic;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// This class offers utililty methods around introspection and CGLIB interaction.
    /// </summary>
    public class PropertyHelper
    {
        /// <summary>
        /// Return getter for the given method and CGLIB FastClass.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="method">to return getter for</param>
        /// <param name="fastClass">is the CGLIB fast classs to make FastMethod for</param>
        /// <param name="eventAdapterService">factory for event beans and event types</param>
        /// <returns>property getter</returns>
        public static EventPropertyGetterSPI GetGetter(String propertyName, MethodInfo method, FastClass fastClass, EventAdapterService eventAdapterService)
        {
            // Construct the appropriate property getter CGLib or reflect
            if (fastClass != null)
            {
#if true
                var fastProp = fastClass.GetProperty(propertyName);
                if (fastProp != null)
                    return new CGLibPropertyGetter(fastProp.Target, fastProp, eventAdapterService);
                else
                    return new CGLibPropertyMethodGetter(method, fastClass.GetMethod(method), eventAdapterService);
#else
                return new LambdaPropertyGetter(method, eventAdapterService);
#endif
            }
            else
            {
                return new ReflectionPropMethodGetter(method, eventAdapterService);
            }
        }

        /// <summary>
        /// Introspects the given class and returns event property descriptors for each
        /// property found in the class itself, it's superclasses and all interfaces this class
        /// and the superclasses implements.
        /// </summary>
        /// <param name="type">is the Class to introspect</param>
        /// <returns>
        /// list of properties
        /// </returns>
        public static IList<InternalEventPropDescriptor> GetProperties(Type type)
        {
            // Determine all interfaces implemented and the interface's parent interfaces if any
            ICollection<Type> propertyOrigClasses = new HashSet<Type>();
            GetImplementedInterfaceParents(type, propertyOrigClasses);

            // Add class itself
            propertyOrigClasses.Add(type);

            // Get the set of property names for all classes
            return GetPropertiesForTypes(propertyOrigClasses);
        }

        /// <summary>
        /// Introspects the given class and returns event property descriptors for each
        /// writable property found in the class itself, it's superclasses and all interfaces
        /// this class and the superclasses implements.
        /// </summary>
        /// <param name="type">is the Class to introspect</param>
        /// <returns>
        /// list of properties
        /// </returns>
        public static ICollection<WriteablePropertyDescriptor> GetWritableProperties(Type type)
        {
            // Determine all interfaces implemented and the interface's parent interfaces if any
            ICollection<Type> propertyOrigClasses = new HashSet<Type>();
            GetImplementedInterfaceParents(type, propertyOrigClasses);

            // Add class itself
            propertyOrigClasses.Add(type);

            // Get the set of property names for all classes
            return GetWritablePropertiesForClasses(propertyOrigClasses);
        }

        private static void GetImplementedInterfaceParents(Type clazz, ICollection<Type> classesResult)
        {
            var interfaces = clazz.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++)
            {
                classesResult.Add(interfaces[i]);
                GetImplementedInterfaceParents(interfaces[i], classesResult);
            }
        }

        private static ICollection<WriteablePropertyDescriptor> GetWritablePropertiesForClasses(ICollection<Type> propertyTypes)
        {
            ICollection<WriteablePropertyDescriptor> result = new HashSet<WriteablePropertyDescriptor>();

            foreach (var type in propertyTypes)
            {
                AddIntrospectPropertiesWritable(type, result);
            }

            return result;
        }

        private static IList<InternalEventPropDescriptor> GetPropertiesForTypes(IEnumerable<Type> propertyClasses)
        {
            var result = new List<InternalEventPropDescriptor>();
            foreach (var type in propertyClasses)
            {
                var magicType = MagicType.GetCachedType(type);

                foreach (SimpleMagicPropertyInfo propertyInfo in magicType.GetAllProperties(true).Where(p => p.GetMethod != null))
                {
                    result.Add(new InternalEventPropDescriptor(
                                   propertyInfo.Name,
                                   propertyInfo.GetMethod,
                                   propertyInfo.EventPropertyType));
                }
            }

            RemoveDuplicateProperties(result);
            RemoveCLRProperties(result);

            return result;
        }

        /// <summary>
        /// Remove language specific properties from the given list of property
        /// descriptors.
        /// </summary>
        /// <param name="properties">is the list of property descriptors</param>
        public static void RemoveCLRProperties(IList<InternalEventPropDescriptor> properties)
        {
            var toRemove = new List<InternalEventPropDescriptor>();

            // add removed entries to separate list
            foreach (var desc in properties)
            {
                if (desc.DeclaringType == typeof(object))
                {
                    toRemove.Add(desc);
                }
            }

            // remove
            foreach (var desc in toRemove)
            {
                properties.Remove(desc);
            }
        }

        /// <summary>
        /// Removed duplicate properties using the property name to find unique properties.
        /// </summary>
        /// <param name="properties">is a list of property descriptors</param>
        public static void RemoveDuplicateProperties(IList<InternalEventPropDescriptor> properties)
        {
            var set = new Dictionary<String, InternalEventPropDescriptor>();
            var toRemove = new List<InternalEventPropDescriptor>();

            // add duplicates to separate list
            foreach (var desc in properties)
            {
                if (set.ContainsKey(desc.PropertyName))
                {
                    toRemove.Add(desc);
                    continue;
                }
                set.Put(desc.PropertyName, desc);
            }

            // remove duplicates
            foreach (InternalEventPropDescriptor desc in toRemove)
            {
                properties.Remove(desc);
            }
        }

        private static void AddIntrospectPropertiesWritable(Type clazz, ICollection<WriteablePropertyDescriptor> result)
        {
            MagicType magic = MagicType.GetCachedType(clazz);

            foreach (var magicProperty in magic.GetAllProperties(true).Where(p => p.CanWrite))
            {
                result.Add(new WriteablePropertyDescriptor(
                               magicProperty.Name,
                               magicProperty.PropertyType,
                               magicProperty.SetMethod));
            }
        }

        public static String GetGetterMethodName(String propertyName)
        {
            return GetGetterSetterMethodName(propertyName, "Get");
        }

        public static String GetSetterMethodName(String propertyName)
        {
            return GetGetterSetterMethodName(propertyName, "Set");
        }

        private static String GetGetterSetterMethodName(String propertyName, String operation)
        {
            var writer = new StringWriter();
            writer.Write(operation);
            writer.Write(Char.ToUpperInvariant(propertyName[0]));
            writer.Write(propertyName.Substring(1));
            return writer.ToString();
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
