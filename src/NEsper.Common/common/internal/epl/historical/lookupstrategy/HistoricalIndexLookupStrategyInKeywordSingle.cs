///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.join.exec.inkeyword;

namespace com.espertech.esper.common.@internal.epl.historical.lookupstrategy
{
    public class HistoricalIndexLookupStrategyInKeywordSingle : HistoricalIndexLookupStrategy
    {
        private ExprEvaluator[] evaluators;

        private EventBean[] eventsPerStream;
        private int lookupStream;

        public ExprEvaluator[] Evaluators {
            set => evaluators = value;
        }

        public int LookupStream {
            set {
                lookupStream = value;
                eventsPerStream = new EventBean[value + 1];
            }
        }

        public IEnumerator<EventBean> Lookup(
            EventBean lookupEvent,
            EventTable[] index,
            ExprEvaluatorContext context)
        {
            if (index[0] is PropertyHashedEventTable) {
                var idx = (PropertyHashedEventTable)index[0];
                eventsPerStream[lookupStream] = lookupEvent;

                var result = InKeywordTableLookupUtil.SingleIndexLookup(evaluators, eventsPerStream, context, idx);

                return result?.GetEnumerator();
            }

            return index[0].GetEnumerator();
        }
    }
} // end of namespace