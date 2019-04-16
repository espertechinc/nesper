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
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.eval
{
    public class SelectEvalStreamWUndRecastObjectArrayFactory
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
            var oaResultType = (ObjectArrayEventType) targetType;
            var oaStreamType = (ObjectArrayEventType) eventTypes[streamNumber];

            // (A) fully assignment-compatible: same number, name and type of fields, no additional expressions: Straight repackage
            if (oaResultType.IsDeepEqualsConsiderOrder(oaStreamType) && selectExprForgeContext.ExprForges.Length == 0) {
                return new OAInsertProcessorSimpleRepackage(selectExprForgeContext, streamNumber, targetType);
            }

            // (B) not completely assignable: find matching properties
            var writables = EventTypeUtility.GetWriteableProperties(oaResultType, true);
            IList<Item> items = new List<Item>();
            IList<WriteablePropertyDescriptor> written = new List<WriteablePropertyDescriptor>();

            // find the properties coming from the providing source stream
            foreach (var writeable in writables) {
                var propertyName = writeable.PropertyName;

                var hasIndexSource = oaStreamType.PropertiesIndexes.TryGetValue(propertyName, out var indexSource);
                var hasIndexTarget = oaResultType.PropertiesIndexes.TryGetValue(propertyName, out var indexTarget);
                if (hasIndexSource) {
                    var setOneType = oaStreamType.Types.Get(propertyName);
                    var setTwoType = oaResultType.Types.Get(propertyName);
                    var setTwoTypeFound = oaResultType.Types.ContainsKey(propertyName);
                    var message = BaseNestableEventUtil.ComparePropType(
                        propertyName, setOneType, setTwoType, setTwoTypeFound, oaResultType.Name);
                    if (message != null) {
                        throw new ExprValidationException(message.Message, message);
                    }

                    items.Add(new Item(indexTarget, indexSource, null, null));
                    written.Add(writeable);
                }
            }

            // find the properties coming from the expressions of the select clause
            var count = written.Count;
            for (var i = 0; i < selectExprForgeContext.ExprForges.Length; i++) {
                var columnName = selectExprForgeContext.ColumnNames[i];
                var forge = selectExprForgeContext.ExprForges[i];
                var exprNode = exprNodes[i];

                var writable = FindWritable(columnName, writables);
                if (writable == null) {
                    throw new ExprValidationException(
                        "Failed to find column '" + columnName + "' in target type '" + oaResultType.Name + "'");
                }

                TypeWidenerSPI widener;
                try {
                    widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprNode),
                        exprNode.Forge.EvaluationType,
                        writable.PropertyType, columnName, false, null, statementName);
                }
                catch (TypeWidenerException ex) {
                    throw new ExprValidationException(ex.Message, ex);
                }

                items.Add(new Item(count, -1, forge, widener));
                written.Add(writable);
                count++;
            }

            // make manufacturer
            var itemsArr = items.ToArray();
            EventBeanManufacturerForge manufacturer;
            try {
                manufacturer = EventTypeUtility.GetManufacturer(
                    oaResultType,
                    written.ToArray(), importService, true, selectExprForgeContext.EventTypeAvroHandler);
            }
            catch (EventBeanManufactureException e) {
                throw new ExprValidationException("Failed to write to type: " + e.Message, e);
            }

            return new OAInsertProcessorAllocate(streamNumber, itemsArr, manufacturer, targetType);
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

        internal class OAInsertProcessorSimpleRepackage : SelectExprProcessorForge
        {
            private readonly SelectExprForgeContext selectExprForgeContext;
            private readonly int underlyingStreamNumber;

            internal OAInsertProcessorSimpleRepackage(
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
                var value = ExprDotMethod(
                    Cast(typeof(ObjectArrayBackedEventBean), ArrayAtIndex(refEPS, Constant(underlyingStreamNumber))),
                    "getProperties");
                methodNode.Block.MethodReturn(
                    ExprDotMethod(eventBeanFactory, "adapterForTypedObjectArray", value, resultEventType));
                return methodNode;
            }
        }

        internal class OAInsertProcessorAllocate : SelectExprProcessorForge
        {
            private readonly Item[] items;
            private readonly EventBeanManufacturerForge manufacturer;
            private readonly int underlyingStreamNumber;

            internal OAInsertProcessorAllocate(
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
                var manufacturerField = codegenClassScope.AddFieldUnshared(
                    true, typeof(EventBeanManufacturer), manufacturer.Make(codegenMethodScope, codegenClassScope));
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                var refEPS = exprSymbol.GetAddEPS(methodNode);
                var block = methodNode.Block
                    .DeclareVar(
                        typeof(ObjectArrayBackedEventBean), "theEvent",
                        Cast(
                            typeof(ObjectArrayBackedEventBean), ArrayAtIndex(refEPS, Constant(underlyingStreamNumber))))
                    .DeclareVar(typeof(object[]), "props", NewArrayByLength(typeof(object), Constant(items.Length)));
                foreach (var item in items) {
                    if (item.OptionalFromIndex != -1) {
                        block.AssignArrayElement(
                            "props", Constant(item.ToIndex),
                            ArrayAtIndex(
                                ExprDotMethod(Ref("theEvent"), "getProperties"), Constant(item.OptionalFromIndex)));
                    }
                    else {
                        CodegenExpression value;
                        if (item.OptionalWidener != null) {
                            value = item.Forge.EvaluateCodegen(
                                item.Forge.EvaluationType, methodNode, exprSymbol, codegenClassScope);
                            value = item.OptionalWidener.WidenCodegen(value, methodNode, codegenClassScope);
                        }
                        else {
                            value = item.Forge.EvaluateCodegen(
                                typeof(object), methodNode, exprSymbol, codegenClassScope);
                        }

                        block.AssignArrayElement("props", Constant(item.ToIndex), value);
                    }
                }

                block.MethodReturn(ExprDotMethod(manufacturerField, "make", Ref("props")));
                return methodNode;
            }
        }

        internal class Item
        {
            internal Item(
                int toIndex,
                int optionalFromIndex,
                ExprForge forge,
                TypeWidenerSPI optionalWidener)
            {
                ToIndex = toIndex;
                OptionalFromIndex = optionalFromIndex;
                Forge = forge;
                OptionalWidener = optionalWidener;
            }

            public int ToIndex { get; }

            public int OptionalFromIndex { get; }

            public ExprForge Forge { get; }

            public TypeWidenerSPI OptionalWidener { get; }

            public ExprEvaluator EvaluatorAssigned { get; set; }
        }
    }
} // end of namespace