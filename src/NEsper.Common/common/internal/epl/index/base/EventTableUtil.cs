///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.join.queryplan;


namespace com.espertech.esper.common.@internal.epl.index.@base
{
    public class EventTableUtil
    {
        /// <summary>
        /// Build an index/table instance using the event properties for the event type.
        /// </summary>
        /// <param name="agentInstanceContext">context</param>
        /// <param name="indexedStreamNum">number of stream indexed</param>
        /// <param name="item">plan item</param>
        /// <param name="eventType">type of event to expect</param>
        /// <param name="unique">indicates unique</param>
        /// <param name="optionalIndexName">index name</param>
        /// <param name="optionalValueSerde">value serde if any</param>
        /// <param name="isFireAndForget">indicates fire-and-forget</param>
        /// <returns>table build</returns>
        public static EventTable BuildIndex(
            AgentInstanceContext agentInstanceContext,
            int indexedStreamNum,
            QueryPlanIndexItem item,
            EventType eventType,
            bool unique,
            string optionalIndexName,
            DataInputOutputSerde optionalValueSerde,
            bool isFireAndForget)
        {
            var indexProps = item.HashProps;
            var indexTypes = item.HashPropTypes;
            var indexGetter = item.HashGetter;
            var rangeProps = item.RangeProps;
            var rangeTypes = item.RangePropTypes;
            var rangeGetters = item.RangeGetters;
            var rangeKeySerdes = item.RangeKeySerdes;
            var eventTableIndexService = agentInstanceContext.StatementContext.EventTableIndexService;

            EventTable table;
            if (item.AdvancedIndexProvisionDesc != null) {
                table = eventTableIndexService.CreateCustom(
                        optionalIndexName,
                        indexedStreamNum,
                        eventType,
                        item.IsUnique,
                        item.AdvancedIndexProvisionDesc)
                    .MakeEventTables(agentInstanceContext, null)[0];
            }
            else if (rangeProps == null || rangeProps.Length == 0) {
                if (indexProps == null || indexProps.Length == 0) {
                    var factory = eventTableIndexService.CreateUnindexed(
                        indexedStreamNum,
                        eventType,
                        optionalValueSerde,
                        isFireAndForget,
                        item.StateMgmtSettings);
                    table = factory.MakeEventTables(agentInstanceContext, null)[0];
                }
                else {
                    var factory = eventTableIndexService.CreateHashedOnly(
                        indexedStreamNum,
                        eventType,
                        indexProps,
                        item.TransformFireAndForget,
                        item.HashKeySerde,
                        unique,
                        optionalIndexName,
                        indexGetter,
                        optionalValueSerde,
                        isFireAndForget,
                        item.StateMgmtSettings);
                    table = factory.MakeEventTables(agentInstanceContext, null)[0];
                }
            }
            else {
                if (rangeProps.Length == 1 && (indexProps == null || indexProps.Length == 0)) {
                    var factory = eventTableIndexService.CreateSorted(
                        indexedStreamNum,
                        eventType,
                        rangeProps[0],
                        rangeTypes[0],
                        rangeGetters[0],
                        rangeKeySerdes[0],
                        optionalValueSerde,
                        isFireAndForget,
                        item.StateMgmtSettings);
                    table = factory.MakeEventTables(agentInstanceContext, null)[0];
                }
                else {
                    var factory = eventTableIndexService.CreateComposite(
                        indexedStreamNum,
                        eventType,
                        indexProps,
                        indexTypes,
                        indexGetter,
                        item.TransformFireAndForget,
                        item.HashKeySerde,
                        rangeProps,
                        rangeTypes,
                        rangeGetters,
                        rangeKeySerdes,
                        optionalValueSerde,
                        isFireAndForget);
                    return factory.MakeEventTables(agentInstanceContext, null)[0];
                }
            }

            return table;
        }
    }
} // end of namespace