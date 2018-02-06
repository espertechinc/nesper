///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Index lookup strategy for subqueries.
    /// </summary>
    public class SubordInKeywordSingleTableLookupStrategyFactory : SubordTableLookupStrategyFactory
    {
        protected readonly ExprEvaluator[] Evaluators;
        protected bool IsNWOnTrigger;
        protected int StreamCountOuter;
        protected readonly LookupStrategyDesc StrategyDesc;
    
        public SubordInKeywordSingleTableLookupStrategyFactory(
            bool isNWOnTrigger, int streamCountOuter, IList<ExprNode> exprNodes)
        {
            StreamCountOuter = streamCountOuter;
            Evaluators = ExprNodeUtility.GetEvaluators(exprNodes);
            IsNWOnTrigger = isNWOnTrigger;
            StrategyDesc = new LookupStrategyDesc(
                LookupStrategyType.INKEYWORDSINGLEIDX, ExprNodeUtility.ToExpressionStringsMinPrecedence(exprNodes));
        }
    
        public SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw)
        {
            if (IsNWOnTrigger)
            {
                return new SubordInKeywordSingleTableLookupStrategyNW(Evaluators, (PropertyIndexedEventTableSingle) eventTable[0], StrategyDesc);
            }
            else
            {
                return new SubordInKeywordSingleTableLookupStrategy(StreamCountOuter, Evaluators, (PropertyIndexedEventTableSingle) eventTable[0], StrategyDesc);
            }
        }
    
        public String ToQueryPlan()
        {
            return GetType().Name;
        }
    }
}
