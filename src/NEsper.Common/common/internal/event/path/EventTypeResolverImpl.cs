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
        private readonly BeanEventTypeFactoryPrivate _beanEventTypeFactoryPrivate;
        private readonly IDictionary<string, EventType> _locals;
        private readonly PathRegistry<string, EventType> _path;
        private readonly EventTypeNameResolver _publics;
        private readonly EventSerdeFactory _eventSerdeFactory;

        public EventTypeResolverImpl(
            IDictionary<string, EventType> locals,
            PathRegistry<string, EventType> path,
            EventTypeNameResolver publics,
            BeanEventTypeFactoryPrivate beanEventTypeFactoryPrivate,
            EventSerdeFactory eventSerdeFactory)
        {
            _locals = locals;
            _path = path;
            _publics = publics;
            _beanEventTypeFactoryPrivate = beanEventTypeFactoryPrivate;
            _eventSerdeFactory = eventSerdeFactory;
        }

        public EventType GetTypeByName(string typeName)
        {
            var localType = _locals.Get(typeName);
            if (localType != null) {
                return localType;
            }

            var publicType = _publics.GetTypeByName(typeName);
            if (publicType != null) {
                return publicType;
            }

            try {
                var pair = _path.GetAnyModuleExpectSingle(typeName, null);
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
            return _beanEventTypeFactoryPrivate.GetCreateBeanType(clazz, publicFields);
        }

        public EventTypeSPI Resolve(string name, string moduleName, NameAccessModifier accessModifier)
        {
            return (EventTypeSPI) Resolve(name, moduleName, accessModifier, _publics, _locals, _path);
        }
        
        public EventSerdeFactory GetEventSerdeFactory()
        {
            return _eventSerdeFactory;
        }

        public static EventType Resolve(
            string name,
            string moduleName,
            NameAccessModifier accessModifier,
            EventTypeNameResolver publics,
            IDictionary<string, EventType> locals,
            PathRegistry<string, EventType> path)
        {
            EventTypeSPI type;
            // public can only see public
            if (accessModifier == NameAccessModifier.PRECONFIGURED) {
                type = (EventTypeSPI) publics.GetTypeByName(name);

                // for create-schema the type may be defined by the same module
                if (type == null) {
                    type = (EventTypeSPI) locals.Get(name);
                }
            }
            else if (accessModifier == NameAccessModifier.PUBLIC ||
                     accessModifier == NameAccessModifier.INTERNAL) {
                // path-visibility can be provided as local
                var local = locals.Get(name);
                if (local != null) {
                    if (local.Metadata.AccessModifier == NameAccessModifier.PUBLIC ||
                        local.Metadata.AccessModifier == NameAccessModifier.INTERNAL) {
                        return (EventTypeSPI) local;
                    }
                }

                try {
                    var pair = path.GetAnyModuleExpectSingle(
                        name,
                        Collections.SingletonSet(moduleName));
                    type = (EventTypeSPI) pair?.First;
                }
                catch (PathException e) {
                    throw new EPException(e.Message, e);
                }
            }
            else {
                type = (EventTypeSPI) locals.Get(name);
            }

            if (type == null) {
                throw new EPException(
                    "Failed to find event type '" +
                    name +
                    "' among public types, modules-in-path or the current module itself");
            }

            return type;
        }
    }
} // end of namespace