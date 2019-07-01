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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    public class SupportEventEvaluator : EventEvaluator
    {
        private int countInvoked;

        public EventBean LastEvent { get; set; }

        public ICollection<FilterHandle> LastMatches { get; set; }

        public int CountInvoked
        {
            set => countInvoked = value;
        }

        public void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches)
        {
            countInvoked++;
            LastEvent = theEvent;
            LastMatches = matches;
        }

        public int GetAndResetCountInvoked()
        {
            var count = countInvoked;
            countInvoked = 0;
            return count;
        }

        public void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack)
        {
            throw new UnsupportedOperationException();
        }
    }
} // end of namespace