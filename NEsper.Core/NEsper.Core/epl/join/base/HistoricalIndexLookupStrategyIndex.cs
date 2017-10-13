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
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.table;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>Index lookup strategy into a poll-based cache result.</summary>
    public class HistoricalIndexLookupStrategyIndex : HistoricalIndexLookupStrategy
    {
        private readonly ExprEvaluator[] _evaluators;
        private readonly EventBean[] _eventsPerStream;
        private readonly int _lookupStream;

        public HistoricalIndexLookupStrategyIndex(
            EventType eventType,
            int lookupStream,
            IList<QueryGraphValueEntryHashKeyed> hashKeys)
        {
            _evaluators = new ExprEvaluator[hashKeys.Count];
            for (int i = 0; i < hashKeys.Count; i++)
            {
                _evaluators[i] = hashKeys[i].KeyExpr.ExprEvaluator;
            }
            _eventsPerStream = new EventBean[lookupStream + 1];
            _lookupStream = lookupStream;
        }

        public IEnumerator<EventBean> Lookup(
            EventBean lookupEvent,
            EventTable[] indexTable,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // The table may not be indexed as the cache may not actively cache, in which case indexing doesn't makes sense
            if (indexTable[0] is PropertyIndexedEventTable)
            {
                var index = (PropertyIndexedEventTable) indexTable[0];
                Object[] keys = GetKeys(lookupEvent, exprEvaluatorContext);

                ISet<EventBean> events = index.Lookup(keys);
                if (events != null)
                {
                    return events.GetEnumerator();
                }
                return null;
            }

            return indexTable[0].GetEnumerator();
        }

        public string ToQueryPlan()
        {
            return GetType().Name + " evaluators " + ExprNodeUtility.PrintEvaluators(_evaluators);
        }

        private Object[] GetKeys(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            _eventsPerStream[_lookupStream] = theEvent;
            var evaluateParams = new EvaluateParams(_eventsPerStream, true, exprEvaluatorContext);
            var keys = new Object[_evaluators.Length];
            for (int i = 0; i < _evaluators.Length; i++)
            {
                keys[i] = _evaluators[i].Evaluate(evaluateParams);
            }
            return keys;
        }
    }
} // end of namespace