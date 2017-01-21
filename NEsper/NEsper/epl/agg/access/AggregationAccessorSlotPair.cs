///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.agg.access
{
    /// <summary>
    /// For handling access aggregation functions "first, last, window" a pair of slow and accessor.
    /// </summary>
    public class AggregationAccessorSlotPair
    {
        /// <summary>Ctor. </summary>
        /// <param name="slot">number of accessor</param>
        /// <param name="accessor">accessor</param>
        public AggregationAccessorSlotPair(int slot, AggregationAccessor accessor)
        {
            Slot = slot;
            Accessor = accessor;
        }

        /// <summary>Returns the slot. </summary>
        /// <value>slow</value>
        public int Slot { get; private set; }

        /// <summary>Returns the accessor. </summary>
        /// <value>accessor</value>
        public AggregationAccessor Accessor { get; private set; }
    }
}
