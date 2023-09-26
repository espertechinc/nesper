///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecPlanComputeFactory
    {
        public static FilterSpecPlanCompute Make(FilterSpecPlan plan)
        {
            var hasTopControl = plan.FilterConfirm != null || plan.FilterNegate != null;
            var hasPathControl = false;
            var hasTripletControl = false;

            foreach (var path in plan.Paths) {
                hasPathControl |= path.PathNegate != null;
                hasTripletControl |= path.HasTripletControl;
            }

            if (hasTripletControl) {
                return FilterSpecPlanComputeConditionalTriplets.INSTANCE;
            }

            if (hasPathControl) {
                return FilterSpecPlanComputeConditionalPath.INSTANCE;
            }

            if (hasTopControl) {
                return FilterSpecPlanComputeConditionalTopOnly.INSTANCE;
            }

            return FilterSpecPlanComputeUnconditional.INSTANCE;
        }
    }
} // end of namespace