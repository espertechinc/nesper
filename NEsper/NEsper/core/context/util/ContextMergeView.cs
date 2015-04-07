///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.util
{
    public class ContextMergeView
        : ViewSupport
        , UpdateDispatchView
    {
        private readonly EventType _eventType;

        public ContextMergeView(EventType eventType)
        {
            _eventType = eventType;
        }

        #region UpdateDispatchView Members

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            // no action required
        }

        public void NewResult(UniformPair<EventBean[]> result)
        {
            if (result != null)
            {
                UpdateChildren(result.First, result.Second);
            }
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            throw new UnsupportedOperationException("GetEnumerator not supported");
        }

        #endregion
    }
}