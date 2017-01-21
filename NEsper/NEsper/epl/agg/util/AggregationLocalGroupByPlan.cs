///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.agg.util
{
    public class AggregationLocalGroupByPlan
    {
        public AggregationLocalGroupByPlan(
            int numMethods,
            int numAccess,
            AggregationLocalGroupByColumn[] columns,
            AggregationLocalGroupByLevel optionalLevelTop,
            AggregationLocalGroupByLevel[] allLevels)
        {
            NumMethods = numMethods;
            NumAccess = numAccess;
            Columns = columns;
            OptionalLevelTop = optionalLevelTop;
            AllLevels = allLevels;
        }

        public AggregationLocalGroupByColumn[] Columns { get; private set; }

        public AggregationLocalGroupByLevel OptionalLevelTop { get; private set; }

        public AggregationLocalGroupByLevel[] AllLevels { get; private set; }

        public int NumMethods { get; private set; }

        public int NumAccess { get; private set; }
    }
} // end of namespace