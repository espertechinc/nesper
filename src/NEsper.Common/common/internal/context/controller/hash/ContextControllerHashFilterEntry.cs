///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashFilterEntry : FilterHandleCallback,
        ContextControllerFilterEntry
    {
        private readonly ContextControllerHashImpl callback;
        private readonly IntSeqKey controllerPath;
        private readonly ContextControllerDetailHashItem item;

        private readonly EPStatementHandleCallbackFilter filterHandle;
        private readonly FilterValueSetParam[][] filterValueSet;

        public ContextControllerHashFilterEntry(
            ContextControllerHashImpl callback,
            IntSeqKey controllerPath,
            ContextControllerDetailHashItem item,
            object[] parentPartitionKeys)
        {
            this.callback = callback;
            this.controllerPath = controllerPath;
            this.item = item;

            AgentInstanceContext agentInstanceContext = callback.AgentInstanceContextCreate;
            this.filterHandle = new EPStatementHandleCallbackFilter(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                this);
            FilterValueSetParam[][] addendum = ContextManagerUtil.ComputeAddendumNonStmt(
                parentPartitionKeys,
                item.FilterSpecActivatable,
                callback.Realization);
            this.filterValueSet = item.FilterSpecActivatable.GetValueSet(
                null,
                addendum,
                agentInstanceContext,
                agentInstanceContext.StatementContextFilterEvalEnv);
            agentInstanceContext.FilterService.Add(
                item.FilterSpecActivatable.FilterForEventType,
                filterValueSet,
                filterHandle);
            long filtersVersion = agentInstanceContext.FilterService.FiltersVersion;
            agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                filtersVersion;
        }

        public void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            callback.MatchFound(item, theEvent, controllerPath);
        }

        public bool IsSubSelect {
            get => false;
        }

        public int StatementId {
            get => callback.AgentInstanceContextCreate.StatementContext.StatementId;
        }

        public void Destroy()
        {
            AgentInstanceContext agentInstanceContext = callback.AgentInstanceContextCreate;
            agentInstanceContext.FilterService.Remove(
                filterHandle,
                item.FilterSpecActivatable.FilterForEventType,
                filterValueSet);
            long filtersVersion = agentInstanceContext.FilterService.FiltersVersion;
            agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                filtersVersion;
        }

        public EPStatementHandleCallbackFilter FilterHandle {
            get => filterHandle;
        }

        public void Transfer(
            FilterSpecActivatable activatable,
            AgentInstanceTransferServices xfer)
        {
            xfer.AgentInstanceContext.FilterService.Remove(filterHandle, activatable.FilterForEventType, filterValueSet);
            xfer.TargetFilterService.Add(activatable.FilterForEventType, filterValueSet, filterHandle);
        }
    }
} // end of namespace