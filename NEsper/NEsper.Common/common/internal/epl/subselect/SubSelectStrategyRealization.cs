///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.subselect
{
	public class SubSelectStrategyRealization {
	    private readonly SubordTableLookupStrategy lookupStrategy;
	    private readonly SubselectAggregationPreprocessorBase subselectAggregationPreprocessor;
	    private readonly AggregationService aggregationService;
	    private readonly PriorEvalStrategy priorStrategy;
	    private readonly PreviousGetterStrategy previousStrategy;
	    private readonly Viewable subselectView;
	    private readonly EventTable[] indexes;

	    public SubSelectStrategyRealization(SubordTableLookupStrategy lookupStrategy, SubselectAggregationPreprocessorBase subselectAggregationPreprocessor, AggregationService aggregationService, PriorEvalStrategy priorStrategy, PreviousGetterStrategy previousStrategy, Viewable subselectView, EventTable[] indexes) {
	        this.lookupStrategy = lookupStrategy;
	        this.subselectAggregationPreprocessor = subselectAggregationPreprocessor;
	        this.aggregationService = aggregationService;
	        this.priorStrategy = priorStrategy;
	        this.previousStrategy = previousStrategy;
	        this.subselectView = subselectView;
	        this.indexes = indexes;
	    }

	    public SubordTableLookupStrategy LookupStrategy
	    {
	        get => lookupStrategy;
	    }

	    public SubselectAggregationPreprocessorBase SubselectAggregationPreprocessor
	    {
	        get => subselectAggregationPreprocessor;
	    }

	    public AggregationService AggregationService
	    {
	        get => aggregationService;
	    }

	    public PriorEvalStrategy PriorStrategy
	    {
	        get => priorStrategy;
	    }

	    public PreviousGetterStrategy PreviousStrategy
	    {
	        get => previousStrategy;
	    }

	    public Viewable SubselectView
	    {
	        get => subselectView;
	    }

	    public EventTable[] Indexes
	    {
	        get => indexes;
	    }
	}
} // end of namespace