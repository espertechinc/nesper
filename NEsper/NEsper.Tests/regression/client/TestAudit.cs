///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestAudit
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog AuditLog = LogManager.GetLogger(AuditPath.AUDIT_LOG);
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            EPServiceProviderManager.PurgeAllProviders();

            _listener = new SupportUpdateListener();

            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("SupportBean", typeof (SupportBean));
            configuration.AddEventType("SupportBean_ST0", typeof (SupportBean_ST0));
            configuration.AddEventType("SupportBean_ST1", typeof (SupportBean_ST1));
            configuration.EngineDefaults.LoggingConfig.AuditPattern = "[%u] [%s] [%c] %m";
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            Console.WriteLine("Teardown Complete");
        }

        [Test]
        public void TestDocSample()
        {
            try {
                _epService.EPAdministrator.CreateEPL("create schema OrderEvent(price double)");
    
                var epl = "@Name('All-Order-Events') @Audit('stream,property') select price from OrderEvent";
                _epService.EPAdministrator.CreateEPL(epl).Events += _listener.Update;
            
                if (EventRepresentationEnumExtensions.GetEngineDefault(_epService).IsObjectArrayEvent()) {
                    _epService.EPRuntime.SendEvent(new Object[] {100d}, "OrderEvent");
                }
                else {
                    _epService.EPRuntime.SendEvent(Collections.SingletonDataMap("price", 100d), "OrderEvent");
                }
            } catch(Exception e) {
                Console.WriteLine("Exception caught: {0}", e.GetType());
                throw;
            }
        }

        [Test]
        public void TestAuditBasic()
        {
            // stream, and test audit callback
            var callback = new SupportAuditCallback();
            AuditPath.AuditCallback += callback.Audit;
            AuditLog.Info("*** Stream: ");
            var stmtInput = _epService.EPAdministrator.CreateEPL("@Name('ABC') @Audit('stream') select * from SupportBean(TheString = 'E1')");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(1, callback.Audits.Count);
            var cb = callback.Audits[0];
            Assert.AreEqual("SupportBean(TheString=...) inserted SupportBean[SupportBean(E1, 1)]", cb.Message);
            Assert.AreEqual("ABC", cb.StatementName);
            Assert.AreEqual(EPServiceProviderConstants.DEFAULT_ENGINE_URI, cb.EngineURI);
            Assert.AreEqual(AuditEnum.STREAM, cb.Category);
            AuditPath.AuditCallback -= callback.Audit;
            stmtInput.Dispose();
        }

        [Test]
        public void TestAuditNamedWindowAndInsertInto()
        {
            AuditLog.Info("*** Named Window And Insert-Into: ");
            var stmtNW =
                _epService.EPAdministrator.CreateEPL(
                    "@Name('create') @Audit create window WinOne.win:keepall() as SupportBean");
            var stmtInsertNW =
                _epService.EPAdministrator.CreateEPL(
                    "@Name('insert') @Audit insert into WinOne select * from SupportBean");
            var stmtConsumeNW = _epService.EPAdministrator.CreateEPL("@Name('select') @Audit select * from WinOne");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            stmtNW.Dispose();
            stmtInsertNW.Dispose();
            stmtConsumeNW.Dispose();
        }

        [Test]
        public void TestAuditInsertInto()
        {
            AuditLog.Info("*** Insert-Into: ");
            var stmtInsertInto =
                _epService.EPAdministrator.CreateEPL("@Name('insert') @Audit insert into ABC select * from SupportBean");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            stmtInsertInto.Dispose();
        }

        [Test]
        public void TestAuditSchedule()
        {
            AuditLog.Info("*** Schedule: ");
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            var stmtSchedule =
                _epService.EPAdministrator.CreateEPL(
                    "@Name('ABC') @Audit('schedule') select irstream * from SupportBean.win:time(1 sec)");
            stmtSchedule.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _listener.Reset();
            Log.Info("Sending time");
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            Assert.IsTrue(_listener.IsInvoked);
            _listener.Reset();
            stmtSchedule.Dispose();
        }

        [Test]
        public void TestAuditExpressionDef()
        {
            // exprdef-instances
            AuditLog.Info("*** Expression-Def: ");
            var stmtExprDef = _epService.EPAdministrator.CreateEPL(
                "@Name('ABC') @Audit('exprdef') " +
                "expression DEF { 1 } " +
                "expression INN {  x => x.TheString }" +
                "expression OUT { x => INN(x) } " +
                "select DEF(), OUT(sb) from SupportBean sb");
            stmtExprDef.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("DEF()"));
            stmtExprDef.Dispose();
        }

        [Test]
        public void TestAuditPatternLifecycle()
        {
            // pattern-instances
            AuditLog.Info("*** Pattern-Lifecycle: ");
            var stmtPatternLife =
                _epService.EPAdministrator.CreateEPL(
                    "@Name('ABC') @Audit('pattern-instances') select a.IntPrimitive as val0 from pattern [every a=SupportBean -> (b=SupportBean_ST0 and not SupportBean_ST1)]");
            stmtPatternLife.Events += _listener.Update;
            Log.Info("Sending E1");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Log.Info("Sending E2");
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Log.Info("Sending E3");
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("E3", 3));
            stmtPatternLife.Dispose();
        }

        [Test]
        public void TestAuditPattern()
        {
            // pattern
            AuditLog.Info("*** Pattern: ");
            var stmtPattern =
                _epService.EPAdministrator.CreateEPL(
                    "@Name('ABC') @Audit('pattern') select a.IntPrimitive as val0 from pattern [a=SupportBean -> b=SupportBean_ST0]");
            stmtPattern.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E2", 2));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("val0"));
            stmtPattern.Dispose();
        }

        [Test]
        public void TestAuditView()
        {
            // view
            AuditLog.Info("*** View: ");
            var stmtView =
                _epService.EPAdministrator.CreateEPL(
                    "@Name('ABC') @Audit('view') select IntPrimitive from SupportBean.std:lastevent()");
            stmtView.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            Assert.AreEqual(50, _listener.AssertOneGetNewAndReset().Get("IntPrimitive"));
            stmtView.Dispose();

            var stmtGroupedView =
                _epService.EPAdministrator.CreateEPL(
                    "@Audit Select * From SupportBean.std:groupwin(TheString).win:length(2)");
            stmtGroupedView.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            _listener.Reset();
            stmtGroupedView.Dispose();
        }

        [Test]
        public void TestAuditExpression()
        {
            // expression
            AuditLog.Info("*** Expression: ");
            var stmtExpr =
                _epService.EPAdministrator.CreateEPL(
                    "@Name('ABC') @Audit('expression') select IntPrimitive*100 as val0, Sum(IntPrimitive) as val1 from SupportBean");
            stmtExpr.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            Assert.AreEqual(5000, _listener.AssertOneGetNew().Get("val0"));
            Assert.AreEqual(50, _listener.AssertOneGetNewAndReset().Get("val1"));
            stmtExpr.Dispose();
        }

        [Test]
        public void TestAuditExpressionDetail()
        {
            // expression-detail
            AuditLog.Info("*** Expression-Nested: ");
            var stmtExprNested =
                _epService.EPAdministrator.CreateEPL(
                    "@Name('ABC') @Audit('expression-nested') select ('A'||TheString)||'X' as val0 from SupportBean");
            stmtExprNested.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            Assert.AreEqual("AE1X", _listener.AssertOneGetNewAndReset().Get("val0"));
            stmtExprNested.Dispose();
        }

        [Test]
        public void TestAuditProperty()
        {
            // property
            AuditLog.Info("*** Property: ");
            var stmtProp =
                _epService.EPAdministrator.CreateEPL(
                    "@Name('ABC') @Audit('property') select IntPrimitive from SupportBean");
            stmtProp.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            Assert.AreEqual(50, _listener.AssertOneGetNewAndReset().Get("IntPrimitive"));
            stmtProp.Dispose();
        }

        [Test]
        public void TestAuditAggregation()
        {
            // with aggregation
            _epService.EPAdministrator.CreateEPL(
                "@Audit @Name ('create') create window MyWindow.win:keepall() as SupportBean");
            var eplWithAgg =
                "@Audit @Name('S0') on SupportBean as sel select Count(*) from MyWindow as win having Count(*)=3 order by win.IntPrimitive";
            var stmtWithAgg = _epService.EPAdministrator.CreateEPL(eplWithAgg);
            stmtWithAgg.Dispose();
        }

        [Test]
        public void TestAuditDataFlow()
        {
            // data flow
            var stmtDataflow = _epService.EPAdministrator.CreateEPL(
                "@Audit @Name('df') create dataflow MyFlow " +
                "EventBusSource -> a<SupportBean> {filter:TheString like 'I%'} " +
                "Filter(a) -> b {filter: true}" +
                "LogSink(b) {log:false}");
            var df = _epService.EPRuntime.DataFlowRuntime.Instantiate("MyFlow");
            df.Start();
            _epService.EPRuntime.SendEvent(new SupportBean("I1", 1));
            df.Cancel();

            // context partitions
            _epService.EPAdministrator.CreateEPL(
                "create context WhenEventArrives " +
                "initiated by SupportBean_ST0 as st0 " +
                "terminated by SupportBean_ST1(id=st0.id)");
            _epService.EPAdministrator.CreateEPL(
                "@Audit('ContextPartition') context WhenEventArrives select * from SupportBean");
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean_ST1("E1", 0));
            stmtDataflow.Dispose();
        }

        [Test]
        public void TestAuditTable()
        {
            // table
            AuditLog.Info("*** Table And Insert-Into and Into-table: ");
            EPStatement stmtTable = _epService.EPAdministrator.CreateEPL("@Name('create-table') @Audit create table TableOne(c0 string primary key, cnt count(*))");
            EPStatement stmtIntoTable = _epService.EPAdministrator.CreateEPL("@Name('into-table') @Audit into table TableOne select count(*) as cnt from SupportBean group by TheString");
            EPStatement stmtAccessTable = _epService.EPAdministrator.CreateEPL("@Name('access-table') @Audit select TableOne[id].cnt from SupportBean_ST0");
            stmtAccessTable.AddListener(_listener);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            stmtTable.Dispose();
            stmtIntoTable.Dispose();
            stmtAccessTable.Dispose();
        }

        private class SupportAuditCallback
        {
            private readonly IList<AuditContext> _audits = new List<AuditContext>();
    
            public void Audit(AuditContext auditContext)
            {
                _audits.Add(auditContext);
            }

            public IList<AuditContext> Audits
            {
                get { return _audits; }
            }
        }
    }
}
