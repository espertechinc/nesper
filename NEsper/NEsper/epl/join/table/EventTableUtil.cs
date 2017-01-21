///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.table
{
    public class EventTableUtil
    {
        /// <summary>
        /// Build an index/table instance using the event properties for the event type.
        /// </summary>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="indexedStreamNum">number of stream indexed</param>
        /// <param name="item">The item.</param>
        /// <param name="eventType">type of event to expect</param>
        /// <param name="coerceOnAddOnly">if set to <c>true</c> [coerce on add only].</param>
        /// <param name="unique">if set to <c>true</c> [unique].</param>
        /// <param name="optionalIndexName">Name of the optional index.</param>
        /// <param name="optionalSerde">The optional serde.</param>
        /// <param name="isFireAndForget">if set to <c>true</c> [is fire and forget].</param>
        /// <returns>
        /// table build
        /// </returns>
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
            var indexProps = item.IndexProps;
            var indexCoercionTypes = Normalize(item.OptIndexCoercionTypes);
            var rangeProps = item.RangeProps;
            var rangeCoercionTypes = Normalize(item.OptRangeCoercionTypes);
            var ident = new EventTableFactoryTableIdentAgentInstance(agentInstanceContext);
            var eventTableIndexService = agentInstanceContext.StatementContext.EventTableIndexService;

            EventTable table;
            if (rangeProps == null || rangeProps.Count == 0)
            {
                if (indexProps == null || indexProps.Count == 0)
                {
                    var factory = eventTableIndexService.CreateUnindexed(
                        indexedStreamNum, optionalSerde, isFireAndForget);
                    table = factory.MakeEventTables(ident)[0];
                }
                else
                {
                    // single index key
                    if (indexProps.Count == 1)
                    {
                        if (indexCoercionTypes == null || indexCoercionTypes.Count == 0)
                        {
                            var factory = eventTableIndexService.CreateSingle(
                                indexedStreamNum, eventType, indexProps[0], unique, optionalIndexName, optionalSerde,
                                isFireAndForget);
                            table = factory.MakeEventTables(ident)[0];
                        }
                        else
                        {
                            if (coerceOnAddOnly)
                            {
                                var factory = eventTableIndexService.CreateSingleCoerceAdd(
                                    indexedStreamNum, eventType, indexProps[0], indexCoercionTypes[0], optionalSerde,
                                    isFireAndForget);
                                table = factory.MakeEventTables(ident)[0];
                            }
                            else
                            {
                                var factory = eventTableIndexService.CreateSingleCoerceAll(
                                    indexedStreamNum, eventType, indexProps[0], indexCoercionTypes[0], optionalSerde,
                                    isFireAndForget);
                                table = factory.MakeEventTables(ident)[0];
                            }
                        }
                    }
                        // Multiple index keys
                    else
                    {
                        if (indexCoercionTypes == null || indexCoercionTypes.Count == 0)
                        {
                            var factory = eventTableIndexService.CreateMultiKey(
                                indexedStreamNum, eventType, indexProps, unique, optionalIndexName, optionalSerde,
                                isFireAndForget);
                            table = factory.MakeEventTables(ident)[0];
                        }
                        else
                        {
                            if (coerceOnAddOnly)
                            {
                                var factory = eventTableIndexService.CreateMultiKeyCoerceAdd(
                                    indexedStreamNum, eventType, indexProps, indexCoercionTypes, isFireAndForget);
                                table = factory.MakeEventTables(ident)[0];
                            }
                            else
                            {
                                var factory = eventTableIndexService.CreateMultiKeyCoerceAll(
                                    indexedStreamNum, eventType, indexProps, indexCoercionTypes, isFireAndForget);
                                table = factory.MakeEventTables(ident)[0];
                            }
                        }
                    }
                }
            }
            else
            {
                if ((rangeProps.Count == 1) && (indexProps == null || indexProps.Count == 0))
                {
                    if (rangeCoercionTypes == null)
                    {
                        var factory = eventTableIndexService.CreateSorted(
                            indexedStreamNum, eventType, rangeProps[0], isFireAndForget);
                        return factory.MakeEventTables(ident)[0];
                    }
                    else
                    {
                        var factory = eventTableIndexService.CreateSortedCoerce(
                            indexedStreamNum, eventType, rangeProps[0], rangeCoercionTypes[0], isFireAndForget);
                        return factory.MakeEventTables(ident)[0];
                    }
                }
                else
                {
                    var factory = eventTableIndexService.CreateComposite(
                        indexedStreamNum, eventType, indexProps, indexCoercionTypes, rangeProps, rangeCoercionTypes,
                        isFireAndForget);
                    return factory.MakeEventTables(ident)[0];
                }
            }
            return table;
        }

        private static IList<Type> Normalize(IList<Type> types)
        {
            if (types == null)
            {
                return null;
            }
            if (CollectionUtil.IsAllNullArray(types))
            {
                return null;
            }
            return types;
        }
    }
} // end of namespace
