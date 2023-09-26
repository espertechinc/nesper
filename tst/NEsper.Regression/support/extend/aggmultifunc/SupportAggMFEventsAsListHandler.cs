///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.client.hook.forgeinject;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;

namespace com.espertech.esper.regressionlib.support.extend.aggmultifunc
{
    public class SupportAggMFEventsAsListHandler : AggregationMultiFunctionHandler
    {
        private static readonly AggregationMultiFunctionStateKey AGGREGATION_STATE_KEY =
            new InertAggregationMultiFunctionStateKey();

        public EPChainableType ReturnType => EPChainableTypeHelper.CollectionOfSingleValue(typeof(SupportBean));

        public AggregationMultiFunctionStateKey AggregationStateUniqueKey => AGGREGATION_STATE_KEY;

        public AggregationMultiFunctionStateMode StateMode =>
            new AggregationMultiFunctionStateModeManaged().SetInjectionStrategyAggregationStateFactory(
                new InjectionStrategyClassNewInstance(typeof(SupportAggMFEventsAsListStateFactory)));

        public AggregationMultiFunctionAccessorMode AccessorMode =>
            new AggregationMultiFunctionAccessorModeManaged().SetInjectionStrategyAggregationAccessorFactory(
                new InjectionStrategyClassNewInstance(typeof(SupportAggMFEventsAsListAccessorFactory)));

        public AggregationMultiFunctionAgentMode AgentMode =>
            new AggregationMultiFunctionAgentModeManaged().SetInjectionStrategyAggregationAgentFactory(
                new InjectionStrategyClassNewInstance(typeof(SupportAggMFEventsAsListAggregationAgentFactory)));

        public AggregationMultiFunctionAggregationMethodMode GetAggregationMethodMode(AggregationMultiFunctionAggregationMethodContext ctx)
        {
            throw new UnsupportedOperationException("Table-column-read not implemented");
        }
    }
} // end of namespace