///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.datacache;
using com.espertech.esper.common.@internal.epl.historical.indexingstrategy;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.common.@internal.epl.historical.common
{
    /// <summary>
    ///     Interface for views that poll data based on information from other streams.
    /// </summary>
    public interface HistoricalEventViewable : Viewable,
        AgentInstanceMgmtCallback
    {
#if INHERITED
        EventType EventType { get; }
#endif

        /// <summary>
        ///     Returns true if the parameters expressions to the historical require other stream's data,
        ///     or false if there are no parameters or all parameter expressions are only contants and variables without
        ///     properties of other stream events.
        /// </summary>
        /// <returns>indicator whether properties are required for parameter evaluation</returns>
        bool HasRequiredStreams { get; }

        IThreadLocal<HistoricalDataCache> DataCacheThreadLocal { get; }

        /// <summary>
        ///     Poll for stored historical or reference data using events per stream and
        ///     returing for each event-per-stream row a separate list with events
        ///     representing the poll result.
        /// </summary>
        /// <param name="lookupEventsPerStream">
        ///     is the events per stream where thefirst dimension is a number of rows (often 1 depending on windows used) and
        ///     the second dimension is the number of streams participating in a join.
        /// </param>
        /// <param name="indexingStrategy">the strategy to use for converting poll results into a indexed table for fast lookup</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>array of lists with one list for each event-per-stream row</returns>
        EventTable[][] Poll(
            EventBean[][] lookupEventsPerStream,
            PollResultIndexingStrategy indexingStrategy,
            ExprEvaluatorContext exprEvaluatorContext);
    }
} // end of namespace