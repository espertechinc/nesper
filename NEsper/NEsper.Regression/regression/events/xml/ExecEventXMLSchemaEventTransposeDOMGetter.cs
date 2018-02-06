///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
using System.Xml;

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
    public class ExecEventXMLSchemaEventTransposeDOMGetter : RegressionExecution {
        private const string CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";
    
        public override void Run(EPServiceProvider epService) {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            string schemaUri = SupportContainer.Instance.ResourceManager().ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            eventTypeMeta.SchemaResource = schemaUri;
            // eventTypeMeta.IsXPathPropertyExpr = false; <== the default
            epService.EPAdministrator.Configuration.AddEventType("TestXMLSchemaType", eventTypeMeta);
    
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("insert into MyNestedStream select nested1 from TestXMLSchemaType#lastevent");
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, true),
            }, stmtInsert.EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
    
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("select nested1.attr1 as attr1, nested1.prop1 as prop1, nested1.prop2 as prop2, nested1.nested2.prop3 as prop3, nested1.nested2.prop3[0] as prop3_0, nested1.nested2 as nested2 from MyNestedStream#lastevent");
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("prop1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("prop2", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attr1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("prop3", typeof(int?[]), typeof(int?), false, false, true, false, false),
                    new EventPropertyDescriptor("prop3_0", typeof(int?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("nested2", typeof(XmlNode), null, false, false, false, false, true),
            }, stmtSelect.EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtSelect.EventType);
    
            EPStatement stmtSelectWildcard = epService.EPAdministrator.CreateEPL("select * from MyNestedStream");
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("nested1", typeof(XmlNode), null, false, false, false, false, true),
            }, stmtSelectWildcard.EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtSelectWildcard.EventType);
    
            EPStatement stmtInsertWildcard = epService.EPAdministrator.CreateEPL("insert into MyNestedStreamTwo select nested1.* from TestXMLSchemaType#lastevent");
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("prop1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("prop2", typeof(bool?), null, false, false, false, false, false),
                    new EventPropertyDescriptor("attr1", typeof(string), typeof(char), false, false, true, false, false),
                    new EventPropertyDescriptor("nested2", typeof(XmlNode), null, false, false, false, false, true),
            }, stmtInsertWildcard.EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsertWildcard.EventType);
    
            SupportXML.SendDefaultEvent(epService.EPRuntime, "test");
            EventBean stmtInsertWildcardBean = stmtInsertWildcard.First();
            EPAssertionUtil.AssertProps(stmtInsertWildcardBean, "prop1,prop2,attr1".Split(','),
                    new object[]{"SAMPLE_V1", true, "SAMPLE_ATTR1"});
    
            SupportEventTypeAssertionUtil.AssertConsistency(stmtSelect.First());
            EventBean stmtInsertBean = stmtInsert.First();
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsertWildcard.First());
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsert.First());
    
            EventBean fragmentNested1 = (EventBean) stmtInsertBean.GetFragment("nested1");
            Assert.AreEqual(5, fragmentNested1.Get("nested2.prop3[2]"));
            Assert.AreEqual("TestXMLSchemaType.nested1", fragmentNested1.EventType.Name);
    
            EventBean fragmentNested2 = (EventBean) stmtInsertWildcardBean.GetFragment("nested2");
            Assert.AreEqual(4, fragmentNested2.Get("prop3[1]"));
            Assert.AreEqual("TestXMLSchemaType.nested1.nested2", fragmentNested2.EventType.Name);
        }
    }
} // end of namespace
