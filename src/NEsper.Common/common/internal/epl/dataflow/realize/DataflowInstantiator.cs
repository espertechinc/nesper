///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.aifactory.createdataflow;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.core;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.epl.dataflow.Runnables;
using com.espertech.esper.common.@internal.epl.dataflow.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.dataflow.realize
{
    public class DataflowInstantiator
    {
        public static EPDataFlowInstance Instantiate(
            IContainer container,
            int agentInstanceId,
            DataflowDesc dataflow,
            EPDataFlowInstantiationOptions options)
        {
            var statementContext = dataflow.StatementContext;

            // allocate agent instance context
            var @lock = statementContext.StatementAgentInstanceLockFactory.GetStatementLock(
                statementContext.StatementName,
                statementContext.Annotations,
                statementContext.IsStatelessSelect,
                statementContext.StatementType);
            var handle = new EPStatementAgentInstanceHandle(statementContext.EpStatementHandle, agentInstanceId, @lock);
            var auditProvider = statementContext.StatementInformationals.AuditProvider;
            var instrumentationProvider = statementContext.StatementInformationals.InstrumentationProvider;
            var agentInstanceContext = new AgentInstanceContext(statementContext, handle, null, null, auditProvider, instrumentationProvider);

            // assure variables
            statementContext.VariableManagementService.SetLocalVersion();

            // instantiate operators
            var operators = InstantiateOperators(container, agentInstanceContext, options, dataflow);

            // determine binding of each channel to input methods (ports)
            IList<LogicalChannelBinding> operatorChannelBindings = new List<LogicalChannelBinding>();
            foreach (var channel in dataflow.LogicalChannels) {
                var targetClass = operators.Get(channel.ConsumingOpNum).GetType();
                var consumingMethod = FindMatchingMethod(channel.ConsumingOpPrettyPrint, targetClass, channel, false);

                LogicalChannelBindingMethodDesc onSignalMethod = null;
                if (channel.OutputPort.HasPunctuation) {
                    onSignalMethod = FindMatchingMethod(channel.ConsumingOpPrettyPrint, targetClass, channel, true);
                }

                operatorChannelBindings.Add(new LogicalChannelBinding(channel, consumingMethod, onSignalMethod));
            }

            // obtain realization
            var dataFlowSignalManager = new DataFlowSignalManager();
            var statistics = DataflowInstantiatorHelper.Realize(
                dataflow,
                operators,
                operatorChannelBindings,
                dataFlowSignalManager,
                options,
                agentInstanceContext);

            // For each GraphSource add runnable
            IList<GraphSourceRunnable> sourceRunnables = new List<GraphSourceRunnable>();
            var audit = AuditEnum.DATAFLOW_SOURCE.GetAudit(statementContext.Annotations) != null;
            foreach (var operatorEntry in operators) {
                if (!(operatorEntry.Value is DataFlowSourceOperator)) {
                    continue;
                }

                var meta = dataflow.OperatorMetadata.Get(operatorEntry.Key);

                var graphSource = (DataFlowSourceOperator) operatorEntry.Value;
                var runnable = new GraphSourceRunnable(
                    agentInstanceContext,
                    graphSource,
                    dataflow.DataflowName,
                    options.DataFlowInstanceId,
                    meta.OperatorName,
                    operatorEntry.Key,
                    meta.OperatorPrettyPrint,
                    options.ExceptionHandler,
                    audit);
                sourceRunnables.Add(runnable);

                dataFlowSignalManager.AddSignalListener(operatorEntry.Key, runnable);
            }

            return new EPDataFlowInstanceImpl(
                options.DataFlowInstanceUserObject,
                options.DataFlowInstanceId,
                statistics,
                operators,
                sourceRunnables,
                dataflow,
                agentInstanceContext,
                statistics,
                options.ParametersURIs);
        }

        private static IDictionary<int, object> InstantiateOperators(
            IContainer container,
            AgentInstanceContext agentInstanceContext,
            EPDataFlowInstantiationOptions options,
            DataflowDesc dataflow)
        {
            IDictionary<int, object> operators = new Dictionary<int, object>();

            foreach (var operatorNum in dataflow.OperatorMetadata.Keys) {
                var @operator = InstantiateOperator(
                    container, operatorNum, dataflow, options, agentInstanceContext);
                operators.Put(operatorNum, @operator);
            }

            return operators;
        }

        private static object InstantiateOperator(
            IContainer container,
            int operatorNum,
            DataflowDesc dataflow,
            EPDataFlowInstantiationOptions options,
            AgentInstanceContext agentInstanceContext)
        {
            var operatorFactory = dataflow.OperatorFactories.Get(operatorNum);
            var metadata = dataflow.OperatorMetadata.Get(operatorNum);

            // see if the operator is already provided by options
            var operatorX = options.OperatorProvider?.Provide(
                new EPDataFlowOperatorProviderContext(
                    dataflow.DataflowName,
                    metadata.OperatorName,
                    operatorFactory));
            if (operatorX != null) {
                return operatorX;
            }

            IDictionary<string, object> additionalParameters = null;
            if (options.ParametersURIs != null) {
                var prefix = metadata.OperatorName + "/";
                foreach (var entry in options.ParametersURIs) {
                    if (!entry.Key.StartsWith(prefix)) {
                        continue;
                    }

                    if (additionalParameters == null) {
                        additionalParameters = new Dictionary<string, object>();
                    }

                    additionalParameters.Put(entry.Key.Substring(prefix.Length), entry.Value);
                }
            }

            object @operator;
            try {
                @operator = operatorFactory.Operator(
                    new DataFlowOpInitializeContext(
                        container,
                        dataflow.DataflowName,
                        metadata.OperatorName,
                        operatorNum,
                        agentInstanceContext,
                        additionalParameters,
                        options.DataFlowInstanceId,
                        options.ParameterProvider,
                        operatorFactory,
                        options.DataFlowInstanceUserObject));
            }
            catch (Exception t) {
                var meta = dataflow.OperatorMetadata.Get(operatorNum);
                throw new EPException(
                    "Failed to obtain operator instance for '" + meta.OperatorName + "': " + t.Message,
                    t);
            }

            return @operator;
        }

        private static LogicalChannelBindingMethodDesc FindMatchingMethod(
            string operatorName,
            Type target,
            LogicalChannel channelDesc,
            bool isPunctuation)
        {
            if (isPunctuation) {
                foreach (var method in target.GetMethods()) {
                    if (method.Name.Equals("OnSignal")) {
                        return new LogicalChannelBindingMethodDesc(method, LogicalChannelBindingTypePassAlong.INSTANCE);
                    }
                }

                return null;
            }

            var outputPort = channelDesc.OutputPort;

            Type[] expectedIndividual;
            Type expectedUnderlying;
            EventType expectedUnderlyingType;
            var typeDesc = outputPort.GraphTypeDesc;

            if (typeDesc.IsWildcard) {
                expectedIndividual = new Type[0];
                expectedUnderlying = null;
                expectedUnderlyingType = null;
            }
            else {
                expectedIndividual = new Type[typeDesc.EventType.PropertyNames.Length];
                var i = 0;
                foreach (var descriptor in typeDesc.EventType.PropertyDescriptors) {
                    expectedIndividual[i] = descriptor.PropertyType;
                    i++;
                }

                expectedUnderlying = typeDesc.EventType.UnderlyingType;
                expectedUnderlyingType = typeDesc.EventType;
            }

            string channelSpecificMethodName = null;
            if (channelDesc.ConsumingOptStreamAliasName != null) {
                channelSpecificMethodName = "On" + channelDesc.ConsumingOptStreamAliasName;
            }
            
            var methods = target.GetMethods();
            foreach (var method in methods) {
                var eligible = method.Name.Equals("OnInput");
                if (!eligible && method.Name.Equals(channelSpecificMethodName)) {
                    eligible = true;
                }

                if (!eligible) {
                    continue;
                }

                // handle Object[]
                var paramTypes = method.GetParameterTypes();
                var numParams = paramTypes.Length;

                if (expectedUnderlying != null) {
                    if (numParams == 1 &&
                        TypeHelper.IsAssignmentCompatible(expectedUnderlying, paramTypes[0])) {
                        return new LogicalChannelBindingMethodDesc(
                            method, LogicalChannelBindingTypePassAlong.INSTANCE);
                    }

                    if (numParams == 2 &&
                        paramTypes[0].IsInt32() &&
                        TypeHelper.IsAssignmentCompatible(expectedUnderlying, paramTypes[1])) {
                        return new LogicalChannelBindingMethodDesc(
                            method,
                            new LogicalChannelBindingTypePassAlongWStream(channelDesc.ConsumingOpStreamNum));
                    }
                }

                if (numParams == 1 &&
                    (paramTypes[0] == typeof(object) ||
                     paramTypes[0] == typeof(object[]) &&
                     method.IsVarArgs())) {
                    return new LogicalChannelBindingMethodDesc(
                        method, LogicalChannelBindingTypePassAlong.INSTANCE);
                }

                if (numParams == 2 &&
                    paramTypes[0] == typeof(int) &&
                    (paramTypes[1] == typeof(object) ||
                     paramTypes[1] == typeof(object[]) &&
                     method.IsVarArgs())) {
                    return new LogicalChannelBindingMethodDesc(
                        method,
                        new LogicalChannelBindingTypePassAlongWStream(channelDesc.ConsumingOpStreamNum));
                }

                // if exposing a method that exactly matches each property type in order, use that, i.e. "onInut(String p0, int p1)"
                if (expectedUnderlyingType is ObjectArrayEventType &&
                    TypeHelper.IsSignatureCompatible(expectedIndividual, paramTypes)) {
                    return new LogicalChannelBindingMethodDesc(method, LogicalChannelBindingTypeUnwind.INSTANCE);
                }
            }

            ISet<string> choices = new LinkedHashSet<string>();
            choices.Add(typeof(object).Name);
            choices.Add("Object[]");
            if (expectedUnderlying != null) {
                choices.Add(expectedUnderlying.Name);
            }

            throw new ExprValidationException(
                "Failed to find OnInput method on for operator '" +
                operatorName +
                "' class " +
                target.Name +
                ", expected an OnInput method that takes any of {" +
                CollectionUtil.ToString(choices) +
                "}");
        }
    }
} // end of namespace