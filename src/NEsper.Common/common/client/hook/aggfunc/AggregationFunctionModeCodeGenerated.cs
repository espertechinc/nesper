///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.method.core;

namespace com.espertech.esper.common.client.hook.aggfunc
{
    /// <summary>
    /// Use this class to provide a fully code-generated aggregation function by implementing the internal
    /// <seealso cref="AggregatorMethodFactory" /> interface.
    /// </summary>
    public class AggregationFunctionModeCodeGenerated : AggregationFunctionMode
    {
        private AggregatorMethodFactory aggregatorMethodFactory;

        /// <summary>
        /// Returns the aggregation method factory.
        /// </summary>
        /// <returns>factory</returns>
        public AggregatorMethodFactory AggregatorMethodFactory => aggregatorMethodFactory;

        /// <summary>
        /// Sets the aggregation method factory
        /// </summary>
        /// <param name="aggregatorMethodFactory">factory</param>
        /// <returns>itself</returns>
        public AggregationFunctionModeCodeGenerated SetAggregatorMethodFactory(
            AggregatorMethodFactory aggregatorMethodFactory)
        {
            this.aggregatorMethodFactory = aggregatorMethodFactory;
            return this;
        }
    }
} // end of namespace