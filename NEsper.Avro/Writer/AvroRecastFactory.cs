///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Writer
{
    public class AvroRecastFactory
    {
        public static SelectExprProcessorForge Make(
            EventType[] eventTypes,
            SelectExprForgeContext selectExprForgeContext,
            int streamNumber,
            AvroSchemaEventType targetType,
            ExprNode[] exprNodes,
            string statementName)
        {
            AvroEventType resultType = (AvroEventType) targetType;
            AvroEventType streamType = (AvroEventType) eventTypes[streamNumber];

            // (A) fully assignment-compatible: same number, name and type of fields, no additional expressions: Straight repackage
            if (resultType.Schema.Equals(streamType.Schema) && selectExprForgeContext.ExprForges.Length == 0) {
                return new AvroInsertProcessorSimpleRepackage(selectExprForgeContext, streamNumber, targetType);
            }

            // (B) not completely assignable: find matching properties
            var writables = EventTypeUtility.GetWriteableProperties(resultType, true);
            IList<Item> items = new List<Item>();
            IList<WriteablePropertyDescriptor> written = new List<WriteablePropertyDescriptor>();

            // find the properties coming from the providing source stream
            foreach (var writeable in writables) {
                var propertyName = writeable.PropertyName;

                Field streamTypeField = streamType.SchemaAvro.GetField(propertyName);
                Field resultTypeField = resultType.SchemaAvro.GetField(propertyName);

                if (streamTypeField != null && resultTypeField != null) {
                    if (streamTypeField.Schema.Equals(resultTypeField.Schema)) {
                        items.Add(new Item(resultTypeField, streamTypeField, null, null));
                    }
                    else {
                        throw new ExprValidationException(
                            "Type by name '" +
                            resultType.Name +
                            "' " +
                            "in property '" +
                            propertyName +
                            "' expected schema '" +
                            resultTypeField.Schema +
                            "' but received schema '" +
                            streamTypeField.Schema +
                            "'");
                    }
                }
            }

            // find the properties coming from the expressions of the select clause
            var typeWidenerCustomizer =
                selectExprForgeContext.EventTypeAvroHandler.GetTypeWidenerCustomizer(targetType);
            for (var i = 0; i < selectExprForgeContext.ExprForges.Length; i++) {
                var columnName = selectExprForgeContext.ColumnNames[i];
                var exprNode = exprNodes[i];

                var writable = FindWritable(columnName, writables);
                if (writable == null) {
                    throw new ExprValidationException(
                        "Failed to find column '" + columnName + "' in target type '" + resultType.Name + "'");
                }

                Field resultTypeField = resultType.SchemaAvro.GetField(writable.PropertyName);

                TypeWidenerSPI widener;
                try {
                    widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                        ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(exprNode),
                        exprNode.Forge.EvaluationType,
                        writable.PropertyType,
                        columnName,
                        false,
                        typeWidenerCustomizer,
                        statementName);
                }
                catch (TypeWidenerException ex) {
                    throw new ExprValidationException(ex.Message, ex);
                }

                items.Add(new Item(resultTypeField, null, exprNode.Forge, widener));
                written.Add(writable);
            }

            // make manufacturer
            Item[] itemsArr = items.ToArray();
            return new AvroInsertProcessorAllocate(
                streamNumber,
                itemsArr,
                resultType,
                resultType.SchemaAvro,
                selectExprForgeContext.EventBeanTypedEventFactory);
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

        internal class AvroInsertProcessorSimpleRepackage : SelectExprProcessor,
            SelectExprProcessorForge
        {
            private readonly SelectExprForgeContext _selectExprForgeContext;
            private readonly int _underlyingStreamNumber;

            internal AvroInsertProcessorSimpleRepackage(
                SelectExprForgeContext selectExprForgeContext,
                int underlyingStreamNumber,
                EventType resultType)
            {
                _selectExprForgeContext = selectExprForgeContext;
                _underlyingStreamNumber = underlyingStreamNumber;
                ResultEventType = resultType;
            }

            public EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                AvroGenericDataBackedEventBean theEvent =
                    (AvroGenericDataBackedEventBean) eventsPerStream[_underlyingStreamNumber];
                return _selectExprForgeContext.EventBeanTypedEventFactory.AdapterForTypedAvro(
                    theEvent.Properties,
                    ResultEventType);
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
                var theEvent = CodegenExpressionBuilder.Cast(
                    typeof(AvroGenericDataBackedEventBean),
                    CodegenExpressionBuilder.ArrayAtIndex(
                        refEPS,
                        CodegenExpressionBuilder.Constant(_underlyingStreamNumber)));
                methodNode.Block.MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(
                        eventBeanFactory,
                        "AdapterForTypedAvro",
                        CodegenExpressionBuilder.ExprDotMethod(theEvent, "getProperties"),
                        resultEventType));
                return methodNode;
            }

            public SelectExprProcessor GetSelectExprProcessor(
                ImportService classpathImportService,
                bool isFireAndForget,
                string statementName)
            {
                return this;
            }
        }

        internal class AvroInsertProcessorAllocate : SelectExprProcessor,
            SelectExprProcessorForge
        {
            private readonly EventBeanTypedEventFactory _eventAdapterService;
            private readonly Item[] _items;
            private readonly Schema _resultSchema;
            private readonly int _underlyingStreamNumber;

            internal AvroInsertProcessorAllocate(
                int underlyingStreamNumber,
                Item[] items,
                EventType resultType,
                Schema resultSchema,
                EventBeanTypedEventFactory eventAdapterService)
            {
                _underlyingStreamNumber = underlyingStreamNumber;
                _items = items;
                ResultEventType = resultType;
                _resultSchema = resultSchema;
                _eventAdapterService = eventAdapterService;
            }

            public EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                AvroGenericDataBackedEventBean theEvent =
                    (AvroGenericDataBackedEventBean) eventsPerStream[_underlyingStreamNumber];
                GenericRecord source = theEvent.Properties;
                GenericRecord target = new GenericRecord(_resultSchema.AsRecordSchema());
                foreach (var item in _items) {
                    object value;

                    if (item.OptionalFromIndex != null) {
                        value = source.Get(item.OptionalFromIndex);
                    }
                    else {
                        value = item.EvaluatorAssigned.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                        if (item.OptionalWidener != null) {
                            value = item.OptionalWidener.Widen(value);
                        }
                    }

                    target.Put(item.ToIndex, value);
                }

                return _eventAdapterService.AdapterForTypedAvro(target, ResultEventType);
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
                CodegenExpressionField schema = codegenClassScope.NamespaceScope.AddFieldUnshared(
                    true,
                    typeof(Schema),
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(AvroSchemaUtil),
                        "resolveAvroSchema",
                        EventTypeUtility.ResolveTypeCodegen(ResultEventType, EPStatementInitServicesConstants.REF)));
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                var refEPS = exprSymbol.GetAddEPS(methodNode);
                var block = methodNode.Block
                    .DeclareVar<AvroGenericDataBackedEventBean>(
                        "theEvent",
                        CodegenExpressionBuilder.Cast(
                            typeof(AvroGenericDataBackedEventBean),
                            CodegenExpressionBuilder.ArrayAtIndex(
                                refEPS,
                                CodegenExpressionBuilder.Constant(_underlyingStreamNumber))))
                    .DeclareVar<GenericRecord>(
                        "source",
                        CodegenExpressionBuilder.ExprDotMethod(
                            CodegenExpressionBuilder.Ref("theEvent"),
                            "getProperties"))
                    .DeclareVar<GenericRecord>(
                        "target",
                        CodegenExpressionBuilder.NewInstance(typeof(GenericRecord), schema));
                foreach (var item in _items) {
                    CodegenExpression value;
                    if (item.OptionalFromIndex != null) {
                        value = CodegenExpressionBuilder.ExprDotMethod(
                            CodegenExpressionBuilder.Ref("source"),
                            "get",
                            CodegenExpressionBuilder.Constant(item.OptionalFromIndex));
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

                    block.ExprDotMethod(
                        CodegenExpressionBuilder.Ref("target"),
                        "Put",
                        CodegenExpressionBuilder.Constant(item.ToIndex),
                        value);
                }

                block.MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(
                        eventBeanFactory,
                        "AdapterForTypedAvro",
                        CodegenExpressionBuilder.Ref("target"),
                        resultEventType));
                return methodNode;
            }
        }

        internal class Item
        {
            internal Item(
                Field toIndex,
                Field optionalFromIndex,
                ExprForge forge,
                TypeWidenerSPI optionalWidener)
            {
                ToIndex = toIndex;
                OptionalFromIndex = optionalFromIndex;
                Forge = forge;
                OptionalWidener = optionalWidener;
            }

            public Field ToIndex { get; }

            public Field OptionalFromIndex { get; }

            public ExprForge Forge { get; }

            public TypeWidenerSPI OptionalWidener { get; }

            public ExprEvaluator EvaluatorAssigned { get; set; }
        }
    }
} // end of namespace