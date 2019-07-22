///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    /// Base interface for providing access-aggregations, i.e. aggregations that mirror a data window
    /// but group by the group-by clause and that do not mirror the data windows sorting policy.
    /// </summary>
    public interface AggregationMultiFunctionState
    {
        /// <summary>
        /// Enter an event.
        /// </summary>
        /// <param name="eventsPerStream">all events in all streams, typically implementations pick the relevant stream's events to add</param>
        /// <param name="exprEvaluatorContext">expression eval context</param>
        void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Remove an event.
        /// </summary>
        /// <param name="eventsPerStream">all events in all streams, typically implementations pick the relevant stream's events to remove</param>
        /// <param name="exprEvaluatorContext">expression eval context</param>
        void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Clear all events in the group.
        /// </summary>
        void Clear();
    }
} // end of namespace