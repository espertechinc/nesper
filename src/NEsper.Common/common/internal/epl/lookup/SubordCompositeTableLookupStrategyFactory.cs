///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.composite;
using com.espertech.esper.common.@internal.epl.join.exec.composite;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordCompositeTableLookupStrategyFactory : SubordTableLookupStrategyFactory
    {
        private readonly string[] _expressions;

        internal readonly CompositeIndexQuery InnerIndexQuery;

        public SubordCompositeTableLookupStrategyFactory(
            bool isNWOnTrigger,
            int numStreams,
            string[] expressions,
            ExprEvaluator hashEval,
            QueryGraphValueEntryRange[] rangeEvals)
        {
            _expressions = expressions;
            InnerIndexQuery = CompositeIndexQueryFactory.MakeSubordinate(
                isNWOnTrigger,
                numStreams,
                hashEval,
                rangeEvals);
        }

        public SubordTableLookupStrategy MakeStrategy(
            EventTable[] eventTable,
            AgentInstanceContext agentInstanceContext,
            VirtualDWView vdw)
        {
            return new SubordCompositeTableLookupStrategy(this, (PropertyCompositeEventTable) eventTable[0]);
        }

        public LookupStrategyDesc LookupStrategyDesc =>
            new LookupStrategyDesc(LookupStrategyType.COMPOSITE, _expressions);

        public string ToQueryPlan()
        {
            return GetType().Name + " ranges=" + Arrays.AsList(_expressions);
        }
    }
} // end of namespace