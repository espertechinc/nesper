///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.strategy
{
    public class ExprTableEvalStrategyUngroupedAccess 
        : ExprTableEvalStrategyUngroupedBase
        , ExprTableAccessEvalStrategy
    {
    
        private readonly int slot;
        private readonly AggregationAccessor accessor;
    
        public ExprTableEvalStrategyUngroupedAccess(ILockable @lock, Atomic<ObjectArrayBackedEventBean> aggregationState, int slot, AggregationAccessor accessor)
            : base(@lock, aggregationState)
        {
            this.slot = slot;
            this.accessor = accessor;
        }
    
        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            ObjectArrayBackedEventBean @event = LockTableReadAndGet(context);
            if (@event == null)
            {
                return null;
            }
            AggregationState aggregationState = GetAndLock(@event, context);
            return accessor.GetValue(aggregationState, eventsPerStream, isNewData, context);
        }
    
        public object[] EvaluateTypableSingle(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            throw new IllegalStateException("Not typable");
        }
    
        public ICollection<EventBean> EvaluateGetROCollectionEvents(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            ObjectArrayBackedEventBean @event = LockTableReadAndGet(context);
            if (@event == null)
            {
                return null;
            }
            AggregationState aggregationState = GetAndLock(@event, context);
            return accessor.GetEnumerableEvents(aggregationState, eventsPerStream, isNewData, context);
        }
    
        public EventBean EvaluateGetEventBean(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            ObjectArrayBackedEventBean @event = LockTableReadAndGet(context);
            if (@event == null)
            {
                return null;
            }
            AggregationState aggregationState = GetAndLock(@event, context);
            return accessor.GetEnumerableEvent(aggregationState, eventsPerStream, isNewData, context);
        }
    
        public ICollection<object> EvaluateGetROCollectionScalar(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            ObjectArrayBackedEventBean @event = LockTableReadAndGet(context);
            if (@event == null)
            {
                return null;
            }
            AggregationState aggregationState = GetAndLock(@event, context);
            return accessor.GetEnumerableScalar(aggregationState, eventsPerStream, isNewData, context);
        }
    
        private AggregationState GetAndLock(ObjectArrayBackedEventBean @event, ExprEvaluatorContext exprEvaluatorContext)
        {
            AggregationRowPair row = ExprTableEvalStrategyUtil.GetRow(@event);
            return row.States[slot];
        }
    }
}
