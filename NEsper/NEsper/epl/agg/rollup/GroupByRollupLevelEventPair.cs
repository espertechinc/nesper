///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.epl.agg.rollup
{
    public class GroupByRollupLevelEventPair
    {
        private readonly AggregationGroupByRollupLevel _level;
        private readonly EventBean _event;
    
        public GroupByRollupLevelEventPair(AggregationGroupByRollupLevel level, EventBean @event)
        {
            _level = level;
            _event = @event;
        }

        public AggregationGroupByRollupLevel Level
        {
            get { return _level; }
        }

        public EventBean Event
        {
            get { return _event; }
        }
    }
}
