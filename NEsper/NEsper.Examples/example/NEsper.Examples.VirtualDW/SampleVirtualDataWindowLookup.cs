///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat.collections;

namespace NEsper.Examples.VirtualDW
{
    public class SampleVirtualDataWindowLookup : VirtualDataWindowLookup
    {
        private readonly VirtualDataWindowContext _context;
    
        public SampleVirtualDataWindowLookup(VirtualDataWindowContext context)
        {
            _context = context;
        }
    
        public ISet<EventBean> Lookup(Object[] keys, EventBean[] eventsPerStream)
        {
            // Add code to interogate lookup-keys here.
    
            // Create sample event.
            var eventData = new Dictionary<String, Object>();
            eventData.Put("key1", "sample1");
            eventData.Put("key2", "sample2");
            eventData.Put("value1", 100);
            eventData.Put("value2", 1.5d);
            EventBean theEvent = _context.EventFactory.Wrap(eventData);
            return new HashSet<EventBean> { theEvent };
        }
    }
}
