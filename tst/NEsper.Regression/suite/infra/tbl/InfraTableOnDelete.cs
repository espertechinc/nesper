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
    public class InfraTableOnDelete
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithFlow(execs);
            WithSecondaryIndexUpd(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSecondaryIndexUpd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraDeleteSecondaryIndexUpd());
            return execs;
        }

        public static IList<RegressionExecution> WithFlow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraDeleteFlow());
            return execs;
        }

        private static void AssertValues(
            RegressionEnvironment env,
            string keys,
            int?[] values)
        {
            var keyarr = keys.SplitCsv();
            Assert.AreEqual(keyarr.Length, values.Length);
            for (var i = 0; i < keyarr.Length; i++) {
                env.SendEventBean(new SupportBean_S0(0, keyarr[i]));
                var @event = env.Listener("s0").AssertOneGetNewAndReset();
                Assert.AreEqual(values[i], @event.Get("value"), "Failed for key '" + keyarr[i] + "'");
            }
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

        internal class InfraDeleteSecondaryIndexUpd : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create table MyTable as (pKey0 string primary key, " +
                    "pkey1 int primary key, thesum sum(long))",
                    path);
                env.CompileDeploy(
                    "into table MyTable select sum(LongPrimitive) as thesum from SupportBean group by TheString, IntPrimitive",
                    path);

                MakeSendSupportBean(env, "E1", 10, 2L);
                MakeSendSupportBean(env, "E2", 20, 3L);

                env.Milestone(0);

                MakeSendSupportBean(env, "E1", 11, 4L);
                MakeSendSupportBean(env, "E2", 21, 5L);

                env.CompileDeploy("create index MyIdx on MyTable(pKey0)", path);
                env.CompileDeploy(
                        "@Name('s0') on SupportBean_S0 select sum(thesum) as c0 from MyTable where pKey0=P00",
                        path)
                    .AddListener("s0");

                AssertSum(env, "E1,E2,E3", new long?[] {6L, 8L, null});

                MakeSendSupportBean(env, "E3", 30, 77L);
                MakeSendSupportBean(env, "E2", 21, 2L);

                AssertSum(env, "E1,E2,E3", new long?[] {6L, 10L, 77L});

                env.CompileDeploy(
                    "@Name('on-delete') on SupportBean_S1 delete from MyTable where pKey0=P10 and pkey1=Id",
                    path);

                env.SendEventBean(new SupportBean_S1(11, "E1")); // deletes {"E1", 11, 4L}
                AssertSum(env, "E1,E2,E3", new long?[] {2L, 10L, 77L});

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S1(20, "E2")); // deletes {"E2", 20, 3L}
                AssertSum(env, "E1,E2,E3", new long?[] {2L, 7L, 77L});

                env.UndeployAll();
            }

            private static void AssertSum(
                RegressionEnvironment env,
                string listOfP00,
                long?[] sums)
            {
                var p00s = listOfP00.SplitCsv();
                Assert.AreEqual(p00s.Length, sums.Length);
                for (var i = 0; i < p00s.Length; i++) {
                    env.SendEventBean(new SupportBean_S0(0, p00s[i]));
                    Assert.AreEqual(sums[i], env.Listener("s0").AssertOneGetNewAndReset().Get("c0"));
                }
            }
        }

        internal class InfraDeleteFlow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = new[] {"key", "thesum"};
                env.CompileDeploy("create table varagg as (key string primary key, thesum sum(int))", path);
                env.CompileDeploy(
                    "into table varagg select sum(IntPrimitive) as thesum from SupportBean group by TheString",
                    path);
                env.CompileDeploy("@Name('s0') select varagg[P00].thesum as value from SupportBean_S0", path)
                    .AddListener("s0");
                env.CompileDeploy("@Name('sdf') on SupportBean_S1(Id = 1) delete from varagg where key = P10", path)
                    .AddListener("sdf");
                env.CompileDeploy("@Name('sda') on SupportBean_S1(Id = 2) delete from varagg", path).AddListener("sda");

                object[][] expectedType = {
                    new object[] {"key", typeof(string)},
                    new object[] {"thesum", typeof(int?)}
                };
                SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedType,
                    env.Statement("sda").EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE);

                env.SendEventBean(new SupportBean("G1", 10));
                AssertValues(env, "G1,G2", new int?[] {10, null});

                env.SendEventBean(new SupportBean("G2", 20));
                AssertValues(env, "G1,G2", new int?[] {10, 20});

                env.SendEventBean(new SupportBean_S1(1, "G1"));
                AssertValues(env, "G1,G2", new int?[] {null, 20});
                EPAssertionUtil.AssertProps(
                    env.Listener("sdf").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(2, null));
                AssertValues(env, "G1,G2", new int?[] {null, null});
                EPAssertionUtil.AssertProps(
                    env.Listener("sda").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 20});

                env.UndeployAll();
            }
        }
    }
} // end of namespace