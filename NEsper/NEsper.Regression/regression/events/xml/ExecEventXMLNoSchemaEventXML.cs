///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml.XPath;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.regression.events.xml.ExecEventXMLNoSchemaNestedXMLDOMGetter;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaEventXML : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var desc = new ConfigurationEventTypeXMLDOM();
            desc.AddXPathProperty("event.type", "/event/@type", XPathResultType.String);
            desc.AddXPathProperty("event.uid", "/event/@uid", XPathResultType.String);
            desc.RootElementName = "event";
            configuration.AddEventType("MyEvent", desc);
        }
    
        public override void Run(EPServiceProvider epService) {
            string stmt = "select event.type as type, event.uid as uid from MyEvent";
            EPStatement joinView = epService.EPAdministrator.CreateEPL(stmt);
            var updateListener = new SupportUpdateListener();
            joinView.Events += updateListener.Update;
    
            SendXMLEvent(epService, "<event type=\"a-f-G\" uid=\"terminal.55\" time=\"2007-04-19T13:05:20.22Z\" version=\"2.0\"></event>");
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("a-f-G", theEvent.Get("type"));
            Assert.AreEqual("terminal.55", theEvent.Get("uid"));
        }
    }
} // end of namespace
