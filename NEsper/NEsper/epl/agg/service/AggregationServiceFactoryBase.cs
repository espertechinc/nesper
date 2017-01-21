///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// All aggregation services require evaluation nodes which supply the value to be 
    /// aggregated (summed, averaged, etc.) and aggregation state factories to make new 
    /// aggregation states.
    /// </summary>
    public abstract class AggregationServiceFactoryBase : AggregationServiceFactory
    {
        /// <summary>Evaluation nodes under. </summary>
        protected ExprEvaluator[] Evaluators;

        /// <summary>_prototype aggregation states and factories. </summary>
        protected AggregationMethodFactory[] Aggregators;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluators">are the child node of each aggregation function used for computing the value to be aggregated</param>
        /// <param name="aggregators">aggregation states/factories</param>
        protected AggregationServiceFactoryBase(ExprEvaluator[] evaluators, AggregationMethodFactory[] aggregators)
        {
            Evaluators = evaluators;
            Aggregators = aggregators;

            if (evaluators.Length != aggregators.Length)
            {
                throw new ArgumentException("Expected the same number of evaluates as computer prototypes");
            }
        }

        public abstract AggregationService MakeService(AgentInstanceContext agentInstanceContext, EngineImportService engineImportService, bool isSubquery, int? subqueryNumber);
    }
}