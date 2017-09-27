///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.view;

namespace com.espertech.esper.supportunit.events
{
    public class SupportValueAddEventService : ValueAddEventService
    {
        public void Init(IDictionary<String, ConfigurationRevisionEventType> revisionTypes, IDictionary<String, ConfigurationVariantStream> variantStreams, EventAdapterService eventAdapterService, EventTypeIdGenerator eventTypeIdGenerator)
        {
        }
    
        public void AddRevisionEventType(String name, ConfigurationRevisionEventType config, EventAdapterService eventAdapterService)
        {
        }
    
        public void AddVariantStream(String variantEventTypeName, ConfigurationVariantStream variantStreamConfig, EventAdapterService eventAdapterService, EventTypeIdGenerator eventTypeIdGenerator)
        {
        }
    
        public EventType GetValueAddUnderlyingType(String name)
        {
            return null;
        }
    
        public EventType CreateRevisionType(String namedWindowName, String typeName, StatementStopService statementStopService, EventAdapterService eventAdapterService, EventTypeIdGenerator eventTypeIdGenerator)
        {
            return null;
        }
    
        public bool IsRevisionTypeName(String name)
        {
            return false;
        }
    
        public ValueAddEventProcessor GetValueAddProcessor(String name)
        {
            return null;
        }

        public EventType[] ValueAddedTypes
        {
            get { return new EventType[0]; }
        }
    }
}
