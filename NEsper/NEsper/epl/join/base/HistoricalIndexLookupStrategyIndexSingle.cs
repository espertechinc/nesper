///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.table;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>Index lookup strategy into a poll-based cache result.</summary>
    public class HistoricalIndexLookupStrategyIndexSingle : HistoricalIndexLookupStrategy
    {
        private readonly EventBean[] _eventsPerStream;
        private readonly ExprEvaluator _evaluator;
        private readonly int _lookupStream;
    
        public HistoricalIndexLookupStrategyIndexSingle(int lookupStream, QueryGraphValueEntryHashKeyed hashKey)
        {
            _eventsPerStream = new EventBean[lookupStream + 1];
            _evaluator = hashKey.KeyExpr.ExprEvaluator;
            _lookupStream = lookupStream;
        }
    
        public IEnumerator<EventBean> Lookup(EventBean lookupEvent, EventTable[] indexTable, ExprEvaluatorContext exprEvaluatorContext)
        {
            // The table may not be indexed as the cache may not actively cache, in which case indexing doesn't makes sense
            if (indexTable[0] is PropertyIndexedEventTableSingle)
            {
                var index = (PropertyIndexedEventTableSingle) indexTable[0];
                _eventsPerStream[_lookupStream] = lookupEvent;
                var key = _evaluator.Evaluate(new EvaluateParams(_eventsPerStream, true, exprEvaluatorContext));
    
                var events = index.Lookup(key);
                if (events != null) {
                    return events.GetEnumerator();
                }
                return null;
            }
    
            return indexTable[0].GetEnumerator();
        }
    
        public string ToQueryPlan() {
            return GetType().Name + " evaluator " + _evaluator.GetType().Name;
        }
    }
} // end of namespace
