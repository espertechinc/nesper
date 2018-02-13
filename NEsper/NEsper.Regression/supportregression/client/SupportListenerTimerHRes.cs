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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.client
{
    public class SupportListenerTimerHRes : UpdateListener {
        private List<Pair<long, EventBean[]>> newEvents = Collections.SynchronizedList(new List<Pair<long, EventBean[]>>());
    
        public void Update(EventBean[] newData, EventBean[] oldEvents) {
            long time = PerformanceObserver.NanoTime;
            newEvents.Add(new Pair<long, EventBean[]>(time, newData));
        }
    
        public List<Pair<long, EventBean[]>> GetNewEvents() {
            return newEvents;
        }
    }
} // end of namespace
