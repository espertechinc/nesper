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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>Index lookup strategy into a poll-based cache result.</summary>
    public class HistoricalIndexLookupStrategyIndexSingle : HistoricalIndexLookupStrategy {
        private readonly EventBean[] eventsPerStream;
        private readonly ExprEvaluator evaluator;
        private readonly int lookupStream;
    
        public HistoricalIndexLookupStrategyIndexSingle(int lookupStream, QueryGraphValueEntryHashKeyed hashKey) {
            this.eventsPerStream = new EventBean[lookupStream + 1];
            this.evaluator = hashKey.KeyExpr.ExprEvaluator;
            this.lookupStream = lookupStream;
        }
    
        public IEnumerator<EventBean> Lookup(EventBean lookupEvent, EventTable[] indexTable, ExprEvaluatorContext exprEvaluatorContext) {
            // The table may not be indexed as the cache may not actively cache, in which case indexing doesn't makes sense
            if (indexTable[0] is PropertyIndexedEventTableSingle) {
                PropertyIndexedEventTableSingle index = (PropertyIndexedEventTableSingle) indexTable[0];
                eventsPerStream[lookupStream] = lookupEvent;
                Object key = evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);
    
                ISet<EventBean> events = index.Lookup(key);
                if (events != null) {
                    return Events.GetEnumerator();
                }
                return null;
            }
    
            return indexTable[0].GetEnumerator();
        }
    
        public string ToQueryPlan() {
            return this.GetType().Name + " evaluator " + evaluator.Class.SimpleName;
        }
    }
} // end of namespace
