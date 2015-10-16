///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.subselect;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.pattern;
using com.espertech.esper.rowregex;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
    public class StatementAgentInstanceFactorySelectResult : StatementAgentInstanceFactoryResult
    {
        public StatementAgentInstanceFactorySelectResult(
            Viewable finalView,
            StopCallback stopCallback,
            AgentInstanceContext agentInstanceContext,
            AggregationService optionalAggegationService,
            IDictionary<ExprSubselectNode, SubSelectStrategyHolder> subselectStrategies,
            IDictionary<ExprPriorNode, ExprPriorEvalStrategy> priorNodeStrategies,
            IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> previousNodeStrategies,
            RegexExprPreviousEvalStrategy regexExprPreviousEvalStrategy,
            IDictionary<ExprTableAccessNode, ExprTableAccessEvalStrategy> tableAccessStrategies,
            IList<StatementAgentInstancePreload> preloadList,
            EvalRootState[] patternRoots,
            StatementAgentInstancePostLoad optionalPostLoadJoin,
            Viewable[] topViews,
            Viewable[] eventStreamViewables,
            ViewableActivationResult[] viewableActivationResults) : base(
                finalView, stopCallback, agentInstanceContext, optionalAggegationService, subselectStrategies,
                priorNodeStrategies, previousNodeStrategies, regexExprPreviousEvalStrategy, tableAccessStrategies, preloadList)
        {
            TopViews = topViews;
            PatternRoots = patternRoots;
            OptionalPostLoadJoin = optionalPostLoadJoin;
            EventStreamViewables = eventStreamViewables;
            ViewableActivationResults = viewableActivationResults;
        }

        public Viewable[] TopViews { get; private set; }

        public EvalRootState[] PatternRoots { get; private set; }

        public StatementAgentInstancePostLoad OptionalPostLoadJoin { get; private set; }

        public Viewable[] EventStreamViewables { get; private set; }

        public ViewableActivationResult[] ViewableActivationResults { get; private set; }
    }
}