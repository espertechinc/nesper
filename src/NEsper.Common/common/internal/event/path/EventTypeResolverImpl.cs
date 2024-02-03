///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
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
        private readonly IDictionary<string, EventType> locals;
        private readonly PathRegistry<string, EventType> path;
        private readonly EventTypeNameResolver publics;
        private readonly BeanEventTypeFactoryPrivate beanEventTypeFactoryPrivate;
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

        public EventTypeSPI Resolve(
            string name,
            string moduleName,
            NameAccessModifier accessModifier)
        {
            return (EventTypeSPI) Resolve(name, moduleName, accessModifier, publics, locals, path);
        }

        public static EventType Resolve(
            string name,
            string moduleName,
            NameAccessModifier accessModifier,
            EventTypeNameResolver publics,
            IDictionary<string, EventType> locals,
            PathRegistry<string, EventType> path)
        {
            EventType type;
            // public can only see public
            if (accessModifier == NameAccessModifier.PRECONFIGURED) {
                type = publics.GetTypeByName(name);
                // for create-schema the type may be defined by the same module
                if (type == null) {
                    type = locals.Get(name);
                }
            }
            else if (accessModifier == NameAccessModifier.PUBLIC || accessModifier == NameAccessModifier.INTERNAL) {
                // path-visibility can be provided as local
                var local = locals.Get(name);
                if (local != null) {
                    if (local.Metadata.AccessModifier == NameAccessModifier.PUBLIC ||
                        local.Metadata.AccessModifier == NameAccessModifier.INTERNAL) {
                        return local;
                    }
                }

                try {
                    var pair = path.GetAnyModuleExpectSingle(
                        name,
                        Collections.SingletonSet(moduleName));
                    type = pair?.First;
                }
                catch (PathException e) {
                    throw new EPException(e.Message, e);
                }
            }
            else {
                type = locals.Get(name);
            }

            if (type == null) {
                throw new EPException(
                    "Failed to find event type '" +
                    name +
                    "' among public types, modules-in-path or the current module itself");
            }

            return type;
        }

        public EventSerdeFactory EventSerdeFactory => eventSerdeFactory;
    }
} // end of namespace