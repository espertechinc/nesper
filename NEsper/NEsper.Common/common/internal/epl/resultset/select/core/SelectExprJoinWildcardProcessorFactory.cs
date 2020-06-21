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
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.resultset.select.eval;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.compiletime;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class SelectExprJoinWildcardProcessorFactory
    {
        public static SelectExprProcessorForgeWForgables Create(
            SelectProcessorArgs args,
            InsertIntoDesc insertIntoDesc,
            Func<String, String> eventTypeNamePostfix) 
        {
            var streamNames = args.TypeService.StreamNames;
            var streamTypes = args.TypeService.EventTypes;
            var moduleName = args.ModuleName;
            var additionalForgeables = new List<StmtClassForgeableFactory>();
            
            if (streamNames.Length < 2 || streamTypes.Length < 2 || streamNames.Length != streamTypes.Length) {
                throw new ArgumentException(
                    "Stream names and types parameter length is invalid, expected use of this class is for join statements");
            }

            // Create EventType of result join events
            var selectProperties = new LinkedHashMap<string, object>();
            var streamTypesWTables = new EventType[streamTypes.Length];
            var hasTables = false;
            for (var i = 0; i < streamTypes.Length; i++) {
                streamTypesWTables[i] = streamTypes[i];
                var table = args.TableCompileTimeResolver.ResolveTableFromEventType(streamTypesWTables[i]);
                if (table != null) {
                    hasTables = true;
                    streamTypesWTables[i] = table.PublicEventType;
                }

                selectProperties.Put(streamNames[i], streamTypesWTables[i]);
            }

            // If we have a name for this type, add it
            var representation = EventRepresentationUtil.GetRepresentation(
                args.Annotations,
                args.Configuration,
                AssignedType.NONE);
            EventType resultEventType;

            SelectExprProcessorForge processor = null;
            if (insertIntoDesc != null) {
                var existingType = args.EventTypeCompileTimeResolver.GetTypeByName(insertIntoDesc.EventTypeName);
                if (existingType != null) {
                    processor = SelectExprInsertEventBeanFactory.GetInsertUnderlyingJoinWildcard(
                        existingType,
                        streamNames,
                        streamTypesWTables,
                        args.ImportService,
                        args.StatementName,
                        args.EventTypeAvroHandler);
                }
            }

            if (processor == null) {
                if (insertIntoDesc != null) {
                    var eventTypeName = eventTypeNamePostfix.Invoke(insertIntoDesc.EventTypeName);
                    var visibility =
                        args.CompileTimeServices.ModuleVisibilityRules.GetAccessModifierEventType(
                            args.StatementRawInfo,
                            eventTypeName);
                    var metadata = new Func<EventTypeApplicationType, EventTypeMetadata>(
                        apptype => new EventTypeMetadata(
                            eventTypeName,
                            moduleName,
                            EventTypeTypeClass.STREAM,
                            apptype,
                            visibility,
                            EventTypeBusModifier.NONBUS,
                            false,
                            EventTypeIdPair.Unassigned()));
                    if (representation == EventUnderlyingType.MAP) {
                        IDictionary<string, object> propertyTypes =
                            EventTypeUtility.GetPropertyTypesNonPrimitive(selectProperties);
                        resultEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                            metadata.Invoke(EventTypeApplicationType.MAP),
                            propertyTypes,
                            null,
                            null,
                            null,
                            null,
                            args.BeanEventTypeFactoryPrivate,
                            args.EventTypeCompileTimeResolver);
                    }
                    else if (representation == EventUnderlyingType.OBJECTARRAY) {
                        IDictionary<string, object> propertyTypes =
                            EventTypeUtility.GetPropertyTypesNonPrimitive(selectProperties);
                        resultEventType = BaseNestableEventUtil.MakeOATypeCompileTime(
                            metadata.Invoke(EventTypeApplicationType.OBJECTARR),
                            propertyTypes,
                            null,
                            null,
                            null,
                            null,
                            args.BeanEventTypeFactoryPrivate,
                            args.EventTypeCompileTimeResolver);
                    }
                    else if (representation == EventUnderlyingType.AVRO) {
                        resultEventType = args.EventTypeAvroHandler.NewEventTypeFromNormalized(
                            metadata.Invoke(EventTypeApplicationType.AVRO),
                            args.EventTypeCompileTimeResolver,
                            EventBeanTypedEventFactoryCompileTime.INSTANCE,
                            selectProperties,
                            args.Annotations,
                            null,
                            null,
                            null,
                            args.StatementName);
                    } else if (representation == EventUnderlyingType.JSON) {
                        EventTypeForgeablesPair pair = JsonEventTypeUtility.MakeJsonTypeCompileTimeNewType(
                            metadata.Invoke(EventTypeApplicationType.JSON),
                            selectProperties,
                            null,
                            null,
                            args.StatementRawInfo,
                            args.CompileTimeServices);
                        resultEventType = pair.EventType;
                        additionalForgeables.AddAll(pair.AdditionalForgeables);
                    }
                    else {
                        throw new IllegalStateException("Unrecognized code " + representation);
                    }

                    args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                }
                else {
                    var eventTypeName = eventTypeNamePostfix.Invoke(
                        args.CompileTimeServices.EventTypeNameGeneratorStatement.AnonymousTypeName);
                    IDictionary<string, object> propertyTypes =
                        EventTypeUtility.GetPropertyTypesNonPrimitive(selectProperties);
                    var metadata = new Func<EventTypeApplicationType, EventTypeMetadata>(
                        type => new EventTypeMetadata(
                            eventTypeName,
                            moduleName,
                            EventTypeTypeClass.STATEMENTOUT,
                            type,
                            NameAccessModifier.TRANSIENT,
                            EventTypeBusModifier.NONBUS,
                            false,
                            EventTypeIdPair.Unassigned()));
                    if (representation == EventUnderlyingType.MAP) {
                        resultEventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                            metadata.Invoke(EventTypeApplicationType.MAP),
                            propertyTypes,
                            null,
                            null,
                            null,
                            null,
                            args.BeanEventTypeFactoryPrivate,
                            args.EventTypeCompileTimeResolver);
                    }
                    else if (representation == EventUnderlyingType.OBJECTARRAY) {
                        resultEventType = BaseNestableEventUtil.MakeOATypeCompileTime(
                            metadata.Invoke(EventTypeApplicationType.OBJECTARR),
                            propertyTypes,
                            null,
                            null,
                            null,
                            null,
                            args.BeanEventTypeFactoryPrivate,
                            args.EventTypeCompileTimeResolver);
                    }
                    else if (representation == EventUnderlyingType.AVRO) {
                        resultEventType = args.EventTypeAvroHandler.NewEventTypeFromNormalized(
                            metadata.Invoke(EventTypeApplicationType.AVRO),
                            args.EventTypeCompileTimeResolver,
                            args.BeanEventTypeFactoryPrivate.EventBeanTypedEventFactory,
                            selectProperties,
                            args.Annotations,
                            null,
                            null,
                            null,
                            args.StatementName);
                    } else if (representation == EventUnderlyingType.JSON) {
                        EventTypeForgeablesPair pair = JsonEventTypeUtility.MakeJsonTypeCompileTimeNewType(
                            metadata.Invoke(EventTypeApplicationType.JSON),
                            propertyTypes,
                            null,
                            null,
                            args.StatementRawInfo,
                            args.CompileTimeServices);
                        resultEventType = pair.EventType;
                        additionalForgeables.AddAll(pair.AdditionalForgeables);
                    }
                    else {
                        throw new IllegalStateException("Unrecognized enum " + representation);
                    }

                    args.EventTypeCompileTimeRegistry.NewType(resultEventType);
                }

                // NOTE: Processors herein maintain their own result-event-type as they become inner types,
                //       for example "insert into VariantStream select * from A, B"
                if (resultEventType is ObjectArrayEventType) {
                    processor = new SelectEvalJoinWildcardProcessorObjectArray(streamNames, resultEventType);
                }
                else if (resultEventType is MapEventType) {
                    processor = new SelectEvalJoinWildcardProcessorMap(streamNames, resultEventType);
                }
                else if (resultEventType is AvroSchemaEventType) {
                    processor = args.EventTypeAvroHandler.OutputFactory.MakeJoinWildcard(streamNames, resultEventType);
                } else if (resultEventType is JsonEventType) {
                    processor = new SelectEvalJoinWildcardProcessorJson(streamNames, (JsonEventType) resultEventType);
                }
            }

            if (!hasTables) {
                return new SelectExprProcessorForgeWForgables(processor, additionalForgeables);
            }
            processor = new SelectEvalJoinWildcardProcessorTableRows(streamTypes, processor, args.TableCompileTimeResolver);
            return new SelectExprProcessorForgeWForgables(processor, additionalForgeables);
        }
    }
} // end of namespace