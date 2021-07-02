using System.Numerics;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodBigInteger : ExprDotEvalSumMethod
        {
            private long _cnt;
            private BigInteger _sum;

            public ExprDotEvalSumMethodBigInteger()
            {
                _sum = BigInteger.Zero;
            }

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                _cnt++;
                _sum += @object.AsBigInteger();
            }

            public object Value {
                get {
                    if (_cnt == 0) {
                        return null;
                    }

                    return _sum;
                }
            }
        }
    }
}