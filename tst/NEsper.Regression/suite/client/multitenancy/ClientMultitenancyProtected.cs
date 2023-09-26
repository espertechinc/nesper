///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.multitenancy
{
    public class ClientMultitenancyProtected
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInfra(execs);
            WithVariable(execs);
            WithContext(execs);
            WithEventType(execs);
            WithExpr(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithExpr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientMultitenancyProtectedExpr());
            return execs;
        }

        public static IList<RegressionExecution> WithEventType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientMultitenancyProtectedEventType());
            return execs;
        }

        public static IList<RegressionExecution> WithContext(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientMultitenancyProtectedContext());
            return execs;
        }

        public static IList<RegressionExecution> WithVariable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientMultitenancyProtectedVariable());
            return execs;
        }

        public static IList<RegressionExecution> WithInfra(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientMultitenancyProtectedInfra(true));
            execs.Add(new ClientMultitenancyProtectedInfra(false));
            return execs;
        }

        private static void AssertSelect(
            RegressionEnvironment env,
            string deploymentId,
            object expected)
        {
            Assert.AreEqual(expected, env.Runtime.DeploymentService.GetStatement(deploymentId, "s0").First().Get("c0"));
        }

        private static void AssertContextNoRow(
            RegressionEnvironment env,
            string deploymentId)
        {
            Assert.IsFalse(env.Runtime.DeploymentService.GetStatement(deploymentId, "s0").GetEnumerator().MoveNext());
        }

        private static void AssertContext(
            RegressionEnvironment env,
            string deploymentId,
            long expected)
        {
            Assert.AreEqual(
                expected,
                env.Runtime.DeploymentService.GetStatement(deploymentId, "s0").First().Get("cnt"));
        }

        private static void AssertVariable(
            RegressionEnvironment env,
            string deploymentId,
            int expected)
        {
            Assert.AreEqual(
                expected,
                env.Runtime.DeploymentService.GetStatement(deploymentId, "create").First().Get("myvar"));
        }

        private static void AssertRowsNamedWindow(
            RegressionEnvironment env,
            string deploymentId,
            string ident)
        {
            EPAssertionUtil.AssertPropsPerRow(
                env.Runtime.DeploymentService.GetStatement(deploymentId, "create").GetEnumerator(),
                new[] { "col1", "myIdent" },
                new[] { new object[] { "E1", ident }, new object[] { "E2", ident } });
        }

        internal class ClientMultitenancyProtectedInfra : RegressionExecution
        {
            private readonly bool namedWindow;

            public ClientMultitenancyProtectedInfra(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var eplInfra = namedWindow
                    ? "@name('create') create window MyInfra#keepall as (col1 string, myIdent string);\n"
                    : "@name('create') create table MyInfra(col1 string primary key, myIdent string);\n";
                var epl = eplInfra +
                          "insert into MyInfra select TheString as col1, $X as myIdent from SupportBean;\n";
                var idOne = env.DeployGetId(env.Compile(epl.Replace("$X", "'A'")));
                var idTwo = env.DeployGetId(env.Compile(epl.Replace("$X", "'B'")));

                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 0));
                AssertRowsNamedWindow(env, idOne, "A");
                AssertRowsNamedWindow(env, idTwo, "B");

                env.Undeploy(idOne);
                AssertRowsNamedWindow(env, idTwo, "B");
                Assert.IsNull(env.Runtime.DeploymentService.GetStatement(idOne, "create"));

                env.Undeploy(idTwo);
                Assert.IsNull(env.Runtime.DeploymentService.GetStatement(idTwo, "create"));
            }


            public string Name()
            {
                return $"{this.GetType().Name}{{namedWindow={namedWindow}}}";
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        internal class ClientMultitenancyProtectedVariable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('create') create variable int myvar = $X;\n" +
                          "on pattern[every timer:interval(10)] set myvar = myvar + 1;\n";
                env.AdvanceTime(0);
                var idOne = env.DeployGetId(env.Compile(epl.Replace("$X", "10")));
                var idTwo = env.DeployGetId(env.Compile(epl.Replace("$X", "20")));

                AssertVariable(env, idOne, 10);
                AssertVariable(env, idTwo, 20);

                env.AdvanceTime(10000);

                AssertVariable(env, idOne, 11);
                AssertVariable(env, idTwo, 21);

                env.Undeploy(idOne);

                env.AdvanceTime(20000);

                Assert.IsNull(env.Runtime.DeploymentService.GetStatement(idOne, "create"));
                AssertVariable(env, idTwo, 22);

                env.Undeploy(idTwo);
                Assert.IsNull(env.Runtime.DeploymentService.GetStatement(idTwo, "create"));
            }


            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        internal class ClientMultitenancyProtectedContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('create') create context MyContext start SupportBean(TheString=$X) as sb end after 1 year;\n" +
                    "@name('s0') context MyContext select count(*) as cnt from SupportBean;\n";
                var idOne = env.DeployGetId(env.Compile(epl.Replace("$X", "'A'")));
                var idTwo = env.DeployGetId(env.Compile(epl.Replace("$X", "'B'")));

                AssertContextNoRow(env, idOne);
                AssertContextNoRow(env, idTwo);

                env.SendEventBean(new SupportBean("B", 0));

                AssertContextNoRow(env, idOne);
                AssertContext(env, idTwo, 1);

                env.SendEventBean(new SupportBean("A", 0));
                env.SendEventBean(new SupportBean("A", 0));

                AssertContext(env, idOne, 2);
                AssertContext(env, idTwo, 3);

                env.Undeploy(idOne);

                env.SendEventBean(new SupportBean("X", 0));
                AssertContext(env, idTwo, 4);

                env.Undeploy(idTwo);
            }


            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        internal class ClientMultitenancyProtectedEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne = "create schema MySchema as (col1 string);\n" +
                             "insert into MySchema select TheString as col1 from SupportBean;\n" +
                             "@name('s0') select count(*) as c0 from MySchema;\n";
                var idOne = env.DeployGetId(env.Compile(eplOne));

                var eplTwo = "create schema MySchema as (totalme int);\n" +
                             "insert into MySchema select IntPrimitive as totalme from SupportBean;\n" +
                             "@name('s0') select sum(totalme) as c0 from MySchema;\n";
                var idTwo = env.DeployGetId(env.Compile(eplTwo));

                AssertSelect(env, idOne, 0L);
                AssertSelect(env, idTwo, null);

                env.SendEventBean(new SupportBean("E1", 10));

                AssertSelect(env, idOne, 1L);
                AssertSelect(env, idTwo, 10);

                env.SendEventBean(new SupportBean("E2", 20));

                AssertSelect(env, idOne, 2L);
                AssertSelect(env, idTwo, 30);

                env.Undeploy(idOne);
                Assert.IsNull(env.Runtime.DeploymentService.GetStatement(idOne, "s0"));

                env.SendEventBean(new SupportBean("E3", 30));

                AssertSelect(env, idTwo, 60);

                env.Undeploy(idTwo);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        internal class ClientMultitenancyProtectedExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var eplOne = "create expression my_expression { 1 } ;\n" +
                             "@name('s0') select my_expression as c0 from SupportBean#lastevent;\n";
                var idOne = env.DeployGetId(env.Compile(eplOne));

                var eplTwo = "create expression my_expression { 2 } ;\n" +
                             "@name('s0') select my_expression as c0 from SupportBean#lastevent;\n";
                var idTwo = env.DeployGetId(env.Compile(eplTwo));
                env.SendEventBean(new SupportBean());

                AssertSelect(env, idOne, 1);
                AssertSelect(env, idTwo, 2);

                env.Undeploy(idOne);
                Assert.IsNull(env.Runtime.DeploymentService.GetStatement(idOne, "s0"));

                AssertSelect(env, idTwo, 2);

                env.Undeploy(idTwo);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }
    }
} // end of namespace