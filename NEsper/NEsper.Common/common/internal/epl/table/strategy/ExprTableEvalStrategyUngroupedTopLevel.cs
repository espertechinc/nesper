///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public class ExprTableEvalStrategyUngroupedTopLevel : ExprTableEvalStrategyUngroupedBase,
        ExprTableEvalStrategy
    {
        public ExprTableEvalStrategyUngroupedTopLevel(
            TableAndLockProviderUngrouped provider,
            ExprTableEvalStrategyFactory factory)
            : base(provider, factory)
        {
        }

        public override object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            ObjectArrayBackedEventBean @event = LockTableReadAndGet(context);
            if (@event == null) {
                return null;
            }

            AggregationRow row = ExprTableEvalStrategyUtil.GetRow(@event);
            return ExprTableEvalStrategyUtil.EvalMap(
                @event,
                row,
                factory.Table.MetaData.Columns,
                eventsPerStream,
                isNewData,
                context);
        }

        public override object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            ObjectArrayBackedEventBean @event = LockTableReadAndGet(context);
            if (@event == null) {
                return null;
            }

            AggregationRow row = ExprTableEvalStrategyUtil.GetRow(@event);
            IDictionary<string, TableMetadataColumn> items = factory.Table.MetaData.Columns;
            return ExprTableEvalStrategyUtil.EvalTypable(@event, row, items, eventsPerStream, isNewData, context);
        }

        public override ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public override EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }

        public override ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return null;
        }
    }
} // end of namespace