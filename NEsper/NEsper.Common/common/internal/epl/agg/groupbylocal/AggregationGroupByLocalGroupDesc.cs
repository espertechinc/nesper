///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.groupbylocal
{
    public class AggregationGroupByLocalGroupDesc
    {
        public AggregationGroupByLocalGroupDesc(int numColumns, AggregationGroupByLocalGroupLevel[] levels)
        {
            NumColumns = numColumns;
            Levels = levels;
        }

        public AggregationGroupByLocalGroupLevel[] Levels { get; private set; }

        public int NumColumns { get; private set; }
    }
} // end of namespace