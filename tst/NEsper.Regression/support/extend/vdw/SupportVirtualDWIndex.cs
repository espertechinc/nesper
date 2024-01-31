///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;

namespace com.espertech.esper.regressionlib.support.extend.vdw
{
    public class SupportVirtualDWIndex : VirtualDataWindowLookup
    {
        private readonly VirtualDataWindowContext context;
        private readonly SupportVirtualDW supportVirtualDW;

        public SupportVirtualDWIndex(
            SupportVirtualDW supportVirtualDW,
            VirtualDataWindowContext context)
        {
            this.supportVirtualDW = supportVirtualDW;
            this.context = context;
        }

        public ISet<EventBean> Lookup(
            object[] keys,
            EventBean[] eventsPerStream)
        {
            supportVirtualDW.LastKeys = keys;
            supportVirtualDW.LastAccessEvents = eventsPerStream;
            ISet<EventBean> events = new HashSet<EventBean>();
            foreach (var item in supportVirtualDW.Data) {
                events.Add(context.EventFactory.Wrap(item));
            }

            return events;
        }
    }
} // end of namespace