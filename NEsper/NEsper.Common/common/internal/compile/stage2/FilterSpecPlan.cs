///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
	public class FilterSpecPlan {
	    public readonly static FilterSpecPlan EMPTY_PLAN;

	    static FilterSpecPlan() {
	        EMPTY_PLAN = new FilterSpecPlan(FilterSpecPlanPath.EMPTY_ARRAY, null, null);
	        EMPTY_PLAN.Initialize();
	    }

	    private FilterSpecPlanPath[] _paths;
	    private ExprEvaluator _filterConfirm;
	    private ExprEvaluator _filterNegate;
	    private MatchedEventConvertor _convertor;
	    private FilterSpecPlanCompute _compute;

	    public FilterSpecPlan() {
	    }

	    public FilterSpecPlan(FilterSpecPlanPath[] paths, ExprEvaluator filterConfirm, ExprEvaluator controlNegate) {
	        this._paths = paths;
	        this._filterConfirm = filterConfirm;
	        this._filterNegate = controlNegate;
	    }

	    public FilterSpecPlanPath[] Paths {
		    get => _paths;
		    set => _paths = value;
	    }

	    public ExprEvaluator FilterConfirm {
		    get => _filterConfirm;
		    set => _filterConfirm = value;
	    }

	    public ExprEvaluator FilterNegate {
		    get => _filterNegate;
		    set => _filterNegate = value;
	    }

	    public MatchedEventConvertor Convertor {
		    get => _convertor;
		    set => _convertor = value;
	    }

	    public FilterSpecPlanCompute Compute {
		    get => _compute;
		    set => _compute = value;
	    }

	    public void Initialize() {
	        _compute = FilterSpecPlanComputeFactory.Make(this);
	    }

	    public FilterValueSetParam[][] EvaluateValueSet(
		    MatchedEventMap matchedEvents,
		    ExprEvaluatorContext exprEvaluatorContext, 
		    StatementContextFilterEvalEnv filterEvalEnv)
	    {
	        return _compute.Compute(this, matchedEvents, exprEvaluatorContext, filterEvalEnv);
	    }
	}
} // end of namespace
