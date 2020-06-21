///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using static com.espertech.esper.common.@internal.epl.expression.codegen.CodegenLegoCompareEquals; //codegenEqualsNonNullNoCoerce;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeNCForgeEquals
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

            var methodNode = codegenMethodScope.MakeChild(typeof(bool?), typeof(ExprEqualsNodeForgeNCForgeEquals), codegenClassScope);
            var block = methodNode.Block
                .DeclareVar(lhsType, "left", lhs.EvaluateCodegen(lhsType, methodNode, exprSymbol, codegenClassScope))
                .DeclareVar(rhsType, "right", rhs.EvaluateCodegen(rhsType, methodNode, exprSymbol, codegenClassScope));

            if (!lhsType.IsPrimitive) {
                block.IfRefNullReturnNull("left");
            }

            if (!rhsType.IsPrimitive) {
                block.IfRefNullReturnNull("right");
            }

            CodegenExpression compare;
            if (!lhsType.IsArray) {
                compare = CodegenEqualsNonNullNoCoerce(Ref("left"), lhsType, Ref("right"), rhsType);
            }
            else {
                if (!MultiKeyPlanner.RequiresDeepEquals(lhsType.GetElementType())) {
                    compare = StaticMethod(typeof(Arrays), "Equals", Ref("left"), Ref("right"));
                }
                else {
                    compare = StaticMethod(typeof(Arrays), "DeepEquals", Ref("left"), Ref("right"));
                }
            }

            if (!forge.ForgeRenderable.IsNotEquals) {
                block.MethodReturn(compare);
            }
            else {
                block.MethodReturn(Not(compare));
            }

            return methodNode;
        }
    }
} // end of namespace