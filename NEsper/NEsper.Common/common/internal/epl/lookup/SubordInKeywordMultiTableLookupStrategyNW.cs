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
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.@join.exec.inkeyword;

namespace com.espertech.esper.common.@internal.epl.lookup
{
    /// <summary>
    ///     Index lookup strategy for subqueries for in-keyword single-index sided.
    /// </summary>
    public class SubordInKeywordMultiTableLookupStrategyNW : SubordTableLookupStrategy
    {
        private readonly SubordInKeywordMultiTableLookupStrategyFactory _factory;
        private readonly PropertyHashedEventTable[] _indexes;

        public SubordInKeywordMultiTableLookupStrategyNW(
            SubordInKeywordMultiTableLookupStrategyFactory factory,
            PropertyHashedEventTable[] indexes)
        {
            this._factory = factory;
            this._indexes = indexes;
        }

        public ICollection<EventBean> Lookup(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            if (context.InstrumentationProvider.Activated()) {
                context.InstrumentationProvider.QIndexSubordLookup(this, null, null);
                ISet<EventBean> result = InKeywordTableLookupUtil.MultiIndexLookup(
                    _factory.evaluator,
                    eventsPerStream,
                    context,
                    _indexes);
                context.InstrumentationProvider.AIndexSubordLookup(result, null);
                return result;
            }

            return InKeywordTableLookupUtil.MultiIndexLookup(_factory.evaluator, eventsPerStream, context, _indexes);
        }

        public LookupStrategyDesc StrategyDesc => _factory.LookupStrategyDesc;

        public string ToQueryPlan()
        {
            return GetType().Name;
        }
    }
} // end of namespace