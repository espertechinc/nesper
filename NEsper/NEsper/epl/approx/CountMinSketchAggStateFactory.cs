///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.approx
{
    public class CountMinSketchAggStateFactory : AggregationStateFactory
    {
        public CountMinSketchAggStateFactory(ExprAggCountMinSketchNode parent, CountMinSketchSpec specification)
        {
            Parent = parent;
            Specification = specification;
        }

        public AggregationState CreateAccess(MethodResolutionService methodResolutionService, int agentInstanceId, int groupId, int aggregationId, bool join, object groupKey, AggregationServicePassThru passThru)
        {
            return methodResolutionService.MakeCountMinSketch(agentInstanceId, groupId, aggregationId, Specification);
        }

        public ExprNode AggregationExpression
        {
            get { return Parent; }
        }

        public CountMinSketchSpec Specification { get; private set; }

        public ExprAggCountMinSketchNode Parent { get; private set; }
    }
}
