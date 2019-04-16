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
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordHashedTableLookupStrategyPropFactory : SubordTableLookupStrategyFactory
    {
        internal readonly ExprEvaluator Evaluator;

        public SubordHashedTableLookupStrategyPropFactory(
            string[] properties,
            int[] keyStreamNums,
            ExprEvaluator evaluator)
        {
            Properties = properties;
            KeyStreamNums = keyStreamNums;
            Evaluator = evaluator;
        }

        /// <summary>
        ///     Returns properties to use from lookup event to look up in index.
        /// </summary>
        /// <returns>properties to use from lookup event</returns>
        public string[] Properties { get; }

        public int[] KeyStreamNums { get; }

        public SubordTableLookupStrategy MakeStrategy(
            EventTable[] eventTable,
            AgentInstanceContext agentInstanceContext,
            VirtualDWView vdw)
        {
            return new SubordHashedTableLookupStrategyProp(this, (PropertyHashedEventTable) eventTable[0]);
        }

        public LookupStrategyDesc LookupStrategyDesc => new LookupStrategyDesc(
            Properties.Length == 1 ? LookupStrategyType.SINGLEPROP : LookupStrategyType.MULTIPROP, Properties);

        public string ToQueryPlan()
        {
            return GetType().Name +
                   " indexProps=" + CompatExtensions.RenderAny(Properties) +
                   " keyStreamNums=" + CompatExtensions.RenderAny(KeyStreamNums);
        }
    }
} // end of namespace