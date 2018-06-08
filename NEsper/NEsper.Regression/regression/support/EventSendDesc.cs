///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.supportregression.bean;

namespace com.espertech.esper.regression.support
{
    public class EventSendDesc
    {
        private SupportMarketDataBean theEvent;
        private String eventDesc;
    
        public EventSendDesc(SupportMarketDataBean theEvent, String eventDesc) {
            this.theEvent = theEvent;
            this.eventDesc = eventDesc;
        }

        public SupportMarketDataBean Event
        {
            get { return theEvent; }
        }

        public string EventDesc
        {
            get { return eventDesc; }
        }
    }
}
