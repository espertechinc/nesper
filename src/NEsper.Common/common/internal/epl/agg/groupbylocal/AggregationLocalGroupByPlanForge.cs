///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.groupbylocal
{
    public class AggregationLocalGroupByPlanForge
    {
        public AggregationLocalGroupByPlanForge(
            int numMethods,
            int numAccess,
            AggregationLocalGroupByColumnForge[] columns,
            AggregationLocalGroupByLevelForge optionalLevelTop,
            AggregationLocalGroupByLevelForge[] allLevels)
        {
            NumMethods = numMethods;
            NumAccess = numAccess;
            ColumnsForges = columns;
            OptionalLevelTopForge = optionalLevelTop;
            AllLevelsForges = allLevels;
        }

        public AggregationLocalGroupByColumnForge[] ColumnsForges { get; }

        public AggregationLocalGroupByLevelForge OptionalLevelTopForge { get; }

        public AggregationLocalGroupByLevelForge[] AllLevelsForges { get; }

        public int NumMethods { get; }

        public int NumAccess { get; }
    }
} // end of namespace