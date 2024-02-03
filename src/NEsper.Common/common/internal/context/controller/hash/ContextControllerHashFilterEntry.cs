///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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

            var agentInstanceContext = callback.AgentInstanceContextCreate;
            filterHandle = new EPStatementHandleCallbackFilter(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                this);
            var addendum = ContextManagerUtil.ComputeAddendumNonStmt(
                parentPartitionKeys,
                item.FilterSpecActivatable,
                callback.Realization);
            filterValueSet = item.FilterSpecActivatable.GetValueSet(
                null,
                addendum,
                agentInstanceContext,
                agentInstanceContext.StatementContextFilterEvalEnv);
            agentInstanceContext.FilterService.Add(
                item.FilterSpecActivatable.FilterForEventType,
                filterValueSet,
                filterHandle);
            var filtersVersion = agentInstanceContext.FilterService.FiltersVersion;
            agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                filtersVersion;
        }

        public void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            callback.MatchFound(item, theEvent, controllerPath);
        }

        public bool IsSubSelect => false;

        public int StatementId => callback.AgentInstanceContextCreate.StatementContext.StatementId;

        public void Destroy()
        {
            var agentInstanceContext = callback.AgentInstanceContextCreate;
            agentInstanceContext.FilterService.Remove(
                filterHandle,
                item.FilterSpecActivatable.FilterForEventType,
                filterValueSet);
            var filtersVersion = agentInstanceContext.FilterService.FiltersVersion;
            agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                filtersVersion;
        }

        public EPStatementHandleCallbackFilter FilterHandle => filterHandle;

        public void Transfer(
            FilterSpecActivatable activatable,
            AgentInstanceTransferServices xfer)
        {
            xfer.AgentInstanceContext.FilterService.Remove(
                filterHandle,
                activatable.FilterForEventType,
                filterValueSet);
            xfer.TargetFilterService.Add(activatable.FilterForEventType, filterValueSet, filterHandle);
        }
    }
} // end of namespace