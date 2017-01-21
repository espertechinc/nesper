///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.service;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// View to dispatch for a single stream (no join).
    ///  </summary>
    public sealed class SingleStreamDispatchView : ViewSupport, EPStatementDispatch
    {
        private bool _hasData = false;
        private readonly FlushedEventBuffer _newDataBuffer = new FlushedEventBuffer();
        private readonly FlushedEventBuffer _oldDataBuffer = new FlushedEventBuffer();

        public override EventType EventType
        {
            get { return Parent.EventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return Parent.GetEnumerator();
        }

        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            _newDataBuffer.Add(newData);
            _oldDataBuffer.Add(oldData);
            _hasData = true;
        }

        public void Execute()
        {
            if (_hasData)
            {
                _hasData = false;
                UpdateChildren(_newDataBuffer.GetAndFlush(), _oldDataBuffer.GetAndFlush());
            }
        }
    }
}
