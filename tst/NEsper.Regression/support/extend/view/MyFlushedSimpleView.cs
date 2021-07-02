///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.regressionlib.support.extend.view
{
    public class MyFlushedSimpleView : ViewSupport,
        AgentInstanceMgmtCallback
    {
        private IList<EventBean> _events;
        private EventType _eventType;

        public MyFlushedSimpleView(AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            _events = new List<EventBean>();
        }

        public override EventType EventType => _eventType;

        public void Stop(AgentInstanceStopServices services)
        {
            child.Update(_events.ToArray(), null);
            _events = new List<EventBean>();
        }

        public void Transfer(AgentInstanceTransferServices services)
        {
        }

        public override Viewable Parent {
            set {
                base.Parent = value;
                if (value != null) {
                    _eventType = value.EventType;
                }
            }
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    _events.Add(newData[0]);
                }
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _events.GetEnumerator();
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
} // end of namespace