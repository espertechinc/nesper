using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodDouble : ExprDotEvalSumMethod
        {
            private long cnt;
            private double sum;

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                cnt++;
                sum += @object.AsDouble();
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