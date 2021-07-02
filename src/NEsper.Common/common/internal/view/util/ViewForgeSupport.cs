///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.util
{
    public class ViewForgeSupport
    {
        public static object ValidateAndEvaluate(
            string viewName,
            ExprNode expression,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            return ValidateAndEvaluateExpr(
                viewName,
                expression,
                new StreamTypeServiceImpl(false),
                viewForgeEnv,
                0,
                streamNumber);
        }

        public static object EvaluateAssertNoProperties(
            string viewName,
            ExprNode expression,
            int index)
        {
            ValidateNoProperties(viewName, expression, index);
            return expression.Forge.ExprEvaluator.Evaluate(null, false, null);
        }

        /// <summary>
        ///     Assert and throws an exception if the expression passed returns a non-constant value.
        /// </summary>
        /// <param name="viewName">textual name of view</param>
        /// <param name="expression">expression to check</param>
        /// <param name="index">number offset of expression in view parameters</param>
        /// <throws>ViewParameterException if assertion fails</throws>
        public static void AssertReturnsNonConstant(
            string viewName,
            ExprNode expression,
            int index)
        {
            if (expression.Forge.ForgeConstantType.IsCompileTimeConstant) {
                var message = "Invalid view parameter expression " +
                              index +
                              GetViewDesc(viewName) +
                              ", the expression returns a constant result value, are you sure?";
                throw new ViewParameterException(message);
            }
        }

        public static void ValidateNoProperties(
            string viewName,
            ExprNode expression,
            int index)
        {
            var visitor = new ExprNodeSummaryVisitor();
            expression.Accept(visitor);
            if (!visitor.IsPlain) {
                var message = "Invalid view parameter expression " +
                              index +
                              GetViewDesc(viewName) +
                              ", " +
                              visitor.Message +
                              " are not allowed within the expression";
                throw new ViewParameterException(message);
            }
        }

        public static object ValidateAndEvaluateExpr(
            string viewName,
            ExprNode expression,
            StreamTypeService streamTypeService,
            ViewForgeEnv viewForgeEnv,
            int expressionNumber,
            int streamNumber)
        {
            var validated = ValidateExpr(
                viewName,
                expression,
                streamTypeService,
                viewForgeEnv,
                expressionNumber,
                streamNumber);

            try {
                return validated.Forge.ExprEvaluator.Evaluate(null, true, null);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                var message = "Failed to evaluate parameter expression " + expressionNumber + GetViewDesc(viewName);
                if (ex.Message != null) {
                    message += ": " + ex.Message;
                }

                throw new ViewParameterException(message, ex);
            }
        }

        public static ExprForge ValidateSizeSingleParam(
            string viewName,
            IList<ExprNode> expressionParameters,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            var validated = Validate(viewName, expressionParameters, viewForgeEnv, streamNumber);
            if (validated.Length != 1) {
                throw new ViewParameterException(GetViewParamMessage(viewName));
            }

            return ValidateSizeParam(viewName, validated[0], 0);
        }

        public static ExprForge ValidateSizeParam(
            string viewName,
            ExprNode sizeNode,
            int expressionNumber)
        {
            var forge = sizeNode.Forge;
            
            var sizeType = sizeNode.Forge.EvaluationType;
            if (sizeType.IsNullTypeSafe()) {
                throw new ViewParameterException(GetViewParamMessage(viewName));
            }
            
            var sizeTypeBoxed = sizeType.GetBoxedType();
            if (!sizeTypeBoxed.IsNumeric() || sizeTypeBoxed.IsFloatingPointClass() || sizeTypeBoxed.IsInt64()) {
                throw new ViewParameterException(GetViewParamMessage(viewName));
            }

            if (sizeNode.Forge.ForgeConstantType.IsCompileTimeConstant) {
                var size = Evaluate(forge.ExprEvaluator, expressionNumber, viewName);
                if (!size.IsNumber()) {
                    throw new IllegalStateException(nameof(size) + " is not a number");
                }

                if (!ValidateSize(size)) {
                    throw new ViewParameterException(GetSizeValidationMsg(viewName, size));
                }
            }

            return forge;
        }

        /// <summary>
        ///     Validate the view parameter expressions and return the validated expression for later execution.
        ///     <para />
        ///     Does not evaluate the expression.
        /// </summary>
        /// <param name="viewName">textual name of view</param>
        /// <param name="eventType">is the event type of the parent view or stream attached.</param>
        /// <param name="expressions">view expression parameter to validate</param>
        /// <param name="allowConstantResult">
        ///     true to indicate whether expressions that return a constantresult should be allowed; false to indicate that if an
        ///     expression is known to return a constant result
        ///     the expression is considered invalid
        /// </param>
        /// <param name="streamNumber">stream number</param>
        /// <param name="viewForgeEnv">view forge env</param>
        /// <returns>object result value of parameter expressions</returns>
        /// <throws>ViewParameterException if the expressions fail to validate</throws>
        public static ExprNode[] Validate(
            string viewName,
            EventType eventType,
            IList<ExprNode> expressions,
            bool allowConstantResult,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            IList<ExprNode> results = new List<ExprNode>();
            var expressionNumber = 0;
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(eventType, null, false);
            foreach (var expr in expressions) {
                var validated = ValidateExpr(
                    viewName,
                    expr,
                    streamTypeService,
                    viewForgeEnv,
                    expressionNumber,
                    streamNumber);
                results.Add(validated);

                if (!allowConstantResult && validated.Forge.ForgeConstantType.IsCompileTimeConstant) {
                    var message = "Invalid view parameter expression " +
                                  expressionNumber +
                                  GetViewDesc(viewName) +
                                  ", the expression returns a constant result value, are you sure?";
                    throw new ViewParameterException(message);
                }

                expressionNumber++;
            }

            return results.ToArray();
        }

        public static ExprNode[] Validate(
            string viewName,
            IList<ExprNode> expressions,
            ViewForgeEnv viewForgeEnv,
            int streamNumber)
        {
            var results = new ExprNode[expressions.Count];
            var expressionNumber = 0;
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(false);
            foreach (var expr in expressions) {
                results[expressionNumber] = ValidateExpr(
                    viewName,
                    expr,
                    streamTypeService,
                    viewForgeEnv,
                    expressionNumber,
                    streamNumber);
                expressionNumber++;
            }

            return results;
        }

        public static ExprNode ValidateExpr(
            string viewName,
            ExprNode expression,
            StreamTypeService streamTypeService,
            ViewForgeEnv viewForgeEnv,
            int expressionNumber,
            int streamNumber)
        {
            ExprNode validated;
            try {
                var names = new ExprValidationMemberNameQualifiedView(streamNumber);
                var validationContext = new ExprValidationContextBuilder(
                        streamTypeService,
                        viewForgeEnv.StatementRawInfo,
                        viewForgeEnv.StatementCompileTimeServices)
                    .WithMemberName(names)
                    .Build();
                validated = ExprNodeUtilityValidate.GetValidatedSubtree(
                    ExprNodeOrigin.VIEWPARAMETER,
                    expression,
                    validationContext);
            }
            catch (ExprValidationException ex) {
                var message = "Invalid parameter expression " + expressionNumber + GetViewDesc(viewName);
                if (ex.Message != null) {
                    message += ": " + ex.Message;
                }

                throw new ViewParameterException(message, ex);
            }

            return validated;
        }

        public static object Evaluate(
            ExprEvaluator evaluator,
            int expressionNumber,
            string viewName)
        {
            try {
                return evaluator.Evaluate(null, true, null);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                var message = "Failed to evaluate parameter expression " + expressionNumber + GetViewDesc(viewName);
                if (ex.Message != null) {
                    message += ": " + ex.Message;
                }

                throw new ViewParameterException(message, ex);
            }
        }

        public static void ValidateNoParameters(
            string viewName,
            IList<ExprNode> expressionParameters)
        {
            if (!expressionParameters.IsEmpty()) {
                var errorMessage = viewName + " view requires an empty parameter list";
                throw new ViewParameterException(errorMessage);
            }
        }

        private static string GetViewParamMessage(string viewName)
        {
            return viewName + " view requires a single integer-type parameter";
        }

        private static bool ValidateSize(object size)
        {
            return !(size == null || size.AsInt32() <= 0);
        }

        private static string GetSizeValidationMsg(
            string viewName,
            object size)
        {
            return viewName + " view requires a positive integer for size but received " + size;
        }

        private static string GetViewDesc(string viewName)
        {
            return " for " + viewName + " view";
        }
    }
} // end of namespace