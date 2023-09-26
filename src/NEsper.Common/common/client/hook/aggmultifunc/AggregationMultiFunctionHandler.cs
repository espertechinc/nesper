///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    ///     Part of the aggregation multi-function extension API, this class represents
    ///     one of more aggregation function expression instances. This class is responsible for providing
    ///     a state reader (called accessor) for returning value from aggregation state, and for
    ///     providing return type information of the accessor, and for providing state factory
    ///     information.
    ///     <para>
    ///         Note the information returned by <seealso cref="ReturnType" /> must match the
    ///         value objects returned by accessors provided by <seealso cref="AccessorMode" />.
    ///     </para>
    ///     <para>
    ///         For example, assuming you have an EPL statement such as {@code select search(), query() from MyEvent}
    ///         then you would likely use one handler class and two handler objects (one for search and one for query).
    ///     </para>
    /// </summary>
    public interface AggregationMultiFunctionHandler
    {
        /// <summary>
        ///     Provide return type.
        ///     <para>
        ///         The accessor return values must match the return type declared herein.
        ///     </para>
        ///     <para>
        ///         Use <seelaso cref="EPChainableTypeHelper.SingleValue" /> to indicate that the accessor
        ///         returns a single value. The accessor should return the single value upon invocation of
        ///         <seealso cref="AggregationMultiFunctionAccessor.GetValue" />.
        ///         The accessor should return a null value for all other accessor methods.
        ///     </para>
        ///     <para>
        ///         Use {@link EPTypeHelper#collectionOfEvents(EventType)} to indicate that the accessor
        ///         returns a collection of events. The accessor should return a value in
        ///         <seealso cref="AggregationMultiFunctionAccessor.GetEnumerableEvents" />.
        ///         The accessor can also return an array of underlying event objects in
        ///         <seealso cref="AggregationMultiFunctionAccessor.GetValue" />.
        ///         The accessor should return a null value for all other accessor methods.
        ///     </para>
        ///     <para>
        ///         Use {@link EPTypeHelper#singleEvent(EventType)} to indicate that the accessor
        ///         returns a single event. The accessor should return a value in
        ///         <seealso cref="AggregationMultiFunctionAccessor.GetEnumerableEvent" />.
        ///         The accessor can also return the underlying event object in
        ///         {@link AggregationMultiFunctionAccessor#getValue.
        ///         The accessor should return a null value for all other accessor methods.
        ///     </para>
        ///     <para>
        ///         Use <seealso cref="EPChainableTypeHelper.CollectionOfSingleValue" /> to indicate that the accessor
        ///         returns a collection of single values (scalar, object etc.). The accessor should return a
        ///         Collection in
        ///         <seealso cref="AggregationMultiFunctionAccessor.GetValue" />.
        ///         The accessor should return a null value for all other accessor methods.
        ///     </para>
        ///     <para>
        ///         Use <seealso cref="EPChainableTypeHelper.Array" /> to indicate that the accessor
        ///         returns an array of single values. The accessor should return an array in
        ///         {@link AggregationMultiFunctionAccessor#getValue(AggregationMultiFunctionState, EventBean[], boolean,
        ///         ExprEvaluatorContext)}.
        ///         The accessor should return a null value for all other accessor methods.
        ///     </para>
        /// </summary>
        /// <value>expression result type</value>
        EPChainableType ReturnType { get; }

        /// <summary>
        ///     Return a state-key object that determines how the runtime shares aggregation state
        ///     between multiple aggregation functions that may appear in the same EPL statement.
        ///     <para>
        ///         The runtime applies equals-semantics to determine state sharing. If
        ///         two <seealso cref="AggregationMultiFunctionStateKey" /> instances are equal (implement hashCode and equals)
        ///         then the runtime shares a single aggregation state instance for the two
        ///         aggregation function expressions.
        ///     </para>
        ///     <para>
        ///         If your aggregation function never needs shared state
        ///         simple return {@code new AggregationStateKey(){}}.
        ///     </para>
        ///     <para>
        ///         If your aggregation function always shares state
        ///         simple declare {@code private static final AggregationStateKey MY_KEY = new AggregationStateKey() {};}
        ///         and {@code return MY_KEY}; (if using multiple handlers declare the key on the factory level).
        ///     </para>
        /// </summary>
        /// <value>state key</value>
        AggregationMultiFunctionStateKey AggregationStateUniqueKey { get; }

        /// <summary>
        ///     Describes to the compiler how it should manage code for providing aggregation state.
        /// </summary>
        /// <value>mode object</value>
        AggregationMultiFunctionStateMode StateMode { get; }

        /// <summary>
        ///     Describes to the compiler how it should manage code for providing aggregation accessors.
        /// </summary>
        /// <value>mode object</value>
        AggregationMultiFunctionAccessorMode AccessorMode { get; }

        /// <summary>
        ///     Describes to the compiler how it should manage code for providing aggregation agents.
        /// </summary>
        /// <value>mode object</value>
        AggregationMultiFunctionAgentMode AgentMode { get; }

        /// <summary>
        ///     Describes to the compiler how it should manage code for providing table column reader.
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns>mode object</returns>
        AggregationMultiFunctionAggregationMethodMode GetAggregationMethodMode(
            AggregationMultiFunctionAggregationMethodContext ctx);
    }
} // end of namespace