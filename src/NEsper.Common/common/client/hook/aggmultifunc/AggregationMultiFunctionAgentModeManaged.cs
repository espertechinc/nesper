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
    /// Use this class to provide an agent wherein there is no need to write code that generates code,
    /// </summary>
    public class AggregationMultiFunctionAgentModeManaged : AggregationMultiFunctionAgentMode
    {
        private InjectionStrategy _injectionStrategyAggregationAgentFactory;

        public AggregationMultiFunctionAgentModeManaged()
        {
        }

        public AggregationMultiFunctionAgentModeManaged(InjectionStrategy injectionStrategyAggregationAgentFactory)
        {
            _injectionStrategyAggregationAgentFactory = injectionStrategyAggregationAgentFactory;
        }

        /// <summary>
        /// Returns the injection strategy for the aggregation agent factory
        /// </summary>
        /// <returns>strategy</returns>
        public InjectionStrategy InjectionStrategyAggregationAgentFactory {
            get => _injectionStrategyAggregationAgentFactory;
            set => _injectionStrategyAggregationAgentFactory = value;
        }

        /// <summary>
        /// Sets the injection strategy for the aggregation agent factory
        /// </summary>
        /// <param name="strategy">strategy</param>
        /// <returns>itself</returns>
        public AggregationMultiFunctionAgentModeManaged SetInjectionStrategyAggregationAgentFactory(
            InjectionStrategy strategy)
        {
            this._injectionStrategyAggregationAgentFactory = strategy;
            return this;
        }
    }
} // end of namespace