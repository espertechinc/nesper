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
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.eventtyperepo
{
    public class EventTypeRepositoryOATypeUtil
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void BuildOATypes(
            EventTypeRepositoryImpl repo,
            IDictionary<string, ConfigurationCommonEventTypeObjectArray> objectArrayTypeConfigurations,
            IDictionary<string, IDictionary<string, object>> nestableObjectArrayNames,
            BeanEventTypeFactory beanEventTypeFactory,
            ImportService importService)
        {
            var creationOrder = EventTypeRepositoryUtil.GetCreationOrder(
                EmptySet<string>.Instance,
                nestableObjectArrayNames.Keys,
                objectArrayTypeConfigurations);

            foreach (var objectArrayName in creationOrder) {
                if (repo.GetTypeByName(objectArrayName) != null) {
                    continue;
                }

                var objectArrayConfig = objectArrayTypeConfigurations.Get(objectArrayName);
                var propertyTypes = nestableObjectArrayNames.Get(objectArrayName);
                propertyTypes = ResolveClassesForStringPropertyTypes(propertyTypes, importService);
                var propertyTypesCompiled = EventTypeUtility.CompileMapTypeProperties(propertyTypes, repo);

                AddNestableObjectArrayType(objectArrayName, propertyTypesCompiled, objectArrayConfig, beanEventTypeFactory, repo);
            }
        }
        
        private static void AddNestableObjectArrayType(
            string eventTypeName,
            IDictionary<string, object> propertyTypesMayHavePrimitive,
            ConfigurationCommonEventTypeObjectArray optionalConfig,
            BeanEventTypeFactory beanEventTypeFactory,
            EventTypeRepositoryImpl repo)
        {
            if (optionalConfig != null && optionalConfig.SuperTypes.Count > 1) {
                throw new EventAdapterException(ConfigurationCommonEventTypeObjectArray.SINGLE_SUPERTYPE_MSG);
            }

            var propertyTypes =
                EventTypeUtility.GetPropertyTypesNonPrimitive(propertyTypesMayHavePrimitive);
            var metadata = new EventTypeMetadata(
                eventTypeName,
                null,
                EventTypeTypeClass.APPLICATION,
                EventTypeApplicationType.OBJECTARR,
                NameAccessModifier.PRECONFIGURED,
                EventTypeBusModifier.NONBUS,
                false,
                new EventTypeIdPair(CRC32Util.ComputeCRC32(eventTypeName), -1));
            string[] superTypes = null;
            if (optionalConfig != null && optionalConfig.SuperTypes != null && !optionalConfig.SuperTypes.IsEmpty()) {
                superTypes = optionalConfig.SuperTypes.ToArray();
            }

            var newEventType = beanEventTypeFactory.EventTypeFactory.CreateObjectArray(
                metadata,
                propertyTypes,
                superTypes,
                optionalConfig != null ? optionalConfig.StartTimestampPropertyName : null,
                optionalConfig != null ? optionalConfig.EndTimestampPropertyName : null,
                beanEventTypeFactory,
                repo);

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

                // Since it's the same, return the existing type
                return;
            }

            repo.AddType(newEventType);
        }

        private static IDictionary<string, object> ResolveClassesForStringPropertyTypes(
            IDictionary<string, object> properties,
            ImportService importService)
        {
            IDictionary<string, object> propertyTypes = new LinkedHashMap<string, object>();
            foreach (var entry in properties) {
                var property = entry.Key;
                propertyTypes.Put(property, entry.Value);
                if (!(entry.Value is string)) {
                    continue;
                }

                var className = (string) entry.Value;
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

            try {
                var clazz = TypeHelper.GetTypeForSimpleName(type, importService.ClassForNameProvider);
                if (clazz == null) {
                    return null;
                }

                if (isArray) {
                    clazz = clazz.MakeArrayType();
                }

                return clazz;
            }
            catch (TypeLoadException e) {
                Log.Error($"Unable to load type \"{e.Message}\"", e);
                return null;
            }
        }
    }
} // end of namespace