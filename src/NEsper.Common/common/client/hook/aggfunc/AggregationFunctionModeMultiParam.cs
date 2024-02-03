///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.forgeinject;

namespace com.espertech.esper.common.client.hook.aggfunc
{
    /// <summary>
    ///     Use this class to provide an aggregation function wherein there is no need to write code that generates code,
    ///     wherein the aggregation function takes a multiple parameters and
    ///     wherein the compiler does not handle filtering and distinct.
    /// </summary>
    public class AggregationFunctionModeMultiParam : AggregationFunctionMode
    {
        /// <summary>
        ///     Returns the injection strategy for the aggregation function factory
        /// </summary>
        /// <returns>strategy</returns>
        public InjectionStrategy InjectionStrategyAggregationFunctionFactory { get; set; }

        /// <summary>
        ///     Returns indicator whether a serializer-deserialize to provide read and write methods is provided by
        ///     <seealso cref="Serde" />
        /// </summary>
        /// <value>ha-indicator</value>
        public bool HasHA { get; private set; }

        /// <summary>
        ///     Returns the class providing the serde
        /// </summary>
        /// <returns>serde class</returns>
        public Type Serde { get; private set; }

        /// <summary>
        ///     Sets the injection strategy for the aggregation function factory
        /// </summary>
        /// <param name="strategy">strategy</param>
        /// <returns>itself</returns>
        public AggregationFunctionModeMultiParam SetInjectionStrategyAggregationFunctionFactory(
            InjectionStrategy strategy)
        {
            InjectionStrategyAggregationFunctionFactory = strategy;
            return this;
        }

        /// <summary>
        ///     Sets indicator whether a serializer-deserialize to provide read and write methods is provided by
        ///     <seealso cref="Serde" />
        /// </summary>
        /// <param name="hasHA">ha-indicator</param>
        /// <returns>itself</returns>
        public AggregationFunctionModeMultiParam SetHasHA(bool hasHA)
        {
            HasHA = hasHA;
            return this;
        }

        /// <summary>
        ///     Sets the class providing the serde
        /// </summary>
        /// <param name="serde">serde class</param>
        /// <returns>itself</returns>
        public AggregationFunctionModeMultiParam SetSerde(Type serde)
        {
            Serde = serde;
            return this;
        }
    }
} // end of namespace