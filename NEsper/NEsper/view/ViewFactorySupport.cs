///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.visitor;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Abstract base class for view factories that do not make re-useable views and
    /// that do not share view resources with expression nodes.
    /// </summary>
    public abstract class ViewFactorySupport : ViewFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public bool CanReuse(View view)
        {
            return false;
        }
    
        /// <summary>Validate the view parameter expression and evaluate the expression returning the result object. </summary>
        /// <param name="viewName">textual name of view</param>
        /// <param name="statementContext">context with statement services</param>
        /// <param name="expression">view expression parameter to validate</param>
        /// <returns>object result value of parameter expression</returns>
        /// <throws>ViewParameterException if the expressions fail to validate</throws>
        public static Object ValidateAndEvaluate(String viewName, StatementContext statementContext, ExprNode expression)
        {
            return ValidateAndEvaluateExpr(viewName, statementContext, expression, new StreamTypeServiceImpl(statementContext.EngineURI, false), 0);
        }
    
        /// <summary>Validate the view parameter expressions and evaluate the expressions returning the result object. </summary>
        /// <param name="viewName">textual name of view</param>
        /// <param name="statementContext">context with statement services</param>
        /// <param name="expressions">view expression parameter to validate</param>
        /// <returns>object result value of parameter expressions</returns>
        /// <throws>ViewParameterException if the expressions fail to validate</throws>
        public static IList<Object> ValidateAndEvaluate(String viewName, StatementContext statementContext, IList<ExprNode> expressions)
        {
            IList<Object> results = new List<Object>();
            var expressionNumber = 0;
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(statementContext.EngineURI, false);
            foreach (var expr in expressions)
            {
                var result = ValidateAndEvaluateExpr(viewName, statementContext, expr, streamTypeService, expressionNumber);
                results.Add(result);
                expressionNumber++;
            }
            return results;
        }
    
        /// <summary>Validate the view parameter expressions and return the validated expression for later execution. <para />Does not evaluate the expression. </summary>
        /// <param name="viewName">textual name of view</param>
        /// <param name="eventType">is the event type of the parent view or stream attached.</param>
        /// <param name="statementContext">context with statement services</param>
        /// <param name="expressions">view expression parameter to validate</param>
        /// <param name="allowConstantResult">true to indicate whether expressions that return a constantresult should be allowed; false to indicate that if an expression is known to return a constant result the expression is considered invalid </param>
        /// <returns>object result value of parameter expressions</returns>
        /// <throws>ViewParameterException if the expressions fail to validate</throws>
        public static ExprNode[] Validate(String viewName, EventType eventType, StatementContext statementContext, IList<ExprNode> expressions, bool allowConstantResult)
        {
            IList<ExprNode> results = new List<ExprNode>();
            var expressionNumber = 0;
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(eventType, null, false, statementContext.EngineURI);
            foreach (var expr in expressions)
            {
                var validated = ValidateExpr(viewName, statementContext, expr, streamTypeService, expressionNumber);
                results.Add(validated);
    
                if ((!allowConstantResult) && (validated.IsConstantResult))
                {
                    var message = "Invalid view parameter expression " + expressionNumber + GetViewDesc(viewName) + ", the expression returns a constant result value, are you sure?";
                    Log.Error(message);
                    throw new ViewParameterException(message);
                }
    
                expressionNumber++;
            }
            return results.ToArray();
        }
    
        /// <summary>Assert and throws an exception if the expression passed returns a non-constant value. </summary>
        /// <param name="viewName">textual name of view</param>
        /// <param name="expression">expression to check</param>
        /// <param name="index">number offset of expression in view parameters</param>
        /// <throws>ViewParameterException if assertion fails</throws>
        public static void AssertReturnsNonConstant(String viewName, ExprNode expression, int index)
        {
            if (expression.IsConstantResult)
            {
                var message = "Invalid view parameter expression " + index + GetViewDesc(viewName) + ", the expression returns a constant result value, are you sure?";
                Log.Error(message);
                throw new ViewParameterException(message);
            }
        }
    
        /// <summary>Assert and throws an exception if the expression uses event property values. </summary>
        /// <param name="viewName">textual name of view</param>
        /// <param name="expression">expression to check</param>
        /// <param name="index">number offset of expression in view parameters</param>
        /// <param name="exprEvaluatorContext">context for expression evaluation</param>
        /// <returns>expression evaluation value</returns>
        /// <throws>ViewParameterException if assertion fails</throws>
        public static Object EvaluateAssertNoProperties(String viewName, ExprNode expression, int index, ExprEvaluatorContext exprEvaluatorContext)
        {
            var visitor = new ExprNodeSummaryVisitor();
            expression.Accept(visitor);
            if (!visitor.IsPlain)
            {
                var message = "Invalid view parameter expression " + index + GetViewDesc(viewName) + ", " + visitor.Message + " are not allowed within the expression";
                throw new ViewParameterException(message);
            }
    
            return expression.ExprEvaluator.Evaluate(new EvaluateParams(null, false, exprEvaluatorContext));
        }
    
        public static Object ValidateAndEvaluateExpr(String viewName, StatementContext statementContext, ExprNode expression, StreamTypeService streamTypeService, int expressionNumber)
        {
            var validated = ValidateExpr(viewName, statementContext, expression, streamTypeService, expressionNumber);
    
            try
            {
                return validated.ExprEvaluator.Evaluate(new EvaluateParams(null, true, new ExprEvaluatorContextStatement(statementContext, false)));
            }
            catch (Exception ex)
            {
                var message = "Failed to evaluate parameter expression " + expressionNumber + GetViewDesc(viewName);
                if (ex.Message != null)
                {
                    message += ": " + ex.Message;
                }
                Log.Error(message, ex);
                throw new ViewParameterException(message, ex);
            }
        }
    
        public static ExprNode ValidateExpr(String viewName, StatementContext statementContext, ExprNode expression, StreamTypeService streamTypeService, int expressionNumber)
        {
            ExprNode validated;
            try
            {
                var exprEvaluatorContext = new ExprEvaluatorContextStatement(statementContext, false);
                var validationContext = new ExprValidationContext(
                    streamTypeService, statementContext.MethodResolutionService, null,
                    statementContext.SchedulingService, statementContext.VariableService,
                    statementContext.TableService, exprEvaluatorContext,
                    statementContext.EventAdapterService, statementContext.StatementName,
                    statementContext.StatementId, statementContext.Annotations,
                    statementContext.ContextDescriptor, statementContext.ScriptingService,
                    false, false, false, false, null, false);
                validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.VIEWPARAMETER, expression, validationContext);
            }
            catch (ExprValidationException ex)
            {
                var message = "Invalid parameter expression " + expressionNumber + GetViewDesc(viewName);
                if (ex.Message != null)
                {
                    message += ": " + ex.Message;
                }
                Log.Error(message, ex);
                throw new ViewParameterException(message, ex);
            }
            return validated;
        }
    
        private static String GetViewDesc(String viewName)
        {
            return " for " + viewName + " view";
        }

        public static String GetViewParamMessageNumericOrTimePeriod(String viewName)
        {
            return viewName + " view requires a single numeric or time period parameter";
        }

        public abstract void SetViewParameters(ViewFactoryContext viewFactoryContext, IList<ExprNode> viewParameters);

        public abstract void Attach(
            EventType parentEventType,
            StatementContext statementContext,
            ViewFactory optionalParentFactory,
            IList<ViewFactory> parentViewFactories);

        public abstract View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext);
        public abstract string ViewName { get; }
        public abstract EventType EventType { get; }
    }
}
