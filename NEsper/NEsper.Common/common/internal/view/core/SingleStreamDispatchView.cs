///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.view.core
{
    /// <summary>
    ///     View to dispatch for a single stream (no join).
    /// </summary>
    public class SingleStreamDispatchView : ViewSupport,
        EPStatementDispatch
    {
        private bool hasData;
        private readonly FlushedEventBuffer newDataBuffer = new FlushedEventBuffer();
        private readonly FlushedEventBuffer oldDataBuffer = new FlushedEventBuffer();

        public override EventType EventType => Parent.EventType;

        public void Execute()
        {
            if (hasData) {
                hasData = false;
                Child.Update(newDataBuffer.GetAndFlush(), oldDataBuffer.GetAndFlush());
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Parent.GetEnumerator();
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            newDataBuffer.Add(newData);
            oldDataBuffer.Add(oldData);
            hasData = true;
        }
    }
} // end of namespace