using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public abstract class IntervalOpEvalDateWithEndBase : IntervalOpEval
        {
            protected readonly IntervalComputerEval intervalComputer;
            private readonly ExprEvaluator evaluatorEndTimestamp;

            protected IntervalOpEvalDateWithEndBase(
                IntervalComputerEval intervalComputer,
                ExprEvaluator evaluatorEndTimestamp)
            {
                this.intervalComputer = intervalComputer;
                this.evaluatorEndTimestamp = evaluatorEndTimestamp;
            }

            public abstract object Evaluate(
                long startTs,
                long endTs,
                object parameterStartTs,
                object parameterEndTs,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context);

            public object Evaluate(
                long startTs,
                long endTs,
                object parameterStartTs,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                var paramEndTs = evaluatorEndTimestamp.Evaluate(eventsPerStream, isNewData, context);
                if (paramEndTs == null) {
                    return null;
                }

                return Evaluate(startTs, endTs, parameterStartTs, paramEndTs, eventsPerStream, isNewData, context);
            }
        }
    }
}