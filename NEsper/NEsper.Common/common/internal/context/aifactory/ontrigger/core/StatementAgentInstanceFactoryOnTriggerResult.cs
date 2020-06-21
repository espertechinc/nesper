///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.core
{
    public class StatementAgentInstanceFactoryOnTriggerResult : StatementAgentInstanceFactoryResult
    {
        public StatementAgentInstanceFactoryOnTriggerResult(
            Viewable finalView,
            AgentInstanceMgmtCallback stopCallback,
            AgentInstanceContext agentInstanceContext,
            AggregationService optionalAggegationService,
            IDictionary<int, SubSelectFactoryResult> subselectStrategies,
            PriorEvalStrategy[] priorStrategies,
            PreviousGetterStrategy[] previousGetterStrategies,
            RowRecogPreviousStrategy regexExprPreviousEvalStrategy,
            IDictionary<int, ExprTableEvalStrategy> tableAccessStrategies,
            IList<StatementAgentInstancePreload> preloadList,
            Runnable postContextMergeRunnable,
            EvalRootState optPatternRoot,
            ViewableActivationResult viewableActivationResult)
            : base(
                finalView,
                stopCallback,
                agentInstanceContext,
                optionalAggegationService,
                subselectStrategies,
                priorStrategies,
                previousGetterStrategies,
                regexExprPreviousEvalStrategy,
                tableAccessStrategies,
                preloadList,
                postContextMergeRunnable)
        {
            OptPatternRoot = optPatternRoot;
            ViewableActivationResult = viewableActivationResult;
        }

        public EvalRootState OptPatternRoot { get; }

        public ViewableActivationResult ViewableActivationResult { get; }
    }
} // end of namespace