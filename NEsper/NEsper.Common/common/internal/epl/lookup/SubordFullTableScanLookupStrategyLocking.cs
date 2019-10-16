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
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordFullTableScanLookupStrategyLocking : SubordTableLookupStrategy
    {
        private readonly IEnumerable<EventBean> _contents;
        private readonly IReaderWriterLock _statementLock;

        public SubordFullTableScanLookupStrategyLocking(
            IEnumerable<EventBean> contents,
            IReaderWriterLock statementLock)
        {
            this._contents = contents;
            this._statementLock = statementLock;
        }

        public ICollection<EventBean> Lookup(
            EventBean[] events,
            ExprEvaluatorContext context)
        {
            if (context.InstrumentationProvider.Activated()) {
                context.InstrumentationProvider.QIndexSubordLookup(this, null, null);
                var result = LookupInternal();
                context.InstrumentationProvider.AIndexSubordLookup(result, null);
                return result;
            }

            return LookupInternal();
        }

        public LookupStrategyDesc StrategyDesc => LookupStrategyDesc.SCAN;

        public string ToQueryPlan()
        {
            return GetType().Name;
        }

        private ICollection<EventBean> LookupInternal()
        {
            using (_statementLock.AcquireReadLock()) {
                var result = new ArrayDeque<EventBean>();
                foreach (var eventBean in _contents) {
                    result.Add(eventBean);
                }

                return result;
            }
        }
    }
} // end of namespace