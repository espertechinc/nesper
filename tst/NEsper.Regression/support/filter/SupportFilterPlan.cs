///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.filter
{
    public class SupportFilterPlan
    {
        public SupportFilterPlan(
            string controlConfirm,
            string controlNegate,
            params SupportFilterPlanPath[] paths)
        {
            Paths = paths;
            ControlConfirm = controlConfirm;
            ControlNegate = controlNegate;
        }

        public SupportFilterPlan(params SupportFilterPlanPath[] paths) : this(null, null, paths)
        {
        }

        public SupportFilterPlanPath[] Paths { get; set; }

        public string ControlConfirm { get; set; }

        public string ControlNegate { get; set; }
    }
} // end of namespace