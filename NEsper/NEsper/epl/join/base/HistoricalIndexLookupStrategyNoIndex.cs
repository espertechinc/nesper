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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Full table scan strategy for a poll-based cache result.
    /// </summary>
    public class HistoricalIndexLookupStrategyNoIndex : HistoricalIndexLookupStrategy
    {
        #region HistoricalIndexLookupStrategy Members

        public IEnumerator<EventBean> Lookup(EventBean lookupEvent,
                                             EventTable[] index,
                                             ExprEvaluatorContext context)
        {
            return index[0].GetEnumerator();
        }

        public string ToQueryPlan()
        {
            return GetType().FullName;
        }

        #endregion
    }
}