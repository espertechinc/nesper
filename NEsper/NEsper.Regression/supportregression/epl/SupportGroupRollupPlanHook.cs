///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.rollup;

namespace com.espertech.esper.supportregression.epl
{
    public class SupportGroupRollupPlanHook : GroupByRollupPlanHook
    {
        public static void Reset() {
            Plan = null;
        }
    
        public void Query(GroupByRollupPlanDesc desc) {
            if (Plan != null) {
                throw new IllegalStateException();
            }
            Plan = desc;
        }

        public static GroupByRollupPlanDesc Plan { get; private set; }
    }
}
