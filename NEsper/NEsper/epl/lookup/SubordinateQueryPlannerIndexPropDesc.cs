///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.lookup
{
    public class SubordinateQueryPlannerIndexPropDesc
    {
        public SubordinateQueryPlannerIndexPropDesc(
            string[] hashIndexPropsProvided,
            Type[] hashIndexCoercionType,
            string[] rangeIndexPropsProvided,
            Type[] rangeIndexCoercionType,
            SubordinateQueryPlannerIndexPropListPair listPair,
            SubordPropHashKey[] hashJoinedProps,
            SubordPropRangeKey[] rangeJoinedProps)
        {
            HashIndexPropsProvided = hashIndexPropsProvided;
            HashIndexCoercionType = hashIndexCoercionType;
            RangeIndexPropsProvided = rangeIndexPropsProvided;
            RangeIndexCoercionType = rangeIndexCoercionType;
            ListPair = listPair;
            HashJoinedProps = hashJoinedProps;
            RangeJoinedProps = rangeJoinedProps;
        }

        public string[] HashIndexPropsProvided { get; private set; }

        public Type[] HashIndexCoercionType { get; private set; }

        public string[] RangeIndexPropsProvided { get; private set; }

        public Type[] RangeIndexCoercionType { get; private set; }

        public SubordinateQueryPlannerIndexPropListPair ListPair { get; private set; }

        public SubordPropHashKey[] HashJoinedProps { get; private set; }

        public SubordPropRangeKey[] RangeJoinedProps { get; private set; }
    }
}