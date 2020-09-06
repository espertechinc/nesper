using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodFactoryLong : ExprDotEvalSumMethodFactory
        {
            internal static readonly ExprDotEvalSumMethodFactoryLong INSTANCE = new ExprDotEvalSumMethodFactoryLong();

            private ExprDotEvalSumMethodFactoryLong()
            {
            }

            public ExprDotEvalSumMethod SumAggregator => new ExprDotEvalSumMethodLong();

            public Type ValueType => typeof(long?);

            public void CodegenDeclare(CodegenBlock block)
            {
                block.DeclareVar<long>("sum", Constant(0L));
                block.DeclareVar<long>("cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", ExprDotMethod(value, "AsInt64"));
            }

            public void CodegenEnterObjectTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", ExprDotMethod(value, "AsInt64"));
            }

            public void CodegenReturn(CodegenBlock block)
            {
                CodegenReturnSumOrNull(block);
            }
        }
    }
}