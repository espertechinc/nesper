///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    /// <summary>
    ///     For handling access aggregation functions "first, last, window" a pair of slow and accessorForge.
    /// </summary>
    public class AggregationAccessorSlotPairForge
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="slot">number of accessorForge</param>
        /// <param name="accessorForge">accessorForge</param>
        public AggregationAccessorSlotPairForge(
            int slot,
            AggregationAccessorForge accessorForge)
        {
            Slot = slot;
            AccessorForge = accessorForge;
        }

        /// <summary>
        ///     Returns the slot.
        /// </summary>
        /// <value>slow</value>
        public int Slot { get; }

        /// <summary>
        ///     Returns the accessorForge.
        /// </summary>
        /// <value>accessorForge</value>
        public AggregationAccessorForge AccessorForge { get; }
    }
} // end of namespace