///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationTAAReaderLinearFirstLast : AggregationMultiFunctionTableReader
    {
        private AggregationAccessorLinearType accessType;
        private ExprEvaluator optionalEvaluator;

        public void SetAccessType(AggregationAccessorLinearType accessType)
        {
            this.accessType = accessType;
        }

        public void SetOptionalEvaluator(ExprEvaluator optionalEvaluator)
        {
            this.optionalEvaluator = optionalEvaluator;
        }

        public object GetValue(
            int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var events = (IList<EventBean>) row.GetCollectionOfEvents(
                aggColNum, eventsPerStream, isNewData, exprEvaluatorContext);
            if (events == null) {
                return null;
            }

            EventBean target;
            if (accessType == AggregationAccessorLinearType.FIRST) {
                target = events[0];
            }
            else {
                target = events.Get(events.Count - 1);
            }

            if (optionalEvaluator == null) {
                return target.Underlying;
            }

            var eventsPerStreamBuf = {target};
            return optionalEvaluator.Evaluate(eventsPerStreamBuf, isNewData, exprEvaluatorContext);
        }

        public ICollection<object> GetValueCollectionScalar(
            int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public ICollection<object> GetValueCollectionEvents(
            int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public EventBean GetValueEventBean(
            int aggColNum, AggregationRow row, EventBean[] eventsPerStream, bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var events = (IList<EventBean>) row.GetCollectionOfEvents(
                aggColNum, eventsPerStream, isNewData, exprEvaluatorContext);
            if (events == null) {
                return null;
            }

            if (accessType == AggregationAccessorLinearType.FIRST) {
                return events[0];
            }

            return events.Get(events.Count - 1);
        }
    }
} // end of namespace