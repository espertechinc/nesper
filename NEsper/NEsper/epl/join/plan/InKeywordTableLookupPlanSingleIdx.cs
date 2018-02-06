///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Plan to perform an indexed table lookup.
    /// </summary>
    public class InKeywordTableLookupPlanSingleIdx : TableLookupPlan
    {
        private readonly IList<ExprNode> _expressions;

        /// <summary>Ctor. </summary>
        /// <param name="lookupStream">stream that generates event to look up for</param>
        /// <param name="indexedStream">stream to index table lookup</param>
        /// <param name="indexNum">index number for the table containing the full unindexed contents</param>
        /// <param name="expressions"></param>
        public InKeywordTableLookupPlanSingleIdx(
            int lookupStream,
            int indexedStream, 
            TableLookupIndexReqKey indexNum,
            IList<ExprNode> expressions)
            : base(lookupStream, indexedStream, new TableLookupIndexReqKey[] { indexNum })
        {
            _expressions = expressions;
        }

        public IList<ExprNode> Expressions => _expressions;

        public override TableLookupKeyDesc KeyDescriptor => new TableLookupKeyDesc(
            Collections.GetEmptyList<QueryGraphValueEntryHashKeyed>(),
            Collections.GetEmptyList<QueryGraphValueEntryRange>());

        public override JoinExecTableLookupStrategy MakeStrategyInternal(EventTable[] eventTable, EventType[] eventTypes)
        {
            var single = (PropertyIndexedEventTableSingle) eventTable[0];
            var evaluators = new ExprEvaluator[_expressions.Count];
            for (var i = 0; i < _expressions.Count; i++) {
                evaluators[i] = _expressions[i].ExprEvaluator;
            }
            return new InKeywordSingleTableLookupStrategyExpr(
                evaluators, LookupStream, single, new LookupStrategyDesc(
                    LookupStrategyType.INKEYWORDSINGLEIDX, ExprNodeUtility.ToExpressionStringsMinPrecedence(_expressions)));
        }
    
        public override String ToString()
        {
            return GetType().Name + " " +
                    base.ToString() +
                   " keyProperties=" + ExprNodeUtility.ToExpressionStringMinPrecedenceAsList(_expressions);
        }
    }
}
