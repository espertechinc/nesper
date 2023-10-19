///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.manufacturer;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public partial class SelectExprInsertEventBeanFactory
    {
        public static SelectExprProcessorForge GetInsertUnderlyingNonJoin(
            EventType eventType,
            bool isUsingWildcard,
            StreamTypeService typeService,
            ExprForge[] forges,
            string[] columnNames,
            object[] expressionReturnTypes,
            InsertIntoDesc insertIntoDesc,
            string[] columnNamesAsProvided,
            bool allowNestableTargetFragmentTypes,
            string statementName,
            ImportServiceCompileTime importService,
            EventTypeAvroHandler eventTypeAvroHandler)
        {
            // handle single-column coercion to underlying, i.e. "insert into MapDefinedEvent select doSomethingReturnMap() from MyEvent"
            if (expressionReturnTypes.Length == 1 &&
                expressionReturnTypes[0] is Type &&
                insertIntoDesc.ColumnNames.IsEmpty() &&
                columnNamesAsProvided[0] == null) {
                var resultType = (Type)expressionReturnTypes[0];
                var compatible = (eventType is BaseNestableEventType || eventType is AvroSchemaEventType) &&
                                 TypeHelper.IsSubclassOrImplementsInterface(resultType, eventType.UnderlyingType);
                compatible = compatible | (eventType is JsonEventType && resultType == typeof(string));

                if (compatible) {
                    if (eventType is MapEventType) {
                        return new SelectExprInsertNativeExpressionCoerceMap(eventType, forges[0]);
                    }
                    else if (eventType is ObjectArrayEventType) {
                        return new SelectExprInsertNativeExpressionCoerceObjectArray(eventType, forges[0]);
                    }
                    else if (eventType is AvroSchemaEventType) {
                        return new SelectExprInsertNativeExpressionCoerceAvro(eventType, forges[0]);
                    }
                    else if (eventType is JsonEventType) {
                        return new SelectExprInsertNativeExpressionCoerceJson(eventType, forges[0]);
                    }
                    else {
                        throw new IllegalStateException("Unrecognied event type " + eventType);
                    }
                }
            }

            // handle special case where the target type has no properties and there is a single "null" value selected
            if (eventType.PropertyDescriptors.Count == 0 &&
                columnNames.Length == 1 &&
                columnNames[0].Equals("null") &&
                (expressionReturnTypes[0] == null || expressionReturnTypes[0] == null) &&
                !isUsingWildcard) {
                EventBeanManufacturerForge eventManufacturer;
                try {
                    eventManufacturer = EventTypeUtility.GetManufacturer(
                        eventType,
                        Array.Empty<WriteablePropertyDescriptor>(),
                        importService,
                        true,
                        eventTypeAvroHandler);
                }
                catch (EventBeanManufactureException e) {
                    throw new ExprValidationException(e.Message, e);
                }

                return new SelectExprInsertNativeNoEval(eventType, eventManufacturer);
            }

            // handle writing to defined columns
            var writableProps =
                EventTypeUtility.GetWriteableProperties(eventType, false, false);
            var isEligible = CheckEligible(eventType, writableProps, allowNestableTargetFragmentTypes);
            if (!isEligible) {
                return null;
            }

            try {
                return InitializeSetterManufactor(
                    eventType,
                    writableProps,
                    isUsingWildcard,
                    typeService,
                    forges,
                    columnNames,
                    expressionReturnTypes,
                    statementName,
                    importService,
                    eventTypeAvroHandler);
            }
            catch (ExprValidationException ex) {
                if (!(eventType is BeanEventType type)) {
                    throw;
                }

                // Try constructor injection
                try {
                    return InitializeCtorInjection(type, forges, expressionReturnTypes, importService);
                }
                catch (ExprValidationException ctorEx) {
                    if (writableProps.IsEmpty()) {
                        throw;
                    }

                    throw ex;
                }
            }
        }

        public static SelectExprProcessorForge GetInsertUnderlyingJoinWildcard(
            EventType eventType,
            string[] streamNames,
            EventType[] streamTypes,
            ImportServiceCompileTime importService,
            string statementName,
            EventTypeAvroHandler eventTypeAvroHandler)
        {
            var writableProps =
                EventTypeUtility.GetWriteableProperties(eventType, false, false);
            var isEligible = CheckEligible(eventType, writableProps, false);
            if (!isEligible) {
                return null;
            }

            try {
                return InitializeJoinWildcardInternal(
                    eventType,
                    writableProps,
                    streamNames,
                    streamTypes,
                    statementName,
                    importService,
                    eventTypeAvroHandler);
            }
            catch (ExprValidationException ex) {
                if (!(eventType is BeanEventType type)) {
                    throw;
                }

                // Try constructor injection
                try {
                    var forges = new ExprForge[streamTypes.Length];
                    var resultTypes = new object[streamTypes.Length];
                    for (var i = 0; i < streamTypes.Length; i++) {
                        forges[i] = new ExprForgeJoinWildcard(i, streamTypes[i].UnderlyingType);
                        resultTypes[i] = forges[i].EvaluationType;
                    }

                    return InitializeCtorInjection(type, forges, resultTypes, importService);
                }
                catch (ExprValidationException ctorEx) {
                    if (writableProps.IsEmpty()) {
                        throw;
                    }

                    throw ex;
                }
            }
        }

        private static bool CheckEligible(
            EventType eventType,
            ISet<WriteablePropertyDescriptor> writableProps,
            bool allowNestableTargetFragmentTypes)
        {
            if (writableProps == null) {
                return false; // no writable properties, not a writable type, proceed
            }

            // For map event types this class does not handle fragment inserts; all fragments are required however and must be explicit
            if (!allowNestableTargetFragmentTypes &&
                (eventType is BaseNestableEventType || eventType is AvroSchemaEventType)) {
                foreach (var prop in eventType.PropertyDescriptors) {
                    if (prop.IsFragment) {
                        return false;
                    }
                }
            }

            return true;
        }

        private static SelectExprProcessorForge InitializeSetterManufactor(
            EventType eventType,
            ISet<WriteablePropertyDescriptor> writables,
            bool isUsingWildcard,
            StreamTypeService typeService,
            ExprForge[] expressionForges,
            string[] columnNames,
            object[] expressionReturnTypes,
            string statementName,
            ImportServiceCompileTime importService,
            EventTypeAvroHandler eventTypeAvroHandler)
        {
            var typeWidenerCustomizer = eventTypeAvroHandler.GetTypeWidenerCustomizer(eventType);
            IList<WriteablePropertyDescriptor> writablePropertiesList = new List<WriteablePropertyDescriptor>();
            IList<ExprForge> forgesList = new List<ExprForge>();
            IList<TypeWidenerSPI> widenersList = new List<TypeWidenerSPI>();

            // loop over all columns selected, if any
            for (var i = 0; i < columnNames.Length; i++) {
                WriteablePropertyDescriptor selectedWritable = null;
                TypeWidenerSPI widener = null;
                var forge = expressionForges[i];

                foreach (var desc in writables) {
                    if (!desc.PropertyName.Equals(columnNames[i])) {
                        continue;
                    }

                    var columnType = expressionReturnTypes[i];
                    if (columnType == null) {
                        try {
                            TypeWidenerFactory.GetCheckPropertyAssignType(
                                columnNames[i],
                                null,
                                desc.PropertyType,
                                desc.PropertyName,
                                false,
                                typeWidenerCustomizer,
                                statementName);
                        }
                        catch (TypeWidenerException ex) {
                            throw new ExprValidationException(ex.Message, ex);
                        }
                    } else if (columnType is EventType columnEventTypeX) {
                        var returnType = columnEventTypeX.UnderlyingType;
                        try {
                            widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                                columnNames[i],
                                columnEventTypeX.UnderlyingType,
                                desc.PropertyType,
                                desc.PropertyName,
                                false,
                                typeWidenerCustomizer,
                                statementName);
                        }
                        catch (TypeWidenerException ex) {
                            throw new ExprValidationException(ex.Message, ex);
                        }

                        // handle evaluator returning an event
                        if (desc.PropertyType is Type &&
                            TypeHelper.IsSubclassOrImplementsInterface(returnType, desc.PropertyType)) {
                            selectedWritable = desc;
                            widener = new ProxyTypeWidenerSPI() {
                                ProcWiden = (input) => {
                                    if (input is EventBean bean) {
                                        return bean.Underlying;
                                    }

                                    return input;
                                },

                                ProcWidenCodegen = (
                                    expression,
                                    codegenMethodScope,
                                    codegenClassScope) => {
                                    var method = codegenMethodScope
                                        .MakeChild(typeof(object), typeof(TypeWidenerSPI), codegenClassScope)
                                        .AddParam<object>("input")
                                        .Block
                                        .IfCondition(InstanceOf(Ref("input"), typeof(EventBean)))
                                        .BlockReturn(
                                            ExprDotMethod(Cast(typeof(EventBean), Ref("input")), "getUnderlying"))
                                        .MethodReturn(Ref("input"));
                                    return LocalMethodBuild(method).Pass(expression).Call();
                                }
                            };
                            continue;
                        }

                        // find stream
                        var streamNum = 0;
                        for (var j = 0; j < typeService.EventTypes.Length; j++) {
                            if (typeService.EventTypes[j] == columnEventTypeX) {
                                streamNum = j;
                                break;
                            }
                        }

                        forge = new ExprForgeStreamUnderlying(
                            streamNum,
                            typeService.EventTypes[streamNum].UnderlyingType);
                    }
                    else if (columnType is EventType[] types) {
                        // handle case where the select-clause contains an fragment array
                        var columnEventType = types[0];
                        var componentReturnType = columnEventType.UnderlyingType;
                        var arrayReturnType = TypeHelper.GetArrayType(componentReturnType);

                        var allowObjectArrayToCollectionConversion = eventType is AvroSchemaEventType;
                        try {
                            widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                                columnNames[i],
                                arrayReturnType,
                                desc.PropertyType,
                                desc.PropertyName,
                                allowObjectArrayToCollectionConversion,
                                typeWidenerCustomizer,
                                statementName);
                        }
                        catch (TypeWidenerException ex) {
                            throw new ExprValidationException(ex.Message, ex);
                        }

                        var inner = forge;
                        forge = new ExprForgeStreamWithInner(inner, componentReturnType);
                    }
                    else if (columnType is Type) {
                        try {
                            widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                                columnNames[i],
                                (Type)columnType,
                                desc.PropertyType,
                                desc.PropertyName,
                                false,
                                typeWidenerCustomizer,
                                statementName);
                        }
                        catch (TypeWidenerException ex) {
                            throw new ExprValidationException(ex.Message, ex);
                        }
                    }
                    else {
                        var message = "Invalid assignment of column '" +
                                      columnNames[i] +
                                      "' of type '" +
                                      columnType +
                                      "' to event property '" +
                                      desc.PropertyName +
                                      "' typed as '" +
                                      desc.PropertyType.CleanName() +
                                      "', column and parameter types mismatch";
                        throw new ExprValidationException(message);
                    }

                    selectedWritable = desc;
                    break;
                }

                if (selectedWritable == null) {
                    var message = "Column '" +
                                  columnNames[i] +
                                  "' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?)";
                    throw new ExprValidationException(message);
                }

                // add
                writablePropertiesList.Add(selectedWritable);
                forgesList.Add(forge);
                widenersList.Add(widener);
            }

            // handle wildcard
            if (isUsingWildcard) {
                var sourceType = typeService.EventTypes[0];
                foreach (var eventPropDescriptor in sourceType.PropertyDescriptors) {
                    if (eventPropDescriptor.IsRequiresIndex || eventPropDescriptor.IsRequiresMapkey) {
                        continue;
                    }

                    WriteablePropertyDescriptor selectedWritable = null;
                    TypeWidenerSPI widener = null;
                    ExprForge forge = null;

                    foreach (var writableDesc in writables) {
                        if (!writableDesc.PropertyName.Equals(eventPropDescriptor.PropertyName)) {
                            continue;
                        }

                        try {
                            widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                                eventPropDescriptor.PropertyName,
                                eventPropDescriptor.PropertyType,
                                writableDesc.PropertyType,
                                writableDesc.PropertyName,
                                false,
                                typeWidenerCustomizer,
                                statementName);
                        }
                        catch (TypeWidenerException ex) {
                            throw new ExprValidationException(ex.Message, ex);
                        }

                        selectedWritable = writableDesc;

                        var propertyName = eventPropDescriptor.PropertyName;
                        var getter = ((EventTypeSPI)sourceType).GetGetterSPI(propertyName);
                        forge = new ExprForgeStreamWithGetter(getter);
                        break;
                    }

                    if (selectedWritable == null) {
                        var message = "Event property '" +
                                      eventPropDescriptor.PropertyName +
                                      "' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?)";
                        throw new ExprValidationException(message);
                    }

                    writablePropertiesList.Add(selectedWritable);
                    forgesList.Add(forge);
                    widenersList.Add(widener);
                }
            }

            // assign
            var writableProperties = writablePropertiesList.ToArray();
            var exprForges = forgesList.ToArray();
            var wideners = widenersList.ToArray();

            EventBeanManufacturerForge eventManufacturer;
            try {
                eventManufacturer = EventTypeUtility.GetManufacturer(
                    eventType,
                    writableProperties,
                    importService,
                    false,
                    eventTypeAvroHandler);
            }
            catch (EventBeanManufactureException e) {
                throw new ExprValidationException(e.Message, e);
            }

            if (eventManufacturer == null) {
                return null;
            }

            return SelectExprInsertNativeBase.MakeInsertNative(eventType, eventManufacturer, exprForges, wideners);
        }

        private static SelectExprProcessorForge InitializeCtorInjection(
            BeanEventType beanEventType,
            ExprForge[] forges,
            object[] expressionReturnTypes,
            ImportServiceCompileTime importService)
        {
            var pair = InstanceManufacturerUtil.GetManufacturer(
                beanEventType.UnderlyingType,
                importService,
                forges,
                expressionReturnTypes);
            var eventManufacturer =
                new EventBeanManufacturerCtorForge(pair.First, beanEventType);
            return new SelectExprInsertNativeNoWiden(beanEventType, eventManufacturer, pair.Second);
        }

        private static SelectExprProcessorForge InitializeJoinWildcardInternal(
            EventType eventType,
            ISet<WriteablePropertyDescriptor> writables,
            string[] streamNames,
            EventType[] streamTypes,
            string statementName,
            ImportServiceCompileTime importService,
            EventTypeAvroHandler eventTypeAvroHandler)
        {
            var typeWidenerCustomizer = eventTypeAvroHandler.GetTypeWidenerCustomizer(eventType);
            IList<WriteablePropertyDescriptor> writablePropertiesList = new List<WriteablePropertyDescriptor>();
            IList<ExprForge> forgesList = new List<ExprForge>();
            IList<TypeWidenerSPI> widenersList = new List<TypeWidenerSPI>();

            // loop over all columns selected, if any
            for (var i = 0; i < streamNames.Length; i++) {
                WriteablePropertyDescriptor selectedWritable = null;
                TypeWidenerSPI widener = null;

                foreach (var desc in writables) {
                    if (!desc.PropertyName.Equals(streamNames[i])) {
                        continue;
                    }

                    try {
                        widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                            streamNames[i],
                            streamTypes[i].UnderlyingType,
                            desc.PropertyType,
                            desc.PropertyName,
                            false,
                            typeWidenerCustomizer,
                            statementName);
                    }
                    catch (TypeWidenerException ex) {
                        throw new ExprValidationException(ex.Message, ex);
                    }

                    selectedWritable = desc;
                    break;
                }

                if (selectedWritable == null) {
                    var message = "Stream underlying object for stream '" +
                                  streamNames[i] +
                                  "' could not be assigned to any of the properties of the underlying type (missing column names, event property or setter method?)";
                    throw new ExprValidationException(message);
                }

                ExprForge forge = new ExprForgeStreamUnderlying(i, streamTypes[i].UnderlyingType);

                // add
                writablePropertiesList.Add(selectedWritable);
                forgesList.Add(forge);
                widenersList.Add(widener);
            }

            // assign
            var writableProperties = writablePropertiesList.ToArray();
            var exprForges = forgesList.ToArray();
            var wideners = widenersList.ToArray();

            EventBeanManufacturerForge eventManufacturer;
            try {
                eventManufacturer = EventTypeUtility.GetManufacturer(
                    eventType,
                    writableProperties,
                    importService,
                    false,
                    eventTypeAvroHandler);
            }
            catch (EventBeanManufactureException e) {
                throw new ExprValidationException(e.Message, e);
            }

            return SelectExprInsertNativeBase.MakeInsertNative(eventType, eventManufacturer, exprForges, wideners);
        }
    }
} // end of namespace