///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class CodegenLegoBooleanExpression
    {
        private const string PASS_NAME = "pass";

        /// <summary>
        ///     Generates code like this (premade expr assumed):
        ///     boolean/Boolean result = expression.evaluate(eps, isNewData, context);
        ///     if (result == null (optional early exit if null)  ||   (!? (Boolean) result)) {
        ///     return false/true;
        ///     }
        /// </summary>
        /// <param name="block">block</param>
        /// <param name="earlyExitIfNull">indicator</param>
        /// <param name="resultEarlyExit">indicator</param>
        /// <param name="checkFor">indicator</param>
        /// <param name="resultIfCheckPasses">indicator</param>
        /// <param name="evaluationType">type</param>
        /// <param name="expression">expr</param>
        public static void CodegenReturnBoolIfNullOrBool(
            CodegenBlock block,
            Type evaluationType,
            CodegenExpression expression,
            bool earlyExitIfNull,
            bool? resultEarlyExit,
            bool checkFor,
            bool resultIfCheckPasses)
        {
            if (evaluationType != typeof(bool) && evaluationType != typeof(bool?)) {
                throw new IllegalStateException("Invalid non-boolean expression");
            }

            var unboxPass = Unbox(Ref(PASS_NAME), evaluationType);

            block.DeclareVar(evaluationType, PASS_NAME, expression);
            //block.Debug("Pass = {0}", Ref(PASS_NAME));

            var passCheck = NotOptional(!checkFor, unboxPass);

            if (evaluationType.CanNotBeNull()) {
                block.IfCondition(passCheck).BlockReturn(Constant(resultIfCheckPasses));
                return;
            }

            if (earlyExitIfNull) {
                if (resultEarlyExit == null) {
                    throw new IllegalStateException("Invalid null for result-early-exit");
                }

                block.IfRefNull(PASS_NAME).BlockReturn(Constant(resultEarlyExit));
                block.IfCondition(passCheck).BlockReturn(Constant(resultIfCheckPasses));
                return;
            }

            block
                .IfCondition(And(NotEqualsNull(Ref(PASS_NAME)), passCheck))
                .BlockReturn(Constant(resultIfCheckPasses));
        }

        /// <summary>
        ///     Generates code like this (premade expr assumed):
        ///     boolean/Boolean result = expression.evaluate(eps, isNewData, context);
        ///     if (result != null &amp;&amp; (!(Boolean) result)) {
        ///     return value;
        ///     }
        /// </summary>
        /// <param name="block">block</param>
        /// <param name="evaluationType">eval type</param>
        /// <param name="expression">expression</param>
        /// <param name="value">value</param>
        public static void CodegenReturnValueIfNotNullAndNotPass(
            CodegenBlock block,
            Type evaluationType,
            CodegenExpression expression,
            CodegenExpression value)
        {
            CodegenDoIfNotNullAndNotPass(block, evaluationType, expression, false, false, value);
        }

        /// <summary>
        ///     Generates code like this (premade expr assumed):
        ///     boolean/Boolean result = expression.evaluate(eps, isNewData, context);
        ///     if (result == null || (!(Boolean) result)) {
        ///     return value;
        ///     }
        /// </summary>
        /// <param name="block">block</param>
        /// <param name="evaluationType">eval type</param>
        /// <param name="expression">expression</param>
        /// <param name="value">value</param>
        public static void CodegenReturnValueIfNullOrNotPass(
            CodegenBlock block,
            Type evaluationType,
            CodegenExpression expression,
            CodegenExpression value)
        {
            CodegenDoIfNullOrNotPass(block, evaluationType, expression, false, false, value);
        }

        /// <summary>
        ///     Generates code like this (premade expr assumed):
        ///     boolean/Boolean result = expression.evaluate(eps, isNewData, context);
        ///     if (result == null || (!(Boolean) result)) {
        ///     break;
        ///     }
        /// </summary>
        /// <param name="block">block</param>
        /// <param name="evaluationType">eval type</param>
        /// <param name="expression">expression</param>
        public static void CodegenBreakIfNotNullAndNotPass(
            CodegenBlock block,
            Type evaluationType,
            CodegenExpression expression)
        {
            CodegenDoIfNotNullAndNotPass(block, evaluationType, expression, false, true, ConstantNull());
        }

        /// <summary>
        ///     Generates code like this (premade expr assumed):
        ///     if (pass != null &amp;&amp; false.Equals(pass)) {
        ///     continue;
        ///     }
        /// </summary>
        /// <param name="block">block</param>
        /// <param name="evaluationType">eval type</param>
        /// <param name="expression">expression</param>
        public static void CodegenContinueIfNotNullAndNotPass(
            CodegenBlock block,
            Type evaluationType,
            CodegenExpression expression)
        {
            CodegenDoIfNotNullAndNotPass(block, evaluationType, expression, true, false, ConstantNull());
        }

        /// <summary>
        ///     Generates code like this (premade expr assumed):
        ///     if (pass == null || false.Equals(pass)) {
        ///     continue;
        ///     }
        /// </summary>
        /// <param name="block">block</param>
        /// <param name="evaluationType">eval type</param>
        /// <param name="expression">expression</param>
        public static void CodegenContinueIfNullOrNotPass(
            CodegenBlock block,
            Type evaluationType,
            CodegenExpression expression)
        {
            CodegenDoIfNullOrNotPass(block, evaluationType, expression, true, false, ConstantNull());
        }

        private static void CodegenDoIfNotNullAndNotPass(
            CodegenBlock block,
            Type evaluationType,
            CodegenExpression expression,
            bool doContinue,
            bool doBreakLoop,
            CodegenExpression returnValue)
        {
            if (evaluationType != typeof(bool) && evaluationType != typeof(bool?)) {
                throw new IllegalStateException("Invalid non-boolean expression");
            }

            block.DeclareVar(evaluationType, PASS_NAME, expression);
            var passCheck = Not(Unbox(Ref(PASS_NAME), evaluationType));

            CodegenExpression condition;
            if (evaluationType.CanNotBeNull()) {
                condition = passCheck;
            }
            else {
                condition = And(NotEqualsNull(Ref(PASS_NAME)), passCheck);
            }

            if (doContinue) {
                block.IfCondition(condition).BlockContinue();
            }
            else if (doBreakLoop) {
                block.IfCondition(condition).BreakLoop();
            }
            else {
                block.IfCondition(condition).BlockReturn(returnValue);
            }
        }

        private static void CodegenDoIfNullOrNotPass(
            CodegenBlock block,
            Type evaluationType,
            CodegenExpression expression,
            bool doContinue,
            bool doBreakLoop,
            CodegenExpression returnValue)
        {
            if (evaluationType != typeof(bool) && evaluationType != typeof(bool?)) {
                throw new IllegalStateException("Invalid non-boolean expression");
            }

            block.DeclareVar(evaluationType, PASS_NAME, expression);
            var passCheck = Not(Unbox(Ref(PASS_NAME), evaluationType));

            CodegenExpression condition;
            if (evaluationType.CanNotBeNull()) {
                condition = passCheck;
            }
            else {
                condition = Or(EqualsNull(Ref(PASS_NAME)), passCheck);
            }

            if (doContinue) {
                block.IfCondition(condition).BlockContinue();
            }
            else if (doBreakLoop) {
                block.IfCondition(condition).BreakLoop();
            }
            else {
                block.IfCondition(condition).BlockReturn(returnValue);
            }
        }
    }
} // end of namespace