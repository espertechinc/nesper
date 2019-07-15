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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using SupportBean_A = com.espertech.esper.regressionlib.support.bean.SupportBean_A;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableOnDelete
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();

            execs.Add(new InfraDeleteCondition(true));
            execs.Add(new InfraDeleteCondition(false));

            execs.Add(new InfraDeletePattern(true));
            execs.Add(new InfraDeletePattern(false));

            execs.Add(new InfraDeleteAll(true));
            execs.Add(new InfraDeleteAll(false));

            return execs;
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

        private static long GetCount(
            RegressionEnvironment env,
            RegressionPath path,
            string windowOrTableName)
        {
            return env.CompileExecuteFAF("select count(*) as c0 from " + windowOrTableName, path)
                .Array[0]
                .Get("c0")
                .AsLong();
        }

        internal class InfraDeleteAll : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraDeleteAll(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                // create window
                var stmtTextCreate = namedWindow
                    ? "@Name('CreateInfra') create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@Name('CreateInfra') create table MyInfra (a string primary key, b int)";
                var path = new RegressionPath();
                env.CompileDeploy(stmtTextCreate, path).AddListener("CreateInfra");

                // create delete stmt
                var stmtTextDelete = "@Name('OnDelete') on SupportBean_A delete from MyInfra";
                env.CompileDeploy(stmtTextDelete, path).AddListener("OnDelete");
                EPAssertionUtil.AssertEqualsAnyOrder(
                    env.Statement("OnDelete").EventType.PropertyNames,
                    new[] {"a", "b"});

                // create insert into
                var stmtTextInsertOne =
                    "@Name('Insert') insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // create consumer
                string[] fields = {"a", "b"};
                var stmtTextSelect = "@Name('Select') select irstream MyInfra.a as a, b from MyInfra as s1";
                env.CompileDeploy(stmtTextSelect, path).AddListener("Select");

                // Delete all events, no result expected
                SendSupportBean_A(env, "A1");
                Assert.IsFalse(env.Listener("CreateInfra").IsInvoked);
                Assert.IsFalse(env.Listener("Select").IsInvoked);
                Assert.IsFalse(env.Listener("OnDelete").IsInvoked);
                Assert.AreEqual(0, GetCount(env, path, "MyInfra"));

                // send 1 event
                SendSupportBean(env, "E1", 1);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("CreateInfra").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", 1});
                    EPAssertionUtil.AssertProps(
                        env.Listener("Select").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", 1});
                }
                else {
                    Assert.IsFalse(env.Listener("CreateInfra").IsInvoked);
                    Assert.IsFalse(env.Listener("Select").IsInvoked);
                }

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("CreateInfra"),
                    fields,
                    new[] {new object[] {"E1", 1}});
                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("OnDelete"), fields, null);
                Assert.AreEqual(1, GetCount(env, path, "MyInfra"));

                env.Milestone(0);

                // Delete all events, 1 row expected
                SendSupportBean_A(env, "A2");
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("CreateInfra").AssertOneGetOldAndReset(),
                        fields,
                        new object[] {"E1", 1});
                    EPAssertionUtil.AssertProps(
                        env.Listener("Select").AssertOneGetOldAndReset(),
                        fields,
                        new object[] {"E1", 1});
                    EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("OnDelete"), fields, new object[0][]);
                }

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("CreateInfra"), fields, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("OnDelete").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});
                Assert.AreEqual(0, GetCount(env, path, "MyInfra"));

                // send 2 events
                SendSupportBean(env, "E2", 2);
                SendSupportBean(env, "E3", 3);
                env.Listener("CreateInfra").Reset();
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("CreateInfra"),
                    fields,
                    new[] {new object[] {"E2", 2}, new object[] {"E3", 3}});
                Assert.IsFalse(env.Listener("OnDelete").IsInvoked);
                Assert.AreEqual(2, GetCount(env, path, "MyInfra"));

                env.Milestone(1);

                // Delete all events, 2 rows expected
                SendSupportBean_A(env, "A2");
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("CreateInfra").LastOldData[0],
                        fields,
                        new object[] {"E2", 2});
                    EPAssertionUtil.AssertProps(
                        env.Listener("CreateInfra").LastOldData[1],
                        fields,
                        new object[] {"E3", 3});
                    EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("OnDelete"), fields, new object[0][]);
                }

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("CreateInfra"), fields, null);
                Assert.AreEqual(2, env.Listener("OnDelete").LastNewData.Length);
                EPAssertionUtil.AssertProps(
                    env.Listener("OnDelete").LastNewData[0],
                    fields,
                    new object[] {"E2", 2});
                EPAssertionUtil.AssertProps(
                    env.Listener("OnDelete").LastNewData[1],
                    fields,
                    new object[] {"E3", 3});
                Assert.AreEqual(0, GetCount(env, path, "MyInfra"));

                env.UndeployAll();
            }
        }

        internal class InfraDeletePattern : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraDeletePattern(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                // create infra
                var stmtTextCreate = namedWindow
                    ? "@Name('CreateInfra') create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@Name('CreateInfra') create table MyInfra(a string primary key, b int)";
                var path = new RegressionPath();
                env.CompileDeploy(stmtTextCreate, path).AddListener("CreateInfra");

                // create delete stmt
                var stmtTextDelete =
                    "@Name('OnDelete') on pattern [every ea=SupportBean_A or every eb=SupportBean_B] delete from MyInfra";
                env.CompileDeploy(stmtTextDelete, path).AddListener("OnDelete");

                // create insert into
                var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // send 1 event
                string[] fields = {"a", "b"};
                SendSupportBean(env, "E1", 1);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("CreateInfra").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", 1});
                    EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("OnDelete"), fields, null);
                }

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("CreateInfra"),
                    fields,
                    new[] {new object[] {"E1", 1}});
                Assert.AreEqual(1, GetCount(env, path, "MyInfra"));

                // Delete all events using A, 1 row expected
                SendSupportBean_A(env, "A1");
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("CreateInfra").AssertOneGetOldAndReset(),
                        fields,
                        new object[] {"E1", 1});
                }

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("CreateInfra"), fields, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("OnDelete").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});
                Assert.AreEqual(0, GetCount(env, path, "MyInfra"));

                env.Milestone(0);

                // send 1 event
                SendSupportBean(env, "E2", 2);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("CreateInfra").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E2", 2});
                }

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("CreateInfra"),
                    fields,
                    new[] {new object[] {"E2", 2}});
                Assert.AreEqual(1, GetCount(env, path, "MyInfra"));

                // Delete all events using B, 1 row expected
                SendSupportBean_B(env, "B1");
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("CreateInfra").AssertOneGetOldAndReset(),
                        fields,
                        new object[] {"E2", 2});
                }

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("CreateInfra"), fields, null);
                EPAssertionUtil.AssertProps(
                    env.Listener("OnDelete").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2});
                Assert.AreEqual(0, GetCount(env, path, "MyInfra"));

                env.UndeployAll();
            }
        }

        internal class InfraDeleteCondition : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraDeleteCondition(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // create infra
                var stmtTextCreate = namedWindow
                    ? "@Name('CreateInfra') create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@Name('CreateInfra') create table MyInfra (a string primary key, b int)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("CreateInfra");

                // create delete stmt
                var stmtTextDelete = "on SupportBean_A delete from MyInfra where 'X' || a || 'X' = Id";
                env.CompileDeploy(stmtTextDelete, path);

                // create delete stmt
                stmtTextDelete = "on SupportBean_B delete from MyInfra where b < 5";
                env.CompileDeploy(stmtTextDelete, path);

                // create insert into
                var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // send 3 event
                SendSupportBean(env, "E1", 1);
                SendSupportBean(env, "E2", 2);

                env.Milestone(0);

                SendSupportBean(env, "E3", 3);
                Assert.AreEqual(3, GetCount(env, path, "MyInfra"));
                env.Listener("CreateInfra").Reset();
                string[] fields = {"a", "b"};
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("CreateInfra").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});

                // delete E2
                SendSupportBean_A(env, "XE2X");
                if (namedWindow) {
                    Assert.AreEqual(1, env.Listener("CreateInfra").LastOldData.Length);
                    EPAssertionUtil.AssertProps(
                        env.Listener("CreateInfra").LastOldData[0],
                        fields,
                        new object[] {"E2", 2});
                }

                env.Listener("CreateInfra").Reset();
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("CreateInfra").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E3", 3}});
                Assert.AreEqual(2, GetCount(env, path, "MyInfra"));

                SendSupportBean(env, "E7", 7);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("CreateInfra").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E3", 3}, new object[] {"E7", 7}});
                Assert.AreEqual(3, GetCount(env, path, "MyInfra"));

                env.Milestone(1);

                // delete all under 5
                SendSupportBean_B(env, "B1");
                if (namedWindow) {
                    Assert.AreEqual(2, env.Listener("CreateInfra").LastOldData.Length);
                    EPAssertionUtil.AssertProps(
                        env.Listener("CreateInfra").LastOldData[0],
                        fields,
                        new object[] {"E1", 1});
                    EPAssertionUtil.AssertProps(
                        env.Listener("CreateInfra").LastOldData[1],
                        fields,
                        new object[] {"E3", 3});
                }

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("CreateInfra").GetEnumerator(),
                    fields,
                    new[] {new object[] {"E7", 7}});
                Assert.AreEqual(1, GetCount(env, path, "MyInfra"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace