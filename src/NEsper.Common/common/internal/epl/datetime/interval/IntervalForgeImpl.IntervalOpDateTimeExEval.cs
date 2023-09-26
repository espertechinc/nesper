using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public class IntervalOpDateTimeExEval : IntervalOpEvalBase
        {
            public IntervalOpDateTimeExEval(IntervalComputerEval intervalComputer)
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
                var time = ((DateTimeEx) parameter).UtcMillis;
                return intervalComputer.Compute(startTs, endTs, time, time, eventsPerStream, isNewData, context);
            }

            public static CodegenExpression Codegen(
                IntervalOpDateTimeExForge forge,
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool?),
                        typeof(IntervalOpEval),
                        codegenClassScope)
                    .AddParam(typeof(long), "startTs")
                    .AddParam(typeof(long), "endTs")
                    .AddParam(typeof(DateTimeEx), "parameter");

                methodNode.Block
                    .DeclareVar<long>("time", CodegenExpressionBuilder.ExprDotName(CodegenExpressionBuilder.Ref("parameter"), "UtcMillis"))
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