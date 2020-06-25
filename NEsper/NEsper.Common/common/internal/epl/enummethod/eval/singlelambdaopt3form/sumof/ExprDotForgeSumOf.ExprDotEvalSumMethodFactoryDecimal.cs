using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodFactoryDecimal : ExprDotEvalSumMethodFactory
        {
            internal readonly static ExprDotEvalSumMethodFactoryDecimal INSTANCE = new ExprDotEvalSumMethodFactoryDecimal();

            private ExprDotEvalSumMethodFactoryDecimal() {
            }

            public ExprDotEvalSumMethod SumAggregator {
                get { return new ExprDotEvalSumMethodDecimal(); }
            }

            public Type ValueType {
                get { return typeof(decimal?); }
            }

            public void CodegenDeclare(CodegenBlock block) {
                block.DeclareVar<decimal>("sum", Constant(0.0m));
                block.DeclareVar<long>("cnt", Constant(0));
            }

            public void CodegenEnterNumberTypedNonNull(CodegenBlock block, CodegenExpressionRef value) {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", value);
            }

            public void CodegenEnterObjectTypedNonNull(CodegenBlock block, CodegenExpressionRef value) {
                block.IncrementRef("cnt");
                block.AssignCompound("sum", "+", ExprDotMethod(value, "AsDecimal"));
            }

            public void CodegenReturn(CodegenBlock block) {
                CodegenReturnSumOrNull(block);
            }
        }
    }
}