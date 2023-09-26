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
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public abstract class ExprTableEvalStrategyGroupedBase : ExprTableEvalStrategy
    {
        private readonly ExprTableEvalStrategyFactory _factory;
        private readonly TableAndLockProviderGrouped _provider;

        public ExprTableEvalStrategyFactory Factory => _factory;

        public ExprTableEvalStrategyGroupedBase(
            TableAndLockProviderGrouped provider,
            ExprTableEvalStrategyFactory factory)
        {
            _provider = provider;
            _factory = factory;
        }

        public abstract object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract ICollection<EventBean> EvaluateGetROCollectionEvents(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        public abstract EventBean EvaluateGetEventBean(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        public abstract ICollection<object> EvaluateGetROCollectionScalar(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        public abstract object[] EvaluateTypableSingle(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);


        protected TableInstanceGrouped LockTableRead(ExprEvaluatorContext context)
        {
            var tableAndLockGrouped = _provider.Get();
            TableEvalLockUtil.ObtainLockUnless(tableAndLockGrouped.Lock, context);
            return tableAndLockGrouped.Grouped;
        }

        public AggregationRow GetAggregationRow(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var row = GetRow(eventsPerStream, isNewData, context);
            if (row == null) {
                return null;
            }

            return ExprTableEvalStrategyUtil.GetRow(row);
        }

        protected ObjectArrayBackedEventBean GetRow(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var groupKey = _factory.GroupKeyEval.Evaluate(eventsPerStream, isNewData, context);
            var tableAndLockGrouped = _provider.Get();
            TableEvalLockUtil.ObtainLockUnless(tableAndLockGrouped.Lock, context);
            if (groupKey is object[] key) {
                groupKey = tableAndLockGrouped.Grouped.Table.PrimaryKeyObjectArrayTransform.From(key);
            }

            return tableAndLockGrouped.Grouped.GetRowForGroupKey(groupKey);
        }
    }
} // end of namespace