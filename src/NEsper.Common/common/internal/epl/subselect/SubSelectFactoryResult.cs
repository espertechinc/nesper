///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private readonly ViewableActivationResult _subselectActivationResult;
        private readonly SubordTableLookupStrategy _lookupStrategy;
        private readonly SubselectAggregationPreprocessorBase _subselectAggregationPreprocessor;
        private readonly AggregationService _aggregationService;
        private readonly PriorEvalStrategy _priorStrategy;
        private readonly PreviousGetterStrategy _previousStrategy;
        private readonly Viewable _subselectView;
        private readonly EventTable[] _indexes;

        public SubSelectFactoryResult(
            ViewableActivationResult subselectActivationResult,
            SubSelectStrategyRealization realization,
            SubordTableLookupStrategy lookupStrategy)
        {
            _subselectActivationResult = subselectActivationResult;
            _lookupStrategy = lookupStrategy;
            _subselectAggregationPreprocessor = realization.SubselectAggregationPreprocessor;
            _aggregationService = realization.AggregationService;
            _priorStrategy = realization.PriorStrategy;
            _previousStrategy = realization.PreviousStrategy;
            _subselectView = realization.SubselectView;
            _indexes = realization.Indexes;
        }

        public ViewableActivationResult SubselectActivationResult {
            get => _subselectActivationResult;
        }

        public SubordTableLookupStrategy LookupStrategy {
            get => _lookupStrategy;
        }

        public SubselectAggregationPreprocessorBase SubselectAggregationPreprocessor {
            get => _subselectAggregationPreprocessor;
        }

        public AggregationService AggregationService {
            get => _aggregationService;
        }

        public PriorEvalStrategy PriorStrategy {
            get => _priorStrategy;
        }

        public PreviousGetterStrategy PreviousStrategy {
            get => _previousStrategy;
        }

        public Viewable SubselectView {
            get => _subselectView;
        }

        public EventTable[] GetIndexes()
        {
            return _indexes;
        }
    }
} // end of namespace