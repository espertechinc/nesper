using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodFactoryDouble : ExprDotEvalSumMethodFactory
        {
            internal static readonly ExprDotEvalSumMethodFactoryDouble INSTANCE =
                new ExprDotEvalSumMethodFactoryDouble();

            private ExprDotEvalSumMethodFactoryDouble()
            {
            }

            public ExprDotEvalSumMethod SumAggregator => new ExprDotEvalSumMethodDouble();

            public Type ValueType => typeof(double?);

            public void CodegenDeclare(CodegenBlock block)
            {
                block.DeclareVar<double>("sum", Constant(0.0d));
                block.DeclareVar<long>("cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", ExprDotMethod(value, "AsDouble"));
            }

            public void CodegenEnterObjectTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", ExprDotMethod(value, "AsDouble"));
            }

            public void CodegenReturn(CodegenBlock block)
            {
                CodegenReturnSumOrNull(block);
            }
        }
    }
}