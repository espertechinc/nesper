using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.singlelambdaopt3form.sumof
{
    public partial class ExprDotForgeSumOf
    {
        private class ExprDotEvalSumMethodInteger : ExprDotEvalSumMethod
        {
            private int _sum;
            private long _cnt;

            public void Enter(object @object)
            {
                if (@object == null) {
                    return;
                }

                _cnt++;
                _sum += @object.AsInt32();
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