///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public partial class ExprCastNode
    {
        /// <summary>
        ///     Casting and parsing computer.
        /// </summary>
        public interface CasterParserComputer
        {
            /// <summary>
            ///     Compute an result performing casting and parsing.
            /// </summary>
            /// <param name="input">to process</param>
            /// <param name="eventsPerStream">events per stream</param>
            /// <param name="newData">new data indicator</param>
            /// <param name="exprEvaluatorContext">evaluation context</param>
            /// <returns>cast or parse result</returns>
            object Compute(
                object input,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext exprEvaluatorContext);
        }
    }
}