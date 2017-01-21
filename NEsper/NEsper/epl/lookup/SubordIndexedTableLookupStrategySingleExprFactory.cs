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
    /// MapIndex lookup strategy for subqueries.
    /// </summary>
    public class SubordIndexedTableLookupStrategySingleExprFactory : SubordTableLookupStrategyFactory
    {
        protected readonly ExprEvaluator Evaluator;
        protected bool IsNWOnTrigger;
        protected int StreamCountOuter;
        protected readonly LookupStrategyDesc StrategyDesc;
    
        public SubordIndexedTableLookupStrategySingleExprFactory(bool isNWOnTrigger, int streamCountOuter, SubordPropHashKey hashKey)
        {
            StreamCountOuter = streamCountOuter;
            Evaluator = hashKey.HashKey.KeyExpr.ExprEvaluator;
            IsNWOnTrigger = isNWOnTrigger;
            StrategyDesc = new LookupStrategyDesc(LookupStrategyType.SINGLEEXPR, new String[] { hashKey.HashKey.KeyExpr.ToExpressionStringMinPrecedenceSafe() });
        }

        public virtual SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw)
        {
            if (IsNWOnTrigger)
            {
                return new SubordIndexedTableLookupStrategySingleExprNW(
                    Evaluator, (PropertyIndexedEventTableSingle) eventTable[0], StrategyDesc);
            }
            else
            {
                return new SubordIndexedTableLookupStrategySingleExpr(
                    StreamCountOuter, Evaluator, (PropertyIndexedEventTableSingle) eventTable[0], StrategyDesc);
            }
        }

        public String ToQueryPlan()
        {
            return GetType().FullName + " evaluator " + Evaluator.GetType().Name;
        }
    }
}
