///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.pattern;
using com.espertech.esper.util;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Denotes a value for use by the in-keyword within a list of values
    /// </summary>
    public interface FilterSpecParamInValue : MetaDefItem
    {
        /// <summary>
        /// Returns the actual value to filter for from prior matching events
        /// </summary>
        /// <param name="matchedEvents">is a map of matching events</param>
        /// <param name="evaluatorContext">The evaluator context.</param>
        /// <returns>filter-for value</returns>
        Object GetFilterValue(MatchedEventMap matchedEvents, ExprEvaluatorContext evaluatorContext);
    }
}
