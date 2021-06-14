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

namespace com.espertech.esper.regressionlib.suite.infra.nwtable
{
    public class InfraNWTableContext
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new InfraContext(true));
            execs.Add(new InfraContext(false));
            return execs;
        }

        private static void Register(
            RegressionEnvironment env,
            RegressionPath path,
            int num,
            string epl)
        {
            env.CompileDeploy("@Name('s" + num + "')" + epl, path).AddListener("s" + num);
        }

        private static void MakeSendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var b = new SupportBean(theString, intPrimitive);
            b.LongPrimitive = longPrimitive;
            env.SendEventBean(b);
        }

        internal class InfraContext : RegressionExecution
        {
            private readonly bool namedWindow;

            public InfraContext(bool namedWindow)
            {
                this.namedWindow = namedWindow;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create context ContextOne start SupportBean_S0 end SupportBean_S1", path);

                var eplCreate = namedWindow
                    ? "context ContextOne create window MyInfra#keepall as (pkey0 string, pkey1 int, c0 long)"
                    : "context ContextOne create table MyInfra as (pkey0 string primary key, pkey1 int primary key, c0 long)";
                env.CompileDeploy(eplCreate, path);

                env.CompileDeploy(
                    "context ContextOne insert into MyInfra select " +
                    " TheString as pkey0," +
                    " IntPrimitive as pkey1," +
                    " LongPrimitive as c0" +
                    " from SupportBean",
                    path);

                env.SendEventBean(new SupportBean_S0(0)); // start

                MakeSendSupportBean(env, "E1", 10, 100);
                MakeSendSupportBean(env, "E2", 20, 200);

                Register(
                    env,
                    path,
                    1,
                    "context ContextOne select * from MyInfra output snapshot when terminated");
                Register(
                    env,
                    path,
                    2,
                    "context ContextOne select count(*) as thecnt from MyInfra output snapshot when terminated");
                Register(
                    env,
                    path,
                    3,
                    "context ContextOne select pkey0, count(*) as thecnt from MyInfra output snapshot when terminated");
                Register(
                    env,
                    path,
                    4,
                    "context ContextOne select pkey0, count(*) as thecnt from MyInfra group by pkey0 output snapshot when terminated");
                Register(
                    env,
                    path,
                    5,
                    "context ContextOne select pkey0, pkey1, count(*) as thecnt from MyInfra group by pkey0 output snapshot when terminated");
                Register(
                    env,
                    path,
                    6,
                    "context ContextOne select pkey0, pkey1, count(*) as thecnt from MyInfra group by rollup (pkey0, pkey1) output snapshot when terminated");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(0)); // end

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s1").GetAndResetLastNewData(),
                    new[] {"pkey0", "pkey1", "c0"},
                    new[] {
                        new object[] {"E1", 10, 100L}, new object[] {"E2", 20, 200L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    new[] {"thecnt"},
                    new object[] {
                        2L
                    });
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s3").GetAndResetLastNewData(),
                    new[] {"pkey0", "thecnt"},
                    new[] {
                        new object[] {"E1", 2L}, new object[] {"E2", 2L}
                    });
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s4").GetAndResetLastNewData(),
                    new[] {"pkey0", "thecnt"},
                    new[] {
                        new object[] {"E1", 1L}, new object[] {"E2", 1L}
                    });
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s5").GetAndResetLastNewData(),
                    new[] {"pkey0", "pkey1", "thecnt"},
                    new[] {
                        new object[] {"E1", 10, 1L}, new object[] {"E2", 20, 1L}
                    });
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s6").GetAndResetLastNewData(),
                    new[] {"pkey0", "pkey1", "thecnt"},
                    new[] {
                        new object[] {"E1", 10, 1L}, new object[] {"E2", 20, 1L}, new object[] {"E1", null, 1L},
                        new object[] {"E2", null, 1L}, new object[] {null, null, 2L}
                    });

                env.UndeployAll();
            }
        }
    }
} // end of namespace