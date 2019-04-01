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
    public class SupportListenerTimerHRes
    {
        private readonly IList<Pair<long, EventBean[]>> _newEvents = 
            CompatExtensions.AsSyncList(new List<Pair<long, EventBean[]>>());
    
        public void Update(object sender, UpdateEventArgs args) {
            long time = PerformanceObserver.NanoTime;
            _newEvents.Add(new Pair<long, EventBean[]>(time, args.NewEvents));
        }

        public IList<Pair<long, EventBean[]>> NewEvents => _newEvents;
    }
} // end of namespace
