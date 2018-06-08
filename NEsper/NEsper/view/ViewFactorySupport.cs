///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;
using com.espertech.esper.util;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Abstract base class for view factories that do not make re-useable views and that do
    /// not share view resources with expression nodes.
    /// </summary>
    public abstract class ViewFactorySupport : ViewFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Validate the view parameter expression and evaluate the expression returning the result object.
        /// </summary>
        /// <param name="viewName">textual name of view</param>
        /// <param name="statementContext">context with statement services</param>
        /// <param name="expression">view expression parameter to validate</param>
        /// <exception cref="ViewParameterException">if the expressions fail to validate</exception>
        /// <returns>object result value of parameter expression</returns>
        public static Object ValidateAndEvaluate(
            string viewName,
            StatementContext statementContext,
            ExprNode expression)
        {
            return ValidateAndEvaluateExpr(
                viewName, statementContext, expression, new StreamTypeServiceImpl(statementContext.EngineURI, false), 0);
        }

        public static ExprNode[] Validate(
            string viewName,
            StatementContext statementContext,
            IList<ExprNode> expressions)
        {
            var results = new ExprNode[expressions.Count];
            int expressionNumber = 0;
            var streamTypeService = new StreamTypeServiceImpl(statementContext.EngineURI, false);
            foreach (ExprNode expr in expressions)
            {
                results[expressionNumber] = ValidateExpr(
                    viewName, statementContext, expr, streamTypeService, expressionNumber);
                expressionNumber++;
            }
            return results;
        }

        /// <summary>
        /// Validate the view parameter expressions and return the validated expression for later execution.
        /// <para>
        /// Does not evaluate the expression.
        /// </para>
        /// </summary>
        /// <param name="viewName">textual name of view</param>
        /// <param name="eventType">is the event type of the parent view or stream attached.</param>
        /// <param name="statementContext">context with statement services</param>
        /// <param name="expressions">view expression parameter to validate</param>
        /// <param name="allowConstantResult">
        /// true to indicate whether expressions that return a constant
        /// result should be allowed; false to indicate that if an expression is known to return a constant result
        /// the expression is considered invalid
        /// </param>
        /// <exception cref="ViewParameterException">if the expressions fail to validate</exception>
        /// <returns>object result value of parameter expressions</returns>
        public static ExprNode[] Validate(
            string viewName,
            EventType eventType,
            StatementContext statementContext,
            IList<ExprNode> expressions,
            bool allowConstantResult)
        {
            var results = new List<ExprNode>();
            int expressionNumber = 0;
            var streamTypeService = new StreamTypeServiceImpl(eventType, null, false, statementContext.EngineURI);
            foreach (ExprNode expr in expressions)
            {
                ExprNode validated = ValidateExpr(viewName, statementContext, expr, streamTypeService, expressionNumber);
                results.Add(validated);

                if ((!allowConstantResult) && (validated.IsConstantResult))
                {
                    string message = "Invalid view parameter expression " + expressionNumber + GetViewDesc(viewName) +
                                     ", the expression returns a constant result value, are you sure?";
                    Log.Error(message);
                    throw new ViewParameterException(message);
                }

                expressionNumber++;
            }
            return results.ToArray();
        }

        /// <summary>
        /// Assert and throws an exception if the expression passed returns a non-constant value.
        /// </summary>
        /// <param name="viewName">textual name of view</param>
        /// <param name="expression">expression to check</param>
        /// <param name="index">number offset of expression in view parameters</param>
        /// <exception cref="ViewParameterException">if assertion fails</exception>
        public static void AssertReturnsNonConstant(string viewName, ExprNode expression, int index)
        {
            if (expression.IsConstantResult)
            {
                string message = "Invalid view parameter expression " + index + GetViewDesc(viewName) +
                                 ", the expression returns a constant result value, are you sure?";
                Log.Error(message);
                throw new ViewParameterException(message);
            }
        }

        public static Object EvaluateAssertNoProperties(
            string viewName,
            ExprNode expression,
            int index,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ValidateNoProperties(viewName, expression, index);
            return expression.ExprEvaluator.Evaluate(new EvaluateParams(null, false, exprEvaluatorContext));
        }

        public static void ValidateNoProperties(string viewName, ExprNode expression, int index)
        {
            var visitor = new ExprNodeSummaryVisitor();
            expression.Accept(visitor);
            if (!visitor.IsPlain)
            {
                string message = "Invalid view parameter expression " + index + GetViewDesc(viewName) + ", " +
                                 visitor.GetMessage() + " are not allowed within the expression";
                throw new ViewParameterException(message);
            }
        }

        public static Object ValidateAndEvaluateExpr(
            string viewName,
            StatementContext statementContext,
            ExprNode expression,
            StreamTypeService streamTypeService,
            int expressionNumber)
        {
            ExprNode validated = ValidateExpr(
                viewName, statementContext, expression, streamTypeService, expressionNumber);

            try
            {
                return validated.ExprEvaluator.Evaluate(
                    new EvaluateParams(null, true, new ExprEvaluatorContextStatement(statementContext, false)));
            }
            catch (Exception ex)
            {
                string message = "Failed to evaluate parameter expression " + expressionNumber + GetViewDesc(viewName);
                if (!string.IsNullOrWhiteSpace(ex.Message))
                {
                    message += ": " + ex.Message;
                }
                Log.Error(message, ex);
                throw new ViewParameterException(message, ex);
            }
        }

        public static Object Evaluate(
            ExprEvaluator evaluator,
            int expressionNumber,
            string viewName,
            StatementContext statementContext)
        {
            try
            {
                return evaluator.Evaluate(new EvaluateParams(null, true, new ExprEvaluatorContextStatement(statementContext, false)));
            }
            catch (Exception ex)
            {
                string message = "Failed to evaluate parameter expression " + expressionNumber + GetViewDesc(viewName);
                if (!string.IsNullOrWhiteSpace(ex.Message))
                {
                    message += ": " + ex.Message;
                }
                Log.Error(message, ex);
                throw new ViewParameterException(message, ex);
            }
        }

        public static ExprNode ValidateExpr(
            string viewName,
            StatementContext statementContext,
            ExprNode expression,
            StreamTypeService streamTypeService,
            int expressionNumber)
        {
            ExprNode validated;
            try
            {
                var exprEvaluatorContext = new ExprEvaluatorContextStatement(statementContext, false);
                var validationContext = new ExprValidationContext(
                    statementContext.Container,
                    streamTypeService,
                    statementContext.EngineImportService,
                    statementContext.StatementExtensionServicesContext, null,
                    statementContext.SchedulingService,
                    statementContext.VariableService,
                    statementContext.TableService, exprEvaluatorContext,
                    statementContext.EventAdapterService,
                    statementContext.StatementName,
                    statementContext.StatementId,
                    statementContext.Annotations,
                    statementContext.ContextDescriptor,
                    statementContext.ScriptingService,
                    false, false, false, false, null, false);
                validated = ExprNodeUtility.GetValidatedSubtree(
                    ExprNodeOrigin.VIEWPARAMETER, expression, validationContext);
            }
            catch (ExprValidationException ex)
            {
                string message = "Invalid parameter expression " + expressionNumber + GetViewDesc(viewName);
                if (!string.IsNullOrWhiteSpace(ex.Message))
                {
                    message += ": " + ex.Message;
                }
                Log.Error(message, ex);
                throw new ViewParameterException(message, ex);
            }
            return validated;
        }

        private static string GetViewDesc(string viewName)
        {
            return " for " + viewName + " view";
        }

        public static ExprEvaluator ValidateSizeSingleParam(
            string viewName,
            ViewFactoryContext viewFactoryContext,
            IList<ExprNode> expressionParameters)
        {
            ExprNode[] validated = ViewFactorySupport.Validate(
                viewName, viewFactoryContext.StatementContext, expressionParameters);
            if (validated.Length != 1)
            {
                throw new ViewParameterException(GetViewParamMessage(viewName));
            }
            return ValidateSizeParam(viewName, viewFactoryContext.StatementContext, validated[0], 0);
        }

        public static ExprEvaluator ValidateSizeParam(
            string viewName,
            StatementContext statementContext,
            ExprNode sizeNode,
            int expressionNumber)
        {
            var sizeEvaluator = sizeNode.ExprEvaluator;
            var returnType = sizeEvaluator.ReturnType.GetBoxedType();
            if (!returnType.IsNumeric() ||
                returnType.IsFloatingPointClass() ||
                returnType.IsBoxedType<long>())
            {
                throw new ViewParameterException(GetViewParamMessage(viewName));
            }
            if (sizeNode.IsConstantResult)
            {
                var size = ViewFactorySupport.Evaluate(sizeEvaluator, expressionNumber, viewName, statementContext);
                if (!ValidateSize(size))
                {
                    throw new ViewParameterException(GetSizeValidationMsg(viewName, size));
                }
            }
            return sizeEvaluator;
        }

        public static int EvaluateSizeParam(string viewName, ExprEvaluator sizeEvaluator, AgentInstanceContext context)
        {
            var size = sizeEvaluator.Evaluate(new EvaluateParams(null, true, context));
            if (!ValidateSize(size))
            {
                throw new EPException(GetSizeValidationMsg(viewName, size));
            }
            return size.AsInt();
        }

        private static bool ValidateSize(object size)
        {
            return !(size == null || size.AsInt() <= 0);
        }

        private static string GetViewParamMessage(string viewName)
        {
            return viewName + " view requires a single integer-type parameter";
        }

        private static string GetSizeValidationMsg(string viewName, object size)
        {
            return viewName + " view requires a positive integer for size but received " + size;
        }

        public static void ValidateNoParameters(string viewName, IList<ExprNode> expressionParameters)
        {
            if (!expressionParameters.IsEmpty())
            {
                string errorMessage = viewName + " view requires an empty parameter list";
                throw new ViewParameterException(errorMessage);
            }
        }

        public bool CanReuse(View view, AgentInstanceContext agentInstanceContext)
        {
            return false;
        }

        public abstract void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParameters);

        public abstract void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories);

        public abstract View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext);
        public abstract EventType EventType { get; }
        public abstract string ViewName { get; }
    }
} // end of namespace
