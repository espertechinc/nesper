///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Aggregation service for use when only first/last/window aggregation functions are used an none other.
    /// </summary>
    public class AggSvcGroupByAccessOnlyFactory : AggregationServiceFactory
    {
        private readonly AggregationAccessorSlotPair[] _accessors;
        private readonly AggregationStateFactory[] _accessAggSpecs;
        private readonly Object _groupKeyBinding;
        private readonly bool _isJoin;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="accessors">accessor definitions</param>
        /// <param name="accessAggSpecs">access aggregations</param>
        /// <param name="groupKeyBinding">The group key binding.</param>
        /// <param name="isJoin">true for join, false for single-stream</param>
        public AggSvcGroupByAccessOnlyFactory(
            AggregationAccessorSlotPair[] accessors,
            AggregationStateFactory[] accessAggSpecs,
            Object groupKeyBinding,
            bool isJoin)
        {
            _accessors = accessors;
            _accessAggSpecs = accessAggSpecs;
            _groupKeyBinding = groupKeyBinding;
            _isJoin = isJoin;
        }
    
        public AggregationService MakeService(AgentInstanceContext agentInstanceContext, MethodResolutionService methodResolutionService)
        {
            return new AggSvcGroupByAccessOnlyImpl(methodResolutionService, _groupKeyBinding, _accessors, _accessAggSpecs, _isJoin);
        }
    }
}
