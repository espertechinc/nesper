///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.forgeinject;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    /// Use this class to provide an accessor wherein there is no need to write code that generates code,
    /// </summary>
    public class AggregationMultiFunctionAccessorModeManaged : AggregationMultiFunctionAccessorMode
    {
        private InjectionStrategy _injectionStrategyAggregationAccessorFactory;

        public AggregationMultiFunctionAccessorModeManaged()
        {
        }

        public AggregationMultiFunctionAccessorModeManaged(
            InjectionStrategy injectionStrategyAggregationAccessorFactory)
        {
            _injectionStrategyAggregationAccessorFactory = injectionStrategyAggregationAccessorFactory;
        }

        /// <summary>
        /// Returns the injection strategy for the aggregation accessor factory
        /// </summary>
        /// <returns>strategy</returns>
        public InjectionStrategy InjectionStrategyAggregationAccessorFactory {
            get => _injectionStrategyAggregationAccessorFactory;
            set => _injectionStrategyAggregationAccessorFactory = value;
        }

        /// <summary>
        /// Sets the injection strategy for the aggregation accessor factory
        /// </summary>
        /// <param name="strategy">strategy</param>
        /// <returns>itself</returns>
        public AggregationMultiFunctionAccessorModeManaged WithInjectionStrategyAggregationAccessorFactory(
            InjectionStrategy strategy)
        {
            _injectionStrategyAggregationAccessorFactory = strategy;
            return this;
        }
    }
} // end of namespace