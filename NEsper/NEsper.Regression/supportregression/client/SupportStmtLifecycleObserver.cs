///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.core.service;

namespace com.espertech.esper.supportregression.client
{
    public class SupportStmtLifecycleObserver
    {
        public SupportStmtLifecycleObserver()
        {
            Events = new List<StatementLifecycleEvent>();
        }

        #region StatementLifecycleObserver Members

        public void Observe(Object sender, StatementLifecycleEvent theEvent)
        {
            Events.Add(theEvent);
            LastContext = theEvent.Parameters;
        }

        #endregion

        public object[] LastContext { get; private set; }

        public List<StatementLifecycleEvent> Events { get; private set; }

        public string EventsAsString
        {
            get { return Events.Aggregate("", (current, theEvent) => current + (theEvent.EventType + ";")); }
        }

        public void Flush()
        {
            Events.Clear();
        }
    }
}