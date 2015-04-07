///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.annotation;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.dataflow.annotations;
using com.espertech.esper.dataflow.interfaces;
using com.espertech.esper.dataflow.util;
using com.espertech.esper.util;

namespace com.espertech.esper.dataflow.core
{
    public class RealizationFactoryInterface
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static DataflowStartDesc Realize(String dataFlowName,
                                             IDictionary<int, Object> operators,
                                             IDictionary<int, OperatorMetadataDescriptor> operatorMetadata,
                                             ICollection<int> operatorBuildOrder,
                                             IList<LogicalChannelBinding> bindings,
                                             DataFlowSignalManager dataFlowSignalManager,
                                             EPDataFlowInstantiationOptions options,
                                             EPServicesContext services,
                                             StatementContext statementContext)
        {
            // First pass: inject runtime context
            IDictionary<int, EPDataFlowEmitter> runtimeContexts = new Dictionary<int, EPDataFlowEmitter>();
            OperatorStatisticsProvider statisticsProvider = null;
            if (options.IsOperatorStatistics())
            {
                statisticsProvider = new OperatorStatisticsProvider(operatorMetadata);
            }

            bool audit = AuditEnum.DATAFLOW_OP.GetAudit(statementContext.Annotations) != null;
            foreach (int producerOpNum in operatorBuildOrder)
            {
                String operatorPrettyPrint = operatorMetadata.Get(producerOpNum).OperatorPrettyPrint;
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Generating runtime context for " + operatorPrettyPrint);
                }

                // determine the number of output streams
                Object producingOp = operators.Get(producerOpNum);
                int numOutputStreams = operatorMetadata.Get(producerOpNum).OperatorSpec.Output.Items.Count;
                IList<ObjectBindingPair>[] targets = GetOperatorConsumersPerStream(
                    numOutputStreams, producerOpNum, operators, operatorMetadata, bindings);

                EPDataFlowEmitter runtimeContext = GenerateRuntimeContext(
                    statementContext.EngineURI, statementContext.StatementName, audit, dataFlowName, producerOpNum,
                    operatorPrettyPrint, dataFlowSignalManager, targets, options);

                if (options.IsOperatorStatistics())
                {
                    runtimeContext = new EPDataFlowEmitterWrapperWStatistics(
                        runtimeContext, producerOpNum, statisticsProvider, options.IsCpuStatistics());
                }

                TypeHelper.SetFieldForAnnotation(producingOp, typeof (DataFlowContextAttribute), runtimeContext);
                runtimeContexts.Put(producerOpNum, runtimeContext);
            }

            // Second pass: hook punctuation such that it gets forwarded
            foreach (int producerOpNum in operatorBuildOrder)
            {
                String operatorPrettyPrint = operatorMetadata.Get(producerOpNum).OperatorPrettyPrint;
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("Handling signals for " + operatorPrettyPrint);
                }

                // determine consumers that receive punctuation
                ICollection<int> consumingOperatorsWithPunctuation = new HashSet<int>();
                foreach (LogicalChannelBinding binding in bindings)
                {
                    if (!binding.LogicalChannel.OutputPort.HasPunctuation ||
                        binding.LogicalChannel.OutputPort.ProducingOpNum != producerOpNum)
                    {
                        continue;
                    }
                    consumingOperatorsWithPunctuation.Add(binding.LogicalChannel.ConsumingOpNum);
                }

