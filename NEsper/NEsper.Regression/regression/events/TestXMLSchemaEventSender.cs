///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Xml.XPath;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

//import static com.espertech.esper.supportregression.event.SupportXML.getDocument;

namespace com.espertech.esper.regression.events
{
    [TestFixture]
	public class TestXMLSchemaEventSender
    {
        [Test]
	    public void TestXML()
        {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        var typeMeta = new ConfigurationEventTypeXMLDOM();
	        typeMeta.RootElementName = "a";
	        typeMeta.AddXPathProperty("element1", "/a/b/c", XPathResultType.String);
	        configuration.AddEventType("AEvent", typeMeta);

	        var listener = new SupportUpdateListener();
	        var epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }

	        var stmtText = "select b.c as type, element1 from AEvent";
	        var stmt = epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(listener);

	        var doc = SupportXML.GetDocument("<a><b><c>text</c></b></a>");
	        var sender = epService.EPRuntime.GetEventSender("AEvent");
	        sender.SendEvent(doc);

	        var theEvent = listener.AssertOneGetNewAndReset();
	        Assert.AreEqual("text", theEvent.Get("type"));
	        Assert.AreEqual("text", theEvent.Get("element1"));

	        // send wrong event
	        try {
                sender.SendEvent(SupportXML.GetDocument("<xxxx><b><c>text</c></b></xxxx>"));
	            Assert.Fail();
	        } catch (EPException ex) {
	            Assert.AreEqual("Unexpected root element name 'xxxx' encountered, expected a root element name of 'a'", ex.Message);
	        }

	        try {
	            sender.SendEvent(new SupportBean());
                Assert.Fail();
	        } catch (EPException ex) {
	            Assert.AreEqual("Unexpected event object type '" + Name.Of<SupportBean>()+ "' encountered, please supply a XmlDocument or XmlElement node", ex.Message);
	        }

	        // test adding a second type for the same root element
	        configuration = SupportConfigFactory.GetConfiguration();
	        typeMeta = new ConfigurationEventTypeXMLDOM();
	        typeMeta.RootElementName = "a";
            typeMeta.AddXPathProperty("element2", "//c", XPathResultType.String);
	        typeMeta.IsEventSenderValidatesRoot = false;
	        epService.EPAdministrator.Configuration.AddEventType("BEvent", typeMeta);

	        stmtText = "select element2 from BEvent#lastevent";
	        var stmtTwo = epService.EPAdministrator.CreateEPL(stmtText);

	        // test sender that doesn't care about the root element
	        var senderTwo = epService.EPRuntime.GetEventSender("BEvent");
            senderTwo.SendEvent(SupportXML.GetDocument("<xxxx><b><c>text</c></b></xxxx>"));    // allowed, not checking

            theEvent = stmtTwo.First();
	        Assert.AreEqual("text", theEvent.Get("element2"));

	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	    }
	}
} // end of namespace
