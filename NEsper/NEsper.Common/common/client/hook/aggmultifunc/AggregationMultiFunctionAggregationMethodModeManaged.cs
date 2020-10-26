///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.forgeinject;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
	/// <summary>
	/// Use this class to provide a aggregation method wherein there is no need to write code that generates code,
	/// </summary>
	public class AggregationMultiFunctionAggregationMethodModeManaged : AggregationMultiFunctionAggregationMethodMode
	{
		private InjectionStrategy _injectionStrategyAggregationMethodFactory;

		public AggregationMultiFunctionAggregationMethodModeManaged()
		{
		}

		public AggregationMultiFunctionAggregationMethodModeManaged(InjectionStrategy injectionStrategyAggregationMethodFactory)
		{
			this._injectionStrategyAggregationMethodFactory = injectionStrategyAggregationMethodFactory;
		}

		/// <summary>
		/// Returns the injection strategy for the aggregation table reader factory
		/// </summary>
		/// <value>strategy</value>
		public InjectionStrategy InjectionStrategyAggregationMethodFactory {
			get => _injectionStrategyAggregationMethodFactory;
			set => _injectionStrategyAggregationMethodFactory = value;
		}

		/// <summary>
		/// Sets the injection strategy for the aggregation table reader factory
		/// </summary>
		/// <param name="strategy">strategy</param>
		/// <returns>itself</returns>
		public AggregationMultiFunctionAggregationMethodModeManaged SetInjectionStrategyAggregationMethodFactory(InjectionStrategy strategy)
		{
			this._injectionStrategyAggregationMethodFactory = strategy;
			return this;
		}
	}
} // end of namespace
