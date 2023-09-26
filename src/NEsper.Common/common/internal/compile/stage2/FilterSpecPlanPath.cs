///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    public class FilterSpecPlanPath
    {
        public static readonly FilterSpecPlanPath[] EMPTY_ARRAY = Array.Empty<FilterSpecPlanPath>();

        private FilterSpecPlanPathTriplet[] triplets;
        private ExprEvaluator pathNegate;

        public FilterSpecPlanPathTriplet[] Triplets {
            get => triplets;
            set => triplets = value;
        }

        public ExprEvaluator PathNegate {
            get => pathNegate;
            set => pathNegate = value;
        }

        public FilterSpecPlanPath()
        {
        }

        public FilterSpecPlanPath(FilterSpecPlanPathTriplet[] triplets)
        {
            this.triplets = triplets;
        }

        public bool HasTripletControl {
            get {
                foreach (var triplet in triplets) {
                    if (triplet.TripletConfirm != null) {
                        return true;
                    }
                }

                return false;
            }
        }
    }
} // end of namespace