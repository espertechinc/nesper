using System.Numerics;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodBigInteger : ExprDotEvalSumMethod
        {
            private long cnt;
            private BigInteger sum;

            public ExprDotEvalSumMethodBigInteger()
            {
                sum = BigInteger.Zero;
            }

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                cnt++;
                sum += @object.AsBigInteger();
            }

            public object Value {
                get {
                    if (cnt == 0) {
                        return null;
                    }

                    return sum;
                }
            }
        }
    }
}