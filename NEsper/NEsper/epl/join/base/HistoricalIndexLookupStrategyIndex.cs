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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// MapIndex lookup strategy into a poll-based cache result.
    /// </summary>
    public class HistoricalIndexLookupStrategyIndex : HistoricalIndexLookupStrategy
    {
        private readonly EventBean[] _eventsPerStream;
        private readonly int _lookupStream;
        private readonly ExprEvaluator[] _evaluators;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventType">event type to expect for lookup</param>
        /// <param name="lookupStream">The lookup stream.</param>
        /// <param name="hashKeys">The hash keys.</param>
        public HistoricalIndexLookupStrategyIndex(EventType eventType, int lookupStream, IList<QueryGraphValueEntryHashKeyed> hashKeys)
        {
            _evaluators = new ExprEvaluator[hashKeys.Count];
            for (int i = 0; i < hashKeys.Count; i++) {
                _evaluators[i] = hashKeys[i].KeyExpr.ExprEvaluator;
            }
            _eventsPerStream = new EventBean[lookupStream + 1];
            _lookupStream = lookupStream;
        }
    
        public IEnumerator<EventBean> Lookup(EventBean lookupEvent, EventTable[] indexTable, ExprEvaluatorContext exprEvaluatorContext)
        {
            // The table may not be indexed as the cache may not actively cache, in which case indexing doesn't makes sense
            if (indexTable[0] is PropertyIndexedEventTable)
            {
                var index = (PropertyIndexedEventTable)indexTable[0];
                var keys = GetKeys(lookupEvent, exprEvaluatorContext);
    
                var events = index.Lookup(keys);
                if (events != null)
                {
                    return events.GetEnumerator();
                }
                return null;
            }

            return indexTable[0].GetEnumerator();
        }
    
        private Object[] GetKeys(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            _eventsPerStream[_lookupStream] = theEvent;
            var keys = new Object[_evaluators.Length];
            for (int i = 0; i < _evaluators.Length; i++) {
                keys[i] = _evaluators[i].Evaluate(new EvaluateParams(_eventsPerStream, true, exprEvaluatorContext));
            }
            return keys;
        }
    
        public String ToQueryPlan() {
            return GetType().Name + " evaluators " + ExprNodeUtility.PrintEvaluators(_evaluators);
        }
    }
}
