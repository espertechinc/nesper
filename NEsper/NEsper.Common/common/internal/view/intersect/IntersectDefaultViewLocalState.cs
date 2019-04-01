///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.view.intersect
{
    public class IntersectDefaultViewLocalState
    {
        public IntersectDefaultViewLocalState(EventBean[][] oldEventsPerView)
        {
            OldEventsPerView = oldEventsPerView;
        }

        public EventBean[][] OldEventsPerView { get; }

        public ISet<EventBean> RemovalEvents { get; } = new HashSet<EventBean>();

        public bool HasRemovestreamData { get; set; }

        public bool IsRetainObserverEvents { get; set; }

        public bool IsDiscardObserverEvents { get; set; }
    }
} // end of namespace