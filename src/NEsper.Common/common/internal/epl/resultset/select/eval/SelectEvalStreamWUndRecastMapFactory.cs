///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalStreamWUndRecastMapFactory
    {
        public static SelectExprProcessorForge Make(
            EventType[] eventTypes,
            SelectExprForgeContext selectExprForgeContext,
            int streamNumber,
            EventType targetType,
            ExprNode[] exprNodes,
            ImportServiceCompileTime importService,
            string statementName)
        {
            var mapResultType = (MapEventType) targetType;
            var mapStreamType = (MapEventType) eventTypes[streamNumber];

            // (A) fully assignment-compatible: same number, name and type of fields, no additional expressions: Straight repackage
            var typeSameMssage = BaseNestableEventType.IsDeepEqualsProperties(
                mapResultType.Name,
                mapResultType.Types,
                mapStreamType.Types);
            if (typeSameMssage == null && selectExprForgeContext.ExprForges.Length == 0) {
                return new MapInsertProcessorSimpleRepackage(selectExprForgeContext, streamNumber, targetType);
            }

            // (B) not completely assignable: find matching properties
            var writables = EventTypeUtility.GetWriteableProperties(mapResultType, true, false);
            IList<Item> items = new List<Item>();
            IList<WriteablePropertyDescriptor> written = new List<WriteablePropertyDescriptor>();

            // find the properties coming from the providing source stream
            var count = 0;
            foreach (var writeable in writables) {
                var propertyName = writeable.PropertyName;

                if (mapStreamType.Types.ContainsKey(propertyName)) {
                    var setOneType = mapStreamType.Types.Get(propertyName);
                    var setTwoType = mapResultType.Types.Get(propertyName);
                    var setTwoTypeFound = mapResultType.Types.ContainsKey(propertyName);
                    var message = BaseNestableEventUtil.ComparePropType(
                        propertyName,
                        setOneType,
                        setTwoType,
                        setTwoTypeFound,
                        mapResultType.Name);
                    if (message != null) {
                        throw new ExprValidationException(message.Message, message);
                    }

                    items.Add(new Item(count, propertyName, null, null));
                    written.Add(writeable);
                    count++;
                }
            }

            // find the properties coming from the expressions of the select clause
            for (var i = 0; i < selectExprForgeContext.ExprForges.Length; i++) {
                var columnName = selectExprForgeContext.ColumnNames[i];
                var exprNode = exprNodes[i];

                var writable = FindWritable(columnName, writables);
                if (writable == null) {
                    throw new ExprValidationException(
                        "Failed to find column '" + columnName + "' in target type '" + mapResultType.Name + "'");
                }

                try {
                    var widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprNode),
                        exprNode.Forge.EvaluationType,
                        writable.PropertyType,
                        columnName,
                        false,
                        null,
                        statementName);
                    items.Add(new Item(count, null, exprNode.Forge, widener));
                    written.Add(writable);
                    count++;
                }
                catch (TypeWidenerException ex) {
                    throw new ExprValidationException(ex.Message, ex);
                }
            }

            // make manufacturer
            Item[] itemsArr = items.ToArray();
            EventBeanManufacturerForge manufacturer;
            try {
                manufacturer = EventTypeUtility.GetManufacturer(
                    mapResultType,
                    written.ToArray(),
                    importService,
                    true,
                    null);
            }
            catch (EventBeanManufactureException e) {
                throw new ExprValidationException("Failed to write to type: " + e.Message, e);
            }

            return new MapInsertProcessorAllocate(streamNumber, itemsArr, manufacturer, targetType);
        }

        private static WriteablePropertyDescriptor FindWritable(
            string columnName,
            ISet<WriteablePropertyDescriptor> writables)
        {
            foreach (var writable in writables) {
                if (writable.PropertyName.Equals(columnName)) {
                    return writable;
                }
            }

            return null;
        }

        internal class MapInsertProcessorSimpleRepackage : SelectExprProcessorForge
        {
            private readonly SelectExprForgeContext selectExprForgeContext;
            private readonly int underlyingStreamNumber;

            internal MapInsertProcessorSimpleRepackage(
                SelectExprForgeContext selectExprForgeContext,
                int underlyingStreamNumber,
                EventType resultType)
            {
                this.selectExprForgeContext = selectExprForgeContext;
                this.underlyingStreamNumber = underlyingStreamNumber;
                ResultEventType = resultType;
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
                var refEPS = exprSymbol.GetAddEPS(methodNode);
                var value = ExprDotName(
                    Cast(typeof(MappedEventBean), ArrayAtIndex(refEPS, Constant(underlyingStreamNumber))),
                    "Properties");
                methodNode.Block.MethodReturn(
                    ExprDotMethod(eventBeanFactory, "AdapterForTypedMap", value, resultEventType));
                return methodNode;
            }
        }

        internal class MapInsertProcessorAllocate : SelectExprProcessorForge
        {
            private readonly Item[] items;
            private readonly EventBeanManufacturerForge manufacturer;
            private readonly int underlyingStreamNumber;

            internal MapInsertProcessorAllocate(
                int underlyingStreamNumber,
                Item[] items,
                EventBeanManufacturerForge manufacturer,
                EventType resultType)
            {
                this.underlyingStreamNumber = underlyingStreamNumber;
                this.items = items;
                this.manufacturer = manufacturer;
                ResultEventType = resultType;
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
                var manufacturerField = codegenClassScope.AddDefaultFieldUnshared(
                    true,
                    typeof(EventBeanManufacturer),
                    manufacturer.Make(methodNode.Block, codegenMethodScope, codegenClassScope));
                var refEPS = exprSymbol.GetAddEPS(methodNode);
                var block = methodNode.Block
                    .DeclareVar<MappedEventBean>(
                        "theEvent",
                        Cast(typeof(MappedEventBean), ArrayAtIndex(refEPS, Constant(underlyingStreamNumber))))
                    .DeclareVar<object[]>("props", NewArrayByLength(typeof(object), Constant(items.Length)));
                foreach (Item item in items) {
                    CodegenExpression value;
                    if (item.OptionalPropertyName != null) {
                        value = ExprDotMethodChain(Ref("theEvent"))
                            .Get("Properties")
                            .Add(
                                "Get",
                                Constant(item.OptionalPropertyName));
                    }
                    else {
                        if (item.OptionalWidener != null) {
                            value = item.Forge.EvaluateCodegen(
                                item.Forge.EvaluationType,
                                methodNode,
                                exprSymbol,
                                codegenClassScope);
                            value = item.OptionalWidener.WidenCodegen(value, methodNode, codegenClassScope);
                        }
                        else {
                            value = item.Forge.EvaluateCodegen(
                                typeof(object),
                                methodNode,
                                exprSymbol,
                                codegenClassScope);
                        }
                    }

                    block.AssignArrayElement("props", Constant(item.ToIndex), value);
                }

                block.MethodReturn(ExprDotMethod(manufacturerField, "Make", Ref("props")));
                return methodNode;
            }
        }

        internal class Item
        {
            internal Item(
                int toIndex,
                string optionalPropertyName,
                ExprForge forge,
                TypeWidenerSPI optionalWidener)
            {
                ToIndex = toIndex;
                OptionalPropertyName = optionalPropertyName;
                Forge = forge;
                OptionalWidener = optionalWidener;
            }

            public int ToIndex { get; }

            public string OptionalPropertyName { get; }

            public ExprForge Forge { get; }

            public TypeWidenerSPI OptionalWidener { get; }

            public ExprEvaluator EvaluatorAssigned { get; set; }
        }
    }
} // end of namespace