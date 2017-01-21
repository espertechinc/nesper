///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.subselect
{
    /// <summary>
    /// Record holding lookup resource references for use by <seealso cref="SubSelectActivationCollection" />
    /// </summary>
    public class SubSelectStrategyHolder
    {
        public SubSelectStrategyHolder(
            ExprSubselectStrategy stategy,
            AggregationService subselectAggregationService,
            IDictionary<ExprPriorNode, ExprPriorEvalStrategy> priorStrategies,
            IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> previousNodeStrategies,
            Viewable subselectView,
            StatementAgentInstancePostLoad postLoad,
            ViewableActivationResult subselectActivationResult)
        {
            Stategy = stategy;
            SubselectAggregationService = subselectAggregationService;
            PriorStrategies = priorStrategies;
            PreviousNodeStrategies = previousNodeStrategies;
            SubselectView = subselectView;
            PostLoad = postLoad;
            SubselectActivationResult = subselectActivationResult;
        }

        public ExprSubselectStrategy Stategy { get; private set; }

        public AggregationService SubselectAggregationService { get; private set; }

        public IDictionary<ExprPriorNode, ExprPriorEvalStrategy> PriorStrategies { get; private set; }

        public IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> PreviousNodeStrategies { get; private set; }

        public Viewable SubselectView { get; private set; }

        public StatementAgentInstancePostLoad PostLoad { get; private set; }

        public ViewableActivationResult SubselectActivationResult { get; private set; }
    }
}