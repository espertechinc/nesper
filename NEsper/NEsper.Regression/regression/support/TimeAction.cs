///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.supportregression.bean;

namespace com.espertech.esper.regression.support
{
    public class TimeAction
    {
        private readonly IList<EventSendDesc> events;

        public TimeAction()
        {
            events = new List<EventSendDesc>();
        }

        public string ActionDesc { set; get; }

        public IList<EventSendDesc> Events
        {
            get { return events; }
        }

        public void Add(SupportMarketDataBean theEvent, String eventDesc)
        {
            events.Add(new EventSendDesc(theEvent, eventDesc));
        }
    }
}
