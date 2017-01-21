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
        ///     Returns the type that the node's evaluate method returns an instance of.
        /// </summary>
        /// <value>The type of the return.</value>
        /// <returns>type returned when evaluated</returns>
        Type ReturnType { get; }
    }

    public class EvaluateParams
    {
        private readonly EventBean[] _eventsPerStream;

        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly bool _isNewData;


        public EvaluateParams(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            _eventsPerStream = eventsPerStream;

            _isNewData = isNewData;

            _exprEvaluatorContext = exprEvaluatorContext;
        }

        public EventBean[] EventsPerStream
        {
            get { return _eventsPerStream; }
        }

        public bool IsNewData
        {
            get { return _isNewData; }
        }

        public ExprEvaluatorContext ExprEvaluatorContext
        {
            get { return _exprEvaluatorContext; }
        }

        public static readonly EvaluateParams Empty = new EvaluateParams(null, false, null);
    }

    public delegate Object ExprEvaluatorDelegate(EvaluateParams evaluateParams);


    public class ProxyExprEvaluator : ExprEvaluator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyExprEvaluator" /> class.
        /// </summary>
        /// <param name="procEvaluate">The @delegate.</param>
        /// <param name="returnType">Type of the return.</param>
        public ProxyExprEvaluator(ExprEvaluatorDelegate procEvaluate, Type returnType)
        {
            ProcEvaluate = procEvaluate;
            ReturnType = returnType;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyExprEvaluator" /> class.
        /// </summary>
        /// <param name="procEvaluate">The @delegate.</param>
        public ProxyExprEvaluator(ExprEvaluatorDelegate procEvaluate)
        {
            ProcEvaluate = procEvaluate;
            ReturnType = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProxyExprEvaluator" /> class.
        /// </summary>
        public ProxyExprEvaluator()
        {
            ProcEvaluate = evaluateParams => null;
            ProcReturnType = () => null;
        }

        public ExprEvaluatorDelegate ProcEvaluate { get; set; }

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