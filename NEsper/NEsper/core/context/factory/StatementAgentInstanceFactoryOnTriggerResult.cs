///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.pattern;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactoryOnTriggerResult : StatementAgentInstanceFactoryResult
    {
        public StatementAgentInstanceFactoryOnTriggerResult(
            Viewable finalView,
            StopCallback stopCallback,
            AgentInstanceContext agentInstanceContext,
            AggregationService aggregationService,
            IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategies,
            EvalRootState optPatternRoot,
            IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> tableAccessStrategies,
            ViewableActivationResult viewableActivationResult) : base(
                finalView,
                stopCallback,
                agentInstanceContext,
                aggregationService,
                subselectStrategies,
                Collections.GetEmptyMap<ExprPriorNode, ExprPriorEvalStrategy>(),
                Collections.GetEmptyMap<ExprPreviousNode, ExprPreviousEvalStrategy>(),
                null,
                tableAccessStrategies,
                Collections.GetEmptyList<StatementAgentInstancePreload>())
        {
            OptPatternRoot = optPatternRoot;
            ViewableActivationResult = viewableActivationResult;
        }

        public EvalRootState OptPatternRoot { get; private set; }

        public ViewableActivationResult ViewableActivationResult { get; private set; }
    }
}
