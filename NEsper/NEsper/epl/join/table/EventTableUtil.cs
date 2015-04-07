///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.join.table
{
    public class EventTableUtil
    {
        /// <summary>
        /// Build an index/table instance using the event properties for the event type.
        /// </summary>
        /// <param name="indexedStreamNum">number of stream indexed</param>
        /// <param name="item">The item.</param>
        /// <param name="eventType">type of event to expect</param>
        /// <param name="coerceOnAddOnly">if set to <c>true</c> [coerce on add only].</param>
        /// <param name="unique">if set to <c>true</c> [unique].</param>
        /// <param name="optionalIndexName">Name of the optional index.</param>
        /// <returns>table build</returns>
        public static EventTable BuildIndex(
            int indexedStreamNum,
            QueryPlanIndexItem item,
            EventType eventType,
            bool coerceOnAddOnly,
            bool unique,
            String optionalIndexName)
        {
            IList<string> indexProps = item.IndexProps;
            IList<Type> indexCoercionTypes = Normalize(item.OptIndexCoercionTypes);
            IList<string> rangeProps = item.RangeProps;
            IList<Type> rangeCoercionTypes = Normalize(item.OptRangeCoercionTypes);

            EventTable table;
            if (rangeProps == null || rangeProps.Count == 0)
            {
                if (indexProps == null || indexProps.Count == 0)
                {
                    table = new UnindexedEventTable(indexedStreamNum);
                }
                else
                {
                    // single index key
                    if (indexProps.Count == 1)
                    {
                        if (indexCoercionTypes == null || indexCoercionTypes.Count == 0)
                        {
                            var factory = new PropertyIndexedEventTableSingleFactory(
                                indexedStreamNum, eventType, indexProps[0], unique, optionalIndexName);
                            table = factory.MakeEventTables()[0];
                        }
                        else
                        {
                            if (coerceOnAddOnly)
                            {
                                var factory = new PropertyIndexedEventTableSingleCoerceAddFactory(
                                    indexedStreamNum, eventType, indexProps[0], indexCoercionTypes[0]);
                                table = factory.MakeEventTables()[0];
                            }
                            else
                            {
                                var factory = new PropertyIndexedEventTableSingleCoerceAllFactory(
                                    indexedStreamNum, eventType, indexProps[0], indexCoercionTypes[0]);
                                table = factory.MakeEventTables()[0];
                            }
                        }
                    }
                        // Multiple index keys
                    else
                    {
                        if (indexCoercionTypes == null || indexCoercionTypes.Count == 0)
                        {
                            var factory = new PropertyIndexedEventTableFactory(
                                indexedStreamNum, eventType, indexProps, unique, optionalIndexName);
                            table = factory.MakeEventTables()[0];
                        }
                        else
                        {
                            if (coerceOnAddOnly)
                            {
                                var factory = new PropertyIndexedEventTableCoerceAddFactory(
                                    indexedStreamNum, eventType, indexProps, indexCoercionTypes);
                                table = factory.MakeEventTables()[0];
                            }
                            else
                            {
                                var factory = new PropertyIndexedEventTableCoerceAllFactory(
                                    indexedStreamNum, eventType, indexProps, indexCoercionTypes);
                                table = factory.MakeEventTables()[0];
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
                        var factory = new PropertySortedEventTableFactory(indexedStreamNum, eventType, rangeProps[0]);
                        return factory.MakeEventTables()[0];
                    }
                    else
                    {
                        var factory = new PropertySortedEventTableCoercedFactory(
                            indexedStreamNum, eventType, rangeProps[0], rangeCoercionTypes[0]);
                        return factory.MakeEventTables()[0];
                    }
                }
                else
                {
                    var factory = new PropertyCompositeEventTableFactory(
                        indexedStreamNum, eventType, indexProps, indexCoercionTypes, rangeProps, rangeCoercionTypes);
                    return factory.MakeEventTables()[0];
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
}