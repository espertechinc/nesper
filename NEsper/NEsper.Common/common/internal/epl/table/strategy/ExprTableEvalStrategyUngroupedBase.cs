///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.table.strategy
{
    public abstract class ExprTableEvalStrategyUngroupedBase : ExprTableEvalStrategy
    {
        protected internal readonly ExprTableEvalStrategyFactory factory;
        protected internal readonly TableAndLockProviderUngrouped provider;

        public ExprTableEvalStrategyUngroupedBase(
            TableAndLockProviderUngrouped provider,
            ExprTableEvalStrategyFactory factory)
        {
            this.provider = provider;
            this.factory = factory;
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
            var pair = provider.Get();
            TableEvalLockUtil.ObtainLockUnless(pair.Lock, context);
            return pair.Ungrouped.EventUngrouped;
        }
    }
} // end of namespace