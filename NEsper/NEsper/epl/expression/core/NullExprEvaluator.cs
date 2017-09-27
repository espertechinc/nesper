///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.expression.core
{
    public class NullExprEvaluator : ExprEvaluator
    {
        /// <summary>
        ///     Returns the type that the node's evaluate method returns an instance of.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///     type returned when evaluated
        /// </returns>
        public Type ReturnType
        {
            get { return null; }
        }

        /// <summary>
        /// Evaluate event tuple and return result.
        /// </summary>
        /// <param name="evaluateParams">The evaluate params.</param>
        /// <returns>
        /// evaluation result, a bool value for OR/AND-type evalution nodes.
        /// </returns>
        public object Evaluate(EvaluateParams evaluateParams)
        {
            return null;
        }
    }
}
