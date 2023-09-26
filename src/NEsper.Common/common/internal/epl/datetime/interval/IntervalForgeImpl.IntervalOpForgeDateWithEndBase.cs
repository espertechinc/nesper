using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public abstract class IntervalOpForgeDateWithEndBase : IntervalOpForge
        {
            protected readonly IntervalComputerForge intervalComputer;
            protected readonly ExprForge forgeEndTimestamp;

            public IntervalOpForgeDateWithEndBase(
                IntervalComputerForge intervalComputer,
                ExprForge forgeEndTimestamp)
            {
                this.intervalComputer = intervalComputer;
                this.forgeEndTimestamp = forgeEndTimestamp;
            }

            protected abstract CodegenExpression CodegenEvaluate(
                CodegenExpressionRef startTs,
                CodegenExpressionRef endTs,
                CodegenExpressionRef paramStartTs,
                CodegenExpressionRef paramEndTs,
                CodegenMethod parentNode,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope);

            public abstract IntervalOpEval MakeEval();

            public CodegenExpression Codegen(
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool?), typeof(IntervalOpForgeDateWithEndBase), codegenClassScope)
                    .AddParam<long>("startTs")
                    .AddParam<long>("endTs")
                    .AddParam(parameterType, "paramStartTs");

                var evaluationType = forgeEndTimestamp.EvaluationType;
                methodNode.Block.DeclareVar(
                    evaluationType,
                    "paramEndTs",
                    forgeEndTimestamp.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope));
                if (!evaluationType.IsPrimitive) {
                    methodNode.Block.IfRefNullReturnNull("paramEndTs");
                }

                var expression = CodegenEvaluate(
                    CodegenExpressionBuilder.Ref("startTs"),
                    CodegenExpressionBuilder.Ref("endTs"),
                    CodegenExpressionBuilder.Ref("paramStartTs"),
                    CodegenExpressionBuilder.Ref("paramEndTs"),
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                methodNode.Block.MethodReturn(expression);
                return CodegenExpressionBuilder.LocalMethod(methodNode, start, end, parameter);
            }
        }
    }
}