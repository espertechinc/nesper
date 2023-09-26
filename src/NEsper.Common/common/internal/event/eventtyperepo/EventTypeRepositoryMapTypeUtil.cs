///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.eventtyperepo
{
    public class EventTypeRepositoryMapTypeUtil
    {
        public static void BuildMapTypes(
            EventTypeRepositoryImpl repo,
            IDictionary<string, ConfigurationCommonEventTypeMap> mapTypeConfigurations,
            IDictionary<string, Properties> mapTypes,
            IDictionary<string, IDictionary<string, object>> nestableMapEvents,
            BeanEventTypeFactory beanEventTypeFactory,
            ImportService importService)
        {
            var creationOrder = EventTypeRepositoryUtil.GetCreationOrder(
                mapTypes.Keys,
                nestableMapEvents.Keys,
                mapTypeConfigurations);
            foreach (var mapName in creationOrder) {
                if (repo.GetTypeByName(mapName) != null) {
                    continue;
                }

                var mapConfig = mapTypeConfigurations.Get(mapName);
                if (mapTypes.TryGetValue(mapName, out var propertiesUnnested)) {
                    var propertyTypes = CreatePropertyTypes(propertiesUnnested, importService);
                    var propertyTypesCompiled = EventTypeUtility.CompileMapTypeProperties(propertyTypes, repo);
                    AddNestableMapType(mapName, propertyTypesCompiled, mapConfig, repo, beanEventTypeFactory, repo);
                }

                if (nestableMapEvents.TryGetValue(mapName, out var propertiesNestable)) {
                    var propertiesNestableCompiled =
                        EventTypeUtility.CompileMapTypeProperties(propertiesNestable, repo);
                    AddNestableMapType(
                        mapName,
                        propertiesNestableCompiled,
                        mapConfig,
                        repo,
                        beanEventTypeFactory,
                        repo);
                }
            }
        }

        protected internal static IDictionary<string, ICollection<string>> ToTypesReferences<T>(
            IDictionary<string, T> mapTypeConfigurations)
            where T : ConfigurationCommonEventTypeWithSupertype
        {
            IDictionary<string, ICollection<string>> result = new LinkedHashMap<string, ICollection<string>>();
            foreach (var entry in mapTypeConfigurations) {
                result.Put(entry.Key, entry.Value.SuperTypes);
            }

            return result;
        }

        private static void AddNestableMapType(
            string eventTypeName,
            IDictionary<string, object> propertyTypesMayHavePrimitive,
            ConfigurationCommonEventTypeMap optionalConfig,
            EventTypeRepositoryImpl repo,
            BeanEventTypeFactory beanEventTypeFactory,
            EventTypeNameResolver eventTypeNameResolver)
        {
            var metadata = new EventTypeMetadata(
                eventTypeName,
                null,
                EventTypeTypeClass.APPLICATION,
                EventTypeApplicationType.MAP,
                NameAccessModifier.PRECONFIGURED,
                EventTypeBusModifier.NONBUS,
                false,
                new EventTypeIdPair(CRC32Util.ComputeCRC32(eventTypeName), -1));

            var propertyTypes =
                EventTypeUtility.GetPropertyTypesNonPrimitive(propertyTypesMayHavePrimitive);
            string[] superTypes = null;
            if (optionalConfig != null && optionalConfig.SuperTypes != null && !optionalConfig.SuperTypes.IsEmpty()) {
                superTypes = optionalConfig.SuperTypes.ToArray();
            }

            var newEventType = beanEventTypeFactory.EventTypeFactory.CreateMap(
                metadata,
                propertyTypes,
                superTypes,
                optionalConfig?.StartTimestampPropertyName,
                optionalConfig?.EndTimestampPropertyName,
                beanEventTypeFactory,
                eventTypeNameResolver);

            var existingType = repo.GetTypeByName(eventTypeName);
            if (existingType != null) {
                // The existing type must be the same as the type createdStatement
                if (newEventType.EqualsCompareType(existingType) != null) {
                    var message = newEventType.CompareEquals(existingType);
                    throw new EPException(
                        "Event type named '" +
                        eventTypeName +
                        "' has already been declared with differing column name or type information: " +
                        message.Message,
                        message);
                }

                return;
            }

            repo.AddType(newEventType);
        }

        private static IDictionary<string, object> CreatePropertyTypes(
            Properties properties,
            ImportService importService)
        {
            IDictionary<string, object> propertyTypes = new LinkedHashMap<string, object>();
            foreach (var entry in properties) {
                var property = entry.Key;
                var className = entry.Value;
                var clazz = ResolveClassForTypeName(className, importService);
                if (clazz != null) {
                    propertyTypes.Put(property, clazz);
                }
            }

            return propertyTypes;
        }

        private static Type ResolveClassForTypeName(
            string type,
            ImportService importService)
        {
            var isArray = false;
            if (type != null && EventTypeUtility.IsPropertyArray(type)) {
                isArray = true;
                type = EventTypeUtility.GetPropertyRemoveArray(type);
            }

            if (type == null) {
                throw new ConfigurationException("A null value has been provided for the type");
            }

            var clazz = TypeHelper.GetTypeForSimpleName(type, importService.TypeResolver);
            if (clazz == null) {
                throw new ConfigurationException("The type '" + type + "' is not a recognized type");
            }

            if (isArray) {
                clazz = clazz.MakeArrayType();
            }

            return clazz;
        }
    }
} // end of namespace