///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectStrategyRealization
    {
        private readonly SubordTableLookupStrategy _lookupStrategy;
        private readonly SubselectAggregationPreprocessorBase _subselectAggregationPreprocessor;
        private readonly AggregationService _aggregationService;
        private readonly PriorEvalStrategy _priorStrategy;
        private readonly PreviousGetterStrategy _previousStrategy;
        private readonly Viewable _subselectView;
        private readonly EventTable[] _indexes;

        public SubSelectStrategyRealization(
            SubordTableLookupStrategy lookupStrategy,
            SubselectAggregationPreprocessorBase subselectAggregationPreprocessor,
            AggregationService aggregationService,
            PriorEvalStrategy priorStrategy,
            PreviousGetterStrategy previousStrategy,
            Viewable subselectView,
            EventTable[] indexes)
        {
            _lookupStrategy = lookupStrategy;
            _subselectAggregationPreprocessor = subselectAggregationPreprocessor;
            _aggregationService = aggregationService;
            _priorStrategy = priorStrategy;
            _previousStrategy = previousStrategy;
            _subselectView = subselectView;
            _indexes = indexes;
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

        public EventTable[] Indexes {
            get => _indexes;
        }
    }
} // end of namespace