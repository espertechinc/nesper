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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.table.strategy
{
    public abstract class ExprTableEvalStrategyGroupByBase
    {
        private readonly ILockable _lock;
        protected readonly IDictionary<Object, ObjectArrayBackedEventBean> AggregationState;

        protected ExprTableEvalStrategyGroupByBase(
            ILockable @lock,
            IDictionary<Object, ObjectArrayBackedEventBean> aggregationState)
        {
            _lock = @lock;
            AggregationState = aggregationState;
        }

        protected ObjectArrayBackedEventBean LockTableReadAndGet(object group, ExprEvaluatorContext context)
        {
            ExprTableEvalLockUtil.ObtainLockUnless(_lock, context);
            return AggregationState.Get(group);
        }

        protected void LockTableRead(ExprEvaluatorContext context)
        {
            ExprTableEvalLockUtil.ObtainLockUnless(_lock, context);
        }
    }
}