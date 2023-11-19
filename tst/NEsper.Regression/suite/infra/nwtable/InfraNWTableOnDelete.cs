///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithCondition(execs);
            WithPattern(execs);
            WithAll(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraDeleteAll(true));
            execs.Add(new InfraDeleteAll(false));
            return execs;
        }

        public static IList<RegressionExecution> WithPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraDeletePattern(true));
            execs.Add(new InfraDeletePattern(false));
            return execs;
        }

        public static IList<RegressionExecution> WithCondition(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraDeleteCondition(true));
            execs.Add(new InfraDeleteCondition(false));
            return execs;
        }

        private class InfraDeleteAll : RegressionExecution
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
                    ? "@name('CreateInfra') @public create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@name('CreateInfra') @public create table MyInfra (a string primary key, b int)";
                var path = new RegressionPath();
                env.CompileDeploy(stmtTextCreate, path).AddListener("CreateInfra");

                // create delete stmt
                var stmtTextDelete = "@name('OnDelete') on SupportBean_A delete from MyInfra";
                env.CompileDeploy(stmtTextDelete, path).AddListener("OnDelete");
                env.AssertStatement(
                    "OnDelete",
                    statement => EPAssertionUtil.AssertEqualsAnyOrder(
                        statement.EventType.PropertyNames,
                        new string[] { "a", "b" }));

                // create insert into
                var stmtTextInsertOne =
                    "@name('Insert') insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // create consumer
                var fields = new string[] { "a", "b" };
                var stmtTextSelect = "@name('Select') select irstream MyInfra.a as a, b from MyInfra as s1";
                env.CompileDeploy(stmtTextSelect, path).AddListener("Select");

                // Delete all events, no result expected
                SendSupportBean_A(env, "A1");
                env.AssertListenerNotInvoked("CreateInfra");
                env.AssertListenerNotInvoked("Select");
                env.AssertListenerNotInvoked("OnDelete");
                env.AssertThat(() => Assert.AreEqual(0, GetCount(env, path, "MyInfra")));

                // send 1 event
                SendSupportBean(env, "E1", 1);
                if (namedWindow) {
                    env.AssertPropsNew("CreateInfra", fields, new object[] { "E1", 1 });
                    env.AssertPropsNew("Select", fields, new object[] { "E1", 1 });
                }
                else {
                    env.AssertListenerNotInvoked("CreateInfra");
                    env.AssertListenerNotInvoked("Select");
                }

                env.AssertPropsPerRowIterator("CreateInfra", fields, new object[][] { new object[] { "E1", 1 } });
                env.AssertPropsPerRowIterator("OnDelete", fields, null);
                env.AssertThat(() => Assert.AreEqual(1, GetCount(env, path, "MyInfra")));

                env.Milestone(0);

                // Delete all events, 1 row expected
                SendSupportBean_A(env, "A2");
                if (namedWindow) {
                    env.AssertPropsOld("CreateInfra", fields, new object[] { "E1", 1 });
                    env.AssertPropsOld("Select", fields, new object[] { "E1", 1 });
                    env.AssertPropsPerRowIterator("OnDelete", fields, Array.Empty<object[]>());
                }

                env.AssertPropsPerRowIterator("CreateInfra", fields, null);
                env.AssertPropsNew("OnDelete", fields, new object[] { "E1", 1 });
                env.AssertThat(() => Assert.AreEqual(0, GetCount(env, path, "MyInfra")));

                // send 2 events
                SendSupportBean(env, "E2", 2);
                SendSupportBean(env, "E3", 3);
                env.ListenerReset("CreateInfra");
                env.AssertPropsPerRowIterator(
                    "CreateInfra",
                    fields,
                    new object[][] { new object[] { "E2", 2 }, new object[] { "E3", 3 } });
                env.AssertListenerNotInvoked("OnDelete");
                env.AssertThat(() => Assert.AreEqual(2, GetCount(env, path, "MyInfra")));

                env.Milestone(1);

                // Delete all events, 2 rows expected
                SendSupportBean_A(env, "A2");
                if (namedWindow) {
                    env.AssertListener(
                        "CreateInfra",
                        listener => {
                            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "E2", 2 });
                            EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[] { "E3", 3 });
                        });
                    env.AssertPropsPerRowIterator("OnDelete", fields, Array.Empty<object[]>());
                }

                env.AssertPropsPerRowIterator("CreateInfra", fields, null);
                env.AssertListener(
                    "OnDelete",
                    listener => {
                        EPAssertionUtil.AssertProps(listener.LastNewData[0], fields, new object[] { "E2", 2 });
                        EPAssertionUtil.AssertProps(listener.LastNewData[1], fields, new object[] { "E3", 3 });
                    });
                env.AssertThat(() => Assert.AreEqual(0, GetCount(env, path, "MyInfra")));

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        private class InfraDeletePattern : RegressionExecution
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
                    ? "@name('CreateInfra') @public create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@name('CreateInfra') @public create table MyInfra(a string primary key, b int)";
                var path = new RegressionPath();
                env.CompileDeploy(stmtTextCreate, path).AddListener("CreateInfra");

                // create delete stmt
                var stmtTextDelete =
                    "@name('OnDelete') on pattern [every ea=SupportBean_A or every eb=SupportBean_B] delete from MyInfra";
                env.CompileDeploy(stmtTextDelete, path).AddListener("OnDelete");

                // create insert into
                var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // send 1 event
                var fields = new string[] { "a", "b" };
                SendSupportBean(env, "E1", 1);
                if (namedWindow) {
                    env.AssertPropsNew("CreateInfra", fields, new object[] { "E1", 1 });
                    env.AssertPropsPerRowIterator("OnDelete", fields, null);
                }

                env.AssertPropsPerRowIterator("CreateInfra", fields, new object[][] { new object[] { "E1", 1 } });
                env.AssertThat(() => Assert.AreEqual(1, GetCount(env, path, "MyInfra")));

                // Delete all events using A, 1 row expected
                SendSupportBean_A(env, "A1");
                if (namedWindow) {
                    env.AssertPropsOld("CreateInfra", fields, new object[] { "E1", 1 });
                }

                env.AssertPropsPerRowIterator("CreateInfra", fields, null);
                env.AssertPropsNew("OnDelete", fields, new object[] { "E1", 1 });
                env.AssertThat(() => Assert.AreEqual(0, GetCount(env, path, "MyInfra")));

                env.Milestone(0);

                // send 1 event
                SendSupportBean(env, "E2", 2);
                if (namedWindow) {
                    env.AssertPropsNew("CreateInfra", fields, new object[] { "E2", 2 });
                }

                env.AssertPropsPerRowIterator("CreateInfra", fields, new object[][] { new object[] { "E2", 2 } });
                env.AssertThat(() => Assert.AreEqual(1, GetCount(env, path, "MyInfra")));

                // Delete all events using B, 1 row expected
                SendSupportBean_B(env, "B1");
                if (namedWindow) {
                    env.AssertPropsOld("CreateInfra", fields, new object[] { "E2", 2 });
                }

                env.AssertPropsPerRowIterator("CreateInfra", fields, null);
                env.AssertPropsNew("OnDelete", fields, new object[] { "E2", 2 });
                env.AssertThat(() => Assert.AreEqual(0, GetCount(env, path, "MyInfra")));

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
        }

        private class InfraDeleteCondition : RegressionExecution
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
                    ? "@name('CreateInfra') @public create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@name('CreateInfra') @public create table MyInfra (a string primary key, b int)";
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
                env.AssertThat(() => Assert.AreEqual(3, GetCount(env, path, "MyInfra")));
                env.ListenerReset("CreateInfra");
                var fields = new string[] { "a", "b" };
                env.AssertPropsPerRowIterator(
                    "CreateInfra",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });

                // delete E2
                SendSupportBean_A(env, "XE2X");
                if (namedWindow) {
                    env.AssertPropsOld("CreateInfra", fields, new object[] { "E2", 2 });
                }

                env.ListenerReset("CreateInfra");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "CreateInfra",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });
                env.AssertThat(() => Assert.AreEqual(2, GetCount(env, path, "MyInfra")));

                SendSupportBean(env, "E7", 7);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "CreateInfra",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 }, new object[] { "E7", 7 } });
                env.AssertThat(() => Assert.AreEqual(3, GetCount(env, path, "MyInfra")));

                env.Milestone(1);

                // delete all under 5
                SendSupportBean_B(env, "B1");
                if (namedWindow) {
                    env.AssertListener(
                        "CreateInfra",
                        listener => {
                            Assert.AreEqual(2, listener.LastOldData.Length);
                            EPAssertionUtil.AssertProps(listener.LastOldData[0], fields, new object[] { "E1", 1 });
                            EPAssertionUtil.AssertProps(listener.LastOldData[1], fields, new object[] { "E3", 3 });
                        });
                }

                env.AssertPropsPerRowIteratorAnyOrder(
                    "CreateInfra",
                    fields,
                    new object[][] { new object[] { "E7", 7 } });
                env.AssertThat(() => Assert.AreEqual(1, GetCount(env, path, "MyInfra")));

                env.UndeployAll();
            }

            public string Name()
            {
                return this.GetType().Name +
                       "{" +
                       "namedWindow=" +
                       namedWindow +
                       '}';
            }
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
                .AsInt64();
        }
    }
} // end of namespace