///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compiler.client;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableSubquery
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();

            execs.Add(new InfraSubquerySceneOne(true));
            execs.Add(new InfraSubquerySceneOne(false));

            execs.Add(new InfraSubquerySelfCheck(true));
            execs.Add(new InfraSubquerySelfCheck(false));

            execs.Add(new InfraSubqueryDeleteInsertReplace(true));
            execs.Add(new InfraSubqueryDeleteInsertReplace(false));

            execs.Add(new InfraInvalidSubquery(true));
            execs.Add(new InfraInvalidSubquery(false));

            execs.Add(new InfraUncorrelatedSubqueryAggregation(true));
            execs.Add(new InfraUncorrelatedSubqueryAggregation(false));

            return execs;
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            long longPrimitive,
            long? longBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.LongPrimitive = longPrimitive;
            bean.LongBoxed = longBoxed;
            env.SendEventBean(bean);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intBoxed)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntBoxed = intBoxed;
            env.SendEventBean(bean);
        }

        private static void SendMarketBean(
            RegressionEnvironment env,
            string symbol)
        {
            var bean = new SupportMarketDataBean(symbol, 0, 0L, "");
            env.SendEventBean(bean);
        }

        public class InfraSubquerySceneOne : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSubquerySceneOne(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // create infra
                var stmtTextCreate = namedWindow
                    ? "@name('create') create window MyInfra.win:keepall() as SupportBean"
                    : "@name('create') create table MyInfra(TheString string primary key, IntPrimitive int)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsertOne =
                    "@name('insert') insert into MyInfra select TheString, IntPrimitive from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                env.SendEventBean(new SupportBean("A1", 1));
                env.SendEventBean(new SupportBean("B2", 2));
                env.SendEventBean(new SupportBean("C3", 3));

                // create subquery
                var stmtSubquery =
                    "@name('subq') select (select IntPrimitive from MyInfra where TheString = S0.P00) as c0 from SupportBean_S0 as S0";
                env.CompileDeploy(stmtSubquery, path).AddListener("subq");

                env.Milestone(0);

                AssertSubquery(env, "A1", 1);

                env.Milestone(1);

                AssertSubquery(env, "B2", 2);

                // cleanup
                env.UndeployAll();
            }

            private void AssertSubquery(
                RegressionEnvironment env,
                string p00,
                int expected)
            {
                env.SendEventBean(new SupportBean_S0(0, p00));
                EPAssertionUtil.AssertProps(
                    env.Listener("subq").AssertOneGetNewAndReset(),
                    new [] { "c0" },
                    new object[] {expected});
            }
        }

        internal class InfraUncorrelatedSubqueryAggregation : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraUncorrelatedSubqueryAggregation(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                // create window
                var stmtTextCreate = namedWindow
                    ? "@name('create') create window MyInfraUCS#keepall as select TheString as a, LongPrimitive as b from SupportBean"
                    : "@name('create') create table MyInfraUCS(a string primary key, b long)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsertOne =
                    "insert into MyInfraUCS select TheString as a, LongPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // create consumer
                var stmtTextSelectOne =
                    "select irstream (select sum(b) from MyInfraUCS) as value, Symbol from SupportMarketDataBean";
                env.CompileDeploy("@name('selectOne')" + stmtTextSelectOne, path).AddListener("selectOne");

                SendMarketBean(env, "M1");
                string[] fieldsStmt = {"value", "Symbol"};
                EPAssertionUtil.AssertProps(
                    env.Listener("selectOne").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {null, "M1"});

                env.Milestone(0);

                SendSupportBean(env, "S1", 5L, -1L);
                SendMarketBean(env, "M2");
                EPAssertionUtil.AssertProps(
                    env.Listener("selectOne").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {5L, "M2"});

                SendSupportBean(env, "S2", 10L, -1L);
                SendMarketBean(env, "M3");
                EPAssertionUtil.AssertProps(
                    env.Listener("selectOne").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {15L, "M3"});

                // create 2nd consumer
                env.CompileDeploy("@name('selectTwo')" + stmtTextSelectOne, path).AddListener("selectTwo"); // same stmt

                env.Milestone(1);

                SendSupportBean(env, "S3", 8L, -1L);
                SendMarketBean(env, "M4");
                EPAssertionUtil.AssertProps(
                    env.Listener("selectOne").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {23L, "M4"});
                EPAssertionUtil.AssertProps(
                    env.Listener("selectTwo").AssertOneGetNewAndReset(),
                    fieldsStmt,
                    new object[] {23L, "M4"});

                env.UndeployAll();
            }
        }

        internal class InfraInvalidSubquery : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraInvalidSubquery(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplCreate = namedWindow
                    ? "create window MyInfraIS#keepall as SupportBean"
                    : "create table MyInfraIS(TheString string)";
                env.CompileDeploy(eplCreate, path);

                try {
                    env.CompileWCheckedEx("select (select TheString from MyInfraIS#lastevent) from MyInfraIS", path);
                    Assert.Fail();
                }
                catch (EPCompileException ex) {
                    if (namedWindow) {
                        Assert.AreEqual(
                            "Failed to plan subquery number 1 querying MyInfraIS: Consuming statements to a named window cannot declare a data window view onto the named window [select (select TheString from MyInfraIS#lastevent) from MyInfraIS]",
                            ex.Message);
                    }
                    else {
                        SupportMessageAssertUtil.AssertMessage(ex, "Views are not supported with tables");
                    }
                }

                env.UndeployAll();
            }
        }

        internal class InfraSubqueryDeleteInsertReplace : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSubqueryDeleteInsertReplace(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate = namedWindow
                    ? "@name('create') create window MyInfra#keepall as select TheString as key, IntBoxed as value from SupportBean"
                    : "@name('create') create table MyInfra(key string primary key, value int primary key)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // delete
                var stmtTextDelete = "@name('delete') on SupportBean delete from MyInfra where key = TheString";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // create insert into
                var stmtTextInsertOne =
                    "insert into MyInfra select TheString as key, IntBoxed as value from SupportBean as S0";
                env.CompileDeploy(stmtTextInsertOne, path);

                SendSupportBean(env, "E1", 1);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", 1});
                }

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}});

                env.Milestone(0);

                SendSupportBean(env, "E2", 2);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E2", 2});
                    EPAssertionUtil.AssertPropsPerRow(
                        env.GetEnumerator("create"),
                        fields,
                        new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});
                }
                else {
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        env.GetEnumerator("create"),
                        fields,
                        new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});
                }

                SendSupportBean(env, "E1", 3);
                if (namedWindow) {
                    Assert.AreEqual(2, env.Listener("create").NewDataList.Count);
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").OldDataList[0][0],
                        fields,
                        new object[] {"E1", 1});
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").NewDataList[1][0],
                        fields,
                        new object[] {"E1", 3});
                }

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E2", 2}, new object[] {"E1", 3}});

                env.UndeployAll();
            }
        }

        internal class InfraSubquerySelfCheck : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraSubquerySelfCheck(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"key", "value"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate = namedWindow
                    ? "@name('create') create window MyInfraSSS#keepall as select TheString as key, IntBoxed as value from SupportBean"
                    : "@name('create') create table MyInfraSSS (key string primary key, value int)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into (not does insert if key already exists)
                var stmtTextInsertOne =
                    "insert into MyInfraSSS select TheString as key, IntBoxed as value from SupportBean as S0" +
                    " where not exists (select * from MyInfraSSS as win where win.key = S0.TheString)";
                env.CompileDeploy(stmtTextInsertOne, path);

                SendSupportBean(env, "E1", 1);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", 1});
                }
                else {
                    Assert.IsFalse(env.Listener("create").IsInvoked);
                }

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}});

                SendSupportBean(env, "E2", 2);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E2", 2});
                }

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

                SendSupportBean(env, "E1", 3);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

                env.Milestone(0);

                SendSupportBean(env, "E3", 4);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E3", 4});
                    EPAssertionUtil.AssertPropsPerRow(
                        env.GetEnumerator("create"),
                        fields,
                        new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 4}});
                }
                else {
                    EPAssertionUtil.AssertPropsPerRowAnyOrder(
                        env.GetEnumerator("create"),
                        fields,
                        new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 4}});
                }

                // Add delete
                var stmtTextDelete = "@name('delete') on SupportBean_A delete from MyInfraSSS where key = Id";
                env.CompileDeploy(stmtTextDelete, path).AddListener("delete");

                // delete E2
                env.SendEventBean(new SupportBean_A("E2"));
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetOldAndReset(),
                        fields,
                        new object[] {"E2", 2});
                }

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E3", 4}});

                env.Milestone(1);

                SendSupportBean(env, "E2", 5);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E2", 5});
                }

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E3", 4}, new object[] {"E2", 5}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace