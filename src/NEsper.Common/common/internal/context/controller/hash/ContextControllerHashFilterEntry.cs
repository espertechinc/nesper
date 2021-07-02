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
        private readonly ContextControllerHashImpl _callback;
        private readonly IntSeqKey _controllerPath;
        private readonly ContextControllerDetailHashItem _item;

        private readonly EPStatementHandleCallbackFilter _filterHandle;
        private readonly FilterValueSetParam[][] _filterValueSet;

        public ContextControllerHashFilterEntry(
            ContextControllerHashImpl callback,
            IntSeqKey controllerPath,
            ContextControllerDetailHashItem item,
            object[] parentPartitionKeys)
        {
            this._callback = callback;
            this._controllerPath = controllerPath;
            this._item = item;

            AgentInstanceContext agentInstanceContext = callback.AgentInstanceContextCreate;
            _filterHandle = new EPStatementHandleCallbackFilter(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                this);
            FilterValueSetParam[][] addendum = ContextManagerUtil.ComputeAddendumNonStmt(
                parentPartitionKeys,
                item.FilterSpecActivatable,
                callback.Realization);
            _filterValueSet = item.FilterSpecActivatable.GetValueSet(
                null,
                addendum,
                agentInstanceContext,
                agentInstanceContext.StatementContextFilterEvalEnv);
            agentInstanceContext.FilterService.Add(
                item.FilterSpecActivatable.FilterForEventType,
                _filterValueSet,
                _filterHandle);
            long filtersVersion = agentInstanceContext.FilterService.FiltersVersion;
            agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                filtersVersion;
        }

        public void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            _callback.MatchFound(_item, theEvent, _controllerPath);
        }

        public bool IsSubSelect {
            get => false;
        }

        public int StatementId {
            get => _callback.AgentInstanceContextCreate.StatementContext.StatementId;
        }

        public void Destroy()
        {
            AgentInstanceContext agentInstanceContext = _callback.AgentInstanceContextCreate;
            agentInstanceContext.FilterService.Remove(
                _filterHandle,
                _item.FilterSpecActivatable.FilterForEventType,
                _filterValueSet);
            long filtersVersion = agentInstanceContext.FilterService.FiltersVersion;
            agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion =
                filtersVersion;
        }

        public EPStatementHandleCallbackFilter FilterHandle {
            get => _filterHandle;
        }

        public void Transfer(
            FilterSpecActivatable activatable,
            AgentInstanceTransferServices xfer)
        {
            xfer.AgentInstanceContext.FilterService.Remove(_filterHandle, activatable.FilterForEventType, _filterValueSet);
            xfer.TargetFilterService.Add(activatable.FilterForEventType, _filterValueSet, _filterHandle);
        }
    }
} // end of namespace