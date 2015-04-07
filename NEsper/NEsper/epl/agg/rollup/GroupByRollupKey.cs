///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.epl.agg.rollup
{
    public class GroupByRollupKey
    {
        private readonly EventBean[] _generator;
        private readonly Object _groupKey;
        private readonly AggregationGroupByRollupLevel _level;

        public GroupByRollupKey(EventBean[] generator, AggregationGroupByRollupLevel level, Object groupKey)
        {
            _generator = generator;
            _level = level;
            _groupKey = groupKey;
        }

        public EventBean[] Generator
        {
            get { return _generator; }
        }

        public AggregationGroupByRollupLevel Level
        {
            get { return _level; }
        }

        public object GroupKey
        {
            get { return _groupKey; }
        }

        public override String ToString()
        {
            return "GroupRollupKey{" +
                   "level=" + _level +
                   ", groupKey=" + _groupKey +
                   '}';
        }
    }
}