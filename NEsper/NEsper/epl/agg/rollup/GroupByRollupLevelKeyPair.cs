///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.epl.agg.rollup
{
    public class GroupByRollupLevelKeyPair
    {
        public GroupByRollupLevelKeyPair(AggregationGroupByRollupLevel level, Object key)
        {
            Level = level;
            Key = key;
        }

        public AggregationGroupByRollupLevel Level { get; private set; }

        public object Key { get; private set; }

        public override bool Equals(Object o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;

            var that = (GroupByRollupLevelKeyPair) o;

            if (Key != null ? !Key.Equals(that.Key) : that.Key != null)
                return false;
            if (!Level.Equals(that.Level))
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            int result = Level.GetHashCode();
            result = 31*result + (Key != null ? Key.GetHashCode() : 0);
            return result;
        }
    }
}