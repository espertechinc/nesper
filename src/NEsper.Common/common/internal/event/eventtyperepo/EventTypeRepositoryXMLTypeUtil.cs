///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.eventtyperepo
{
    public class EventTypeRepositoryXMLTypeUtil
    {
        public static void BuildXMLTypes(
            EventTypeRepositoryImpl repo,
            IDictionary<string, ConfigurationCommonEventTypeXMLDOM> eventTypesXMLDOM,
            BeanEventTypeFactory beanEventTypeFactory,
            XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory,
            IResourceManager resourceManager)
        {
            // Add from the configuration the XML DOM names and type def
            foreach (var entry in eventTypesXMLDOM) {
                if (repo.GetTypeByName(entry.Key) != null) {
                    continue;
                }
                
                SchemaModel schemaModel = null;
                if (entry.Value.SchemaResource != null || entry.Value.SchemaText != null) {
                    try {
                        schemaModel = XSDSchemaMapper.LoadAndMap(
                            entry.Value.SchemaResource,
                            entry.Value.SchemaText,
                            resourceManager);
                    }
                    catch (Exception ex) {
                        throw new ConfigurationException(ex.Message, ex);
                    }
                }

                try {
                    AddXMLDOMType(
                        repo,
                        entry.Key,
                        entry.Value,
                        schemaModel,
                        beanEventTypeFactory,
                        xmlFragmentEventTypeFactory);
                }
                catch (Exception ex) {
                    throw new ConfigurationException(ex.Message, ex);
                }
            }
        }

        private static void AddXMLDOMType(
            EventTypeRepositoryImpl repo,
            string eventTypeName,
            ConfigurationCommonEventTypeXMLDOM detail,
            SchemaModel schemaModel,
            BeanEventTypeFactory beanEventTypeFactory,
            XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory)
        {
            if (detail.RootElementName == null) {
                throw new EventAdapterException("Required root element name has not been supplied");
            }

            var existingType = repo.GetTypeByName(eventTypeName);
            if (existingType != null) {
                var message = "Event type named '" +
                              eventTypeName +
                              "' has already been declared with differing column name or type information";
                throw new ConfigurationException(message);
            }

            var propertyAgnostic = detail.SchemaResource == null && detail.SchemaText == null;
            var metadata = new EventTypeMetadata(
                eventTypeName,
                null,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.XML,
                NameAccessModifier.PRECONFIGURED,
                EventTypeBusModifier.BUS,
                propertyAgnostic,
                new EventTypeIdPair(CRC32Util.ComputeCRC32(eventTypeName), -1));
            var type = beanEventTypeFactory.EventTypeFactory.CreateXMLType(
                metadata,
                detail,
                schemaModel,
                null,
                metadata.Name,
                beanEventTypeFactory,
                xmlFragmentEventTypeFactory,
                repo);
            repo.AddType(type);

            if (type is SchemaXMLEventType) {
                xmlFragmentEventTypeFactory.AddRootType((SchemaXMLEventType) type);
            }
        }
    }
} // end of namespace