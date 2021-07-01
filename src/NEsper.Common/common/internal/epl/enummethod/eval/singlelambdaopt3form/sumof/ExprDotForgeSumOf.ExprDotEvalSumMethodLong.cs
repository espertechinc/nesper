using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodLong : ExprDotEvalSumMethod
        {
            private long cnt;
            private long sum;

            public void Enter(object value)
            {
                if (value == null) {
                    return;
                }

                cnt++;
                sum += value.AsInt64();
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