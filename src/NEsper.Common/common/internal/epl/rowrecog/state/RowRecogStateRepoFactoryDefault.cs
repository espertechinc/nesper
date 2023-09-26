///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.rowrecog.core;

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    public class RowRecogStateRepoFactoryDefault : RowRecogStateRepoFactory
    {
        public static readonly RowRecogStateRepoFactoryDefault INSTANCE = new RowRecogStateRepoFactoryDefault();

        private RowRecogStateRepoFactoryDefault()
        {
        }

        public RowRecogPartitionStateRepo MakeSingle(
            RowRecogPreviousStrategyImpl prevGetter,
            AgentInstanceContext agentInstanceContext,
            RowRecogNFAView view,
            bool keepScheduleState,
            RowRecogPartitionTerminationStateComparator terminationStateCompare,
            StateMgmtSetting unpartitionedStateMgmtSettings,
            StateMgmtSetting scheduleMgmtStateMgmtSettings)
        {
            return new RowRecogPartitionStateRepoNoGroup(prevGetter, keepScheduleState, terminationStateCompare);
        }

        public RowRecogPartitionStateRepo MakePartitioned(
            RowRecogPreviousStrategyImpl prevGetter,
            RowRecogPartitionStateRepoGroupMeta stateRepoGroupMeta,
            AgentInstanceContext agentInstanceContext,
            RowRecogNFAView view,
            bool keepScheduleState,
            RowRecogPartitionTerminationStateComparator terminationStateCompare,
            StateMgmtSetting partitionMgmtStateMgmtSettings,
            StateMgmtSetting scheduleMgmtStateMgmtSettings)
        {
            return new RowRecogPartitionStateRepoGroup(
                prevGetter,
                stateRepoGroupMeta,
                keepScheduleState,
                terminationStateCompare);
        }
    }
} // end of namespace