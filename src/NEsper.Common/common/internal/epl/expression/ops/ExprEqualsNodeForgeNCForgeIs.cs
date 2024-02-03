///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeNCForgeIs
    {
        public static CodegenMethod Codegen(
            ExprEqualsNodeForgeNC forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope,
            ExprForge lhs,
            ExprForge rhs)
        {
            var lhsType = lhs.EvaluationType;
            var rhsType = rhs.EvaluationType;
            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool),
                typeof(ExprEqualsNodeForgeNCForgeIs),
                codegenClassScope);

            CodegenExpression compare;
            if (rhsType != null && lhsType != null) {
                if (!lhsType.IsArray) {
                    methodNode.Block
                        .DeclareVar<object>(
                            "left",
                            lhs.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                        .DeclareVar<object>(
                            "right",
                            rhs.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope));
                    compare = StaticMethod<object>("Equals", Ref("left"), Ref("right"));
                }
                else {
                    methodNode.Block
                        .DeclareVar(
                            lhsType,
                            "left",
                            lhs.EvaluateCodegen(lhsType, methodNode, exprSymbol, codegenClassScope))
                        .DeclareVar(
                            rhsType,
                            "right",
                            rhs.EvaluateCodegen(rhsType, methodNode, exprSymbol, codegenClassScope));
                    if (!MultiKeyPlanner.RequiresDeepEquals(lhsType.GetElementType())) {
                        compare = StaticMethod(typeof(Arrays), "AreEqual", Ref("left"), Ref("right"));
                    }
                    else {
                        compare = StaticMethod(typeof(Arrays), "DeepEquals", Ref("left"), Ref("right"));
                    }
                }

                methodNode.Block.DeclareVarNoInit(typeof(bool), "result")
                    .IfRefNull("left")
                    .AssignRef("result", EqualsNull(Ref("right")))
                    .IfElse()
                    .AssignRef("result", And(NotEqualsNull(Ref("right")), compare))
                    .BlockEnd();
            }
            else {
                if (lhsType == null && rhsType == null) {
                    methodNode.Block.DeclareVar<bool>("result", ConstantTrue());
                }
                else if (lhsType == null) {
                    methodNode.Block
                        .DeclareVar<object>(
                            "right",
                            rhs.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                        .DeclareVar<bool>("result", EqualsNull(Ref("right")));
                }
                else {
                    methodNode.Block
                        .DeclareVar<object>(
                            "left",
                            lhs.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                        .DeclareVar<bool>("result", EqualsNull(Ref("left")));
                }
            }

            if (!forge.ForgeRenderable.IsNotEquals) {
                methodNode.Block.MethodReturn(Ref("result"));
            }
            else {
                methodNode.Block.MethodReturn(Not(Ref("result")));
            }

            return methodNode;
        }
    }
} // end of namespace