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
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using static com.espertech.esper.regression.events.xml.ExecEventXMLSchemaXPathBacked;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.xml
{
    public class ExecEventXMLSchemaAddRemoveType : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("TestXMLSchemaType", GetConfigTestType(null, false));
        }
    
        public override void Run(EPServiceProvider epService) {
            ConfigurationOperations configOps = epService.EPAdministrator.Configuration;
    
            // test remove type with statement used (no force)
            configOps.AddEventType("MyXMLEvent", GetConfigTestType("p01", false));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select p01 from MyXMLEvent", "stmtOne");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyXMLEvent").ToArray(), new string[]{"stmtOne"});
    
            try {
                configOps.RemoveEventType("MyXMLEvent", false);
            } catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("MyXMLEvent"));
            }
    
            // destroy statement and type
            stmt.Dispose();
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyXMLEvent").IsEmpty());
            Assert.IsTrue(configOps.IsEventTypeExists("MyXMLEvent"));
            Assert.IsTrue(configOps.RemoveEventType("MyXMLEvent", false));
            Assert.IsFalse(configOps.RemoveEventType("MyXMLEvent", false));    // try double-remove
            Assert.IsFalse(configOps.IsEventTypeExists("MyXMLEvent"));
            try {
                epService.EPAdministrator.CreateEPL("select p01 from MyXMLEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            // add back the type
            configOps.AddEventType("MyXMLEvent", GetConfigTestType("p20", false));
            Assert.IsTrue(configOps.IsEventTypeExists("MyXMLEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyXMLEvent").IsEmpty());
    
            // compile
            epService.EPAdministrator.CreateEPL("select p20 from MyXMLEvent", "stmtTwo");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyXMLEvent").ToArray(), new string[]{"stmtTwo"});
            try {
                epService.EPAdministrator.CreateEPL("select p01 from MyXMLEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            // remove with force
            try {
                configOps.RemoveEventType("MyXMLEvent", false);
            } catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("MyXMLEvent"));
            }
            Assert.IsTrue(configOps.RemoveEventType("MyXMLEvent", true));
            Assert.IsFalse(configOps.IsEventTypeExists("MyXMLEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyXMLEvent").IsEmpty());
    
            // add back the type
            configOps.AddEventType("MyXMLEvent", GetConfigTestType("p03", false));
            Assert.IsTrue(configOps.IsEventTypeExists("MyXMLEvent"));
    
            // compile
            epService.EPAdministrator.CreateEPL("select p03 from MyXMLEvent");
            try {
                epService.EPAdministrator.CreateEPL("select p20 from MyXMLEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
        }
    }
} // end of namespace
