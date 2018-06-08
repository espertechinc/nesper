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
    /// MapIndex lookup strategy for subqueries.
    /// </summary>
    public class SubordIndexedTableLookupStrategyExprFactory : SubordTableLookupStrategyFactory
    {
        private readonly ExprEvaluator[] _evaluators;
        private readonly bool _isNWOnTrigger;
        private readonly int _numStreamsOuter;
        private readonly LookupStrategyDesc _strategyDesc;
    
        public SubordIndexedTableLookupStrategyExprFactory(bool isNWOnTrigger, int numStreamsOuter, IList<SubordPropHashKey> hashKeys)
        {
            _evaluators = new ExprEvaluator[hashKeys.Count];
            var expressions = new String[_evaluators.Length];
            for (int i = 0; i < hashKeys.Count; i++)
            {
                _evaluators[i] = hashKeys[i].HashKey.KeyExpr.ExprEvaluator;
                expressions[i] = hashKeys[i].HashKey.KeyExpr.ToExpressionStringMinPrecedenceSafe();
            }
            _isNWOnTrigger = isNWOnTrigger;
            _numStreamsOuter = numStreamsOuter;
            _strategyDesc = new LookupStrategyDesc(LookupStrategyType.MULTIEXPR, expressions);
        }
    
        public virtual SubordTableLookupStrategy MakeStrategy(EventTable[] eventTable, VirtualDWView vdw)
        {
            if (_isNWOnTrigger)
            {
                return new SubordIndexedTableLookupStrategyExprNW(_evaluators, (PropertyIndexedEventTable) eventTable[0], _strategyDesc);
            }
            else
            {
                return new SubordIndexedTableLookupStrategyExpr(_numStreamsOuter, _evaluators, (PropertyIndexedEventTable)eventTable[0], _strategyDesc);
            }
        }
    
        public String ToQueryPlan()
        {
            return GetType().FullName + " evaluators " + ExprNodeUtility.PrintEvaluators(_evaluators);
        }

        protected bool IsNWOnTrigger => _isNWOnTrigger;

        protected int NumStreamsOuter => _numStreamsOuter;

        protected LookupStrategyDesc StrategyDesc => _strategyDesc;

        protected ExprEvaluator[] Evaluators => _evaluators;
    }
}
