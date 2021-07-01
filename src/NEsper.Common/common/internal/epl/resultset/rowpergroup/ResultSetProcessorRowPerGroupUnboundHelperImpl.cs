///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergroup
{
    public class ResultSetProcessorRowPerGroupUnboundHelperImpl : ResultSetProcessorRowPerGroupUnboundHelper
    {
        private readonly IDictionary<object, EventBean> groupReps = new LinkedHashMap<object, EventBean>();

        public void Put(
            object key,
            EventBean @event)
        {
            groupReps.Put(key, @event);
        }

        public IEnumerator<EventBean> ValueEnumerator()
        {
            return groupReps.Values.GetEnumerator();
        }

        public void RemovedAggregationGroupKey(object key)
        {
            groupReps.Remove(key);
        }

        public void Destroy()
        {
            // no action required
        }
    }
} // end of namespace