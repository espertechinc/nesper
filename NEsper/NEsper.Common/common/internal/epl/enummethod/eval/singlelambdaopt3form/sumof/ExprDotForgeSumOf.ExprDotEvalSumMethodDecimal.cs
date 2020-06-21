using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodDecimal : ExprDotEvalSumMethod
        {
            private long cnt;
            private decimal sum;

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                cnt++;
                sum += @object.AsDecimal();
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