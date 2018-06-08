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
using com.espertech.esper.plugin;

namespace com.espertech.esper.epl.agg.factory
{
	public class AggregationStateFactoryPlugin : AggregationStateFactory
    {
	    protected internal readonly ExprPlugInAggMultiFunctionNodeFactory Parent;
	    protected internal readonly PlugInAggregationMultiFunctionStateFactory StateFactory;

	    public AggregationStateFactoryPlugin(ExprPlugInAggMultiFunctionNodeFactory parent)
        {
	        Parent = parent;
	        StateFactory = parent.HandlerPlugin.StateFactory;
	    }

	    public AggregationState CreateAccess(int agentInstanceId, bool join, object groupBy, AggregationServicePassThru passThru)
        {
	        var context = new PlugInAggregationMultiFunctionStateContext(agentInstanceId, groupBy);
	        return StateFactory.MakeAggregationState(context);
	    }

	    public ExprNode AggregationExpression => Parent.AggregationExpression;
    }
} // end of namespace
