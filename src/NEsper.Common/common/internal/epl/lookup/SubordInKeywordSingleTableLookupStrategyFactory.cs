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

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordInKeywordSingleTableLookupStrategyFactory : SubordTableLookupStrategyFactory
    {
        internal readonly ExprEvaluator[] evaluators;
        internal readonly string[] expressions;
        internal readonly bool isNWOnTrigger;
        internal readonly int streamCountOuter;

        public SubordInKeywordSingleTableLookupStrategyFactory(
            bool isNWOnTrigger,
            int streamCountOuter,
            ExprEvaluator[] evaluators,
            string[] expressions)
        {
            this.isNWOnTrigger = isNWOnTrigger;
            this.streamCountOuter = streamCountOuter;
            this.evaluators = evaluators;
            this.expressions = expressions;
        }

        public SubordTableLookupStrategy MakeStrategy(
            EventTable[] eventTable,
            ExprEvaluatorContext exprEvaluatorContext,
            VirtualDWView vdw)
        {
            if (isNWOnTrigger) {
                return new SubordInKeywordSingleTableLookupStrategyNW(this, (PropertyHashedEventTable) eventTable[0]);
            }

            return new SubordInKeywordSingleTableLookupStrategy(this, (PropertyHashedEventTable) eventTable[0]);
        }

        public LookupStrategyDesc LookupStrategyDesc => new LookupStrategyDesc(
            LookupStrategyType.INKEYWORDSINGLEIDX,
            expressions);

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace