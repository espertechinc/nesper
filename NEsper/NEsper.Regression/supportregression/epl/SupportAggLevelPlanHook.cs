///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.collection;
using com.espertech.esper.epl.agg.util;

namespace com.espertech.esper.supportregression.epl
{
	public class SupportAggLevelPlanHook : AggregationLocalLevelHook
    {
	    private static Pair<AggregationGroupByLocalGroupDesc,AggregationLocalGroupByPlan> _desc;

	    public void Planned(AggregationGroupByLocalGroupDesc localGroupDesc, AggregationLocalGroupByPlan localGroupByPlan)
        {
	        _desc = new Pair<AggregationGroupByLocalGroupDesc,AggregationLocalGroupByPlan>(localGroupDesc, localGroupByPlan);
	    }

	    public static Pair<AggregationGroupByLocalGroupDesc,AggregationLocalGroupByPlan> GetAndReset()
        {
	        var tmp = _desc;
	        _desc = null;
	        return tmp;
	    }
	}
} // end of namespace
