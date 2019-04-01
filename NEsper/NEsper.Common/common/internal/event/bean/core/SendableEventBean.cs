///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
    public class SendableEventBean : SendableEvent
    {
        private readonly string typeName;

        public SendableEventBean(object @event, string typeName)
        {
            Underlying = @event;
            this.typeName = typeName;
        }

        public void Send(EventServiceSendEventCommon eventService)
        {
            eventService.SendEventBean(Underlying, typeName);
        }

        public object Underlying { get; }
    }
} // end of namespace