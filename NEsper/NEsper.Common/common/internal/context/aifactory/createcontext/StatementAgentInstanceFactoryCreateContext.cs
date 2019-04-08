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
        private string contextName;

        public EventType StatementEventType { get; private set; }

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

        public void SetContextName(string contextName)
        {
            this.contextName = contextName;
        }

        public void SetStatementEventType(EventType statementEventType)
        {
            StatementEventType = statementEventType;
        }

        public void Ready(StatementContext statementContext, ModuleIncidentals moduleIncidentals, bool recovery)
        {
            ContextManager contextManager =
                statementContext.ContextManagementService.GetContextManager(statementContext.DeploymentId, contextName);
            contextManager.SetStatementContext(statementContext);
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            ContextManager manager = agentInstanceContext.ContextManagementService.GetContextManager(
                agentInstanceContext.DeploymentId, contextName);
            agentInstanceContext.EpStatementAgentInstanceHandle.FilterFaultHandler = manager;

            ContextManagerRealization realization = manager.AllocateNewRealization(agentInstanceContext);
            return new StatementAgentInstanceFactoryCreateContextResult(
                new ZeroDepthStreamNoIterate(StatementEventType), AgentInstanceStopCallbackConstants.INSTANCE_NO_ACTION,
                agentInstanceContext, null, null, null, null, null, null, new EmptyList<StatementAgentInstancePreload>(),
                realization);
        }

        public void StatementCreate(StatementContext statementContext)
        {
            CopyOnWriteList<ContextStateListener> listeners = statementContext.ContextManagementService.Listeners;
            ContextStateEventUtil.DispatchContext(
                listeners,
                () => new ContextStateEventContextCreated(
                    statementContext.RuntimeURI, statementContext.DeploymentId, contextName),
                (listener, @event) => listener.OnContextCreated(@event));
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
            ContextManager manager =
                statementContext.ContextManagementService.GetContextManager(statementContext.DeploymentId, contextName);
            int count = manager.CountStatements(stmt => !stmt.DeploymentId.Equals(statementContext.DeploymentId));
            if (count != 0) {
                throw new UndeployPreconditionException(
                    "Context by name '" + contextName +
                    "' is still referenced by statements and may not be undeployed");
            }
        }

        public void StatementDestroy(StatementContext statementContext)
        {
            statementContext.ContextManagementService.DestroyedContext(
                statementContext.RuntimeURI, statementContext.DeploymentId, contextName);
        }

        public StatementAgentInstanceLock ObtainAgentInstanceLock(
            StatementContext statementContext, int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext);
        }
    }
} // end of namespace