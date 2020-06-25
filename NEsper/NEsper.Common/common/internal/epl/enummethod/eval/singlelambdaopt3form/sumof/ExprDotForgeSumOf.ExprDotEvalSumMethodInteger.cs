using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodInteger : ExprDotEvalSumMethod {
            private int sum;
            private long cnt;

            public void Enter(object @object) {
                if (@object == null) {
                    return;
                }
                cnt++;
                sum += @object.AsInt32();
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