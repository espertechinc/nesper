///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.strategy
{
    public abstract class ExprTableEvalStrategyUngroupedBase
    {
        private readonly ILockable _lock;
        protected readonly Atomic<ObjectArrayBackedEventBean> AggregationState;

        protected ExprTableEvalStrategyUngroupedBase(
            ILockable @lock,
            Atomic<ObjectArrayBackedEventBean> aggregationState)
        {
            _lock = @lock;
            AggregationState = aggregationState;
        }

        protected ObjectArrayBackedEventBean LockTableReadAndGet(ExprEvaluatorContext context)
        {
            ExprTableEvalLockUtil.ObtainLockUnless(_lock, context);
            return AggregationState.Get();
        }
    }
}