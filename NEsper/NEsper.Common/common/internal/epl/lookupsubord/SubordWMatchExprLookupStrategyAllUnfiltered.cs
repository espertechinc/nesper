///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.lookupsubord
{
    public class SubordWMatchExprLookupStrategyAllUnfiltered : SubordWMatchExprLookupStrategy
    {
        private readonly IEnumerable<EventBean> source;

        public SubordWMatchExprLookupStrategyAllUnfiltered(IEnumerable<EventBean> source)
        {
            this.source = source;
        }

        public EventBean[] Lookup(
            EventBean[] newData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            exprEvaluatorContext.InstrumentationProvider.QInfraTriggeredLookup("fulltablescan_unfiltered");
            var @out = source.ToArray();
            exprEvaluatorContext.InstrumentationProvider.AInfraTriggeredLookup(@out);
            return @out;
        }

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName();
        }

        public override string ToString()
        {
            return ToQueryPlan();
        }
    }
} // end of namespace