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
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.aifactory.update
{
    public class StatementAgentInstanceFactoryUpdate : StatementAgentInstanceFactory,
        StatementReadyCallback
    {
        private InternalEventRouterDesc desc;
        private IDictionary<int, SubSelectFactory> subselects;
        private InternalRoutePreprocessView viewable;

        public InternalEventRouterDesc Desc {
            set => desc = value;
        }

        public IDictionary<int, SubSelectFactory> Subselects {
            set => subselects = value;
        }

        public EventType StatementEventType => desc.EventType;

        public void StatementCreate(StatementContext statementContext)
        {
        }

        public void StatementDestroy(StatementContext statementContext)
        {
        }

        public void StatementDestroyPreconditions(StatementContext statementContext)
        {
        }

        public StatementAgentInstanceFactoryResult NewContext(
            AgentInstanceContext agentInstanceContext,
            bool isRecoveringResilient)
        {
            IList<AgentInstanceMgmtCallback> stopCallbacks = new List<AgentInstanceMgmtCallback>();
            stopCallbacks.Add(
                new ProxyAgentInstanceMgmtCallback(
                    services => agentInstanceContext.InternalEventRouter.RemovePreprocessing(desc.EventType, desc),
                    services => { }));

            var subselectActivations = SubSelectHelperStart.StartSubselects(
                subselects,
                agentInstanceContext,
                stopCallbacks,
                isRecoveringResilient);

            var hasSubselect = !subselectActivations.IsEmpty();
            agentInstanceContext.InternalEventRouter.AddPreprocessing(
                desc,
                viewable,
                agentInstanceContext.StatementContext,
                hasSubselect);

            var stopCallback = AgentInstanceUtil.FinalizeSafeStopCallbacks(stopCallbacks);
            return new StatementAgentInstanceFactoryUpdateResult(
                viewable,
                stopCallback,
                agentInstanceContext,
                subselectActivations);
        }

        public AIRegistryRequirements RegistryRequirements => AIRegistryRequirements.NoRequirements();

        public IReaderWriterLock ObtainAgentInstanceLock(
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
            viewable = new InternalRoutePreprocessView(desc.EventType, statementContext.StatementResultService);
            desc.Annotations = statementContext.Annotations;
        }
    }
} // end of namespaceom