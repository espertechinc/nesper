///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.collection;

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>Implementation of access function for single-stream (not joins). </summary>
    public class AggregationStateSortedJoin : AggregationStateSortedImpl
    {
        protected readonly RefCountedSetAtomicInteger<EventBean> Refs;
    
        public AggregationStateSortedJoin(AggregationStateSortedSpec spec) : base(spec)
        {
            Refs = new RefCountedSetAtomicInteger<EventBean>();
        }
    
        protected override bool ReferenceEvent(EventBean theEvent)
        {
            return Refs.Add(theEvent);
        }
    
        protected override bool DereferenceEvent(EventBean theEvent)
        {
            return Refs.Remove(theEvent);
        }
    }
}
