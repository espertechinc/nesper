using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.output.core;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        private class JoinPlanResult
        {
            private readonly OutputProcessView outputProcessView;
            private readonly JoinPreloadMethod preloadMethod;
            private readonly JoinSetComposerDesc joinSetComposerDesc;

            public JoinPlanResult(
                OutputProcessView viewable,
                JoinPreloadMethod preloadMethod,
                JoinSetComposerDesc joinSetComposerDesc)
            {
                outputProcessView = viewable;
                this.preloadMethod = preloadMethod;
                this.joinSetComposerDesc = joinSetComposerDesc;
            }

            public OutputProcessView Viewable => outputProcessView;

            public JoinPreloadMethod PreloadMethod => preloadMethod;

            public JoinSetComposerDesc JoinSetComposerDesc => joinSetComposerDesc;
        }
    }
}