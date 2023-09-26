///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    public class IntersectAsymetricViewLocalState
    {
        private readonly EventBean[][] oldEventsPerView;
        private readonly ISet<EventBean> removalEvents = new HashSet<EventBean>();
        private readonly ArrayDeque<EventBean> newEvents = new ArrayDeque<EventBean>();

        private EventBean[] newDataChildView;
        private bool hasRemovestreamData;
        private bool retainObserverEvents;
        private bool discardObserverEvents;
        private ISet<EventBean> oldEvents = new HashSet<EventBean>();

        public IntersectAsymetricViewLocalState(EventBean[][] oldEventsPerView)
        {
            this.oldEventsPerView = oldEventsPerView;
        }

        public EventBean[][] OldEventsPerView => oldEventsPerView;

        public ISet<EventBean> RemovalEvents => removalEvents;

        public ArrayDeque<EventBean> NewEvents => newEvents;

        public EventBean[] NewDataChildView {
            get => newDataChildView;
            set => newDataChildView = value;
        }

        public bool HasRemovestreamData {
            set => hasRemovestreamData = value;
            get => hasRemovestreamData;
        }

        public bool IsRetainObserverEvents {
            get => retainObserverEvents;
            set => retainObserverEvents = value;
        }

        public bool IsDiscardObserverEvents {
            get => discardObserverEvents;
            set => discardObserverEvents = value;
        }

        public ISet<EventBean> OldEvents {
            get => oldEvents;
            set => oldEvents = value;
        }
    }
} // end of namespace