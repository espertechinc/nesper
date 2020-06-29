using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodFactoryInteger : ExprDotEvalSumMethodFactory
        {
            internal static readonly ExprDotEvalSumMethodFactoryInteger INSTANCE = new ExprDotEvalSumMethodFactoryInteger();

            private ExprDotEvalSumMethodFactoryInteger()
            {
            }

            public ExprDotEvalSumMethod SumAggregator {
                get { return new ExprDotEvalSumMethodInteger(); }
            }

            public Type ValueType {
                get { return typeof(int?); }
            }

            public void CodegenDeclare(CodegenBlock block)
            {
                block.DeclareVar<int>("sum", Constant(0));
                block.DeclareVar<long>("cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", value);
            }

            public void CodegenEnterObjectTypedNonNull(
                CodegenBlock block,
                CodegenExpressionRef value)
            {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", ExprDotMethod(value, "AsInt32"));
            }

            public void CodegenReturn(CodegenBlock block)
            {
                CodegenReturnSumOrNull(block);
            }
        }
    }
}