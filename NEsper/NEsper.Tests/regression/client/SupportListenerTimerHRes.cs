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
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;


namespace com.espertech.esper.regression.client
{
    public class SupportListenerTimerHRes
    {
        private readonly IList<Pair<long, EventBean[]>> _newEvents = new List<Pair<long, EventBean[]>>().AsSyncList();

        public void Update(Object sender, UpdateEventArgs e)
        {
            long time = DateTimeHelper.CurrentTimeNanos;
            _newEvents.Add(new Pair<long, EventBean[]>(time, e.NewEvents));
        }

        public IList<Pair<long, EventBean[]>> NewEvents
        {
            get { return _newEvents; }
        }
    }
}
