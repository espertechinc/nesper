using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.resultset.@select.eval
{
    public partial class SelectEvalStreamWUndRecastObjectArrayFactory
    {
        private class Item
        {
            private readonly int toIndex;
            private readonly int optionalFromIndex;
            private readonly ExprForge forge;
            private readonly TypeWidenerSPI optionalWidener;
            private ExprEvaluator evaluatorAssigned;

            internal Item(
                int toIndex,
                int optionalFromIndex,
                ExprForge forge,
                TypeWidenerSPI optionalWidener)
            {
                this.toIndex = toIndex;
                this.optionalFromIndex = optionalFromIndex;
                this.forge = forge;
                this.optionalWidener = optionalWidener;
            }

            public int ToIndex => toIndex;

            public int OptionalFromIndex => optionalFromIndex;

            public ExprForge Forge => forge;

            public TypeWidenerSPI OptionalWidener => optionalWidener;

            public ExprEvaluator EvaluatorAssigned {
                get => evaluatorAssigned;

                set => evaluatorAssigned = value;
            }
        }
    }
}