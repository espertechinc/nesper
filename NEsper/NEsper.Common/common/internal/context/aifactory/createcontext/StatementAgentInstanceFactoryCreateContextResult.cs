///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.prior;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createcontext
{
	public class StatementAgentInstanceFactoryCreateContextResult : StatementAgentInstanceFactoryResult {

	    private readonly ContextManagerRealization contextManagerRealization;

	    public StatementAgentInstanceFactoryCreateContextResult(Viewable finalView, AgentInstanceStopCallback stopCallback, AgentInstanceContext agentInstanceContext, AggregationService optionalAggegationService, IDictionary<int, SubSelectFactoryResult> subselectStrategies, PriorEvalStrategy[] priorStrategies, PreviousGetterStrategy[] previousGetterStrategies, RowRecogPreviousStrategy regexExprPreviousEvalStrategy, IDictionary<int, ExprTableEvalStrategy> tableAccessStrategies, IList<StatementAgentInstancePreload> preloadList, ContextManagerRealization contextManagerRealization)

	    	 : base(finalView, stopCallback, agentInstanceContext, optionalAggegationService, subselectStrategies, priorStrategies, previousGetterStrategies, regexExprPreviousEvalStrategy, tableAccessStrategies, preloadList)

	    {
	        this.contextManagerRealization = contextManagerRealization;
	    }

	    public ContextManagerRealization ContextManagerRealization {
	        get => contextManagerRealization;
	    }
	}
} // end of namespace