///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    public class RowRecogPartitionStateRepoGroupMeta
    {
        public RowRecogPartitionStateRepoGroupMeta(
            bool hasInterval,
            ExprEvaluator partitionExpression,
            AgentInstanceContext agentInstanceContext)
        {
            IsInterval = hasInterval;
            PartitionExpression = partitionExpression;
            AgentInstanceContext = agentInstanceContext;
        }

        public bool IsInterval { get; }

        public ExprEvaluator PartitionExpression { get; }

        public AgentInstanceContext AgentInstanceContext { get; }

        public EventBean[] EventsPerStream { get; } = new EventBean[1];
    }
} // end of namespace