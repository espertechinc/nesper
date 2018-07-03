///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.util;

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Introspector that considers explicitly configured event properties only.
    /// </summary>
    public class PropertyListBuilderExplicit : PropertyListBuilder
    {
        private readonly ConfigurationEventTypeLegacy _legacyConfig;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="legacyConfig">is a legacy type specification containinginformation about explicitly configured fields and methods </param>
        public PropertyListBuilderExplicit(ConfigurationEventTypeLegacy legacyConfig)
        {
            if (legacyConfig == null)
            {
                throw new ArgumentException("Required configuration not passed");
            }
            _legacyConfig = legacyConfig;
        }
    
        public IList<InternalEventPropDescriptor> AssessProperties(Type clazz)
        {
            var result = new List<InternalEventPropDescriptor>();
            GetExplicitProperties(result, clazz, _legacyConfig);
            return result;
        }

        /// <summary>
        /// Populates explicitly-defined properties into the result list.
        /// </summary>
        /// <param name="result">is the resulting list of event property descriptors</param>
        /// <param name="type">is the class to introspect</param>
        /// <param name="legacyConfig">supplies specification of explicit methods and fields to expose</param>
        public static void GetExplicitProperties(IList<InternalEventPropDescriptor> result,
                                                 Type type,
                                                 ConfigurationEventTypeLegacy legacyConfig)
        {
            foreach (ConfigurationEventTypeLegacy.LegacyFieldPropDesc desc in legacyConfig.FieldProperties)
            {
                result.Add(MakeDesc(type, desc));
            }

            foreach (ConfigurationEventTypeLegacy.LegacyMethodPropDesc desc in legacyConfig.MethodProperties)
            {
                result.Add(MakeDesc(type, desc));
            }
        }
    
        private static InternalEventPropDescriptor MakeDesc(Type type, ConfigurationEventTypeLegacy.LegacyMethodPropDesc methodDesc)
        {
            MethodInfo[] methods = type.GetMethods();
            MethodInfo method = null;

            foreach (MethodInfo _mi in methods)
            {
                if (_mi.Name != methodDesc.AccessorMethodName)
                {
                    continue;
                }
                if (_mi.ReturnType == typeof(void))
                {
                    continue;
                }
                if (_mi.GetParameters().Length >= 2)
                {
                    continue;
                }
                if (_mi.GetParameters().Length == 0)
                {
                    method = _mi;
                    break;
                }

                var parameterType = _mi.GetParameters()[0].ParameterType;
                if (parameterType.IsNotInt32() && (parameterType != typeof(string)))
                {
                    continue;
                }
    
                method = _mi;
                break;
            }
    
            if (method == null)
            {
                throw new ConfigurationException("Configured method named '" +
                        methodDesc.AccessorMethodName + "' not found for class " + type.Name);
            }
    
            return MakeMethodDesc(method, methodDesc.Name);
        }
    
        private static InternalEventPropDescriptor MakeDesc(Type type, ConfigurationEventTypeLegacy.LegacyFieldPropDesc fieldDesc)
        {
            FieldInfo field = type.GetField(fieldDesc.AccessorFieldName);
            if ( field != null ) {
                return MakeFieldDesc(field, fieldDesc.Name);
            }

            throw new ConfigurationException(
                "Configured field named '" +
                fieldDesc.AccessorFieldName + "' not found for class " + type.Name);
        }
    
        /// <summary>
        /// Makes a simple-type event property descriptor based on a reflected field.
        /// </summary>
        /// <param name="field">is the public field</param>
        /// <param name="name">is the name of the event property</param>
        /// <returns>
        /// property descriptor
        /// </returns>
        public static InternalEventPropDescriptor MakeFieldDesc(FieldInfo field, String name)
        {
            return new InternalEventPropDescriptor(name, field, EventPropertyType.SIMPLE);
        }

        /// <summary>
        /// Makes an event property descriptor based on a reflected method, considering the
        /// methods parameters to determine if this is an indexed or mapped event property.
        /// </summary>
        /// <param name="method">is the public method</param>
        /// <param name="name">is the name of the event property</param>
        /// <returns>
        /// property descriptor
        /// </returns>
        public static InternalEventPropDescriptor MakeMethodDesc(MethodInfo method, String name)
        {
            EventPropertyType propertyType = EventPropertyType.SIMPLE;

            ParameterInfo[] methodParameters = method.GetParameters();
            if ( methodParameters.Length == 1 ) {
                var parameterType = methodParameters[0].ParameterType;
                propertyType = parameterType == typeof(string) ? EventPropertyType.MAPPED : EventPropertyType.INDEXED;
            }
    
            return new InternalEventPropDescriptor(name, method, propertyType);
        }
    }
}
