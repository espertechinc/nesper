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
	public class AggregationStateFactoryLinear : AggregationStateFactory
    {
	    protected internal readonly ExprAggMultiFunctionLinearAccessNode Expr;
        protected internal readonly int StreamNum;

	    public AggregationStateFactoryLinear(ExprAggMultiFunctionLinearAccessNode expr, int streamNum)
        {
	        Expr = expr;
	        StreamNum = streamNum;
	    }

	    public AggregationState CreateAccess(int agentInstanceId, bool join, object groupKey, AggregationServicePassThru passThru)
        {
	        if (join)
            {
	            return new AggregationStateJoinImpl(StreamNum);
	        }
	        return new AggregationStateImpl(StreamNum);
	    }

	    public ExprNode AggregationExpression
	    {
	        get { return Expr; }
	    }
    }
} // end of namespace
