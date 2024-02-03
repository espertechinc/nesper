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

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationMethodSortedWrapperCollection : TransformCollection<object, ICollection<EventBean>>
    {
        public AggregationMethodSortedWrapperCollection(ICollection<object> trueCollection) : base(
            trueCollection,
            _ => _,
            AggregatorAccessSortedImpl.CheckedPayloadGetCollEvents)
        {
        }
    }
} // end of namespace