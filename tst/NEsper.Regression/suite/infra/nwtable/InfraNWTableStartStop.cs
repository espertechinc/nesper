///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableStartStop
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithConsumer(execs);
            WithInserter(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInserter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraStartStopInserter(true));
            execs.Add(new InfraStartStopInserter(false));
            return execs;
        }

        public static IList<RegressionExecution> WithConsumer(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraStartStopConsumer(true));
            execs.Add(new InfraStartStopConsumer(false));
            return execs;
        }

        private class InfraStartStopInserter : RegressionExecution
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
                    ? "@name('create') @public create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@name('create') @public create table MyInfra(a string primary key, b int primary key)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsertOne =
                    "@name('insert') insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // create consumer
                var fields = new string[] { "a", "b" };
                var stmtTextSelect = "@name('select') select a, b from MyInfra as s1";
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");

                // send 1 event
                SendSupportBean(env, "E1", 1);
                if (namedWindow) {
                    env.AssertPropsNew("create", fields, new object[] { "E1", 1 });
                    env.AssertPropsNew("select", fields, new object[] { "E1", 1 });
                }

                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1 } });

                // stop inserter
                env.UndeployModuleContaining("insert");

                SendSupportBean(env, "E2", 2);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("select");

                // start inserter
                env.CompileDeploy(stmtTextInsertOne, path);

                // consumer receives the next event
                SendSupportBean(env, "E3", 3);
                if (namedWindow) {
                    env.AssertPropsNew("create", fields, new object[] { "E3", 3 });
                    env.AssertPropsNew("select", fields, new object[] { "E3", 3 });
                    env.AssertPropsPerRowIterator(
                        "select",
                        fields,
                        new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });
                }

                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });

                // destroy inserter
                env.UndeployModuleContaining("insert");

                SendSupportBean(env, "E4", 4);
                env.AssertListenerNotInvoked("create");
                env.AssertListenerNotInvoked("select");

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

        private class InfraStartStopConsumer : RegressionExecution
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
                    ? "@name('create') @public create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean"
                    : "@name('create') @public create table MyInfra(a string primary key, b int primary key)";
                env.CompileDeploy(stmtTextCreate, path).AddListener("create");

                // create insert into
                var stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
                env.CompileDeploy(stmtTextInsertOne, path);

                // create consumer
                var fields = new string[] { "a", "b" };
                var stmtTextSelect = "@name('select') select a, b from MyInfra as s1";
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");

                // send 1 event
                SendSupportBean(env, "E1", 1);
                if (namedWindow) {
                    env.AssertPropsNew("create", fields, new object[] { "E1", 1 });
                    env.AssertPropsNew("select", fields, new object[] { "E1", 1 });
                }

                env.AssertPropsPerRowIterator("create", fields, new object[][] { new object[] { "E1", 1 } });

                // stop consumer
                var selectListenerTemp = env.Listener("select");
                env.UndeployModuleContaining("select");
                SendSupportBean(env, "E2", 2);
                if (namedWindow) {
                    env.AssertPropsNew("create", fields, new object[] { "E2", 2 });
                }

                ClassicAssert.IsFalse(selectListenerTemp.IsInvoked);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });

                // start consumer: the consumer has the last event even though he missed it
                env.CompileDeploy(stmtTextSelect, path).AddListener("select");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "select",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });

                // consumer receives the next event
                SendSupportBean(env, "E3", 3);
                if (namedWindow) {
                    env.AssertPropsNew("create", fields, new object[] { "E3", 3 });
                    env.AssertPropsNew("select", fields, new object[] { "E3", 3 });
                    env.AssertPropsPerRowIterator(
                        "select",
                        fields,
                        new object[][]
                            { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
                }

                env.AssertPropsPerRowIteratorAnyOrder(
                    "create",
                    fields,
                    new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });

                // destroy consumer
                selectListenerTemp = env.Listener("select");
                env.UndeployModuleContaining("select");
                SendSupportBean(env, "E4", 4);
                if (namedWindow) {
                    env.AssertPropsNew("create", fields, new object[] { "E4", 4 });
                }

                ClassicAssert.IsFalse(selectListenerTemp.IsInvoked);

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

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.OBSERVEROPS);
            }
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
    }
} // end of namespace