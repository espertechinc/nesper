///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.agg.aggregator
{
    public class AggregatorRateEverFilter : AggregatorRateEver
    {
        public AggregatorRateEverFilter(long interval, long oneSecondTime, TimeProvider timeProvider)
            : base(interval, oneSecondTime, timeProvider)
        {
        }

        public override void Enter(object @object)
        {
            if (true.Equals(@object)) {
                base.Enter(@object);
            }
        }
    }
} // end of namespace