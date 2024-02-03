///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecPlanComputeHelper
    {
        public static FilterValueSetParam[][] ComputeFixedLength(
            FilterSpecPlanPath[] paths,
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            var valueList = new FilterValueSetParam[paths.Length][];
            for (var i = 0; i < paths.Length; i++) {
                var path = paths[i];
                valueList[i] = new FilterValueSetParam[path.Triplets.Length];
                PopulateValueSet(valueList[i], matchedEvents, path.Triplets, exprEvaluatorContext, filterEvalEnv);
            }

            return valueList;
        }

        internal static void PopulateValueSet(
            FilterValueSetParam[] valueList,
            MatchedEventMap matchedEvents,
            FilterSpecPlanPathTriplet[] triplets,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv)
        {
            var count = 0;
            foreach (var specParam in triplets) {
                var valueParam = specParam.Param.GetFilterValue(matchedEvents, exprEvaluatorContext, filterEvalEnv);
                valueList[count] = valueParam;
                count++;
            }
        }
    }
} // end of namespace