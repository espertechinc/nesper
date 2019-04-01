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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.indexlookupplan;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.inkeyword
{
	/// <summary>
	/// Lookup on an index using a set of expression results as key values.
	/// </summary>
	public class InKeywordSingleTableLookupStrategyExpr : JoinExecTableLookupStrategy {
	    private readonly InKeywordTableLookupPlanSingleIdxFactory factory;
	    private readonly PropertyHashedEventTable index;
	    private readonly EventBean[] eventsPerStream;

	    public InKeywordSingleTableLookupStrategyExpr(InKeywordTableLookupPlanSingleIdxFactory factory, PropertyHashedEventTable index) {
	        this.factory = factory;
	        this.index = index;
	        this.eventsPerStream = new EventBean[factory.LookupStream + 1];
	    }

	    public ISet<EventBean> Lookup(EventBean theEvent, Cursor cursor, ExprEvaluatorContext exprEvaluatorContext) {
	        InstrumentationCommon instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
	        instrumentationCommon.QIndexJoinLookup(this, index);

	        eventsPerStream[factory.LookupStream] = theEvent;
	        ISet<EventBean> result = InKeywordTableLookupUtil.SingleIndexLookup(factory.Expressions, eventsPerStream, exprEvaluatorContext, index);

	        instrumentationCommon.AIndexJoinLookup(result, null);
	        return result;
	    }

	    public override string ToString() {
	        return "IndexedTableLookupStrategyExpr expressions" +
	                " index=(" + index + ')';
	    }

	    public LookupStrategyType LookupStrategyType
	    {
	        get => LookupStrategyType.INKEYWORDSINGLEIDX;
	    }
	}
} // end of namespace