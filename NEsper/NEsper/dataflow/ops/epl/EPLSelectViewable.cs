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
using com.espertech.esper.view;


namespace com.espertech.esper.dataflow.ops.epl
{
    public class EPLSelectViewable : ViewSupport
    {
        private View _childView;
        private readonly EventBean[] _eventBatch = new EventBean[1];
        private readonly EventType _eventType;

        public EPLSelectViewable(EventType eventType)
        {
            _eventType = eventType;
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            return;
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.Empty<EventBean>();
        }

        public void Process(EventBean theEvent)
        {
            _eventBatch[0] = theEvent;
            _childView.Update(_eventBatch, null);
        }

        public override View AddView(View view)
        {
            _childView = view;
            return base.AddView(view);
        }
    }
}
