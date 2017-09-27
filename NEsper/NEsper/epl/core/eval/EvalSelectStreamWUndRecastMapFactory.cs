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
using com.espertech.esper.events.map;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core.eval
{
    public class EvalSelectStreamWUndRecastMapFactory
    {
        public static SelectExprProcessor Make(
            EventType[] eventTypes,
            SelectExprContext selectExprContext,
            int streamNumber,
            EventType targetType,
            ExprNode[] exprNodes,
            EngineImportService engineImportService,
            string statementName,
            string engineURI)
        {
            var mapResultType = (MapEventType)targetType;
            var mapStreamType = (MapEventType)eventTypes[streamNumber];

            // (A) fully assignment-compatible: same number, name and type of fields, no additional expressions: Straight repackage
            String typeSameMssage = BaseNestableEventType.IsDeepEqualsProperties(mapResultType.Name, mapResultType.Types, mapStreamType.Types);
            if (typeSameMssage == null && selectExprContext.ExpressionNodes.Length == 0)
            {
                return new MapInsertProcessorSimpleRepackage(selectExprContext, streamNumber, targetType);
            }

            // (B) not completely assignable: find matching properties
            ICollection<WriteablePropertyDescriptor> writables = selectExprContext.EventAdapterService.GetWriteableProperties(mapResultType, true);
            IList<Item> items = new List<Item>();
            IList<WriteablePropertyDescriptor> written = new List<WriteablePropertyDescriptor>();

            // find the properties coming from the providing source stream
            int count = 0;
            foreach (WriteablePropertyDescriptor writeable in writables)
            {
                String propertyName = writeable.PropertyName;

                if (mapStreamType.Types.ContainsKey(propertyName))
                {
                    Object setOneType = mapStreamType.Types.Get(propertyName);
                    Object setTwoType = mapResultType.Types.Get(propertyName);
                    bool setTwoTypeFound = mapResultType.Types.ContainsKey(propertyName);
                    String message = BaseNestableEventUtil.ComparePropType(propertyName, setOneType, setTwoType, setTwoTypeFound, mapResultType.Name);
                    if (message != null)
                    {
                        throw new ExprValidationException(message);
                    }
                    items.Add(new Item(count, propertyName, null, null));
                    written.Add(writeable);
                    count++;
                }
            }

            // find the properties coming from the expressions of the select clause
            for (int i = 0; i < selectExprContext.ExpressionNodes.Length; i++)
            {
                String columnName = selectExprContext.ColumnNames[i];
                ExprEvaluator evaluator = selectExprContext.ExpressionNodes[i];
                ExprNode exprNode = exprNodes[i];

                WriteablePropertyDescriptor writable = FindWritable(columnName, writables);
                if (writable == null)
                {
                    throw new ExprValidationException("Failed to find column '" + columnName + "' in target type '" + mapResultType.Name + "'");
                }

                TypeWidener widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                    ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(exprNode),
                    exprNode.ExprEvaluator.ReturnType,
                    writable.PropertyType, columnName,
                    false, null, statementName, engineURI);
                items.Add(new Item(count, null, evaluator, widener));
                written.Add(writable);
                count++;
            }

            // make manufacturer
            Item[] itemsArr = items.ToArray();
            EventBeanManufacturer manufacturer;
            try
            {
                manufacturer = selectExprContext.EventAdapterService.GetManufacturer(mapResultType,
                        written.ToArray(), engineImportService, true);
            }
            catch (EventBeanManufactureException e)
            {
                throw new ExprValidationException("Failed to write to type: " + e.Message, e);
            }

            return new MapInsertProcessorAllocate(streamNumber, itemsArr, manufacturer, targetType);
        }

        private static WriteablePropertyDescriptor FindWritable(String columnName, ICollection<WriteablePropertyDescriptor> writables)
        {
            return writables.FirstOrDefault(writable => writable.PropertyName.Equals(columnName));
        }

        private class MapInsertProcessorSimpleRepackage : SelectExprProcessor
        {
            private readonly SelectExprContext _selectExprContext;
            private readonly int _underlyingStreamNumber;
            private readonly EventType _resultType;

            internal MapInsertProcessorSimpleRepackage(SelectExprContext selectExprContext, int underlyingStreamNumber, EventType resultType)
            {
                _selectExprContext = selectExprContext;
                _underlyingStreamNumber = underlyingStreamNumber;
                _resultType = resultType;
            }

            public EventType ResultEventType
            {
                get { return _resultType; }
            }

            public EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
            {
                var theEvent = (MappedEventBean)eventsPerStream[_underlyingStreamNumber];
                return _selectExprContext.EventAdapterService.AdapterForTypedMap(theEvent.Properties, _resultType);
            }
        }

        private class MapInsertProcessorAllocate : SelectExprProcessor
        {
            private readonly int _underlyingStreamNumber;
            private readonly Item[] _items;
            private readonly EventBeanManufacturer _manufacturer;
            private readonly EventType _resultType;

            internal MapInsertProcessorAllocate(int underlyingStreamNumber, Item[] items, EventBeanManufacturer manufacturer, EventType resultType)
            {
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
                var theEvent = (MappedEventBean)eventsPerStream[_underlyingStreamNumber];
                var props = new Object[_items.Length];
                foreach (Item item in _items)
                {
                    Object value;

                    if (item.OptionalPropertyName != null)
                    {
                        value = theEvent.Properties.Get(item.OptionalPropertyName);
                    }
                    else
                    {
                        value = item.Evaluator.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                        if (item.OptionalWidener != null)
                        {
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
            internal Item(int toIndex, String optionalPropertyName, ExprEvaluator evaluator, TypeWidener optionalWidener)
            {
                ToIndex = toIndex;
                OptionalPropertyName = optionalPropertyName;
                Evaluator = evaluator;
                OptionalWidener = optionalWidener;
            }

            public readonly int ToIndex;
            public readonly string OptionalPropertyName;
            public readonly ExprEvaluator Evaluator;
            public readonly TypeWidener OptionalWidener;
        }
    }
}
