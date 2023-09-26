using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public class IntervalOpCalWithEndEval : IntervalOpEvalDateWithEndBase
        {
            public IntervalOpCalWithEndEval(
                IntervalComputerEval intervalComputer,
                ExprEvaluator evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)

            {
            }

            public override object Evaluate(
                long startTs,
                long endTs,
                object parameterStartTs,
                object parameterEndTs,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                return intervalComputer.Compute(
                    startTs,
                    endTs,
                    ((DateTimeEx) parameterStartTs).UtcMillis,
                    ((DateTimeEx) parameterEndTs).UtcMillis,
                    eventsPerStream,
                    isNewData,
                    context);
            }
        }
    }
}