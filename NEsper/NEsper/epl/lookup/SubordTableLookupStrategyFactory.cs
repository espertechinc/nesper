///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Strategy for looking up, in some sort of table or index, or a set of events, potentially based on the
    /// events properties, and returning a set of matched events.
    /// </summary>
    public interface SubordTableLookupStrategyFactory
    {
        SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw);
        String ToQueryPlan();
    }
}