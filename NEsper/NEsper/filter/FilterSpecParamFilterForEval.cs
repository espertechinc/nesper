///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.pattern;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    public interface FilterSpecParamFilterForEval : MetaDefItem
    {
        /// <summary>
        /// Returns the filter value representing the endpoint.
        /// </summary>
        /// <param name="matchedEvents">is the prior results</param>
        /// <param name="exprEvaluatorContext">eval context</param>
        /// <returns>filter value</returns>
        object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext exprEvaluatorContext);
    }
} // end of namespace
