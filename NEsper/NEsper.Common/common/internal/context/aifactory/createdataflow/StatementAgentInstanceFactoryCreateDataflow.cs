///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.context.aifactory.createdataflow
{
    public class StatementAgentInstanceFactoryCreateDataflow : StatementAgentInstanceFactory,
        StatementReadyCallback
    {
        private DataflowDesc dataflow;

        private Viewable viewable;

        public EventType EventType {
            set => viewable = new ViewableDefaultImpl(value);
        }

        public DataflowDesc Dataflow {
            set => dataflow = value;
        }

        public void StatementCreate(StatementContext statementContext)
        {
        }

        public void StatementDestroy(StatementContext statementContext)
        {
            statementContext.StatementContextRuntimeServices.DataflowService.RemoveDataflow(
                statementContext.DeploymentId, dataflow);
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            return new StatementAgentInstanceFactoryCreateDataflowResult(
                viewable, AgentInstanceStopCallbackNoAction.INSTANCE, agentInstanceContext, dataflow);
        }

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

        public EventType StatementEventType => viewable.EventType;

        public StatementAgentInstanceLock ObtainAgentInstanceLock(
            StatementContext statementContext, int agentInstanceId)
        {
            return AgentInstanceUtil.NewLock(statementContext);
        }

        public void Ready(StatementContext statementContext, ModuleIncidentals moduleIncidentals, bool recovery)
        {
            foreach (var entry in dataflow.OperatorFactories) {
                entry.Value.InitializeFactory(
                    new DataFlowOpFactoryInitializeContext(dataflow.DataflowName, entry.Key, statementContext));
            }

            dataflow.StatementContext = statementContext;
            statementContext.StatementContextRuntimeServices.DataflowService.AddDataflow(
                statementContext.DeploymentId, dataflow);
        }
    }
} // end of namespace