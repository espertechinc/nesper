///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.named;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.epl
{
    public class SupportNamedWindowObserver : NamedWindowLifecycleObserver
    {
        private readonly IList<NamedWindowLifecycleEvent> _events = new List<NamedWindowLifecycleEvent>();
    
        public void Observe(NamedWindowLifecycleEvent theEvent)
        {
            _events.Add(theEvent);
        }

        public IList<NamedWindowLifecycleEvent> Events
        {
            get { return _events; }
        }

        public NamedWindowLifecycleEvent GetFirstAndReset()
        {
            Assert.AreEqual(1, _events.Count);
            NamedWindowLifecycleEvent theEvent = _events[0];
            _events.Clear();
            return theEvent;
        }
    }
}
