///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.core
{
    public class SelectExprInsertEventBeanFactory
    {
        public static SelectExprProcessor GetInsertUnderlyingNonJoin(
            EventAdapterService eventAdapterService,
            EventType eventType,
            bool isUsingWildcard,
            StreamTypeService typeService,
            ExprEvaluator[] expressionNodes,
            String[] columnNames,
            Object[] expressionReturnTypes,
            EngineImportService engineImportService,
            InsertIntoDesc insertIntoDesc,
            String[] columnNamesAsProvided,
            bool allowNestableTargetFragmentTypes)
        {
            // handle single-column coercion to underlying, i.e. "insert into MapDefinedEvent select doSomethingReturnMap() from MyEvent"
            if (expressionReturnTypes.Length == 1 &&
                    expressionReturnTypes[0] is Type &&
                    eventType is BaseNestableEventType &&
                    TypeHelper.IsSubclassOrImplementsInterface((Type) expressionReturnTypes[0], eventType.UnderlyingType) &&
                    insertIntoDesc.ColumnNames.IsEmpty() &&
                    columnNamesAsProvided[0] == null) {
    
                if (eventType is MapEventType) {
                    return new SelectExprInsertNativeExpressionCoerceMap(eventType, expressionNodes[0], eventAdapterService);
                }
                return new SelectExprInsertNativeExpressionCoerceObjectArray(eventType, expressionNodes[0], eventAdapterService);
            }
    
            // handle writing to defined columns
            var writableProps = eventAdapterService.GetWriteableProperties(eventType, false);
            var isEligible = CheckEligible(eventType, writableProps, allowNestableTargetFragmentTypes);
            if (!isEligible) {
                return null;
            }
    
            try {
                return InitializeSetterManufactor(eventType, writableProps, isUsingWildcard, typeService, expressionNodes, columnNames, expressionReturnTypes, engineImportService, eventAdapterService);
            }
            catch (ExprValidationException ex) {
                if (!(eventType is BeanEventType)) {
                    throw;
                }
                // Try constructor injection
                try {
                    return InitializeCtorInjection((BeanEventType)eventType, expressionNodes, expressionReturnTypes, engineImportService, eventAdapterService);
                }
                catch (ExprValidationException) {
                    if (writableProps.IsEmpty()) {
                        throw;
                    }
                }

                throw;
            }
        }

        public static SelectExprProcessor GetInsertUnderlyingJoinWildcard(
            EventAdapterService eventAdapterService,
            EventType eventType,
            String[] streamNames,
            EventType[] streamTypes,
            EngineImportService engineImportService)
        {
            var writableProps = eventAdapterService.GetWriteableProperties(eventType, false);
            var isEligible = CheckEligible(eventType, writableProps, false);
            if (!isEligible) {
                return null;
            }
    
            try {
                return InitializeJoinWildcardInternal(eventType, writableProps, streamNames, streamTypes, engineImportService, eventAdapterService);
            }
            catch (ExprValidationException ex) {
                if (!(eventType is BeanEventType)) {
                    throw;
                }
                // Try constructor injection
                try {
                    var evaluators = new ExprEvaluator[streamTypes.Length];
                    var resultTypes = new Object[streamTypes.Length];
                    for (var i = 0; i < streamTypes.Length; i++) {
                        evaluators[i] = new ExprEvaluatorJoinWildcard(i, streamTypes[i].UnderlyingType);
                        resultTypes[i] = evaluators[i].ReturnType;
                    }

                    return InitializeCtorInjection((BeanEventType)eventType, evaluators, resultTypes, engineImportService, eventAdapterService);
                }
                catch (ExprValidationException) {
                    if (writableProps.IsEmpty()) {
                        throw;
                    }
                    throw ex;
                }
            }
        }
    
        private static bool CheckEligible(EventType eventType, ICollection<WriteablePropertyDescriptor> writableProps, bool allowNestableTargetFragmentTypes)
        {
            if (writableProps == null)
            {
                return false;    // no writable properties, not a writable type, proceed
            }
    
            // For map event types this class does not handle fragment inserts; all fragments are required however and must be explicit
            if (!allowNestableTargetFragmentTypes && eventType is BaseNestableEventType)
            {
                return eventType.PropertyDescriptors.All(prop => !prop.IsFragment);
            }
    
            return true;
        }

        private static SelectExprProcessor InitializeSetterManufactor(
            EventType eventType,
            ICollection<WriteablePropertyDescriptor> writables,
            bool isUsingWildcard,
            StreamTypeService typeService,
            ExprEvaluator[] expressionNodes,
            String[] columnNames,
            Object[] expressionReturnTypes,
            EngineImportService engineImportService,
            EventAdapterService eventAdapterService)
        {
            IList<WriteablePropertyDescriptor> writablePropertiesList = new List<WriteablePropertyDescriptor>();
            IList<ExprEvaluator> evaluatorsList = new List<ExprEvaluator>();
            IList<TypeWidener> widenersList = new List<TypeWidener>();
    
            // loop over all columns selected, if any
            for (var i = 0; i < columnNames.Length; i++)
            {
                WriteablePropertyDescriptor selectedWritable = null;
                TypeWidener widener = null;
                var evaluator = expressionNodes[i];
    
                foreach (var desc in writables)
                {
                    if (!desc.PropertyName.Equals(columnNames[i]))
                    {
                        continue;
                    }
    
                    var columnType = expressionReturnTypes[i];
                    if (columnType == null)
                    {
                        TypeWidenerFactory.GetCheckPropertyAssignType(columnNames[i], null, desc.PropertyType, desc.PropertyName);
                    }
                    else if (columnType is EventType)
                    {
                        var columnEventType = (EventType) columnType;
                        var returnType = columnEventType.UnderlyingType;
                        widener = TypeWidenerFactory.GetCheckPropertyAssignType(columnNames[i], columnEventType.UnderlyingType, desc.PropertyType, desc.PropertyName);
    
                        // handle evaluator returning an event
                        if (TypeHelper.IsSubclassOrImplementsInterface(returnType, desc.PropertyType)) {
                            selectedWritable = desc;
                            widener = input =>
                            {
                                var eventBean = input as EventBean;
                                if (eventBean != null) {
                                    return eventBean.Underlying;
                                }
                                return input;
                            };
                            continue;
                        }
    
                        // find stream
                        var streamNum = 0;
                        for (var j = 0; j < typeService.EventTypes.Length; j++)
                        {
                            if (Equals(typeService.EventTypes[j], columnEventType))
                            {
                                streamNum = j;
                                break;
                            }
                        }
                        var streamNumEval = streamNum;
                        evaluator = new ProxyExprEvaluator
                        {
                            ProcEvaluate = evaluateParams => 
                            {
                                var theEvent = evaluateParams.EventsPerStream[streamNumEval];
                                if (theEvent != null)
                                {
                                    return theEvent.Underlying;
                                }
                                return null;
                            },
    
                            ProcReturnType = () => 
                            {
                                return returnType;
                            },
    
                        };
                    }
                    // handle case where the select-clause contains an fragment array
                    else if (columnType is EventType[])
                    {
                        var columnEventType = ((EventType[]) columnType)[0];
                        var componentReturnType = columnEventType.UnderlyingType;
                        var arrayReturnType = Array.CreateInstance(componentReturnType, 0).GetType();
    
                        widener = TypeWidenerFactory.GetCheckPropertyAssignType(columnNames[i], arrayReturnType, desc.PropertyType, desc.PropertyName);
                        var inner = evaluator;
                        evaluator = new ProxyExprEvaluator
                        {
                            ProcEvaluate = evaluateParams =>
                            {
                                var result = inner.Evaluate(evaluateParams);
                                if (!(result is EventBean[])) {
                                    return null;
                                }
                                var events = (EventBean[]) result;
                                var values = Array.CreateInstance(componentReturnType, events.Length);
                                for (var ii = 0; ii < events.Length; ii++)
                                {
                                    values.SetValue(events[ii].Underlying, ii);
                                }
                                return values;
                            },
    
                            ProcReturnType = () => 
                            {
                                return componentReturnType;
                            },
    
                        };
                    }
                    else if (!(columnType is Type))
                    {
                        var message = "Invalid assignment of column '" + columnNames[i] +
                                "' of type '" + columnType +
                                "' to event property '" + desc.PropertyName +
                                "' typed as '" + desc.PropertyType.FullName +
                                "', column and parameter types mismatch";
                        throw new ExprValidationException(message);
                    }
                    else
                    {
                        widener = TypeWidenerFactory.GetCheckPropertyAssignType(columnNames[i], (Type) columnType, desc.PropertyType, desc.PropertyName);
                    }
    
                    selectedWritable = desc;
                    break;
                }
    
                if (selectedWritable == null)
                {
                    var message = "Column '" + columnNames[i] +
                            "' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?)";
                    throw new ExprValidationException(message);
                }
    
                // add
                writablePropertiesList.Add(selectedWritable);
                evaluatorsList.Add(evaluator);
                widenersList.Add(widener);
            }
    
            // handle wildcard
            if (isUsingWildcard)
            {
                var sourceType = typeService.EventTypes[0];
                foreach (var eventPropDescriptor in sourceType.PropertyDescriptors)
                {
                    if (eventPropDescriptor.RequiresIndex || (eventPropDescriptor.RequiresMapKey))
                    {
                        continue;
                    }
    
                    WriteablePropertyDescriptor selectedWritable = null;
                    TypeWidener widener = null;
                    ExprEvaluator evaluator = null;
    
                    foreach (var writableDesc in writables)
                    {
                        if (!writableDesc.PropertyName.Equals(eventPropDescriptor.PropertyName))
                        {
                            continue;
                        }

                        widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                            eventPropDescriptor.PropertyName, 
                            eventPropDescriptor.PropertyType,
                            writableDesc.PropertyType, 
                            writableDesc.PropertyName);
                        selectedWritable = writableDesc;
    
                        var propertyName = eventPropDescriptor.PropertyName;
                        var propertyType = eventPropDescriptor.PropertyType;
                        evaluator = new ProxyExprEvaluator
                        {
                            ProcEvaluate = evaluateParams =>
                            {
                                var theEvent = evaluateParams.EventsPerStream[0];
                                if (theEvent != null)
                                {
                                    return theEvent.Get(propertyName);
                                }
                                return null;
                            },
    
                            ProcReturnType = () => 
                            {
                                return propertyType;
                            },
                        };
                        break;
                    }
    
                    if (selectedWritable == null)
                    {
                        var message = "Event property '" + eventPropDescriptor.PropertyName +
                                "' could not be assigned to any of the properties of the underlying type (missing column names, event property, setter method or constructor?)";
                        throw new ExprValidationException(message);
                    }
    
                    writablePropertiesList.Add(selectedWritable);
                    evaluatorsList.Add(evaluator);
                    widenersList.Add(widener);
                }
            }
    
            // assign
            var writableProperties = writablePropertiesList.ToArray();
            var exprEvaluators = evaluatorsList.ToArray();
            var wideners = widenersList.ToArray();
    
            EventBeanManufacturer eventManufacturer;
            try
            {
                eventManufacturer = eventAdapterService.GetManufacturer(eventType, writableProperties, engineImportService, false);
            }
            catch (EventBeanManufactureException e)
            {
                throw new ExprValidationException(e.Message, e);
            }
    
            return new SelectExprInsertNativeWidening(eventType, eventManufacturer, exprEvaluators, wideners);
        }

        private static SelectExprProcessor InitializeCtorInjection(
            BeanEventType beanEventType,
            ExprEvaluator[] exprEvaluators,
            Object[] expressionReturnTypes,
            EngineImportService engineImportService,
            EventAdapterService eventAdapterService)
        {
            var pair = InstanceManufacturerUtil.GetManufacturer(beanEventType.UnderlyingType, engineImportService, exprEvaluators, expressionReturnTypes);
            var eventManufacturer = new EventBeanManufacturerCtor(pair.First, beanEventType, eventAdapterService);
            return new SelectExprInsertNativeNoWiden(beanEventType, eventManufacturer, pair.Second);
        }

        private static SelectExprProcessor InitializeJoinWildcardInternal(
            EventType eventType,
            ICollection<WriteablePropertyDescriptor> writables,
            String[] streamNames,
            EventType[] streamTypes,
            EngineImportService engineImportService,
            EventAdapterService eventAdapterService)
        {
            IList<WriteablePropertyDescriptor> writablePropertiesList = new List<WriteablePropertyDescriptor>();
            IList<ExprEvaluator> evaluatorsList = new List<ExprEvaluator>();
            IList<TypeWidener> widenersList = new List<TypeWidener>();
    
            // loop over all columns selected, if any
            for (var i = 0; i < streamNames.Length; i++)
            {
                WriteablePropertyDescriptor selectedWritable = null;
                TypeWidener widener = null;
    
                foreach (var desc in writables)
                {
                    if (!desc.PropertyName.Equals(streamNames[i]))
                    {
                        continue;
                    }
    
                    widener = TypeWidenerFactory.GetCheckPropertyAssignType(streamNames[i], streamTypes[i].UnderlyingType, desc.PropertyType, desc.PropertyName);
                    selectedWritable = desc;
                    break;
                }
    
                if (selectedWritable == null)
                {
                    var message = "Stream underlying object for stream '" + streamNames[i] +
                            "' could not be assigned to any of the properties of the underlying type (missing column names, event property or setter method?)";
                    throw new ExprValidationException(message);
                }
    
                var streamNum = i;
                var returnType = streamTypes[streamNum].UnderlyingType;
                ExprEvaluator evaluator = new ProxyExprEvaluator
                {
                    ProcEvaluate = args => 
                    {
                        EventBean theEvent = args.EventsPerStream[streamNum];
                        if (theEvent != null)
                        {
                            return theEvent.Underlying;
                        }
                        return null;
                    },
    
                    ProcReturnType = () => 
                    {
                        return returnType;
                    },
    
                };
    
                // add
                writablePropertiesList.Add(selectedWritable);
                evaluatorsList.Add(evaluator);
                widenersList.Add(widener);
            }
    
            // assign
            var writableProperties = writablePropertiesList.ToArray();
            var exprEvaluators = evaluatorsList.ToArray();
            var wideners = widenersList.ToArray();
    
            EventBeanManufacturer eventManufacturer;
            try
            {
                eventManufacturer = eventAdapterService.GetManufacturer(eventType, writableProperties, engineImportService, false);
            }
            catch (EventBeanManufactureException e)
            {
                throw new ExprValidationException(e.Message, e);
            }
    
            return new SelectExprInsertNativeWidening(eventType, eventManufacturer, exprEvaluators, wideners);
        }
    
        public abstract class SelectExprInsertNativeExpressionCoerceBase : SelectExprProcessor
        {
            protected readonly EventType EventType;
            protected readonly ExprEvaluator ExprEvaluator;
            protected readonly EventAdapterService EventAdapterService;
    
            protected SelectExprInsertNativeExpressionCoerceBase(EventType eventType, ExprEvaluator exprEvaluator, EventAdapterService eventAdapterService)
            {
                EventType = eventType;
                ExprEvaluator = exprEvaluator;
                EventAdapterService = eventAdapterService;
            }

            public EventType ResultEventType
            {
                get { return EventType; }
            }

            abstract public EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext);
        }

        internal class SelectExprInsertNativeExpressionCoerceMap : SelectExprInsertNativeExpressionCoerceBase
        {
            internal SelectExprInsertNativeExpressionCoerceMap(EventType eventType, ExprEvaluator exprEvaluator, EventAdapterService eventAdapterService)
                : base(eventType, exprEvaluator, eventAdapterService)
            {
            }
    
            public override EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
            {
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
                var result = ExprEvaluator.Evaluate(evaluateParams);
                if (result == null) {
                    return null;
                }
                return EventAdapterService.AdapterForTypedMap((IDictionary<string, object>) result, EventType);
            }
        }

        internal class SelectExprInsertNativeExpressionCoerceObjectArray : SelectExprInsertNativeExpressionCoerceBase
        {
            internal SelectExprInsertNativeExpressionCoerceObjectArray(EventType eventType, ExprEvaluator exprEvaluator, EventAdapterService eventAdapterService)
                : base(eventType, exprEvaluator, eventAdapterService)
            {
            }
    
            public override EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
            {
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
                var result = ExprEvaluator.Evaluate(evaluateParams);
                if (result == null)
                {
                    return null;
                }
                return EventAdapterService.AdapterForTypedObjectArray((Object[]) result, EventType);
            }
        }

        internal class SelectExprInsertNativeExpressionCoerceNative : SelectExprInsertNativeExpressionCoerceBase
        {
            internal SelectExprInsertNativeExpressionCoerceNative(EventType eventType, ExprEvaluator exprEvaluator, EventAdapterService eventAdapterService)
                : base(eventType, exprEvaluator, eventAdapterService)
            {
            }
    
            public override EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
            {
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
                var result = ExprEvaluator.Evaluate(evaluateParams);
                if (result == null)
                {
                    return null;
                }
                return EventAdapterService.AdapterForTypedObject(result, EventType);
            }
        }

        internal abstract class SelectExprInsertNativeBase : SelectExprProcessor
        {
            private readonly EventType _eventType;
            protected readonly EventBeanManufacturer EventManufacturer;
            protected readonly ExprEvaluator[] ExprEvaluators;
    
            protected SelectExprInsertNativeBase(EventType eventType, EventBeanManufacturer eventManufacturer, ExprEvaluator[] exprEvaluators)
            {
                _eventType = eventType;
                EventManufacturer = eventManufacturer;
                ExprEvaluators = exprEvaluators;
            }

            public EventType ResultEventType
            {
                get { return _eventType; }
            }

            public abstract EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext);
        }

        internal class SelectExprInsertNativeWidening : SelectExprInsertNativeBase
        {
            private readonly TypeWidener[] _wideners;

            internal SelectExprInsertNativeWidening(EventType eventType, EventBeanManufacturer eventManufacturer, ExprEvaluator[] exprEvaluators, TypeWidener[] wideners)
                : base(eventType, eventManufacturer, exprEvaluators)
            {
                _wideners = wideners;
            }
    
            public override EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
            {
                var values = new Object[ExprEvaluators.Length];
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
    
                for (var i = 0; i < ExprEvaluators.Length; i++)
                {
                    var evalResult = ExprEvaluators[i].Evaluate(evaluateParams);
                    if ((evalResult != null) && (_wideners[i] != null))
                    {
                        evalResult = _wideners[i].Invoke(evalResult);
                    }
                    values[i] = evalResult;
                }
    
                return EventManufacturer.Make(values);
            }
        }

        internal class SelectExprInsertNativeNoWiden : SelectExprInsertNativeBase
        {
            internal SelectExprInsertNativeNoWiden(EventType eventType, EventBeanManufacturer eventManufacturer, ExprEvaluator[] exprEvaluators)
                : base(eventType, eventManufacturer, exprEvaluators)
            {
            }
    
            public override EventBean Process(EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
            {
                var values = new Object[ExprEvaluators.Length];
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
    
                for (var i = 0; i < ExprEvaluators.Length; i++)
                {
                    var evalResult = ExprEvaluators[i].Evaluate(evaluateParams);
                    values[i] = evalResult;
                }
    
                return EventManufacturer.Make(values);
            }
        }

        internal class ExprEvaluatorJoinWildcard : ExprEvaluator
        {
            private readonly int _streamNum;
            private readonly Type _returnType;

            internal ExprEvaluatorJoinWildcard(int streamNum, Type returnType)
            {
                _streamNum = streamNum;
                _returnType = returnType;
            }

            public object Evaluate(EvaluateParams evaluateParams)
            {
                var bean = evaluateParams.EventsPerStream[_streamNum];
                if (bean == null) {
                    return null;
                }
                return bean.Underlying;
            }

            public Type ReturnType
            {
                get { return _returnType; }
            }
        }
    }
}
