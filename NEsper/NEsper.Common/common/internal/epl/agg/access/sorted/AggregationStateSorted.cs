///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.agg.access.sorted.AggregatorAccessSortedImpl;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationStateSorted
    {
        public int Count { get; set; }

        public OrderedDictionary<object, object> Sorted { get; set; }

        public EventBean FirstValue {
            get {
                if (Sorted.IsEmpty()) {
                    return null;
                }

                var max = Sorted.First();
                return CheckedPayloadMayDeque(max.Value);
            }
        }

        public EventBean LastValue {
            get {
                if (Sorted.IsEmpty()) {
                    return null;
                }

                var min = Sorted.Last();
                return CheckedPayloadMayDeque(min.Value);
            }
        }

        public ICollection<EventBean> CollectionReadOnly()
        {
            return new AggregationStateSortedWrappingCollection(Sorted, Count);
        }
    }
} // end of namespace