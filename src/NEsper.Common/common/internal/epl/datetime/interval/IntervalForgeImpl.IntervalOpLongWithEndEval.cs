using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public class IntervalOpLongWithEndEval : IntervalOpEvalDateWithEndBase
        {
            public IntervalOpLongWithEndEval(
                IntervalComputerEval intervalComputer,
                ExprEvaluator evaluatorEndTimestamp) : base(intervalComputer, evaluatorEndTimestamp)
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
                    parameterStartTs.AsInt64(),
                    parameterEndTs.AsInt64(),
                    eventsPerStream,
                    isNewData,
                    context);
            }
        }
    }
}