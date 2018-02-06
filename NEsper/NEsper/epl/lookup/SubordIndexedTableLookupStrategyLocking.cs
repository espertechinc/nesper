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

namespace com.espertech.esper.epl.lookup
{
    /// <summary>
    /// MapIndex lookup strategy for subqueries.
    /// </summary>
    public class SubordIndexedTableLookupStrategyLocking : SubordTableLookupStrategy
    {
        private readonly SubordTableLookupStrategy _inner;
        private readonly IReaderWriterLock _statementLock;

        public SubordIndexedTableLookupStrategyLocking(SubordTableLookupStrategy inner, IReaderWriterLock statementLock)
        {
            _inner = inner;
            _statementLock = statementLock;
        }
    
        public ICollection<EventBean> Lookup(EventBean[] events, ExprEvaluatorContext context)
        {
            using(_statementLock.AcquireReadLock())
            {
                ICollection<EventBean> result = _inner.Lookup(events, context);
                if (result != null) {
                    return new ArrayDeque<EventBean>(result);
                }
                else {
                    return Collections.GetEmptySet<EventBean>();
                }
            }
        }

        public LookupStrategyDesc StrategyDesc => _inner.StrategyDesc;

        public String ToQueryPlan()
        {
            return GetType().Name + " inner " + _inner.ToQueryPlan();
        }
    }
}
