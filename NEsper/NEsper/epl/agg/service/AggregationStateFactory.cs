///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
    public interface AggregationStateFactory
    {
        AggregationState CreateAccess(int agentInstanceId, bool @join, object groupKey, AggregationServicePassThru passThru);
        ExprNode AggregationExpression { get; }
    }

    public class ProxyAggregationStateFactory : AggregationStateFactory
    {
        public Func<int, bool, object, AggregationServicePassThru, AggregationState> ProcCreateAccess { get; set; }
        public Func<ExprNode> ProcAggregationExpression { get; set; } 

        public AggregationState CreateAccess(int agentInstanceId, bool @join, object groupKey, AggregationServicePassThru passThru)
        {
            return ProcCreateAccess(agentInstanceId, join, groupKey, passThru);
        }

        public ExprNode AggregationExpression
        {
            get { return ProcAggregationExpression(); }
        }
    }
}
