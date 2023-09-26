///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup
{
    public class EventArrayAndSortKeyArray
    {
        private readonly IList<EventBean>[] eventsPerLevel;
        private readonly IList<object>[] sortKeyPerLevel;

        public EventArrayAndSortKeyArray(
            IList<EventBean>[] eventsPerLevel,
            IList<object>[] sortKeyPerLevel)
        {
            this.eventsPerLevel = eventsPerLevel;
            this.sortKeyPerLevel = sortKeyPerLevel;
        }

        public IList<EventBean>[] EventsPerLevel => eventsPerLevel;

        public IList<object>[] SortKeyPerLevel => sortKeyPerLevel;

        public void Reset()
        {
            foreach (var anEventsPerLevel in eventsPerLevel) {
                anEventsPerLevel.Clear();
            }

            if (sortKeyPerLevel != null) {
                foreach (var anSortKeyPerLevel in sortKeyPerLevel) {
                    anSortKeyPerLevel.Clear();
                }
            }
        }
    }
} // end of namespace