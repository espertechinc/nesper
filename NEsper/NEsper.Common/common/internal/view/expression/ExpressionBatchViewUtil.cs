///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.expression
{
    public class ExpressionBatchViewUtil
    {
        public static bool Evaluate(
            EventBean[] eventsPerStream,
            AgentInstanceContext agentInstanceContext,
            ExpressionViewFactoryBase factory,
            AggregationService aggregationService)
        {
            // Evaluation with aggregation requires a lock on the factory as the aggregation-field is assigned per-factory
            if (aggregationService != null) {
                lock (factory) {
                    factory.AggregationResultFutureAssignable.Assign(aggregationService);
                    var resultX = factory.ExpiryEval.Evaluate(eventsPerStream, true, agentInstanceContext);
                    if (resultX == null) {
                        return false;
                    }

                    return true.Equals(resultX);
                }
            }

            var result = factory.ExpiryEval.Evaluate(eventsPerStream, true, agentInstanceContext);
            if (result == null) {
                return false;
            }

            return true.Equals(result);
        }
    }
} // end of namespace