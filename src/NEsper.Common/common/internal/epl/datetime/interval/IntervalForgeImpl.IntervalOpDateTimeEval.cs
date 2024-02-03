using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public class IntervalOpDateTimeEval : IntervalOpEvalBase
        {
            public IntervalOpDateTimeEval(IntervalComputerEval intervalComputer)
                : base(intervalComputer)
            {
            }

            public override object Evaluate(
                long startTs,
                long endTs,
                object parameter,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                var time = DatetimeLongCoercerDateTime.CoerceToMillis((DateTime) parameter);
                return intervalComputer.Compute(startTs, endTs, time, time, eventsPerStream, isNewData, context);
            }

            public static CodegenExpression Codegen(
                IntervalOpDateTimeForge forge,
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool?), typeof(IntervalOpDateTimeEval), codegenClassScope)
                    .AddParam(typeof(long), "startTs")
                    .AddParam(typeof(long), "endTs")
                    .AddParam(typeof(DateTime), "parameter");

                methodNode.Block
                    .DeclareVar<long>(
                        "time",
                        CodegenExpressionBuilder.StaticMethod(typeof(DatetimeLongCoercerDateTime), "CoerceToMillis", CodegenExpressionBuilder.Ref("parameter")))
                    .MethodReturn(
                        forge.IntervalComputer.Codegen(
                            CodegenExpressionBuilder.Ref("startTs"),
                            CodegenExpressionBuilder.Ref("endTs"),
                            CodegenExpressionBuilder.Ref("time"),
                            CodegenExpressionBuilder.Ref("time"),
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                return CodegenExpressionBuilder.LocalMethod(methodNode, start, end, parameter);
            }
        }
    }
}