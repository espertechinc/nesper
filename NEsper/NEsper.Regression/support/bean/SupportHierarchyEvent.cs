///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportHierarchyEvent
    {
        public SupportHierarchyEvent(
            int? event_criteria_id,
            int? priority,
            int? parentEventCriteriaId)
        {
            Event_criteria_id = event_criteria_id;
            Priority = priority;
            Parent_event_criteria_id = parentEventCriteriaId;
        }

        public int? Event_criteria_id { get; }

        public int? Priority { get; }

        public int? Parent_event_criteria_id { get; }

        public override string ToString()
        {
            return "ecId=" +
                   Event_criteria_id +
                   " prio=" +
                   Priority +
                   " parent=" +
                   Parent_event_criteria_id;
        }
    }
} // end of namespace