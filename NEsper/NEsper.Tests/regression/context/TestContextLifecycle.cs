///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.virtualdw;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextLifecycle
    {
        private EPServiceProvider _epService;
        private EPServiceProviderSPI _spi;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddPlugInVirtualDataWindow("test", "vdw", typeof(SupportVirtualDWFactory).FullName, SupportVirtualDW.ITERATE);    // configure with iteration
    
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            _spi = (EPServiceProviderSPI) _epService;
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestSplitStream()
        {
            SupportUpdateListener listener = new SupportUpdateListener();
            String eplOne = "create context CtxSegmentedByTarget partition by TheString from SupportBean;" +
               "@Name('out') context CtxSegmentedByTarget on SupportBean insert into NewSupportBean select * where IntPrimitive = 100;";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplOne);
    
            _epService.EPAdministrator.CreateEPL("select * from NewSupportBean").Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            _epService.EPAdministrator.DestroyAllStatements();
            listener.Reset();
    
            // test with subquery
            String[] fields = "mymax".Split(',');
            String eplTwo = "create context CtxSegmentedByTarget partition by TheString from SupportBean;" +
                    "context CtxSegmentedByTarget create window NewEvent.std:unique(TheString) as SupportBean;" +
                    "@Name('out') context CtxSegmentedByTarget on SupportBean " +
                    "insert into NewEvent select * where IntPrimitive = 100 " +
                    "insert into NewEventTwo select (select max(IntPrimitive) from NewEvent) as mymax  " +
                    "output all;";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplTwo);
    
            _epService.EPAdministrator.CreateEPL("select * from NewEventTwo").Events += listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new Object[] {100});
        }
    
        [Test]
        public void TestVirtualDataWindow()
        {
            SupportVirtualDWFactory.Windows.Clear();
            SupportVirtualDWFactory.IsDestroyed = false;
            
            _epService.EPAdministrator.CreateEPL("create context CtxSegmented as partition by TheString from SupportBean");
            _epService.EPAdministrator.CreateEPL("context CtxSegmented create window TestVDWWindow.test:vdw() as SupportBean");
            _epService.EPAdministrator.CreateEPL("select * from TestVDWWindow");
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(2, SupportVirtualDWFactory.Windows.Count);   // Independent windows for independent contexts
    
            _epService.EPAdministrator.DestroyAllStatements();
            foreach (SupportVirtualDW vdw in SupportVirtualDWFactory.Windows) {
                Assert.IsTrue(vdw.IsDestroyed);
            }
            Assert.IsTrue(SupportVirtualDWFactory.IsDestroyed);
        }
    
        [Test]
        public void TestNWOtherContextOnExpr()
        {
            _epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
            _epService.EPAdministrator.CreateEPL("create context TenToFive as start (0, 10, *, *, *) end (0, 17, *, *, *)");
    
            // Trigger not in context
            EPStatement stmtNamedWindow = _epService.EPAdministrator.CreateEPL("context NineToFive create window MyWindow.win:keepall() as SupportBean");
            try {
                _epService.EPAdministrator.CreateEPL("on SupportBean_S0 s0 merge MyWindow mw when matched then Update set IntPrimitive = 1");
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Cannot create on-trigger expression: Named window 'MyWindow' was declared with context 'NineToFive', please declare the same context name [on SupportBean_S0 s0 merge MyWindow mw when matched then Update set IntPrimitive = 1]", ex.Message);
            }
    
            // Trigger in different context
            try {
                _epService.EPAdministrator.CreateEPL("context TenToFive on SupportBean_S0 s0 merge MyWindow mw when matched then Update set IntPrimitive = 1");
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Cannot create on-trigger expression: Named window 'MyWindow' was declared with context 'NineToFive', please use the same context instead [context TenToFive on SupportBean_S0 s0 merge MyWindow mw when matched then Update set IntPrimitive = 1]", ex.Message);
            }
    
            // Named window not in context, trigger in different context
            stmtNamedWindow.Dispose();
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
            try {
                _epService.EPAdministrator.CreateEPL("context TenToFive on SupportBean_S0 s0 merge MyWindow mw when matched then Update set IntPrimitive = 1");
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Cannot create on-trigger expression: Named window 'MyWindow' was declared with context 'null', please use the same context instead [context TenToFive on SupportBean_S0 s0 merge MyWindow mw when matched then Update set IntPrimitive = 1]", ex.Message);
            }
        }
    
        [Test]
        public void TestLifecycle()
        {
    
            String epl = "@Name('context') create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)";
            ContextManagementService ctxMgmtService = _spi.ContextManagementService;
            SchedulingService schedulingService = _spi.SchedulingService;
            Assert.AreEqual(0, ctxMgmtService.ContextCount);
            Assert.AreEqual(0, schedulingService.ScheduleHandleCount);
    
            // create and destroy
            EPStatement stmtContext = _epService.EPAdministrator.CreateEPL(epl);
            Assert.AreEqual(1, ctxMgmtService.ContextCount);
            Assert.AreEqual(0, schedulingService.ScheduleHandleCount);
    
            stmtContext.Dispose();
            Assert.AreEqual(0, ctxMgmtService.ContextCount);
    
            // create context, create statement, destroy statement, destroy context
            stmtContext = _epService.EPAdministrator.CreateEPL(epl);
            Assert.AreEqual(1, ctxMgmtService.ContextCount);
    
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("@Name('C') context NineToFive select * from SupportBean");
            Assert.AreEqual(1, schedulingService.ScheduleHandleCount);
    
            stmt.Dispose();
            Assert.AreEqual(0, schedulingService.ScheduleHandleCount);
    
            stmtContext.Dispose();
            Assert.AreEqual(0, ctxMgmtService.ContextCount);
    
            // create same context
            _epService.EPAdministrator.CreateEPL(epl);
            _epService.EPAdministrator.CreateEPL("@Name('C') context NineToFive select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('D') context NineToFive select * from SupportBean");
            _epService.EPAdministrator.DestroyAllStatements();
            Assert.AreEqual(0, ctxMgmtService.ContextCount);
            Assert.AreEqual(0, schedulingService.ScheduleHandleCount);
        }
    
        [Test]
        public void TestInvalid()
        {
            // same context twice
            String eplCreateCtx = "create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)";
            EPStatement stmtContext = _epService.EPAdministrator.CreateEPL(eplCreateCtx);
            TryInvalid(eplCreateCtx, "Error starting statement: Context by name 'NineToFive' already exists [");
    
            // still in use
            _epService.EPAdministrator.CreateEPL("context NineToFive select * from SupportBean");
            stmtContext.Dispose();
            TryInvalid(eplCreateCtx, "Error starting statement: Context by name 'NineToFive' is still referenced by statements and may not be changed");
    
            // not found
            TryInvalid("context EightToSix select * from SupportBean", "Error starting statement: Context by name 'EightToSix' has not been declared [");
    
            // test Update: Update is not allowed as it is processed out-of-context by runtime
            _epService.EPAdministrator.CreateEPL("insert into ABCStream select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByAString partition by TheString from SupportBean");
            try {
                _epService.EPAdministrator.CreateEPL("context SegmentedByAString Update istream ABCStream set IntPrimitive = (select id from SupportBean_S0.std:lastevent()) where IntPrimitive < 0");
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Update IStream is not supported in conjunction with a context [context SegmentedByAString Update istream ABCStream set IntPrimitive = (select id from SupportBean_S0.std:lastevent()) where IntPrimitive < 0]", ex.Message);
            }

            // context declaration for create-context
            _epService.EPAdministrator.CreateEPL("create context ABC start @now end after 5 seconds");
            TryInvalid("context ABC create context DEF start @now end after 5 seconds",
                    "Error starting statement: A create-context statement cannot itself be associated to a context, please declare a nested context instead [context ABC create context DEF start @now end after 5 seconds]");
        }
    
        private void TryInvalid(String epl, String expected)
        {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                if (!ex.Message.StartsWith(expected)) {
                    throw new Exception("Expected/Received:\n" + expected + "\n" + ex.Message + "\n");
                }
                Assert.IsTrue(expected.Trim().Length != 0);
            }
        }
    }
}
