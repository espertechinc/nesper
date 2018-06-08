///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.core.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.events.avro;
using com.espertech.esper.util;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

using Enumerable = System.Linq.Enumerable;

namespace NEsper.Avro.Writer
{
    public class AvroRecastFactory
    {
        public static SelectExprProcessor Make(
            EventType[] eventTypes,
            SelectExprContext selectExprContext,
            int streamNumber,
            AvroSchemaEventType targetType,
            ExprNode[] exprNodes,
            string statementName,
            string engineURI)
        {
            var resultType = (AvroEventType) targetType;
            var streamType = (AvroEventType) eventTypes[streamNumber];
    
            // (A) fully assignment-compatible: same number, name and type of fields, no additional expressions: Straight repackage
            if (resultType.Schema.Equals(streamType.Schema) && selectExprContext.ExpressionNodes.Length == 0) {
                return new AvroInsertProcessorSimpleRepackage(selectExprContext, streamNumber, targetType);
            }
    
            // (B) not completely assignable: find matching properties
            var writables = selectExprContext.EventAdapterService.GetWriteableProperties(resultType, true);
            var items = new List<Item>();
            var written = new List<WriteablePropertyDescriptor>();
    
            // find the properties coming from the providing source stream
            foreach (var writeable in writables) {
                var propertyName = writeable.PropertyName;
    
                Field streamTypeField = streamType.SchemaAvro.GetField(propertyName);
                //int indexSource = streamTypeField == null ? null : streamTypeField.Pos;
                Field resultTypeField = resultType.SchemaAvro.GetField(propertyName);
                //int indexTarget = resultTypeField == null ? null : resultTypeField.Pos;
    
                if (streamTypeField != null && resultTypeField != null) {
                    if (streamTypeField.Schema.Equals(resultTypeField.Schema)) {
                        items.Add(new Item(resultTypeField, streamTypeField, null, null));
                    } else {
                        throw new ExprValidationException("Type by name '" + resultType.Name + "' " +
                                                          "in property '" + propertyName +
                                                          "' expected schema '" + resultTypeField.Schema +
                                                          "' but received schema '" + streamTypeField.Schema +
                                                          "'");
                    }
                }
            }
    
            // find the properties coming from the expressions of the select clause
            var typeWidenerCustomizer = selectExprContext.EventAdapterService.GetTypeWidenerCustomizer(targetType);
            for (var i = 0; i < selectExprContext.ExpressionNodes.Length; i++) {
                var columnName = selectExprContext.ColumnNames[i];
                var evaluator = selectExprContext.ExpressionNodes[i];
                var exprNode = exprNodes[i];
    
                var writable = FindWritable(columnName, writables);
                if (writable == null) {
                    throw new ExprValidationException("Failed to find column '" + columnName + "' in target type '" + resultType.Name + "'");
                }
                Field resultTypeField = resultType.SchemaAvro.GetField(writable.PropertyName);

                var widener =
                    TypeWidenerFactory.GetCheckPropertyAssignType(
                        exprNode.ToExpressionStringMinPrecedenceSafe(),
                        exprNode.ExprEvaluator.ReturnType,
                        writable.PropertyType, columnName, false, typeWidenerCustomizer, statementName, engineURI);
                items.Add(new Item(resultTypeField, null, evaluator, widener));
                written.Add(writable);
            }
    
            // make manufacturer
            var itemsArr = items.ToArray();
            return new AvroInsertProcessorAllocate(streamNumber, itemsArr, resultType, resultType.SchemaAvro, selectExprContext.EventAdapterService);
        }
    
        private static WriteablePropertyDescriptor FindWritable(string columnName, IEnumerable<WriteablePropertyDescriptor> writables)
        {
            return Enumerable.FirstOrDefault(writables, writable => writable.PropertyName.Equals(columnName));
        }

        private class AvroInsertProcessorSimpleRepackage : SelectExprProcessor
        {
            private readonly SelectExprContext _selectExprContext;
            private readonly int _underlyingStreamNumber;
            private readonly EventType _resultEventType;

            internal AvroInsertProcessorSimpleRepackage(SelectExprContext selectExprContext, int underlyingStreamNumber, EventType resultType) {
                _selectExprContext = selectExprContext;
                _underlyingStreamNumber = underlyingStreamNumber;
                _resultEventType = resultType;
            }

            public EventType ResultEventType
            {
                get { return _resultEventType; }
            }

            public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext) {
                var theEvent = (AvroGenericDataBackedEventBean) eventsPerStream[_underlyingStreamNumber];
                return _selectExprContext.EventAdapterService.AdapterForTypedAvro(theEvent.Properties, ResultEventType);
            }
        }
    
        private class AvroInsertProcessorAllocate : SelectExprProcessor
        {
            private readonly int _underlyingStreamNumber;
            private readonly Item[] _items;
            private readonly EventType _resultType;
            private readonly Schema _resultSchema;
            private readonly EventAdapterService _eventAdapterService;
    
            public AvroInsertProcessorAllocate(int underlyingStreamNumber, Item[] items, EventType resultType, Schema resultSchema, EventAdapterService eventAdapterService)
            {
                _underlyingStreamNumber = underlyingStreamNumber;
                _items = items;
                _resultType = resultType;
                _resultSchema = resultSchema;
                _eventAdapterService = eventAdapterService;
            }

            public EventType ResultEventType
            {
                get { return _resultType; }
            }

            public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
            {
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
                var source = ((AvroGenericDataBackedEventBean)eventsPerStream[_underlyingStreamNumber]).Properties;
                var target = new GenericRecord(_resultSchema.AsRecordSchema());

                foreach (var item in _items)
                {
                    Object value;
    
                    if (item.OptionalFromField != null) {
                        value = source.Get(item.OptionalFromField);
                    } else {
                        value = item.Evaluator.Evaluate(evaluateParams);
                        if (item.OptionalWidener != null) {
                            value = item.OptionalWidener.Invoke(value);
                        }
                    }
    
                    target.Put(item.ToField, value);
                }
    
                return _eventAdapterService.AdapterForTypedAvro(target, _resultType);
            }
        }
    
        private class Item
        {
            internal Item(Field toToField, Field optionalFromField, ExprEvaluator evaluator, TypeWidener optionalWidener)
            {
                ToField = toToField;
                OptionalFromField = optionalFromField;
                Evaluator = evaluator;
                OptionalWidener = optionalWidener;
            }

            public Field ToField { get; private set; }

            public Field OptionalFromField { get; private set; }

            public ExprEvaluator Evaluator { get; private set; }

            public TypeWidener OptionalWidener { get; private set; }
        }
    }
} // end of namespace
