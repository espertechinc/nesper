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
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util.support;


using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLNoSchemaEventTransposeDOM : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsIterableUnbound = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            var eventTypeMeta = new ConfigurationEventTypeXMLDOM();
            eventTypeMeta.RootElementName = "simpleEvent";
            // eventTypeMeta.IsXPathPropertyExpr = false; <== the default
            epService.EPAdministrator.Configuration.AddEventType("TestXMLSchemaType", eventTypeMeta);
    
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL("insert into MyNestedStream select nested1 from TestXMLSchemaType");
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[]{
                    new EventPropertyDescriptor("nested1", typeof(string), typeof(char), false, false, true, false, false),
            }, stmtInsert.EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsert.EventType);
    
            EPStatement stmtSelectWildcard = epService.EPAdministrator.CreateEPL("select * from TestXMLSchemaType");
            EPAssertionUtil.AssertEqualsAnyOrder(new EventPropertyDescriptor[0], stmtSelectWildcard.EventType.PropertyDescriptors);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtSelectWildcard.EventType);
    
            SupportXML.SendDefaultEvent(epService.EPRuntime, "test");
            EventBean stmtInsertWildcardBean = stmtInsert.First();
            EventBean stmtSelectWildcardBean = stmtSelectWildcard.First();
            Assert.IsNotNull(stmtInsertWildcardBean.Get("nested1"));
            SupportEventTypeAssertionUtil.AssertConsistency(stmtSelectWildcardBean);
            SupportEventTypeAssertionUtil.AssertConsistency(stmtInsert.First());
    
            Assert.AreEqual(0, stmtSelectWildcardBean.EventType.PropertyNames.Length);
        }
    }
} // end of namespace
