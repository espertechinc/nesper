///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.virtualdw;

namespace com.espertech.esper.epl.lookup
{
    public class SubordWMatchExprLookupStrategyFactoryIndexedFiltered : SubordWMatchExprLookupStrategyFactory
    {
        private readonly ExprEvaluator _exprEvaluator;
        private readonly SubordTableLookupStrategyFactory _lookupStrategyFactory;
    
        public SubordWMatchExprLookupStrategyFactoryIndexedFiltered(ExprEvaluator exprEvaluator, SubordTableLookupStrategyFactory lookupStrategyFactory)
        {
            this._exprEvaluator = exprEvaluator;
            this._lookupStrategyFactory = lookupStrategyFactory;
        }

        public SubordWMatchExprLookupStrategy Realize(EventTable[] indexes, AgentInstanceContext agentInstanceContext, IEnumerable<EventBean> scanIterable, VirtualDWView virtualDataWindow)
        {
            SubordTableLookupStrategy strategy = _lookupStrategyFactory.MakeStrategy(indexes, virtualDataWindow);
            return new SubordWMatchExprLookupStrategyIndexedFiltered(_exprEvaluator, strategy);
        }
    
        public string ToQueryPlan()
        {
            return this.GetType().Name + " " + " strategy " + _lookupStrategyFactory.ToQueryPlan();
        }

        public SubordTableLookupStrategyFactory OptionalInnerStrategy => _lookupStrategyFactory;
    }
}
