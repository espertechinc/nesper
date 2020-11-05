///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.lookup;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    /// <summary>
    ///     Strategy for looking up, in some sort of table or index, or a set of events, potentially based on the
    ///     events properties, and returning a set of matched events.
    /// </summary>
    public class SubordTableLookupStrategyFactoryVDW : SubordTableLookupStrategyFactory
    {
        public IndexedPropDesc[] IndexHashedProps { get; set; }

        public IndexedPropDesc[] IndexBtreeProps { get; set; }

        public bool IsNwOnTrigger { get; set; }

        public ExprEvaluator[] HashEvals { get; set; }

        public Type[] HashCoercionTypes { get; set; }

        public QueryGraphValueEntryRange[] RangeEvals { get; set; }

        public Type[] RangeCoercionTypes { get; set; }

        public int NumOuterStreams { get; set; }

        public SubordTableLookupStrategy MakeStrategy(
            EventTable[] eventTable,
            AgentInstanceContext agentInstanceContext,
            VirtualDWView vdw)
        {
            return vdw.GetSubordinateLookupStrategy(this, agentInstanceContext);
        }

        public LookupStrategyDesc LookupStrategyDesc => new LookupStrategyDesc(LookupStrategyType.VDW);
    }
} // end of namespace