///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.codegen;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprLikeNodeForgeConstEval : ExprEvaluator
    {
        private readonly ExprLikeNodeForgeConst forge;
        private readonly ExprEvaluator lhsEval;

        internal ExprLikeNodeForgeConstEval(
            ExprLikeNodeForgeConst forge,
            ExprEvaluator lhsEval)
        {
            this.forge = forge;
            this.lhsEval = lhsEval;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var value = lhsEval.Evaluate(eventsPerStream, isNewData, context);

            if (value == null) {
                return null;
            }

            if (forge.IsNumericValue) {
                value = value.ToString();
            }

            return forge.ForgeRenderable.IsNot ^ forge.LikeUtil.CompareTo((string) value);
        }

        public static CodegenMethod Codegen(
            ExprLikeNodeForgeConst forge,
            ExprNode lhs,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var mLikeUtil = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(LikeUtil),
                forge.LikeUtilInit);


            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprLikeNodeForgeConstEval),
                codegenClassScope);
            if (!forge.IsNumericValue) {
                methodNode.Block
                    .DeclareVar<string>(
                        "value",
                        lhs.Forge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .MethodReturn(GetLikeCode(forge, mLikeUtil, Ref("value")));
            }
            else {
                methodNode.Block.DeclareVar<object>(
                        "value",
                        lhs.Forge.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .MethodReturn(GetLikeCode(forge, mLikeUtil, ExprDotMethod(Ref("value"), "ToString")));
            }

            return methodNode;
        }

        internal static CodegenExpression GetLikeCode(
            ExprLikeNodeForge forge,
            CodegenExpression refLike,
            CodegenExpression stringExpr)
        {
            var eval = ExprDotMethod(refLike, "CompareTo", stringExpr);
            return !forge.ForgeRenderable.IsNot ? eval : Not(eval);
        }
    }
} // end of namespace