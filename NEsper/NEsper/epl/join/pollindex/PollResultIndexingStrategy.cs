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
using com.espertech.esper.core.service;
using com.espertech.esper.epl.join.table;

namespace com.espertech.esper.epl.join.pollindex
{
    /// <summary>
    /// A strategy for converting a poll-result into a potentially indexed table.
    /// <para/>
    /// Some implementations may decide to not index the poll result and simply hold 
    /// a reference to the result. Other implementations may use predetermined index 
    /// properties to index the poll result for faster lookup.
    /// </summary>
    public interface PollResultIndexingStrategy
    {
        /// <summary>
        /// Build and index of a poll result.
        /// </summary>
        /// <param name="pollResult">result of a poll operation</param>
        /// <param name="isActiveCache">true to indicate that caching is active and therefore index building makes sense asthe index structure is not a throw-away.</param>
        /// <param name="statementContext"></param>
        /// <returns>indexed collection of poll results</returns>
        EventTable[] Index(IList<EventBean> pollResult, bool isActiveCache, StatementContext statementContext);
    
        String ToQueryPlan();
    }

    public class ProxyPollResultIndexingStrategy : PollResultIndexingStrategy
    {
        public Func<IList<EventBean>, bool, StatementContext, EventTable[]> ProcIndex { get; set; }
        public Func<string> ProcToQueryPlan { get; set; }

        public EventTable[] Index(IList<EventBean> pollResult, bool isActiveCache, StatementContext statementContext)
        {
            return ProcIndex(pollResult, isActiveCache, statementContext);
        }

        public string ToQueryPlan()
        {
            return ProcToQueryPlan();
        }
    }
}
