using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    public abstract class AggregationMultiFunctionAccessorBase : AggregationMultiFunctionAccessor
    {
        public abstract object GetValue(
            AggregationMultiFunctionState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        public ICollection<EventBean> GetEnumerableEvents(
            AggregationMultiFunctionState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public EventBean GetEnumerableEvent(
            AggregationMultiFunctionState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public ICollection<object> GetEnumerableScalar(
            AggregationMultiFunctionState state,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }
    }
}