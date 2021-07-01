///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationMethodSortedWrapperValueEnumerator
    {
        public static IEnumerator<ICollection<EventBean>> For(IEnumerator<object> enumerator)
        {
            while (enumerator.MoveNext()) {
                yield return AggregatorAccessSortedImpl.CheckedPayloadGetCollEvents(enumerator.Current);
            }
        }
    }
} // end of namespace