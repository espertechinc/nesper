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
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.service
{
    public class EventTypeRepositoryBeanTypeUtil
    {
        public static void BuildBeanTypes(
            BeanEventTypeStemService beanEventTypeStemService,
            EventTypeRepositoryImpl repo,
            IDictionary<string, Type> beanTypes,
            BeanEventTypeFactoryPrivate privateFactory,
            IDictionary<string, ConfigurationCommonEventTypeBean> configs)
        {
            if (beanTypes.IsEmpty()) {
                beanTypes = new Dictionary<string, Type>();
            }

            AddPredefinedBeanEventTypes(beanTypes);

            foreach (var beanType in beanTypes) {
                if (repo.GetTypeByName(beanType.Key) == null) {
                    BuildPublicBeanType(
                        beanEventTypeStemService,
                        repo,
                        beanType.Key,
                        beanType.Value,
                        privateFactory,
                        configs);
                }
            }
        }

        private static void BuildPublicBeanType(
            BeanEventTypeStemService beanEventTypeStemService,
            EventTypeRepository repo,
            string eventTypeName,
            Type clazz,
            BeanEventTypeFactoryPrivate privateFactory,
            IDictionary<string, ConfigurationCommonEventTypeBean> configs)
        {
            // check existing type
            var existingType = repo.GetTypeByName(eventTypeName);
            if (existingType != null) {
                if (existingType.Metadata.ApplicationType != EventTypeApplicationType.CLASS) {
                    throw new ConfigurationException(
                        "Event type named '" +
                        eventTypeName +
                        "' has already been declared with differing underlying type information: Class " +
                        existingType.UnderlyingType.FullName +
                        " versus " +
                        clazz.Name);
                }

                var beanEventType = (BeanEventType) existingType;
                if (beanEventType.UnderlyingType != clazz) {
                    throw new ConfigurationException(
                        "Event type named '" +
                        eventTypeName +
                        "' has already been declared with differing underlying type information: Class " +
                        existingType.UnderlyingType.FullName +
                        " versus " +
                        beanEventType.UnderlyingType);
                }

                return;
            }

            var optionalConfig = configs.Get(eventTypeName);

            // check-allocate bean-stem
            var stem = beanEventTypeStemService.GetCreateStem(clazz, optionalConfig);

            // metadata
            var publicId = CRC32Util.ComputeCRC32(eventTypeName);

            var metadata = new EventTypeMetadata(
                eventTypeName,
                null,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.CLASS,
                NameAccessModifier.PRECONFIGURED,
                EventTypeBusModifier.NONBUS,
                false,
                new EventTypeIdPair(publicId, -1));

            // supertypes
            var superTypes = GetSuperTypes(stem.SuperTypes, beanEventTypeStemService, repo, privateFactory, configs);
            var deepSuperTypes = GetDeepSupertypes(
                stem.DeepSuperTypes,
                beanEventTypeStemService,
                repo,
                privateFactory,
                configs);

            // bean type
            var startTS = optionalConfig == null ? null : optionalConfig.StartTimestampPropertyName;
            var endTS = optionalConfig == null ? null : optionalConfig.EndTimestampPropertyName;
            var eventType = privateFactory.EventTypeFactory.CreateBeanType(
                stem,
                metadata,
                privateFactory,
                superTypes,
                deepSuperTypes,
                startTS,
                endTS);

            repo.AddType(eventType);
        }

        private static EventType[] GetSuperTypes(
            Type[] superTypes,
            BeanEventTypeStemService beanEventTypeStemService,
            EventTypeRepository repo,
            BeanEventTypeFactoryPrivate privateFactory,
            IDictionary<string, ConfigurationCommonEventTypeBean> configs)
        {
            if (superTypes == null || superTypes.Length == 0) {
                return null;
            }

            var types = new EventType[superTypes.Length];
            for (var i = 0; i < types.Length; i++) {
                types[i] = GetBuildSuperType(superTypes[i], beanEventTypeStemService, repo, privateFactory, configs);
            }

            return types;
        }

        private static ISet<EventType> GetDeepSupertypes(
            ISet<Type> superTypes,
            BeanEventTypeStemService beanEventTypeStemService,
            EventTypeRepository repo,
            BeanEventTypeFactoryPrivate privateFactory,
            IDictionary<string, ConfigurationCommonEventTypeBean> configs)
        {
            if (superTypes == null || superTypes.IsEmpty()) {
                return new EmptySet<EventType>();
            }

            var supers = new LinkedHashSet<EventType>();
            foreach (var clazz in superTypes) {
                supers.Add(GetBuildSuperType(clazz, beanEventTypeStemService, repo, privateFactory, configs));
            }

            return supers;
        }

        public static EventType GetBuildSuperType(
            Type clazz,
            BeanEventTypeStemService beanEventTypeStemService,
            EventTypeRepository repo,
            BeanEventTypeFactoryPrivate privateFactory,
            IDictionary<string, ConfigurationCommonEventTypeBean> configs)
        {
            var existingSuperTypeNames = beanEventTypeStemService.PublicClassToTypeNames.Get(clazz);
            if (existingSuperTypeNames != null) {
                var eventType = repo.GetTypeByName(existingSuperTypeNames[0]);
                if (eventType != null) {
                    return eventType;
                }
            }

            BuildPublicBeanType(beanEventTypeStemService, repo, clazz.FullName, clazz, privateFactory, configs);
            return repo.GetTypeByName(clazz.FullName);
        }

        private static void AddPredefinedBeanEventTypes(IDictionary<string, Type> resolvedBeanEventTypes)
        {
            AddPredefinedBeanEventType(typeof(StatementMetric), resolvedBeanEventTypes);
            AddPredefinedBeanEventType(typeof(RuntimeMetric), resolvedBeanEventTypes);
        }

        private static void AddPredefinedBeanEventType(
            Type clazz,
            IDictionary<string, Type> resolvedBeanEventTypes)
        {
            var clazzFullName = clazz.FullName;
            var existing = resolvedBeanEventTypes.Get(clazzFullName);
            if (existing != null && existing != clazz) {
                throw new ConfigurationException(
                    "Predefined event type " +
                    clazzFullName +
                    " expected class " +
                    clazz.CleanName() +
                    " but is already defined to another class " +
                    existing.CleanName());
            }

            resolvedBeanEventTypes.Put(clazzFullName, clazz);
        }
    }
} // end of namespace