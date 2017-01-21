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

namespace com.espertech.esper.events.bean
{
    /// <summary>
    /// Implementation for a property list builder that considers any public method and
    /// public field as the exposed event properties, plus any explicitly configured
    /// props.
    /// </summary>
    public class PropertyListBuilderPublic : PropertyListBuilder
    {
        private readonly ConfigurationEventTypeLegacy _legacyConfig;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="legacyConfig">configures legacy type</param>
        public PropertyListBuilderPublic(ConfigurationEventTypeLegacy legacyConfig)
        {
            if (legacyConfig == null)
            {
                throw new ArgumentException("Required configuration not passed");
            }
            this._legacyConfig = legacyConfig;
        }
    
        public IList<InternalEventPropDescriptor> AssessProperties(Type clazz)
        {
            var result = new List<InternalEventPropDescriptor>();
            PropertyListBuilderExplicit.GetExplicitProperties(result, clazz, _legacyConfig);
            AddPublicFields(result, clazz);
            AddPublicMethods(result, clazz);
            return result;
        }
    
        private static void AddPublicMethods(IList<InternalEventPropDescriptor> result, Type clazz)
        {
            var methods = clazz.GetMethods();
            for (int i = 0; i < methods.Length; i++)
            {
                if (methods[i].ReturnType == typeof(void))
                {
                    continue;
                }

                var parameters = methods[i].GetParameters();
                if (parameters.Length >= 2)
                {
                    continue;
                }
                if (parameters.Length == 1)
                {
                    Type parameterType = parameters[0].ParameterType;
                    if ((parameterType != typeof(int)) && ((parameterType != typeof(int?))) &&
                        (parameterType != typeof(String)))
                    {
                        continue;
                    }
                }
    
                InternalEventPropDescriptor desc = PropertyListBuilderExplicit.MakeMethodDesc(methods[i], methods[i].Name);
                result.Add(desc);
            }
    
            PropertyHelper.RemoveCLRProperties(result);
        }

        private static void AddPublicFields(ICollection<InternalEventPropDescriptor> result, Type clazz)
        {
            FieldInfo[] fields = clazz.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                InternalEventPropDescriptor desc = PropertyListBuilderExplicit.MakeFieldDesc(fields[i], fields[i].Name);
                result.Add(desc);
            }
        }
    }
}
