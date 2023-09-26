using System;
using System.Numerics;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodFactoryBigInteger : ExprDotEvalSumMethodFactory
        {
            internal static readonly ExprDotEvalSumMethodFactoryBigInteger INSTANCE =
                new ExprDotEvalSumMethodFactoryBigInteger();

            private ExprDotEvalSumMethodFactoryBigInteger()
            {
            }

            public ExprDotEvalSumMethod SumAggregator => new ExprDotEvalSumMethodBigInteger();

            public Type ValueType => typeof(BigInteger?);

            public void CodegenDeclare(CodegenBlock block)
            {
                block.DeclareVar<BigInteger>("sum", EnumValue(typeof(BigInteger), "Zero"));
                block.DeclareVar<long>("cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", ExprDotMethod(value, "AsBigInteger"));
            }

            public void CodegenEnterObjectTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", ExprDotMethod(value, "AsBigInteger"));
            }

            public void CodegenReturn(CodegenBlock block)
            {
                CodegenReturnSumOrNull(block);
            }
        }
    }
}