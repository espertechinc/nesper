///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filtersvc;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class SupportFilterHandle : FilterHandleCallback,
        FilterHandle
    {
        public int CountInvoked { get; set; }

        public EventBean LastEvent { get; set; }

        public int StatementId => 1;

        public int AgentInstanceId => -1;

        public virtual void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            CountInvoked++;
            LastEvent = theEvent;
        }

        public bool IsSubSelect => false;

        public int GetAndResetCountInvoked()
        {
            var count = CountInvoked;
            CountInvoked = 0;
            return count;
        }
    }
} // end of namespace
