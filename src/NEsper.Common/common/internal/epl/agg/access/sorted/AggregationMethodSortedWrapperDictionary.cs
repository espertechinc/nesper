///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

using static
    com.espertech.esper.common.@internal.epl.agg.access.sorted.
    AggregatorAccessSortedImpl; //checkedPayloadGetCollEvents;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationMethodSortedWrapperDictionary
        : TransformOrderedDictionary<object, ICollection<EventBean>, object, object>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sorted"></param>
        internal AggregationMethodSortedWrapperDictionary(IOrderedDictionary<object, object> sorted) : base(
            sorted,
            _ => _,
            _ => _,
            CheckedPayloadGetCollEvents,
            _ => _)
        {
        }
    }
} // end of namespace