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
        public class IntervalOpEvalLong : IntervalOpEvalBase
        {
            public IntervalOpEvalLong(IntervalComputerEval intervalComputer) : base(intervalComputer)
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
                var time = parameter.AsInt64();
                return intervalComputer.Compute(startTs, endTs, time, time, eventsPerStream, isNewData, context);
            }

            public static CodegenExpression Codegen(
                IntervalComputerForge intervalComputer,
                CodegenExpression startTs,
                CodegenExpression endTs,
                CodegenExpression parameter,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return intervalComputer.Codegen(
                    startTs,
                    endTs,
                    parameter,
                    parameter,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }
        }
    }
}