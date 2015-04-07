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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Interface for use by aggregate expression nodes representing aggregate functions such as 'sum' or 'avg' to 
    /// use to obtain the current value for the function at time of expression evaluation.
    /// </summary>
    public interface AggregationResultFuture
    {
        /// <summary>
        /// Returns current aggregation state, for use by expression node representing an aggregation function.
        /// </summary>
        /// <param name="column">is assigned to the aggregation expression node and passed as an column (index) into a row</param>
        /// <param name="agentInstanceId">the context partition id</param>
        /// <param name="eventsPerStream">The events per stream.</param>
        /// <param name="isNewData">The new data indicator.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <returns>
        /// current aggragation state
        /// </returns>
        object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);
    
        ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
    
        EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
    
        Object GetGroupKey(int agentInstanceId);
    
        ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext);

        ICollection<Object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
    }
}
