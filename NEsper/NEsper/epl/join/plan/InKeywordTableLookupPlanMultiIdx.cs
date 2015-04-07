///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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
    public class InKeywordTableLookupPlanMultiIdx : TableLookupPlan
    {
        private readonly ExprNode _keyExpr;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="lookupStream">stream that generates event to look up for</param>
        /// <param name="indexedStream">stream to index table lookup</param>
        /// <param name="indexNum">index number for the table containing the full unindexed contents</param>
        /// <param name="keyExpr">The key expr.</param>
        public InKeywordTableLookupPlanMultiIdx(int lookupStream, int indexedStream, TableLookupIndexReqKey[] indexNum, ExprNode keyExpr)
            : base(lookupStream, indexedStream, indexNum)
        {
            _keyExpr = keyExpr;
        }

        public ExprNode KeyExpr
        {
            get { return _keyExpr; }
        }

        public override TableLookupKeyDesc KeyDescriptor
        {
            get
            {
                return new TableLookupKeyDesc(
                    Collections.GetEmptyList<QueryGraphValueEntryHashKeyed>(),
                    Collections.GetEmptyList<QueryGraphValueEntryRange>());
            }
        }

        public override JoinExecTableLookupStrategy MakeStrategyInternal(EventTable[] eventTable, EventType[] eventTypes)
        {
            var evaluator = _keyExpr.ExprEvaluator;
            var singles = new PropertyIndexedEventTableSingle[eventTable.Length];
            for (int i = 0; i < eventTable.Length; i++) {
                singles[i] = (PropertyIndexedEventTableSingle) eventTable[i];
            }
            return new InKeywordMultiTableLookupStrategyExpr(evaluator, LookupStream, singles, new LookupStrategyDesc(LookupStrategyType.INKEYWORDMULTIIDX, new String[] { ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(_keyExpr) }));
        }
    
        public override String ToString()
        {
            return this.GetType().Name + " " +
                    base.ToString() +
                   " keyProperties=" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(_keyExpr);
        }
    }
}
