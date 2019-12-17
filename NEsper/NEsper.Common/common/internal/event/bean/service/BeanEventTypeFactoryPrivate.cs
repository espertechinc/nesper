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
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.service
{
    public class BeanEventTypeFactoryPrivate : BeanEventTypeFactory
    {
        private readonly BeanEventTypeStemService stemFactory;

        private readonly IDictionary<Type, BeanEventType> types = new Dictionary<Type, BeanEventType>();

        public BeanEventTypeFactoryPrivate(
            EventBeanTypedEventFactory typedEventFactory,
            EventTypeFactory eventTypeFactory,
            BeanEventTypeStemService stemFactory)
        {
            EventBeanTypedEventFactory = typedEventFactory;
            EventTypeFactory = eventTypeFactory;
            this.stemFactory = stemFactory;
        }

        public NameAccessModifier Visibility => NameAccessModifier.TRANSIENT;

        public BeanEventType GetCreateBeanType(Type clazz)
        {
            var existing = types.Get(clazz);
            if (existing != null) {
                return existing;
            }

            // check-allocate bean-stem
            var stem = stemFactory.GetCreateStem(clazz, null);

            // metadata
            var metadata = new EventTypeMetadata(
                clazz.Name,
                null,
                EventTypeTypeClass.BEAN_INCIDENTAL,
                EventTypeApplicationType.CLASS,
                NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS,
                false,
                ComputeTypeId(clazz.Name));

            // supertypes
            var superTypes = GetSuperTypes(stem.SuperTypes);
            var deepSuperTypes = GetDeepSupertypes(stem.DeepSuperTypes);

            // bean type
            var eventType = EventTypeFactory.CreateBeanType(
                stem,
                metadata,
                this,
                superTypes,
                deepSuperTypes,
                null,
                null);

            types.Put(clazz, eventType);
            return eventType;
        }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }

        public EventTypeFactory EventTypeFactory { get; }

        public EventTypeIdPair ComputeTypeId(string eventTypeName)
        {
            var id = CRC32Util.ComputeCRC32(eventTypeName);
            return new EventTypeIdPair(0, id);
        }

        private EventType[] GetSuperTypes(Type[] superTypes)
        {
            if (superTypes == null || superTypes.Length == 0) {
                return null;
            }

            var types = new EventType[superTypes.Length];
            for (var i = 0; i < types.Length; i++) {
                types[i] = GetCreateBeanType(superTypes[i]);
            }

            return types;
        }

        private ISet<EventType> GetDeepSupertypes(ISet<Type> superTypes)
        {
            if (superTypes == null || superTypes.IsEmpty()) {
                return new EmptySet<EventType>();
            }

            var supers = new LinkedHashSet<EventType>();
            foreach (var clazz in superTypes) {
                supers.Add(GetCreateBeanType(clazz));
            }

            return supers;
        }
    }
} // end of namespace