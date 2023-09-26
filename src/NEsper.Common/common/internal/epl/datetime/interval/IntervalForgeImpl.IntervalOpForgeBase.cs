using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public abstract class IntervalOpForgeBase : IntervalOpForge
        {
            protected readonly IntervalComputerForge intervalComputer;

            public IntervalComputerForge IntervalComputer => intervalComputer;

            public IntervalOpForgeBase(IntervalComputerForge intervalComputer)
            {
                this.intervalComputer = intervalComputer;
            }

            public abstract IntervalOpEval MakeEval();

            public abstract CodegenExpression Codegen(
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope);
        }
    }
}