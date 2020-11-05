///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.order;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public class StatementAgentInstanceFactoryUtil
    {
        public static Pair<ResultSetProcessor, AggregationService> StartResultSetAndAggregation(
            ResultSetProcessorFactoryProvider resultSetProcessorPrototype,
            AgentInstanceContext agentInstanceContext,
            bool isSubquery,
            int? subqueryNumber)
        {
            AggregationService aggregationService = null;
            if (resultSetProcessorPrototype.AggregationServiceFactory != null) {
                aggregationService = resultSetProcessorPrototype.AggregationServiceFactory.MakeService(
                    agentInstanceContext,
                    null,
                    isSubquery,
                    subqueryNumber,
                    null);
            }

            OrderByProcessor orderByProcessor = null;
            if (resultSetProcessorPrototype.OrderByProcessorFactory != null) {
                orderByProcessor = resultSetProcessorPrototype.OrderByProcessorFactory
                    .Instantiate(agentInstanceContext);
            }

            var resultSetProcessor = resultSetProcessorPrototype.ResultSetProcessorFactory.Instantiate(
                orderByProcessor,
                aggregationService,
                agentInstanceContext);

            return new Pair<ResultSetProcessor, AggregationService>(resultSetProcessor, aggregationService);
        }
    }
} // end of namespace