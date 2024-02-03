///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.@event.bean.core
{
    public class BeanEventTypeStemService
    {
        private readonly IDictionary<Type, IList<string>> publicClassToTypeNames;
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly PropertyResolutionStyle defaultPropertyResolutionStyle;
        private readonly AccessorStyle defaultAccessorStyle;
        private readonly IDictionary<Type, BeanEventTypeStem> stems = new Dictionary<Type, BeanEventTypeStem>();

        public BeanEventTypeStemService(
            IDictionary<Type, IList<string>> publicClassToTypeNames,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            PropertyResolutionStyle defaultPropertyResolutionStyle,
            AccessorStyle defaultAccessorStyle)
        {
            this.publicClassToTypeNames = publicClassToTypeNames;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.defaultPropertyResolutionStyle = defaultPropertyResolutionStyle;
            this.defaultAccessorStyle = defaultAccessorStyle;
        }

        public BeanEventTypeStem GetCreateStem(
            Type clazz,
            ConfigurationCommonEventTypeBean optionalConfiguration)
        {
            var stem = stems.Get(clazz);
            if (stem != null) {
                return stem;
            }

            if (optionalConfiguration == null && defaultAccessorStyle != AccessorStyle.NATIVE) {
                optionalConfiguration = new ConfigurationCommonEventTypeBean();
                optionalConfiguration.AccessorStyle = defaultAccessorStyle;
            }

            stem = new BeanEventTypeStemBuilder(optionalConfiguration, defaultPropertyResolutionStyle).Make(clazz);
            stems.Put(clazz, stem);
            return stem;
        }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory => eventBeanTypedEventFactory;

        public IDictionary<Type, IList<string>> PublicClassToTypeNames => publicClassToTypeNames;
    }
} // end of namespace