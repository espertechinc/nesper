///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.accessagg
{
    public class ExprAggAggregationAgentFactory
    {
        public static AggregationAgent Make(int streamNum, ExprNode optionalFilter)
        {
            if (streamNum == 0)
                if (optionalFilter == null)
                    return AggregationAgentDefault.INSTANCE;
                else
                    return new AggregationAgentDefaultWFilter(optionalFilter.ExprEvaluator);

            if (optionalFilter == null)
                return new AggregationAgentRewriteStream(streamNum);
            return new AggregationAgentRewriteStreamWFilter(streamNum, optionalFilter.ExprEvaluator);
        }
    }
} // end of namespace