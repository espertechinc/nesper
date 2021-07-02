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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries against tables, full table scan.
    /// </summary>
    public class SubordIndexedTableLookupTableStrategy : SubordTableLookupStrategy
    {
        private readonly SubordTableLookupStrategy _inner;
        private readonly ILockable _lock;

        public SubordIndexedTableLookupTableStrategy(
            SubordTableLookupStrategy inner,
            ILockable @lock)
        {
            _inner = inner;
            _lock = @lock;
        }

        public ICollection<EventBean> Lookup(
            EventBean[] events,
            ExprEvaluatorContext context)
        {
            if (context.InstrumentationProvider.Activated()) {
                context.InstrumentationProvider.QIndexSubordLookup(this, null, null);
                var result = LookupInternal(events, context);
                context.InstrumentationProvider.AIndexSubordLookup(result, null);
                return result;
            }

            return LookupInternal(events, context);
        }

        public LookupStrategyDesc StrategyDesc => _inner.StrategyDesc;

        public string ToQueryPlan()
        {
            return GetType().Name + " inner " + _inner.ToQueryPlan();
        }

        private ICollection<EventBean> LookupInternal(
            EventBean[] events,
            ExprEvaluatorContext context)
        {
            TableEvalLockUtil.ObtainLockUnless(_lock, context);

            var result = _inner.Lookup(events, context);
            if (result == null) {
                return Collections.GetEmptyList<EventBean>();
            }

            return result;
        }
    }
} // end of namespace