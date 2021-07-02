///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.codegen
{
    public class CodegenLegoMethodExpression
    {
        private const string PASS_NAME = "pass";

        public static CodegenExpression CodegenBooleanExpressionReturnTrueFalse(
            ExprForge forge,
            CodegenClassScope classScope,
            CodegenMethod parent,
            CodegenExpression eps,
            CodegenExpression isNewData,
            CodegenExpression exprEvalCtx)
        {
            CheckEvaluationType(forge);
            var expressionMethod = CodegenBooleanExpressionBoxedToPrimitive(forge, parent, classScope);
            return LocalMethod(expressionMethod, eps, isNewData, exprEvalCtx);
        }

        public static CodegenMethod CodegenExpression(
            ExprForge forge,
            CodegenMethod parent,
            CodegenClassScope classScope)
        {
            var evaluationType = forge.EvaluationType;
            var methodReturnType = typeof(object);
            if (evaluationType != null) {
                methodReturnType = evaluationType.GetBoxedType();
            }
            
            // if (returnMissingValueAsNull) {
            //     evaluationType = evaluationType.GetBoxedType();
            // }
            
            var exprSymbol = new ExprForgeCodegenSymbol(true, null);
            var exprMethod = parent
                .MakeChildWithScope(methodReturnType, typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);

            exprMethod.Block.DebugStack();
            
            if (evaluationType.IsNullTypeSafe()) {
                exprMethod.Block.MethodReturn(ConstantNull());
            }
            else {
                var expression = forge.EvaluateCodegen(methodReturnType, exprMethod, exprSymbol, classScope);
                exprSymbol.DerivedSymbolsCodegen(parent, exprMethod.Block, classScope);

                if (evaluationType.IsVoid()) {
                    exprMethod.Block.Expression(expression);
                }
                else {
                    exprMethod.Block.MethodReturn(expression);
                    //CodegenLegoCast.CastSafeFromObjectType(evaluationType, expression);
                }
            }

            return exprMethod;
        }

        private static CodegenMethod CodegenBooleanExpressionBoxedToPrimitive(
            ExprForge forge,
            CodegenMethod parent,
            CodegenClassScope classScope)
        {
            var evaluationType = forge.EvaluationType;
            var exprSymbol = new ExprForgeCodegenSymbol(true, null);
            var exprMethod = parent
                .MakeChildWithScope(typeof(bool), typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);

            var expression = forge.EvaluateCodegen(evaluationType, exprMethod, exprSymbol, classScope);
            exprSymbol.DerivedSymbolsCodegen(parent, exprMethod.Block, classScope);

            if (evaluationType.IsNullType() || evaluationType.CanNotBeNull()) {
                exprMethod.Block.MethodReturn(expression);
            }
            else {
                exprMethod.Block
                    .DeclareVar(evaluationType, PASS_NAME, expression)
                    .IfRefNull(PASS_NAME)
                    .BlockReturn(ConstantFalse())
                    .MethodReturn(ExprDotName(Ref(PASS_NAME), "Value"));
            }

            return exprMethod;
        }

        private static void CheckEvaluationType(ExprForge forge)
        {
            var evaluationType = forge.EvaluationType;
            if (!evaluationType.IsBoolean()) {
                throw new IllegalStateException("Invalid non-boolean expression");
            }
        }
    }
} // end of namespace