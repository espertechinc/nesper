///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.map
{
    public class SendableEventMap : SendableEvent
    {
        private readonly IDictionary<string, object> @event;
        private readonly string typeName;

        public SendableEventMap(IDictionary<string, object> @event, string typeName)
        {
            this.@event = @event;
            this.typeName = typeName;
        }

        public void Send(EventServiceSendEventCommon eventService)
        {
            eventService.SendEventMap(@event, typeName);
        }

        public object Underlying => @event;
    }
} // end of namespace