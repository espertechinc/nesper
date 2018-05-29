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
    public class ExecEventXMLSchemaEventTransposeNodeArray : RegressionExecution {
        private const string CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";

        public override void Run(EPServiceProvider epService) {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            string schemaUri = SupportContainer.Instance.ResourceManager().ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            eventTypeMeta.SchemaResource = schemaUri;
            epService.EPAdministrator.Configuration.AddEventType("TestXMLSchemaType", eventTypeMeta);
    
            // try array property insert
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("select nested3.nested4 as narr from TestXMLSchemaType#lastevent");
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("narr", typeof(XmlNode[]), typeof(XmlNode), false, false, true, false, true),
            }, stmtInsert.EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
    
            SupportXML.SendDefaultEvent(epService.EPRuntime, "test");
    
            EventBean result = stmtInsert.First();
            SupportEventTypeAssertionUtil.AssertConsistency(result);
            EventBean[] fragments = (EventBean[]) result.GetFragment("narr");
            Assert.AreEqual(3, fragments.Length);
            Assert.AreEqual("SAMPLE_V8", fragments[0].Get("prop5[1]"));
            Assert.AreEqual("SAMPLE_V11", fragments[2].Get("prop5[1]"));
    
            EventBean fragmentItem = (EventBean) result.GetFragment("narr[2]");
            Assert.AreEqual("TestXMLSchemaType.nested3.nested4", fragmentItem.EventType.Name);
            Assert.AreEqual("SAMPLE_V10", fragmentItem.Get("prop5[0]"));
    
            // try array index property insert
            EPStatement stmtInsertItem = epService.EPAdministrator.CreateEPL("select nested3.nested4[1] as narr from TestXMLSchemaType#lastevent");
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("narr", typeof(XmlNode), null, false, false, false, false, true),
            }, stmtInsertItem.EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsertItem.EventType);
    
            SupportXML.SendDefaultEvent(epService.EPRuntime, "test");
    
            EventBean resultItem = stmtInsertItem.First();
            Assert.AreEqual("b", resultItem.Get("narr.id"));
            SupportEventTypeAssertionUtil.AssertConsistency(resultItem);
            EventBean fragmentsInsertItem = (EventBean) resultItem.GetFragment("narr");
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentsInsertItem);
            Assert.AreEqual("b", fragmentsInsertItem.Get("id"));
            Assert.AreEqual("SAMPLE_V9", fragmentsInsertItem.Get("prop5[0]"));
        }
    }
} // end of namespace
