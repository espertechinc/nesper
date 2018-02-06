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
    public class SubordWMatchExprLookupStrategyFactoryAllFiltered : SubordWMatchExprLookupStrategyFactory
    {
        private readonly ExprEvaluator _exprEvaluator;
    
        public SubordWMatchExprLookupStrategyFactoryAllFiltered(ExprEvaluator exprEvaluator)
        {
            this._exprEvaluator = exprEvaluator;
        }

        public SubordWMatchExprLookupStrategy Realize(EventTable[] indexes, AgentInstanceContext agentInstanceContext, IEnumerable<EventBean> scanIterable, VirtualDWView virtualDataWindow)
        {
            return new SubordWMatchExprLookupStrategyAllFiltered(_exprEvaluator, scanIterable);
        }
    
        public string ToQueryPlan()
        {
            return this.GetType().Name;
        }

        public SubordTableLookupStrategyFactory OptionalInnerStrategy => null;
    }
}
