///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.view.internals
{
    public class IntersectBatchViewLocalState
    {
        private readonly EventBean[][] _oldEventsPerView;
        private readonly EventBean[][] _newEventsPerView;
        private readonly ISet<EventBean> _removedEvents = new LinkedHashSet<EventBean>();
        private bool _captureIrNonBatch;
        private bool _ignoreViewIrStream;

        public IntersectBatchViewLocalState(EventBean[][] oldEventsPerView, EventBean[][] newEventsPerView)
        {
            _oldEventsPerView = oldEventsPerView;
            _newEventsPerView = newEventsPerView;
        }

        public EventBean[][] OldEventsPerView
        {
            get { return _oldEventsPerView; }
        }

        public EventBean[][] NewEventsPerView
        {
            get { return _newEventsPerView; }
        }

        public ISet<EventBean> RemovedEvents
        {
            get { return _removedEvents; }
        }

        public bool IsCaptureIRNonBatch
        {
            get { return _captureIrNonBatch; }
            set { _captureIrNonBatch = value; }
        }

        public bool IsIgnoreViewIRStream
        {
            get { return _ignoreViewIrStream; }
            set { _ignoreViewIrStream = value; }
        }
    }
} // end of namespace
