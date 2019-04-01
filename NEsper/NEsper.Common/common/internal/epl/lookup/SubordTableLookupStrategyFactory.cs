///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.virtualdw;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Strategy for looking up, in some sort of table or index, or a set of events, potentially based on the
    ///     events properties, and returning a set of matched events.
    /// </summary>
    public interface SubordTableLookupStrategyFactory
    {
        LookupStrategyDesc LookupStrategyDesc { get; }

        SubordTableLookupStrategy MakeStrategy(
            EventTable[] eventTable, AgentInstanceContext agentInstanceContext, VirtualDWView vdw);
    }
} // end of namespace