///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    public interface FilterSpecParamFilterForEval
    {
        /// <summary>
        ///     Returns the filter value representing the endpoint.
        /// </summary>
        /// <param name="matchedEvents">is the prior results</param>
        /// <param name="exprEvaluatorContext">eval context</param>
        /// <param name="filterEvalEnv">env</param>
        /// <returns>filter value</returns>
        object GetFilterValue(
            MatchedEventMap matchedEvents,
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextFilterEvalEnv filterEvalEnv);
    }
} // end of namespace