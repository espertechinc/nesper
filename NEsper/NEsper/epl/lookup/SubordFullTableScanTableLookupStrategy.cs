///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.strategy;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// Index lookup strategy for tables.
    /// </summary>
    public class SubordFullTableScanTableLookupStrategy : SubordTableLookupStrategy
    {
        private readonly ILockable _tableLevelLock;
        private readonly IEnumerable<EventBean> _contents;

        public SubordFullTableScanTableLookupStrategy(ILockable tableLevelLock, IEnumerable<EventBean> contents)
        {
            _tableLevelLock = tableLevelLock;
            _contents = contents;
        }
    
        public ICollection<EventBean> Lookup(EventBean[] events, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QIndexSubordLookup(this, null, null);
                ICollection<EventBean> result = LookupInternal(context);
                InstrumentationHelper.Get().AIndexSubordLookup(result, null);
                return result;
            }
            return LookupInternal(context);
        }
    
        private ICollection<EventBean> LookupInternal(ExprEvaluatorContext context)
        {
            ExprTableEvalLockUtil.ObtainLockUnless(_tableLevelLock, context);

            var it = _contents.GetEnumerator();
            if (!it.MoveNext()) {
                return null;
            }

            var result = new ArrayDeque<EventBean>(2);
            do
            {
                result.Add(it.Current);
            } while (it.MoveNext());

            return result;
        }

        public LookupStrategyDesc StrategyDesc => new LookupStrategyDesc(LookupStrategyType.FULLTABLESCAN, null);

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
}
