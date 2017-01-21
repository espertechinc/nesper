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
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.epl.@join.table;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>Index lookup strategy into a poll-based cache result. </summary>
    public class HistoricalIndexLookupStrategyInKeywordMulti : HistoricalIndexLookupStrategy
    {
        private readonly EventBean[] _eventsPerStream;
        private readonly ExprEvaluator _evaluator;
        private readonly int _lookupStream;

        /// <summary>Ctor. </summary>
        public HistoricalIndexLookupStrategyInKeywordMulti(int lookupStream, ExprNode expression)
        {
            _eventsPerStream = new EventBean[lookupStream + 1];
            _evaluator = expression.ExprEvaluator;
            _lookupStream = lookupStream;
        }

        public IEnumerator<EventBean> Lookup(
            EventBean lookupEvent,
            EventTable[] indexTable,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            _eventsPerStream[_lookupStream] = lookupEvent;
            ICollection<EventBean> result = InKeywordTableLookupUtil.MultiIndexLookup(
                _evaluator, _eventsPerStream, exprEvaluatorContext, indexTable);
            if (result == null)
            {
                return null;
            }
            return result.GetEnumerator();
        }

        public String ToQueryPlan()
        {
            return this.GetType().Name;
        }
    }
}