                // hook up a listener for each
                foreach (int consumerPunc in consumingOperatorsWithPunctuation)
                {
                    EPDataFlowEmitter context = runtimeContexts.Get(consumerPunc);
                    if (context == null)
                    {
                        continue;
                    }
                    dataFlowSignalManager.AddSignalListener(
                        producerOpNum, new ProxyDataFlowSignalListener
                        {
                            ProcSignal = context.SubmitSignal
                        });
                }
            }

            return new DataflowStartDesc(statisticsProvider);
        }

        private static IList<ObjectBindingPair>[] GetOperatorConsumersPerStream(int numOutputStreams, int producingOperator, IDictionary<int, Object> operators, IDictionary<int, OperatorMetadataDescriptor> operatorMetadata, IList<LogicalChannelBinding> bindings)
        {
            var channelsForProducer = LogicalChannelUtil.GetBindingsConsuming(producingOperator, bindings);
            if (channelsForProducer.IsEmpty())
            {
                return null;
            }

            var submitTargets = new IList<ObjectBindingPair>[numOutputStreams];
            for (int i = 0; i < numOutputStreams; i++)
            {
                submitTargets[i] = new List<ObjectBindingPair>();
            }

            foreach (LogicalChannelBinding binding in channelsForProducer)
            {
                var consumingOp = binding.LogicalChannel.ConsumingOpNum;
                var @operator = operators.Get(consumingOp);
                var producingStreamNum = binding.LogicalChannel.OutputPort.StreamNumber;
                var pairs = submitTargets[producingStreamNum];
                var metadata = operatorMetadata.Get(consumingOp);
                pairs.Add(new ObjectBindingPair(@operator, metadata.OperatorPrettyPrint, binding));
            }
            return submitTargets;
        }

        private static SignalHandler GetSignalHandler(int producerNum, Object target, LogicalChannelBindingMethodDesc consumingSignalBindingDesc)
        {
            if (consumingSignalBindingDesc == null)
            {
                return SignalHandlerDefault.INSTANCE;
            }
            else
            {
                if (consumingSignalBindingDesc.BindingType is LogicalChannelBindingTypePassAlong)
                {
                    return new SignalHandlerDefaultWInvoke(target, consumingSignalBindingDesc.Method);
                }
                else if (consumingSignalBindingDesc.BindingType is LogicalChannelBindingTypePassAlongWStream)
                {
                    var streamInfo =
                        (LogicalChannelBindingTypePassAlongWStream) consumingSignalBindingDesc.BindingType;
                    return new SignalHandlerDefaultWInvokeStream(
                        target, consumingSignalBindingDesc.Method, streamInfo.StreamNum);
                }
                else
                {
                    throw new IllegalStateException(
                        "Unrecognized signal binding: " + consumingSignalBindingDesc.BindingType);
                }
            }
        }

        private static SubmitHandler GetSubmitHandler(String engineURI, String statementName, bool audit, String dataflowName, int producerOpNum, String operatorPrettyPrint, DataFlowSignalManager dataFlowSignalManager, ObjectBindingPair target, EPDataFlowExceptionHandler optionalExceptionHandler)
        {
            var signalHandler = GetSignalHandler(producerOpNum, target.Target, target.Binding.ConsumingSignalBindingDesc);
    
            var receivingOpNum = target.Binding.LogicalChannel.ConsumingOpNum;
            var receivingOpPretty = target.Binding.LogicalChannel.ConsumingOpPrettyPrint;
            var receivingOpName = target.Binding.LogicalChannel.ConsumingOpName;
            var exceptionHandler = new EPDataFlowEmitterExceptionHandler(engineURI, statementName, audit, dataflowName, receivingOpName, receivingOpNum, receivingOpPretty, optionalExceptionHandler);

            var bindingType = target.Binding.ConsumingBindingDesc.BindingType;
            if (bindingType is LogicalChannelBindingTypePassAlong) {
                return new EPDataFlowEmitter1Stream1TargetPassAlong(producerOpNum, dataFlowSignalManager, signalHandler, exceptionHandler, target);
            }
            else if (bindingType is LogicalChannelBindingTypePassAlongWStream) {
                var type = (LogicalChannelBindingTypePassAlongWStream)bindingType;
                return new EPDataFlowEmitter1Stream1TargetPassAlongWStream(producerOpNum, dataFlowSignalManager, signalHandler, exceptionHandler, target, type.StreamNum);
            }
            else if (bindingType is LogicalChannelBindingTypeUnwind) {
                return new EPDataFlowEmitter1Stream1TargetUnwind(producerOpNum, dataFlowSignalManager, signalHandler, exceptionHandler, target);
            }
            else {
                throw new UnsupportedOperationException("TODO");
            }
        }

        private static EPDataFlowEmitter GenerateRuntimeContext(String engineURI,
                                                                String statementName,
                                                                bool audit,
                                                                String dataflowName,
                                                                int producerOpNum,
                                                                String operatorPrettyPrint,
                                                                DataFlowSignalManager dataFlowSignalManager,
                                                                IList<ObjectBindingPair>[] targetsPerStream,
                                                                EPDataFlowInstantiationOptions options)
        {
            // handle no targets
            if (targetsPerStream == null)
            {
                return new EPDataFlowEmitterNoTarget(producerOpNum, dataFlowSignalManager);
            }

            // handle single-stream case
            if (targetsPerStream.Length == 1)
            {
                IList<ObjectBindingPair> targets = targetsPerStream[0];

                // handle single-stream single target case
                if (targets.Count == 1)
                {
                    var target = targets[0];
                    return GetSubmitHandler(
                        engineURI, statementName, audit, dataflowName, producerOpNum, operatorPrettyPrint,
                        dataFlowSignalManager, target, options.GetExceptionHandler());
                }

                var handlers = new SubmitHandler[targets.Count];
                for (int i = 0; i < handlers.Length; i++)
                {
                    handlers[i] = GetSubmitHandler(
                        engineURI, statementName, audit, dataflowName, producerOpNum, operatorPrettyPrint,
                        dataFlowSignalManager, targets[i], options.GetExceptionHandler());
                }
                return new EPDataFlowEmitter1StreamNTarget(producerOpNum, dataFlowSignalManager, handlers);
            }
                // handle multi-stream case
            else
            {
                var handlersPerStream = new SubmitHandler[targetsPerStream.Length][];
                for (int streamNum = 0; streamNum < targetsPerStream.Length; streamNum++)
                {
                    var handlers = new SubmitHandler[targetsPerStream[streamNum].Count];
                    handlersPerStream[streamNum] = handlers;
                    for (int i = 0; i < handlers.Length; i++)
                    {
                        handlers[i] = GetSubmitHandler(
                            engineURI, statementName, audit, dataflowName, producerOpNum, operatorPrettyPrint,
                            dataFlowSignalManager, targetsPerStream[streamNum][i], options.GetExceptionHandler());
                    }
                }
                return new EPDataFlowEmitterNStreamNTarget(producerOpNum, dataFlowSignalManager, handlersPerStream);
            }
        }
    }
}
