///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.approx;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.factory
{
	public class AggregationStateFactoryCountMinSketch : AggregationStateFactory
	{
	    public AggregationStateFactoryCountMinSketch(ExprAggCountMinSketchNode parent, CountMinSketchSpec specification)
        {
	        Parent = parent;
	        Specification = specification;
	    }

	    public AggregationState CreateAccess(int agentInstanceId, bool join, object groupKey, AggregationServicePassThru passThru)
        {
	        return new CountMinSketchAggState(CountMinSketchState.MakeState(Specification), Specification.Agent);
	    }

	    public ExprNode AggregationExpression => Parent;

	    public CountMinSketchSpec Specification { get; private set; }

	    public ExprAggCountMinSketchNode Parent { get; private set; }
	}
} // end of namespace
