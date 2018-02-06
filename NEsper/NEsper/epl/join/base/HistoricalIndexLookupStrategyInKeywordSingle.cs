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
    public class HistoricalIndexLookupStrategyInKeywordSingle : HistoricalIndexLookupStrategy
    {
        private readonly ExprEvaluator[] _evaluators;
        private readonly EventBean[] _eventsPerStream;
        private readonly int _lookupStream;

        public HistoricalIndexLookupStrategyInKeywordSingle(int lookupStream, IList<ExprNode> expressions)
        {
            _eventsPerStream = new EventBean[lookupStream + 1];
            _evaluators = ExprNodeUtility.GetEvaluators(expressions);
            _lookupStream = lookupStream;
        }

        public IEnumerator<EventBean> Lookup(
            EventBean lookupEvent,
            EventTable[] indexTable,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var table = (PropertyIndexedEventTableSingle) indexTable[0];
            _eventsPerStream[_lookupStream] = lookupEvent;

            var result = InKeywordTableLookupUtil.SingleIndexLookup(
                _evaluators, _eventsPerStream, exprEvaluatorContext, table);
            if (result == null)
            {
                return null;
            }
            return result.GetEnumerator();
        }

        public string ToQueryPlan()
        {
            return this.GetType().Name;
        }
    }
} // end of namespace