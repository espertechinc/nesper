///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.util;


namespace com.espertech.esper.epl.core
{
	public class SelectExprJoinWildcardProcessorFactory
	{
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="assignedTypeNumberStack">The assigned type number stack.</param>
        /// <param name="statementId">The statement identifier.</param>
        /// <param name="streamNames">name of each stream</param>
        /// <param name="streamTypes">type of each stream</param>
        /// <param name="eventAdapterService">service for generating events and handling event types</param>
        /// <param name="insertIntoDesc">describes the insert-into clause</param>
        /// <param name="selectExprEventTypeRegistry">registry for event type to statements</param>
        /// <param name="methodResolutionService">for resolving writable properties</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="tableService">The table service.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">Stream names and types parameter length is invalid, expected use of this class is for join statements</exception>
        /// <exception cref="ExprValidationException"></exception>
        /// <throws>com.espertech.esper.epl.expression.core.ExprValidationException if the expression validation failed</throws>
	    public static SelectExprProcessor Create(
	        ICollection<int> assignedTypeNumberStack,
	        string statementId,
	        string[] streamNames,
	        EventType[] streamTypes,
	        EventAdapterService eventAdapterService,
	        InsertIntoDesc insertIntoDesc,
	        SelectExprEventTypeRegistry selectExprEventTypeRegistry,
	        MethodResolutionService methodResolutionService,
	        Attribute[] annotations,
	        ConfigurationInformation configuration,
	        TableService tableService)
	{
	    if ((streamNames.Length < 2) || (streamTypes.Length < 2) || (streamNames.Length != streamTypes.Length))
	    {
	        throw new ArgumentException(
	            "Stream names and types parameter length is invalid, expected use of this class is for join statements");
	    }

	    // Create EventType of result join events
	    var eventTypeMap = new LinkedHashMap<string, object>();
	    var streamTypesWTables = new EventType[streamTypes.Length];
	    var hasTables = false;
	    for (var i = 0; i < streamTypes.Length; i++)
	    {
	        streamTypesWTables[i] = streamTypes[i];
	        var tableName = TableServiceUtil.GetTableNameFromEventType(streamTypesWTables[i]);
	        if (tableName != null)
	        {
	            hasTables = true;
	            streamTypesWTables[i] = tableService.GetTableMetadata(tableName).PublicEventType;
	        }
	        eventTypeMap.Put(streamNames[i], streamTypesWTables[i]);
	    }

	    // If we have a name for this type, add it
	    var useMap = EventRepresentationUtil.IsMap(annotations, configuration, AssignedType.NONE);
	    EventType resultEventType;

	    SelectExprProcessor processor = null;
	    if (insertIntoDesc != null)
	    {
	        EventType existingType = eventAdapterService.GetEventTypeByName(insertIntoDesc.EventTypeName);
	        if (existingType != null)
	        {
	            processor = SelectExprInsertEventBeanFactory.GetInsertUnderlyingJoinWildcard(
	                eventAdapterService, existingType, streamNames, streamTypesWTables,
	                methodResolutionService.EngineImportService);
	        }
	    }

	    if (processor == null)
	    {
	        if (insertIntoDesc != null)
	        {
	            try
	            {
	                if (useMap)
	                {
	                    resultEventType = eventAdapterService.AddNestableMapType(
	                        insertIntoDesc.EventTypeName, eventTypeMap, null, false, false, false, false, true);
	                }
	                else
	                {
	                    resultEventType = eventAdapterService.AddNestableObjectArrayType(
	                        insertIntoDesc.EventTypeName, eventTypeMap, null, false, false, false, false, true, false, null);
	                }
	                selectExprEventTypeRegistry.Add(resultEventType);
	            }
	            catch (EventAdapterException ex)
	            {
	                throw new ExprValidationException(ex.Message);
	            }
	        }
	        else
	        {
	            if (useMap)
	            {
	                resultEventType =
	                    eventAdapterService.CreateAnonymousMapType(
	                        statementId + "_join_" + CollectionUtil.ToString(assignedTypeNumberStack, "_"), eventTypeMap);
	            }
	            else
	            {
	                resultEventType =
	                    eventAdapterService.CreateAnonymousObjectArrayType(
	                        statementId + "_join_" + CollectionUtil.ToString(assignedTypeNumberStack, "_"), eventTypeMap);
	            }
	        }
	        if (resultEventType is ObjectArrayEventType)
	        {
	            processor = new SelectExprJoinWildcardProcessorObjectArray(
	                streamNames, resultEventType, eventAdapterService);
	        }
	        else
	        {
	            processor = new SelectExprJoinWildcardProcessorMap(streamNames, resultEventType, eventAdapterService);
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
