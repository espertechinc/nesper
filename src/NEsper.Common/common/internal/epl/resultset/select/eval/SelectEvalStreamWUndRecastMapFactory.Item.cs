using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.resultset.@select.eval
{
    public partial class SelectEvalStreamWUndRecastMapFactory
    {
        internal class Item
        {
            private readonly int toIndex;
            private readonly string optionalPropertyName;
            private readonly ExprForge forge;
            private readonly TypeWidenerSPI optionalWidener;

            private ExprEvaluator evaluatorAssigned;

            internal Item(
                int toIndex,
                string optionalPropertyName,
                ExprForge forge,
                TypeWidenerSPI optionalWidener)
            {
                this.toIndex = toIndex;
                this.optionalPropertyName = optionalPropertyName;
                this.forge = forge;
                this.optionalWidener = optionalWidener;
            }

            public int ToIndex => toIndex;

            public string OptionalPropertyName => optionalPropertyName;

            public ExprForge Forge => forge;

            public TypeWidenerSPI OptionalWidener => optionalWidener;

            public ExprEvaluator EvaluatorAssigned {
                get => evaluatorAssigned;
                set => evaluatorAssigned = value;
            }
        }
    }
}