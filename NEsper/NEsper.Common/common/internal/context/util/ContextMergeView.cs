///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.context.util
{
    public class ContextMergeView
        : ViewSupport
        , UpdateDispatchView
    {
        public ContextMergeView(EventType eventType)
        {
            EventType = eventType;
        }

        public override EventType EventType { get; }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            // no action required
        }

        public void NewResult(UniformPair<EventBean[]> result)
        {
            if (result != null && Child != null) {
                Child.Update(result.First, result.Second);
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            throw new UnsupportedOperationException("Iterator not supported");
        }
    }
} // end of namespace