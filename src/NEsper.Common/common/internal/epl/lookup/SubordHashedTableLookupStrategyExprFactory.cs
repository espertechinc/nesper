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
    public class SubordHashedTableLookupStrategyExprFactory : SubordTableLookupStrategyFactory
    {
        internal readonly ExprEvaluator Evaluator;
        internal readonly string[] ExpressionTexts;
        internal readonly bool IsNWOnTrigger;
        internal readonly int NumStreamsOuter;

        public SubordHashedTableLookupStrategyExprFactory(
            string[] expressionTexts,
            ExprEvaluator evaluator,
            bool isNWOnTrigger,
            int numStreamsOuter)
        {
            ExpressionTexts = expressionTexts;
            Evaluator = evaluator;
            IsNWOnTrigger = isNWOnTrigger;
            NumStreamsOuter = numStreamsOuter;
        }

        public SubordTableLookupStrategy MakeStrategy(
            EventTable[] eventTable,
            ExprEvaluatorContext exprEvaluatorContext,
            VirtualDWView vdw)
        {
            if (IsNWOnTrigger) {
                return new SubordHashedTableLookupStrategyExprNW(this, (PropertyHashedEventTable)eventTable[0]);
            }

            return new SubordHashedTableLookupStrategyExpr(this, (PropertyHashedEventTable)eventTable[0]);
        }

        public LookupStrategyDesc LookupStrategyDesc => new LookupStrategyDesc(
            ExpressionTexts.Length == 1 ? LookupStrategyType.SINGLEEXPR : LookupStrategyType.MULTIEXPR,
            ExpressionTexts);

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace