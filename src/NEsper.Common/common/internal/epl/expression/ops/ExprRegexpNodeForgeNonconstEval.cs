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
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.ops.ExprRegexpNodeForgeConstEval;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprRegexpNodeForgeNonconstEval : ExprEvaluator
    {
        private readonly ExprRegexpNodeForgeNonconst _forge;
        private readonly ExprEvaluator _lhsEval;
        private readonly ExprEvaluator _patternEval;

        public ExprRegexpNodeForgeNonconstEval(
            ExprRegexpNodeForgeNonconst forge,
            ExprEvaluator lhsEval,
            ExprEvaluator patternEval)
        {
            this._forge = forge;
            this._lhsEval = lhsEval;
            this._patternEval = patternEval;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var patternText = (string) _patternEval.Evaluate(eventsPerStream, isNewData, context);
            if (patternText == null) {
                return null;
            }

            var pattern = ExprRegexNodeCompilePattern(patternText);

            var evalValue = _lhsEval.Evaluate(eventsPerStream, isNewData, context);
            if (evalValue == null) {
                return null;
            }

            if (_forge.IsNumericValue) {
                evalValue = evalValue.ToString();
            }

            // Revisit: Did we previously have an issue where this was using search instead of match?  Revisit the
            // previous version to see if we handled the matching differently.
            return _forge.ForgeRenderable.IsNot ^ pattern.IsMatch((string) evalValue);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="text">regex pattern</param>
        /// <returns>pattern</returns>
        public static Regex ExprRegexNodeCompilePattern(string text)
        {
            try {
                return RegexExtensions.Compile(text, out string patternText);
            }
            catch (ArgumentException ex) {
                throw new EPException("Failed to compile regex pattern '" + text + "': " + ex.Message, ex);
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
                    "ExprRegexNodeCompilePattern",
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