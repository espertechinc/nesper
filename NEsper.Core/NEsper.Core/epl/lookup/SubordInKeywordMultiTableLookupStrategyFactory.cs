///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Index lookup strategy for subqueries.
    /// </summary>
    public class SubordInKeywordMultiTableLookupStrategyFactory : SubordTableLookupStrategyFactory
    {
        protected readonly ExprEvaluator Evaluator;
        protected bool IsNWOnTrigger;
        protected int StreamCountOuter;
        protected readonly LookupStrategyDesc StrategyDesc;
    
        public SubordInKeywordMultiTableLookupStrategyFactory(bool isNWOnTrigger, int streamCountOuter, ExprNode exprNode)
        {
            StreamCountOuter = streamCountOuter;
            Evaluator = exprNode.ExprEvaluator;
            IsNWOnTrigger = isNWOnTrigger;
            StrategyDesc = new LookupStrategyDesc(LookupStrategyType.INKEYWORDMULTIIDX, new String[] {ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(exprNode)});
        }
    
        public SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw)
        {
            if (IsNWOnTrigger)
            {
                return new SubordInKeywordMultiTableLookupStrategyNW(Evaluator, eventTable, StrategyDesc);
            }
            else
            {
                return new SubordInKeywordMultiTableLookupStrategy(StreamCountOuter, Evaluator, eventTable, StrategyDesc);
            }
        }
    
        public String ToQueryPlan()
        {
            return this.GetType().Name;
        }
    }
}
