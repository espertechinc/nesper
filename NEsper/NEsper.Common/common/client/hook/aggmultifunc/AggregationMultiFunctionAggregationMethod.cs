///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    /// Aggregation method that operates on aggregation multi-function state such as provided by a multi-function
    /// aggregation (standalone or table column).
    /// </summary>
    public interface AggregationMultiFunctionAggregationMethod
    {
        /// <summary>
        /// Returns the plain value
        /// </summary>
        /// <param name="aggColNum">column number</param>
        /// <param name="row">aggregation row</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">new-data flag</param>
        /// <param name="exprEvaluatorContext">evaluation context</param>
        /// <returns>value</returns>
        object GetValue(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Return a collection of events or null when not available.
        /// The <seealso cref="EPType" /> returned by the handler indicates whether the compiler allows operations on events.
        /// </summary>
        /// <param name="aggColNum">column number</param>
        /// <param name="row">aggregation row</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">new-data flag</param>
        /// <param name="exprEvaluatorContext">evaluation context</param>
        /// <returns>collection of &lt;seealso cref="EventBean" /&gt;</returns>
        ICollection<EventBean> GetValueCollectionEvents(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Return a collection of values or null when not available.
        /// The <seealso cref="EPType" /> returned by the handler indicates whether the compiler allows operations on events.
        /// </summary>
        /// <param name="aggColNum">column number</param>
        /// <param name="row">aggregation row</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">new-data flag</param>
        /// <param name="exprEvaluatorContext">evaluation context</param>
        /// <returns>collection of values</returns>
        ICollection<object> GetValueCollectionScalar(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Returns a single event or null when not available.
        /// The <seealso cref="EPType" /> returned by the handler indicates whether the compiler allows operations on events.
        /// </summary>
        /// <param name="aggColNum">column number</param>
        /// <param name="row">aggregation row</param>
        /// <param name="eventsPerStream">events</param>
        /// <param name="isNewData">new-data flag</param>
        /// <param name="exprEvaluatorContext">evaluation context</param>
        /// <returns>event</returns>
        EventBean GetValueEventBean(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);
    }
}