///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
	public class AggregationTAAReaderSortedWindow : AggregationMultiFunctionTableReader {
	    public object GetValue(int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
	        return null;
	    }

	    public ICollection<object> GetValueCollectionEvents(int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
	        AggregationStateSorted sorted = (AggregationStateSorted) row.GetAccessState(aggColNum);
	        return sorted.CollectionReadOnly();
	    }

	    public ICollection<object> GetValueCollectionScalar(int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
	        return null;
	    }

	    public EventBean GetValueEventBean(int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
	        return null;
	    }
	}
} // end of namespace