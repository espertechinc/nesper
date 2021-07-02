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
    /// <summary>
    ///     Interface for evaluating of an event tuple.
    /// </summary>
    public interface ExprEvaluator
    {
        /// <summary>
        ///     Evaluate event tuple and return result.
        /// </summary>
        /// <param name="eventsPerStream">event tuple</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="context">context for expression evaluation</param>
        /// <returns>evaluation result, a boolean value for OR/AND-type evaluation nodes.</returns>
        object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);
    }

    public class ProxyExprEvaluator : ExprEvaluator
    {
        public delegate object EvaluateFunc(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        public EvaluateFunc procEvaluate;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return procEvaluate(eventsPerStream, isNewData, context);
        }

        public ProxyExprEvaluator(EvaluateFunc procEvaluate)
        {
            this.procEvaluate = procEvaluate;
        }

        public ProxyExprEvaluator()
        {
        }
    }
} // end of namespace