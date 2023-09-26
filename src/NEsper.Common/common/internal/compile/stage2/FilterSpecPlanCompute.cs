///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    public interface FilterSpecPlanCompute
    {
        FilterValueSetParam[][] Compute(
            FilterSpecPlan plan,
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv);
    }
} // end of namespace