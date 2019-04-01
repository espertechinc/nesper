///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Xml;
using System.Xml.XPath;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaNestedXMLDOMGetter : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            var xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "a";
            xmlDOMEventTypeDesc.AddXPathProperty("element1", "/a/b/c", XPathResultType.String);
            configuration.AddEventType("AEvent", xmlDOMEventTypeDesc);
        }
    
        public override void Run(EPServiceProvider epService) {
    
            string stmt = "select b.c as type, element1, result1 from AEvent";
            EPStatement joinView = epService.EPAdministrator.CreateEPL(stmt);
            var updateListener = new SupportUpdateListener();
            joinView.Events += updateListener.Update;
    
            SendXMLEvent(epService, "<a><b><c></c></b></a>");
            EventBean theEvent = updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("", theEvent.Get("type"));
            Assert.AreEqual("", theEvent.Get("element1"));
    
            SendXMLEvent(epService, "<a><b></b></a>");
            theEvent = updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("type"));
            Assert.AreEqual(null, theEvent.Get("element1"));
    
            SendXMLEvent(epService, "<a><b><c>text</c></b></a>");
            theEvent = updateListener.AssertOneGetNewAndReset();
            Assert.AreEqual("text", theEvent.Get("type"));
            Assert.AreEqual("text", theEvent.Get("element1"));
        }
    
        internal static void SendXMLEvent(EPServiceProvider epService, string xml) {
            var simpleDoc = new XmlDocument();
            simpleDoc.LoadXml(xml);
            epService.EPRuntime.SendEvent(simpleDoc);
        }
    }
    
    
    
    
    
} // end of namespace
