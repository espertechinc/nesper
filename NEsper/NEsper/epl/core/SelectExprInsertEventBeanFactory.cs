///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events.avro;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.util;

using XLR8.CGLib;

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
            string[] columnNames,
            Object[] expressionReturnTypes,
            EngineImportService engineImportService,
            InsertIntoDesc insertIntoDesc,
            string[] columnNamesAsProvided,
            bool allowNestableTargetFragmentTypes,
            string statementName)
        {
            // handle single-column coercion to underlying, i.e. "insert into MapDefinedEvent select DoSomethingReturnMap() from MyEvent"
            if (expressionReturnTypes.Length == 1 &&
                expressionReturnTypes[0] is Type &&
                (eventType is BaseNestableEventType || eventType is AvroSchemaEventType) &&
                TypeHelper.IsSubclassOrImplementsInterface((Type) expressionReturnTypes[0], eventType.UnderlyingType) &&
                insertIntoDesc.ColumnNames.IsEmpty() &&
                columnNamesAsProvided[0] == null)
            {

                if (eventType is MapEventType)
                {
                    return new SelectExprInsertNativeExpressionCoerceMap(
                        eventType, expressionNodes[0], eventAdapterService);
                }
                else if (eventType is ObjectArrayEventType)
                {
                    return new SelectExprInsertNativeExpressionCoerceObjectArray(
                        eventType, expressionNodes[0], eventAdapterService);
                }
                else if (eventType is AvroSchemaEventType)
                {
                    return new SelectExprInsertNativeExpressionCoerceAvro(
                        eventType, expressionNodes[0], eventAdapterService);
                }
                else
                {
                    throw new IllegalStateException("Unrecognied event type " + eventType);
                }
            }

            // handle special case where the target type has no properties and there is a single "null" value selected
            if (eventType.PropertyDescriptors.Count == 0 &&
                columnNames.Length == 1 &&
                columnNames[0].Equals("null") &&
                expressionReturnTypes[0] == null &&
                !isUsingWildcard)
            {

                EventBeanManufacturer eventManufacturer;
                try
                {
                    eventManufacturer = eventAdapterService.GetManufacturer(
                        eventType, new WriteablePropertyDescriptor[0], engineImportService, true);
                }
                catch (EventBeanManufactureException e)
                {
                    throw new ExprValidationException(e.Message, e);
                }
                return new SelectExprInsertNativeNoEval(eventType, eventManufacturer);
            }

            // handle writing to defined columns
            var writableProps = eventAdapterService.GetWriteableProperties(eventType, false);
            var isEligible = CheckEligible(eventType, writableProps, allowNestableTargetFragmentTypes);
            if (!isEligible)
            {
                return null;
            }

            try
            {
                return InitializeSetterManufactor(
                    eventType, writableProps, isUsingWildcard, typeService, expressionNodes, columnNames,
                    expressionReturnTypes, engineImportService, eventAdapterService, statementName);
            }
            catch (ExprValidationException)
            {
                if (!(eventType is BeanEventType))
                {
                    throw;
                }
                // Try constructor injection
                try
                {
                    return InitializeCtorInjection(
                        (BeanEventType) eventType, expressionNodes, expressionReturnTypes, engineImportService,
                        eventAdapterService);
                }
                catch (ExprValidationException)
                {
                    if (writableProps.IsEmpty())
                    {
                        throw;
                    }
                }

                throw;
            }
        }

        public static SelectExprProcessor GetInsertUnderlyingJoinWildcard(
            EventAdapterService eventAdapterService,
            EventType eventType,
            string[] streamNames,
            EventType[] streamTypes,
            EngineImportService engineImportService,
            string statementName,
            string engineURI)
        {
            var writableProps = eventAdapterService.GetWriteableProperties(eventType, false);
            var isEligible = CheckEligible(eventType, writableProps, false);
            if (!isEligible)
            {
                return null;
            }

            try
            {
                return InitializeJoinWildcardInternal(
                    eventType, writableProps, streamNames, streamTypes, engineImportService, eventAdapterService,
                    statementName, engineURI);
            }
            catch (ExprValidationException)
            {
                if (!(eventType is BeanEventType))
                {
                    throw;
                }
                // Try constructor injection
                try
                {
                    var evaluators = new ExprEvaluator[streamTypes.Length];
                    var resultTypes = new Object[streamTypes.Length];
                    for (var i = 0; i < streamTypes.Length; i++)
                    {
                        evaluators[i] = new ExprEvaluatorJoinWildcard(i, streamTypes[i].UnderlyingType);
                        resultTypes[i] = evaluators[i].ReturnType;
                    }

                    return InitializeCtorInjection(
                        (BeanEventType) eventType, evaluators, resultTypes, engineImportService, eventAdapterService);
                }
                catch (ExprValidationException)
                {
                    if (writableProps.IsEmpty())
                    {
                        throw;
                    }
                }

                throw;
            }
        }

        private static bool CheckEligible(
            EventType eventType,
            ICollection<WriteablePropertyDescriptor> writableProps,
            bool allowNestableTargetFragmentTypes)
        {
            if (writableProps == null)
            {
                return false; // no writable properties, not a writable type, proceed
            }

            // For map event types this class does not handle fragment inserts; all fragments are required however and must be explicit
            if (!allowNestableTargetFragmentTypes &&
                (eventType is BaseNestableEventType || eventType is AvroSchemaEventType))
            {
                foreach (var prop in eventType.PropertyDescriptors)
                {
                    if (prop.IsFragment)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static SelectExprProcessor InitializeSetterManufactor(
            EventType eventType,
            ICollection<WriteablePropertyDescriptor> writables,
            bool isUsingWildcard,
            StreamTypeService typeService,
            ExprEvaluator[] expressionNodes,
            string[] columnNames,
            Object[] expressionReturnTypes,
            EngineImportService engineImportService,
            EventAdapterService eventAdapterService,
            string statementName)
        {
            var typeWidenerCustomizer = eventAdapterService.GetTypeWidenerCustomizer(eventType);
            var writablePropertiesList = new List<WriteablePropertyDescriptor>();
            var evaluatorsList = new List<ExprEvaluator>();
            var widenersList = new List<TypeWidener>();

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
                        TypeWidenerFactory.GetCheckPropertyAssignType(
                            columnNames[i], null, desc.PropertyType, desc.PropertyName, false, typeWidenerCustomizer,
                            statementName, typeService.EngineURIQualifier);
                    }
                    else if (columnType is EventType)
                    {
                        var columnEventType = (EventType) columnType;
                        var returnType = columnEventType.UnderlyingType;
                        widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                            columnNames[i], columnEventType.UnderlyingType, desc.PropertyType, desc.PropertyName, false,
                            typeWidenerCustomizer, statementName, typeService.EngineURIQualifier);

                        // handle evaluator returning an event
                        if (TypeHelper.IsSubclassOrImplementsInterface(returnType, desc.PropertyType))
                        {
                            selectedWritable = desc;
                            widener = input =>
                            {
                                var eventBean = input as EventBean;
                                if (eventBean != null)
                                {
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
                                EventBean theEvent = evaluateParams.EventsPerStream[streamNumEval];
                                if (theEvent != null)
                                {
                                    return theEvent.Underlying;
                                }
                                return null;
                            },

                            ProcReturnType = () => returnType
                        };
                    }
                    else if (columnType is EventType[])
                    {
                        // handle case where the select-clause contains an fragment array
                        var columnEventType = ((EventType[]) columnType)[0];
                        var componentReturnType = columnEventType.UnderlyingType;
                        var arrayReturnType = Array.CreateInstance(componentReturnType, 0).GetType();

                        var allowObjectArrayToCollectionConversion = eventType is AvroSchemaEventType;
                        widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                            columnNames[i], arrayReturnType, desc.PropertyType, desc.PropertyName,
                            allowObjectArrayToCollectionConversion, typeWidenerCustomizer, statementName,
                            typeService.EngineURIQualifier);
                        var inner = evaluator;
                        evaluator = new ProxyExprEvaluator
                        {
                            ProcEvaluate = evaluateParams =>
                            {
                                var result = inner.Evaluate(evaluateParams);
                                if (!(result is EventBean[]))
                                {
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

                            ProcReturnType = () => componentReturnType
                        };
                    }
                    else if (!(columnType is Type))
                    {
                        var message = "Invalid assignment of column '" + columnNames[i] +
                                         "' of type '" + columnType +
                                         "' to event property '" + desc.PropertyName +
                                         "' typed as '" + desc.PropertyType.GetCleanName() +
                                         "', column and parameter types mismatch";
                        throw new ExprValidationException(message);
                    }
                    else
                    {
                        widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                            columnNames[i], (Type) columnType, desc.PropertyType, desc.PropertyName, false,
                            typeWidenerCustomizer, statementName, typeService.EngineURIQualifier);
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
                            eventPropDescriptor.PropertyName, eventPropDescriptor.PropertyType, writableDesc.PropertyType,
                            writableDesc.PropertyName, false, typeWidenerCustomizer, statementName,
                            typeService.EngineURIQualifier);
                        selectedWritable = writableDesc;

                        var propertyName = eventPropDescriptor.PropertyName;
                        var propertyType = eventPropDescriptor.PropertyType;
                        evaluator = new ProxyExprEvaluator
                        {
                            ProcEvaluate = evaluateParams =>
                            {
                                EventBean theEvent = evaluateParams.EventsPerStream[0];
                                if (theEvent != null)
                                {
                                    return theEvent.Get(propertyName);
                                }
                                return null;
                            },

                            ProcReturnType = () => propertyType
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
                eventManufacturer = eventAdapterService.GetManufacturer(
                    eventType, writableProperties, engineImportService, false);
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
            Pair<FastConstructor, ExprEvaluator[]> pair =
                InstanceManufacturerUtil.GetManufacturer(
                    beanEventType.UnderlyingType, engineImportService, exprEvaluators, expressionReturnTypes);
            var eventManufacturer = new EventBeanManufacturerCtor(pair.First, beanEventType, eventAdapterService);
            return new SelectExprInsertNativeNoWiden(beanEventType, eventManufacturer, pair.Second);
        }

        private static SelectExprProcessor InitializeJoinWildcardInternal(
            EventType eventType,
            ICollection<WriteablePropertyDescriptor> writables,
            string[] streamNames,
            EventType[] streamTypes,
            EngineImportService engineImportService,
            EventAdapterService eventAdapterService,
            string statementName,
            string engineURI)
        {
            var typeWidenerCustomizer = eventAdapterService.GetTypeWidenerCustomizer(eventType);
            var writablePropertiesList = new List<WriteablePropertyDescriptor>();
            var evaluatorsList = new List<ExprEvaluator>();
            var widenersList = new List<TypeWidener>();

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

                    widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                        streamNames[i], streamTypes[i].UnderlyingType, desc.PropertyType, desc.PropertyName, false,
                        typeWidenerCustomizer, statementName, engineURI);
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
                var evaluator = new ProxyExprEvaluator
                {
                    ProcEvaluate = evaluateParams =>
                    {
                        EventBean theEvent = evaluateParams.EventsPerStream[streamNum];
                        if (theEvent != null)
                        {
                            return theEvent.Underlying;
                        }
                        return null;
                    },

                    ProcReturnType = () => returnType
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
                eventManufacturer = eventAdapterService.GetManufacturer(
                    eventType, writableProperties, engineImportService, false);
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

            internal SelectExprInsertNativeExpressionCoerceBase(
                EventType eventType,
                ExprEvaluator exprEvaluator,
                EventAdapterService eventAdapterService)
            {
                EventType = eventType;
                ExprEvaluator = exprEvaluator;
                EventAdapterService = eventAdapterService;
            }

            public EventType ResultEventType
            {
                get { return EventType; }
            }

            public abstract EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext);
        }

        public class SelectExprInsertNativeExpressionCoerceMap : SelectExprInsertNativeExpressionCoerceBase
        {
            internal SelectExprInsertNativeExpressionCoerceMap(
                EventType eventType,
                ExprEvaluator exprEvaluator,
                EventAdapterService eventAdapterService)
                : base(eventType, exprEvaluator, eventAdapterService)
            {
            }

            public override EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var result = ExprEvaluator.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                if (result == null)
                {
                    return null;
                }
                return EventAdapterService.AdapterForTypedMap((IDictionary<string, object>) result, EventType);
            }
        }

        public class SelectExprInsertNativeExpressionCoerceAvro : SelectExprInsertNativeExpressionCoerceBase
        {
            internal SelectExprInsertNativeExpressionCoerceAvro(
                EventType eventType,
                ExprEvaluator exprEvaluator,
                EventAdapterService eventAdapterService)
                : base(eventType, exprEvaluator, eventAdapterService)
            {
            }

            public override EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var result = ExprEvaluator.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                if (result == null)
                {
                    return null;
                }
                return EventAdapterService.AdapterForTypedAvro(result, EventType);
            }
        }

        public class SelectExprInsertNativeExpressionCoerceObjectArray : SelectExprInsertNativeExpressionCoerceBase
        {
            internal SelectExprInsertNativeExpressionCoerceObjectArray(
                EventType eventType,
                ExprEvaluator exprEvaluator,
                EventAdapterService eventAdapterService)
                : base(eventType, exprEvaluator, eventAdapterService)
            {
            }

            public override EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var result = ExprEvaluator.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                if (result == null)
                {
                    return null;
                }
                return EventAdapterService.AdapterForTypedObjectArray((Object[]) result, EventType);
            }
        }

        public class SelectExprInsertNativeExpressionCoerceNative : SelectExprInsertNativeExpressionCoerceBase
        {
            internal SelectExprInsertNativeExpressionCoerceNative(
                EventType eventType,
                ExprEvaluator exprEvaluator,
                EventAdapterService eventAdapterService)
                : base(eventType, exprEvaluator, eventAdapterService)
            {
            }

            public override EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var result = ExprEvaluator.Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                if (result == null)
                {
                    return null;
                }
                return EventAdapterService.AdapterForTypedObject(result, EventType);
            }
        }

        public abstract class SelectExprInsertNativeBase : SelectExprProcessor
        {
            protected readonly EventBeanManufacturer EventManufacturer;
            protected readonly ExprEvaluator[] ExprEvaluators;
            private readonly EventType _eventType;

            internal SelectExprInsertNativeBase(
                EventType eventType,
                EventBeanManufacturer eventManufacturer,
                ExprEvaluator[] exprEvaluators)
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

        public class SelectExprInsertNativeWidening : SelectExprInsertNativeBase
        {
            private readonly TypeWidener[] _wideners;

            internal SelectExprInsertNativeWidening(
                EventType eventType,
                EventBeanManufacturer eventManufacturer,
                ExprEvaluator[] exprEvaluators,
                TypeWidener[] wideners)
                : base(eventType, eventManufacturer, exprEvaluators)
            {
                _wideners = wideners;
            }

            public override EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext)
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

        public class SelectExprInsertNativeNoWiden : SelectExprInsertNativeBase
        {
            internal SelectExprInsertNativeNoWiden(
                EventType eventType,
                EventBeanManufacturer eventManufacturer,
                ExprEvaluator[] exprEvaluators)
                : base(eventType, eventManufacturer, exprEvaluators)
            {
            }

            public override EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var values = new Object[ExprEvaluators.Length];

                for (var i = 0; i < ExprEvaluators.Length; i++)
                {
                    var evalResult = ExprEvaluators[i].Evaluate(new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext));
                    values[i] = evalResult;
                }

                return EventManufacturer.Make(values);
            }
        }

        public class SelectExprInsertNativeNoEval : SelectExprProcessor
        {
            private static readonly Object[] EMPTY_PROPS = new Object[0];

            private readonly EventType _eventType;
            private readonly EventBeanManufacturer _eventManufacturer;

            internal SelectExprInsertNativeNoEval(EventType eventType, EventBeanManufacturer eventManufacturer)
            {
                _eventType = eventType;
                _eventManufacturer = eventManufacturer;
            }

            public EventBean Process(
                EventBean[] eventsPerStream,
                bool isNewData,
                bool isSynthesize,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                return _eventManufacturer.Make(EMPTY_PROPS);
            }

            public EventType ResultEventType
            {
                get { return _eventType; }
            }
        }

        public class ExprEvaluatorJoinWildcard : ExprEvaluator
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
                return Evaluate(
                    evaluateParams.EventsPerStream,
                    evaluateParams.IsNewData,
                    evaluateParams.ExprEvaluatorContext
                    );
            }

            public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
            {
                var bean = eventsPerStream[_streamNum];
                if (bean == null)
                {
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
} // end of namespace
