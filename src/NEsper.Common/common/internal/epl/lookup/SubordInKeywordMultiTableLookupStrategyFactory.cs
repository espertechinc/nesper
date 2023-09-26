///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.virtualdw;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordInKeywordMultiTableLookupStrategyFactory : SubordTableLookupStrategyFactory
    {
        internal readonly ExprEvaluator evaluator;
        internal readonly string expression;
        internal readonly bool isNWOnTrigger;
        internal readonly int streamCountOuter;

        public SubordInKeywordMultiTableLookupStrategyFactory(
            bool isNWOnTrigger,
            int streamCountOuter,
            ExprEvaluator evaluator,
            string expression)
        {
            this.isNWOnTrigger = isNWOnTrigger;
            this.streamCountOuter = streamCountOuter;
            this.evaluator = evaluator;
            this.expression = expression;
        }

        public SubordTableLookupStrategy MakeStrategy(
            EventTable[] eventTable,
            ExprEvaluatorContext exprEvaluatorContext,
            VirtualDWView vdw)
        {
            var indexes = new PropertyHashedEventTable[eventTable.Length];
            for (var i = 0; i < eventTable.Length; i++) {
                indexes[i] = (PropertyHashedEventTable)eventTable[i];
            }

            if (isNWOnTrigger) {
                return new SubordInKeywordMultiTableLookupStrategyNW(this, indexes);
            }

            return new SubordInKeywordMultiTableLookupStrategy(this, indexes);
        }

        public LookupStrategyDesc LookupStrategyDesc => new LookupStrategyDesc(
            LookupStrategyType.INKEYWORDMULTIIDX,
            new[] { expression });

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace