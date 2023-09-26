///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.order;
using com.espertech.esper.common.@internal.epl.resultset.select.core;

namespace com.espertech.esper.common.@internal.epl.resultset.handthru
{
    public class ResultSetProcessorHandThroughFactory : ResultSetProcessorFactory
    {
        private readonly SelectExprProcessor selectExprProcessor;
        private readonly EventType resultEventType;
        private readonly bool rstream;

        public ResultSetProcessorHandThroughFactory(
            SelectExprProcessor selectExprProcessor,
            EventType resultEventType,
            bool rstream)
        {
            this.selectExprProcessor = selectExprProcessor;
            this.resultEventType = resultEventType;
            this.rstream = rstream;
        }

        public ResultSetProcessor Instantiate(
            OrderByProcessor orderByProcessor,
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return new ResultSetProcessorHandThroughImpl(this, exprEvaluatorContext);
        }

        public bool IsRstream => rstream;

        public EventType ResultEventType => resultEventType;

        public SelectExprProcessor SelectExprProcessor => selectExprProcessor;
    }
} // end of namespace