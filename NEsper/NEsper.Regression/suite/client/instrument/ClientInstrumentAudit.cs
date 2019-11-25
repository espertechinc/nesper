///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.client;

using NUnit.Framework;

using static com.espertech.esper.runtime.client.EPRuntimeProvider;

namespace com.espertech.esper.regressionlib.suite.client.instrument
{
    public class ClientInstrumentAudit
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly ILog AUDITLOG = LogManager.GetLogger(AuditPath.AUDIT_LOG);

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientInstrumentAuditDocSample());
            execs.Add(new ClientInstrumentAuditAudit());
            return execs;
        }

        internal class ClientInstrumentAuditDocSample : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.CompileWBusPublicType("create schema OrderEvent(Price double)");
                env.Deploy(compiled);
                var path = new RegressionPath();
                path.Add(compiled);

                var epl = "@Name('All-Order-Events') @Audit('stream,property') select Price from OrderEvent";
                env.CompileDeploy(epl, path).AddListener("All-Order-Events");

                if (EventRepresentationChoiceExtensions.GetEngineDefault(env.Configuration).IsObjectArrayEvent()) {
                    env.SendEventObjectArray(new object[] {100d}, "OrderEvent");
                }
                else {
                    env.SendEventMap(Collections.SingletonDataMap("Price", 100d), "OrderEvent");
                }

                env.UndeployAll();
            }
        }

        internal class ClientInstrumentAuditAudit : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // stream, and test audit callback
                var callback = new SupportAuditCallback();
                AuditPath.AuditCallback = callback.Audit;

#if false
                AUDITLOG.Info("*** Stream: ");
                env.CompileDeploy("@Name('ABC') @Audit('stream') select * from SupportBean(TheString = 'E1')");
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(1, callback.Audits.Count);
                var cb = callback.Audits[0];
                Assert.AreEqual("SupportBean(TheString=...) inserted SupportBean[SupportBean(E1, 1)]", cb.Message);
                Assert.AreEqual(env.DeploymentId("ABC"), cb.DeploymentId);
                Assert.AreEqual("ABC", cb.StatementName);
                Assert.AreEqual(DEFAULT_RUNTIME_URI, cb.RuntimeURI);
                Assert.AreEqual(AuditEnum.STREAM, cb.Category);
                AuditPath.AuditCallback = null;
                env.UndeployAll();

                AUDITLOG.Info("*** Insert-Into: ");
                env.CompileDeploy("@Name('insert') @Audit insert into ABC select * from SupportBean");
                env.SendEventBean(new SupportBean("E1", 1));
                env.UndeployAll();

                AUDITLOG.Info("*** Named Window And Insert-Into: ");
                env.CompileDeploy("@Name('create') @Audit create window WinOne#keepall as SupportBean", path);
                env.CompileDeploy("@Name('insert') @Audit insert into WinOne select * from SupportBean", path);
                env.CompileDeploy("@Name('select') @Audit select * from WinOne", path);
                env.SendEventBean(new SupportBean("E1", 1));
                env.UndeployAll();
                path.Clear();

                AUDITLOG.Info("*** Schedule: ");
                env.AdvanceTime(0);
                env.CompileDeploy("@Name('ABC') @Audit('schedule') select irstream * from SupportBean#time(1 sec)")
                    .AddListener("ABC");
                env.SendEventBean(new SupportBean("E1", 1));
                env.Listener("ABC").Reset();
                log.Info("Sending time");
                env.AdvanceTime(2000);
                Assert.IsTrue(env.Listener("ABC").IsInvoked);
                env.UndeployAll();

                // property
                AUDITLOG.Info("*** Property: ");
                env.CompileDeploy("@Name('ABC') @Audit('property') select IntPrimitive from SupportBean")
                    .AddListener("ABC");
                env.SendEventBean(new SupportBean("E1", 50));
                Assert.AreEqual(50, env.Listener("ABC").AssertOneGetNewAndReset().Get("IntPrimitive"));
                env.UndeployAll();

                // view
                AUDITLOG.Info("*** View: ");
                env.CompileDeploy("@Name('ABC') @Audit('view') select IntPrimitive from SupportBean#lastevent")
                    .AddListener("ABC");
                env.SendEventBean(new SupportBean("E1", 50));
                Assert.AreEqual(50, env.Listener("ABC").AssertOneGetNewAndReset().Get("IntPrimitive"));
                env.UndeployAll();

                env.CompileDeploy("@Name('s0') @Audit Select * From SupportBean#groupwin(TheString)#length(2)")
                    .AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 50));
                env.UndeployAll();

                env.CompileDeploy(
                        "@Name('s0') @Audit Select * From SupportBean#groupwin(TheString)#length(2)#unique(IntPrimitive)")
                    .AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 50));
                env.UndeployAll();
                // expression
                AUDITLOG.Info("*** Expression: ");
                env.CompileDeploy(
                        "@Name('ABC') @Audit('expression') select IntPrimitive*100 as val0, sum(IntPrimitive) as val1 from SupportBean")
                    .AddListener("ABC");
                env.SendEventBean(new SupportBean("E1", 50));
                Assert.AreEqual(5000, env.Listener("ABC").AssertOneGetNew().Get("val0"));
                Assert.AreEqual(50, env.Listener("ABC").AssertOneGetNewAndReset().Get("val1"));
                env.UndeployAll();

                // expression-detail
                AUDITLOG.Info("*** Expression-Nested: ");
                env.CompileDeploy(
                        "@Name('ABC') @Audit('expression-nested') select ('A'||TheString)||'X' as val0 from SupportBean")
                    .AddListener("ABC");
                env.SendEventBean(new SupportBean("E1", 50));
                Assert.AreEqual("AE1X", env.Listener("ABC").AssertOneGetNewAndReset().Get("val0"));
                env.UndeployAll();

                // pattern
                AUDITLOG.Info("*** Pattern: ");
                env.CompileDeploy(
                        "@Name('ABC') @Audit('pattern') select a.IntPrimitive as val0 from pattern [a=SupportBean -> b=SupportBean_ST0]")
                    .AddListener("ABC");
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean_ST0("E2", 2));
                Assert.AreEqual(1, env.Listener("ABC").AssertOneGetNewAndReset().Get("val0"));
                env.UndeployAll();

                // pattern-instances
                AUDITLOG.Info("*** Pattern-Lifecycle: ");
                env.CompileDeploy(
                        "@Name('ABC') @Audit('pattern-instances') select a.IntPrimitive as val0 from pattern [every a=SupportBean -> (b=SupportBean_ST0 and not SupportBean_ST1)]")
                    .AddListener("ABC");
                log.Info("Sending E1");
                env.SendEventBean(new SupportBean("E1", 1));
                log.Info("Sending E2");
                env.SendEventBean(new SupportBean("E2", 2));
                log.Info("Sending E3");
                env.SendEventBean(new SupportBean_ST1("E3", 3));
                log.Info("Destroy");
                env.UndeployAll();

                // exprdef-instances
                AUDITLOG.Info("*** Expression-Def: ");
                env.CompileDeploy(
                        "@Name('ABC') @Audit('exprdef') " +
                        "expression DEF { 1 } " +
                        "expression INN {  x -> x.TheString }" +
                        "expression OUT { x -> INN(x) } " +
                        "select DEF(), OUT(sb) from SupportBean sb")
                    .AddListener("ABC");
                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual(1, env.Listener("ABC").AssertOneGetNewAndReset().Get("DEF()"));
                env.UndeployAll();
