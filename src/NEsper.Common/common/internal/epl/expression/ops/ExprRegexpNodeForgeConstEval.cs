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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Like-Node Form-1: string input, constant pattern and no or constant escape character
    /// </summary>
    [Serializable]
    public class ExprRegexpNodeForgeConstEval : ExprEvaluator
    {
        private readonly ExprRegexpNodeForgeConst _forge;
        private readonly ExprEvaluator _lhsEval;

        internal ExprRegexpNodeForgeConstEval(
            ExprRegexpNodeForgeConst forge,
            ExprEvaluator lhsEval)
        {
            _forge = forge;
            _lhsEval = lhsEval;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var value = _lhsEval.Evaluate(eventsPerStream, isNewData, context);
            if (value == null) {
                return null;
            }

            if (_forge.IsNumericValue) {
                value = value.RenderAny();
            }

            var stringValue = (string)value;
            var result = _forge.ForgeRenderable.IsNot ^ _forge.Pattern.IsMatch(stringValue); //Matches();
            return result;
        }

        public static CodegenMethod Codegen(
            ExprRegexpNodeForgeConst forge,
            ExprNode lhs,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var mPattern = codegenClassScope.AddDefaultFieldUnshared<Regex>(true, forge.PatternInit);
            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprRegexpNodeForgeConstEval),
                codegenClassScope);

            if (!forge.IsNumericValue) {
                methodNode.Block
                    .DeclareVar<string>(
                        "value",
                        lhs.Forge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .MethodReturn(GetRegexpCode(forge, mPattern, Ref("value")));
            }
            else {
                var valueRender = StaticMethod(typeof(CompatExtensions), "RenderAny", Ref("value"));
                methodNode.Block
                    .DeclareVar<object>(
                        "value",
                        lhs.Forge.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .MethodReturn(GetRegexpCode(forge, mPattern, valueRender));
            }

            return methodNode;
        }

        internal static CodegenExpression GetRegexpCode(
            ExprRegexpNodeForge forge,
            CodegenExpression pattern,
            CodegenExpression stringExpr)
        {
            CodegenExpression eval = ExprDotMethodChain(pattern).Add("IsMatch", stringExpr);
            return !forge.ForgeRenderable.IsNot ? eval : Not(eval);
        }
    }
} // end of namespace