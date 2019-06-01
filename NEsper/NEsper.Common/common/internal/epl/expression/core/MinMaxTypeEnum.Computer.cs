///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public partial class MinMaxTypeEnum
    {
        /// <summary>
        ///     Executes child expression nodes and compares results.
        /// </summary>
        public interface Computer
        {
            /// <summary>
            ///     Executes child expression nodes and compares results, returning the min/max.
            /// </summary>
            /// <param name="eventsPerStream">events per stream</param>
            /// <param name="isNewData">true if new data</param>
            /// <param name="exprEvaluatorContext">expression evaluation context</param>
            /// <returns>result</returns>
            object Execute(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext);
        }
    }
}