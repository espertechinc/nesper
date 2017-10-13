///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation without any grouping (no group-by).
    /// </summary>
    public class AggSvcGroupAllMixedAccessFactory : AggregationServiceFactoryBase
    {
        protected readonly AggregationAccessorSlotPair[] Accessors;
        protected readonly AggregationStateFactory[] AccessAggregations;
        protected readonly bool IsJoin;

        public AggSvcGroupAllMixedAccessFactory(ExprEvaluator[] evaluators, AggregationMethodFactory[] aggregators, AggregationAccessorSlotPair[] accessors, AggregationStateFactory[] accessAggregations, bool @join)
            : base(evaluators, aggregators)
        {
            Accessors = accessors;
            AccessAggregations = accessAggregations;
            IsJoin = join;
        }

        public override AggregationService MakeService(AgentInstanceContext agentInstanceContext, EngineImportService engineImportService, bool isSubquery, int? subqueryNumber)
        {
            AggregationState[] states = AggSvcGroupByUtil.NewAccesses(
                agentInstanceContext.AgentInstanceId, IsJoin, AccessAggregations, null, null);
            AggregationMethod[] aggregatorsAgentInstance = AggSvcGroupByUtil.NewAggregators(
                base.Aggregators);
            return new AggSvcGroupAllMixedAccessImpl(
                Evaluators, aggregatorsAgentInstance, Accessors, states, base.Aggregators, AccessAggregations);
        }
    }
}