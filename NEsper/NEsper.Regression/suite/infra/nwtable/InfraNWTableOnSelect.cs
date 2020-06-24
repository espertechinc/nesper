///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.@internal.kernel.statement;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableOnSelect : IndexBackingTableInfo
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            
            execs.Add(new InfraOnSelectIndexSimple(true));
            execs.Add(new InfraOnSelectIndexSimple(false));

            execs.Add(new InfraOnSelectIndexChoice(true));
            execs.Add(new InfraOnSelectIndexChoice(false));
            
            execs.Add(new InfraWindowAgg(true));
            execs.Add(new InfraWindowAgg(false));

            execs.Add(new InfraSelectAggregationHavingStreamWildcard(true));
            execs.Add(new InfraSelectAggregationHavingStreamWildcard(false));

            execs.Add(new InfraPatternTimedSelect(true));
            execs.Add(new InfraPatternTimedSelect(false));

            execs.Add(new InfraInvalid(true));
            execs.Add(new InfraInvalid(false));

            execs.Add(new InfraSelectCondition(true));
            execs.Add(new InfraSelectCondition(false));

            execs.Add(new InfraSelectJoinColumnsLimit(true));
            execs.Add(new InfraSelectJoinColumnsLimit(false));

            execs.Add(new InfraSelectAggregation(true));
            execs.Add(new InfraSelectAggregation(false));

            execs.Add(new InfraSelectAggregationCorrelated(true));
            execs.Add(new InfraSelectAggregationCorrelated(false));

            execs.Add(new InfraSelectAggregationGrouping(true));
            execs.Add(new InfraSelectAggregationGrouping(false));

            execs.Add(new InfraSelectCorrelationDelete(true));
            execs.Add(new InfraSelectCorrelationDelete(false));

            execs.Add(new InfraPatternCorrelation(true));
            execs.Add(new InfraPatternCorrelation(false));

            return execs;
        }

        private static void AssertIndexChoice(
            RegressionEnvironment env,
            bool namedWindow,
            string[] indexes,
            object[] preloadedEvents,
            string datawindow,
            IndexAssertion[] assertions)
        {
            var path = new RegressionPath();
            var eplCreate = namedWindow
                ? "@Name('create-window') create window MyInfra." + datawindow + " as SupportSimpleBeanOne"
                : "@Name('create-table') create table MyInfra(S1 string primary key, I1 int, D1 double, L1 long)";
            env.CompileDeploy(eplCreate, path);

            env.CompileDeploy("insert into MyInfra select S1,I1,D1,L1 from SupportSimpleBeanOne", path);
            foreach (var index in indexes) {
                env.CompileDeploy("@Name('create-index') " + index, path);
            }

            foreach (var @event in preloadedEvents) {
                env.SendEventBean(@event);
            }

            var count = 0;
            foreach (var assertion in assertions) {
                log.Info("======= Testing #" + count++);
                var consumeEpl = INDEX_CALLBACK_HOOK +
                                 (assertion.Hint == null ? "" : assertion.Hint) +
                                 "@Name('s0') on SupportSimpleBeanTwo as ssb2 " +
                                 "select * " +
                                 "from MyInfra as ssb1 where " +
                                 assertion.WhereClause;

                var epl = "@Name('s0') " + consumeEpl;
                if (assertion.EventSendAssertion == null) {
                    try {
                        env.CompileWCheckedEx(epl, path);
                        Assert.Fail();
                    }
                    catch (EPCompileException ex) {
                        Assert.IsTrue(ex.Message.Contains("index hint busted"));
                    }
                }
                else {
                    env.CompileDeploy(epl, path).AddListener("s0");
                    SupportQueryPlanIndexHook.AssertOnExprTableAndReset(
                        assertion.ExpectedIndexName,
                        assertion.IndexBackingClass);
                    assertion.EventSendAssertion.Invoke();
                    env.UndeployModuleContaining("s0");
                }
            }

            env.UndeployAll();
        }

        private static void AssertReceived(
            RegressionEnvironment env,
            bool namedWindow,
            SupportBean[] beans,
            int[] indexesAll,
            int[] indexesWhere,
            string[] mapKeys,
            object[] mapValues)
        {
            var received = env.Listener("select").AssertOneGetNewAndReset();
            object[] expectedAll;
            object[] expectedWhere;
            if (!namedWindow) {
                expectedAll = SupportBean.GetOAStringAndIntPerIndex(beans, indexesAll);
                expectedWhere = SupportBean.GetOAStringAndIntPerIndex(beans, indexesWhere);
                EPAssertionUtil.AssertEqualsAnyOrder(expectedAll, (object[]) received.Get("c0"));
                var receivedColl = received.Get("c1").UnwrapIntoArray<object>();
                EPAssertionUtil.AssertEqualsAnyOrder(expectedWhere, receivedColl);
            }
            else {
                expectedAll = SupportBean.GetBeansPerIndex(beans, indexesAll);
                expectedWhere = SupportBean.GetBeansPerIndex(beans, indexesWhere);
                EPAssertionUtil.AssertEqualsExactOrder(expectedAll, (object[]) received.Get("c0"));
                EPAssertionUtil.AssertEqualsExactOrder(expectedWhere, (ICollection<object>) received.Get("c1"));
            }

            EPAssertionUtil.AssertPropsMap((IDictionary<object, object>) received.Get("c2"), mapKeys, mapValues);
        }

        private static void SendSupportBean_A(
            RegressionEnvironment env,
            string id)
        {
            var bean = new SupportBean_A(id);
            env.SendEventBean(bean);
        }

        private static void SendSupportBean_B(
            RegressionEnvironment env,
            string id)
        {
            var bean = new SupportBean_B(id);
            env.SendEventBean(bean);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendSupportBean_S0(
            RegressionEnvironment env,
            int id,
            string p00)
        {
            env.SendEventBean(new SupportBean_S0(id, p00));
        }

        private static void SendTimer(
            long timeInMSec,
            RegressionEnvironment env)
        {
            env.AdvanceTime(timeInMSec);
        }

        public class InfraOnSelectIndexSimple : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraOnSelectIndexSimple(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                if (namedWindow) {
                    env.CompileDeploy("create window MyInfra.win:length(5) as (numericKey int, value string)", path);
                }
                else {
                    env.CompileDeploy("create table MyInfra(numericKey int primary key, value string)", path);
                }

                env.CompileDeploy("create index MyIndex on MyInfra(value)", path);
                env.CompileDeploy(
                    "insert into MyInfra select IntPrimitive as numericKey, TheString as value from SupportBean",
                    path);

                var epl = "@Name('out') on SupportBean_S0 as S0 select value from MyInfra where value = P00";
                env.CompileDeploy(epl, path).AddListener("out");

                SendSupportBean(env, "E1", 1);
                SendSupportBean_S0(env, 1, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("out").AssertOneGetNewAndReset(),
                    new [] { "value" },
                    new object[] {"E1"});

                env.Milestone(0);

                SendSupportBean(env, "E2", 2);
                SendSupportBean_S0(env, 2, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("out").AssertOneGetNewAndReset(),
                    new [] { "value" },
                    new object[] {"E2"});

                env.UndeployAll();
            }
        }

        internal class InfraPatternCorrelation : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraPatternCorrelation(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"a", "b"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate = namedWindow
                    ? "@Name('create') create window MyInfraPC#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@Name('create') create table MyInfraPC(a string primary key, b int primary key)";
                env.CompileDeploy(stmtTextCreate, path);

                // create select stmt
                var stmtTextSelect =
                    "@Name('select') on pattern [every ea=SupportBean_A or every eb=SupportBean_B] select mywin.* from MyInfraPC as mywin where a = coalesce(ea.Id, eb.Id)";
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");

                // create insert into
                var stmtTextInsertOne =
                    "insert into MyInfraPC select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // send 3 event
                SendSupportBean(env, "E1", 1);

                env.Milestone(0);

                SendSupportBean(env, "E2", 2);
                SendSupportBean(env, "E3", 3);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                // fire trigger
                SendSupportBean_A(env, "X1");
                Assert.IsFalse(env.Listener("select").IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
                if (namedWindow) {
                    EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("select"), fields, null);
                }

                env.Milestone(1);

                SendSupportBean_B(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2});

                SendSupportBean_A(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                env.Milestone(2);

                SendSupportBean_B(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3", 3});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});

                env.UndeployAll();
            }
        }

        internal class InfraSelectCorrelationDelete : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectCorrelationDelete(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"a", "b"};

                var epl = namedWindow
                    ? "@Name('create') create window MyInfraSCD#keepall as select TheString as a, IntPrimitive as b from SupportBean;\n"
                    : "@Name('create') create table MyInfraSCD(a string primary key, b int primary key);\n";
                epl += "@Name('select') on SupportBean_A select mywin.* from MyInfraSCD as mywin where Id = a;\n";
                epl += "insert into MyInfraSCD select TheString as a, IntPrimitive as b from SupportBean;\n";
                epl += "@Name('delete') on SupportBean_B delete from MyInfraSCD where a = Id;\n";
                env.CompileDeploy(epl).AddListener("select");

                // send 3 event
                SendSupportBean(env, "E1", 1);
                SendSupportBean(env, "E2", 2);
                SendSupportBean(env, "E3", 3);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                env.Milestone(0);

                // fire trigger
                SendSupportBean_A(env, "X1");
                Assert.IsFalse(env.Listener("select").IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});

                SendSupportBean_A(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});

                env.Milestone(1);

                SendSupportBean_A(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});

                // delete event
                SendSupportBean_B(env, "E1");
                Assert.IsFalse(env.Listener("select").IsInvoked);

                SendSupportBean_A(env, "E1");
                Assert.IsFalse(env.Listener("select").IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E2", 2}, new object[] {"E3", 3}});

                env.Milestone(2);

                SendSupportBean_A(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2});

                env.UndeployModuleContaining("select");
            }
        }

        internal class InfraSelectAggregationGrouping : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectAggregationGrouping(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"a", "sumb"};

                var path = new RegressionPath();
                var epl = namedWindow
                    ? "@Name('create') create window MyInfraSAG#keepall as select TheString as a, IntPrimitive as b from SupportBean;\n"
                    : "@Name('create') create table MyInfraSAG(a string primary key, b int primary key);\n";
                epl +=
                    "@Name('select') on SupportBean_A select a, sum(b) as sumb from MyInfraSAG group by a order by a desc;\n";
                epl +=
                    "@Name('selectTwo') on SupportBean_A select a, sum(b) as sumb from MyInfraSAG group by a having sum(b) > 5 order by a desc;\n";
                epl +=
                    "@Name('insert') insert into MyInfraSAG select TheString as a, IntPrimitive as b from SupportBean;\n";
                env.CompileDeploy(epl, path).AddListener("select").AddListener("selectTwo");

                // fire trigger
                SendSupportBean_A(env, "A1");
                Assert.IsFalse(env.Listener("select").IsInvoked);
                Assert.IsFalse(env.Listener("selectTwo").IsInvoked);

                // send 3 events
                SendSupportBean(env, "E1", 1);
                SendSupportBean(env, "E2", 2);

                env.Milestone(0);

                SendSupportBean(env, "E1", 5);
                Assert.IsFalse(env.Listener("select").IsInvoked);
                Assert.IsFalse(env.Listener("selectTwo").IsInvoked);

                // fire trigger
                SendSupportBean_A(env, "A1");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("select").LastNewData,
                    fields,
                    new[] {new object[] {"E2", 2}, new object[] {"E1", 6}});
                Assert.IsNull(env.Listener("select").LastOldData);
                env.Listener("select").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("selectTwo").LastNewData,
                    fields,
                    new[] {new object[] {"E1", 6}});
                Assert.IsNull(env.Listener("select").LastOldData);
                env.Listener("select").Reset();

                env.Milestone(1);

                // send 3 events
                SendSupportBean(env, "E4", -1);
                SendSupportBean(env, "E2", 10);
                SendSupportBean(env, "E1", 100);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                env.Milestone(2);

                SendSupportBean_A(env, "A2");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("select").LastNewData,
                    fields,
                    new[] {new object[] {"E4", -1}, new object[] {"E2", 12}, new object[] {"E1", 106}});

                // create delete stmt, delete E2
                var stmtTextDelete = "on SupportBean_B delete from MyInfraSAG where Id = a";
                env.CompileDeploy(stmtTextDelete, path);
                SendSupportBean_B(env, "E2");

                SendSupportBean_A(env, "A3");
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("select").LastNewData,
                    fields,
                    new[] {new object[] {"E4", -1}, new object[] {"E1", 106}});
                Assert.IsNull(env.Listener("select").LastOldData);
                env.Listener("select").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("selectTwo").LastNewData,
                    fields,
                    new[] {new object[] {"E1", 106}});
                Assert.IsNull(env.Listener("selectTwo").LastOldData);
                env.Listener("selectTwo").Reset();

                var resultType = env.Statement("select").EventType;
                Assert.AreEqual(2, resultType.PropertyNames.Length);
                Assert.AreEqual(typeof(string), resultType.GetPropertyType("a"));
                Assert.AreEqual(typeof(int?), resultType.GetPropertyType("sumb"));

                env.UndeployAll();
            }
        }

        internal class InfraSelectAggregationCorrelated : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectAggregationCorrelated(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"sumb"};

                var epl = namedWindow
                    ? "@Name('create') create window MyInfraSAC#keepall as select TheString as a, IntPrimitive as b from SupportBean;\n"
                    : "@Name('create') create table MyInfraSAC(a string primary key, b int primary key);\n";
                epl += "@Name('select') on SupportBean_A select sum(b) as sumb from MyInfraSAC where a = Id;\n";
                epl += "insert into MyInfraSAC select TheString as a, IntPrimitive as b from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("select").AddListener("create");

                // send 3 event
                SendSupportBean(env, "E1", 1);
                SendSupportBean(env, "E2", 2);

                env.Milestone(0);

                SendSupportBean(env, "E3", 3);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                // fire trigger
                SendSupportBean_A(env, "A1");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null});

                env.Milestone(1);

                // fire trigger
                SendSupportBean_A(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2});

                SendSupportBean(env, "E2", 10);

                env.Milestone(2);

                SendSupportBean_A(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {12});

                var resultType = env.Statement("select").EventType;
                Assert.AreEqual(1, resultType.PropertyNames.Length);
                Assert.AreEqual(typeof(int?), resultType.GetPropertyType("sumb"));

                env.UndeployModuleContaining("create");
            }
        }

        internal class InfraSelectAggregation : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectAggregation(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"sumb"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate = namedWindow
                    ? "@Name('create') create window MyInfraSA#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@Name('create') create table MyInfraSA (a string primary key, b int primary key)";
                env.CompileDeploy(stmtTextCreate, path);

                // create select stmt
                var stmtTextSelect = "@Name('select') on SupportBean_A select sum(b) as sumb from MyInfraSA";
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");

                // create insert into
                var stmtTextInsertOne =
                    "insert into MyInfraSA select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // send 3 event
                SendSupportBean(env, "E1", 1);
                SendSupportBean(env, "E2", 2);
                SendSupportBean(env, "E3", 3);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                env.Milestone(0);

                // fire trigger
                SendSupportBean_A(env, "A1");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {6});

                // create delete stmt
                var stmtTextDelete = "on SupportBean_B delete from MyInfraSA where Id = a";
                env.CompileDeploy(stmtTextDelete, path);

                // Delete E2
                SendSupportBean_B(env, "E2");

                env.Milestone(1);

                // fire trigger
                SendSupportBean_A(env, "A2");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4});

                SendSupportBean(env, "E4", 10);
                SendSupportBean_A(env, "A3");
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {14});

                var resultType = env.Statement("select").EventType;
                Assert.AreEqual(1, resultType.PropertyNames.Length);
                Assert.AreEqual(typeof(int?), resultType.GetPropertyType("sumb"));

                env.UndeployAll();
            }
        }

        internal class InfraSelectJoinColumnsLimit : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectJoinColumnsLimit(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"triggerId", "wina", "b"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate = namedWindow
                    ? "@Name('create') create window MyInfraSA#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@Name('create') create table MyInfraSA (a string primary key, b int)";
                env.CompileDeploy(stmtTextCreate, path);

                // create select stmt
                var stmtTextSelect =
                    "@Name('select') on SupportBean_A as trigger select trigger.Id as triggerId, win.a as wina, b from MyInfraSA as win order by wina";
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");

                // create insert into
                var stmtTextInsertOne =
                    "insert into MyInfraSA select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // send 3 event
                SendSupportBean(env, "E1", 1);
                SendSupportBean(env, "E2", 2);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                env.Milestone(0);

                // fire trigger
                SendSupportBean_A(env, "A1");
                Assert.AreEqual(2, env.Listener("select").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").LastNewData[0],
                    fields,
                    new object[] {"A1", "E1", 1});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").LastNewData[1],
                    fields,
                    new object[] {"A1", "E2", 2});

                // try limit clause
                env.UndeployModuleContaining("select");
                stmtTextSelect =
                    "@Name('select') on SupportBean_A as trigger select trigger.Id as triggerId, win.a as wina, b from MyInfraSA as win order by wina limit 1";
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");

                env.Milestone(1);

                SendSupportBean_A(env, "A1");
                Assert.AreEqual(1, env.Listener("select").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").LastNewData[0],
                    fields,
                    new object[] {"A1", "E1", 1});

                env.UndeployAll();
            }
        }

        internal class InfraSelectCondition : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectCondition(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fieldsCreate = {"a", "b"};
                string[] fieldsOnSelect = {"a", "b", "Id"};
                var path = new RegressionPath();

                // create window
                var infraName = "MyInfraSC" + (namedWindow ? "NW" : "Tbl");
                var stmtTextCreate = namedWindow
                    ? "@Name('create') create window " +
                      infraName +
                      "#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@Name('create') create table " + infraName + " (a string primary key, b int)";
                env.CompileDeploy(stmtTextCreate, path);

                // create select stmt
                var stmtTextSelect = "@Name('select') on SupportBean_A select mywin.*, Id from " +
                                     infraName +
                                     " as mywin where " +
                                     infraName +
                                     ".b < 3 order by a asc";
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");
                Assert.AreEqual(
                    StatementType.ON_SELECT,
                    env.Statement("select").GetProperty(StatementProperty.STATEMENTTYPE));

                // create insert into
                var stmtTextInsertOne = "@Name('insert') insert into " +
                                        infraName +
                                        " select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // send 3 event
                SendSupportBean(env, "E1", 1);

                env.Milestone(0);

                SendSupportBean(env, "E2", 2);
                SendSupportBean(env, "E3", 3);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                // fire trigger
                SendSupportBean_A(env, "A1");
                Assert.AreEqual(2, env.Listener("select").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").LastNewData[0],
                    fieldsCreate,
                    new object[] {"E1", 1});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").GetAndResetLastNewData()[1],
                    fieldsCreate,
                    new object[] {"E2", 2});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fieldsCreate,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
                Assert.IsFalse(env.GetEnumerator("select").MoveNext());

                SendSupportBean(env, "E4", 0);

                env.Milestone(1);

                SendSupportBean_A(env, "A2");
                Assert.AreEqual(3, env.Listener("select").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("select").LastNewData[0],
                    fieldsOnSelect,
                    new object[] {"E1", 1, "A2"});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").LastNewData[1],
                    fieldsOnSelect,
                    new object[] {"E2", 2, "A2"});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").GetAndResetLastNewData()[2],
                    fieldsOnSelect,
                    new object[] {"E4", 0, "A2"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fieldsCreate,
                    new[] {
                        new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}, new object[] {"E4", 0}
                    });
                Assert.IsFalse(env.GetEnumerator("select").MoveNext());

                env.UndeployModuleContaining("select");
                env.UndeployModuleContaining("insert");
                env.UndeployModuleContaining("create");
            }
        }

        internal class InfraInvalid : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraInvalid(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var stmtTextCreate = namedWindow
                    ? "create window MyInfraInvalid#keepall as select * from SupportBean"
                    : "create table MyInfraInvalid (TheString string, IntPrimitive int)";
                env.CompileDeploy(stmtTextCreate, path);

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "on SupportBean_A select * from MyInfraInvalid where sum(IntPrimitive) > 100",
                    "Failed to validate expression: An aggregate function may not appear in a WHERE clause (use the HAVING clause) [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "on SupportBean_A insert into MyStream select * from DUMMY",
                    "A named window or table 'DUMMY' has not been declared [");

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "on SupportBean_A select prev(1, TheString) from MyInfraInvalid",
                    "Failed to validate select-clause expression 'prev(1,TheString)': Previous function cannot be used in this context [");

                env.UndeployAll();
            }
        }

        internal class InfraPatternTimedSelect : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraPatternTimedSelect(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                // test for JIRA ESPER-332
                SendTimer(0, env);
                var path = new RegressionPath();

                var stmtTextCreate = namedWindow
                    ? "create window MyInfraPTS#keepall as select * from SupportBean"
                    : "create table MyInfraPTS as (TheString string)";
                env.CompileDeploy(stmtTextCreate, path);

                var stmtCount =
                    "on pattern[every timer:interval(10 sec)] select count(eve), eve from MyInfraPTS as eve";
                env.CompileDeploy(stmtCount, path);

                var stmtTextOnSelect =
                    "@Name('select') on pattern [every timer:interval(10 sec)] " +
                    "select TheString from MyInfraPTS having count(TheString) > 0";
                env.CompileDeploy(stmtTextOnSelect, path).AddListener("select");

                var stmtTextInsertOne = namedWindow
                    ? "insert into MyInfraPTS select * from SupportBean"
                    : "insert into MyInfraPTS select TheString from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                SendTimer(11000, env);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                env.Milestone(0);

                SendTimer(21000, env);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                SendSupportBean(env, "E1", 1);
                SendTimer(31000, env);
                Assert.AreEqual("E1", env.Listener("select").AssertOneGetNewAndReset().Get("TheString"));

                env.UndeployAll();
            }
        }

        internal class InfraSelectAggregationHavingStreamWildcard : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSelectAggregationHavingStreamWildcard(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                // create window
                var stmtTextCreate = namedWindow
                    ? "create window MyInfraSHS#keepall as (a string, b int)"
                    : "create table MyInfraSHS as (a string primary key, b int primary key)";
                env.CompileDeploy(stmtTextCreate, path);

                var stmtTextInsertOne =
                    "insert into MyInfraSHS select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                var stmtTextSelect =
                    "@Name('select') on SupportBean_A select mwc.* as mwcwin from MyInfraSHS mwc where Id = a group by a having sum(b) = 20";
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");
                Assert.IsFalse(((EPStatementSPI) env.Statement("select")).StatementContext.IsStatelessSelect);

                // send 3 event
                SendSupportBean(env, "E1", 16);
                SendSupportBean(env, "E2", 2);

                env.Milestone(0);

                SendSupportBean(env, "E1", 4);

                // fire trigger
                SendSupportBean_A(env, "E1");
                var events = env.Listener("select").LastNewData;
                Assert.AreEqual(2, events.Length);
                Assert.AreEqual("E1", events[0].Get("mwcwin.a"));
                Assert.AreEqual("E1", events[1].Get("mwcwin.a"));

                env.UndeployAll();
            }
        }

        internal class InfraWindowAgg : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraWindowAgg(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "create window MyInfraWA#keepall as SupportBean"
                    : "create table MyInfraWA(TheString string primary key, IntPrimitive int)";
                env.CompileDeploy(eplCreate, path);
                var eplInsert = namedWindow
                    ? "insert into MyInfraWA select * from SupportBean"
                    : "insert into MyInfraWA select TheString, IntPrimitive from SupportBean";
                env.CompileDeploy(eplInsert, path);
                env.CompileDeploy("on SupportBean_S1 as S1 delete from MyInfraWA where S1.P10 = TheString", path);

                var epl = "@Name('select') on SupportBean_S0 as S0 " +
                          "select window(win.*) as c0," +
                          "window(win.*).where(v -> v.IntPrimitive < 2) as c1, " +
                          "window(win.*).toMap(k=>k.TheString,v->v.IntPrimitive) as c2 " +
                          "from MyInfraWA as win";
                env.CompileDeploy(epl, path).AddListener("select");

                var beans = new SupportBean[3];
                for (var i = 0; i < beans.Length; i++) {
                    beans[i] = new SupportBean("E" + i, i);
                }

                env.SendEventBean(beans[0]);
                env.SendEventBean(beans[1]);
                env.SendEventBean(new SupportBean_S0(10));
                AssertReceived(
                    env,
                    namedWindow,
                    beans,
                    new[] {0, 1},
                    new[] {0, 1},
                    new [] { "E0","E1" },
                    new object[] {0, 1});

                // add bean
                env.SendEventBean(beans[2]);
                env.SendEventBean(new SupportBean_S0(10));
                AssertReceived(
                    env,
                    namedWindow,
                    beans,
                    new[] {0, 1, 2},
                    new[] {0, 1},
                    new [] { "E0","E1","E2" },
                    new object[] {0, 1, 2});

                env.Milestone(0);

                // delete bean
                env.SendEventBean(new SupportBean_S1(11, "E1"));
                env.SendEventBean(new SupportBean_S0(12));
                AssertReceived(
                    env,
                    namedWindow,
                    beans,
                    new[] {0, 2},
                    new[] {0},
                    new [] { "E0","E2" },
                    new object[] {0, 2});

                // delete another bean
                env.SendEventBean(new SupportBean_S1(13, "E0"));
                env.SendEventBean(new SupportBean_S0(14));
                AssertReceived(
                    env,
                    namedWindow,
                    beans,
                    new[] {2},
                    new int[0],
                    new [] { "E2" },
                    new object[] {2});

                env.Milestone(1);

                // delete last bean
                env.SendEventBean(new SupportBean_S1(15, "E2"));
                env.SendEventBean(new SupportBean_S0(16));
                AssertReceived(env, namedWindow, beans, null, null, null, null);

                env.UndeployAll();
            }
        }

        internal class InfraOnSelectIndexChoice : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraOnSelectIndexChoice(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var backingUniqueS1 = "unique hash={S1(string)} btree={} advanced={}";
                var backingUniqueS1L1 = "unique hash={S1(string),L1(long)} btree={} advanced={}";
                var backingNonUniqueS1 = "non-unique hash={S1(string)} btree={} advanced={}";
                var backingUniqueS1D1 = "unique hash={S1(string),D1(double)} btree={} advanced={}";
                var backingBtreeI1 = "non-unique hash={} btree={I1(int)} advanced={}";
                var backingBtreeD1 = "non-unique hash={} btree={D1(double)} advanced={}";
                var expectedIdxNameS1 = namedWindow ? null : "MyInfra";

                object[] preloadedEventsOne =
                    {new SupportSimpleBeanOne("E1", 10, 11, 12), new SupportSimpleBeanOne("E2", 20, 21, 22)};
                IndexAssertionEventSend eventSendAssertion = () => {
                    var fields = new [] { "ssb2.S2","ssb1.S1","ssb1.I1" };
                    env.SendEventBean(new SupportSimpleBeanTwo("E2", 50, 21, 22));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E2", "E2", 20});
                    env.SendEventBean(new SupportSimpleBeanTwo("E1", 60, 11, 12));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", "E1", 10});
                };

                // single index one field (std:unique(S1))
                AssertIndexChoice(
                    env,
                    namedWindow,
                    new string[0],
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new[] {
                        new IndexAssertion(null, "S1 = S2", expectedIdxNameS1, backingUniqueS1, eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            expectedIdxNameS1,
                            backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            "@Hint('index(One)')",
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            expectedIdxNameS1,
                            backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "S1 = ssb2.S2 and L1 = ssb2.L2") // busted
                    });

                // single index one field (std:unique(S1))
                if (namedWindow) {
                    string[] indexOneField = {"create unique index One on MyInfra (S1)"};
                    AssertIndexChoice(
                        env,
                        namedWindow,
                        indexOneField,
                        preloadedEventsOne,
                        "std:unique(S1)",
                        new[] {
                            new IndexAssertion(null, "S1 = S2", "One", backingUniqueS1, eventSendAssertion),
                            new IndexAssertion(
                                null,
                                "S1 = ssb2.S2 and L1 = ssb2.L2",
                                "One",
                                backingUniqueS1,
                                eventSendAssertion),
                            new IndexAssertion(
                                "@Hint('index(One)')",
                                "S1 = ssb2.S2 and L1 = ssb2.L2",
                                "One",
                                backingUniqueS1,
                                eventSendAssertion),
                            new IndexAssertion("@Hint('index(Two,bust)')", "S1 = ssb2.S2 and L1 = ssb2.L2") // busted
                        });
                }

                // single index two field  (std:unique(S1))
                string[] indexTwoField = {"create unique index One on MyInfra (S1, L1)"};
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexTwoField,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new[] {
                        new IndexAssertion(
                            null,
                            "S1 = ssb2.S2",
                            expectedIdxNameS1,
                            backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            "One",
                            backingUniqueS1L1,
                            eventSendAssertion)
                    });
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexTwoField,
                    preloadedEventsOne,
                    "win:keepall()",
                    new[] {
                        new IndexAssertion(
                            null,
                            "S1 = ssb2.S2",
                            expectedIdxNameS1,
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            "One",
                            backingUniqueS1L1,
                            eventSendAssertion)
                    });

                // two index one unique  (std:unique(S1))
                string[] indexSetTwo = {
                    "create index One on MyInfra (S1)",
                    "create unique index Two on MyInfra (S1, D1)"
                };
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexSetTwo,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new[] {
                        new IndexAssertion(
                            null,
                            "S1 = ssb2.S2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            "@Hint('index(One)')",
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            "One",
                            backingNonUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            "@Hint('index(Two,One)')",
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            "One",
                            backingNonUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "S1 = ssb2.S2 and L1 = ssb2.L2"), // busted
                        new IndexAssertion(
                            "@Hint('index(explicit,bust)')",
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "S1 = ssb2.S2 and D1 = ssb2.D2 and L1 = ssb2.L2",
                            namedWindow ? "Two" : "MyInfra",
                            namedWindow ? backingUniqueS1D1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion("@Hint('index(explicit,bust)')", "D1 = ssb2.D2 and L1 = ssb2.L2") // busted
                    });

                // two index one unique  (win:keepall)
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexSetTwo,
                    preloadedEventsOne,
                    "win:keepall()",
                    new[] {
                        new IndexAssertion(
                            null,
                            "S1 = ssb2.S2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            "@Hint('index(One)')",
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            "One",
                            backingNonUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            "@Hint('index(Two,One)')",
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            "One",
                            backingNonUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion("@Hint('index(Two,bust)')", "S1 = ssb2.S2 and L1 = ssb2.L2"), // busted
                        new IndexAssertion(
                            "@Hint('index(explicit,bust)')",
                            "S1 = ssb2.S2 and L1 = ssb2.L2",
                            namedWindow ? "One" : "MyInfra",
                            namedWindow ? backingNonUniqueS1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion(
                            null,
                            "S1 = ssb2.S2 and D1 = ssb2.D2 and L1 = ssb2.L2",
                            namedWindow ? "Two" : "MyInfra",
                            namedWindow ? backingUniqueS1D1 : backingUniqueS1,
                            eventSendAssertion),
                        new IndexAssertion("@Hint('index(explicit,bust)')", "D1 = ssb2.D2 and L1 = ssb2.L2") // busted
                    });

                // range  (std:unique(S1))
                IndexAssertionEventSend noAssertion = () => { };
                string[] indexSetThree = {
                    "create index One on MyInfra (I1 btree)",
                    "create index Two on MyInfra (D1 btree)"
                };
                AssertIndexChoice(
                    env,
                    namedWindow,
                    indexSetThree,
                    preloadedEventsOne,
                    "std:unique(S1)",
                    new[] {
                        new IndexAssertion(null, "I1 between 1 and 10", "One", backingBtreeI1, noAssertion),
                        new IndexAssertion(null, "D1 between 1 and 10", "Two", backingBtreeD1, noAssertion),
                        new IndexAssertion("@Hint('index(One, bust)')", "D1 between 1 and 10") // busted
                    });

                // rel op
                object[] preloadedEventsRelOp = {new SupportSimpleBeanOne("E1", 10, 11, 12)};
                IndexAssertionEventSend relOpAssertion = () => {
                    var fields = new [] { "ssb2.S2","ssb1.S1","ssb1.I1" };
                    env.SendEventBean(new SupportSimpleBeanTwo("EX", 0, 0, 0));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"EX", "E1", 10});
                };
                AssertIndexChoice(
                    env,
                    namedWindow,
                    new string[0],
                    preloadedEventsRelOp,
                    "win:keepall()",
                    new[] {
                        new IndexAssertion(null, "9 < I1", null, namedWindow ? backingBtreeI1 : null, relOpAssertion),
                        new IndexAssertion(null, "10 <= I1", null, namedWindow ? backingBtreeI1 : null, relOpAssertion),
                        new IndexAssertion(null, "I1 <= 10", null, namedWindow ? backingBtreeI1 : null, relOpAssertion),
                        new IndexAssertion(null, "I1 < 11", null, namedWindow ? backingBtreeI1 : null, relOpAssertion),
                        new IndexAssertion(null, "11 > I1", null, namedWindow ? backingBtreeI1 : null, relOpAssertion)
                    });
            }
        }
    }
} // end of namespace