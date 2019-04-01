///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergroup
{
    public interface ResultSetProcessorRowPerGroupUnboundHelper : AggregationRowRemovedCallback
    {
        void Put(object key, EventBean @event);

        //void RemovedAggregationGroupKey(object key);

        IEnumerator<EventBean> ValueEnumerator();

        void Destroy();
    }
} // end of namespace