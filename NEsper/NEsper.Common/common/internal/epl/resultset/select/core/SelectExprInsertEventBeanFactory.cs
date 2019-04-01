///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using com.espertech.esper.collection;
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
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using XLR8.CGLib;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public class SelectExprInsertEventBeanFactory
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
                (eventType is BaseNestableEventType || eventType is AvroSchemaEventType) &&
                TypeHelper.IsSubclassOrImplementsInterface((Type) expressionReturnTypes[0], eventType.UnderlyingType) &&
                insertIntoDesc.ColumnNames.IsEmpty() &&
                columnNamesAsProvided[0] == null) {
                if (eventType is MapEventType) {
                    return new SelectExprInsertNativeExpressionCoerceMap(eventType, forges[0]);
                }

                if (eventType is ObjectArrayEventType) {
                    return new SelectExprInsertNativeExpressionCoerceObjectArray(eventType, forges[0]);
                }

                if (eventType is AvroSchemaEventType) {
                    return new SelectExprInsertNativeExpressionCoerceAvro(eventType, forges[0]);
                }

                throw new IllegalStateException("Unrecognied event type " + eventType);
            }

            // handle special case where the target type has no properties and there is a single "null" value selected
            if (eventType.PropertyDescriptors.Length == 0 &&
                columnNames.Length == 1 &&
                columnNames[0].Equals("null") &&
                expressionReturnTypes[0] == null &&
                !isUsingWildcard) {
                EventBeanManufacturerForge eventManufacturer;
                try {
                    eventManufacturer = EventTypeUtility.GetManufacturer(
                        eventType, new WriteablePropertyDescriptor[0], importService, true,
                        eventTypeAvroHandler);
                }
                catch (EventBeanManufactureException e) {
                    throw new ExprValidationException(e.Message, e);
                }

                return new SelectExprInsertNativeNoEval(eventType, eventManufacturer);
            }

            // handle writing to defined columns
            var writableProps = EventTypeUtility.GetWriteableProperties(eventType, false);
            var isEligible = CheckEligible(eventType, writableProps, allowNestableTargetFragmentTypes);
            if (!isEligible) {
                return null;
            }

            try {
                return InitializeSetterManufactor(
                    eventType, writableProps, isUsingWildcard, typeService, forges, columnNames, expressionReturnTypes,
                    statementName, importService, eventTypeAvroHandler);
            }
            catch (ExprValidationException ex) {
                if (!(eventType is BeanEventType)) {
                    throw;
                }

                // Try constructor injection
                try {
                    return InitializeCtorInjection(
                        (BeanEventType) eventType, forges, expressionReturnTypes, importService);
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
            EventType eventType, string[] streamNames, EventType[] streamTypes,
            ImportServiceCompileTime importService, string statementName,
            EventTypeAvroHandler eventTypeAvroHandler)
        {
            var writableProps = EventTypeUtility.GetWriteableProperties(eventType, false);
            var isEligible = CheckEligible(eventType, writableProps, false);
            if (!isEligible) {
                return null;
            }

            try {
                return InitializeJoinWildcardInternal(
                    eventType, writableProps, streamNames, streamTypes, statementName, importService,
                    eventTypeAvroHandler);
            }
            catch (ExprValidationException ex) {
                if (!(eventType is BeanEventType)) {
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

                    return InitializeCtorInjection(
                        (BeanEventType) eventType, forges, resultTypes, importService);
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
            EventType eventType, ISet<WriteablePropertyDescriptor> writableProps, bool allowNestableTargetFragmentTypes)
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
            EventType eventType, ISet<WriteablePropertyDescriptor> writables, bool isUsingWildcard,
            StreamTypeService typeService, ExprForge[] expressionForges, string[] columnNames,
            object[] expressionReturnTypes, string statementName,
            ImportServiceCompileTime importService, EventTypeAvroHandler eventTypeAvroHandler)
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
                                columnNames[i], null, desc.Type, desc.PropertyName, false, typeWidenerCustomizer,
                                statementName);
                        }
                        catch (TypeWidenerException ex) {
                            throw new ExprValidationException(ex.Message, ex);
                        }
                    }
                    else if (columnType is EventType) {
                        var columnEventType = (EventType) columnType;
                        var returnType = columnEventType.UnderlyingType;
                        try {
                            widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                                columnNames[i], columnEventType.UnderlyingType, desc.Type, desc.PropertyName, false,
                                typeWidenerCustomizer, statementName);
                        }
                        catch (TypeWidenerException ex) {
                            throw new ExprValidationException(ex.Message, ex);
                        }

                        // handle evaluator returning an event
                        if (TypeHelper.IsSubclassOrImplementsInterface(returnType, desc.Type)) {
                            selectedWritable = desc;
                            widener = new ProxyTypeWidenerSPI {
                                ProcWiden = input => {
                                    if (input is EventBean) {
                                        return ((EventBean) input).Underlying;
                                    }

                                    return input;
                                },

                                ProcWidenCodegen = (expression, codegenMethodScope, codegenClassScope) => {
                                    CodegenMethod method = codegenMethodScope
                                        .MakeChild(typeof(object), typeof(TypeWidenerSPI), codegenClassScope)
                                        .AddParam(typeof(object), "input").Block
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
                            if (typeService.EventTypes[j] == columnEventType) {
                                streamNum = j;
                                break;
                            }
                        }

                        forge = new ExprForgeStreamUnderlying(
                            streamNum, typeService.EventTypes[streamNum].UnderlyingType);
                    }
                    else if (columnType is EventType[]) {
                        // handle case where the select-clause contains an fragment array
                        var columnEventType = ((EventType[]) columnType)[0];
                        var componentReturnType = columnEventType.UnderlyingType;
                        Type arrayReturnType = Array.CreateInstance(componentReturnType, 0).GetType();

                        var allowObjectArrayToCollectionConversion = eventType is AvroSchemaEventType;
                        try {
                            widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                                columnNames[i], arrayReturnType, desc.Type, desc.PropertyName,
                                allowObjectArrayToCollectionConversion, typeWidenerCustomizer, statementName);
                        }
                        catch (TypeWidenerException ex) {
                            throw new ExprValidationException(ex.Message, ex);
                        }

                        var inner = forge;
                        forge = new ExprForgeStreamWithInner(inner, componentReturnType);
                    }
                    else if (!(columnType is Type)) {
                        var message = "Invalid assignment of column '" + columnNames[i] +
                                      "' of type '" + columnType +
                                      "' to event property '" + desc.PropertyName +
                                      "' typed as '" + desc.Type.Name +
                                      "', column and parameter types mismatch";
                        throw new ExprValidationException(message);
                    }
                    else {
                        try {
                            widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                                columnNames[i], (Type) columnType, desc.Type, desc.PropertyName, false,
                                typeWidenerCustomizer, statementName);
                        }
                        catch (TypeWidenerException ex) {
                            throw new ExprValidationException(ex.Message, ex);
                        }
                    }

                    selectedWritable = desc;
                    break;
                }

                if (selectedWritable == null) {
                    var message = "Column '" + columnNames[i] +
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
                EventType sourceType = typeService.EventTypes[0];
                foreach (var eventPropDescriptor in sourceType.PropertyDescriptors) {
                    if (eventPropDescriptor.IsRequiresIndex || eventPropDescriptor.IsRequiresMapKey) {
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
                                eventPropDescriptor.PropertyName, eventPropDescriptor.PropertyType, writableDesc.Type,
                                writableDesc.PropertyName, false, typeWidenerCustomizer, statementName);
                        }
                        catch (TypeWidenerException ex) {
                            throw new ExprValidationException(ex.Message, ex);
                        }

                        selectedWritable = writableDesc;

                        var propertyName = eventPropDescriptor.PropertyName;
                        var getter = ((EventTypeSPI) sourceType).GetGetterSPI(propertyName);
                        forge = new ExprForgeStreamWithGetter(getter);
                        break;
                    }

                    if (selectedWritable == null) {
                        var message = "Event property '" + eventPropDescriptor.PropertyName +
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
                    eventType, writableProperties, importService, false, eventTypeAvroHandler);
            }
            catch (EventBeanManufactureException e) {
                throw new ExprValidationException(e.Message, e);
            }

            return new SelectExprInsertNativeWidening(eventType, eventManufacturer, exprForges, wideners);
        }

        private static SelectExprProcessorForge InitializeCtorInjection(
            BeanEventType beanEventType, ExprForge[] forges, object[] expressionReturnTypes,
            ImportServiceCompileTime importService)
        {
            Pair<FastConstructor, ExprForge[]> pair = InstanceManufacturerUtil.GetManufacturer(
                beanEventType.UnderlyingType, importService, forges, expressionReturnTypes);
            var eventManufacturer = new EventBeanManufacturerCtorForge(pair.First, beanEventType);
            return new SelectExprInsertNativeNoWiden(beanEventType, eventManufacturer, pair.Second);
        }

        private static SelectExprProcessorForge InitializeJoinWildcardInternal(
            EventType eventType,
            ISet<WriteablePropertyDescriptor> writables, string[] streamNames,
            EventType[] streamTypes, string statementName,
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
                            streamNames[i], streamTypes[i].UnderlyingType, desc.Type, desc.PropertyName, false,
                            typeWidenerCustomizer, statementName);
                    }
                    catch (TypeWidenerException ex) {
                        throw new ExprValidationException(ex.Message, ex);
                    }

                    selectedWritable = desc;
                    break;
                }

                if (selectedWritable == null) {
                    var message = "Stream underlying object for stream '" + streamNames[i] +
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
                    eventType, writableProperties, importService, false, eventTypeAvroHandler);
            }
            catch (EventBeanManufactureException e) {
                throw new ExprValidationException(e.Message, e);
            }

            return new SelectExprInsertNativeWidening(eventType, eventManufacturer, exprForges, wideners);
        }

        public abstract class SelectExprInsertNativeExpressionCoerceBase : SelectExprProcessorForge
        {
            internal readonly EventType eventType;
            internal readonly ExprForge exprForge;
            internal ExprEvaluator evaluator;

            protected SelectExprInsertNativeExpressionCoerceBase(EventType eventType, ExprForge exprForge)
            {
                this.eventType = eventType;
                this.exprForge = exprForge;
            }

            public EventType ResultEventType => eventType;

            public abstract CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType, 
                CodegenExpression eventBeanFactory, 
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol, 
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope);
        }

        public class SelectExprInsertNativeExpressionCoerceMap : SelectExprInsertNativeExpressionCoerceBase
        {
            protected SelectExprInsertNativeExpressionCoerceMap(EventType eventType, ExprForge exprForge)
                : base(eventType, exprForge)
            {
            }

            public override CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType,
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                var expr = exprForge.EvaluateCodegen(typeof(IDictionary<string,object>), methodNode, exprSymbol, codegenClassScope);
                if (!TypeHelper.IsSubclassOrImplementsInterface(exprForge.EvaluationType, typeof(IDictionary<string, object>))) {
                    expr = Cast(typeof(IDictionary<string, object>), expr);
                }

                methodNode.Block.DeclareVar(typeof(IDictionary<string, object>), "result", expr)
                    .IfRefNullReturnNull("result")
                    .MethodReturn(
                        ExprDotMethod(eventBeanFactory, "adapterForTypedMap", Ref("result"), resultEventType));
                return methodNode;
            }
        }

        public class SelectExprInsertNativeExpressionCoerceAvro : SelectExprInsertNativeExpressionCoerceBase
        {
            protected SelectExprInsertNativeExpressionCoerceAvro(EventType eventType, ExprForge exprForge)
                : base(eventType, exprForge)
            {
            }

            public override CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType,
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                methodNode.Block
                    .DeclareVar(
                        typeof(object), "result",
                        exprForge.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("result")
                    .MethodReturn(
                        ExprDotMethod(eventBeanFactory, "adapterForTypedAvro", Ref("result"), resultEventType));
                return methodNode;
            }
        }

        public class SelectExprInsertNativeExpressionCoerceObjectArray : SelectExprInsertNativeExpressionCoerceBase
        {
            protected SelectExprInsertNativeExpressionCoerceObjectArray(EventType eventType, ExprForge exprForge)
                : base(eventType, exprForge)
            {
            }

            public override CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType,
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                methodNode.Block
                    .DeclareVar(
                        typeof(object[]), "result",
                        exprForge.EvaluateCodegen(typeof(object[]), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("result")
                    .MethodReturn(
                        ExprDotMethod(eventBeanFactory, "adapterForTypedObjectArray", Ref("result"), resultEventType));
                return methodNode;
            }
        }

        public class SelectExprInsertNativeExpressionCoerceNative : SelectExprInsertNativeExpressionCoerceBase
        {
            protected SelectExprInsertNativeExpressionCoerceNative(EventType eventType, ExprForge exprForge)
                : base(eventType, exprForge)
            {
            }

            public override CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType,
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                methodNode.Block
                    .DeclareVar(
                        typeof(object), "result",
                        exprForge.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("result")
                    .MethodReturn(
                        ExprDotMethod(eventBeanFactory, "adapterForTypedBean", Ref("result"), resultEventType));
                return methodNode;
            }
        }

        public abstract class SelectExprInsertNativeBase : SelectExprProcessorForge
        {
            internal readonly EventBeanManufacturerForge eventManufacturer;
            internal readonly ExprForge[] exprForges;

            protected SelectExprInsertNativeBase(
                EventType eventType, EventBeanManufacturerForge eventManufacturer, ExprForge[] exprForges)
            {
                ResultEventType = eventType;
                this.eventManufacturer = eventManufacturer;
                this.exprForges = exprForges;
            }

            public EventType ResultEventType { get; }

            public abstract CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType, 
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol, 
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope);
        }

        public class SelectExprInsertNativeWidening : SelectExprInsertNativeBase
        {
            private readonly TypeWidenerSPI[] wideners;

            public SelectExprInsertNativeWidening(
                EventType eventType, EventBeanManufacturerForge eventManufacturer, ExprForge[] exprForges,
                TypeWidenerSPI[] wideners)
                : base(eventType, eventManufacturer, exprForges)
            {
                this.wideners = wideners;
            }

            public override CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType,
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                var manufacturer = codegenClassScope.AddFieldUnshared(
                    true, typeof(EventBeanManufacturer), eventManufacturer.Make(codegenMethodScope, codegenClassScope));
                var block = methodNode.Block
                    .DeclareVar(
                        typeof(object[]), "values", NewArrayByLength(typeof(object), Constant(exprForges.Length)));
                for (var i = 0; i < exprForges.Length; i++) {
                    var expression = CodegenLegoMayVoid.ExpressionMayVoid(
                        exprForges[i].EvaluationType, exprForges[i], methodNode, exprSymbol, codegenClassScope);
                    if (wideners[i] == null) {
                        block.AssignArrayElement("values", Constant(i), expression);
                    }
                    else {
                        var refname = "evalResult" + i;
                        block.DeclareVar(exprForges[i].EvaluationType, refname, expression);
                        if (!exprForges[i].EvaluationType.IsPrimitive) {
                            block.IfRefNotNull(refname)
                                .AssignArrayElement(
                                    "values", Constant(i),
                                    wideners[i].WidenCodegen(Ref(refname), methodNode, codegenClassScope))
                                .BlockEnd();
                        }
                        else {
                            block.AssignArrayElement(
                                "values", Constant(i),
                                wideners[i].WidenCodegen(Ref(refname), methodNode, codegenClassScope));
                        }
                    }
                }

                block.MethodReturn(ExprDotMethod(manufacturer, "make", Ref("values")));
                return methodNode;
            }
        }

        public class SelectExprInsertNativeNoWiden : SelectExprInsertNativeBase
        {
            public SelectExprInsertNativeNoWiden(
                EventType eventType, EventBeanManufacturerForge eventManufacturer, ExprForge[] exprForges)
                : base(eventType, eventManufacturer, exprForges)
            {
            }

            public override CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType, 
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope, 
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol, 
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                var manufacturer = codegenClassScope.AddFieldUnshared(
                    true, typeof(EventBeanManufacturer), eventManufacturer.Make(codegenMethodScope, codegenClassScope));
                var block = methodNode.Block
                    .DeclareVar(
                        typeof(object[]), "values", NewArrayByLength(typeof(object), Constant(exprForges.Length)));
                for (var i = 0; i < exprForges.Length; i++) {
                    var expression = CodegenLegoMayVoid.ExpressionMayVoid(
                        typeof(object), exprForges[i], methodNode, exprSymbol, codegenClassScope);
                    block.AssignArrayElement("values", Constant(i), expression);
                }

                block.MethodReturn(ExprDotMethod(manufacturer, "make", Ref("values")));
                return methodNode;
            }
        }

        public class SelectExprInsertNativeNoEval : SelectExprProcessorForge
        {
            private readonly EventBeanManufacturerForge eventManufacturer;

            public SelectExprInsertNativeNoEval(EventType eventType, EventBeanManufacturerForge eventManufacturer)
            {
                ResultEventType = eventType;
                this.eventManufacturer = eventManufacturer;
            }

            public EventType ResultEventType { get; }

            public CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType,
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                var manufacturer = codegenClassScope.AddFieldUnshared(
                    true, typeof(EventBeanManufacturer), eventManufacturer.Make(codegenMethodScope, codegenClassScope));
                methodNode.Block.MethodReturn(
                    ExprDotMethod(manufacturer, "make", PublicConstValue(typeof(CollectionUtil), "OBJECTARRAY_EMPTY")));
                return methodNode;
            }
        }

        public class ExprForgeJoinWildcard : ExprForge,
            ExprEvaluator,
            ExprNodeRenderable
        {
            private readonly int streamNum;

            public ExprForgeJoinWildcard(int streamNum, Type returnType)
            {
                this.streamNum = streamNum;
                EvaluationType = returnType;
            }

            public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                var bean = eventsPerStream[streamNum];
                if (bean == null) {
                    return null;
                }

                return bean.Underlying;
            }

            public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

            public ExprEvaluator ExprEvaluator => this;

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                    EvaluationType, typeof(ExprForgeJoinWildcard), codegenClassScope);
                var refEPS = exprSymbol.GetAddEPS(methodNode);
                methodNode.Block
                    .DeclareVar(typeof(EventBean), "bean", ArrayAtIndex(refEPS, Constant(streamNum)))
                    .IfRefNullReturnNull("bean")
                    .MethodReturn(Cast(EvaluationType, ExprDotUnderlying(Ref("bean"))));
                return LocalMethod(methodNode);
            }

            public Type EvaluationType { get; }

            public ExprNodeRenderable ForgeRenderable => this;

            public void ToEPL(StringWriter writer, ExprPrecedenceEnum parentPrecedence)
            {
                writer.Write(GetType().GetSimpleName());
            }
        }

        public class ExprForgeStreamUnderlying : ExprForge,
            ExprEvaluator,
            ExprNodeRenderable
        {
            private readonly Type returnType;

            private readonly int streamNumEval;

            public ExprForgeStreamUnderlying(int streamNumEval, Type returnType)
            {
                this.streamNumEval = streamNumEval;
                this.returnType = returnType;
            }

            public object Evaluate(
                EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            {
                var theEvent = eventsPerStream[streamNumEval];
                if (theEvent != null) {
                    return theEvent.Underlying;
                }

                return null;
            }

            public ExprEvaluator ExprEvaluator => this;

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(returnType, GetType(), codegenClassScope);

                var refEPS = exprSymbol.GetAddEPS(methodNode);
                methodNode.Block
                    .DeclareVar(typeof(EventBean), "theEvent", ArrayAtIndex(refEPS, Constant(streamNumEval)))
                    .IfRefNullReturnNull("theEvent")
                    .MethodReturn(Cast(returnType, ExprDotUnderlying(Ref("theEvent"))));
                return LocalMethod(methodNode);
            }

            public Type EvaluationType => typeof(object);

            public ExprNodeRenderable ForgeRenderable => this;

            public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

            public void ToEPL(StringWriter writer, ExprPrecedenceEnum parentPrecedence)
            {
                writer.Write(typeof(ExprForgeStreamUnderlying).GetSimpleName());
            }
        }

        public class ExprForgeStreamWithInner : ExprForge,
            ExprEvaluator,
            ExprNodeRenderable
        {
            private readonly Type componentReturnType;

            private readonly ExprForge inner;

            public ExprForgeStreamWithInner(ExprForge inner, Type componentReturnType)
            {
                this.inner = inner;
                this.componentReturnType = componentReturnType;
            }

            public object Evaluate(
                EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            {
                throw ExprNodeUtilityMake.MakeUnsupportedCompileTime();
            }

            public ExprEvaluator ExprEvaluator => this;

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var arrayType = TypeHelper.GetArrayType(componentReturnType);
                var methodNode = codegenMethodScope.MakeChild(arrayType, GetType(), codegenClassScope);

                methodNode.Block
                    .DeclareVar(
                        typeof(EventBean[]), "events",
                        Cast(
                            typeof(EventBean[]),
                            inner.EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope)))
                    .IfRefNullReturnNull("events")
                    .DeclareVar(arrayType, "values", NewArrayByLength(componentReturnType, ArrayLength(Ref("events"))))
                    .ForLoopIntSimple("i", ArrayLength(Ref("events")))
                    .AssignArrayElement(
                        "values", Ref("i"),
                        Cast(componentReturnType, ExprDotUnderlying(ArrayAtIndex(Ref("events"), Ref("i")))))
                    .BlockEnd()
                    .MethodReturn(Ref("values"));
                return LocalMethod(methodNode);
            }

            public Type EvaluationType => TypeHelper.GetArrayType(componentReturnType);

            public ExprNodeRenderable ForgeRenderable => this;

            public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

            public void ToEPL(StringWriter writer, ExprPrecedenceEnum parentPrecedence)
            {
                writer.Write(typeof(ExprForgeStreamWithInner).GetSimpleName());
            }
        }

        public class ExprForgeStreamWithGetter : ExprForge,
            ExprEvaluator,
            ExprNodeRenderable
        {
            private readonly EventPropertyGetterSPI getter;

            public ExprForgeStreamWithGetter(EventPropertyGetterSPI getter)
            {
                this.getter = getter;
            }

            public object Evaluate(
                EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
            {
                var theEvent = eventsPerStream[0];
                if (theEvent != null) {
                    return getter.Get(theEvent);
                }

                return null;
            }

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                    typeof(object), typeof(ExprForgeStreamWithGetter), codegenClassScope);
                var refEPS = exprSymbol.GetAddEPS(methodNode);
                methodNode.Block
                    .DeclareVar(typeof(EventBean), "theEvent", ArrayAtIndex(refEPS, Constant(0)))
                    .IfRefNotNull("theEvent")
                    .BlockReturn(getter.EventBeanGetCodegen(Ref("theEvent"), methodNode, codegenClassScope))
                    .MethodReturn(ConstantNull());
                return LocalMethod(methodNode);
            }

            public ExprEvaluator ExprEvaluator => this;

            public Type EvaluationType => typeof(object);

            public ExprNodeRenderable ForgeRenderable => this;

            public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

            public void ToEPL(StringWriter writer, ExprPrecedenceEnum parentPrecedence)
            {
                writer.Write(typeof(ExprForgeStreamWithGetter).GetSimpleName());
            }
        }
    }
} // end of namespace