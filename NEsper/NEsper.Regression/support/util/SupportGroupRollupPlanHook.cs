///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.compat;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportGroupRollupPlanHook : GroupByRollupPlanHook
    {
        private static GroupByRollupPlanDesc plan;

        public void Query(GroupByRollupPlanDesc desc)
        {
            if (plan != null) {
                throw new IllegalStateException();
            }

            plan = desc;
        }

        public static void Reset()
        {
            plan = null;
        }

        public static GroupByRollupPlanDesc GetPlan()
        {
            return plan;
        }
    }
} // end of namespace