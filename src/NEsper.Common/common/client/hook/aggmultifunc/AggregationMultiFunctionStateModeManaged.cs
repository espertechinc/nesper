///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.forgeinject;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    ///     Use this class to provide an state factory wherein there is no need to write code that generates code,
    /// </summary>
    public class AggregationMultiFunctionStateModeManaged : AggregationMultiFunctionStateMode
    {
        public AggregationMultiFunctionStateModeManaged()
        {
        }

        public AggregationMultiFunctionStateModeManaged(InjectionStrategy injectionStrategyAggregationStateFactory)
        {
            InjectionStrategyAggregationStateFactory = injectionStrategyAggregationStateFactory;
        }

        /// <summary>
        ///     Returns the injection strategy for the aggregation state factory
        /// </summary>
        /// <returns>strategy</returns>
        public InjectionStrategy InjectionStrategyAggregationStateFactory { get; set; }

        /// <summary>
        ///     Returns indicator whether a serializer-deserialize to provide read and write methods is provided by
        ///     <seealso cref = "Serde"/>
        /// </summary>
        /// <value>ha-indicator</value>
        public bool HasHA { get; set; }

        /// <summary>
        ///     Returns the class providing the serde
        /// </summary>
        /// <returns>serde class</returns>
        public Type Serde { get; set; }

        public AggregationMultiFunctionStateModeManaged WithInjectionStrategyAggregationStateFactory(
            InjectionStrategy injectionStrategy)
        {
            InjectionStrategyAggregationStateFactory = injectionStrategy;
            return this;
        }
    }
}