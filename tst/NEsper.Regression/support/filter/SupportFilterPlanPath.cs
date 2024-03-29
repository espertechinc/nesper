///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.filter
{
    public class SupportFilterPlanPath
    {
        public SupportFilterPlanPath(params SupportFilterPlanTriplet[] triplets)
        {
            Triplets = triplets;
        }

        public SupportFilterPlanPath(
            string controlNegate,
            params SupportFilterPlanTriplet[] triplets)
        {
            Triplets = triplets;
            ControlNegate = controlNegate;
        }

        public SupportFilterPlanTriplet[] Triplets { get; set; }

        public string ControlNegate { get; set; }
    }
} // end of namespace