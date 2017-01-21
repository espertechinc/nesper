///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.epl.agg.service
{
    public class AggregationGroupByRollupDesc
    {
        public AggregationGroupByRollupDesc(AggregationGroupByRollupLevel[] levels)
        {
            Levels = levels;
            NumLevelsAggregation = levels.Count(level => !level.IsAggregationTop);
        }

        public AggregationGroupByRollupLevel[] Levels { get; private set; }

        public int NumLevelsAggregation { get; private set; }

        public int NumLevels
        {
            get { return Levels.Length; }
        }

        public static AggregationGroupByRollupDesc Make(int[][] indexes)
        {
            var levels = new List<AggregationGroupByRollupLevel>();
            var countOffset = 0;
            var countNumber = -1;
            foreach (var mki in indexes)
            {
                countNumber++;
                if (mki.Length == 0)
                {
                    levels.Add(new AggregationGroupByRollupLevel(countNumber, -1, null));
                }
                else
                {
                    levels.Add(new AggregationGroupByRollupLevel(countNumber, countOffset, mki));
                    countOffset++;
                }
            }
            AggregationGroupByRollupLevel[] levelsarr = levels.ToArray();
            return new AggregationGroupByRollupDesc(levelsarr);
        }
    }
}