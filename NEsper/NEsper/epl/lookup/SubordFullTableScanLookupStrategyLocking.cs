///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// MapIndex lookup strategy for subqueries.
    /// </summary>
    public class SubordFullTableScanLookupStrategyLocking : SubordTableLookupStrategy
    {
        private readonly IEnumerable<EventBean> _contents;
        private readonly IReaderWriterLock _statementLock;

        public SubordFullTableScanLookupStrategyLocking(IEnumerable<EventBean> contents, IReaderWriterLock statementLock)
        {
            _contents = contents;
            _statementLock = statementLock;
        }
    
        public ICollection<EventBean> Lookup(EventBean[] events, ExprEvaluatorContext context)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QIndexSubordLookup(this, null, null);
                ICollection<EventBean> result = LookupInternal();
                InstrumentationHelper.Get().AIndexSubordLookup(result, null);
                return result;
            }
            return LookupInternal();
        }
    
        private ICollection<EventBean> LookupInternal()
        {
            using(_statementLock.AcquireReadLock())
            {
                var result = new ArrayDeque<EventBean>();
                foreach (EventBean eventBean in _contents)
                {
                    result.Add(eventBean);
                }
                return result;
            }
        }

        public LookupStrategyDesc StrategyDesc => new LookupStrategyDesc(LookupStrategyType.FULLTABLESCAN, null);

        public String ToQueryPlan()
        {
            return GetType().Name;
        }
    }
}
