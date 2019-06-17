///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.runtime.@internal.support
{
    public class SupportEventTypeFactory
    {
        /// <summary>Gets the instance.</summary>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        public static SupportEventTypeFactory GetInstance(IContainer container)
        {
            return container.ResolveSingleton(() => new SupportEventTypeFactory(container));
        }

        public IContainer Container { get; set; }

        // NOTE: FIX THESE NAMES
        internal readonly BeanEventTypeStemService BEAN_STEM_SVC;
        internal readonly BeanEventTypeFactory BEAN_EVENT_TYPE_FACTORY;

        internal readonly BeanEventTypeStemBuilder STEM_BUILDER;
        internal readonly Func<string, EventTypeMetadata> METADATA_CLASS;

        internal readonly BeanEventType SUPPORTBEAN_EVENTTTPE;
        internal readonly BeanEventType SUPPORTBEAN_S0_EVENTTTPE;
        internal readonly BeanEventType SUPPORTBEAN_S1_EVENTTTPE;
        internal readonly BeanEventType SUPPORTBEAN_S2_EVENTTTPE;
        internal readonly BeanEventType SUPPORTBEAN_A_EVENTTTPE;
        internal readonly BeanEventType SUPPORTBEANCOMPLEXPROPS_EVENTTTPE;
        internal readonly BeanEventType SUPPORTBEANSIMPLE_EVENTTTPE;

        /// <summary>Initializes a new instance of the <see cref="SupportEventTypeFactory"/> class.</summary>
        /// <param name="container">The container.</param>
        public SupportEventTypeFactory(IContainer container)
        {
            Container = container;

            BEAN_EVENT_TYPE_FACTORY = new BeanEventTypeFactoryPrivate(
                new EventBeanTypedEventFactoryRuntime(null),
                EventTypeFactoryImpl.GetInstance(container),
                BEAN_STEM_SVC);

            SUPPORTBEAN_EVENTTTPE = MakeType(typeof(SupportBean));
            SUPPORTBEAN_S0_EVENTTTPE = MakeType(typeof(SupportBean_S0));
            SUPPORTBEAN_S1_EVENTTTPE = MakeType(typeof(SupportBean_S1));
            SUPPORTBEAN_S2_EVENTTTPE = MakeType(typeof(SupportBean_S2));
            SUPPORTBEAN_A_EVENTTTPE = MakeType(typeof(SupportBean_A));
            SUPPORTBEANCOMPLEXPROPS_EVENTTTPE = MakeType(typeof(SupportBeanComplexProps));
            SUPPORTBEANSIMPLE_EVENTTTPE = MakeType(typeof(SupportBeanSimple));

            BEAN_STEM_SVC = new BeanEventTypeStemService(
                null, null, PropertyResolutionStyle.CASE_SENSITIVE, AccessorStyle.NATIVE);

            STEM_BUILDER = new BeanEventTypeStemBuilder(null, PropertyResolutionStyle.CASE_SENSITIVE);

            METADATA_CLASS = name => new EventTypeMetadata(
                name, null,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.CLASS,
                NameAccessModifier.PROTECTED,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
        }

        public BeanEventType CreateBeanType(Type clazz)
        {
            if (clazz == typeof(SupportBean))
            {
                return SUPPORTBEAN_EVENTTTPE;
            }
            if (clazz == typeof(SupportBean_S0))
            {
                return SUPPORTBEAN_S0_EVENTTTPE;
            }
            if (clazz == typeof(SupportBean_A))
            {
                return SUPPORTBEAN_A_EVENTTTPE;
            }
            if (clazz == typeof(SupportBeanComplexProps))
            {
                return SUPPORTBEANCOMPLEXPROPS_EVENTTTPE;
            }
            if (clazz == typeof(SupportBeanSimple))
            {
                return SUPPORTBEANSIMPLE_EVENTTTPE;
            }
            if (clazz == typeof(SupportBean_S1))
            {
                return SUPPORTBEAN_S1_EVENTTTPE;
            }
            if (clazz == typeof(SupportBean_S2))
            {
                return SUPPORTBEAN_S2_EVENTTTPE;
            }
            throw new UnsupportedOperationException("Unrecognized type " + clazz.Name);
        }

        public EventType CreateMapType(IDictionary<string, object> map)
        {
            var metadata = new EventTypeMetadata(UuidGenerator.Generate(), null, EventTypeTypeClass.STREAM, EventTypeApplicationType.MAP, NameAccessModifier.PROTECTED, EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            return new MapEventType(
                metadata, map, null, null, null, null,
                BEAN_EVENT_TYPE_FACTORY);
        }

        private BeanEventType MakeType(Type clazz)
        {
            return new BeanEventType(
                Container,
                STEM_BUILDER.Make(clazz),
                METADATA_CLASS.Invoke(clazz.Name),
                BEAN_EVENT_TYPE_FACTORY,
                null, null, null, null);
        }
    }
} // end of namespace