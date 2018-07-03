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
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLSchemaXPathBacked : RegressionExecution {
        public static readonly string CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";
    
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType("TestXMLSchemaType", GetConfigTestType(null, true));
        }
    
        internal static ConfigurationEventTypeXMLDOM GetConfigTestType(string additionalXPathProperty, bool isUseXPathPropertyExpression) {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            eventTypeMeta.SchemaResource = SupportContainer.Instance.ResourceManager().ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            eventTypeMeta.AddNamespacePrefix("ss", "samples:schemas:simpleSchema");
            eventTypeMeta.AddXPathProperty("customProp", "count(/ss:simpleEvent/ss:nested3/ss:nested4)", XPathResultType.Number);
            eventTypeMeta.IsXPathPropertyExpr = isUseXPathPropertyExpression;
            if (additionalXPathProperty != null) {
                eventTypeMeta.AddXPathProperty(additionalXPathProperty, "count(/ss:simpleEvent/ss:nested3/ss:nested4)", XPathResultType.Number);
            }
            return eventTypeMeta;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertion(epService, true);
        }
    
        internal static void RunAssertion(EPServiceProvider epService, bool xpath) {
    
            var updateListener = new SupportUpdateListener();
    
            string stmtSelectWild = "select * from TestXMLSchemaType";
            EPStatement wildStmt = epService.EPAdministrator.CreateEPL(stmtSelectWild);
            EventType type = wildStmt.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(type);
    
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, !xpath),
                    new EventPropertyDescriptor("prop4", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("nested3", typeof(XmlNode), null, false, false, false, false, !xpath),
                    new EventPropertyDescriptor("customProp", typeof(double?), null, false, false, false, false, false),
            }, type.PropertyDescriptors);
    
            string stmt =
                    "select nested1 as nodeProp," +
                            "prop4 as nested1Prop," +
                            "nested1.prop2 as nested2Prop," +
                            "nested3.nested4('a').prop5[1] as complexProp," +
                            "nested1.nested2.prop3[2] as indexedProp," +
                            "customProp," +
                            "prop4.attr2 as attrOneProp," +
                            "nested3.nested4[2].id as attrTwoProp" +
                            " from TestXMLSchemaType#length(100)";
    
            EPStatement selectStmt = epService.EPAdministrator.CreateEPL(stmt);
            selectStmt.Events += updateListener.Update;
            type = selectStmt.EventType;
            SupportEventTypeAssertionUtil.AssertConsistency(type);
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("nodeProp", typeof(XmlNode), null, false, false, false, false, !xpath),
                    new EventPropertyDescriptor("nested1Prop", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("nested2Prop", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("complexProp", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("indexedProp", typeof(int?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("customProp", typeof(double?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attrOneProp", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attrTwoProp", typeof(string), typeof(char), false, false, true, false, false),
            }, type.PropertyDescriptors);
    
            XmlDocument eventDoc = SupportXML.SendDefaultEvent(epService.EPRuntime, "test");
    
            Assert.IsNotNull(updateListener.LastNewData);
            EventBean theEvent = updateListener.LastNewData[0];
    
            Assert.AreSame(eventDoc.DocumentElement.ChildNodes.Item(0), theEvent.Get("nodeProp"));
            Assert.AreEqual("SAMPLE_V6", theEvent.Get("nested1Prop"));
            Assert.AreEqual(true, theEvent.Get("nested2Prop"));
            Assert.AreEqual("SAMPLE_V8", theEvent.Get("complexProp"));
            Assert.AreEqual(5, theEvent.Get("indexedProp"));
            Assert.AreEqual(3.0, theEvent.Get("customProp"));
            Assert.AreEqual(true, theEvent.Get("attrOneProp"));
            Assert.AreEqual("c", theEvent.Get("attrTwoProp"));
    
            // Comment-in for performance testing
            //          long start = PerformanceObserver.NanoTime;
            //          for (int i = 0; i < 1000; i++)
            //          {
            //          SendEvent("test");
            //          }
            //          long end = PerformanceObserver.NanoTime;
            //          double delta = (end - start) / 1000d / 1000d / 1000d;
            //          Log.Info(delta);
        }
    }
} // end of namespace
