///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events.avro;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.map;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
    public class SelectExprJoinWildcardProcessorFactory
    {
        public static SelectExprProcessor Create(
            ICollection<int> assignedTypeNumberStack,
            int statementId,
            string statementName,
            string[] streamNames,
            EventType[] streamTypes,
            EventAdapterService eventAdapterService,
            InsertIntoDesc insertIntoDesc,
            SelectExprEventTypeRegistry selectExprEventTypeRegistry,
            EngineImportService engineImportService,
            Attribute[] annotations,
            ConfigurationInformation configuration,
            TableService tableService,
            string engineURI)

        {
            if ((streamNames.Length < 2) || (streamTypes.Length < 2) || (streamNames.Length != streamTypes.Length))
            {
                throw new ArgumentException(
                    "Stream names and types parameter length is invalid, expected use of this class is for join statements");
            }

            // Create EventType of result join events
            var selectProperties = new LinkedHashMap<string, Object>();
            var streamTypesWTables = new EventType[streamTypes.Length];
            bool hasTables = false;
            for (int i = 0; i < streamTypes.Length; i++)
            {
                streamTypesWTables[i] = streamTypes[i];
                string tableName = TableServiceUtil.GetTableNameFromEventType(streamTypesWTables[i]);
                if (tableName != null)
                {
                    hasTables = true;
                    streamTypesWTables[i] = tableService.GetTableMetadata(tableName).PublicEventType;
                }
                selectProperties.Put(streamNames[i], streamTypesWTables[i]);
            }

            // If we have a name for this type, add it
            EventUnderlyingType representation = EventRepresentationUtil.GetRepresentation(
                annotations, configuration, AssignedType.NONE);
            EventType resultEventType;

            SelectExprProcessor processor = null;
            if (insertIntoDesc != null)
            {
                EventType existingType = eventAdapterService.GetEventTypeByName(insertIntoDesc.EventTypeName);
                if (existingType != null)
                {
                    processor = SelectExprInsertEventBeanFactory.GetInsertUnderlyingJoinWildcard(
                        eventAdapterService, existingType, streamNames, streamTypesWTables, engineImportService,
                        statementName, engineURI);
                }
            }

            if (processor == null)
            {
                if (insertIntoDesc != null)
                {
                    try
                    {
                        if (representation == EventUnderlyingType.MAP)
                        {
                            resultEventType = eventAdapterService.AddNestableMapType(
                                insertIntoDesc.EventTypeName, selectProperties, null, false, false, false, false, true);
                        }
                        else if (representation == EventUnderlyingType.OBJECTARRAY)
                        {
                            resultEventType = eventAdapterService.AddNestableObjectArrayType(
                                insertIntoDesc.EventTypeName, selectProperties, null, false, false, false, false, true,
                                false, null);
                        }
                        else if (representation == EventUnderlyingType.AVRO)
                        {
                            resultEventType = eventAdapterService.AddAvroType(
                                insertIntoDesc.EventTypeName, selectProperties, false, false, false, false, true,
                                annotations, null, statementName, engineURI);
                        }
                        else
                        {
                            throw new IllegalStateException("Unrecognized code " + representation);
                        }
                        selectExprEventTypeRegistry.Add(resultEventType);
                    }
                    catch (EventAdapterException ex)
                    {
                        throw new ExprValidationException(ex.Message, ex);
                    }
                }
                else
                {
                    if (representation == EventUnderlyingType.MAP)
                    {
                        resultEventType =
                            eventAdapterService.CreateAnonymousMapType(
                                statementId + "_join_" + CollectionUtil.ToString(assignedTypeNumberStack, "_"),
                                selectProperties, true);
                    }
                    else if (representation == EventUnderlyingType.OBJECTARRAY)
                    {
                        resultEventType =
                            eventAdapterService.CreateAnonymousObjectArrayType(
                                statementId + "_join_" + CollectionUtil.ToString(assignedTypeNumberStack, "_"),
                                selectProperties);
                    }
                    else if (representation == EventUnderlyingType.AVRO)
                    {
                        resultEventType =
                            eventAdapterService.CreateAnonymousAvroType(
                                statementId + "_join_" + CollectionUtil.ToString(assignedTypeNumberStack, "_"),
                                selectProperties, annotations, statementName, engineURI);
                    }
                    else
                    {
                        throw new IllegalStateException("Unrecognized enum " + representation);
                    }
                }
                if (resultEventType is ObjectArrayEventType)
                {
                    processor = new SelectExprJoinWildcardProcessorObjectArray(
                        streamNames, resultEventType, eventAdapterService);
                }
                else if (resultEventType is MapEventType)
                {
                    processor = new SelectExprJoinWildcardProcessorMap(
                        streamNames, resultEventType, eventAdapterService);
                }
                else if (resultEventType is AvroSchemaEventType)
                {
                    processor = eventAdapterService.EventAdapterAvroHandler.GetOutputFactory().MakeJoinWildcard(
                        streamNames, resultEventType, eventAdapterService);
                }
            }

            if (!hasTables)
            {
                return processor;
            }
            return new SelectExprJoinWildcardProcessorTableRows(streamTypes, processor, tableService);
        }
    }
} // end of namespace
