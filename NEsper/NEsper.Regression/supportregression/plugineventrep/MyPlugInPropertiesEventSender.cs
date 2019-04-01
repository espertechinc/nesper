///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;

namespace com.espertech.esper.supportregression.plugineventrep
{
    public class MyPlugInPropertiesEventSender : EventSender
    {
        private readonly MyPlugInPropertiesEventType _type;
        private readonly EPRuntimeEventSender _runtimeSender;
    
        public MyPlugInPropertiesEventSender(MyPlugInPropertiesEventType type, EPRuntimeEventSender runtimeSender)
        {
            _type = type;
            _runtimeSender = runtimeSender;
        }
    
        public void SendEvent(Object theEvent)
        {
            if (!(theEvent is Properties))
            {
                throw new EPException("Sender expects a properties event");
            }
            EventBean eventBean = new MyPlugInPropertiesEventBean(_type, (Properties) theEvent);
            _runtimeSender.ProcessWrappedEvent(eventBean);
        }
    
        public void Route(Object theEvent)
        {
            if (!(theEvent is Properties))
            {
                throw new EPException("Sender expects a properties event");
            }
            EventBean eventBean = new MyPlugInPropertiesEventBean(_type, (Properties) theEvent);
            _runtimeSender.RouteEventBean(eventBean);
        }
    }
}
