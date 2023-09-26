///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecPlanPathTriplet
    {
        private FilterSpecParam param;
        private ExprEvaluator tripletConfirm;

        public FilterSpecPlanPathTriplet()
        {
        }

        public FilterSpecPlanPathTriplet(FilterSpecParam param)
        {
            this.param = param;
        }

        public FilterSpecParam Param {
            get => param;
            set => param = value;
        }

        public ExprEvaluator TripletConfirm {
            get => tripletConfirm;
            set => tripletConfirm = value;
        }
    }
} // end of namespace