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

namespace com.espertech.esper.common.@internal.epl.historical.lookupstrategy
{
    public class HistoricalIndexLookupStrategyMulti : HistoricalIndexLookupStrategy
    {
        private int indexUsed;
        private HistoricalIndexLookupStrategy innerLookupStrategy;

        public int IndexUsed {
            set => indexUsed = value;
        }

        public HistoricalIndexLookupStrategy InnerLookupStrategy {
            set => innerLookupStrategy = value;
        }

        public IEnumerator<EventBean> Lookup(
            EventBean lookupEvent,
            EventTable[] index,
            ExprEvaluatorContext context)
        {
            if (index[0] is MultiIndexEventTable) {
                var multiIndex = (MultiIndexEventTable) index[0];
                var indexToUse = multiIndex.Tables[indexUsed];
                return innerLookupStrategy.Lookup(lookupEvent, new[] {indexToUse}, context);
            }

            return index[0].GetEnumerator();
        }
    }
} // end of namespace