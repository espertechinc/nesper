///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.agg.core;

namespace com.espertech.esper.common.@internal.epl.agg.rollup
{
    public class GroupByRollupKey
    {
        public GroupByRollupKey(
            EventBean[] generator,
            AggregationGroupByRollupLevel level,
            object groupKey)
        {
            Generator = generator;
            Level = level;
            GroupKey = groupKey;
        }

        public EventBean[] Generator { get; }

        public AggregationGroupByRollupLevel Level { get; }

        public object GroupKey { get; }

        public override string ToString()
        {
            return "GroupRollupKey{" +
                   "level=" +
                   Level +
                   ", groupKey=" +
                   GroupKey +
                   '}';
        }
    }
} // end of namespace