using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public class IntervalOpDateTimeExForge : IntervalOpForgeBase
        {
            public IntervalOpDateTimeExForge(IntervalComputerForge intervalComputer)
                : base(intervalComputer)
            {
            }

            public override IntervalOpEval MakeEval()
            {
                return new IntervalOpDateTimeExEval(intervalComputer.MakeComputerEval());
            }

            public override CodegenExpression Codegen(
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalOpDateTimeExEval.Codegen(
                    this,
                    start,
                    end,
                    parameter,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }
        }
    }
}