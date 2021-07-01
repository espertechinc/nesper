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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    ///     NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableSubquery
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithAgainstKeyed(execs);
            WithAgainstUnkeyed(execs);
            WithSecondaryIndex(execs);
            WithInFilter(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInFilter(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableSubqueryInFilter());
            return execs;
        }

        public static IList<RegressionExecution> WithSecondaryIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableSubquerySecondaryIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithAgainstUnkeyed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableSubqueryAgainstUnkeyed());
            return execs;
        }

        public static IList<RegressionExecution> WithAgainstKeyed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableSubqueryAgainstKeyed());
            return execs;
        }

        private static void AssertValues(
            RegressionEnvironment env,
            string keys,
            int?[] values)
        {
            var keyarr = keys.SplitCsv();
            for (var i = 0; i < keyarr.Length; i++) {
                env.SendEventBean(new SupportBean_S0(0, keyarr[i]));
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(
                    values[i],
                    @event.Get("value"),
                    "Failed for key '" + keyarr[i] + "'");
            }
        }

        private static void SendInsertUpdate(
            RegressionEnvironment env,
            string p00,
            string p01,
            string p02,
            int value)
        {
            env.SendEventBean(new SupportBean_S0(value, p00, p01, p02));
        }

        private static void AssertSubselect(
            RegressionEnvironment env,
            string @string,
            int? expectedSum)
        {
            var fields = new[] {"c0"};
            env.SendEventBean(new SupportBean(@string, -1));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {expectedSum});
        }

        internal class InfraTableSubqueryInFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl = "create table MyTable(tablecol string primary key);\n" +
                             "insert into MyTable select P00 as tablecol from SupportBean_S0;\n" +
                             "@Name('s0') select * from SupportBean(TheString=(select tablecol from MyTable).orderBy().firstOf())";
                env.CompileDeploy(epl).AddListener("s0");

                SendAssert(env, "E", false);
                SendS0(env, "E");
                SendAssert(env, "E", true);
                SendS0(env, "C");
                SendAssert(env, "E", false);
                SendAssert(env, "C", true);

                env.Milestone(0);

                SendAssert(env, "A", false);
                SendAssert(env, "C", true);
                SendS0(env, "A");
                SendAssert(env, "A", true);
                SendAssert(env, "C", false);

                env.UndeployAll();
            }

            private void SendS0(
                RegressionEnvironment env,
                string p00)
            {
                env.SendEventBean(new SupportBean_S0(0, p00));
            }

            private void SendAssert(
                RegressionEnvironment env,
                string theString,
                bool expected)
            {
                env.SendEventBean(new SupportBean(theString, 0));
                Assert.AreEqual(expected, env.Listener("s0").IsInvokedAndReset());
            }
        }

        internal class InfraTableSubqueryAgainstKeyed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                env.CompileDeploy(
                    "create table varagg as (" +
                    "key string primary key, total sum(int))",
                    path);
                env.CompileDeploy(
                    "into table varagg " +
                    "select sum(IntPrimitive) as total from SupportBean group by TheString",
                    path);
                env.CompileDeploy(
                        "@Name('s0') select (select total from varagg where key = S0.P00) as value " +
                        "from SupportBean_S0 as S0",
                        path)
                    .AddListener("s0");

                env.SendEventBean(new SupportBean("G2", 200));
                AssertValues(env, "G1,G2", new int?[] {null, 200});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 100));
                AssertValues(env, "G1,G2", new int?[] {100, 200});

                env.UndeployAll();
            }
        }

        internal class InfraTableSubqueryAgainstUnkeyed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                env.CompileDeploy("create table InfraOne (string string, IntPrimitive int)", path);
                env.CompileDeploy(
                        "@Name('s0') select (select IntPrimitive from InfraOne where string = S0.P00) as c0 from SupportBean_S0 as S0",
                        path)
                    .AddListener("s0");
                env.CompileDeploy(
                    "insert into InfraOne select TheString as string, IntPrimitive from SupportBean",
                    path);

                env.SendEventBean(new SupportBean("E1", 10));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(0, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    new[] {"c0"},
                    new object[] {10});

                env.UndeployAll();
            }
        }

        internal class InfraTableSubquerySecondaryIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                var eplTable =
                    "create table MyTable(k0 string primary key, k1 string primary key, p2 string, value int)";
                env.CompileDeploy(eplTable, path);

                var eplIndex = "create index MyIndex on MyTable(p2)";
                env.CompileDeploy(eplIndex, path);

                var eplInto = "on SupportBean_S0 merge MyTable " +
                              "where P00 = k0 and P01 = k1 " +
                              "when not matched then insert select P00 as k0, P01 as k1, P02 as p2, Id as value " +
                              "when matched then update set p2 = P02, value = Id ";
                env.CompileDeploy(eplInto, path);

                var eplSubselect =
                    "@Name('s0') select (select value from MyTable as tbl where sb.TheString = tbl.p2) as c0 from SupportBean as sb";
                env.CompileDeploy(eplSubselect, path).AddListener("s0");

                SendInsertUpdate(env, "G1", "SG1", "P2_1", 10);
                AssertSubselect(env, "P2_1", 10);

                env.Milestone(0);

                SendInsertUpdate(env, "G1", "SG1", "P2_2", 11);

                env.Milestone(1);

                AssertSubselect(env, "P2_1", null);
                AssertSubselect(env, "P2_2", 11);

                env.UndeployAll();
            }
        }
    }
} // end of namespace