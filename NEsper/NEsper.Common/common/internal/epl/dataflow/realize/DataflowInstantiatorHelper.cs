///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.context.aifactory.createdataflow;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class DataflowInstantiatorHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static OperatorStatisticsProvider Realize(
            DataflowDesc dataflow,
            IDictionary<int, object> operators,
            IList<LogicalChannelBinding> bindings,
            DataFlowSignalManager dataFlowSignalManager,
            EPDataFlowInstantiationOptions options,
            AgentInstanceContext agentInstanceContext)
        {
            IDictionary<int, OperatorMetadataDescriptor> operatorMetadata = dataflow.OperatorMetadata;

            // First pass: inject runtime context
            IDictionary<int, EPDataFlowEmitter> runtimeContexts = new Dictionary<int, EPDataFlowEmitter>();
            OperatorStatisticsProvider statisticsProvider = null;
            if (options.IsOperatorStatistics) {
                statisticsProvider = new OperatorStatisticsProvider(operatorMetadata);
            }

            foreach (int producerOpNum in dataflow.OperatorBuildOrder) {
                var operatorPrettyPrint = operatorMetadata.Get(producerOpNum).OperatorPrettyPrint;
                if (Log.IsDebugEnabled) {
                    Log.Debug("Generating runtime context for " + operatorPrettyPrint);
                }

                // determine the number of output streams
                var producingOp = operators.Get(producerOpNum);
                var numOutputStreams = operatorMetadata.Get(producerOpNum).NumOutputPorts;
                var targets = GetOperatorConsumersPerStream(
                    numOutputStreams,
                    producerOpNum,
                    operators,
                    operatorMetadata,
                    bindings);

                var runtimeContext = GenerateRuntimeContext(
                    agentInstanceContext,
                    dataflow,
                    options.DataFlowInstanceId,
                    producerOpNum,
                    operatorPrettyPrint,
                    dataFlowSignalManager,
                    targets,
                    options);

                if (options.IsOperatorStatistics) {
                    runtimeContext = new EPDataFlowEmitterWrapperWStatistics(
                        runtimeContext,
                        producerOpNum,
                        statisticsProvider,
                        options.IsCpuStatistics);
                }

                TypeHelper.SetFieldForAnnotation(producingOp, typeof(DataFlowContextAttribute), runtimeContext);
                runtimeContexts.Put(producerOpNum, runtimeContext);
            }

            // Second pass: hook punctuation such that it gets forwarded
            foreach (int producerOpNum in dataflow.OperatorBuildOrder) {
                var operatorPrettyPrint = operatorMetadata.Get(producerOpNum).OperatorPrettyPrint;
                if (Log.IsDebugEnabled) {
                    Log.Debug("Handling signals for " + operatorPrettyPrint);
                }

                // determine consumers that receive punctuation
                ISet<int> consumingOperatorsWithPunctuation = new HashSet<int>();
                foreach (var binding in bindings) {
                    if (!binding.LogicalChannel.OutputPort.HasPunctuation ||
                        binding.LogicalChannel.OutputPort.ProducingOpNum != producerOpNum) {
                        continue;
                    }

                    consumingOperatorsWithPunctuation.Add(binding.LogicalChannel.ConsumingOpNum);
                }

                // hook up a listener for each
                foreach (var consumerPunc in consumingOperatorsWithPunctuation) {
                    var context = runtimeContexts.Get(consumerPunc);
                    if (context == null) {
                        continue;
                    }

                    dataFlowSignalManager.AddSignalListener(
                        producerOpNum,
                        new ProxyDataFlowSignalListener {
                            ProcProcessSignal = signal => context.SubmitSignal(signal)
                        });
                }
            }

            return statisticsProvider;
        }

        private static IList<ObjectBindingPair>[] GetOperatorConsumersPerStream(
            int numOutputStreams,
            int producingOperator,
            IDictionary<int, object> operators,
            IDictionary<int, OperatorMetadataDescriptor> operatorMetadata,
            IList<LogicalChannelBinding> bindings)
        {
            IList<LogicalChannelBinding> channelsForProducer =
                LogicalChannelUtil.GetBindingsConsuming(producingOperator, bindings);
            if (channelsForProducer.IsEmpty()) {
                return null;
            }

            var submitTargets = new IList<ObjectBindingPair>[numOutputStreams];
            for (var i = 0; i < numOutputStreams; i++) {
                submitTargets[i] = new List<ObjectBindingPair>();
            }

            foreach (var binding in channelsForProducer) {
                var consumingOp = binding.LogicalChannel.ConsumingOpNum;
                var @operator = operators.Get(consumingOp);
                var producingStreamNum = binding.LogicalChannel.OutputPort.StreamNumber;
                var pairs = submitTargets[producingStreamNum];
                var metadata = operatorMetadata.Get(consumingOp);
                pairs.Add(new ObjectBindingPair(@operator, metadata.OperatorPrettyPrint, binding));
            }

            return submitTargets;
        }

        private static SignalHandler GetSignalHandler(
            int producerNum,
            object target,
            LogicalChannelBindingMethodDesc consumingSignalBindingDesc,
            ImportService importService)
        {
            if (consumingSignalBindingDesc == null) {
                return SignalHandlerDefault.INSTANCE;
            }

            if (consumingSignalBindingDesc.BindingType is LogicalChannelBindingTypePassAlong) {
                return new SignalHandlerDefaultWInvoke(target, consumingSignalBindingDesc.Method);
            }

            if (consumingSignalBindingDesc.BindingType is LogicalChannelBindingTypePassAlongWStream) {
                var streamInfo = (LogicalChannelBindingTypePassAlongWStream) consumingSignalBindingDesc.BindingType;
                return new SignalHandlerDefaultWInvokeStream(
                    target,
                    consumingSignalBindingDesc.Method,
                    streamInfo.StreamNum);
            }

            throw new IllegalStateException("Unrecognized signal binding: " + consumingSignalBindingDesc.BindingType);
        }

        private static SubmitHandler GetSubmitHandler(
            AgentInstanceContext agentInstanceContext,
            string dataflowName,
            string instanceId,
            int producerOpNum,
            string operatorPrettyPrint,
            DataFlowSignalManager dataFlowSignalManager,
            ObjectBindingPair target,
            EPDataFlowExceptionHandler optionalExceptionHandler,
            ImportService importService)
        {
            var signalHandler = GetSignalHandler(
                producerOpNum,
                target.Target,
                target.Binding.ConsumingSignalBindingDesc,
                importService);

            var receivingOpNum = target.Binding.LogicalChannel.ConsumingOpNum;
            var receivingOpPretty = target.Binding.LogicalChannel.ConsumingOpPrettyPrint;
            var receivingOpName = target.Binding.LogicalChannel.ConsumingOpName;
            var exceptionHandler = new EPDataFlowEmitterExceptionHandler(
                agentInstanceContext,
                dataflowName,
                instanceId,
                receivingOpName,
                receivingOpNum,
                receivingOpPretty,
                optionalExceptionHandler);

            var bindingType = target.Binding.ConsumingBindingDesc.BindingType;
            if (bindingType is LogicalChannelBindingTypePassAlong) {
                return new EPDataFlowEmitter1Stream1TargetPassAlong(
                    producerOpNum,
                    dataFlowSignalManager,
                    signalHandler,
                    exceptionHandler,
                    target,
                    importService);
            }

            if (bindingType is LogicalChannelBindingTypePassAlongWStream) {
                var type = (LogicalChannelBindingTypePassAlongWStream) bindingType;
                return new EPDataFlowEmitter1Stream1TargetPassAlongWStream(
                    producerOpNum,
                    dataFlowSignalManager,
                    signalHandler,
                    exceptionHandler,
                    target,
                    type.StreamNum,
                    importService);
            }

            if (bindingType is LogicalChannelBindingTypeUnwind) {
                return new EPDataFlowEmitter1Stream1TargetUnwind(
                    producerOpNum,
                    dataFlowSignalManager,
                    signalHandler,
                    exceptionHandler,
                    target,
                    importService);
            }

            throw new UnsupportedOperationException("Unsupported binding type '" + bindingType + "'");
        }

        private static EPDataFlowEmitter GenerateRuntimeContext(
            AgentInstanceContext agentInstanceContext,
            DataflowDesc dataflow,
            string instanceId,
            int producerOpNum,
            string operatorPrettyPrint,
            DataFlowSignalManager dataFlowSignalManager,
            IList<ObjectBindingPair>[] targetsPerStream,
            EPDataFlowInstantiationOptions options)
        {
            // handle no targets
            if (targetsPerStream == null) {
                return new EPDataFlowEmitterNoTarget(producerOpNum, dataFlowSignalManager);
            }

            var dataflowName = dataflow.DataflowName;
            var classpathImportService = agentInstanceContext.ImportServiceRuntime;

            // handle single-stream case
            if (targetsPerStream.Length == 1) {
                var targets = targetsPerStream[0];

                // handle single-stream single target case
                if (targets.Count == 1) {
                    var target = targets[0];
                    return GetSubmitHandler(
                        agentInstanceContext,
                        dataflow.DataflowName,
                        instanceId,
                        producerOpNum,
                        operatorPrettyPrint,
                        dataFlowSignalManager,
                        target,
                        options.ExceptionHandler,
                        classpathImportService);
                }

                var handlers = new SubmitHandler[targets.Count];
                for (var i = 0; i < handlers.Length; i++) {
                    handlers[i] = GetSubmitHandler(
                        agentInstanceContext,
                        dataflowName,
                        instanceId,
                        producerOpNum,
                        operatorPrettyPrint,
                        dataFlowSignalManager,
                        targets[i],
                        options.ExceptionHandler,
                        classpathImportService);
                }

                return new EPDataFlowEmitter1StreamNTarget(producerOpNum, dataFlowSignalManager, handlers);
            }

            // handle multi-stream case
            var handlersPerStream = new SubmitHandler[targetsPerStream.Length][];
            for (var streamNum = 0; streamNum < targetsPerStream.Length; streamNum++) {
                var handlers = new SubmitHandler[targetsPerStream[streamNum].Count];
                handlersPerStream[streamNum] = handlers;
                for (var i = 0; i < handlers.Length; i++) {
                    handlers[i] = GetSubmitHandler(
                        agentInstanceContext,
                        dataflowName,
                        instanceId,
                        producerOpNum,
                        operatorPrettyPrint,
                        dataFlowSignalManager,
                        targetsPerStream[streamNum][i],
                        options.ExceptionHandler,
                        classpathImportService);
                }
            }

            return new EPDataFlowEmitterNStreamNTarget(producerOpNum, dataFlowSignalManager, handlersPerStream);
        }
    }
} // end of namespace