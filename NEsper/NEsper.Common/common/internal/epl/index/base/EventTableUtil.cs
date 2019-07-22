///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.@join.queryplan;

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    public class EventTableUtil
    {
        /// <summary>
        ///     Build an index/table instance using the event properties for the event type.
        /// </summary>
        /// <param name="indexedStreamNum">number of stream indexed</param>
        /// <param name="eventType">type of event to expect</param>
        /// <param name="optionalIndexName">index name</param>
        /// <param name="agentInstanceContext">context</param>
        /// <param name="item">plan item</param>
        /// <param name="optionalSerde">serde if any</param>
        /// <param name="isFireAndForget">indicates fire-and-forget</param>
        /// <param name="unique">indicates unique</param>
        /// <param name="coerceOnAddOnly">indicator whether to coerce on value-add</param>
        /// <returns>table build</returns>
        public static EventTable BuildIndex(
            AgentInstanceContext agentInstanceContext,
            int indexedStreamNum,
            QueryPlanIndexItem item,
            EventType eventType,
            bool coerceOnAddOnly,
            bool unique,
            string optionalIndexName,
            object optionalSerde,
            bool isFireAndForget)
        {
            var indexProps = item.HashProps;
            var indexTypes = item.HashPropTypes;
            var indexGetter = item.HashGetter;
            var rangeProps = item.RangeProps;
            var rangeTypes = item.RangePropTypes;
            var rangeGetters = item.RangeGetters;
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
                        optionalSerde,
                        isFireAndForget,
                        agentInstanceContext.StatementContext);
                    table = factory.MakeEventTables(agentInstanceContext, null)[0];
                }
                else {
                    var factory = eventTableIndexService.CreateHashedOnly(
                        indexedStreamNum,
                        eventType,
                        indexProps,
                        indexTypes,
                        unique,
                        optionalIndexName,
                        indexGetter,
                        optionalSerde,
                        isFireAndForget,
                        agentInstanceContext.StatementContext);
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
                        optionalSerde,
                        isFireAndForget,
                        agentInstanceContext.StatementContext);
                    table = factory.MakeEventTables(agentInstanceContext, null)[0];
                }
                else {
                    var factory = eventTableIndexService.CreateComposite(
                        indexedStreamNum,
                        eventType,
                        indexProps,
                        indexTypes,
                        indexGetter,
                        rangeProps,
                        rangeTypes,
                        rangeGetters,
                        optionalSerde,
                        isFireAndForget);
                    return factory.MakeEventTables(agentInstanceContext, null)[0];
                }
            }

            return table;
        }
    }
} // end of namespace