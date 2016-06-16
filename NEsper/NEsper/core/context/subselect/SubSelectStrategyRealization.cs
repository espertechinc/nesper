///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.factory;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.subquery;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.subselect
{
    /// <summary>
    /// Record holding lookup resource references for use by <seealso cref="com.espertech.esper.core.context.subselect.SubSelectActivationCollection" />.
    /// </summary>
    public class SubSelectStrategyRealization
    {
        public SubSelectStrategyRealization(
            SubordTableLookupStrategy strategy,
            SubselectAggregationPreprocessorBase subselectAggregationPreprocessor,
            AggregationService subselectAggregationService,
            IDictionary<ExprPriorNode, ExprPriorEvalStrategy> priorNodeStrategies,
            IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> previousNodeStrategies,
            Viewable subselectView,
            StatementAgentInstancePostLoad postLoad)
        {
            Strategy = strategy;
            SubselectAggregationPreprocessor = subselectAggregationPreprocessor;
            SubselectAggregationService = subselectAggregationService;
            PriorNodeStrategies = priorNodeStrategies;
            PreviousNodeStrategies = previousNodeStrategies;
            SubselectView = subselectView;
            PostLoad = postLoad;
        }

        public SubordTableLookupStrategy Strategy { get; private set; }

        public SubselectAggregationPreprocessorBase SubselectAggregationPreprocessor { get; private set; }

        public AggregationService SubselectAggregationService { get; private set; }

        public IDictionary<ExprPriorNode, ExprPriorEvalStrategy> PriorNodeStrategies { get; private set; }

        public IDictionary<ExprPreviousNode, ExprPreviousEvalStrategy> PreviousNodeStrategies { get; private set; }

        public Viewable SubselectView { get; private set; }

        public StatementAgentInstancePostLoad PostLoad { get; private set; }
    }
}