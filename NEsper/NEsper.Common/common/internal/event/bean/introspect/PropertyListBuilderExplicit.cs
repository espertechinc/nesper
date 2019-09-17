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

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
    /// <summary>
    ///     Introspector that considers explicitly configured event properties only.
    /// </summary>
    public class PropertyListBuilderExplicit : PropertyListBuilder
    {
        private readonly ConfigurationCommonEventTypeBean legacyConfig;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="legacyConfig">
        ///     is a legacy type specification containinginformation about explicitly configured fields and methods
        /// </param>
        public PropertyListBuilderExplicit(ConfigurationCommonEventTypeBean legacyConfig)
        {
            if (legacyConfig == null) {
                throw new ArgumentException("Required configuration not passed");
            }

            this.legacyConfig = legacyConfig;
        }

        public IList<PropertyStem> AssessProperties(Type clazz)
        {
            IList<PropertyStem> result = new List<PropertyStem>();
            GetExplicitProperties(result, clazz, legacyConfig);
            return result;
        }

        /// <summary>
        ///     Populates explicitly-defined properties into the result list.
        /// </summary>
        /// <param name="result">is the resulting list of event property descriptors</param>
        /// <param name="clazz">is the class to introspect</param>
        /// <param name="legacyConfig">supplies specification of explicit methods and fields to expose</param>
        protected internal static void GetExplicitProperties(
            IList<PropertyStem> result,
            Type clazz,
            ConfigurationCommonEventTypeBean legacyConfig)
        {
            foreach (var desc in legacyConfig.FieldProperties) {
                result.Add(MakeDesc(clazz, desc));
            }

            foreach (var desc in legacyConfig.MethodProperties) {
                result.Add(MakeDesc(clazz, desc));
            }
        }

        private static PropertyStem MakeDesc(
            Type clazz,
            ConfigurationCommonEventTypeBean.LegacyMethodPropDesc methodDesc)
        {
            var methods = clazz.GetMethods();
            MethodInfo method = null;
            for (var i = 0; i < methods.Length; i++) {
                if (!methods[i].Name.Equals(methodDesc.AccessorMethodName)) {
                    continue;
                }

                if (methods[i].ReturnType == typeof(void)) {
                    continue;
                }

                var parameterTypes = methods[i].GetParameterTypes();
                if (parameterTypes.Length >= 2) {
                    continue;
                }

                if (parameterTypes.Length == 0) {
                    method = methods[i];
                    break;
                }

                var parameterType = parameterTypes[0];
                if (parameterType != typeof(int) && parameterType != typeof(int?) && parameterType != typeof(string)) {
                    continue;
                }

                method = methods[i];
                break;
            }

            if (method == null) {
                throw new ConfigurationException(
                    "Configured method named '" +
                    methodDesc.AccessorMethodName +
                    "' not found for class " +
                    clazz.Name);
            }

            return MakeMethodDesc(method, methodDesc.Name);
        }

        private static PropertyStem MakeDesc(
            Type clazz,
            ConfigurationCommonEventTypeBean.LegacyFieldPropDesc fieldDesc)
        {
            var field = clazz.GetField(fieldDesc.AccessorFieldName);
            if (field == null) {
                throw new ConfigurationException(
                    "Configured field named '" +
                    fieldDesc.AccessorFieldName +
                    "' not found for class " +
                    clazz.CleanName());
            }

            return MakeFieldDesc(field, fieldDesc.Name);
        }

        /// <summary>
        ///     Makes a simple-type event property descriptor based on a reflected field.
        /// </summary>
        /// <param name="field">is the public field</param>
        /// <param name="name">is the name of the event property</param>
        /// <returns>property descriptor</returns>
        protected internal static PropertyStem MakeFieldDesc(
            FieldInfo field,
            string name)
        {
            return new PropertyStem(name, field, PropertyType.SIMPLE);
        }

        /// <summary>
        ///     Makes an event property descriptor based on a reflected method, considering
        ///     the methods parameters to determine if this is an indexed or mapped event property.
        /// </summary>
        /// <param name="method">is the public method</param>
        /// <param name="name">is the name of the event property</param>
        /// <returns>property descriptor</returns>
        protected internal static PropertyStem MakeMethodDesc(
            MethodInfo method,
            string name)
        {
            PropertyType propertyType;

            var parameterTypes = method.GetParameterTypes();
            if (parameterTypes.Length == 1) {
                var parameterType = parameterTypes[0];
                if (parameterType == typeof(string)) {
                    propertyType = PropertyType.MAPPED;
                }
                else {
                    propertyType = PropertyType.INDEXED;
                }
            }
            else {
                propertyType = PropertyType.SIMPLE;
            }

            return new PropertyStem(name, method, propertyType);
        }
    }
} // end of namespace