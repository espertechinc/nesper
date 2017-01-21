///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.join.@base
{
    /// <summary>
    /// Processes a join result set constisting of sets of tuples of events.
    /// </summary>
    public interface JoinSetProcessor
    {
        /// <summary>Process join result set. </summary>
        /// <param name="newEvents">set of event tuples representing new data</param>
        /// <param name="oldEvents">set of event tuples representing old data</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        void Process(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, ExprEvaluatorContext exprEvaluatorContext);
    }
}
