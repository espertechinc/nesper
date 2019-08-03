///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text.RegularExpressions;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.ops.ExprRegexpNodeForgeConstEval;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprRegexpNodeForgeNonconstEval : ExprEvaluator
    {
        private readonly ExprRegexpNodeForgeNonconst forge;
        private readonly ExprEvaluator lhsEval;
        private readonly ExprEvaluator patternEval;

        public ExprRegexpNodeForgeNonconstEval(
            ExprRegexpNodeForgeNonconst forge,
            ExprEvaluator lhsEval,
            ExprEvaluator patternEval)
        {
            this.forge = forge;
            this.lhsEval = lhsEval;
            this.patternEval = patternEval;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var patternText = (string) patternEval.Evaluate(eventsPerStream, isNewData, context);
            if (patternText == null) {
                return null;
            }

            var pattern = ExprRegexNodeCompilePattern(patternText);

            var evalValue = lhsEval.Evaluate(eventsPerStream, isNewData, context);
            if (evalValue == null) {
                return null;
            }

            if (forge.IsNumericValue) {
                evalValue = evalValue.ToString();
            }

            // Revisit: Did we previously have an issue where this was using search instead of match?  Revisit the
            // previous version to see if we handled the matching differently.
            return forge.ForgeRenderable.IsNot ^ pattern.IsMatch((string) evalValue);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="text">regex pattern</param>
        /// <returns>pattern</returns>
        public static Regex ExprRegexNodeCompilePattern(string text)
        {
            try {
                return new Regex(text);
            }
            catch (ArgumentException ex) {
                throw new EPException("Error compiling regex pattern '" + text + "': " + ex.Message, ex);
            }
        }

        public static CodegenMethod Codegen(
            ExprRegexpNodeForgeNonconst forge,
            ExprNode lhs,
            ExprNode pattern,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprRegexpNodeForgeNonconstEval),
                codegenClassScope);
            var blockMethod = methodNode.Block
                .DeclareVar<string>(
                    "patternText",
                    pattern.Forge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("patternText");

            // initial like-setup
            blockMethod.DeclareVar<Regex>(
                "pattern",
                StaticMethod(
                    typeof(ExprRegexpNodeForgeNonconstEval),
                    "exprRegexNodeCompilePattern",
                    Ref("patternText")));

            if (!forge.IsNumericValue) {
                blockMethod.DeclareVar<string>(
                        "value",
                        lhs.Forge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .MethodReturn(GetRegexpCode(forge, Ref("pattern"), Ref("value")));
            }
            else {
                blockMethod.DeclareVar<object>(
                        "value",
                        lhs.Forge.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .MethodReturn(GetRegexpCode(forge, Ref("pattern"), ExprDotMethod(Ref("value"), "ToString")));
            }

            return methodNode;
        }
    }
} // end of namespace