///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.factory
{
	public class AggregationStateFactoryMinMaxByEver : AggregationStateFactory
    {
	    protected internal readonly ExprAggMultiFunctionSortedMinMaxByNode Expr;
        protected internal readonly AggregationStateMinMaxByEverSpec Spec;

	    public AggregationStateFactoryMinMaxByEver(ExprAggMultiFunctionSortedMinMaxByNode expr, AggregationStateMinMaxByEverSpec spec)
        {
	        Expr = expr;
	        Spec = spec;
	    }

	    public AggregationState CreateAccess(
	        int agentInstanceId, 
	        bool join, 
	        object groupKey, 
	        AggregationServicePassThru passThru)
        {
            if (Spec.OptionalFilter != null) {
                return new AggregationStateMinMaxByEverWFilter(Spec);
            }
	        return new AggregationStateMinMaxByEver(Spec);
	    }

	    public ExprNode AggregationExpression => Expr;
    }
} // end of namespace
