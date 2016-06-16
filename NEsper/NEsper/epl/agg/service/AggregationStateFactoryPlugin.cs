///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.accessagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.plugin;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationStateFactoryPlugin : AggregationStateFactory
    {
        private readonly ExprPlugInAggMultiFunctionNodeFactory _parent;
        private readonly PlugInAggregationMultiFunctionStateFactory _stateFactory;
    
        public AggregationStateFactoryPlugin(ExprPlugInAggMultiFunctionNodeFactory parent)
        {
            _parent = parent;
            _stateFactory = parent.HandlerPlugin.StateFactory;
        }

        public AggregationState CreateAccess(MethodResolutionService methodResolutionService, int agentInstanceId, int groupId, int aggregationId, bool join, Object groupBy, AggregationServicePassThru passThru)
        {
            return methodResolutionService.MakeAccessAggPlugin(agentInstanceId, groupId, aggregationId, join, _stateFactory, groupBy);
        }

        public ExprNode AggregationExpression
        {
            get { return _parent.AggregationExpression; }
        }
    }
}
