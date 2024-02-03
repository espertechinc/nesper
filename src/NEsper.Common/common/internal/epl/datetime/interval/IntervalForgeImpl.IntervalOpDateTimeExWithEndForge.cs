using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public class IntervalOpDateTimeExWithEndForge : IntervalOpForgeDateWithEndBase
        {
            public IntervalOpDateTimeExWithEndForge(
                IntervalComputerForge intervalComputer,
                ExprForge forgeEndTimestamp)
                : base(intervalComputer, forgeEndTimestamp)
            {
            }

            public override IntervalOpEval MakeEval()
            {
                return new IntervalOpCalWithEndEval(
                    intervalComputer.MakeComputerEval(),
                    forgeEndTimestamp.ExprEvaluator);
            }

            protected override CodegenExpression CodegenEvaluate(CodegenExpressionRef startTs,
                CodegenExpressionRef endTs,
                CodegenExpression paramStartTs,
                CodegenExpression paramEndTs,
                CodegenMethod parentNode,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return intervalComputer.Codegen(
                    startTs,
                    endTs,
                    CodegenExpressionBuilder.ExprDotName(paramStartTs, "UtcMillis"),
                    CodegenExpressionBuilder.ExprDotName(paramEndTs, "UtcMillis"),
                    parentNode,
                    exprSymbol,
                    codegenClassScope);
            }
        }
    }
}