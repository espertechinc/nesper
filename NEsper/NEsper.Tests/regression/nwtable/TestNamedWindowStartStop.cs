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
using com.espertech.esper.core.service;
using com.espertech.esper.epl.named;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowStartStop 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerWindow;
        private SupportUpdateListener _listenerSelect;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _listenerWindow = new SupportUpdateListener();
            _listenerSelect = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listenerWindow = null;
            _listenerSelect = null;
        }
    
        [Test]
        public void TestAddRemoveType()
        {
            ConfigurationOperations configOps = _epService.EPAdministrator.Configuration;
    
            // test remove type with statement used (no force)
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("create window MyWindowEventType.win:keepall() (a int, b string)", "stmtOne");
            EPAssertionUtil.AssertEqualsExactOrder(configOps.GetEventTypeNameUsedBy("MyWindowEventType").ToArray(), new string[]{"stmtOne"});
    
            try {
                configOps.RemoveEventType("MyWindowEventType", false);
            }
            catch (ConfigurationException ex) {
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
                _epService.EPAdministrator.CreateEPL("select a from MyWindowEventType");
                Assert.Fail();
            }
            catch (EPException ex) {
                // expected
            }
    
            // add back the type
            stmt = _epService.EPAdministrator.CreateEPL("create window MyWindowEventType.win:keepall() (c int, d string)", "stmtOne");
            Assert.IsTrue(configOps.IsEventTypeExists("MyWindowEventType"));
            Assert.IsFalse(configOps.GetEventTypeNameUsedBy("MyWindowEventType").IsEmpty());
    
            // compile
            _epService.EPAdministrator.CreateEPL("select d from MyWindowEventType", "stmtTwo");
            object[] usedBy = configOps.GetEventTypeNameUsedBy("MyWindowEventType").ToArray();
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"stmtOne", "stmtTwo"}, usedBy);
            try {
                _epService.EPAdministrator.CreateEPL("select a from MyWindowEventType");
                Assert.Fail();
            }
            catch (EPException ex) {
                // expected
            }
    
            // remove with force
            try {
                configOps.RemoveEventType("MyWindowEventType", false);
            }
            catch (ConfigurationException ex) {
                Assert.IsTrue(ex.Message.Contains("MyWindowEventType"));
            }
            Assert.IsTrue(configOps.RemoveEventType("MyWindowEventType", true));
            Assert.IsFalse(configOps.IsEventTypeExists("MyWindowEventType"));
            Assert.IsTrue(configOps.GetEventTypeNameUsedBy("MyWindowEventType").IsEmpty());
    
            // add back the type
            stmt.Dispose();
            stmt = _epService.EPAdministrator.CreateEPL("create window MyWindowEventType.win:keepall() (f int)", "stmtOne");
            Assert.IsTrue(configOps.IsEventTypeExists("MyWindowEventType"));
    
            // compile
            _epService.EPAdministrator.CreateEPL("select f from MyWindowEventType");
            try {
                _epService.EPAdministrator.CreateEPL("select c from MyWindowEventType");
                Assert.Fail();
            }
            catch (EPException ex) {
                // expected
            }
        }
    
        [Test]
        public void TestStartStopCreator()
        {
            // create window
            string stmtTextCreate = "create window MyWindow.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate, "stmtCreateFirst");
            stmtCreate.AddListener(_listenerWindow);
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " delete from MyWindow";
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete, "stmtDelete");
    
            // create insert into
            string stmtTextInsertOne = "insert into MyWindow select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL(stmtTextInsertOne, "stmtInsert");
    
            // create consumer
            string[] fields = new string[] {"a", "b"};
            string stmtTextSelect = "select a, b from MyWindow as s1";
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect, "stmtSelect");
            stmtSelect.AddListener(_listenerSelect);
    
            // send 1 event
            SendSupportBean("E1", 1);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
    
            // stop creator
            stmtCreate.Stop();
            SendSupportBean("E2", 2);
            Assert.IsFalse(_listenerSelect.IsInvoked);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.That(stmtCreate.GetEnumerator(), Is.Not.Null);
            Assert.That(stmtCreate.GetEnumerator().MoveNext(), Is.False);
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
    
            // start creator
            stmtCreate.Start();
            SendSupportBean("E3", 3);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
            Assert.IsFalse(_listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E3", 3 } });
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E3", 3 } });
    
            // stop and start consumer: should pick up last event
            stmtSelect.Stop();
            stmtSelect.Start();
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E3", 3 } });
    
            SendSupportBean("E4", 4);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4});
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E3", 3 }, new object[] { "E4", 4 } });
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E3", 3 }, new object[] { "E4", 4 } });
    
            // destroy creator
            stmtCreate.Dispose();
            SendSupportBean("E5", 5);
            Assert.IsFalse(_listenerSelect.IsInvoked);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.That(stmtCreate.GetEnumerator(), Is.Not.Null);
            Assert.That(stmtCreate.GetEnumerator().MoveNext(), Is.False);
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E3", 3 }, new object[] { "E4", 4 } });
    
            // create window anew
            stmtTextCreate = "create window MyWindow.win:keepall() as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate, "stmtCreate");
            stmtCreate.AddListener(_listenerWindow);
    
            SendSupportBean("E6", 6);
            EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E6", 6});
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E6", 6 } });
            Assert.IsFalse(_listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E3", 3 }, new object[] { "E4", 4 } });
    
            // create select stmt
            string stmtTextOnSelect = "on " + typeof(SupportBean_A).FullName + " insert into A select * from MyWindow";
            EPStatement stmtOnSelect = _epService.EPAdministrator.CreateEPL(stmtTextOnSelect, "stmtOnSelect");
    
            // assert statement-type reference
            EPServiceProviderSPI spi = (EPServiceProviderSPI) _epService;
    
            Assert.IsTrue(spi.StatementEventTypeRef.IsInUse("MyWindow"));
            ICollection<string> stmtNames = spi.StatementEventTypeRef.GetStatementNamesForType("MyWindow");
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
    
        private SupportBean SendSupportBean(string theString, int intPrimitive)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
}
