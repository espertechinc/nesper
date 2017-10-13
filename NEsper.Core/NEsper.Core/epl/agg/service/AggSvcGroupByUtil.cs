///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;

namespace com.espertech.esper.epl.agg.service
{
    public class AggSvcGroupByUtil {
        public static AggregationMethod[] NewAggregators(AggregationMethodFactory[] prototypes) {
            var row = new AggregationMethod[prototypes.Length];
            for (int i = 0; i < prototypes.Length; i++) {
                row[i] = prototypes[i].Make();
            }
            return row;
        }
    
        public static AggregationState[] NewAccesses(int agentInstanceId, bool isJoin, AggregationStateFactory[] accessAggSpecs, Object groupKey, AggregationServicePassThru passThru) {
            var row = new AggregationState[accessAggSpecs.Length];
            int i = 0;
            foreach (AggregationStateFactory spec in accessAggSpecs) {
                row[i] = spec.CreateAccess(agentInstanceId, isJoin, groupKey, passThru);   // no group id assigned
                i++;
            }
            return row;
        }
    }
} // end of namespace
