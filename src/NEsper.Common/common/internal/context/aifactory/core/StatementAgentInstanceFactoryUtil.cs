///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.order;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.common.@internal.context.aifactory.core
{
    public class StatementAgentInstanceFactoryUtil
    {
        public static Pair<ResultSetProcessor, AggregationService> StartResultSetAndAggregation(
            ResultSetProcessorFactoryProvider resultSetProcessorPrototype,
            ExprEvaluatorContext exprEvaluatorContext,
            bool isSubquery,
            int? subqueryNumber)
        {
            AggregationService aggregationService = null;
            if (resultSetProcessorPrototype.AggregationServiceFactory != null) {
                aggregationService = resultSetProcessorPrototype.AggregationServiceFactory.MakeService(
                    exprEvaluatorContext,
                    null,
                    subqueryNumber,
                    null);
            }

            OrderByProcessor orderByProcessor = null;
            if (resultSetProcessorPrototype.OrderByProcessorFactory != null) {
                orderByProcessor =
                    resultSetProcessorPrototype.OrderByProcessorFactory.Instantiate(exprEvaluatorContext);
            }

            var resultSetProcessor = resultSetProcessorPrototype.ResultSetProcessorFactory.Instantiate(
                orderByProcessor,
                aggregationService,
                exprEvaluatorContext);

            return new Pair<ResultSetProcessor, AggregationService>(resultSetProcessor, aggregationService);
        }
    }
} // end of namespace