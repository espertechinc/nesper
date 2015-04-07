///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.rettype;

namespace com.espertech.esper.plugin
{
	/// <summary>
	/// Part of the aggregation multi-function extension API, this class represents
	/// one of more aggregation function expression instances. This class is responsible for providing
	/// a state reader (called accessor) for returning value from aggregation state, and for
	/// providing return type information of the accessor, and for providing state factory
	/// information.
	/// <para />Note the information returned by <seealso cref="ReturnType" /> must match the
	/// value objects returned by <seealso cref="Accessor" />.
	/// <para />
	/// For example, assuming you have an EPL statement such as
	///  <code>select search(), query() from MyEvent</code>
	/// then you would likely use one handler class and two handler objects (one for search and one for query).
	/// </summary>
	public interface PlugInAggregationMultiFunctionHandler
    {
	    /// <summary>
	    /// Returns the read function (an 'accessor').
	    /// <para />
	    /// Typically your application creates one accessor class
	    /// for each aggregation function name. So if you have two aggregation
	    /// functions such as "query" and "search" you would have two
	    /// accessor classes, one for "query" and one for "search".
	    /// <para />
	    /// Each aggregation function as it occurs in an EPL statement
	    /// obtains its own accessor. Your application can
	    /// return the same accessor object for all aggregation functions,
	    /// or different accessor objects for each aggregation function.
	    /// <para />
	    /// The objects returned by your accessor must match the
	    /// return type declared through <seealso cref="ReturnType" />.
	    /// </summary>
	    /// <value>accessor</value>
	    AggregationAccessor Accessor { get; }

	    /// <summary>
	    /// Provide return type.
	    /// <para />
	    /// The accessor return values must match the return type declared herein.
	    /// <para />
	    /// Use <seealso cref="com.espertech.esper.epl.rettype.EPTypeHelper.SingleValue(Type)" /> to indicate that the accessor
	    /// returns a single value. The accessor should return the single value upon invocation of
	    /// <seealso cref="AggregationAccessor.GetValue(com.espertech.esper.epl.agg.access.AggregationState, com.espertech.esper.client.EventBean[], bool, com.espertech.esper.epl.expression.core.ExprEvaluatorContext)" />.
	    /// The accessor should return a null value for all other accessor methods.
	    /// <para />
        /// Use <seealso cref="com.espertech.esper.epl.rettype.EPTypeHelper.CollectionOfEvents(com.espertech.esper.client.EventType)" /> to indicate that the accessor
	    /// returns a collection of events. The accessor should return a value in
	    /// <seealso cref="AggregationAccessor.GetEnumerableEvents(com.espertech.esper.epl.agg.access.AggregationState, com.espertech.esper.client.EventBean[], bool, com.espertech.esper.epl.expression.core.ExprEvaluatorContext)" />.
	    /// The accessor can also return an array of underlying event objects in
	    /// <seealso cref="AggregationAccessor.GetValue(com.espertech.esper.epl.agg.access.AggregationState, com.espertech.esper.client.EventBean[], bool, com.espertech.esper.epl.expression.core.ExprEvaluatorContext)" />.
	    /// The accessor should return a null value for all other accessor methods.
	    /// <para />
        /// Use <seealso cref="com.espertech.esper.epl.rettype.EPTypeHelper.SingleEvent(com.espertech.esper.client.EventType)" /> to indicate that the accessor
	    /// returns a single event. The accessor should return a value in
	    /// <seealso cref="AggregationAccessor.GetEnumerableEvent(com.espertech.esper.epl.agg.access.AggregationState, com.espertech.esper.client.EventBean[], bool, com.espertech.esper.epl.expression.core.ExprEvaluatorContext)" />.
	    /// The accessor can also return the underlying event object in
	    /// <seealso cref="AggregationAccessor.GetValue(com.espertech.esper.epl.agg.access.AggregationState, com.espertech.esper.client.EventBean[], bool, com.espertech.esper.epl.expression.core.ExprEvaluatorContext)" />.
	    /// The accessor should return a null value for all other accessor methods.
	    /// <para />
        /// Use <seealso cref="com.espertech.esper.epl.rettype.EPTypeHelper.CollectionOfSingleValue(Type)" /> to indicate that the accessor
	    /// returns a collection of single values (scalar, object etc.). The accessor should return a java.util.Collection in
	    /// <seealso cref="AggregationAccessor.GetValue(com.espertech.esper.epl.agg.access.AggregationState, com.espertech.esper.client.EventBean[], bool, com.espertech.esper.epl.expression.core.ExprEvaluatorContext)" />.
	    /// The accessor should return a null value for all other accessor methods.
	    /// <para />
        /// Use <seealso cref="com.espertech.esper.epl.rettype.EPTypeHelper.Array(Type)" /> to indicate that the accessor
	    /// returns an array of single values. The accessor should return an array in
	    /// <seealso cref="AggregationAccessor.GetValue(com.espertech.esper.epl.agg.access.AggregationState, com.espertech.esper.client.EventBean[], bool, com.espertech.esper.epl.expression.core.ExprEvaluatorContext)" />.
	    /// The accessor should return a null value for all other accessor methods.
	    /// </summary>
	    /// <value>expression result type</value>
	    EPType ReturnType { get; }

	    /// <summary>
	    /// Return a state-key object that determines how the engine shares aggregation state
	    /// between multiple aggregation functions that may appear in the same EPL statement.
	    /// <para />
	    /// The engine applies equals-semantics to determine state sharing. If
	    /// two <seealso cref="AggregationStateKey" /> instances are equal (implement hashCode and equals)
	    /// then the engine shares a single aggregation state instance for the two
	    /// aggregation function expressions.
	    /// <para />
	    /// If your aggregation function never needs shared state
	    /// simple return <code>new AggregationStateKey(){}</code>.
	    /// <para />
	    /// If your aggregation function always shares state
	    /// simple declare <code>private static final AggregationStateKey MY_KEY = new AggregationStateKey() {};</code>and <code>return MY_KEY</code>; (if using multiple handlers declare the key on the factory level).
	    /// </summary>
	    /// <value>state key</value>
	    AggregationStateKey AggregationStateUniqueKey { get; }

	    /// <summary>
	    /// Return the state factory for the sharable aggregation function state.
	    /// <para />
	    /// The engine only obtains a state factory once for all shared aggregation state.
	    /// </summary>
	    /// <value>state factory</value>
	    PlugInAggregationMultiFunctionStateFactory StateFactory { get; }

	    AggregationAgent GetAggregationAgent(PlugInAggregationMultiFunctionAgentContext agentContext);
	}
} // end of namespace
