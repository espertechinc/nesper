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
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.bean.service
{
	public class EventTypeRepositoryBeanTypeUtil {
	    public static void BuildBeanTypes(BeanEventTypeStemService beanEventTypeStemService,
	                                      EventTypeRepositoryImpl repo,
	                                      IDictionary<string, Type> beanTypes,
	                                      BeanEventTypeFactoryPrivate privateFactory,
	                                      IDictionary<string, ConfigurationCommonEventTypeBean> configs) {

	        if (beanTypes.IsEmpty()) {
	            beanTypes = new Dictionary<>();
	        }
	        AddPredefinedBeanEventTypes(beanTypes);

	        foreach (KeyValuePair<string, Type> beanType in beanTypes) {
	            BuildPublicBeanType(beanEventTypeStemService, repo, beanType.Key, beanType.Value, privateFactory, configs);
	        }
	    }

	    private static void BuildPublicBeanType(BeanEventTypeStemService beanEventTypeStemService, EventTypeRepository repo, string eventTypeName, Type clazz, BeanEventTypeFactoryPrivate privateFactory, IDictionary<string, ConfigurationCommonEventTypeBean> configs) {

	        // check existing type
	        EventType existingType = repo.GetTypeByName(eventTypeName);
	        if (existingType != null) {
	            if (existingType.Metadata.ApplicationType != EventTypeApplicationType.CLASS) {
	                throw new ConfigurationException("Event type named '" + eventTypeName +
	                        "' has already been declared with differing underlying type information: Class " + existingType.UnderlyingType.Name +
	                        " versus " + clazz.Name);
	            }
	            BeanEventType beanEventType = (BeanEventType) existingType;
	            if (beanEventType.UnderlyingType != clazz) {
	                throw new ConfigurationException("Event type named '" + eventTypeName +
	                        "' has already been declared with differing underlying type information: Class " + existingType.UnderlyingType.Name +
	                        " versus " + beanEventType.UnderlyingType);
	            }
	            return;
	        }

	        ConfigurationCommonEventTypeBean optionalConfig = configs.Get(eventTypeName);

	        // check-allocate bean-stem
	        BeanEventTypeStem stem = beanEventTypeStemService.GetCreateStem(clazz, optionalConfig);

	        // metadata
	        long publicId = CRC32Util.ComputeCRC32(eventTypeName);
	        EventTypeMetadata metadata = new EventTypeMetadata(eventTypeName, null, EventTypeTypeClass.STREAM, EventTypeApplicationType.CLASS, NameAccessModifier.PRECONFIGURED, EventTypeBusModifier.NONBUS, false, new EventTypeIdPair(publicId, -1));

	        // supertypes
	        EventType[] superTypes = GetSuperTypes(stem.SuperTypes, beanEventTypeStemService, repo, privateFactory, configs);
	        ISet<EventType> deepSuperTypes = GetDeepSupertypes(stem.DeepSuperTypes, beanEventTypeStemService, repo, privateFactory, configs);

	        // bean type
	        string startTS = optionalConfig == null ? null : optionalConfig.StartTimestampPropertyName;
	        string endTS = optionalConfig == null ? null : optionalConfig.EndTimestampPropertyName;
	        BeanEventType eventType = privateFactory.EventTypeFactory.CreateBeanType(stem, metadata, privateFactory, superTypes, deepSuperTypes, startTS, endTS);

	        repo.AddType(eventType);
	    }

	    private static EventType[] GetSuperTypes(Type[] superTypes, BeanEventTypeStemService beanEventTypeStemService, EventTypeRepository repo, BeanEventTypeFactoryPrivate privateFactory, IDictionary<string, ConfigurationCommonEventTypeBean> configs) {
	        if (superTypes == null || superTypes.Length == 0) {
	            return null;
	        }
	        EventType[] types = new EventType[superTypes.Length];
	        for (int i = 0; i < types.Length; i++) {
	            types[i] = GetBuildSuperType(superTypes[i], beanEventTypeStemService, repo, privateFactory, configs);
	        }
	        return types;
	    }

	    private static ISet<EventType> GetDeepSupertypes(ISet<Type> superTypes, BeanEventTypeStemService beanEventTypeStemService, EventTypeRepository repo, BeanEventTypeFactoryPrivate privateFactory, IDictionary<string, ConfigurationCommonEventTypeBean> configs) {
	        if (superTypes == null || superTypes.IsEmpty()) {
	            return Collections.EmptySet();
	        }
	        LinkedHashSet<EventType> supers = new LinkedHashSet<>(4);
	        foreach (Type clazz in superTypes) {
	            supers.Add(GetBuildSuperType(clazz, beanEventTypeStemService, repo, privateFactory, configs));
	        }
	        return supers;
	    }

	    public static EventType GetBuildSuperType(Type clazz, BeanEventTypeStemService beanEventTypeStemService, EventTypeRepository repo, BeanEventTypeFactoryPrivate privateFactory, IDictionary<string, ConfigurationCommonEventTypeBean> configs) {
	        IList<string> existingSuperTypeNames = beanEventTypeStemService.PublicClassToTypeNames.Get(clazz);
	        if (existingSuperTypeNames != null) {
	            EventType eventType = repo.GetTypeByName(existingSuperTypeNames[0]);
	            if (eventType != null) {
	                return eventType;
	            }
	        }
	        BuildPublicBeanType(beanEventTypeStemService, repo, clazz.Name, clazz, privateFactory, configs);
	        return repo.GetTypeByName(clazz.Name);
	    }

	    private static void AddPredefinedBeanEventTypes(IDictionary<string, Type> resolvedBeanEventTypes) {
	        AddPredefinedBeanEventType(typeof(StatementMetric), resolvedBeanEventTypes);
	        AddPredefinedBeanEventType(typeof(RuntimeMetric), resolvedBeanEventTypes);
	    }

	    private static void AddPredefinedBeanEventType(Type clazz, IDictionary<string, Type> resolvedBeanEventTypes) {
	        Type existing = resolvedBeanEventTypes.Get(clazz.Name);
	        if (existing != null && existing != clazz) {
	            throw new ConfigurationException("Predefined event type " + clazz.Name + " expected class " + clazz.Name + " but is already defined to another class " + existing.Name);
	        }
	        resolvedBeanEventTypes.Put(clazz.Name, clazz);
	    }
	}
} // end of namespace