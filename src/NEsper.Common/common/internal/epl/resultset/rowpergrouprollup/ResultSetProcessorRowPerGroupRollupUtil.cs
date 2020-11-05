///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.epl.resultset.order;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
    public class ResultSetProcessorRowPerGroupRollupUtil
    {
        public const string METHOD_MAKEGROUPREPSPERLEVELBUF = "MakeGroupRepsPerLevelBuf";
        public const string METHOD_MAKERSTREAMSORTEDARRAYBUF = "MakeRStreamSortedArrayBuf";
        public const string METHOD_GETOLDEVENTSSORTKEYS = "GetOldEventsSortKeys";

        public static EventsAndSortKeysPair GetOldEventsSortKeys(
            int oldEventCount,
            EventArrayAndSortKeyArray rstreamEventSortArrayBuf,
            OrderByProcessor orderByProcessor,
            AggregationGroupByRollupDesc rollupDesc)
        {
            var oldEventsArr = new EventBean[oldEventCount];
            object[] oldEventsSortKeys = null;
            if (orderByProcessor != null) {
                oldEventsSortKeys = new object[oldEventCount];
            }

            var countEvents = 0;
            var countSortKeys = 0;
            foreach (var level in rollupDesc.Levels) {
                var events = rstreamEventSortArrayBuf.EventsPerLevel[level.LevelNumber];
                foreach (var @event in events) {
                    oldEventsArr[countEvents++] = @event;
                }

                if (orderByProcessor != null) {
                    var sortKeys = rstreamEventSortArrayBuf.SortKeyPerLevel[level.LevelNumber];
                    foreach (var sortKey in sortKeys) {
                        oldEventsSortKeys[countSortKeys++] = sortKey;
                    }
                }
            }

            return new EventsAndSortKeysPair(oldEventsArr, oldEventsSortKeys);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="length">num-levels</param>
        /// <param name="isSorting">sorting flag</param>
        /// <returns>buffer</returns>
        public static EventArrayAndSortKeyArray MakeRStreamSortedArrayBuf(
            int length,
            bool isSorting)
        {
            var eventsPerLevel = new IList<EventBean>[length];
            IList<object>[] sortKeyPerLevel = null;
            if (isSorting) {
                sortKeyPerLevel = new IList<object>[length];
            }

            for (var i = 0; i < length; i++) {
                eventsPerLevel[i] = new List<EventBean>();
                if (isSorting) {
                    sortKeyPerLevel[i] = new List<object>();
                }
            }

            return new EventArrayAndSortKeyArray(eventsPerLevel, sortKeyPerLevel);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="levelCount">num-levels</param>
        /// <returns>buffer</returns>
        public static IDictionary<object, EventBean[]>[] MakeGroupRepsPerLevelBuf(int levelCount)
        {
            IDictionary<object, EventBean[]>[] groupRepsPerLevelBuf =
                new LinkedHashMap<object, EventBean[]>[levelCount];
            for (var i = 0; i < levelCount; i++) {
                groupRepsPerLevelBuf[i] = new LinkedHashMap<object, EventBean[]>();
            }

            return groupRepsPerLevelBuf;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="resultSetProcessorHelperFactory">helper factory</param>
        /// <param name="agentInstanceContext">context</param>
        /// <param name="groupKeyTypes">types</param>
        /// <param name="groupByRollupDesc">rollup into</param>
        /// <param name="outputConditionPolledFactory">condition factory</param>
        /// <returns>helpers</returns>
        public static ResultSetProcessorGroupedOutputFirstHelper[] InitializeOutputFirstHelpers(
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
            AgentInstanceContext agentInstanceContext,
            Type[] groupKeyTypes,
            AggregationGroupByRollupDesc groupByRollupDesc,
            OutputConditionPolledFactory outputConditionPolledFactory)
        {
            var outputFirstHelpers = new ResultSetProcessorGroupedOutputFirstHelper[groupByRollupDesc.Levels.Length];
            for (var i = 0; i < groupByRollupDesc.Levels.Length; i++) {
                outputFirstHelpers[i] = resultSetProcessorHelperFactory.MakeRSGroupedOutputFirst(
                    agentInstanceContext,
                    groupKeyTypes,
                    outputConditionPolledFactory,
                    groupByRollupDesc,
                    i,
                    null);
            }

            return outputFirstHelpers;
        }
    }
} // end of namespace