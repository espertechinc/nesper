///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecPlanComputeConditionalTriplets : FilterSpecPlanComputeConditional
    {
        public static readonly FilterSpecPlanComputeConditionalTriplets INSTANCE =
            new FilterSpecPlanComputeConditionalTriplets();

        protected override FilterValueSetParam[][] Compute(
            EventBean[] eventsPerStream,
            FilterSpecPlan plan,
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            if (plan.FilterNegate != null) {
                var controlResult = (bool?)plan.FilterNegate.Evaluate(
                    eventsPerStream,
                    true,
                    exprEvaluatorContext);
                if (controlResult == null || false.Equals(controlResult)) {
                    return null;
                }
            }

            if (plan.FilterConfirm != null) {
                var controlResult = (bool?)plan.FilterConfirm.Evaluate(
                    eventsPerStream,
                    true,
                    exprEvaluatorContext);
                if (controlResult != null && true.Equals(controlResult)) {
                    return FilterValueSetParamConstants.EMPTY;
                }
            }

            return ComputePathsWithNegate(eventsPerStream, plan, matchedEvents, exprEvaluatorContext, filterEvalEnv);
        }

        private FilterValueSetParam[][] ComputePathsWithNegate(
            EventBean[] eventsPerStream,
            FilterSpecPlan plan,
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            var paths = plan.Paths;
            var pathList = new List<FilterValueSetParam[]>(paths.Length);
            foreach (var path in paths) {
                var pass = true;
                if (path.PathNegate != null) {
                    var controlResult = (bool?)path.PathNegate.Evaluate(
                        eventsPerStream,
                        true,
                        exprEvaluatorContext);
                    if (controlResult == null || false.Equals(controlResult)) {
                        pass = false;
                    }
                }

                if (pass) {
                    var valueList = ComputeTriplets(
                        pathList,
                        path,
                        eventsPerStream,
                        matchedEvents,
                        exprEvaluatorContext,
                        filterEvalEnv);
                    pathList.Add(valueList);
                }
            }

            if (pathList.IsEmpty()) {
                return null; // all path negated
            }

            return pathList.ToArray();
        }

        private FilterValueSetParam[] ComputeTriplets(
            IList<FilterValueSetParam[]> pathList,
            FilterSpecPlanPath path,
            EventBean[] eventsPerStream,
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            var triplets = path.Triplets;
            IList<FilterValueSetParam> valueList = new List<FilterValueSetParam>(triplets.Length);
            foreach (var triplet in triplets) {
                if (triplet.TripletConfirm != null) {
                    var controlResult = (bool?)triplet.TripletConfirm.Evaluate(
                        eventsPerStream,
                        true,
                        exprEvaluatorContext);
                    if (controlResult != null && true.Equals(controlResult)) {
                        continue;
                    }
                }

                var valueParam = triplet.Param.GetFilterValue(
                    matchedEvents,
                    exprEvaluatorContext,
                    filterEvalEnv);
                valueList.Add(valueParam);
            }

            return valueList.ToArray();
        }
    }
} // end of namespace