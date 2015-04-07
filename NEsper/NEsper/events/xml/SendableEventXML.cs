///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;

using com.espertech.esper.client;

namespace com.espertech.esper.events.xml
{
    public class SendableEventXML : SendableEvent
    {
        private readonly XmlNode _event;

        public SendableEventXML(XmlNode theEvent)
        {
            _event = theEvent;
        }

        public void Send(EPRuntime runtime)
        {
            runtime.SendEvent(_event);
        }

        public object Underlying
        {
            get { return _event; }
        }
    }
}
