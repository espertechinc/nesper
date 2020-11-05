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
    public abstract class ExprTableEvalStrategyUngroupedBase : ExprTableEvalStrategy
    {
        private readonly ExprTableEvalStrategyFactory _factory;
        private readonly TableAndLockProviderUngrouped _provider;

        public ExprTableEvalStrategyUngroupedBase(
            TableAndLockProviderUngrouped provider,
            ExprTableEvalStrategyFactory factory)
        {
            this._provider = provider;
            this._factory = factory;
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

        protected ObjectArrayBackedEventBean LockTableReadAndGet(ExprEvaluatorContext context)
        {
            var pair = _provider.Get();
            TableEvalLockUtil.ObtainLockUnless(pair.Lock, context);
            return pair.Ungrouped.EventUngrouped;
        }

        public AggregationRow GetAggregationRow(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            ObjectArrayBackedEventBean row = LockTableReadAndGet(context);
            if (row == null) {
                return null;
            }

            return ExprTableEvalStrategyUtil.GetRow(row);
        }

        public ExprTableEvalStrategyFactory Factory => _factory;

        public TableAndLockProviderUngrouped Provider => _provider;
    }
} // end of namespace