///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.common.client.serde;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationGroupByRollupDesc
    {
        public AggregationGroupByRollupDesc(AggregationGroupByRollupLevel[] levels)
        {
            Levels = levels;
            NumLevelsAggregation = levels.Count(level => !level.IsAggregationTop);
        }

        public AggregationGroupByRollupLevel[] Levels { get; }

        public int NumLevelsAggregation { get; }

        public int NumLevels => Levels.Length;

        public DataInputOutputSerde[] KeySerdes => Levels.Select(v => v.SubkeySerde).ToArray();
    }
}