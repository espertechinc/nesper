///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyUtil
    {
        public static AggregationRowPair GetRow(ObjectArrayBackedEventBean eventBean)
        {
            return (AggregationRowPair) eventBean.Properties[0];
        }
    
        internal static IDictionary<String, object> EvalMap(ObjectArrayBackedEventBean @event, AggregationRowPair row, IDictionary<String, TableMetadataColumn> items, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var cols = new Dictionary<string, object>();
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            foreach (var entry in items)
            {
                if (entry.Value is TableMetadataColumnPlain) {
                    var plain = (TableMetadataColumnPlain) entry.Value;
                    cols.Put(entry.Key, @event.Properties[plain.IndexPlain]);
                }
                else {
                    var aggcol = (TableMetadataColumnAggregation) entry.Value;
                    if (!aggcol.Factory.IsAccessAggregation) {
                        cols.Put(entry.Key, row.Methods[aggcol.MethodOffset].Value);
                    }
                    else {
                        var pair = aggcol.AccessAccessorSlotPair;
                        var value = pair.Accessor.GetValue(row.States[pair.Slot], evaluateParams);
                        cols.Put(entry.Key, value);
                    }
                }
            }
            return cols;
        }
    
        internal static object[] EvalTypable(ObjectArrayBackedEventBean @event,
                                             AggregationRowPair row,
                                             IDictionary<String, TableMetadataColumn> items,
                                             EventBean[] eventsPerStream,
                                             bool isNewData,
                                             ExprEvaluatorContext exprEvaluatorContext)
        {
            var values = new object[items.Count];
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            var count = 0;
            foreach (var entry in items)
            {
                if (entry.Value is TableMetadataColumnPlain)
                {
                    var plain = (TableMetadataColumnPlain) entry.Value;
                    values[count] = @event.Properties[plain.IndexPlain];
                }
                else
                {
                    var aggcol = (TableMetadataColumnAggregation) entry.Value;
                    if (!aggcol.Factory.IsAccessAggregation)
                    {
                        values[count] = row.Methods[aggcol.MethodOffset].Value;
                    }
                    else
                    {
                        var pair = aggcol.AccessAccessorSlotPair;
                        values[count] = pair.Accessor.GetValue(row.States[pair.Slot], evaluateParams);
                    }
                }
                count++;
            }
            return values;
        }
    
        internal static object EvalAccessorGetValue(AggregationRowPair row, AggregationAccessorSlotPair pair, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext context)
        {
            return pair.Accessor.GetValue(row.States[pair.Slot], new EvaluateParams(eventsPerStream, newData, context));
        }
    
        internal static ICollection<EventBean> EvalGetROCollectionEvents(AggregationRowPair row, AggregationAccessorSlotPair pair, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext context)
        {
            return pair.Accessor.GetEnumerableEvents(row.States[pair.Slot], new EvaluateParams(eventsPerStream, newData, context));
        }
    
        internal static EventBean EvalGetEventBean(AggregationRowPair row, AggregationAccessorSlotPair pair, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext context)
        {
            return pair.Accessor.GetEnumerableEvent(row.States[pair.Slot], new EvaluateParams(eventsPerStream, newData, context));
        }
    
        internal static ICollection<object> EvalGetROCollectionScalar(AggregationRowPair row, AggregationAccessorSlotPair pair, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext context)
        {
            return pair.Accessor.GetEnumerableScalar(row.States[pair.Slot], new EvaluateParams(eventsPerStream, newData, context));
        }
    
        internal static object EvalMethodGetValue(AggregationRowPair row, int index)
        {
            return row.Methods[index].Value;
        }
    }
}
