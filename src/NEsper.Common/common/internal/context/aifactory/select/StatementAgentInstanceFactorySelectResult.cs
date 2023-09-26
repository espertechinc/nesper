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
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public class StatementAgentInstanceFactorySelectResult : StatementAgentInstanceFactoryResult
    {
        public StatementAgentInstanceFactorySelectResult(
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
            EvalRootState[] patternRoots,
            JoinSetComposer joinSetComposer,
            Viewable[] topViews,
            Viewable[] eventStreamViewables,
            ViewableActivationResult[] viewableActivationResults,
            ResultSetProcessor resultSetProcessor)
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
            TopViews = topViews;
            PatternRoots = patternRoots;
            JoinSetComposer = joinSetComposer;
            EventStreamViewables = eventStreamViewables;
            ViewableActivationResults = viewableActivationResults;
            ResultSetProcessor = resultSetProcessor;
        }

        public Viewable[] TopViews { get; }

        public EvalRootState[] PatternRoots { get; }

        public Viewable[] EventStreamViewables { get; }

        public ViewableActivationResult[] ViewableActivationResults { get; }

        public JoinSetComposer JoinSetComposer { get; }

        public ResultSetProcessor ResultSetProcessor { get; }
    }
} // end of namespace