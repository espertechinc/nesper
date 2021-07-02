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

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableContext
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithPartitioned(execs);
            WithNonOverlapping(execs);
            WithTableContextInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTableContextInvalid(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraTableContextInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithNonOverlapping(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraNonOverlapping());
            return execs;
        }

        public static IList<RegressionExecution> WithPartitioned(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new InfraPartitioned());
            return execs;
        }

        internal class InfraTableContextInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create context SimpleCtx start after 1 sec end after 1 sec", path);
                env.CompileDeploy(
                    "context SimpleCtx create table MyTable(pkey string primary key, thesum sum(int), col0 string)",
                    path);

                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "select * from MyTable",
                    "Table by name 'MyTable' has been declared for context 'SimpleCtx' and can only be used within the same context [");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "select (select * from MyTable) from SupportBean",
                    "Failed to plan subquery number 1 querying MyTable: Mismatch in context specification, the context for the table 'MyTable' is 'SimpleCtx' and the query specifies no context  [select (select * from MyTable) from SupportBean]");
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    "insert into MyTable select TheString as pkey from SupportBean",
                    "Table by name 'MyTable' has been declared for context 'SimpleCtx' and can only be used within the same context [");

                env.UndeployAll();
            }
        }

        internal class InfraNonOverlapping : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create context CtxNowTillS0 start @now end SupportBean_S0", path);
                env.CompileDeploy(
                    "context CtxNowTillS0 create table MyTable(pkey string primary key, thesum sum(int), col0 string)",
                    path);
                env.CompileDeploy(
                    "context CtxNowTillS0 into table MyTable select sum(IntPrimitive) as thesum from SupportBean group by TheString",
                    path);
                env.CompileDeploy(
                        "@Name('s0') context CtxNowTillS0 select pkey as c0, thesum as c1 from MyTable output snapshot when terminated",
                        path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 50));
                env.SendEventBean(new SupportBean("E2", 20));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 60));
                env.SendEventBean(new SupportBean_S0(-1)); // terminated
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    new[] {"c0", "c1"},
                    new[] {
                        new object[] {"E1", 110},
                        new object[] {"E2", 20}
                    });

                env.CompileDeploy("context CtxNowTillS0 create index MyIdx on MyTable(col0)", path);
                env.CompileDeploy("context CtxNowTillS0 select * from MyTable, SupportBean_S1 where col0 = P11", path);

                env.SendEventBean(new SupportBean("E3", 90));
                env.SendEventBean(new SupportBean("E1", 30));
                env.SendEventBean(new SupportBean("E3", 10));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(-1)); // terminated
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Listener("s0").GetAndResetLastNewData(),
                    new[] {"c0", "c1"},
                    new[] {
                        new object[] {"E1", 30},
                        new object[] {"E3", 100}
                    });

                env.UndeployAll();
            }
        }

        internal class InfraPartitioned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context CtxPerString " +
                    "partition by TheString from SupportBean, P00 from SupportBean_S0",
                    path);
                env.CompileDeploy("context CtxPerString create table MyTable(thesum sum(int))", path);
                env.CompileDeploy(
                    "context CtxPerString into table MyTable select sum(IntPrimitive) as thesum from SupportBean",
                    path);
                env.CompileDeploy(
                        "@Name('s0') context CtxPerString select MyTable.thesum as c0 from SupportBean_S0",
                        path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 50));
                env.SendEventBean(new SupportBean("E2", 20));
                env.SendEventBean(new SupportBean("E1", 60));
                env.SendEventBean(new SupportBean_S0(0, "E1"));
                Assert.AreEqual(110, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0, "E2"));
                Assert.AreEqual(20, env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));

                env.UndeployAll();
            }
        }
    }
} // end of namespace