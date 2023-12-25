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

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecPlanComputeHelper;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecPlanComputeConditionalPath : FilterSpecPlanComputeConditional
    {
        public static readonly FilterSpecPlanComputeConditionalPath INSTANCE =
            new FilterSpecPlanComputeConditionalPath();

        protected override FilterValueSetParam[][] Compute(
            EventBean[] eventsPerStream,
            FilterSpecPlan plan,
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            if (plan.FilterNegate != null) {
                var controlResult = plan.FilterNegate.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                if (controlResult == null || false.Equals(controlResult)) {
                    return null;
                }
            }

            if (plan.FilterConfirm != null) {
                var controlResult = plan.FilterConfirm.Evaluate(eventsPerStream, true, exprEvaluatorContext);
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
            IList<FilterValueSetParam[]> pathList = new List<FilterValueSetParam[]>(paths.Length);
            foreach (var path in paths) {
                if (path.PathNegate != null)
                {
                    var controlResult = path.PathNegate?.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                    if (controlResult == null || false.Equals(controlResult))
                    {
                        continue;
                    }
                }

                var triplets = path.Triplets;
                var valueList = new FilterValueSetParam[triplets.Length];
                PopulateValueSet(valueList, matchedEvents, path.Triplets, exprEvaluatorContext, filterEvalEnv);
                pathList.Add(valueList);
            }

            if (pathList.IsEmpty()) {
                return null; // all path negated
            }

            return pathList.ToArray();
        }
    }
} // end of namespace