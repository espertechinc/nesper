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
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.xml;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaSimpleXMLXPathProperties : RegressionExecution {
        internal static readonly string XML_NOSCHEMAEVENT =
                "<myevent>\n" +
                        "  <element1>VAL1</element1>\n" +
                        "  <element2>\n" +
                        "    <element21 id=\"e21_1\">VAL21-1</element21>\n" +
                        "    <element21 id=\"e21_2\">VAL21-2</element21>\n" +
                        "  </element2>\n" +
                        "  <element3 attrString=\"VAL3\" attrNum=\"5\" attrBool=\"true\"/>\n" +
                        "  <element4><element41>VAL4-1</element41></element4>\n" +
                        "</myevent>";
    
        public override void Configure(Configuration configuration) {
            var xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "myevent";
            xmlDOMEventTypeDesc.AddXPathProperty("xpathElement1", "/myevent/element1", XPathResultType.String);
            xmlDOMEventTypeDesc.AddXPathProperty("xpathCountE21", "count(/myevent/element2/element21)", XPathResultType.Number);
            xmlDOMEventTypeDesc.AddXPathProperty("xpathAttrString", "/myevent/element3/@attrString", XPathResultType.String);
            xmlDOMEventTypeDesc.AddXPathProperty("xpathAttrNum", "/myevent/element3/@attrNum", XPathResultType.Number);
            xmlDOMEventTypeDesc.AddXPathProperty("xpathAttrBool", "/myevent/element3/@attrBool", XPathResultType.Boolean);
            xmlDOMEventTypeDesc.AddXPathProperty("stringCastLong", "/myevent/element3/@attrNum", XPathResultType.String, "long");
            xmlDOMEventTypeDesc.AddXPathProperty("stringCastDouble", "/myevent/element3/@attrNum", XPathResultType.String, "double");
            xmlDOMEventTypeDesc.AddXPathProperty("numCastInt", "/myevent/element3/@attrNum", XPathResultType.Number, "int");
            xmlDOMEventTypeDesc.XPathFunctionResolver = typeof(SupportXPathFunctionResolver).FullName;
            xmlDOMEventTypeDesc.XPathVariableResolver = typeof(SupportXPathVariableResolver).FullName;
            configuration.AddEventType("TestXMLNoSchemaType", xmlDOMEventTypeDesc);
    
            xmlDOMEventTypeDesc = new ConfigurationEventTypeXMLDOM();
            xmlDOMEventTypeDesc.RootElementName = "my.event2";
            configuration.AddEventType("TestXMLWithDots", xmlDOMEventTypeDesc);
        }
    
        public override void Run(EPServiceProvider epService) {
            var updateListener = new SupportUpdateListener();
    
            // assert type metadata
            EventTypeSPI type = (EventTypeSPI) ((EPServiceProviderSPI) epService).EventAdapterService.GetEventTypeByName("TestXMLNoSchemaType");
            Assert.AreEqual(ApplicationType.XML, type.Metadata.OptionalApplicationType);
            Assert.AreEqual(null, type.Metadata.OptionalSecondaryNames);
            Assert.AreEqual("TestXMLNoSchemaType", type.Metadata.PrimaryName);
            Assert.AreEqual("TestXMLNoSchemaType", type.Metadata.PublicName);
            Assert.AreEqual("TestXMLNoSchemaType", type.Name);
            Assert.AreEqual(TypeClass.APPLICATION, type.Metadata.TypeClass);
            Assert.AreEqual(true, type.Metadata.IsApplicationConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfigured);
            Assert.AreEqual(true, type.Metadata.IsApplicationPreConfiguredStatic);
    
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("xpathElement1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("xpathCountE21", typeof(double?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("xpathAttrString", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("xpathAttrNum", typeof(double?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("xpathAttrBool", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("stringCastLong", typeof(long?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("stringCastDouble", typeof(double?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("numCastInt", typeof(int?), null, false, false, false, false, false),
            }, type.PropertyDescriptors);
    
            string stmt =
                    "select xpathElement1, xpathCountE21, xpathAttrString, xpathAttrNum, xpathAttrBool," +
                            "stringCastLong," +
                            "stringCastDouble," +
                            "numCastInt " +
                            "from TestXMLNoSchemaType#length(100)";
    
            EPStatement joinView = epService.EPAdministrator.CreateEPL(stmt);
            joinView.Events += updateListener.Update;
    
            // Generate document with the specified in element1 to confirm we have independent events
            SendEvent(epService, "EventA");
            AssertDataSimpleXPath(updateListener, "EventA");
    
            SendEvent(epService, "EventB");
            AssertDataSimpleXPath(updateListener, "EventB");
        }
    
        internal static void AssertDataSimpleXPath(SupportUpdateListener updateListener, string element1) {
            Assert.IsNotNull(updateListener.LastNewData);
            EventBean theEvent = updateListener.LastNewData[0];
    
            Assert.AreEqual(element1, theEvent.Get("xpathElement1"));
            Assert.AreEqual(2.0, theEvent.Get("xpathCountE21"));
            Assert.AreEqual("VAL3", theEvent.Get("xpathAttrString"));
            Assert.AreEqual(5d, theEvent.Get("xpathAttrNum"));
            Assert.AreEqual(true, theEvent.Get("xpathAttrBool"));
            Assert.AreEqual(5L, theEvent.Get("stringCastLong"));
            Assert.AreEqual(5d, theEvent.Get("stringCastDouble"));
            Assert.AreEqual(5, theEvent.Get("numCastInt"));
        }
    
        internal static void SendEvent(EPServiceProvider epService, string value) {
            string xml = XML_NOSCHEMAEVENT.RegexReplaceAll("VAL1", value);
            Log.Debug(".sendEvent value=" + value);

            var simpleDoc = new XmlDocument();
            simpleDoc.LoadXml(xml);
    
            epService.EPRuntime.SendEvent(simpleDoc);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
