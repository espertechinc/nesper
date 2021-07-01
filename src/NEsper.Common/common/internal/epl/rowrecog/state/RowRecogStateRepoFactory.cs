///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.rowrecog.core;

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    /// <summary>
    ///     Service for creating match-recognize factory and state services.
    /// </summary>
    public interface RowRecogStateRepoFactory
    {
        RowRecogPartitionStateRepo MakeSingle(
            RowRecogPreviousStrategyImpl prevGetter,
            AgentInstanceContext agentInstanceContext,
            RowRecogNFAView view,
            bool keepScheduleState,
            RowRecogPartitionTerminationStateComparator terminationStateCompare);

        RowRecogPartitionStateRepo MakePartitioned(
            RowRecogPreviousStrategyImpl prevGetter,
            RowRecogPartitionStateRepoGroupMeta stateRepoGroupMeta,
            AgentInstanceContext agentInstanceContext,
            RowRecogNFAView view,
            bool keepScheduleState,
            RowRecogPartitionTerminationStateComparator terminationStateCompare);
    }
} // end of namespace