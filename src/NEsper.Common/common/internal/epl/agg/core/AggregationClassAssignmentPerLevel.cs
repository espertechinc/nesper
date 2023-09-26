///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationClassAssignmentPerLevel
    {
        private readonly AggregationClassAssignment[] optionalTop;
        private readonly AggregationClassAssignment[][] optionalPerLevel;

        public AggregationClassAssignmentPerLevel(
            AggregationClassAssignment[] optionalTop,
            AggregationClassAssignment[][] optionalPerLevel)
        {
            this.optionalTop = optionalTop;
            this.optionalPerLevel = optionalPerLevel;
        }

        public AggregationClassAssignment[] OptionalTop => optionalTop;

        public AggregationClassAssignment[][] OptionalPerLevel => optionalPerLevel;
    }
} // end of namespace