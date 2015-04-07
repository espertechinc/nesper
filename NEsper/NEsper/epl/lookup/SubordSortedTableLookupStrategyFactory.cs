///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.exec.sorted;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// MapIndex lookup strategy for subqueries.
    /// </summary>
    public class SubordSortedTableLookupStrategyFactory : SubordTableLookupStrategyFactory
    {
        private readonly SubordPropRangeKey _rangeKey;
    
        private readonly SortedAccessStrategy _strategy;
    
        private readonly LookupStrategyDesc _strategyDesc;
    
        public SubordSortedTableLookupStrategyFactory(bool isNWOnTrigger, int numStreams, SubordPropRangeKey rangeKey)
        {
            _rangeKey = rangeKey;
            _strategy = SortedAccessStrategyFactory.Make(isNWOnTrigger, -1, numStreams, rangeKey);
            _strategyDesc = new LookupStrategyDesc(LookupStrategyType.RANGE, ExprNodeUtility.ToExpressionStringsMinPrecedence(rangeKey.RangeInfo.Expressions));
        }
    
        public SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw) {
            return new SubordSortedTableLookupStrategy(_strategy, (PropertySortedEventTable) eventTable[0], _strategyDesc);
        }
    
        public String ToQueryPlan() {
            return GetType().Name + " range " + _rangeKey.ToQueryPlan();
        }
    }
}
