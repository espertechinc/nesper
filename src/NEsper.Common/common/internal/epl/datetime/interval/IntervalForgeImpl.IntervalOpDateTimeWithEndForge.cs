using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public class IntervalOpDateTimeWithEndForge : IntervalOpForgeDateWithEndBase
        {
            public IntervalOpDateTimeWithEndForge(
                IntervalComputerForge intervalComputer,
                ExprForge evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)

            {
            }

            public override IntervalOpEval MakeEval()
            {
                return new IntervalOpDateTimeWithEndEval(
                    intervalComputer.MakeComputerEval(),
                    forgeEndTimestamp.ExprEvaluator);
            }

            protected override CodegenExpression CodegenEvaluate(
                CodegenExpressionRef startTs,
                CodegenExpressionRef endTs,
                CodegenExpressionRef paramStartTs,
                CodegenExpressionRef paramEndTs,
                CodegenMethod parentNode,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return intervalComputer.Codegen(
                    startTs,
                    endTs,
                    CodegenExpressionBuilder.StaticMethod(typeof(DatetimeLongCoercerDateTime), "CoerceToMillis", paramStartTs),
                    CodegenExpressionBuilder.StaticMethod(typeof(DatetimeLongCoercerDateTime), "CoerceToMillis", paramEndTs),
                    parentNode,
                    exprSymbol,
                    codegenClassScope);
            }
        }
    }
}