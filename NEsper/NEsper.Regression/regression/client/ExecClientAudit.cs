///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.dataflow;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientAudit : RegressionExecution {
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog AUDITLOG = LogManager.GetLogger(AuditPath.AUDIT_LOG);
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            configuration.AddEventType("SupportBean_ST1", typeof(SupportBean_ST1));
            configuration.EngineDefaults.Logging.AuditPattern = "[%u] [%s] [%c] %m";
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionDocSample(epService);
            RunAssertionAudit(epService);
        }
    
        private void RunAssertionDocSample(EPServiceProvider epService) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create schema OrderEvent(price double)");
    
            string epl = "@Name('All-Order-Events') @Audit('stream,property') select price from OrderEvent";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
    
            if (EventRepresentationChoiceExtensions.GetEngineDefault(epService).IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{100d}, "OrderEvent");
            } else {
                epService.EPRuntime.SendEvent(Collections.SingletonDataMap("price", 100d), "OrderEvent");
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionAudit(EPServiceProvider epService) {
    
            // stream, and test audit callback
            var callback = new SupportAuditCallback();
            AuditPath.AuditCallback = callback.Audit;
            AUDITLOG.Info("*** Stream: ");
            EPStatement stmtInput = epService.EPAdministrator.CreateEPL("@Name('ABC') @Audit('stream') select * from SupportBean(TheString = 'E1')");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(1, callback.Audits.Count);
            AuditContext cb = callback.Audits[0];
            Assert.AreEqual("SupportBean(TheString=...) inserted SupportBean[SupportBean(E1, 1)]", cb.Message);
            Assert.AreEqual("ABC", cb.StatementName);
            Assert.AreEqual(EPServiceProviderConstants.DEFAULT_ENGINE_URI, cb.EngineURI);
            Assert.AreEqual(AuditEnum.STREAM, cb.Category);
            AuditPath.AuditCallback = null;
            stmtInput.Dispose();
    
            AUDITLOG.Info("*** Named Window And Insert-Into: ");
            EPStatement stmtNW = epService.EPAdministrator.CreateEPL("@Name('create') @Audit create window WinOne#keepall as SupportBean");
            EPStatement stmtInsertNW = epService.EPAdministrator.CreateEPL("@Name('insert') @Audit insert into WinOne select * from SupportBean");
            EPStatement stmtConsumeNW = epService.EPAdministrator.CreateEPL("@Name('select') @Audit select * from WinOne");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            stmtNW.Dispose();
            stmtInsertNW.Dispose();
            stmtConsumeNW.Dispose();
    
            AUDITLOG.Info("*** Insert-Into: ");
            EPStatement stmtInsertInto = epService.EPAdministrator.CreateEPL("@Name('insert') @Audit insert into ABC select * from SupportBean");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            stmtInsertInto.Dispose();
    
            AUDITLOG.Info("*** Schedule: ");
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            EPStatement stmtSchedule = epService.EPAdministrator.CreateEPL("@Name('ABC') @Audit('schedule') select irstream * from SupportBean#time(1 sec)");
            var listener = new SupportUpdateListener();
            stmtSchedule.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            listener.Reset();
            Log.Info("Sending time");
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            Assert.IsTrue(listener.IsInvoked);
            listener.Reset();
            stmtSchedule.Dispose();
    
            // exprdef-instances
            AUDITLOG.Info("*** Expression-Def: ");
            EPStatement stmtExprDef = epService.EPAdministrator.CreateEPL("@Name('ABC') @Audit('exprdef') " +
                    "expression DEF { 1 } " +
                    "expression INN {  x => x.TheString }" +
                    "expression OUT { x => INN(x) } " +
                    "select DEF(), OUT(sb) from SupportBean sb");
            stmtExprDef.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("DEF()"));
            stmtExprDef.Dispose();
    
            // pattern-instances
            AUDITLOG.Info("*** Pattern-Lifecycle: ");
            EPStatement stmtPatternLife = epService.EPAdministrator.CreateEPL("@Name('ABC') @Audit('pattern-instances') select a.IntPrimitive as val0 from pattern [every a=SupportBean -> (b=SupportBean_ST0 and not SupportBean_ST1)]");
            stmtPatternLife.Events += listener.Update;
            Log.Info("Sending E1");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Log.Info("Sending E2");
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Log.Info("Sending E3");
            epService.EPRuntime.SendEvent(new SupportBean_ST1("E3", 3));
            stmtPatternLife.Dispose();
    
            // pattern
            AUDITLOG.Info("*** Pattern: ");
            EPStatement stmtPattern = epService.EPAdministrator.CreateEPL("@Name('ABC') @Audit('pattern') select a.IntPrimitive as val0 from pattern [a=SupportBean -> b=SupportBean_ST0]");
            stmtPattern.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E2", 2));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("val0"));
            stmtPattern.Dispose();
    
            // view
            AUDITLOG.Info("*** View: ");
            EPStatement stmtView = epService.EPAdministrator.CreateEPL("@Name('ABC') @Audit('view') select IntPrimitive from SupportBean#lastevent");
            stmtView.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            Assert.AreEqual(50, listener.AssertOneGetNewAndReset().Get("IntPrimitive"));
            stmtView.Dispose();
    
            EPStatement stmtGroupedView = epService.EPAdministrator.CreateEPL("@Audit Select * From SupportBean#groupwin(TheString)#length(2)");
            stmtGroupedView.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            listener.Reset();
            stmtGroupedView.Dispose();
    
            EPStatement stmtGroupedWIntersectionView = epService.EPAdministrator.CreateEPL("@Audit Select * From SupportBean#groupwin(TheString)#length(2)#unique(IntPrimitive)");
            stmtGroupedWIntersectionView.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            listener.Reset();
            stmtGroupedWIntersectionView.Dispose();
    
            // expression
            AUDITLOG.Info("*** Expression: ");
            EPStatement stmtExpr = epService.EPAdministrator.CreateEPL("@Name('ABC') @Audit('expression') select IntPrimitive*100 as val0, sum(IntPrimitive) as val1 from SupportBean");
            stmtExpr.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            Assert.AreEqual(5000, listener.AssertOneGetNew().Get("val0"));
            Assert.AreEqual(50, listener.AssertOneGetNewAndReset().Get("val1"));
            stmtExpr.Dispose();
    
            // expression-detail
            AUDITLOG.Info("*** Expression-Nested: ");
            EPStatement stmtExprNested = epService.EPAdministrator.CreateEPL("@Name('ABC') @Audit('expression-nested') select ('A'||TheString)||'X' as val0 from SupportBean");
            stmtExprNested.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            Assert.AreEqual("AE1X", listener.AssertOneGetNewAndReset().Get("val0"));
            stmtExprNested.Dispose();
    
            // property
            AUDITLOG.Info("*** Property: ");
            EPStatement stmtProp = epService.EPAdministrator.CreateEPL("@Name('ABC') @Audit('property') select IntPrimitive from SupportBean");
            stmtProp.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            Assert.AreEqual(50, listener.AssertOneGetNewAndReset().Get("IntPrimitive"));
            stmtProp.Dispose();
    
            // with aggregation
            epService.EPAdministrator.CreateEPL("@Audit @Name ('create') create window MyWindow#keepall as SupportBean");
            string eplWithAgg = "@Audit @Name('S0') on SupportBean as sel select count(*) from MyWindow as win having count(*)=3 order by win.IntPrimitive";
            EPStatement stmtWithAgg = epService.EPAdministrator.CreateEPL(eplWithAgg);
            stmtWithAgg.Dispose();
    
            // data flow
            EPStatement stmtDataflow = epService.EPAdministrator.CreateEPL("@Audit @Name('df') create dataflow MyFlow " +
                    "EventBusSource -> a<SupportBean> {filter:TheString like 'I%'} " +
                    "Filter(a) -> b {filter: true}" +
                    "LogSink(b) {log:false}");
            EPDataFlowInstance df = epService.EPRuntime.DataFlowRuntime.Instantiate("MyFlow");
            df.Start();
            epService.EPRuntime.SendEvent(new SupportBean("I1", 1));
            df.Cancel();
    
            // context partitions
            epService.EPAdministrator.CreateEPL("create context WhenEventArrives " +
                    "initiated by SupportBean_ST0 as st0 " +
                    "terminated by SupportBean_ST1(id=st0.id)");
            epService.EPAdministrator.CreateEPL("@Audit('ContextPartition') context WhenEventArrives select * from SupportBean");
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean_ST1("E1", 0));
            stmtDataflow.Dispose();
    
            // table
            AUDITLOG.Info("*** Table And Insert-Into and Into-table: ");
            EPStatement stmtTable = epService.EPAdministrator.CreateEPL("@Name('create-table') @Audit create table TableOne(c0 string primary key, cnt count(*))");
            EPStatement stmtIntoTable = epService.EPAdministrator.CreateEPL("@Name('into-table') @Audit into table TableOne select count(*) as cnt from SupportBean group by TheString");
            EPStatement stmtAccessTable = epService.EPAdministrator.CreateEPL("@Name('access-table') @Audit select TableOne[id].cnt from SupportBean_ST0");
            stmtAccessTable.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            stmtTable.Dispose();
            stmtIntoTable.Dispose();
            stmtAccessTable.Dispose();
        }
    }
} // end of namespace
