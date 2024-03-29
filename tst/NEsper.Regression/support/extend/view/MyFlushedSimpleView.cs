///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private IList<EventBean> events;
        private EventType eventType;

        public MyFlushedSimpleView(AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            events = new List<EventBean>();
        }

        public override EventType EventType => eventType;

        public void Stop(AgentInstanceStopServices services)
        {
            Child.Update(events.ToArray(), null);
            events = new List<EventBean>();
        }

        public void Transfer(AgentInstanceTransferServices services)
        {
        }

        public override Viewable Parent {
            set {
                base.Parent = value;
                if (value != null) {
                    eventType = value.EventType;
                }
            }
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    events.Add(newData[0]);
                }
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return events.GetEnumerator();
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
} // end of namespace