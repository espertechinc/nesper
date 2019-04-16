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
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    /// Part of the aggregation multi-function extension API, this class represents
    /// one of more aggregation function expression instances. This class is responsible for providing
    /// a state reader (called accessor) for returning value from aggregation state, and for
    /// providing return type information of the accessor, and for providing state factory
    /// information.
    /// <para />Note the information returned by {@link #getReturnType()} must match the
    /// value objects returned by accessors provided by {@link #getAccessorMode()}.
    /// <para />For example, assuming you have an EPL statement such as {@code select search(), query() from MyEvent}
    /// then you would likely use one handler class and two handler objects (one for search and one for query).
    /// </summary>
    public interface AggregationMultiFunctionHandler
    {
        /// <summary>
        /// Provide return type.
        /// <para />The accessor return values must match the return type declared herein.
        /// <para />Use {@link EPTypeHelper#singleValue(Class)} (Class)} to indicate that the accessor
        /// returns a single value. The accessor should return the single value upon invocation of
        /// {@link AggregationMultiFunctionAccessor#getValue(AggregationMultiFunctionState, EventBean[], boolean, ExprEvaluatorContext)}.
        /// The accessor should return a null value for all other accessor methods.
        /// <para />Use {@link EPTypeHelper#collectionOfEvents(EventType)} to indicate that the accessor
        /// returns a collection of events. The accessor should return a value in
        /// {@link AggregationMultiFunctionAccessor#getEnumerableEvents(AggregationMultiFunctionState, EventBean[], boolean, ExprEvaluatorContext)}.
        /// The accessor can also return an array of underlying event objects in
        /// {@link AggregationMultiFunctionAccessor#getValue(AggregationMultiFunctionState, EventBean[], boolean, ExprEvaluatorContext)}.
        /// The accessor should return a null value for all other accessor methods.
        /// <para />Use {@link EPTypeHelper#singleEvent(EventType)} to indicate that the accessor
        /// returns a single event. The accessor should return a value in
        /// {@link AggregationMultiFunctionAccessor#getEnumerableEvent(AggregationMultiFunctionState, EventBean[], boolean, ExprEvaluatorContext)}.
        /// The accessor can also return the underlying event object in
        /// {@link AggregationMultiFunctionAccessor#getValue(AggregationMultiFunctionState, EventBean[], boolean, ExprEvaluatorContext)}.
        /// The accessor should return a null value for all other accessor methods.
        /// <para />Use {@link EPTypeHelper#collectionOfSingleValue(Class)} to indicate that the accessor
        /// returns a collection of single values (scalar, object etc.). The accessor should return a java.util.Collection in
        /// {@link AggregationMultiFunctionAccessor#getValue(AggregationMultiFunctionState, EventBean[], boolean, ExprEvaluatorContext)}.
        /// The accessor should return a null value for all other accessor methods.
        /// <para />Use {@link EPTypeHelper#array(Class)} to indicate that the accessor
        /// returns an array of single values. The accessor should return an array in
        /// {@link AggregationMultiFunctionAccessor#getValue(AggregationMultiFunctionState, EventBean[], boolean, ExprEvaluatorContext)}.
        /// The accessor should return a null value for all other accessor methods.
        /// </summary>
        /// <value>expression result type</value>
        EPType ReturnType { get; }

        /// <summary>
        /// Return a state-key object that determines how the runtimeshares aggregation state
        /// between multiple aggregation functions that may appear in the same EPL statement.
        /// <para />The runtimeapplies equals-semantics to determine state sharing. If
        /// two <seealso cref="AggregationMultiFunctionStateKey" /> instances are equal (implement hashCode and equals)
        /// then the runtimeshares a single aggregation state instance for the two
        /// aggregation function expressions.
        /// <para />If your aggregation function never needs shared state
        /// simple return {@code new AggregationStateKey(){}}.
        /// <para />If your aggregation function always shares state
        /// simple declare {@code private static final AggregationStateKey MY_KEY = new AggregationStateKey() {};}
        /// and {@code return MY_KEY}; (if using multiple handlers declare the key on the factory level).
        /// </summary>
        /// <value>state key</value>
        AggregationMultiFunctionStateKey AggregationStateUniqueKey { get; }

        /// <summary>
        /// Describes to the compiler how it should manage code for providing aggregation state.
        /// </summary>
        /// <value>mode object</value>
        AggregationMultiFunctionStateMode StateMode { get; }

        /// <summary>
        /// Describes to the compiler how it should manage code for providing aggregation accessors.
        /// </summary>
        /// <value>mode object</value>
        AggregationMultiFunctionAccessorMode AccessorMode { get; }

        /// <summary>
        /// Describes to the compiler how it should manage code for providing aggregation agents.
        /// </summary>
        /// <value>mode object</value>
        AggregationMultiFunctionAgentMode AgentMode { get; }

        /// <summary>
        /// Describes to the compiler how it should manage code for providing table column reader.
        /// </summary>
        /// <value>mode object</value>
        AggregationMultiFunctionTableReaderMode TableReaderMode { get; }
    }
} // end of namespace