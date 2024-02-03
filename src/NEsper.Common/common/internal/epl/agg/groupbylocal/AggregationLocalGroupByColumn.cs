///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.agg.groupbylocal
{
    public class AggregationLocalGroupByColumn
    {
        public AggregationLocalGroupByColumn(
            bool defaultGroupLevel,
            int fieldNum,
            int levelNum)
        {
            IsDefaultGroupLevel = defaultGroupLevel;
            FieldNum = fieldNum;
            LevelNum = levelNum;
        }

        public bool IsDefaultGroupLevel { get; }

        public int FieldNum { get; }

        public int LevelNum { get; }
    }
} // end of namespace