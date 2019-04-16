///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
    /// <summary>
    ///     Implementation for a property list builder that considers any public method
    ///     and public field as the exposed event properties, plus any explicitly configured props.
    /// </summary>
    public class PropertyListBuilderPublic : PropertyListBuilder
    {
        private readonly ConfigurationCommonEventTypeBean legacyConfig;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="legacyConfig">configures legacy type</param>
        public PropertyListBuilderPublic(ConfigurationCommonEventTypeBean legacyConfig)
        {
            this.legacyConfig = legacyConfig ?? throw new ArgumentException("Required configuration not passed");
        }

        public IList<PropertyStem> AssessProperties(Type clazz)
        {
            IList<PropertyStem> result = new List<PropertyStem>();
            PropertyListBuilderExplicit.GetExplicitProperties(result, clazz, legacyConfig);
            AddPublicFields(result, clazz);
            AddPublicMethods(result, clazz);
            return result;
        }

        private static void AddPublicMethods(
            IList<PropertyStem> result,
            Type clazz)
        {
            var methods = clazz.GetMethods();
            for (var i = 0; i < methods.Length; i++) {
                if (methods[i].ReturnType == typeof(void)) {
                    continue;
                }

                var parameterTypes = methods[i].GetParameterTypes();
                if (parameterTypes.Length >= 2) {
                    continue;
                }

                if (parameterTypes.Length == 1) {
                    var parameterType = parameterTypes[0];
                    if (parameterType != typeof(int)
                        && parameterType != typeof(int?)
                        && parameterType != typeof(string)) {
                        continue;
                    }
                }

                var desc = PropertyListBuilderExplicit.MakeMethodDesc(methods[i], methods[i].Name);
                result.Add(desc);
            }

            PropertyHelper.RemovePlatformProperties(result);
        }

        private static void AddPublicFields(
            IList<PropertyStem> result,
            Type clazz)
        {
            var fields = clazz.GetFields();
            for (var i = 0; i < fields.Length; i++) {
                var desc = PropertyListBuilderExplicit.MakeFieldDesc(fields[i], fields[i].Name);
                result.Add(desc);
            }
        }
    }
} // end of namespace