///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    public class BeanEventTypeRepoUtil
    {
        public static BeanEventTypeStemService MakeBeanEventTypeStemService(
            Configuration configurationSnapshot,
            IDictionary<string, Type> resolvedBeanEventTypes,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            IDictionary<Type, IList<string>> publicClassToTypeNames = EmptyDictionary<Type, IList<string>>.Instance;
            if (!resolvedBeanEventTypes.IsEmpty()) {
                publicClassToTypeNames = new Dictionary<Type, IList<string>>();
                foreach (var entry in resolvedBeanEventTypes) {
                    var names = publicClassToTypeNames.Get(entry.Value);
                    if (names == null) {
                        names = new List<string>(1);
                        publicClassToTypeNames.Put(entry.Value, names);
                    }

                    names.Add(entry.Key);
                }
            }

            return new BeanEventTypeStemService(
                publicClassToTypeNames,
                eventBeanTypedEventFactory,
                configurationSnapshot.Common.EventMeta.ClassPropertyResolutionStyle,
                configurationSnapshot.Common.EventMeta.DefaultAccessorStyle);
        }

        public static IDictionary<string, Type> ResolveBeanEventTypes(
            IDictionary<string, string> typeToClassName,
            ImportService importService)
        {
            if (typeToClassName.IsEmpty()) {
                return EmptyDictionary<string, Type>.Instance;
            }

            IDictionary<string, Type> resolved = new LinkedHashMap<string, Type>();
            foreach (var entry in typeToClassName) {
                Type type;
                try {
                    type = importService.ResolveClassForBeanEventType(entry.Value);
                }
                catch (ImportException ex) {
                    throw new ConfigurationException("Class named '" + entry.Value + "' was not found", ex);
                }

                resolved.Put(entry.Key, type);
            }

            return resolved;
        }
    }
} // end of namespace