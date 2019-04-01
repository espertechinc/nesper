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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public abstract class ContextControllerKeyedFilterEntry : FilterHandleCallback,
        ContextControllerFilterEntry
    {
        protected internal readonly ContextControllerKeyedImpl callback;
        protected internal readonly IntSeqKey controllerPath;
        protected internal readonly ContextControllerDetailKeyedItem item;
        protected internal readonly object[] parentPartitionKeys;

        protected internal EPStatementHandleCallbackFilter filterHandle;
        protected internal FilterValueSetParam[][] filterValueSet;

        public ContextControllerKeyedFilterEntry(
            ContextControllerKeyedImpl callback, IntSeqKey controllerPath, ContextControllerDetailKeyedItem item,
            object[] parentPartitionKeys)
        {
            this.callback = callback;
            this.controllerPath = controllerPath;
            this.item = item;
            this.parentPartitionKeys = parentPartitionKeys;
        }

        public EPStatementHandleCallbackFilter FilterHandle => filterHandle;

        public abstract void MatchFound(EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches);

        public bool IsSubSelect => false;

        public int StatementId => callback.AgentInstanceContextCreate.StatementContext.StatementId;

        public abstract void Destroy();

        protected void Start(FilterSpecActivatable activatable)
        {
            if (filterHandle != null) {
                throw new IllegalStateException("Already started");
            }

            var agentInstanceContext = callback.AgentInstanceContextCreate;
            filterHandle = new EPStatementHandleCallbackFilter(
                agentInstanceContext.EpStatementAgentInstanceHandle, this);
            var addendum = ContextManagerUtil.ComputeAddendumNonStmt(
                parentPartitionKeys, activatable, callback.Realization);
            filterValueSet = activatable.GetValueSet(
                null, addendum, agentInstanceContext, agentInstanceContext.StatementContextFilterEvalEnv);
            agentInstanceContext.FilterService.Add(activatable.FilterForEventType, filterValueSet, filterHandle);
            var filtersVersion = agentInstanceContext.FilterService.FiltersVersion;
            agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                filtersVersion;
        }

        protected void Stop(FilterSpecActivatable activatable)
        {
            if (filterHandle == null) {
                return;
            }

            var agentInstanceContext = callback.AgentInstanceContextCreate;
            agentInstanceContext.FilterService.Remove(filterHandle, activatable.FilterForEventType, filterValueSet);
            var filtersVersion = agentInstanceContext.FilterService.FiltersVersion;
            agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                filtersVersion;
            filterHandle = null;
            filterValueSet = null;
        }
    }
} // end of namespace