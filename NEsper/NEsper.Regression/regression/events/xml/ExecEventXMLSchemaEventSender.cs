///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Xml;
using System.Xml.XPath;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.events.SupportXML;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLSchemaEventSender : RegressionExecution {
        public override void Configure(Configuration configuration) {
            var typeMeta = new ConfigurationEventTypeXMLDOM();
            typeMeta.RootElementName = "a";
            typeMeta.AddXPathProperty("element1", "/a/b/c", XPathResultType.String);
            configuration.AddEventType("AEvent", typeMeta);
        }
    
        public override void Run(EPServiceProvider epService) {
            var listener = new SupportUpdateListener();
            string stmtText = "select b.c as type, element1 from AEvent";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += listener.Update;
    
            XmlDocument doc = GetDocument("<a><b><c>text</c></b></a>");
            EventSender sender = epService.EPRuntime.GetEventSender("AEvent");
            sender.SendEvent(doc);
    
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("text", theEvent.Get("type"));
            Assert.AreEqual("text", theEvent.Get("element1"));
    
            // send wrong event
            try {
                sender.SendEvent(GetDocument("<xxxx><b><c>text</c></b></xxxx>"));
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Unexpected root element name 'xxxx' encountered, expected a root element name of 'a'", ex.Message);
            }
    
            try {
                sender.SendEvent(new SupportBean());
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Unexpected event object type '" + typeof(SupportBean).FullName + "' encountered, please supply a XmlDocument or XmlElement node", ex.Message);
            }
    
            // test adding a second type for the same root element
            var typeMeta = new ConfigurationEventTypeXMLDOM();
            typeMeta.RootElementName = "a";
            typeMeta.AddXPathProperty("element2", "//c", XPathResultType.String);
            typeMeta.IsEventSenderValidatesRoot = false;
            epService.EPAdministrator.Configuration.AddEventType("BEvent", typeMeta);
    
            stmtText = "select element2 from BEvent#lastevent";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmtText);
    
            // test sender that doesn't care about the root element
            EventSender senderTwo = epService.EPRuntime.GetEventSender("BEvent");
            senderTwo.SendEvent(GetDocument("<xxxx><b><c>text</c></b></xxxx>"));    // allowed, not checking
    
            theEvent = stmtTwo.First();
            Assert.AreEqual("text", theEvent.Get("element2"));
        }
    }
} // end of namespace
