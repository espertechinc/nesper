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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.exec.@base;
using com.espertech.esper.epl.@join.table;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Plan to perform an indexed table lookup.
    /// </summary>
    public class IndexedTableLookupPlanSingle : TableLookupPlan
    {
        private readonly QueryGraphValueEntryHashKeyed _hashKey;
    
        /// <summary>Ctor. </summary>
        /// <param name="lookupStream">stream that generates event to look up for</param>
        /// <param name="indexedStream">stream to index table lookup</param>
        /// <param name="indexNum">index number for the table containing the full unindexed contents</param>
        /// <param name="hashKey">properties to use in lookup event to access index</param>
        public IndexedTableLookupPlanSingle(int lookupStream, int indexedStream, TableLookupIndexReqKey indexNum, QueryGraphValueEntryHashKeyed hashKey)
            : base(lookupStream, indexedStream, new TableLookupIndexReqKey[] { indexNum })
        {
            _hashKey = hashKey;
        }

        public override TableLookupKeyDesc KeyDescriptor
        {
            get
            {
                return new TableLookupKeyDesc(
                    Collections.SingletonList(_hashKey),
                    Collections.GetEmptyList<QueryGraphValueEntryRange>());
            }
        }

        public override JoinExecTableLookupStrategy MakeStrategyInternal(EventTable[] eventTable, EventType[] eventTypes)
        {
            var index = (PropertyIndexedEventTableSingle) eventTable[0];
            if (_hashKey is QueryGraphValueEntryHashKeyedExpr) {
                var expr = (QueryGraphValueEntryHashKeyedExpr) _hashKey;
                return new IndexedTableLookupStrategySingleExpr(expr.KeyExpr, base.LookupStream, index,
                        new LookupStrategyDesc(LookupStrategyType.SINGLEEXPR, new String[] {ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(expr.KeyExpr)}));
            }
            else if (_hashKey is QueryGraphValueEntryHashKeyedProp) {
                var prop = (QueryGraphValueEntryHashKeyedProp) _hashKey;
                return new IndexedTableLookupStrategySingle(eventTypes[LookupStream], prop.KeyProperty, index);
            }
            else {
                throw new ArgumentException("Invalid hashkey instance " + _hashKey);
            }
        }

        public QueryGraphValueEntryHashKeyed HashKey
        {
            get { return _hashKey; }
        }

        public override String ToString()
        {
            return "IndexedTableLookupPlan " +
                    base.ToString() +
                   " keyProperty=" + KeyDescriptor;
        }
    }
}
