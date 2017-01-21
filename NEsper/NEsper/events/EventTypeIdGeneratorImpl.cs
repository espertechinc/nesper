///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.events.bean;

namespace com.espertech.esper.events
{
    public class EventTypeIdGeneratorImpl : EventTypeIdGenerator
    {
        private int _currentEventTypeId;

        public int GetTypeId(String typeName)
        {
            return Interlocked.Increment(ref _currentEventTypeId);
        }

        public void AssignedType(String name, BeanEventType eventType)
        {
            // no op required
        }
    }
}
