///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createcontext
{
    public class StatementAgentInstanceFactoryCreateContext : StatementAgentInstanceFactory,
        StatementReadyCallback
    {
        public string ContextName { get; set; }

        public EventType StatementEventType { get; set; }

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            var manager = agentInstanceContext.ContextManagementService.GetContextManager(
                agentInstanceContext.DeploymentId,
                ContextName);
            agentInstanceContext.EpStatementAgentInstanceHandle.FilterFaultHandler = manager;

            var realization = manager.AllocateNewRealization(agentInstanceContext);
            return new StatementAgentInstanceFactoryCreateContextResult(
                new ZeroDepthStreamNoIterate(StatementEventType),
                AgentInstanceStopCallbackConstants.INSTANCE_NO_ACTION,
                agentInstanceContext,
                null,
                null,
                null,
                null,
                null,
                null,
                new EmptyList<StatementAgentInstancePreload>(),
                realization);
        }

        public void StatementCreate(StatementContext statementContext)
        {
            var listeners = statementContext.ContextManagementService.Listeners;
            ContextStateEventUtil.DispatchContext(
                listeners,
                () => new ContextStateEventContextCreated(
                    statementContext.RuntimeURI,
                    statementContext.DeploymentId,
                    ContextName),
                (
                    listener,
                    @event) => listener.OnContextCreated(@event));
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
            var manager =
                statementContext.ContextManagementService.GetContextManager(statementContext.DeploymentId, ContextName);
            var count = manager.CountStatements(stmt => !stmt.DeploymentId.Equals(statementContext.DeploymentId));
            if (count != 0) {
                throw new UndeployPreconditionException(
                    "Context by name '" +
                    ContextName +
                    "' is still referenced by statements and may not be undeployed");
            }
        }

        public void StatementDestroy(StatementContext statementContext)
        {
            statementContext.ContextManagementService.DestroyedContext(
                statementContext.RuntimeURI,
                statementContext.DeploymentId,
                ContextName);
        }

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
            var contextManager =
                statementContext.ContextManagementService.GetContextManager(statementContext.DeploymentId, ContextName);
            contextManager.SetStatementContext(statementContext);
        }
    }
} // end of namespace