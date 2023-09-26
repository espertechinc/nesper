using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalForgeImpl
    {
        public class IntervalOpDateTimeWithEndEval : IntervalOpEvalDateWithEndBase
        {
            public IntervalOpDateTimeWithEndEval(
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
                    DatetimeLongCoercerDateTime.CoerceToMillis((DateTime) parameterStartTs),
                    DatetimeLongCoercerDateTime.CoerceToMillis((DateTime) parameterEndTs),
                    eventsPerStream,
                    isNewData,
                    context);
            }
        }
    }
}