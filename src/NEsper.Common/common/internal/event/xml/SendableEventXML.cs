///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.xml
{
    public class SendableEventXML : SendableEvent
    {
        private readonly XmlNode _event;
        private readonly string _typeName;

        public SendableEventXML(
            XmlNode @event,
            string typeName)
        {
            this._event = @event;
            this._typeName = typeName;
        }

        public void Send(EventServiceSendEventCommon eventService)
        {
            eventService.SendEventXMLDOM(_event, _typeName);
        }

        public object Underlying => _event;
    }
} // end of namespace