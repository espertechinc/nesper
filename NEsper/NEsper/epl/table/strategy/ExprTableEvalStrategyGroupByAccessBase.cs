///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;

namespace com.espertech.esper.epl.table.strategy
{
    public abstract class ExprTableEvalStrategyGroupByAccessBase 
        : ExprTableEvalStrategyGroupByBase 
        , ExprTableAccessEvalStrategy
    {
        private readonly AggregationAccessorSlotPair _pair;

        public abstract object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext);
        public abstract ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
        public abstract EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);
        public abstract ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context);

        protected ExprTableEvalStrategyGroupByAccessBase(TableAndLockProviderGrouped provider, AggregationAccessorSlotPair pair)
            : base(provider)
        {
            _pair = pair;
        }
    
        protected object EvaluateInternal(object group, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var row = LockTableReadAndGet(group, context);
            if (row == null)
            {
                return null;
            }
            return ExprTableEvalStrategyUtil.EvalAccessorGetValue(ExprTableEvalStrategyUtil.GetRow(row), _pair, eventsPerStream, isNewData, context);
        }
    
        public object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            throw new IllegalStateException("Not typable");
        }
    
        protected ICollection<EventBean> EvaluateGetROCollectionEventsInternal(object group, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var row = LockTableReadAndGet(group, context);
            if (row == null)
            {
                return null;
            }
            return ExprTableEvalStrategyUtil.EvalGetROCollectionEvents(ExprTableEvalStrategyUtil.GetRow(row), _pair, eventsPerStream, isNewData, context);
        }
    
        protected EventBean EvaluateGetEventBeanInternal(object group, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var row = LockTableReadAndGet(group, context);
            if (row == null)
            {
                return null;
            }
            return ExprTableEvalStrategyUtil.EvalGetEventBean(ExprTableEvalStrategyUtil.GetRow(row), _pair, eventsPerStream, isNewData, context);
        }
    
        protected ICollection<object> EvaluateGetROCollectionScalarInternal(object group, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var row = LockTableReadAndGet(group, context);
            if (row == null)
            {
                return null;
            }
            return ExprTableEvalStrategyUtil.EvalGetROCollectionScalar(ExprTableEvalStrategyUtil.GetRow(row), _pair, eventsPerStream, isNewData, context);
        }
    }
}