#endif

                // data flow
                env.CompileDeploy(
                    "@Audit @Name('df') create dataflow MyFlow " +
                    "EventBusSource -> a<SupportBean> {filter:TheString like 'I%'} " +
                    "filter(a) -> b {filter: true}" +
                    "LogSink(b) {log:false}");
                var df = env.Runtime.DataFlowService.Instantiate(env.DeploymentId("df"), "MyFlow");
                df.Start();
                env.SendEventBean(new SupportBean("I1", 1));
                df.Cancel();

                // context partitions
                env.CompileDeploy(
                    "create context WhenEventArrives " +
                    "initiated by SupportBean_ST0 as st0 " +
                    "terminated by SupportBean_ST1(Id=st0.Id);\n" +
                    "@Audit('ContextPartition') context WhenEventArrives select * from SupportBean;\n");
                env.SendEventBean(new SupportBean_ST0("E1", 0));
                env.SendEventBean(new SupportBean_ST1("E1", 0));
                env.UndeployAll();

                // table
                AUDITLOG.Info("*** Table And Insert-Into and Into-table: ");
                env.CompileDeploy(
                    "@Name('create-table') @Audit create table TableOne(c0 string primary key, cnt count(*))",
                    path);
                env.CompileDeploy(
                    "@Name('into-table') @Audit into table TableOne select count(*) as cnt from SupportBean group by TheString",
                    path);
                env.CompileDeploy("@Name('access-table') @Audit select TableOne[Id].cnt from SupportBean_ST0", path)
                    .AddListener("access-table");
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean_ST0("E1", 0));
                env.UndeployAll();
                path.Clear();

                // int-expression with endpoint-included
                env.CompileDeploy("@Audit select * from SupportBean#keepall where IntPrimitive in (1:3)");
                env.SendEventBean(new SupportBean("E1", 1));
                env.UndeployAll();
            }
        }
    }
} // end of namespace