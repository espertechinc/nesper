///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.support.patternassert
{
    public class EventSendDesc
    {
        public EventSendDesc(
            SupportMarketDataBean theEvent,
            string eventDesc)
        {
            TheEvent = theEvent;
            EventDesc = eventDesc;
        }

        public SupportMarketDataBean TheEvent { get; }

        public string EventDesc { get; }
    }
} // end of namespace