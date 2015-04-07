///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// An order-by processor that sorts events according to the expressions
	/// in the order_by clause.
	/// </summary>
	public class OrderByProcessorRowLimitOnlyFactory : OrderByProcessorFactory
    {
	    private readonly RowLimitProcessorFactory _rowLimitProcessorFactory;

	    public OrderByProcessorRowLimitOnlyFactory(RowLimitProcessorFactory rowLimitProcessorFactory)
        {
	        _rowLimitProcessorFactory = rowLimitProcessorFactory;
	    }

	    public OrderByProcessor Instantiate(AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
        {
	        RowLimitProcessor rowLimitProcessor = _rowLimitProcessorFactory.Instantiate(agentInstanceContext);
	        return new OrderByProcessorRowLimitOnly(rowLimitProcessor);
	    }
	}
} // end of namespace
