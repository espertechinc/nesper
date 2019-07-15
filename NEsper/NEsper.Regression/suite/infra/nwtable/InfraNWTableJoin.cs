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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableJoin
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraNWTableJoinSimple(true));
            execs.Add(new InfraNWTableJoinSimple(false));
            return execs;
        }

        public class InfraNWTableJoinSimple : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraNWTableJoinSimple(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                string[] fields = {"c0", "c1"};
                var path = new RegressionPath();

                // create window
                var stmtTextCreate = "create schema MyEvent(cId string);\n";
                stmtTextCreate += namedWindow
                    ? "create window MyInfra.win:keepall() as MyEvent"
                    : "create table MyInfra(cId string primary key)";
                env.CompileDeployWBusPublicType(stmtTextCreate, path);

                // create insert into
                var stmtTextInsert = "insert into MyInfra select * from MyEvent";
                env.CompileDeploy(stmtTextInsert, path);

                // create join
                var stmtTextJoin =
                    "@Name('s0') select ce.cId as c0, sb.IntPrimitive as c1 from MyInfra as ce, SupportBean#keepall() as sb" +
                    " where sb.TheString = ce.cId";
                env.CompileDeploy(stmtTextJoin, path).AddListener("s0");

                SendMyEvent(env, "C1");
                SendMyEvent(env, "C2");
                SendMyEvent(env, "C3");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("C2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"C2", 1});

                env.SendEventBean(new SupportBean("C1", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"C1", 4});

                env.UndeployAll();
            }

            private void SendMyEvent(
                RegressionEnvironment env,
                string c1)
            {
                env.SendEventMap(Collections.SingletonDataMap("cId", c1), "MyEvent");
            }
        }
    }
} // end of namespace