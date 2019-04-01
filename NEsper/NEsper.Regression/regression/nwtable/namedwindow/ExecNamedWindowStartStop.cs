///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowStartStop : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionAddRemoveType(epService);
            RunAssertionStartStopCreator(epService);
        }
    
        private void RunAssertionAddRemoveType(EPServiceProvider epService) {
            ConfigurationOperations configOps = epService.EPAdministrator.Configuration;
    
            // test remove type with statement used (no force)
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create window MyWindowEventType#keepall (a int, b string)", "stmtOne");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyWindowEventType").ToArray(), new string[]{"stmtOne"});
    
            try {
                configOps.RemoveEventType("MyWindowEventType", false);
            } catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("MyWindowEventType"));
            }
    
            // destroy statement and type
            stmt.Dispose();
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyWindowEventType").IsEmpty());
            Assert.IsTrue(configOps.IsEventTypeExists("MyWindowEventType"));
            Assert.IsTrue(configOps.RemoveEventType("MyWindowEventType", false));
            Assert.IsFalse(configOps.RemoveEventType("MyWindowEventType", false));    // try double-remove
            Assert.IsFalse(configOps.IsEventTypeExists("MyWindowEventType"));
            try {
                epService.EPAdministrator.CreateEPL("select a from MyWindowEventType");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            // add back the type
            stmt = epService.EPAdministrator.CreateEPL("create window MyWindowEventType#keepall (c int, d string)", "stmtOne");
            Assert.IsTrue(configOps.IsEventTypeExists("MyWindowEventType"));
            Assert.IsFalse(configOps.GetEventTypeNameUsedBy("MyWindowEventType").IsEmpty());
    
            // compile
            epService.EPAdministrator.CreateEPL("select d from MyWindowEventType", "stmtTwo");
            object[] usedBy = configOps.GetEventTypeNameUsedBy("MyWindowEventType").ToArray();
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"stmtOne", "stmtTwo"}, usedBy);
            try {
                epService.EPAdministrator.CreateEPL("select a from MyWindowEventType");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
    
            // remove with force
            try {
                configOps.RemoveEventType("MyWindowEventType", false);
            } catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("MyWindowEventType"));
            }
            Assert.IsTrue(configOps.RemoveEventType("MyWindowEventType", true));
            Assert.IsFalse(configOps.IsEventTypeExists("MyWindowEventType"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyWindowEventType").IsEmpty());
    
            // add back the type
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("create window MyWindowEventType#keepall (f int)", "stmtOne");
            Assert.IsTrue(configOps.IsEventTypeExists("MyWindowEventType"));
    
            // compile
            epService.EPAdministrator.CreateEPL("select f from MyWindowEventType");
            try {
                epService.EPAdministrator.CreateEPL("select c from MyWindowEventType");
                Assert.Fail();
            } catch (EPException) {
                // expected
            }
        }
    
        private void RunAssertionStartStopCreator(EPServiceProvider epService) {
            // create window
            string stmtTextCreate = "create window MyWindow#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate, "stmtCreateFirst");
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " delete from MyWindow";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete, "stmtDelete");
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL(stmtTextInsertOne, "stmtInsert");
    
            // create consumer
            var fields = new string[]{"a", "b"};
            string stmtTextSelect = "select a, b from MyWindow as s1";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect, "stmtSelect");
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;
    
            // send 1 event
            SendSupportBean(epService, "E1", 1);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            // stop creator
            stmtCreate.Stop();
            SendSupportBean(epService, "E2", 2);
            Assert.IsFalse(listenerSelect.IsInvoked);
            Assert.IsFalse(listenerWindow.IsInvoked);
            //Assert.IsNull(stmtCreate.GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            // start creator
            stmtCreate.Start();
            SendSupportBean(epService, "E3", 3);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
            Assert.IsFalse(listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E3", 3}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[] {"E3", 3}});
    
            // stop and start consumer: should pick up last event
            stmtSelect.Stop();
            stmtSelect.Start();
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[] {"E3", 3}});
    
            SendSupportBean(epService, "E4", 4);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E3", 3}, new object[] {"E4", 4}});
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[] {"E3", 3}, new object[] {"E4", 4}});
    
            // destroy creator
            stmtCreate.Dispose();
            SendSupportBean(epService, "E5", 5);
            Assert.IsFalse(listenerSelect.IsInvoked);
            Assert.IsFalse(listenerWindow.IsInvoked);
            //Assert.IsNull(stmtCreate.GetEnumerator());
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[] {"E3", 3}, new object[] {"E4", 4}});
    
            // create window anew
            stmtTextCreate = "create window MyWindow#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate, "stmtCreate");
            stmtCreate.Events += listenerWindow.Update;
    
            SendSupportBean(epService, "E6", 6);
            EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E6", 6});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E6", 6}});
            Assert.IsFalse(listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[] {"E3", 3}, new object[] {"E4", 4}});
    
            // create select stmt
            string stmtTextOnSelect = "on " + typeof(SupportBean_A).FullName + " insert into A select * from MyWindow";
            EPStatement stmtOnSelect = epService.EPAdministrator.CreateEPL(stmtTextOnSelect, "stmtOnSelect");
    
            // assert statement-type reference
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
    
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse("MyWindow"));
            var stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType("MyWindow");
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"stmtCreate", "stmtSelect", "stmtInsert", "stmtDelete", "stmtOnSelect"}, stmtNames.ToArray());
    
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"stmtCreate", "stmtInsert"}, stmtNames.ToArray());
    
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean_A).FullName));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean_A).FullName);
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"stmtDelete", "stmtOnSelect"}, stmtNames.ToArray());
    
            stmtInsert.Dispose();
            stmtDelete.Dispose();
    
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse("MyWindow"));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType("MyWindow");
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"stmtCreate", "stmtSelect", "stmtOnSelect"}, stmtNames.ToArray());
    
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean).FullName);
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"stmtCreate"}, stmtNames.ToArray());
    
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean_A).FullName));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean_A).FullName);
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"stmtOnSelect"}, stmtNames.ToArray());
    
            stmtCreate.Dispose();
    
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse("MyWindow"));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType("MyWindow");
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"stmtSelect", "stmtOnSelect"}, stmtNames.ToArray());
    
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
    
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean_A).FullName));
            stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType(typeof(SupportBean_A).FullName);
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"stmtOnSelect"}, stmtNames.ToArray());
    
            stmtOnSelect.Dispose();
            stmtSelect.Dispose();
    
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse("MyWindow"));
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean).FullName));
            Assert.IsFalse(spi.StatementEventTypeRef.IsInUse(typeof(SupportBean_A).FullName));
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
