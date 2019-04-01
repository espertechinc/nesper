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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
	/// <summary>
	/// Accessor for access aggregation functions.
	/// </summary>
	public interface AggregationMultiFunctionAccessor {
	    /// <summary>
	    /// Return the aggregation state value either as a scalar value or any other object.
	    /// <para />For enumeration over scalar values or objects return an array or collection of scalar or object values.
	    /// <para />Use the #getEnumerableEvents method to return a collection of events.
	    /// <para />Use the #getEnumerableEvent to return a single events.
	    /// </summary>
	    /// <param name="state">aggregation state, downcast as needed</param>
	    /// <param name="eventsPerStream">events</param>
	    /// <param name="isNewData">new-data indicator</param>
	    /// <param name="exprEvaluatorContext">eval context</param>
	    /// <returns>return value</returns>
	    object GetValue(AggregationMultiFunctionState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);

	    /// <summary>
	    /// Return the aggregation state value consisting of a collection of events.
	    /// </summary>
	    /// <param name="state">aggregation state, downcast as needed</param>
	    /// <param name="eventsPerStream">events</param>
	    /// <param name="isNewData">new-data indicator</param>
	    /// <param name="exprEvaluatorContext">eval context</param>
	    /// <returns>return collection of events or null or empty collection</returns>
	    ICollection<EventBean> GetEnumerableEvents(AggregationMultiFunctionState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);

	    /// <summary>
	    /// Return the aggregation state value consisting of a single event.
	    /// </summary>
	    /// <param name="state">aggregation state, downcast as needed</param>
	    /// <param name="eventsPerStream">events</param>
	    /// <param name="isNewData">new-data indicator</param>
	    /// <param name="exprEvaluatorContext">eval context</param>
	    /// <returns>return event or null</returns>
	    EventBean GetEnumerableEvent(AggregationMultiFunctionState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);

	    /// <summary>
	    /// Return the aggregation state value consisting of a collection of scalar values.
	    /// </summary>
	    /// <param name="state">aggregation state, downcast as needed</param>
	    /// <param name="eventsPerStream">events</param>
	    /// <param name="isNewData">new-data indicator</param>
	    /// <param name="exprEvaluatorContext">eval context</param>
	    /// <returns>return collection of scalar or null or empty collection</returns>
	    ICollection<object> GetEnumerableScalar(AggregationMultiFunctionState state, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);
	}
} // end of namespace