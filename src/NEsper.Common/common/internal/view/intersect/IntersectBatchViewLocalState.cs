///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.view.intersect
{
    public class IntersectBatchViewLocalState
    {
        public IntersectBatchViewLocalState(
            EventBean[][] oldEventsPerView,
            EventBean[][] newEventsPerView)
        {
            OldEventsPerView = oldEventsPerView;
            NewEventsPerView = newEventsPerView;
        }

        public EventBean[][] OldEventsPerView { get; }

        public EventBean[][] NewEventsPerView { get; }

        public ISet<EventBean> RemovedEvents { get; } = new LinkedHashSet<EventBean>();

        public bool IsCaptureIRNonBatch { get; set; }

        public bool IsIgnoreViewIRStream { get; set; }
    }
} // end of namespace