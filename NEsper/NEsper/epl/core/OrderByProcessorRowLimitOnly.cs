///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// An order-by processor that sorts events according to the expressions
	/// in the order_by clause.
	/// </summary>
	public class OrderByProcessorRowLimitOnly : OrderByProcessor
    {
	    private readonly RowLimitProcessor _rowLimitProcessor;

	    public OrderByProcessorRowLimitOnly(RowLimitProcessor rowLimitProcessor)
        {
	        _rowLimitProcessor = rowLimitProcessor;
	    }

	    public EventBean[] Sort(EventBean[] outgoingEvents, EventBean[][] generatingEvents, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        return _rowLimitProcessor.DetermineLimitAndApply(outgoingEvents);
	    }

	    public EventBean[] Sort(EventBean[] outgoingEvents, EventBean[][] generatingEvents, object[] groupByKeys, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        return _rowLimitProcessor.DetermineLimitAndApply(outgoingEvents);
	    }

	    public EventBean[] Sort(EventBean[] outgoingEvents, IList<GroupByRollupKey> currentGenerators, bool newData, AgentInstanceContext agentInstanceContext, OrderByElement[][] elementsPerLevel) {
	        return _rowLimitProcessor.DetermineLimitAndApply(outgoingEvents);
	    }

	    public object GetSortKey(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        return null;
	    }

	    public object GetSortKey(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext, OrderByElement[] elementsForLevel) {
	        return null;
	    }

	    public object[] GetSortKeyPerRow(EventBean[] generatingEvents, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        return null;
	    }

	    public EventBean[] Sort(EventBean[] outgoingEvents, object[] orderKeys, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        return _rowLimitProcessor.DetermineLimitAndApply(outgoingEvents);
	    }
	}
} // end of namespace
