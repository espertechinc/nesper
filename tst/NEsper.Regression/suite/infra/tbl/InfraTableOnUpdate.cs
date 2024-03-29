///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    /// NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableOnUpdate
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithTwoKey(execs);
            WithMultikeyWArrayOneArray(execs);
            WithMultikeyWArrayTwoArray(execs);
            WithMultikeyWArrayTwoArrayNonGetter(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayTwoArrayNonGetter(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableOnUpdateMultikeyWArrayTwoArrayNonGetter());
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayTwoArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableOnUpdateMultikeyWArrayTwoArray());
            return execs;
        }

        public static IList<RegressionExecution> WithMultikeyWArrayOneArray(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableOnUpdateMultikeyWArrayOneArray());
            return execs;
        }

        public static IList<RegressionExecution> WithTwoKey(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableOnUpdateTwoKey());
            return execs;
        }

        private class InfraTableOnUpdateMultikeyWArrayTwoArrayNonGetter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('tbl') create table MyTable(k1 int[primitive] primary key, k2 int[primitive] primary key, v int);\n" +
                    "on SupportBean_S0 update MyTable set v = Id where k1 = toIntArray(P00) and k2 = toIntArray(P01);\n" +
                    "on SupportEventWithManyArray(Id like 'I%') insert into MyTable select IntOne as k1, IntTwo as k2, Value as v;\n";
                env.CompileDeploy(epl);

                SendManyArray(env, "I1", new int[] { 1 }, new int[] { 1, 2 }, 10);
                SendManyArray(env, "I2", new int[] { 1 }, new int[] { 1, 2, 3 }, 20);
                SendManyArray(env, "I3", new int[] { 2 }, new int[] { 1 }, 30);
                SendS0(env, "1", "1, 2, 3", 21);

                env.Milestone(0);

                SendS0(env, "1", "1,2", 11);
                SendS0(env, "2", "1", 31);
                SendS0(env, "1", "1", 99);
                SendS0(env, "2", "1, 2", 99);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "tbl",
                    "k1,k2,v".SplitCsv(),
                    new object[][] {
                        new object[] { new int[] { 1 }, new int[] { 1, 2 }, 11 },
                        new object[] { new int[] { 1 }, new int[] { 1, 2, 3 }, 21 },
                        new object[] { new int[] { 2 }, new int[] { 1 }, 31 }
                    });

                env.UndeployAll();
            }

            private void SendS0(
                RegressionEnvironment env,
                string p00,
                string p01,
                int id)
            {
                env.SendEventBean(new SupportBean_S0(id, p00, p01));
            }

            private void SendManyArray(
                RegressionEnvironment env,
                string id,
                int[] intOne,
                int[] intTwo,
                int value)
            {
                env.SendEventBean(
                    new SupportEventWithManyArray(id).WithIntOne(intOne).WithIntTwo(intTwo).WithValue(value));
            }
        }

        private class InfraTableOnUpdateMultikeyWArrayTwoArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('tbl') create table MyTable(k1 int[primitive] primary key, k2 int[primitive] primary key, v int);\n" +
                    "on SupportEventWithManyArray(Id like 'U%') update MyTable set v = Value where k1 = IntOne and k2 = IntTwo;\n" +
                    "on SupportEventWithManyArray(Id like 'I%') insert into MyTable select IntOne as k1, IntTwo as k2, Value as v;\n";
                env.CompileDeploy(epl);

                SendManyArray(env, "I1", new int[] { 1 }, new int[] { 1, 2 }, 10);
                SendManyArray(env, "I2", new int[] { 1 }, new int[] { 1, 2, 3 }, 20);
                SendManyArray(env, "I3", new int[] { 2 }, new int[] { 1 }, 30);
                SendManyArray(env, "U2", new int[] { 1 }, new int[] { 1, 2, 3 }, 21);

                env.Milestone(0);

                SendManyArray(env, "U1", new int[] { 1 }, new int[] { 1, 2 }, 11);
                SendManyArray(env, "U3", new int[] { 2 }, new int[] { 1 }, 31);
                SendManyArray(env, "U4", new int[] { 1 }, new int[] { 1 }, 99);
                SendManyArray(env, "U5", new int[] { 2 }, new int[] { 1, 2 }, 99);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "tbl",
                    "k1,k2,v".SplitCsv(),
                    new object[][] {
                        new object[] { new int[] { 1 }, new int[] { 1, 2 }, 11 },
                        new object[] { new int[] { 1 }, new int[] { 1, 2, 3 }, 21 },
                        new object[] { new int[] { 2 }, new int[] { 1 }, 31 }
                    });

                env.UndeployAll();
            }

            private void SendManyArray(
                RegressionEnvironment env,
                string id,
                int[] intOne,
                int[] intTwo,
                int value)
            {
                env.SendEventBean(
                    new SupportEventWithManyArray(id).WithIntOne(intOne).WithIntTwo(intTwo).WithValue(value));
            }
        }

        private class InfraTableOnUpdateMultikeyWArrayOneArray : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('tbl') create table MyTable(k1 int[primitive] primary key, v int);\n" +
                          "on SupportEventWithManyArray(Id like 'U%') update MyTable set v = Value where k1 = IntOne;\n" +
                          "on SupportEventWithManyArray(Id like 'I%') insert into MyTable select IntOne as k1, Value as v;\n";
                env.CompileDeploy(epl);

                SendManyArray(env, "I1", new int[] { 1, 2 }, 10);
                SendManyArray(env, "I2", new int[] { 1, 2, 3 }, 20);
                SendManyArray(env, "I3", new int[] { 1 }, 30);
                SendManyArray(env, "U2", new int[] { 1, 2, 3 }, 21);

                env.Milestone(0);

                SendManyArray(env, "U1", new int[] { 1, 2 }, 11);
                SendManyArray(env, "U3", new int[] { 1 }, 31);
                SendManyArray(env, "U4", new int[] { }, 99);
                SendManyArray(env, "U5", new int[] { 1, 2, 4 }, 99);

                env.AssertPropsPerRowIteratorAnyOrder(
                    "tbl",
                    "k1,v".SplitCsv(),
                    new object[][] {
                        new object[] { new int[] { 1, 2 }, 11 }, new object[] { new int[] { 1, 2, 3 }, 21 },
                        new object[] { new int[] { 1 }, 31 }
                    });

                env.UndeployAll();
            }

            private void SendManyArray(
                RegressionEnvironment env,
                string id,
                int[] ints,
                int value)
            {
                env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(ints).WithValue(value));
            }
        }

        private class InfraTableOnUpdateTwoKey : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "keyOne,keyTwo,p0".SplitCsv();
                var path = new RegressionPath();

                env.CompileDeploy(
                    "@public create table varagg as (" +
                    "keyOne string primary key, keyTwo int primary key, p0 long)",
                    path);
                env.CompileDeploy(
                    "on SupportBean merge varagg where TheString = keyOne and " +
                    "IntPrimitive = keyTwo when not matched then insert select TheString as keyOne, IntPrimitive as keyTwo, 1 as p0",
                    path);
                env.CompileDeploy("@name('s0') select varagg[P00, Id].p0 as value from SupportBean_S0", path)
                    .AddListener("s0");
                env.CompileDeploy(
                        "@name('update') on SupportTwoKeyEvent update varagg set p0 = NewValue " +
                        "where K1 = keyOne and K2 = keyTwo",
                        path)
                    .AddListener("update");

                var expectedType = new object[][] {
                    new object[] { "keyOne", typeof(string) }, new object[] { "keyTwo", typeof(int?) },
                    new object[] { "p0", typeof(long?) }
                };
                env.AssertStatement(
                    "update",
                    statement => {
                        var updateStmtEventType = statement.EventType;
                        SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                            expectedType,
                            updateStmtEventType,
                            SupportEventTypeAssertionEnum.NAME,
                            SupportEventTypeAssertionEnum.TYPE);
                    });

                env.SendEventBean(new SupportBean("G1", 10));
                AssertValues(env, new object[][] { new object[] { "G1", 10 } }, new long?[] { 1L });

                env.Milestone(0);

                env.SendEventBean(new SupportTwoKeyEvent("G1", 10, 2));
                AssertValues(env, new object[][] { new object[] { "G1", 10 } }, new long?[] { 2L });
                env.AssertPropsIRPair("update", fields, new object[] { "G1", 10, 2L }, new object[] { "G1", 10, 1L });

                // try property method invocation
                env.CompileDeploy("@public create table MyTableSuppBean as (sb SupportBean)", path);
                env.CompileDeploy("on SupportBean_S0 update MyTableSuppBean sb set sb.SetLongPrimitive(10)", path);
                env.UndeployAll();
            }
        }

        private static void AssertValues(
            RegressionEnvironment env,
            object[][] keys,
            long?[] values)
        {
            ClassicAssert.AreEqual(keys.Length, values.Length);
            for (var i = 0; i < keys.Length; i++) {
                env.SendEventBean(new SupportBean_S0(keys[i][1].AsInt32(), (string)keys[i][0]));
                var index = i;
                env.AssertEventNew(
                    "s0",
                    @event => ClassicAssert.AreEqual(
                        values[index],
                        @event.Get("value"),
                        "Failed for key '" + CompatExtensions.RenderAny(keys[index]) + "'"));
            }
        }

        public static int[] ToIntArray(string text)
        {
            return text.SplitCsv()
                .Select(_ => int.Parse(_.Trim()))
                .ToArray();
        }
    }
} // end of namespace