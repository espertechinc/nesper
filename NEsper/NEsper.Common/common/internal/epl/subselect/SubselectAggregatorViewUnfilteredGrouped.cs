///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.subselect
{
	public class SubselectAggregatorViewUnfilteredGrouped : SubselectAggregatorViewBase {
	    public SubselectAggregatorViewUnfilteredGrouped(AggregationService aggregationService, ExprEvaluator optionalFilterExpr, ExprEvaluatorContext exprEvaluatorContext, ExprEvaluator groupKeys) : base(aggregationService, optionalFilterExpr, exprEvaluatorContext, groupKeys)
	        {
	    }

	    public override void Update(EventBean[] newData, EventBean[] oldData) {
	        exprEvaluatorContext.InstrumentationProvider.QSubselectAggregation();

	        if (newData != null) {
	            foreach (EventBean theEvent in newData) {
	                eventsPerStream[0] = theEvent;
	                object groupKey = GenerateGroupKey(true);
	                aggregationService.ApplyEnter(eventsPerStream, groupKey, exprEvaluatorContext);
	            }
	        }

	        if (oldData != null) {
	            foreach (EventBean theEvent in oldData) {
	                eventsPerStream[0] = theEvent;
	                object groupKey = GenerateGroupKey(false);
	                aggregationService.ApplyLeave(eventsPerStream, groupKey, exprEvaluatorContext);
	            }
	        }

	        exprEvaluatorContext.InstrumentationProvider.ASubselectAggregation();
	    }
	}
} // end of namespace