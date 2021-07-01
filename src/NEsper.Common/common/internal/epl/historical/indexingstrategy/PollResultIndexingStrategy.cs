///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    /// <summary>
    /// A strategy for converting a poll-result into a potentially indexed table.
    /// <para />Some implementations may decide to not index the poll result and simply hold a reference to the result.
    /// Other implementations may use predetermined index properties to index the poll result for faster lookup.
    /// </summary>
    public interface PollResultIndexingStrategy
    {
        /// <summary>
        /// Build and index of a poll result.
        /// </summary>
        /// <param name="pollResult">result of a poll operation</param>
        /// <param name="isActiveCache">true to indicate that caching is active and therefore index building makes sense asthe index structure is not a throw-away.
        /// </param>
        /// <param name="agentInstanceContext">statement context</param>
        /// <returns>indexed collection of poll results</returns>
        EventTable[] Index(
            IList<EventBean> pollResult,
            bool isActiveCache,
            AgentInstanceContext agentInstanceContext);
    }

    public class ProxyPollResultIndexingStrategy : PollResultIndexingStrategy
    {
        public Func<IList<EventBean>, bool, AgentInstanceContext, EventTable[]> ProcIndex;

        public ProxyPollResultIndexingStrategy()
        {
        }

        public ProxyPollResultIndexingStrategy(
            Func<IList<EventBean>, bool, AgentInstanceContext, EventTable[]> procIndex)
        {
            ProcIndex = procIndex;
        }

        public EventTable[] Index(
            IList<EventBean> pollResult,
            bool isActiveCache,
            AgentInstanceContext agentInstanceContext)
        {
            return ProcIndex?.Invoke(pollResult, isActiveCache, agentInstanceContext);
        }
    }
} // end of namespace