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
    ///     Index lookup strategy for tables.
    /// </summary>
    public class SubordFullTableScanTableLookupStrategy : SubordTableLookupStrategy
    {
        private readonly IEnumerable<EventBean> _contents;
        private readonly ILockable _tableLevelLock;

        public SubordFullTableScanTableLookupStrategy(
            ILockable tableLevelLock,
            IEnumerable<EventBean> contents)
        {
            this._tableLevelLock = tableLevelLock;
            this._contents = contents;
        }

        public ICollection<EventBean> Lookup(
            EventBean[] events,
            ExprEvaluatorContext context)
        {
            if (context.InstrumentationProvider.Activated()) {
                context.InstrumentationProvider.QIndexSubordLookup(this, null, null);
                var result = LookupInternal(context);
                context.InstrumentationProvider.AIndexSubordLookup(result, null);
                return result;
            }

            return LookupInternal(context);
        }

        public LookupStrategyDesc StrategyDesc => LookupStrategyDesc.SCAN;

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        private ICollection<EventBean> LookupInternal(ExprEvaluatorContext context)
        {
            TableEvalLockUtil.ObtainLockUnless(_tableLevelLock, context);

            IEnumerator<EventBean> it = _contents.GetEnumerator();
            if (!it.MoveNext()) {
                return null;
            }

            var result = new ArrayDeque<EventBean>(2);
            for (; it.MoveNext();) {
                EventBean eventBean = it.Current;
                result.Add(eventBean);
            }

            return result;
        }
    }
} // end of namespace