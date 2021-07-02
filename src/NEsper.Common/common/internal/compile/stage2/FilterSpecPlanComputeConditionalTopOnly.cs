///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;

using static com.espertech.esper.common.@internal.compile.stage2.FilterSpecPlanComputeHelper;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecPlanComputeConditionalTopOnly : FilterSpecPlanComputeConditional
    {
        public static readonly FilterSpecPlanComputeConditionalTopOnly INSTANCE = new FilterSpecPlanComputeConditionalTopOnly();

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

            return ComputeFixedLength(plan.Paths, matchedEvents, exprEvaluatorContext, filterEvalEnv);
        }
    }
} // end of namespace