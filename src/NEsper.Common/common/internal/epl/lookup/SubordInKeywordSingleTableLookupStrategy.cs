///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.join.exec.inkeyword;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries.
    /// </summary>
    public class SubordInKeywordSingleTableLookupStrategy : SubordTableLookupStrategy
    {
        private readonly EventBean[] _events;
        private readonly SubordInKeywordSingleTableLookupStrategyFactory _factory;
        private readonly PropertyHashedEventTable _index;

        public SubordInKeywordSingleTableLookupStrategy(
            SubordInKeywordSingleTableLookupStrategyFactory factory,
            PropertyHashedEventTable index)
        {
            _factory = factory;
            _index = index;
            _events = new EventBean[factory.streamCountOuter + 1];
        }

        public ICollection<EventBean> Lookup(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            if (context.InstrumentationProvider.Activated()) {
                context.InstrumentationProvider.QIndexSubordLookup(this, _index, null);
                Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
                var resultActivated = InKeywordTableLookupUtil.SingleIndexLookup(
                    _factory.evaluators,
                    _events,
                    context,
                    _index);
                context.InstrumentationProvider.AIndexSubordLookup(resultActivated, null);
                return resultActivated;
            }

            Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
            return InKeywordTableLookupUtil.SingleIndexLookup(
                _factory.evaluators,
                _events,
                context,
                _index);
        }

        public LookupStrategyDesc StrategyDesc => _factory.LookupStrategyDesc;

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace