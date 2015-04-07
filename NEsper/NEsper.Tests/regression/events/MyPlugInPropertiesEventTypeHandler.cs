///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.plugin;

namespace com.espertech.esper.regression.events
{
    public class MyPlugInPropertiesEventTypeHandler : PlugInEventTypeHandler
    {
        private readonly MyPlugInPropertiesEventType _eventType;
    
        public MyPlugInPropertiesEventTypeHandler(MyPlugInPropertiesEventType eventType)
        {
            _eventType = eventType;
        }
    
        public EventSender GetSender(EPRuntimeEventSender runtimeEventSender)
        {
            return new MyPlugInPropertiesEventSender(_eventType, runtimeEventSender);
        }

        public EventType EventType
        {
            get { return _eventType; }
        }
    }
}
