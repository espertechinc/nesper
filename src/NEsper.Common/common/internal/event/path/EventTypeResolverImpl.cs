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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.serde.runtime.@event;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.path
{
    public class EventTypeResolverImpl : EventTypeResolver,
        EventTypeNameResolver
    {
        private readonly BeanEventTypeFactoryPrivate beanEventTypeFactoryPrivate;
        private readonly IDictionary<string, EventType> locals;
        private readonly PathRegistry<string, EventType> path;
        private readonly EventTypeNameResolver publics;
        private readonly EventSerdeFactory eventSerdeFactory;

        public EventTypeResolverImpl(
            IDictionary<string, EventType> locals,
            PathRegistry<string, EventType> path,
            EventTypeNameResolver publics,
            BeanEventTypeFactoryPrivate beanEventTypeFactoryPrivate,
            EventSerdeFactory eventSerdeFactory)
        {
            this.locals = locals;
            this.path = path;
            this.publics = publics;
            this.beanEventTypeFactoryPrivate = beanEventTypeFactoryPrivate;
            this.eventSerdeFactory = eventSerdeFactory;
        }

        public EventType GetTypeByName(string typeName)
        {
            var localType = locals.Get(typeName);
            if (localType != null) {
                return localType;
            }

            var publicType = publics.GetTypeByName(typeName);
            if (publicType != null) {
                return publicType;
            }

            try {
                var pair = path.GetAnyModuleExpectSingle(typeName, null);
                return pair?.First;
            }
            catch (PathException e) {
                throw new EPException("Event type name '" + typeName + "' is ambiguous: " + e.Message, e);
            }
        }

        public BeanEventType ResolvePrivateBean(
            Type clazz,
            bool publicFields)
        {
            return beanEventTypeFactoryPrivate.GetCreateBeanType(clazz, publicFields);
        }

        public EventTypeSPI Resolve(EventTypeMetadata metadata)
        {
            return (EventTypeSPI) Resolve(metadata, publics, locals, path);
        }

        public EventSerdeFactory GetEventSerdeFactory()
        {
            return eventSerdeFactory;
        }

        public static EventType Resolve(
            EventTypeMetadata metadata,
            EventTypeNameResolver publics,
            IDictionary<string, EventType> locals,
            PathRegistry<string, EventType> path)
        {
            EventTypeSPI type;
            // public can only see public
            if (metadata.AccessModifier == NameAccessModifier.PRECONFIGURED) {
                type = (EventTypeSPI) publics.GetTypeByName(metadata.Name);

                // for create-schema the type may be defined by the same module
                if (type == null) {
                    type = (EventTypeSPI) locals.Get(metadata.Name);
                }
            }
            else if (metadata.AccessModifier == NameAccessModifier.PUBLIC ||
                     metadata.AccessModifier == NameAccessModifier.INTERNAL) {
                // path-visibility can be provided as local
                var local = locals.Get(metadata.Name);
                if (local != null) {
                    if (local.Metadata.AccessModifier == NameAccessModifier.PUBLIC ||
                        local.Metadata.AccessModifier == NameAccessModifier.INTERNAL) {
                        return (EventTypeSPI) local;
                    }
                }

                try {
                    var pair = path.GetAnyModuleExpectSingle(
                        metadata.Name,
                        Collections.SingletonSet(metadata.ModuleName));
                    type = (EventTypeSPI) pair?.First;
                }
                catch (PathException e) {
                    throw new EPException(e.Message, e);
                }
            }
            else {
                type = (EventTypeSPI) locals.Get(metadata.Name);
            }

            if (type == null) {
                throw new EPException(
                    "Failed to find event type '" +
                    metadata.Name +
                    "' among public types, modules-in-path or the current module itself");
            }

            return type;
        }
    }
} // end of namespace