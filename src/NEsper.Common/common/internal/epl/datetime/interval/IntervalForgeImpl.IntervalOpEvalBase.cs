using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public abstract class IntervalOpEvalBase : IntervalOpEval
        {
            protected readonly IntervalComputerEval intervalComputer;

            public IntervalOpEvalBase(IntervalComputerEval intervalComputer)
            {
                this.intervalComputer = intervalComputer;
            }

            public abstract object Evaluate(
                long startTs,
                long endTs,
                object parameter,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context);
        }
    }
}