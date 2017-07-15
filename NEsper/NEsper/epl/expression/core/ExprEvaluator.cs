///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    ///     Interface for evaluating of an event tuple.
    /// </summary>
    public interface ExprEvaluator
    {
        /// <summary>
        /// Evaluate event tuple and return result.
        /// </summary>
        /// <param name="evaluateParams">The evaluate params.</param>
        /// <returns>
        /// evaluation result, a bool value for OR/AND-type evalution nodes.
        /// </returns>
        Object Evaluate(EvaluateParams evaluateParams);

        /// <summary>
        /// Evaluate event tuple and return result.
        /// </summary>
        /// <param name="eventsPerStream">The events per stream.</param>
        /// <param name="isNewData">if set to <c>true</c> [is new data].</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        /// <returns>
        /// evaluation result, a bool value for OR/AND-type evalution nodes.
        /// </returns>
        //Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        ///     Returns the type that the node's evaluate method returns an instance of.
        /// </summary>
        /// <value>The type of the return.</value>
        /// <returns>type returned when evaluated</returns>
        Type ReturnType { get; }
    }
}