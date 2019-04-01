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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.events.bean
{
    public class ExecEventBeanAddRemoveType : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            ConfigurationOperations configOps = epService.EPAdministrator.Configuration;
    
            // test remove type with statement used (no force)
            configOps.AddEventType("MyBeanEvent", typeof(SupportBean_A));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select id from MyBeanEvent", "stmtOne");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyBeanEvent").ToArray(), new string[]{"stmtOne"});
    
            try {
                configOps.RemoveEventType("MyBeanEvent", false);
            } catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("MyBeanEvent"));
            }
    
            // destroy statement and type
            stmt.Dispose();
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyBeanEvent").IsEmpty());
            Assert.IsTrue(configOps.IsEventTypeExists("MyBeanEvent"));
            Assert.IsTrue(configOps.RemoveEventType("MyBeanEvent", false));
            Assert.IsFalse(configOps.RemoveEventType("MyBeanEvent", false));    // try double-remove
            Assert.IsFalse(configOps.IsEventTypeExists("MyBeanEvent"));
            try {
                epService.EPAdministrator.CreateEPL("select id from MyBeanEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            // add back the type
            configOps.AddEventType("MyBeanEvent", typeof(SupportBean));
            Assert.IsTrue(configOps.IsEventTypeExists("MyBeanEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyBeanEvent").IsEmpty());
    
            // compile
            epService.EPAdministrator.CreateEPL("select BoolPrimitive from MyBeanEvent", "stmtTwo");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyBeanEvent").ToArray(), new string[]{"stmtTwo"});
            try {
                epService.EPAdministrator.CreateEPL("select id from MyBeanEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            // remove with force
            try {
                configOps.RemoveEventType("MyBeanEvent", false);
            } catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("MyBeanEvent"));
            }
            Assert.IsTrue(configOps.RemoveEventType("MyBeanEvent", true));
            Assert.IsFalse(configOps.IsEventTypeExists("MyBeanEvent"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyBeanEvent").IsEmpty());
    
            // add back the type
            configOps.AddEventType("MyBeanEvent", typeof(SupportMarketDataBean));
            Assert.IsTrue(configOps.IsEventTypeExists("MyBeanEvent"));
    
            // compile
            epService.EPAdministrator.CreateEPL("select feed from MyBeanEvent");
            try {
                epService.EPAdministrator.CreateEPL("select BoolPrimitive from MyBeanEvent");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
        }
    }
} // end of namespace
