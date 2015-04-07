///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByNoAccessFactory : AggregationServiceFactoryBase
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluators">evaluate the sub-expression within the aggregate function (ie. Sum(4*myNum))</param>
        /// <param name="prototypes">collect the aggregation state that evaluators evaluate to, act as prototypes for new aggregationsaggregation states for each group</param>
        /// <param name="groupKeyBinding">The group key binding.</param>
        public AggSvcGroupByNoAccessFactory(
            ExprEvaluator[] evaluators,
            AggregationMethodFactory[] prototypes,
            Object groupKeyBinding)
            : base(evaluators, prototypes, groupKeyBinding)
        {
        }

        public override AggregationService MakeService(
            AgentInstanceContext agentInstanceContext,
            MethodResolutionService methodResolutionService)
        {
            return new AggSvcGroupByNoAccessImpl(Evaluators, Aggregators, GroupKeyBinding, methodResolutionService);
        }
    }
}