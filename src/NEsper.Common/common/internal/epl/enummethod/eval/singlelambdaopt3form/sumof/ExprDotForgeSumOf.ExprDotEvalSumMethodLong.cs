using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodLong : ExprDotEvalSumMethod
        {
            private long _cnt;
            private long _sum;

            public void Enter(object value)
            {
                if (value == null) {
                    return;
                }

                _cnt++;
                _sum += value.AsInt64();
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