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
using com.espertech.esper.common.@internal.epl.index.composite;
using com.espertech.esper.common.@internal.epl.join.exec.composite;
using com.espertech.esper.common.@internal.epl.join.querygraph;

namespace com.espertech.esper.common.@internal.epl.historical.lookupstrategy
{
    public class HistoricalIndexLookupStrategyComposite : HistoricalIndexLookupStrategy
    {
        private CompositeIndexQuery chain;
        private ExprEvaluator hashGetter;

        private int lookupStream;
        private QueryGraphValueEntryRange[] rangeProps;

        public int LookupStream {
            set => lookupStream = value;
        }

        public ExprEvaluator HashGetter {
            set => hashGetter = value;
        }

        public QueryGraphValueEntryRange[] RangeProps {
            set => rangeProps = value;
        }

        public CompositeIndexQuery Chain {
            set => chain = value;
        }

        public IEnumerator<EventBean> Lookup(
            EventBean lookupEvent,
            EventTable[] index,
            ExprEvaluatorContext context)
        {
            if (index[0] is PropertyCompositeEventTable) {
                var idx = (PropertyCompositeEventTable) index[0];
                var map = idx.Index;
                var events = chain.Get(lookupEvent, map, context, idx.PostProcessor);
                if (events != null) {
                    return events.GetEnumerator();
                }

                return null;
            }

            return index[0].GetEnumerator();
        }

        public void Init()
        {
            chain = CompositeIndexQueryFactory.MakeJoinSingleLookupStream(false, lookupStream, hashGetter, rangeProps);
        }
    }
} // end of namespace