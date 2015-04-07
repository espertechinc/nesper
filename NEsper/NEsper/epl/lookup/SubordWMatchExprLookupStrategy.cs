///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// A lookup strategy that receives an additional match expression.
    /// </summary>
    public interface SubordWMatchExprLookupStrategy
    {
        /// <summary>
        /// Determines the events.
        /// </summary>
        /// <param name="newData">is the correlation events</param>
        /// <returns>the events</returns>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        EventBean[] Lookup(EventBean[] newData, ExprEvaluatorContext exprEvaluatorContext);
    
        string ToQueryPlan();
    }
}
