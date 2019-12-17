///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.strategy;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.join.queryplan
{
    /// <summary>
    ///     Plan to perform a nested iteration over child nodes.
    /// </summary>
    public class NestedIterationNode : QueryPlanNode
    {
        private readonly QueryPlanNode[] childNodes;
        private readonly int[] nestingOrder;

        public NestedIterationNode(
            QueryPlanNode[] childNodes,
            int[] nestingOrder)
        {
            this.childNodes = childNodes;
            this.nestingOrder = nestingOrder;
        }

        public override ExecNode MakeExec(
            AgentInstanceContext agentInstanceContext,
            IDictionary<TableLookupIndexReqKey, EventTable>[] indexesPerStream,
            EventType[] streamTypes,
            Viewable[] streamViews,
            VirtualDWView[] viewExternal,
            ILockable[] tableSecondaryIndexLocks)
        {
            var execNode = new NestedIterationExecNode(nestingOrder);
            foreach (var child in childNodes) {
                var childExec = child.MakeExec(
                    agentInstanceContext,
                    indexesPerStream,
                    streamTypes,
                    streamViews,
                    viewExternal,
                    tableSecondaryIndexLocks);
                execNode.AddChildNode(childExec);
            }

            return execNode;
        }
    }
} // end of namespace