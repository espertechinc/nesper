///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Sorter and row limiter in one: sorts using a sorter and row limits
    /// </summary>
    public class OrderByProcessorOrderedLimitFactory : OrderByProcessorFactory
    {
        private readonly OrderByProcessorFactoryImpl _orderByProcessorFactory;
        private readonly RowLimitProcessorFactory _rowLimitProcessorFactory;

        public OrderByProcessorOrderedLimitFactory(OrderByProcessorFactoryImpl orderByProcessorFactory, RowLimitProcessorFactory rowLimitProcessorFactory)
        {
            _orderByProcessorFactory = orderByProcessorFactory;
            _rowLimitProcessorFactory = rowLimitProcessorFactory;
        }
    
        public OrderByProcessor Instantiate(AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
        {
            var orderByProcessor = (OrderByProcessorImpl) _orderByProcessorFactory.Instantiate(aggregationService, agentInstanceContext);
            var rowLimitProcessor = _rowLimitProcessorFactory.Instantiate(agentInstanceContext);
            return new OrderByProcessorOrderedLimit(orderByProcessor, rowLimitProcessor);
        }
    }
}
