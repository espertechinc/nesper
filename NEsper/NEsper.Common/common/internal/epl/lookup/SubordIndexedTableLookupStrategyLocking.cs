///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordIndexedTableLookupStrategyLocking : SubordTableLookupStrategy
    {
        private readonly SubordTableLookupStrategy _inner;
        private readonly StatementAgentInstanceLock _statementLock;

        public SubordIndexedTableLookupStrategyLocking(
            SubordTableLookupStrategy inner, StatementAgentInstanceLock statementLock)
        {
            this._inner = inner;
            this._statementLock = statementLock;
        }

        public ICollection<EventBean> Lookup(EventBean[] events, ExprEvaluatorContext context)
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

        private ICollection<EventBean> LookupInternal(EventBean[] events, ExprEvaluatorContext context)
        {
            _statementLock.AcquireReadLock();
            try {
                var result = _inner.Lookup(events, context);
                if (result != null) {
                    return new ArrayDeque<EventBean>(result);
                }
                else {
                    return Collections.GetEmptyList<object>();
                }
            }
            finally {
                _statementLock.ReleaseReadLock();
            }
        }
    }
} // end of namespace