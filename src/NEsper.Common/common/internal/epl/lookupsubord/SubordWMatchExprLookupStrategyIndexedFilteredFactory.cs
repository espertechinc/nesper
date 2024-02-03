///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.virtualdw;

namespace com.espertech.esper.common.@internal.epl.lookupsubord
{
    public class SubordWMatchExprLookupStrategyIndexedFilteredFactory : SubordWMatchExprLookupStrategyFactory
    {
        private readonly ExprEvaluator exprEvaluator;

        public SubordWMatchExprLookupStrategyIndexedFilteredFactory(
            ExprEvaluator exprEvaluator,
            SubordTableLookupStrategyFactory lookupStrategyFactory)
        {
            this.exprEvaluator = exprEvaluator;
            OptionalInnerStrategy = lookupStrategyFactory;
        }

        public SubordTableLookupStrategyFactory OptionalInnerStrategy { get; }

        public SubordWMatchExprLookupStrategy Realize(
            EventTable[] indexes,
            AgentInstanceContext agentInstanceContext,
            IEnumerable<EventBean> scanIterable,
            VirtualDWView virtualDataWindow)
        {
            var strategy = OptionalInnerStrategy.MakeStrategy(indexes, agentInstanceContext, virtualDataWindow);
            return new SubordWMatchExprLookupStrategyIndexedFiltered(exprEvaluator, strategy);
        }

        public string ToQueryPlan()
        {
            return GetType().Name + " " + " strategy " + OptionalInnerStrategy.GetType().Name;
        }
    }
} // end of namespace