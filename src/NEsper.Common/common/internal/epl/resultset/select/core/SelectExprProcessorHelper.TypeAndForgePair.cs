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
                Type = type;
                Forge = forge;
            }

            public ExprForge Forge { get; }

            public object Type { get; }
        }
    }
}