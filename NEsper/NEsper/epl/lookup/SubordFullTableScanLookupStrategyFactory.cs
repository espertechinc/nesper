///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Factory for lookup on an unindexed table returning the full table as matching events.
    /// </summary>
    public class SubordFullTableScanLookupStrategyFactory : SubordTableLookupStrategyFactory
    {
        public SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw)
        {
            return new SubordFullTableScanLookupStrategy((UnindexedEventTable)eventTable[0]);
        }

        public String ToQueryPlan()
        {
            return GetType().Name;
        }
    }
}
