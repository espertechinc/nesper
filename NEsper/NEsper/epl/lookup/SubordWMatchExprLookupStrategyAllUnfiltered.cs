///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.lookup
{
    public class SubordWMatchExprLookupStrategyAllUnfiltered : SubordWMatchExprLookupStrategy
    {
        private readonly IEnumerable<EventBean> _source;

        public SubordWMatchExprLookupStrategyAllUnfiltered(IEnumerable<EventBean> source)
        {
            this._source = source;
        }
    
        public EventBean[] Lookup(EventBean[] newData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QInfraTriggeredLookup(SubordWMatchExprLookupStrategyType.FULLTABLESCAN_UNFILTERED); }

            var result = _source.ToArray();
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AInfraTriggeredLookup(result); }
            return result;
        }
    
        public override string ToString()
        {
            return ToQueryPlan();
        }
    
        public string ToQueryPlan()
        {
            return this.GetType().Name;
        }
    }
}
