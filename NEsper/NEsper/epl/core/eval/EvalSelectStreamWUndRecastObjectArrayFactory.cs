///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalSelectStreamWUndRecastObjectArrayFactory
    {
        public static SelectExprProcessor Make(EventType[] eventTypes, SelectExprContext selectExprContext, int streamNumber, EventType targetType, ExprNode[] exprNodes, EngineImportService engineImportService)
        {
            var oaResultType = (ObjectArrayEventType) targetType;
            var oaStreamType = (ObjectArrayEventType) eventTypes[streamNumber];
    
            // (A) fully assignment-compatible: same number, name and type of fields, no additional expressions: Straight repackage
            if (oaResultType.IsDeepEqualsConsiderOrder(oaStreamType) && selectExprContext.ExpressionNodes.Length == 0) {
                return new OAInsertProcessorSimpleRepackage(selectExprContext, streamNumber, targetType);
            }
    
            // (B) not completely assignable: find matching properties
            ICollection<WriteablePropertyDescriptor> writables = selectExprContext.EventAdapterService.GetWriteableProperties(oaResultType, true);
            IList<Item> items = new List<Item>();
            IList<WriteablePropertyDescriptor> written = new List<WriteablePropertyDescriptor>();
    
            // find the properties coming from the providing source stream
            foreach (WriteablePropertyDescriptor writeable in writables) {
                String propertyName = writeable.PropertyName;

                int indexSource;
                int indexTarget = oaResultType.PropertiesIndexes.Get(propertyName);

                if (oaStreamType.PropertiesIndexes.TryGetValue(propertyName, out indexSource))
                {
                    var setOneType = oaStreamType.Types.Get(propertyName);
                    var setTwoType = oaResultType.Types.Get(propertyName);
                    var setTwoTypeFound = oaResultType.Types.ContainsKey(propertyName);
                    var message = BaseNestableEventUtil.ComparePropType(propertyName, setOneType, setTwoType, setTwoTypeFound, oaResultType.Name);
                    if (message != null) {
                        throw new ExprValidationException(message);
                    }
                    items.Add(new Item(indexTarget, indexSource, null, null));
                    written.Add(writeable);
                }
            }
    
            // find the properties coming from the expressions of the select clause
            int count = written.Count;
            for (int i = 0; i < selectExprContext.ExpressionNodes.Length; i++) {
                String columnName = selectExprContext.ColumnNames[i];
                ExprEvaluator evaluator = selectExprContext.ExpressionNodes[i];
                ExprNode exprNode = exprNodes[i];
    
                WriteablePropertyDescriptor writable = FindWritable(columnName, writables);
                if (writable == null) {
                    throw new ExprValidationException("Failed to find column '" + columnName + "' in target type '" + oaResultType.Name + "'");
                }
    
                TypeWidener widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                    ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(exprNode),
                    exprNode.ExprEvaluator.ReturnType,
                    writable.PropertyType, columnName);
                items.Add(new Item(count, -1, evaluator, widener));
                written.Add(writable);
                count++;
            }
    
            // make manufacturer
            Item[] itemsArr = items.ToArray();
            EventBeanManufacturer manufacturer;
            try {
                manufacturer = selectExprContext.EventAdapterService.GetManufacturer(oaResultType,
                        written.ToArray(), engineImportService, true);
            }
            catch (EventBeanManufactureException e) {
                throw new ExprValidationException("Failed to write to type: " + e.Message, e);
            }
    
            return new OAInsertProcessorAllocate(streamNumber, itemsArr, manufacturer, targetType);
        }
    
        private static WriteablePropertyDescriptor FindWritable(String columnName, ICollection<WriteablePropertyDescriptor> writables)
        {
            return writables.FirstOrDefault(writable => writable.PropertyName.Equals(columnName));
        }

        private class OAInsertProcessorSimpleRepackage : SelectExprProcessor
        {
            private readonly SelectExprContext _selectExprContext;
            private readonly int _underlyingStreamNumber;
            private readonly EventType _resultType;

            internal OAInsertProcessorSimpleRepackage(SelectExprContext selectExprContext, int underlyingStreamNumber, EventType resultType) {
                _selectExprContext = selectExprContext;
                _underlyingStreamNumber = underlyingStreamNumber;
                _resultType = resultType;
            }

            public EventType ResultEventType
            {
                get { return _resultType; }
            }

            public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext) {
                ObjectArrayBackedEventBean theEvent = (ObjectArrayBackedEventBean) eventsPerStream[_underlyingStreamNumber];
                return _selectExprContext.EventAdapterService.AdapterForTypedObjectArray(theEvent.Properties, _resultType);
            }
        }
    
        private class OAInsertProcessorAllocate : SelectExprProcessor
        {
            private readonly int _underlyingStreamNumber;
            private readonly Item[] _items;
            private readonly EventBeanManufacturer _manufacturer;
            private readonly EventType _resultType;

            internal OAInsertProcessorAllocate(int underlyingStreamNumber, Item[] items, EventBeanManufacturer manufacturer, EventType resultType) {
                _underlyingStreamNumber = underlyingStreamNumber;
                _items = items;
                _manufacturer = manufacturer;
                _resultType = resultType;
            }

            public EventType ResultEventType
            {
                get { return _resultType; }
            }

            public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
            {
                ObjectArrayBackedEventBean theEvent = (ObjectArrayBackedEventBean) eventsPerStream[_underlyingStreamNumber];
    
                Object[] props = new Object[_items.Length];
                foreach (Item item in _items) {
                    Object value;
    
                    if (item.OptionalFromIndex != -1) {
                        value = theEvent.Properties[item.OptionalFromIndex];
                    }
                    else {
                        value = item.Evaluator.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                        if (item.OptionalWidener != null) {
                            value = item.OptionalWidener.Invoke(value);
                        }
                    }
    
                    props[item.ToIndex] = value;
                }
    
                return _manufacturer.Make(props);
            }
        }
    
        private class Item
        {
            internal Item(int toIndex, int optionalFromIndex, ExprEvaluator evaluator, TypeWidener optionalWidener)
            {
                ToIndex = toIndex;
                OptionalFromIndex = optionalFromIndex;
                Evaluator = evaluator;
                OptionalWidener = optionalWidener;
            }

            public readonly int ToIndex;

            public readonly int OptionalFromIndex;

            public readonly ExprEvaluator Evaluator;

            public readonly TypeWidener OptionalWidener;
        }
    }
}
