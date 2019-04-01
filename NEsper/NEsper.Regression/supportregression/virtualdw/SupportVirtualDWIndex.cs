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
using com.espertech.esper.client.hook;

namespace com.espertech.esper.supportregression.virtualdw
{
    public class SupportVirtualDWIndex : VirtualDataWindowLookup
    {
        private readonly SupportVirtualDW _supportVirtualDw;
        private readonly VirtualDataWindowContext _context;
    
        public SupportVirtualDWIndex(SupportVirtualDW supportVirtualDW, VirtualDataWindowContext context)
        {
            _supportVirtualDw = supportVirtualDW;
            _context = context;
        }
    
        public ISet<EventBean> Lookup(object[] keys, EventBean[] eventsPerStream)
        {
            _supportVirtualDw.LastKeys = keys;
            _supportVirtualDw.LastAccessEvents = eventsPerStream;

            var events = new HashSet<EventBean>();
            foreach (var item in _supportVirtualDw.Data) {
                events.Add(_context.EventFactory.Wrap(item));
            }

            return events;
        }
    }
}
