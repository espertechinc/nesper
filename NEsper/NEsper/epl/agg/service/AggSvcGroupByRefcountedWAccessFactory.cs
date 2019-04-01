///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByRefcountedWAccessFactory : AggregationServiceFactoryBase
    {
        private readonly AggregationAccessorSlotPair[] _accessors;
        private readonly AggregationStateFactory[] _accessAggregations;
        private readonly bool _isJoin;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluators">- evaluate the sub-expression within the aggregate function (ie. Sum(4*myNum))</param>
        /// <param name="prototypes">
        /// - collect the aggregation state that evaluators evaluate to, act as prototypes for new aggregations
        /// aggregation states for each group
        /// </param>
        /// <param name="accessors">accessor definitions</param>
        /// <param name="accessAggregations">access aggs</param>
        /// <param name="isJoin">true for join, false for single-stream</param>
        public AggSvcGroupByRefcountedWAccessFactory(
            ExprEvaluator[] evaluators,
            AggregationMethodFactory[] prototypes,
            AggregationAccessorSlotPair[] accessors,
            AggregationStateFactory[] accessAggregations,
            bool isJoin)
            : base(evaluators, prototypes)
        {
            _accessors = accessors;
            _accessAggregations = accessAggregations;
            _isJoin = isJoin;
        }

        public override AggregationService MakeService(
            AgentInstanceContext agentInstanceContext,
            EngineImportService engineImportService,
            bool isSubquery,
            int? subqueryNumber)
        {
            return new AggSvcGroupByRefcountedWAccessImpl(Evaluators, Aggregators, _accessors, _accessAggregations, _isJoin);
        }
    }
} // end of namespace
