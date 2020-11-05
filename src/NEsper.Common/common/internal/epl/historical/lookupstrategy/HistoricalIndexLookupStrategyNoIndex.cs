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
    /// <summary>
    ///     Full table scan strategy for a poll-based cache result.
    /// </summary>
    public class HistoricalIndexLookupStrategyNoIndex : HistoricalIndexLookupStrategy
    {
        public static readonly HistoricalIndexLookupStrategyNoIndex INSTANCE =
            new HistoricalIndexLookupStrategyNoIndex();

        private HistoricalIndexLookupStrategyNoIndex()
        {
        }

        public IEnumerator<EventBean> Lookup(
            EventBean lookupEvent,
            EventTable[] index,
            ExprEvaluatorContext context)
        {
            return index[0].GetEnumerator();
        }
    }
} // end of namespace