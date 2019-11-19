///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.sorted;
using com.espertech.esper.common.@internal.epl.@join.exec.sorted;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.virtualdw;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordSortedTableLookupStrategyFactory : SubordTableLookupStrategyFactory
    {
        private readonly string _expression;
        internal readonly SortedAccessStrategy strategy;

        public SubordSortedTableLookupStrategyFactory(
            bool isNWOnTrigger,
            int numStreams,
            string expression,
            QueryGraphValueEntryRange range)
        {
            this._expression = expression;
            strategy = SortedAccessStrategyFactory.Make(isNWOnTrigger, -1, numStreams, range);
        }

        public LookupStrategyDesc LookupStrategyDesc =>
            new LookupStrategyDesc(LookupStrategyType.RANGE, new[] {_expression});

        public SubordTableLookupStrategy MakeStrategy(
            EventTable[] eventTable,
            AgentInstanceContext agentInstanceContext,
            VirtualDWView vdw)
        {
            return new SubordSortedTableLookupStrategy(this, (PropertySortedEventTable) eventTable[0]);
        }
    }
} // end of namespace