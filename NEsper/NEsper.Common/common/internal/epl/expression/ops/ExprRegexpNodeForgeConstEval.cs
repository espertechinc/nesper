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
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Like-Node Form-1: string input, constant pattern and no or constant escape character
    /// </summary>
    [Serializable]
    public class ExprRegexpNodeForgeConstEval : ExprEvaluator
    {
        private readonly ExprRegexpNodeForgeConst forge;
        private readonly ExprEvaluator lhsEval;

        internal ExprRegexpNodeForgeConstEval(ExprRegexpNodeForgeConst forge, ExprEvaluator lhsEval)
        {
            this.forge = forge;
            this.lhsEval = lhsEval;
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var value = lhsEval.Evaluate(eventsPerStream, isNewData, context);
            if (value == null) {
                return null;
            }

            if (forge.IsNumericValue) {
                value = value.ToString();
            }

            var stringValue = (string) value;
            var result = forge.ForgeRenderable.IsNot ^ forge.Pattern.IsMatch(stringValue); //Matches();
            return result;
        }

        public static CodegenMethod Codegen(
            ExprRegexpNodeForgeConst forge, ExprNode lhs, CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope)
        {
            CodegenExpression mPattern = codegenClassScope.AddFieldUnshared<Regex>(true, forge.PatternInit);
            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?), typeof(ExprRegexpNodeForgeConstEval), codegenClassScope);

            if (!forge.IsNumericValue) {
                methodNode.Block
                    .DeclareVar(
                        typeof(string), "value",
                        lhs.Forge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .MethodReturn(GetRegexpCode(forge, mPattern, Ref("value")));
            }
            else {
                methodNode.Block
                    .DeclareVar(
                        typeof(object), "value",
                        lhs.Forge.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .MethodReturn(GetRegexpCode(forge, mPattern, ExprDotMethod(Ref("value"), "toString")));
            }

            return methodNode;
        }

        private static CodegenExpression GetRegexpCode(
            ExprRegexpNodeForge forge, CodegenExpression pattern, CodegenExpression stringExpr)
        {
            CodegenExpression eval = ExprDotMethodChain(pattern).Add("matcher", stringExpr).Add("matches");
            return !forge.ForgeRenderable.IsNot ? eval : Not(eval);
        }
    }
} // end of namespace