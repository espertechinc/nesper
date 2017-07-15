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
    public class ProxyExprEvaluator : ExprEvaluator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="com.espertech.esper.epl.expression.core.ProxyExprEvaluator" /> class.
        /// </summary>
        /// <param name="procEvaluate">The @delegate.</param>
        /// <param name="returnType">Type of the return.</param>
        public ProxyExprEvaluator(Func<EvaluateParams, Object> procEvaluate, Type returnType)
        {
            ProcEvaluate = procEvaluate;
            ReturnType = returnType;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="com.espertech.esper.epl.expression.core.ProxyExprEvaluator" /> class.
        /// </summary>
        /// <param name="procEvaluate">The @delegate.</param>
        public ProxyExprEvaluator(Func<EvaluateParams, Object> procEvaluate)
        {
            ProcEvaluate = procEvaluate;
            ReturnType = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="com.espertech.esper.epl.expression.core.ProxyExprEvaluator" /> class.
        /// </summary>
        public ProxyExprEvaluator()
        {
            ProcEvaluate = evaluateParams => null;
            ProcReturnType = () => null;
        }

        public Func<EvaluateParams, Object> ProcEvaluate { get; set; }

        /// <summary>
        /// Proxy method for handling the return type.
        /// </summary>
        public Func<Type> ProcReturnType { get; set; }

        /// <summary>
        ///     Returns the type that the node's evaluate method returns an instance of.
        /// </summary>
        /// <value></value>
        /// <returns>
        ///     type returned when evaluated
        /// </returns>
        public Type ReturnType
        {
            get { return ProcReturnType.Invoke(); }
            set { ProcReturnType = () => value; }
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
            return ProcEvaluate.Invoke(evaluateParams);
        }
    }
}
