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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.exec.composite;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// MapIndex lookup strategy into a poll-based cache result.
    /// </summary>
    public class HistoricalIndexLookupStrategyComposite : HistoricalIndexLookupStrategy
    {
        private readonly CompositeIndexQuery _chain;

        public HistoricalIndexLookupStrategyComposite(int lookupStream, IList<QueryGraphValueEntryHashKeyed> hashKeys, Type[] keyCoercionTypes, IList<QueryGraphValueEntryRange> rangeKeyPairs, Type[] rangeCoercionTypes)
        {
            _chain = CompositeIndexQueryFactory.MakeJoinSingleLookupStream(false, lookupStream, hashKeys, keyCoercionTypes, rangeKeyPairs, rangeCoercionTypes);
        }
    
        public IEnumerator<EventBean> Lookup(EventBean lookupEvent, EventTable[] indexTable, ExprEvaluatorContext context)
        {
            // The table may not be indexed as the cache may not actively cache, in which case indexing doesn't makes sense
            if (indexTable[0] is PropertyCompositeEventTable)
            {
                var table = (PropertyCompositeEventTable) indexTable[0];
                var index = table.IndexTable;
    
                var events = _chain.Get(lookupEvent, index, context, table.PostProcessor);
                if (events != null)
                {
                    return events.GetEnumerator();
                }
                return null;
            }
    
            return indexTable[0].GetEnumerator();
        }
    
        public String ToQueryPlan()
        {
            return string.Format("{0} chain {1}", GetType().Name, _chain.GetType().Name);
        }
    }
}
