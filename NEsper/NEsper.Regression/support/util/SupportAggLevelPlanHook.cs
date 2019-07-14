///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.collection;
using com.espertech.esper.common.@internal.epl.agg.groupbylocal;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportAggLevelPlanHook : AggregationLocalLevelHook
    {
        private static Pair<AggregationGroupByLocalGroupDesc, AggregationLocalGroupByPlanForge> desc;

        public void Planned(
            AggregationGroupByLocalGroupDesc localGroupDesc,
            AggregationLocalGroupByPlanForge localGroupByPlan)
        {
            desc = new Pair<AggregationGroupByLocalGroupDesc, AggregationLocalGroupByPlanForge>(
                localGroupDesc,
                localGroupByPlan);
        }

        public static Pair<AggregationGroupByLocalGroupDesc, AggregationLocalGroupByPlanForge> GetAndReset()
        {
            var tmp = desc;
            desc = null;
            return tmp;
        }
    }
} // end of namespace