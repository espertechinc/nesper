using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.view.core;

namespace NEsper.Examples.OHLC
{
    public class OHLCBarPlugInViewFactory : ViewFactory
    {
        public ExprEvaluator TimestampExpression { get; set; }

        public ExprEvaluator ValueExpression { get; set; }

        public void Init(
            ViewFactoryContext viewFactoryContext,
            EPStatementInitServices services)
        {
        }

        public View MakeView(AgentInstanceViewFactoryChainContext agentInstanceViewFactoryContext)
        {
            return new OHLCBarPlugInView(this, agentInstanceViewFactoryContext);
        }

        public EventType EventType { get; set; }

        public string ViewName => nameof(OHLCBarPlugInView);
    }
}