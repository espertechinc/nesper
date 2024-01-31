///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    public class OrderByProcessorUtil
    {
        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="outgoingEvents">outgoing</param>
        /// <param name="sortValuesMultiKeys">keys</param>
        /// <param name="comparator">comparator</param>
        /// <returns>sorted</returns>
        public static EventBean[] SortGivenOutgoingAndSortKeys(
            EventBean[] outgoingEvents,
            IList<object> sortValuesMultiKeys,
            IComparer<object> comparator)
        {
            // Map the sort values to the corresponding outgoing events
            var sortToOutgoing = new Dictionary<object, IList<EventBean>>()
                .WithNullKeySupport();
            var countOne = 0;
            foreach (var sortValues in sortValuesMultiKeys) {
                var list = sortToOutgoing.Get(sortValues);
                if (list == null) {
                    list = new List<EventBean>();
                }

                list.Add(outgoingEvents[countOne++]);
                sortToOutgoing.Put(sortValues, list);
            }

            // Sort the sort values
            sortValuesMultiKeys.SortInPlace(comparator);

            // Sort the outgoing events in the same order
            ISet<object> sortSet = new LinkedHashSet<object>(sortValuesMultiKeys);
            var result = new EventBean[outgoingEvents.Length];
            var countTwo = 0;
            foreach (var sortValues in sortSet) {
                ICollection<EventBean> output = sortToOutgoing.Get(sortValues);
                foreach (var theEvent in output) {
                    result[countTwo++] = theEvent;
                }
            }

            return result;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="outgoingEvents">events</param>
        /// <param name="orderKeys">keys</param>
        /// <param name="comparator">comparator</param>
        /// <returns>sorted</returns>
        public static EventBean[] SortWOrderKeys(
            EventBean[] outgoingEvents,
            object[] orderKeys,
            IComparer<object> comparator)
        {
            var sort = new OrderedListDictionary<object, object>(comparator);

            if (outgoingEvents == null || outgoingEvents.Length < 2) {
                return outgoingEvents;
            }

            for (var i = 0; i < outgoingEvents.Length; i++) {
                var entry = sort.Get(orderKeys[i]);
                if (entry == null) {
                    sort.Put(orderKeys[i], outgoingEvents[i]);
                }
                else if (entry is EventBean bean) {
                    IList<EventBean> list = new List<EventBean>();
                    list.Add(bean);
                    list.Add(outgoingEvents[i]);
                    sort.Put(orderKeys[i], list);
                }
                else {
                    var list = (IList<EventBean>)entry;
                    list.Add(outgoingEvents[i]);
                }
            }

            var result = new EventBean[outgoingEvents.Length];
            var count = 0;
            foreach (var entry in sort.Values) {
                if (entry is IList<EventBean> output) {
                    foreach (var theEvent in output) {
                        result[count++] = theEvent;
                    }
                }
                else {
                    result[count++] = (EventBean)entry;
                }
            }

            return result;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="outgoingEvents">outgoing</param>
        /// <param name="orderKeys">keys</param>
        /// <param name="comparator">comparator</param>
        /// <returns>min or max</returns>
        public static EventBean DetermineLocalMinMaxWOrderKeys(
            EventBean[] outgoingEvents,
            object[] orderKeys,
            IComparer<object> comparator)
        {
            object localMinMax = null;
            EventBean outgoingMinMaxBean = null;

            for (var i = 0; i < outgoingEvents.Length; i++) {
                var newMinMax = localMinMax == null || comparator.Compare(localMinMax, orderKeys[i]) > 0;
                if (newMinMax) {
                    localMinMax = orderKeys[i];
                    outgoingMinMaxBean = outgoingEvents[i];
                }
            }

            return outgoingMinMaxBean;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="outgoingEvents">outgoing</param>
        /// <param name="orderKeys">keys</param>
        /// <param name="comparator">comparator</param>
        /// <param name="rowLimitProcessor">row limit</param>
        /// <returns>min or max</returns>
        public static EventBean[] SortWOrderKeysWLimit(
            EventBean[] outgoingEvents,
            object[] orderKeys,
            IComparer<object> comparator,
            RowLimitProcessor rowLimitProcessor)
        {
            rowLimitProcessor.DetermineCurrentLimit();

            if (rowLimitProcessor.CurrentRowLimit == 1 &&
                rowLimitProcessor.CurrentOffset == 0 &&
                outgoingEvents != null &&
                outgoingEvents.Length > 1) {
                var minmax = DetermineLocalMinMaxWOrderKeys(outgoingEvents, orderKeys, comparator);
                return new[] { minmax };
            }

            var sorted = SortWOrderKeys(outgoingEvents, orderKeys, comparator);
            return rowLimitProcessor.ApplyLimit(sorted);
        }
    }
} // end of namespace