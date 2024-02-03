using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

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
                CodegenExpression paramStartTs,
                CodegenExpression paramEndTs,
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
                if (evaluationType.CanBeNull()) {
                    methodNode.Block.IfRefNullReturnNull("paramEndTs");
                }

                var paramStartTs = CodegenExpressionBuilder.Unbox(
                    CodegenExpressionBuilder.Ref("paramStartTs"), parameterType);
                var paramEndTs = CodegenExpressionBuilder.Unbox(
                    CodegenExpressionBuilder.Ref("paramEndTs"), evaluationType);
                
                var expression = CodegenEvaluate(
                    CodegenExpressionBuilder.Ref("startTs"),
                    CodegenExpressionBuilder.Ref("endTs"),
                    paramStartTs,
                    paramEndTs,
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                methodNode.Block.MethodReturn(expression);
                return CodegenExpressionBuilder.LocalMethod(methodNode, start, end, parameter);
            }
        }
    }
}