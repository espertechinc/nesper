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
using com.espertech.esper.common.@internal.supportunit.bean;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.supportunit.@event
{
    public class SupportEventTypeFactory
    {
        private readonly IContainer _container;

        private SupportEventTypeFactory(IContainer container)
        {
            _container = container;

            STEM_BUILDER = new BeanEventTypeStemBuilder(
                null,
                PropertyResolutionStyle.CASE_SENSITIVE);

            BEAN_STEM_SVC = new BeanEventTypeStemService(
                null,
                null,
                PropertyResolutionStyle.CASE_SENSITIVE,
                AccessorStyle.NATIVE);

            BEAN_EVENT_TYPE_FACTORY = new BeanEventTypeFactoryPrivate(
                new EventBeanTypedEventFactoryRuntime(null),
                EventTypeFactoryImpl.GetInstance(container),
                BEAN_STEM_SVC);

            METADATA_CLASS = name => new EventTypeMetadata(
                name,
                null,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.CLASS,
                NameAccessModifier.INTERNAL,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());

            SUPPORTBEAN_EVENTTTPE = MakeType(typeof(SupportBean));
            SUPPORTBEAN_S0_EVENTTTPE = MakeType(typeof(SupportBean_S0));
            SUPPORTBEAN_S1_EVENTTTPE = MakeType(typeof(SupportBean_S1));
            SUPPORTBEAN_S2_EVENTTTPE = MakeType(typeof(SupportBean_S2));
            SUPPORTBEAN_S3_EVENTTTPE = MakeType(typeof(SupportBean_S3));
            SUPPORTBEAN_S4_EVENTTTPE = MakeType(typeof(SupportBean_S4));
            SUPPORTBEAN_A_EVENTTTPE = MakeType(typeof(SupportBean_A));
            SUPPORTMARKETDATABEAN_EVENTTTPE = MakeType(typeof(SupportMarketDataBean));
            SUPPORTBEANSTRING_EVENTTTPE = MakeType(typeof(SupportBeanString));
            SUPPORTBEANCOMPLEXPROPS_EVENTTTPE = MakeType(typeof(SupportBeanComplexProps));
            SUPPORTLEGACYBEAN_EVENTTTPE = MakeType(typeof(SupportLegacyBean));
            SUPPORTBEANCOMBINEDPROPS_EVENTTTPE = MakeType(typeof(SupportBeanCombinedProps));
            SUPPORTBEANPROPERTYNAMES_EVENTTTPE = MakeType(typeof(SupportBeanPropertyNames));
            SUPPORTBEANSIMPLE_EVENTTTPE = MakeType(typeof(SupportBeanSimple));
            SUPPORTBEANITERABLEPROPS_EVENTTTPE = MakeType(typeof(SupportBeanIterableProps));
            SUPPORTABCDEEVENT_EVENTTTPE = MakeType(typeof(SupportABCDEEvent));
            SUPPORTBEANITERABLEPROPSCONTAINER_EVENTTYPE = MakeType(typeof(SupportBeanIterablePropsContainer));
        }

        public static SupportEventTypeFactory GetInstance(IContainer container)
        {
            return container.ResolveSingleton(() => new SupportEventTypeFactory(container));
        }

        public static void RegisterSingleton(IContainer container)
        {
            container.Register<SupportEventTypeFactory>(
                xx => new SupportEventTypeFactory(container),
                Lifespan.Singleton);
        }

        public readonly BeanEventTypeStemService BEAN_STEM_SVC;
        public readonly BeanEventTypeFactory BEAN_EVENT_TYPE_FACTORY;
        public readonly BeanEventTypeStemBuilder STEM_BUILDER;

        public readonly Func<string, EventTypeMetadata> METADATA_CLASS;

        public readonly BeanEventType SUPPORTBEAN_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEAN_S0_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEAN_S1_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEAN_S2_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEAN_S3_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEAN_S4_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEAN_A_EVENTTTPE;
        public readonly BeanEventType SUPPORTMARKETDATABEAN_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEANSTRING_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEANCOMPLEXPROPS_EVENTTTPE;
        public readonly BeanEventType SUPPORTLEGACYBEAN_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEANCOMBINEDPROPS_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEANPROPERTYNAMES_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEANSIMPLE_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEANITERABLEPROPS_EVENTTTPE;
        public readonly BeanEventType SUPPORTABCDEEVENT_EVENTTTPE;
        public readonly BeanEventType SUPPORTBEANITERABLEPROPSCONTAINER_EVENTTYPE;

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
            if (clazz == typeof(SupportMarketDataBean))
            {
                return SUPPORTMARKETDATABEAN_EVENTTTPE;
            }
            if (clazz == typeof(SupportBeanComplexProps))
            {
                return SUPPORTBEANCOMPLEXPROPS_EVENTTTPE;
            }
            if (clazz == typeof(SupportBeanSimple))
            {
                return SUPPORTBEANSIMPLE_EVENTTTPE;
            }
            if (clazz == typeof(SupportBeanCombinedProps))
            {
                return SUPPORTBEANCOMBINEDPROPS_EVENTTTPE;
            }
            if (clazz == typeof(SupportBean_S1))
            {
                return SUPPORTBEAN_S1_EVENTTTPE;
            }
            if (clazz == typeof(SupportBean_S2))
            {
                return SUPPORTBEAN_S2_EVENTTTPE;
            }
            if (clazz == typeof(SupportBean_S3))
            {
                return SUPPORTBEAN_S3_EVENTTTPE;
            }
            if (clazz == typeof(SupportBean_S4))
            {
                return SUPPORTBEAN_S4_EVENTTTPE;
            }
            if (clazz == typeof(SupportABCDEEvent))
            {
                return SUPPORTABCDEEVENT_EVENTTTPE;
            }
            throw new UnsupportedOperationException("Unrecognized type " + clazz.Name);
        }

        public EventType CreateMapType(IDictionary<string, object> map)
        {
            EventTypeMetadata metadata = new EventTypeMetadata(
                UuidGenerator.Generate(),
                null,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.MAP,
                NameAccessModifier.INTERNAL,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            return new MapEventType(metadata, map, null, null, null, null, BEAN_EVENT_TYPE_FACTORY);
        }

        private BeanEventType MakeType(Type clazz)
        {
            return new BeanEventType(
                _container,
                STEM_BUILDER.Make(clazz),
                METADATA_CLASS.Invoke(clazz.Name),
                BEAN_EVENT_TYPE_FACTORY,
                null, null, null, null);
        }
    }
} // end of namespace
