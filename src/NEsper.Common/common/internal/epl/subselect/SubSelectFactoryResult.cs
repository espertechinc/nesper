///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectFactoryResult
    {
        private readonly ViewableActivationResult subselectActivationResult;
        private readonly SubordTableLookupStrategy lookupStrategy;
        private readonly SubselectAggregationPreprocessorBase subselectAggregationPreprocessor;
        private readonly AggregationService aggregationService;
        private readonly PriorEvalStrategy priorStrategy;
        private readonly PreviousGetterStrategy previousStrategy;
        private readonly Viewable subselectView;
        private readonly EventTable[] indexes;

        public SubSelectFactoryResult(
            ViewableActivationResult subselectActivationResult,
            SubSelectStrategyRealization realization,
            SubordTableLookupStrategy lookupStrategy)
        {
            this.subselectActivationResult = subselectActivationResult;
            this.lookupStrategy = lookupStrategy;
            subselectAggregationPreprocessor = realization.SubselectAggregationPreprocessor;
            aggregationService = realization.AggregationService;
            priorStrategy = realization.PriorStrategy;
            previousStrategy = realization.PreviousStrategy;
            subselectView = realization.SubselectView;
            indexes = realization.Indexes;
        }

        public ViewableActivationResult SubselectActivationResult => subselectActivationResult;
        public SubordTableLookupStrategy LookupStrategy => lookupStrategy;

        public SubselectAggregationPreprocessorBase SubselectAggregationPreprocessor =>
            subselectAggregationPreprocessor;

        public AggregationService AggregationService => aggregationService;
        public PriorEvalStrategy PriorStrategy => priorStrategy;
        public PreviousGetterStrategy PreviousStrategy => previousStrategy;
        public Viewable SubselectView => subselectView;

        public EventTable[] Indexes => indexes;
    }
} // end of namespace