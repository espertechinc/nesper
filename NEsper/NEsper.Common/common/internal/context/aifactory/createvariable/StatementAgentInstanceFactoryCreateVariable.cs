///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.context.util.StatementCPCacheService;

namespace com.espertech.esper.common.@internal.context.aifactory.createvariable
{
    public class StatementAgentInstanceFactoryCreateVariable : StatementAgentInstanceFactory,
        StatementReadyCallback
    {
        private ExprEvaluator variableInitialValueExpr;

        private string variableName;

        public string VariableName {
            set => variableName = value;
        }

        public ExprEvaluator VariableInitialValueExpr {
            set => variableInitialValueExpr = value;
        }

        public ResultSetProcessorFactoryProvider ResultSetProcessorFactoryProvider { get; set; }

        public EventType StatementEventType => ResultSetProcessorFactoryProvider.ResultEventType;

        public void StatementCreate(StatementContext statementContext)
        {
        }

        public void StatementDestroy(StatementContext statementContext)
        {
            statementContext.VariableManagementService.RemoveVariableIfFound(
                statementContext.DeploymentId, variableName);
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            var variableService = agentInstanceContext.VariableManagementService;
            var deploymentId = agentInstanceContext.DeploymentId;
            var agentInstanceId = agentInstanceContext.AgentInstanceId;
            IList<AgentInstanceStopCallback> stopCallbacks = new List<AgentInstanceStopCallback>(2);

            // allocate state
            // for create-variable with contexts we allocate on new-context
            if (agentInstanceContext.ContextProperties != null) {
                NullableObject<object> initialValue = null;
                if (variableInitialValueExpr != null) {
                    initialValue = new NullableObject<object>(
                        variableInitialValueExpr.Evaluate(null, true, agentInstanceContext));
                }

                agentInstanceContext.VariableManagementService.AllocateVariableState(
                    agentInstanceContext.DeploymentId, variableName, agentInstanceContext.AgentInstanceId,
                    isRecoveringResilient, initialValue, agentInstanceContext.EventBeanTypedEventFactory);
            }

            stopCallbacks.Add(
                new ProxyAgentInstanceStopCallback {
                    ProcStop = services => {
                        services.AgentInstanceContext.VariableManagementService.DeallocateVariableState(
                            services.AgentInstanceContext.DeploymentId, variableName,
                            agentInstanceContext.AgentInstanceId);
                    }
                });

            // register callback for listener-updates
            var reader = variableService.GetReader(deploymentId, variableName, agentInstanceContext.AgentInstanceId);
            var createVariableView = new CreateVariableView(this, agentInstanceContext, reader);
            variableService.RegisterCallback(
                deploymentId, variableName, agentInstanceContext.AgentInstanceId, createVariableView);
            stopCallbacks.Add(
                new ProxyAgentInstanceStopCallback {
                    ProcStop = services => {
                        services.AgentInstanceContext.VariableManagementService.UnregisterCallback(
                            deploymentId, variableName, agentInstanceId, createVariableView);
                    }
                });

            // create result-processing
            var pair = StatementAgentInstanceFactoryUtil.StartResultSetAndAggregation(
                ResultSetProcessorFactoryProvider, agentInstanceContext, false, null);
            var @out = new OutputProcessViewSimpleWProcessor(agentInstanceContext, pair.First);
            @out.Parent = createVariableView;
            createVariableView.Child = @out;

            var stopCallback = AgentInstanceUtil.FinalizeSafeStopCallbacks(stopCallbacks);
            return new StatementAgentInstanceFactoryCreateVariableResult(@out, stopCallback, agentInstanceContext);
        }

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

        public StatementAgentInstanceLock ObtainAgentInstanceLock(
            StatementContext statementContext,
            int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext);
        }

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            var meta = moduleIncidentals.Variables.Get(variableName);
            if (meta == null) {
                throw new UnsupportedOperationException("Missing variable information '" + variableName + "'");
            }

            // evaluate initial value
            if (meta.ValueWhenAvailable == null && variableInitialValueExpr != null &&
                meta.OptionalContextName == null && !recovery) {
                var initialValue = variableInitialValueExpr.Evaluate(
                    null, true, new ExprEvaluatorContextStatement(statementContext, false));
                var svc = statementContext.VariableManagementService;
                svc.CheckAndWrite(statementContext.DeploymentId, variableName, DEFAULT_AGENT_INSTANCE_ID, initialValue);
                svc.Commit();
                svc.SetLocalVersion();
            }
        }
    }
} // end of namespace