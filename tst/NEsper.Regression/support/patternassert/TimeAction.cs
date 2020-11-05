///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.support.patternassert
{
    public class TimeAction
    {
        public TimeAction()
        {
            Events = new List<EventSendDesc>();
        }

        public IList<EventSendDesc> Events { get; }

        public string ActionDesc { get; set; }

        public void Add(
            SupportMarketDataBean theEvent,
            string eventDesc)
        {
            Events.Add(new EventSendDesc(theEvent, eventDesc));
        }
    }
} // end of namespace