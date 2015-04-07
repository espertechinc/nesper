///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.rettype
{
    /// <summary>
    /// Clazz can be either - Collection - Array i.e. "EventType[].class"
    /// </summary>
    public class EventMultiValuedEPType : EPType
    {
        internal EventMultiValuedEPType(Type container, EventType component)
        {
            Container = container;
            Component = component;
        }

        public Type Container { get; private set; }

        public EventType Component { get; private set; }
    }
}
