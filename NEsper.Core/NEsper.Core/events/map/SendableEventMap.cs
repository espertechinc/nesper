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

namespace com.espertech.esper.events.map
{
    public class SendableEventMap : SendableEvent
    {
        private readonly IDictionary<String, Object> _event;
        private readonly String _typeName;

        public SendableEventMap(IDictionary<String, Object> theEvent, String typeName)
        {
            _event = theEvent;
            _typeName = typeName;
        }

        public void Send(EPRuntime runtime)
        {
            runtime.SendEvent(_event, _typeName);
        }

        public object Underlying
        {
            get { return _event; }
        }
    }
}
