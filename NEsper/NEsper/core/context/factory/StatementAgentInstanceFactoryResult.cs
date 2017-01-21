///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.rowregex;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public abstract class StatementAgentInstanceFactoryResult
    {
        protected StatementAgentInstanceFactoryResult(
            Viewable finalView,
            StopCallback stopCallback,
            AgentInstanceContext agentInstanceContext,
            AggregationService optionalAggegationService,
            IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategies,
            IDictionary<ExprPriorNode, ExprPriorEvalStrategy> priorNodeStrategies,
            IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> previousNodeStrategies,
            RegexExprPreviousEvalStrategy regexExprPreviousEvalStrategy,
            IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> tableAccessStrategies,
            IList<StatementAgentInstancePreload> preloadList)
        {
            FinalView = finalView;
            StopCallback = stopCallback;
            AgentInstanceContext = agentInstanceContext;
            OptionalAggegationService = optionalAggegationService;
            SubselectStrategies = subselectStrategies;
            PriorNodeStrategies = priorNodeStrategies;
            PreviousNodeStrategies = previousNodeStrategies;
            RegexExprPreviousEvalStrategy = regexExprPreviousEvalStrategy;
            TableAccessEvalStrategies = tableAccessStrategies;
            PreloadList = preloadList;
        }

        public Viewable FinalView { get; private set; }

        public StopCallback StopCallback { get; set; }

        public AgentInstanceContext AgentInstanceContext { get; private set; }

        public AggregationService OptionalAggegationService { get; private set; }

        public IDictionary<ExprSubselectNode, SubSelectStrategyHolder> SubselectStrategies { get; private set; }

        public IDictionary<ExprPriorNode, ExprPriorEvalStrategy> PriorNodeStrategies { get; private set; }

        public IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> PreviousNodeStrategies { get; private set; }

        public IList<StatementAgentInstancePreload> PreloadList { get; private set; }

        public RegexExprPreviousEvalStrategy RegexExprPreviousEvalStrategy { get; private set; }

        public IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> TableAccessEvalStrategies { get; private set; }
    }
}