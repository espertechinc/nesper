using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public partial class SelectExprProcessorHelper
    {
        private class TypeAndForgePair
        {
            internal TypeAndForgePair(
                object type,
                ExprForge forge)
            {
                this.Type = type;
                this.Forge = forge;
            }

            public ExprForge Forge { get; }

            public object Type { get; }
        }
    }
}