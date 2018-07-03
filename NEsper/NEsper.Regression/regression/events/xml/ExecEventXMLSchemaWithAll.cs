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
using com.espertech.esper.compat.container;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLSchemaWithAll : RegressionExecution {
        public static readonly string CLASSLOADER_SCHEMA_WITH_ALL_URI = "regression/simpleSchemaWithAll.xsd";
    
        public override void Configure(Configuration configuration) {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "event-page-visit";
            string schemaUri = SupportContainer.Instance.ResourceManager().ResolveResourceURL(CLASSLOADER_SCHEMA_WITH_ALL_URI).ToString();
            eventTypeMeta.SchemaResource = schemaUri;
            eventTypeMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchemaWithAll");
            eventTypeMeta.AddXPathProperty("url", "/ss:event-page-visit/ss:url", XPathResultType.String);
            configuration.AddEventType("PageVisitEvent", eventTypeMeta);
        }
    
        public override void Run(EPServiceProvider epService) {
            // url='page4'
            string text = "select a.url as sesja from pattern [ every a=PageVisitEvent(url='page1') ]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var updateListener = new SupportUpdateListener();
            stmt.Events += updateListener.Update;
    
            SupportXML.SendEvent(epService.EPRuntime,
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<event-page-visit xmlns=\"samples:schemas:simpleSchemaWithAll\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"samples:schemas:simpleSchemaWithAll simpleSchemaWithAll.xsd\">\n" +
                            "<url>page1</url>" +
                            "</event-page-visit>");
            EventBean theEvent = updateListener.LastNewData[0];
            Assert.AreEqual("page1", theEvent.Get("sesja"));
            updateListener.Reset();
    
            SupportXML.SendEvent(epService.EPRuntime,
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<event-page-visit xmlns=\"samples:schemas:simpleSchemaWithAll\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"samples:schemas:simpleSchemaWithAll simpleSchemaWithAll.xsd\">\n" +
                            "<url>page2</url>" +
                            "</event-page-visit>");
            Assert.IsFalse(updateListener.IsInvoked);
    
            EventType type = epService.EPAdministrator.CreateEPL("select * from PageVisitEvent").EventType;
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("sessionId", typeof(XmlNode), null, false, false, false, false, true),
                    new EventPropertyDescriptor("customerId", typeof(XmlNode), null, false, false, false, false, true),
                    new EventPropertyDescriptor("url", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("method", typeof(XmlNode), null, false, false, false, false, true),
            }, type.PropertyDescriptors);
        }
    }
} // end of namespace
