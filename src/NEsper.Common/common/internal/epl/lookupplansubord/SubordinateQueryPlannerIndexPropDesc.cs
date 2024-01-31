///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.lookupplan;

namespace com.espertech.esper.common.@internal.epl.lookupplansubord
{
    public class SubordinateQueryPlannerIndexPropDesc
    {
        public SubordinateQueryPlannerIndexPropDesc(
            string[] hashIndexPropsProvided,
            Type[] hashIndexCoercionType,
            string[] rangeIndexPropsProvided,
            Type[] rangeIndexCoercionType,
            SubordinateQueryPlannerIndexPropListPair listPair,
            SubordPropHashKeyForge[] hashJoinedProps,
            SubordPropRangeKeyForge[] rangeJoinedProps)
        {
            HashIndexPropsProvided = hashIndexPropsProvided;
            HashIndexCoercionType = hashIndexCoercionType;
            RangeIndexPropsProvided = rangeIndexPropsProvided;
            RangeIndexCoercionType = rangeIndexCoercionType;
            ListPair = listPair;
            HashJoinedProps = hashJoinedProps;
            RangeJoinedProps = rangeJoinedProps;
        }

        public string[] HashIndexPropsProvided { get; }

        public Type[] HashIndexCoercionType { get; }

        public string[] RangeIndexPropsProvided { get; }

        public Type[] RangeIndexCoercionType { get; }

        public SubordinateQueryPlannerIndexPropListPair ListPair { get; }

        public SubordPropHashKeyForge[] HashJoinedProps { get; }

        public SubordPropRangeKeyForge[] RangeJoinedProps { get; }
    }
} // end of namespace