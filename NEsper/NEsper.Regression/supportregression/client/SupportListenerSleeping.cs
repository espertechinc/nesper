///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.client
{
    public class SupportListenerSleeping
    {
        private IList<Pair<long, EventBean[]>> newEvents =
            CompatExtensions.AsSyncList(new List<Pair<long, EventBean[]>>());
    
        private readonly long sleepTime;
    
        public SupportListenerSleeping(long sleepTime) {
            this.sleepTime = sleepTime;
        }

        public void Update(object sender, UpdateEventArgs e) {
            Update(e.NewEvents, e.OldEvents);
        }

        public void Update(EventBean[] newData, EventBean[] oldEvents) {
            long time = PerformanceObserver.NanoTime;
            newEvents.Add(new Pair<long, EventBean[]>(time, newData));
    
            try {
                Thread.Sleep((int) sleepTime);
            } catch (ThreadInterruptedException e) {
                throw new EPRuntimeException(e);
            }
        }

        public IList<Pair<long, EventBean[]>> NewEvents {
            get { return newEvents; }
        }
    }
} // end of namespace
