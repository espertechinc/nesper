///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.filter;

namespace com.espertech.esper.support.filter
{
    public class SupportFilterHandle : FilterHandleCallback
    {
        public void MatchFound(EventBean theEvent, ICollection<FilterHandleCallback> allStmtMatches)
        {
            CountInvoked++;
            LastEvent = theEvent;
        }

        public bool IsSubSelect
        {
            get { return false; }
        }

        public int CountInvoked { get; set; }

        public EventBean LastEvent { get; set; }

        public int GetAndResetCountInvoked()
        {
            int count = CountInvoked;
            CountInvoked = 0;
            return count;
        }

        public string StatementId
        {
            get { return ""; }
        }
    }
}
