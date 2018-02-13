///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.view;

namespace com.espertech.esper.supportregression.client
{
    public class MyFlushedSimpleView : ViewSupport
    {
        private IList<EventBean> _events;
        private EventType _eventType;
    
        public MyFlushedSimpleView(AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            agentInstanceContext.AddTerminationCallback(Stop);
            _events = new List<EventBean>();
        }
    
        public void Stop()
        {
            UpdateChildren(_events.ToArray(), null);
            _events = new List<EventBean>();
        }

        public override Viewable Parent
        {
            set
            {
                base.Parent = value;
                if (value != null)
                {
                    _eventType = value.EventType;
                }
            }
        }

        public View CloneView(AgentInstanceViewFactoryChainContext agentInstanceContext)
        {
            return new MyFlushedSimpleView(agentInstanceContext);
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (newData != null)
            {
                for (int i = 0; i < newData.Length; i++)
                {
                    _events.Add(newData[0]);
                }
            }
        }

        public override EventType EventType
        {
            get { return _eventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return _events.GetEnumerator();
        }
    
        public override String ToString()
        {
            return GetType().FullName;
        }
    }
}
