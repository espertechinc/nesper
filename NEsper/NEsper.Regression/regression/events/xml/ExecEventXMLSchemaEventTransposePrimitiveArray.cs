///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;
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
    public class ExecEventXMLSchemaEventTransposePrimitiveArray : RegressionExecution {
        private const string CLASSLOADER_SCHEMA_URI = "regression/simpleSchema.xsd";
    
        public override void Run(EPServiceProvider epService) {
            var schemaURI = SupportContainer.Instance.ResourceManager().ResolveResourceURL(CLASSLOADER_SCHEMA_URI).ToString();
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            eventTypeMeta.SchemaResource = schemaURI;
            epService.EPAdministrator.Configuration.AddEventType("ABCType", eventTypeMeta);
    
            eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "//nested2";
            eventTypeMeta.SchemaResource = schemaURI;
            eventTypeMeta.IsEventSenderValidatesRoot = false;
            epService.EPAdministrator.Configuration.AddEventType("TestNested2", eventTypeMeta);
    
            // try array property in select
            var stmtInsert = epService.EPAdministrator.CreateEPL("select * from TestNested2#lastevent");
            var listener = new SupportUpdateListener();
            stmtInsert.Events += listener.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(new []{
                    new EventPropertyDescriptor("prop3", typeof(int?[]), typeof(int?), false, false, true, false, false),
            }, stmtInsert.EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
    
            SupportXML.SendDefaultEvent(epService.EPRuntime, "test");
            Assert.IsFalse(listener.IsInvoked);
    
            var sender = epService.EPRuntime.GetEventSender("TestNested2");
            sender.SendEvent(SupportXML.GetDocument("<nested2><prop3>2</prop3><prop3></prop3><prop3>4</prop3></nested2>"));
            var theEvent = stmtInsert.First();
            EPAssertionUtil.AssertEqualsExactOrder((int?[]) theEvent.Get("prop3"), new int?[]{2, null, 4});
            SupportEventTypeAssertionUtil.AssertConsistency(theEvent);
    
            // try array property nested
            var stmtSelect = epService.EPAdministrator.CreateEPL("select nested3.* from ABCType#lastevent");
            SupportXML.SendDefaultEvent(epService.EPRuntime, "test");
            var stmtSelectResult = stmtSelect.First();
            SupportEventTypeAssertionUtil.AssertConsistency(stmtSelectResult);
            Assert.AreEqual(typeof(string[]), stmtSelectResult.EventType.GetPropertyType("nested4[2].prop5"));
            Assert.AreEqual("SAMPLE_V8", stmtSelectResult.Get("nested4[0].prop5[1]"));
            EPAssertionUtil.AssertEqualsExactOrder((string[]) stmtSelectResult.Get("nested4[2].prop5"), new object[]{"SAMPLE_V10", "SAMPLE_V11"});
    
            var fragmentNested4 = (EventBean) stmtSelectResult.GetFragment("nested4[2]");
            EPAssertionUtil.AssertEqualsExactOrder((string[]) fragmentNested4.Get("prop5"), new object[]{"SAMPLE_V10", "SAMPLE_V11"});
            Assert.AreEqual("SAMPLE_V11", fragmentNested4.Get("prop5[1]"));
            SupportEventTypeAssertionUtil.AssertConsistency(fragmentNested4);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    }
} // end of namespace
