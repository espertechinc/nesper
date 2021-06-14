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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    public class BeanEventTypeStemService
    {
        private readonly AccessorStyle _defaultAccessorStyle;
        private readonly PropertyResolutionStyle _defaultPropertyResolutionStyle;

        private readonly IDictionary<Type, BeanEventTypeStem> _stems = new Dictionary<Type, BeanEventTypeStem>();

        public BeanEventTypeStemService(
            IDictionary<Type, IList<string>> publicClassToTypeNames,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            PropertyResolutionStyle defaultPropertyResolutionStyle,
            AccessorStyle defaultAccessorStyle)
        {
            PublicClassToTypeNames = publicClassToTypeNames;
            EventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this._defaultPropertyResolutionStyle = defaultPropertyResolutionStyle;
            this._defaultAccessorStyle = defaultAccessorStyle;
        }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }

        public IDictionary<Type, IList<string>> PublicClassToTypeNames { get; }

        public BeanEventTypeStem GetCreateStem(
            Type clazz,
            ConfigurationCommonEventTypeBean optionalConfiguration)
        {
            var stem = _stems.Get(clazz);
            if (stem != null) {
                return stem;
            }

            if (optionalConfiguration == null && _defaultAccessorStyle != AccessorStyle.NATIVE) {
                optionalConfiguration = new ConfigurationCommonEventTypeBean();
                optionalConfiguration.AccessorStyle = _defaultAccessorStyle;
            }

            stem = new BeanEventTypeStemBuilder(optionalConfiguration, _defaultPropertyResolutionStyle).Make(clazz);
            _stems.Put(clazz, stem);
            return stem;
        }
    }
} // end of namespace