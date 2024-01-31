///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.historical.indexingstrategy
{
    /// <summary>
    ///     Strategy of indexing that simply builds an unindexed table of poll results.
    ///     <para>
    ///         For use when caching is disabled or when no proper index could be build because no where-clause or on-clause
    ///         exists
    ///         or
    ///         these clauses don't yield indexable columns on analysis.
    ///     </para>
    /// </summary>
    public class PollResultIndexingStrategyNoIndex : PollResultIndexingStrategy
    {
        public static readonly PollResultIndexingStrategyNoIndex INSTANCE = new PollResultIndexingStrategyNoIndex();

        private PollResultIndexingStrategyNoIndex()
        {
        }

        public EventTable[] Index(
            IList<EventBean> pollResult,
            bool isActiveCache,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return new EventTable[] { new UnindexedEventTableList(pollResult, -1) };
        }
    }
} // end of namespace