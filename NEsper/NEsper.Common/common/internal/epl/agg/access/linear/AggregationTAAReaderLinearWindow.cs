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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.access.linear
{
    public class AggregationTAAReaderLinearWindow : AggregationMultiFunctionTableReader
    {
        private Type componentType;
        private ExprEvaluator optionalEvaluator;

        public Type ComponentType {
            get => componentType;
            set => componentType = value;
        }

        public ExprEvaluator OptionalEvaluator {
            get => optionalEvaluator;
            set => optionalEvaluator = value;
        }

        public AggregationTAAReaderLinearWindow SetComponentType(Type componentType)
        {
            this.componentType = componentType;
            return this;
        }

        public AggregationTAAReaderLinearWindow SetOptionalEvaluator(ExprEvaluator optionalEvaluator)
        {
            this.optionalEvaluator = optionalEvaluator;
            return this;
        }

        public object GetValue(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var linear = (IList<EventBean>) row.GetCollectionOfEvents(
                aggColNum,
                eventsPerStream,
                isNewData,
                exprEvaluatorContext);
            if (linear == null) {
                return null;
            }

            var array = Array.CreateInstance(componentType, linear.Count);
            using (var enumerator = linear.GetEnumerator()) {
                var count = 0;
                if (optionalEvaluator == null) {
                    while (enumerator.MoveNext()) {
                        var bean = enumerator.Current;
                        array.SetValue(bean.Underlying, count++);
                    }
                }
                else {
                    var events = new EventBean[1];
                    while (enumerator.MoveNext())
                    {
                        events[0] = enumerator.Current;
                        array.SetValue(optionalEvaluator.Evaluate(events, isNewData, exprEvaluatorContext), count++);
                    }
                }
            }

            return array;
        }

        public ICollection<EventBean> GetValueCollectionEvents(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return row.GetCollectionOfEvents(aggColNum, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public ICollection<object> GetValueCollectionScalar(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var linear = (IList<EventBean>) row.GetCollectionOfEvents(
                aggColNum,
                eventsPerStream,
                isNewData,
                exprEvaluatorContext);
            if (linear == null || linear.IsEmpty()) {
                return null;
            }

            IList<object> values = new List<object>(linear.Count);
            using (var enumerator = linear.GetEnumerator()) {
                var eventsPerStreamBuf = new EventBean[1];
                while(enumerator.MoveNext()) {
                    eventsPerStreamBuf[0] = enumerator.Current;
                    var value = optionalEvaluator.Evaluate(eventsPerStreamBuf, true, null);
                    values.Add(value);
                }

                return values;
            }
        }

        public EventBean GetValueEventBean(
            int aggColNum,
            AggregationRow row,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }
    }
} // end of namespace