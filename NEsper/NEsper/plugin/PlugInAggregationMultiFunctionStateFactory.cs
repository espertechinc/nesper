///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.agg.access;

namespace com.espertech.esper.plugin
{
    /// <summary>State factory responsible for allocating a state object for each group when used with group-by. </summary>
    public interface PlugInAggregationMultiFunctionStateFactory {
        /// <summary>Return a new aggregation state holder for a given group (or ungrouped if there is no group-by). </summary>
        /// <param name="stateContext">context includes group key</param>
        /// <returns>state holder, cannot be a null value</returns>
        AggregationState MakeAggregationState(PlugInAggregationMultiFunctionStateContext stateContext);
    }

    public class ProxyPlugInAggregationMultiFunctionStateFactory : PlugInAggregationMultiFunctionStateFactory
    {
        public Func<PlugInAggregationMultiFunctionStateContext, AggregationState> ProcMakeAggregationState;

        public AggregationState MakeAggregationState(PlugInAggregationMultiFunctionStateContext stateContext)
        {
            return ProcMakeAggregationState.Invoke(stateContext);
        }
    }
}
