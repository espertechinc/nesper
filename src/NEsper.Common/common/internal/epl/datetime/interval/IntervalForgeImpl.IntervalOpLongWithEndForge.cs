using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public class IntervalOpLongWithEndForge : IntervalOpForgeDateWithEndBase
        {
            public IntervalOpLongWithEndForge(
                IntervalComputerForge intervalComputer,
                ExprForge evaluatorEndTimestamp) : base(intervalComputer, evaluatorEndTimestamp)
            {
            }

            public override IntervalOpEval MakeEval()
            {
                return new IntervalOpLongWithEndEval(
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
                    paramStartTs,
                    paramEndTs,
                    parentNode,
                    exprSymbol,
                    codegenClassScope);
            }
        }
    }
}