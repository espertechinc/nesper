///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>
    /// Plan to perform an indexed table lookup.
    /// </summary>
    public class IndexedTableLookupPlanMulti : TableLookupPlan
    {
        private readonly IList<QueryGraphValueEntryHashKeyed> _keyProperties;
    
        /// <summary>Ctor. </summary>
        /// <param name="lookupStream">stream that generates event to look up for</param>
        /// <param name="indexedStream">stream to index table lookup</param>
        /// <param name="indexNum">index number for the table containing the full unindexed contents</param>
        /// <param name="keyProperties">properties to use in lookup event to access index</param>
        public IndexedTableLookupPlanMulti(int lookupStream, int indexedStream, TableLookupIndexReqKey indexNum, IList<QueryGraphValueEntryHashKeyed> keyProperties)
            : base(lookupStream, indexedStream, new TableLookupIndexReqKey[] { indexNum })
        {
            _keyProperties = keyProperties;
        }

        public override TableLookupKeyDesc KeyDescriptor
        {
            get { return new TableLookupKeyDesc(_keyProperties, Collections.GetEmptyList<QueryGraphValueEntryRange>()); }
        }

        public override JoinExecTableLookupStrategy MakeStrategyInternal(EventTable[] eventTable, EventType[] eventTypes)
        {
            var index = (PropertyIndexedEventTable) eventTable[0];
            var keyProps = new String[_keyProperties.Count];
            var evaluators = new ExprEvaluator[_keyProperties.Count];
            var expressions = new String[_keyProperties.Count];
            var isStrictlyProps = true;
            for (var i = 0; i < keyProps.Length; i++) {
                isStrictlyProps = isStrictlyProps && _keyProperties[i] is QueryGraphValueEntryHashKeyedProp;
                evaluators[i] = _keyProperties[i].KeyExpr.ExprEvaluator;
                expressions[i] = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(_keyProperties[i].KeyExpr);

                if (_keyProperties[i] is QueryGraphValueEntryHashKeyedProp)
                {
                    keyProps[i] = ((QueryGraphValueEntryHashKeyedProp)_keyProperties[i]).KeyProperty;
                }
                else {
                    isStrictlyProps = false;
                }
            }
            if (isStrictlyProps) {
                return new IndexedTableLookupStrategy(eventTypes[this.LookupStream], keyProps, index);
            }
            else {
                return new IndexedTableLookupStrategyExpr(evaluators, LookupStream, index, new LookupStrategyDesc(LookupStrategyType.MULTIEXPR, expressions));
            }            
        }
    
        public override String ToString()
        {
            return GetType().FullName + " " + base.ToString() + " keyProperties=" + QueryGraphValueEntryHashKeyed.ToQueryPlan(_keyProperties);
        }
    }
}
