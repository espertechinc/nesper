///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.core;

namespace com.espertech.esper.common.@internal.@event.bean.introspect
{
    public class BeanEventTypeStem
    {
        public BeanEventTypeStem(
            Type clazz, ConfigurationCommonEventTypeBean optionalLegacyDef, string[] propertyNames,
            IDictionary<string, PropertyInfo> simpleProperties,
            IDictionary<string, PropertyStem> mappedPropertyDescriptors,
            IDictionary<string, PropertyStem> indexedPropertyDescriptors, Type[] superTypes, ISet<Type> deepSuperTypes,
            PropertyResolutionStyle propertyResolutionStyle,
            IDictionary<string, IList<PropertyInfo>> simpleSmartPropertyTable,
            IDictionary<string, IList<PropertyInfo>> indexedSmartPropertyTable,
            IDictionary<string, IList<PropertyInfo>> mappedSmartPropertyTable,
            EventPropertyDescriptor[] propertyDescriptors,
            IDictionary<string, EventPropertyDescriptor> propertyDescriptorMap)
        {
            Clazz = clazz;
            OptionalLegacyDef = optionalLegacyDef;
            PropertyNames = propertyNames;
            SimpleProperties = simpleProperties;
            MappedPropertyDescriptors = mappedPropertyDescriptors;
            IndexedPropertyDescriptors = indexedPropertyDescriptors;
            SuperTypes = superTypes;
            DeepSuperTypes = deepSuperTypes;
            PropertyResolutionStyle = propertyResolutionStyle;
            SimpleSmartPropertyTable = simpleSmartPropertyTable;
            IndexedSmartPropertyTable = indexedSmartPropertyTable;
            MappedSmartPropertyTable = mappedSmartPropertyTable;
            PropertyDescriptors = propertyDescriptors;
            PropertyDescriptorMap = propertyDescriptorMap;
        }

        public Type Clazz { get; }

        public ConfigurationCommonEventTypeBean OptionalLegacyDef { get; }

        public PropertyResolutionStyle PropertyResolutionStyle { get; }

        public string[] PropertyNames { get; }

        public IDictionary<string, PropertyInfo> SimpleProperties { get; }

        public IDictionary<string, PropertyStem> MappedPropertyDescriptors { get; }

        public IDictionary<string, PropertyStem> IndexedPropertyDescriptors { get; }

        public Type[] SuperTypes { get; }

        public ISet<Type> DeepSuperTypes { get; }

        public IDictionary<string, IList<PropertyInfo>> SimpleSmartPropertyTable { get; }

        public IDictionary<string, IList<PropertyInfo>> IndexedSmartPropertyTable { get; }

        public IDictionary<string, IList<PropertyInfo>> MappedSmartPropertyTable { get; }

        public EventPropertyDescriptor[] PropertyDescriptors { get; }

        public IDictionary<string, EventPropertyDescriptor> PropertyDescriptorMap { get; }
    }
} // end of namespace