///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onsplit
{
    /// <summary>
    ///     Handler for incoming events for split-stream syntax, encapsulates where-clause evaluation strategies.
    /// </summary>
    public interface RouteResultViewHandler
    {
        /// <summary>
        ///     Handle event.
        /// </summary>
        /// <param name="theEvent">to handle</param>
        /// <param name="exprEvaluatorContext">expression eval context</param>
        /// <returns>true if at least one match was found, false if not</returns>
        bool Handle(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext);
    }
} // end of namespace