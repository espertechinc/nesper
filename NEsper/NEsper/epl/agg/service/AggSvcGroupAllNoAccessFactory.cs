///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation without any grouping (no group-by).
    /// </summary>
    public class AggSvcGroupAllNoAccessFactory : AggregationServiceFactoryBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AggSvcGroupAllNoAccessFactory"/> class.
        /// </summary>
        /// <param name="evaluators">are the child node of each aggregation function used for computing the value to be aggregated</param>
        /// <param name="aggregators">aggregation states/factories</param>
        public AggSvcGroupAllNoAccessFactory(ExprEvaluator[] evaluators, AggregationMethodFactory[] aggregators)
            : base(evaluators, aggregators)
        {
        }

        public override AggregationService MakeService(AgentInstanceContext agentInstanceContext, EngineImportService engineImportService, bool isSubquery, int? subqueryNumber)
        {
            AggregationMethod[] aggregatorsAgentInstance = AggSvcGroupByUtil.NewAggregators(
                base.Aggregators);
            return new AggSvcGroupAllNoAccessImpl(Evaluators, aggregatorsAgentInstance, Aggregators);
        }
    }
}