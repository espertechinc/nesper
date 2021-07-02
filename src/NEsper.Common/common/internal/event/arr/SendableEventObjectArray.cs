///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.arr
{
    public class SendableEventObjectArray : SendableEvent
    {
        private readonly object[] _event;
        private readonly string _typeName;

        public SendableEventObjectArray(
            object[] @event,
            string typeName)
        {
            this._event = @event;
            this._typeName = typeName;
        }

        public void Send(EventServiceSendEventCommon eventService)
        {
            eventService.SendEventObjectArray(_event, _typeName);
        }

        public object Underlying => _event;
    }
} // end of namespace