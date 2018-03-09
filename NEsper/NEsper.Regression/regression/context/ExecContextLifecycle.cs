///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.schedule;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.virtualdw;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextLifecycle : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddPlugInVirtualDataWindow("test", "vdw", typeof(SupportVirtualDWFactory), SupportVirtualDW.ITERATE);    // configure with iteration
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionSplitStream(epService);
            RunAssertionVirtualDataWindow(epService);
            RunAssertionNWOtherContextOnExpr(epService);
            RunAssertionLifecycle(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionSplitStream(EPServiceProvider epService) {
            string eplOne = "create context CtxSegmentedByTarget partition by TheString from SupportBean;" +
                    "@Name('out') context CtxSegmentedByTarget on SupportBean insert into NewSupportBean select * where IntPrimitive = 100;";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplOne);
    
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from NewSupportBean").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
            epService.EPAdministrator.DestroyAllStatements();
            listener.Reset();
    
            // test with subquery
            string[] fields = "mymax".Split(',');
            string eplTwo = "create context CtxSegmentedByTarget partition by TheString from SupportBean;" +
                    "context CtxSegmentedByTarget create window NewEvent#unique(TheString) as SupportBean;" +
                    "@Name('out') context CtxSegmentedByTarget on SupportBean " +
                    "insert into NewEvent select * where IntPrimitive = 100 " +
                    "insert into NewEventTwo select (select max(IntPrimitive) from NewEvent) as mymax  " +
                    "output all;";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(eplTwo);
    
            epService.EPAdministrator.CreateEPL("select * from NewEventTwo").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionVirtualDataWindow(EPServiceProvider epService) {
            SupportVirtualDWFactory.Windows.Clear();
            SupportVirtualDWFactory.IsDestroyed = false;
    
            epService.EPAdministrator.CreateEPL("create context CtxSegmented as partition by TheString from SupportBean");
            epService.EPAdministrator.CreateEPL("context CtxSegmented create window TestVDWWindow.test:vdw() as SupportBean");
            epService.EPAdministrator.CreateEPL("select * from TestVDWWindow");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(2, SupportVirtualDWFactory.Windows.Count);   // Independent windows for independent contexts
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (SupportVirtualDW vdw in SupportVirtualDWFactory.Windows) {
                Assert.IsTrue(vdw.IsDestroyed);
            }
            Assert.IsTrue(SupportVirtualDWFactory.IsDestroyed);
        }
    
        private void RunAssertionNWOtherContextOnExpr(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)");
            epService.EPAdministrator.CreateEPL("create context TenToFive as start (0, 10, *, *, *) end (0, 17, *, *, *)");
    
            // Trigger not in context
            EPStatement stmtNamedWindow = epService.EPAdministrator.CreateEPL("context NineToFive create window MyWindow#keepall as SupportBean");
            try {
                epService.EPAdministrator.CreateEPL("on SupportBean_S0 s0 merge MyWindow mw when matched then update set IntPrimitive = 1");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Cannot create on-trigger expression: Named window 'MyWindow' was declared with context 'NineToFive', please declare the same context name [on SupportBean_S0 s0 merge MyWindow mw when matched then update set IntPrimitive = 1]", ex.Message);
            }
    
            // Trigger in different context
            try {
                epService.EPAdministrator.CreateEPL("context TenToFive on SupportBean_S0 s0 merge MyWindow mw when matched then update set IntPrimitive = 1");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Cannot create on-trigger expression: Named window 'MyWindow' was declared with context 'NineToFive', please use the same context instead [context TenToFive on SupportBean_S0 s0 merge MyWindow mw when matched then update set IntPrimitive = 1]", ex.Message);
            }
    
            // Named window not in context, trigger in different context
            stmtNamedWindow.Dispose();
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            try {
                epService.EPAdministrator.CreateEPL("context TenToFive on SupportBean_S0 s0 merge MyWindow mw when matched then update set IntPrimitive = 1");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Cannot create on-trigger expression: Named window 'MyWindow' was declared with context 'null', please use the same context instead [context TenToFive on SupportBean_S0 s0 merge MyWindow mw when matched then update set IntPrimitive = 1]", ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionLifecycle(EPServiceProvider epService) {
    
            string epl = "@Name('context') create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)";
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            ContextManagementService ctxMgmtService = spi.ContextManagementService;
            SchedulingService schedulingService = spi.SchedulingService;
            Assert.AreEqual(0, ctxMgmtService.ContextCount);
            Assert.AreEqual(0, schedulingService.ScheduleHandleCount);
    
            // create and destroy
            EPStatement stmtContext = epService.EPAdministrator.CreateEPL(epl);
            Assert.AreEqual(1, ctxMgmtService.ContextCount);
            Assert.AreEqual(0, schedulingService.ScheduleHandleCount);
    
            stmtContext.Dispose();
            Assert.AreEqual(0, ctxMgmtService.ContextCount);
    
            // create context, create statement, destroy statement, destroy context
            stmtContext = epService.EPAdministrator.CreateEPL(epl);
            Assert.AreEqual(1, ctxMgmtService.ContextCount);
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('C') context NineToFive select * from SupportBean");
            Assert.AreEqual(1, schedulingService.ScheduleHandleCount);
    
            stmt.Dispose();
            Assert.AreEqual(0, schedulingService.ScheduleHandleCount);
    
            stmtContext.Dispose();
            Assert.AreEqual(0, ctxMgmtService.ContextCount);
    
            // create same context
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPAdministrator.CreateEPL("@Name('C') context NineToFive select * from SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('D') context NineToFive select * from SupportBean");
            epService.EPAdministrator.DestroyAllStatements();
            Assert.AreEqual(0, ctxMgmtService.ContextCount);
            Assert.AreEqual(0, schedulingService.ScheduleHandleCount);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            // same context twice
            string eplCreateCtx = "create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)";
            EPStatement stmtContext = epService.EPAdministrator.CreateEPL(eplCreateCtx);
            TryInvalid(epService, eplCreateCtx, "Error starting statement: Context by name 'NineToFive' already exists [");
    
            // still in use
            epService.EPAdministrator.CreateEPL("context NineToFive select * from SupportBean");
            stmtContext.Dispose();
            TryInvalid(epService, eplCreateCtx, "Error starting statement: Context by name 'NineToFive' is still referenced by statements and may not be changed");
    
            // not found
            TryInvalid(epService, "context EightToSix select * from SupportBean", "Error starting statement: Context by name 'EightToSix' has not been declared [");
    
            // test update: update is not allowed as it is processed out-of-context by runtime
            epService.EPAdministrator.CreateEPL("insert into ABCStream select * from SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByAString partition by TheString from SupportBean");
            try {
                epService.EPAdministrator.CreateEPL("context SegmentedByAString update istream ABCStream set IntPrimitive = (select id from SupportBean_S0#lastevent) where IntPrimitive < 0");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Update IStream is not supported in conjunction with a context [context SegmentedByAString update istream ABCStream set IntPrimitive = (select id from SupportBean_S0#lastevent) where IntPrimitive < 0]", ex.Message);
            }
    
            // context declaration for create-context
            epService.EPAdministrator.CreateEPL("create context ABC start @now end after 5 seconds");
            TryInvalid(epService, "context ABC create context DEF start @now end after 5 seconds",
                    "Error starting statement: A create-context statement cannot itself be associated to a context, please declare a nested context instead [context ABC create context DEF start @now end after 5 seconds]");
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    }
} // end of namespace
