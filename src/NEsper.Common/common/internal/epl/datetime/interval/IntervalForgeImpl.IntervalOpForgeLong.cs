using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public class IntervalOpForgeLong : IntervalOpForgeBase
        {
            public IntervalOpForgeLong(IntervalComputerForge intervalComputer) : base(intervalComputer)
            {
            }

            public override IntervalOpEval MakeEval()
            {
                return new IntervalOpEvalLong(intervalComputer.MakeComputerEval());
            }

            public override CodegenExpression Codegen(
                CodegenExpression startTs,
                CodegenExpression endTs,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalOpEvalLong.Codegen(
                    intervalComputer,
                    startTs,
                    endTs,
                    parameter,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }
        }
    }
}