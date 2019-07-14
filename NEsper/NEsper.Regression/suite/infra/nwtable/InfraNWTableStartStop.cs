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
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableStartStop
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraStartStopConsumer(true));
            execs.Add(new InfraStartStopConsumer(false));
            execs.Add(new InfraStartStopInserter(true));
            execs.Add(new InfraStartStopInserter(false));
            return execs;
        }

        private static SupportBean SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
            return bean;
        }

        internal class InfraStartStopInserter : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraStartStopInserter(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                // create window
                var path = new RegressionPath();
                var stmtTextCreate = namedWindow
                    ? "@Name('create') create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@Name('create') create table MyInfra(a string primary key, b int primary key)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsertOne =
                    "@Name('insert') insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // create consumer
                string[] fields = {"a", "b"};
                var stmtTextSelect = "@Name('select') select a, b from MyInfra as s1";
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");

                // send 1 event
                SendSupportBean(env, "E1", 1);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", 1});
                    EPAssertionUtil.AssertProps(
                        env.Listener("select").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", 1});
                }

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}});

                // stop inserter
                env.UndeployModuleContaining("insert");

                SendSupportBean(env, "E2", 2);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                // start inserter
                env.CompileDeploy(stmtTextInsertOne, path);

                // consumer receives the next event
                SendSupportBean(env, "E3", 3);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E3", 3});
                    EPAssertionUtil.AssertProps(
                        env.Listener("select").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E3", 3});
                    EPAssertionUtil.AssertPropsPerRow(
                        env.GetEnumerator("select"),
                        fields,
                        new[] {new object[] {"E1", 1}, new object[] {"E3", 3}});
                }

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E3", 3}});

                // destroy inserter
                env.UndeployModuleContaining("insert");

                SendSupportBean(env, "E4", 4);
                Assert.IsFalse(env.Listener("create").IsInvoked);
                Assert.IsFalse(env.Listener("select").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class InfraStartStopConsumer : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraStartStopConsumer(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                // create window
                var stmtTextCreate = namedWindow
                    ? "@Name('create') create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@Name('create') create table MyInfra(a string primary key, b int primary key)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // create consumer
                string[] fields = {"a", "b"};
                var stmtTextSelect = "@Name('select') select a, b from MyInfra as s1";
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");

                // send 1 event
                SendSupportBean(env, "E1", 1);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", 1});
                    EPAssertionUtil.AssertProps(
                        env.Listener("select").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E1", 1});
                }

                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}});

                // stop consumer
                var selectListenerTemp = env.Listener("select");
                env.UndeployModuleContaining("select");
                SendSupportBean(env, "E2", 2);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E2", 2});
                }

                Assert.IsFalse(selectListenerTemp.IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

                // start consumer: the consumer has the last event even though he missed it
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("select"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}});

                // consumer receives the next event
                SendSupportBean(env, "E3", 3);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E3", 3});
                    EPAssertionUtil.AssertProps(
                        env.Listener("select").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E3", 3});
                    EPAssertionUtil.AssertPropsPerRow(
                        env.GetEnumerator("select"),
                        fields,
                        new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
                }

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("create"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});

                // destroy consumer
                selectListenerTemp = env.Listener("select");
                env.UndeployModuleContaining("select");
                SendSupportBean(env, "E4", 4);
                if (namedWindow) {
                    EPAssertionUtil.AssertProps(
                        env.Listener("create").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"E4", 4});
                }

                Assert.IsFalse(selectListenerTemp.IsInvoked);

                env.UndeployAll();
            }
        }
    }
} // end of namespace