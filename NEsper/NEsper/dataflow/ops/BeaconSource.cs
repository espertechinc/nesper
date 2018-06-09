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
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.dataflow.ops
{
    [DataFlowOperator]
    public class BeaconSource : DataFlowSourceOperator
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly List<string> PARAMETER_PROPERTIES = new List<string>
        {
            "interval",
            "iterations",
            "initialDelay"
        };

#pragma warning disable CS0649
        [DataFlowContext] private EPDataFlowEmitter graphContext;
        [DataFlowOpParameter] private long iterations;
        [DataFlowOpParameter] private double initialDelay;
        [DataFlowOpParameter] private double interval;
#pragma warning restore CS0649

        private readonly IDictionary<string, Object> _allProperties = new LinkedHashMap<string, Object>();
    
        private long _initialDelayMSec;
        private long _periodDelayMSec;
        private long _lastSendTime;
        private long _iterationNumber;
        private bool _produceEventBean;
    
        private ExprEvaluator[] _evaluators;
        private EventBeanManufacturer _manufacturer;
    
        private static WriteablePropertyDescriptor[] SetupProperties(string[] propertyNamesOffered, EventType outputEventType, StatementContext statementContext)
        {
            var writeables = statementContext.EventAdapterService.GetWriteableProperties(outputEventType, false);
            var writablesList = new List<WriteablePropertyDescriptor>();
    
            for (var i = 0; i < propertyNamesOffered.Length; i++) {
                var propertyName = propertyNamesOffered[i];
                WriteablePropertyDescriptor writable = EventTypeUtility.FindWritable(propertyName, writeables);
                if (writable == null) {
                    throw new ExprValidationException("Failed to find writable property '" + propertyName + "' for event type '" + outputEventType.Name + "'");
                }
                writablesList.Add(writable);
            }
    
            return writablesList.ToArray();
        }
    
        [DataFlowOpParameter(All=true)]
        public void SetProperty(string name, Object value)
        {
            _allProperties.Put(name, value);
        }

        public DataFlowOpInitializeResult Initialize(DataFlowOpInitializateContext context)
        {
            _initialDelayMSec = (long) (initialDelay*1000);
            _periodDelayMSec = (long) (interval*1000);

            if (context.OutputPorts.Count != 1)
            {
                throw new ArgumentException(
                    "BeaconSource operator requires one output stream but produces " + context.OutputPorts.Count +
                    " streams");
            }

            // Check if a type is declared
            var port = context.OutputPorts[0];
            ICollection<string> props;
            if (port.OptionalDeclaredType != null && port.OptionalDeclaredType.EventType != null)
            {
                var outputEventType = port.OptionalDeclaredType.EventType;
                _produceEventBean = port.OptionalDeclaredType != null && !port.OptionalDeclaredType.IsUnderlying;

                // compile properties to populate
                props = _allProperties.Keys;
                props.RemoveAll(PARAMETER_PROPERTIES);
                var writables = SetupProperties(props.ToArray(), outputEventType, context.StatementContext);
                _manufacturer = context.ServicesContext.EventAdapterService.GetManufacturer(
                    outputEventType, writables, context.ServicesContext.EngineImportService, false);

                var index = 0;
                _evaluators = new ExprEvaluator[writables.Length];
                TypeWidenerCustomizer typeWidenerCustomizer =
                    context.ServicesContext.EventAdapterService.GetTypeWidenerCustomizer(outputEventType);
                foreach (var writeable in writables)
                {

                    var providedProperty = _allProperties.Get(writeable.PropertyName);
                    if (providedProperty is ExprNode)
                    {
                        var exprNode = (ExprNode) providedProperty;
                        var validated = ExprNodeUtility.ValidateSimpleGetSubtree(
                            ExprNodeOrigin.DATAFLOWBEACON, exprNode, context.StatementContext, null, false);
                        var exprEvaluator = validated.ExprEvaluator;
                        var widener = TypeWidenerFactory.GetCheckPropertyAssignType(
                            ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(validated), exprEvaluator.ReturnType,
                            writeable.PropertyType, writeable.PropertyName, false, typeWidenerCustomizer,
                            context.StatementContext.StatementName, context.Engine.URI);
                        if (widener != null)
                        {
                            _evaluators[index] = new ProxyExprEvaluator
                            {
                                ProcEvaluate = evaluateParams =>
                                {
                                    var value = exprEvaluator.Evaluate(evaluateParams);
                                    return widener.Invoke(value);
                                },
                                ProcReturnType = () => null
                            };
                        }
                        else
                        {
                            _evaluators[index] = exprEvaluator;
                        }
                    }
                    else if (providedProperty == null)
                    {
                        _evaluators[index] = new ProxyExprEvaluator
                        {
                            ProcEvaluate = evaluateParams => null,
                            ProcReturnType = () => null
                        };
                    }
                    else
                    {
                        _evaluators[index] = new ProxyExprEvaluator
                        {
                            ProcEvaluate = evaluateParams => providedProperty,
                            ProcReturnType = () => providedProperty.GetType()
                        };
                    }
                    index++;
                }

                return null; // no changing types
            }

            // No type has been declared, we can create one
            var anonymousTypeName = context.DataflowName + "-beacon";
            var types = new LinkedHashMap<string, Object>();
            props = _allProperties.Keys;
            props.RemoveAll(PARAMETER_PROPERTIES);

            var count = 0;
            _evaluators = new ExprEvaluator[props.Count];
            foreach (var propertyName in props)
            {
                var exprNode = (ExprNode) _allProperties.Get(propertyName);
                var validated = ExprNodeUtility.ValidateSimpleGetSubtree(ExprNodeOrigin.DATAFLOWBEACON, exprNode, context.StatementContext, null, false);
                var evaluateParamsX = new EvaluateParams(null, true, context.AgentInstanceContext);
                var value = validated.ExprEvaluator.Evaluate(evaluateParamsX);
                if (value == null)
                {
                    types.Put(propertyName, null);
                }
                else
                {
                    types.Put(propertyName, value.GetType());
                }
                _evaluators[count] = new ProxyExprEvaluator()
                {
                    ProcEvaluate = (evaluateParams) => value,
                    ProcReturnType = () => null
                };
                count++;
            }

            EventType type =
                context.ServicesContext.EventAdapterService.CreateAnonymousObjectArrayType(anonymousTypeName, types);
            return new DataFlowOpInitializeResult(
                new GraphTypeDesc[]
                {
                    new GraphTypeDesc(false, true, type)
                });
        }

        public void Next()
        {
            if (_iterationNumber == 0 && _initialDelayMSec > 0)
            {
                try
                {
                    Thread.Sleep((int) _initialDelayMSec);
                }
                catch (ThreadInterruptedException)
                {
                    graphContext.SubmitSignal(
                        new DataFlowSignalFinalMarker());
                }
            }

            if (_iterationNumber > 0 && _periodDelayMSec > 0)
            {
                var nsecDelta = _lastSendTime - PerformanceObserver.NanoTime;
                var sleepTime = _periodDelayMSec - nsecDelta/1000000;
                if (sleepTime > 0)
                {
                    try
                    {
                        Thread.Sleep((int) sleepTime);
                    }
                    catch (ThreadInterruptedException)
                    {
                        graphContext.SubmitSignal(
                            new DataFlowSignalFinalMarker());
                    }
                }
            }

            if (iterations > 0 && _iterationNumber >= iterations)
            {
                graphContext.SubmitSignal(
                    new DataFlowSignalFinalMarker());
            }
            else
            {
                _iterationNumber++;
                if (_evaluators != null)
                {
                    var evaluateParams = new EvaluateParams(null, true, null);
                    var row = new Object[_evaluators.Length];
                    for (var i = 0; i < row.Length; i++)
                    {
                        if (_evaluators[i] != null)
                        {
                            row[i] = _evaluators[i].Evaluate(evaluateParams);
                        }
                    }
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("BeaconSource submitting row " + CompatExtensions.Render(row));
                    }

                    Object outputEvent = row;
                    if (_manufacturer != null)
                    {
                        if (!_produceEventBean)
                        {
                            outputEvent = _manufacturer.MakeUnderlying(row);
                        }
                        else
                        {
                            outputEvent = _manufacturer.Make(row);
                        }
                    }
                    graphContext.Submit(outputEvent);
                }
                else
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.Debug("BeaconSource submitting empty row");
                    }
                    graphContext.Submit(new Object[0]);
                }

                if (interval > 0)
                {
                    _lastSendTime = PerformanceObserver.NanoTime;
                }
            }
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
            // no action
        }

        public void Close(DataFlowOpCloseContext openContext)
        {
            // no action
        }

        public class DataFlowSignalFinalMarker : EPDataFlowSignalFinalMarker
        {
        }
    }
} // end of namespace
