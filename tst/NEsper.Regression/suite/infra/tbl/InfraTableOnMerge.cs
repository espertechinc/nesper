///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.infra.tbl
{
    /// <summary>
    /// NOTE: More table-related tests in "nwtable"
    /// </summary>
    public class InfraTableOnMerge
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithTableOnMergeSimple(execs);
            WithOnMergePlainPropsAnyKeyed(execs);
            WithMergeWhereWithMethodRead(execs);
            WithMergeSelectWithAggReadAndEnum(execs);
            WithMergeTwoTables(execs);
            WithTableEMACompute(execs);
            WithTableArrayAssignmentBoxed(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithTableArrayAssignmentBoxed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableArrayAssignmentBoxed());
            return execs;
        }

        public static IList<RegressionExecution> WithTableEMACompute(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableEMACompute());
            return execs;
        }

        public static IList<RegressionExecution> WithMergeTwoTables(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraMergeTwoTables());
            return execs;
        }

        public static IList<RegressionExecution> WithMergeSelectWithAggReadAndEnum(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraMergeSelectWithAggReadAndEnum());
            return execs;
        }

        public static IList<RegressionExecution> WithMergeWhereWithMethodRead(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraMergeWhereWithMethodRead());
            return execs;
        }

        public static IList<RegressionExecution> WithOnMergePlainPropsAnyKeyed(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraOnMergePlainPropsAnyKeyed());
            return execs;
        }

        public static IList<RegressionExecution> WithTableOnMergeSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new InfraTableOnMergeSimple());
            return execs;
        }

        private class InfraTableArrayAssignmentBoxed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "create table MyTable(dbls double[]);\n" +
                    "@priority(2) on SupportBean merge MyTable when not matched then insert select new `System.Nullable<System.Double>`[3] as dbls;\n" +
                    "@priority(1) on SupportBean merge MyTable when matched then update set dbls[IntPrimitive] = 1;\n" +
                    "@name('s0') select MyTable.dbls as c0 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertEventNew(
                    "s0",
                    @event => CollectionAssert.AreEqual(
                        new double?[] { null, 1d, null },
                        @event.Get("c0").UnwrapIntoArray<double?>()));

                env.UndeployAll();
            }
        }

        private class InfraTableEMACompute : RegressionExecution
        {
            /// <summary>
            /// let p = 0.1
            /// a = average(x1, x2, x3, x4, x5)    // Assume 5, in reality use a parameter
            /// y1 = p * x1 + (p - 1) * a          // Recursive calculation initialized with look-ahead average
            /// y2 = p * x2 + (p - 1) * y1
            /// y3 = p * x3 + (p - 1) * y2
            /// ....
            /// The final stream should only publish y5, y6, y7, ...
            /// </summary>
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@public @buseventtype create schema MyEvent(Id string, x double);\n" +
                    "create constant variable int BURN_LENGTH = 5;\n" +
                    "create constant variable double ALPHA = 0.1;\n" +
                    "create table EMA(burnValues double[primitive], cnt int, value double);\n" +
                    "" +
                    "// Seed the row when the table is empty\n" +
                    "@priority(2) on MyEvent merge EMA\n" +
                    "  when not matched then insert select new double[BURN_LENGTH] as burnValues, 0 as cnt, null as value;\n" +
                    "" +
                    "inlined_class \"\"\"\n" +
                    "  public class Helper {\n" +
                    "    public static double ComputeInitialValue(double alpha, double[] burnValues) {\n" +
                    "      double total = 0;\n" +
                    "      for (int i = 0; i < burnValues.Length; i++) {\n" +
                    "        total = total + burnValues[i];\n" +
                    "      }\n" +
                    "      double value = total / burnValues.Length;\n" +
                    "      for (int i = 0; i < burnValues.Length; i++) {\n" +
                    "        value = alpha * burnValues[i] + (1 - alpha) * value;\n" +
                    "      }\n" +
                    "      return value;" +
                    "    }\n" +
                    "  }\n" +
                    "\"\"\"\n" +
                    "// Update the 'value' field with the current value\n" +
                    "@priority(1) on MyEvent merge EMA as ema\n" +
                    "  when matched and cnt < BURN_LENGTH - 1 then update set burnValues[cnt] = x, cnt = cnt + 1\n" +
                    "  when matched and cnt = BURN_LENGTH - 1 then update set burnValues[cnt] = x, cnt = cnt + 1, value = Helper.ComputeInitialValue(ALPHA, burnValues), burnValues = null\n" +
                    "  when matched then update set value = ALPHA * x + (1 - ALPHA) * value;\n" +
                    "" +
                    "// Output value\n" +
                    "@name('output') select EMA.value as burn from MyEvent;\n";
                env.CompileDeploy(epl).AddListener("output");

                SendAssertEMA(env, "E1", 1, null);

                SendAssertEMA(env, "E2", 2, null);

                SendAssertEMA(env, "E3", 3, null);

                SendAssertEMA(env, "E4", 4, null);

                // Last of the burn period
                // We expect:
                // a = (1+2+3+4+5) / 5 = 3
                // y1 = 0.1 * 1 + 0.9 * 3 = 2.8
                // y2 = 0.1 * 2 + 0.9 * 2.8
                //    ... leading to
                // y5 = 3.08588
                SendAssertEMA(env, "E5", 5, 3.08588);

                // Outside burn period
                SendAssertEMA(env, "E6", 6, 3.377292);

                env.Milestone(0);

                SendAssertEMA(env, "E7", 7, 3.7395628);

                SendAssertEMA(env, "E8", 8, 4.16560652);

                env.UndeployAll();
            }
        }

        private class InfraMergeTwoTables : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('T0') create table TableZero(k0 string primary key, v0 int);\n" +
                    "@name('T1') create table TableOne(k1 string primary key, v1 int);\n" +
                    "on SupportBean merge TableZero " +
                    "  where TheString = k0 when not matched " +
                    "  then insert select TheString as k0, IntPrimitive as v0" +
                    "  then insert into TableOne(k1, v1) select TheString, IntPrimitive;\n";
                env.CompileDeploy(epl);

                env.SendEventBean(new SupportBean("E1", 1));
                AssertTables(env, new object[][] { new object[] { "E1", 1 } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E2", 3));
                AssertTables(env, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });

                env.UndeployAll();
            }

            private void AssertTables(
                RegressionEnvironment env,
                object[][] expected)
            {
                env.AssertPropsPerRowIteratorAnyOrder("T0", "k0,v0".SplitCsv(), expected);
                env.AssertPropsPerRowIteratorAnyOrder("T1", "k1,v1".SplitCsv(), expected);
            }
        }

        private class InfraTableOnMergeSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var fields = "k1,v1".SplitCsv();

                env.CompileDeploy("@name('tbl') @public create table varaggKV (k1 string primary key, v1 int)", path);
                env.CompileDeploy(
                    "on SupportBean as sb merge varaggKV as va where sb.TheString = va.k1 " +
                    "when not matched then insert select TheString as k1, IntPrimitive as v1 " +
                    "when matched then update set v1 = IntPrimitive",
                    path);

                env.SendEventBean(new SupportBean("E1", 10));
                env.AssertPropsPerRowIterator("tbl", fields, new object[][] { new object[] { "E1", 10 } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 11));
                env.AssertPropsPerRowIterator("tbl", fields, new object[][] { new object[] { "E1", 11 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 100));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "tbl",
                    fields,
                    new object[][] { new object[] { "E1", 11 }, new object[] { "E2", 100 } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 101));
                env.AssertPropsPerRowIteratorAnyOrder(
                    "tbl",
                    fields,
                    new object[][] { new object[] { "E1", 11 }, new object[] { "E2", 101 } });

                env.UndeployAll();
            }
        }

        private class InfraMergeWhereWithMethodRead : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("@public create table varaggMMR (keyOne string primary key, cnt count(*))", path);
                env.CompileDeploy(
                    "into table varaggMMR select count(*) as cnt " +
                    "from SupportBean#lastevent group by TheString",
                    path);

                env.CompileDeploy("@name('s0') select varaggMMR[P00].keyOne as c0 from SupportBean_S0", path)
                    .AddListener("s0");
                env.CompileDeploy("on SupportBean_S1 merge varaggMMR where cnt = 0 when matched then delete", path);

                env.SendEventBean(new SupportBean("G1", 0));
                env.SendEventBean(new SupportBean("G2", 0));
                AssertKeyFound(env, "G1,G2,G3", new bool[] { true, true, false });

                env.SendEventBean(new SupportBean_S1(0)); // delete
                AssertKeyFound(env, "G1,G2,G3", new bool[] { false, true, false });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G3", 0));
                AssertKeyFound(env, "G1,G2,G3", new bool[] { false, true, true });

                env.SendEventBean(new SupportBean_S1(0)); // delete
                AssertKeyFound(env, "G1,G2,G3", new bool[] { false, false, true });

                env.UndeployAll();
            }
        }

        private class InfraMergeSelectWithAggReadAndEnum : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create table varaggMS (eventset window(*) @type(SupportBean), Total sum(int))",
                    path);
                env.CompileDeploy(
                    "into table varaggMS select window(*) as eventset, " +
                    "sum(IntPrimitive) as Total from SupportBean#length(2)",
                    path);
                env.CompileDeploy(
                    "@public on SupportBean_S0 merge varaggMS " +
                    "when matched then insert into ResultStream select eventset, Total, eventset.takeLast(1) as c0",
                    path);
                env.CompileDeploy("@name('s0') select * from ResultStream", path).AddListener("s0");

                var e1 = new SupportBean("E1", 15);
                env.SendEventBean(e1);

                AssertResultAggRead(env, new object[] { e1 }, 15);

                env.Milestone(0);

                var e2 = new SupportBean("E2", 20);
                env.SendEventBean(e2);

                AssertResultAggRead(env, new object[] { e1, e2 }, 35);

                env.Milestone(1);

                var e3 = new SupportBean("E3", 30);
                env.SendEventBean(e3);

                AssertResultAggRead(env, new object[] { e2, e3 }, 50);

                env.UndeployAll();
            }
        }

        private class InfraOnMergePlainPropsAnyKeyed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                RunOnMergeInsertUpdDeleteSingleKey(env, false, milestone);
                RunOnMergeInsertUpdDeleteSingleKey(env, true, milestone);

                RunOnMergeInsertUpdDeleteTwoKey(env, false, milestone);
                RunOnMergeInsertUpdDeleteTwoKey(env, true, milestone);

                RunOnMergeInsertUpdDeleteUngrouped(env, false, milestone);
                RunOnMergeInsertUpdDeleteUngrouped(env, true, milestone);
            }
        }

        private static void RunOnMergeInsertUpdDeleteUngrouped(
            RegressionEnvironment env,
            bool soda,
            AtomicLong milestone)
        {
            var path = new RegressionPath();
            var eplDeclare = "@public create table varaggIUD (p0 string, sumint sum(int))";
            env.CompileDeploy(soda, eplDeclare, path);

            var fields = "c0,c1".SplitCsv();
            var eplRead =
                "@name('s0') select varaggIUD.p0 as c0, varaggIUD.sumint as c1, varaggIUD as c2 from SupportBean_S0";
            env.CompileDeploy(soda, eplRead, path).AddListener("s0");

            // assert selected column types
            var expectedAggType = new object[][]
                { new object[] { "c0", typeof(string) }, new object[] { "c1", typeof(int?) } };
            env.AssertStatement(
                "s0",
                statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedAggType,
                    statement.EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE));

            // assert no row
            env.SendEventBean(new SupportBean_S0(0));
            env.AssertPropsNew("s0", fields, new object[] { null, null });

            // create merge
            var eplMerge = "on SupportBean merge varaggIUD" +
                           " when not matched then" +
                           " insert select TheString as p0" +
                           " when matched and TheString like \"U%\" then" +
                           " update set p0=\"updated\"" +
                           " when matched and TheString like \"D%\" then" +
                           " delete";
            env.CompileDeploy(soda, eplMerge, path);

            // merge for varagg
            env.SendEventBean(new SupportBean("E1", 0));

            // assert
            env.SendEventBean(new SupportBean_S0(0));
            env.AssertPropsNew("s0", fields, new object[] { "E1", null });

            // also aggregate-into the same key
            env.CompileDeploy(soda, "into table varaggIUD select sum(50) as sumint from SupportBean_S1", path);
            env.SendEventBean(new SupportBean_S1(0));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0));
            env.AssertPropsNew("s0", fields, new object[] { "E1", 50 });

            // update for varagg
            env.SendEventBean(new SupportBean("U2", 10));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0));
            env.AssertEventNew(
                "s0",
                received => {
                    EPAssertionUtil.AssertProps(received, fields, new object[] { "updated", 50 });
                    EPAssertionUtil.AssertPropsMap(
                        (IDictionary<string, object>)received.Get("c2"),
                        "p0,sumint".SplitCsv(),
                        new object[] { "updated", 50 });
                });

            // delete for varagg
            env.SendEventBean(new SupportBean("D3", 0));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0));
            env.AssertPropsNew("s0", fields, new object[] { null, null });

            env.UndeployAll();
        }

        private static void RunOnMergeInsertUpdDeleteSingleKey(
            RegressionEnvironment env,
            bool soda,
            AtomicLong milestone)
        {
            var fieldsTable = "key,p0,p1,p2,sumint".SplitCsv();
            var path = new RegressionPath();
            var eplDeclare =
                "@public create table varaggMIU (key int primary key, p0 string, p1 int, p2 int[], sumint sum(int))";
            env.CompileDeploy(soda, eplDeclare, path);

            var fields = "c0,c1,c2,c3".SplitCsv();
            var eplRead =
                "@name('s0') select varaggMIU[Id].p0 as c0, varaggMIU[Id].p1 as c1, varaggMIU[Id].p2 as c2, varaggMIU[Id].sumint as c3 from SupportBean_S0";
            env.CompileDeploy(soda, eplRead, path).AddListener("s0");

            // assert selected column types
            var expectedAggType = new object[][] {
                new object[] { "c0", typeof(string) },
                new object[] { "c1", typeof(int?) },
                new object[] { "c2", typeof(int?[]) },
                new object[] { "c3", typeof(int?) }
            };
            env.AssertStatement(
                "s0",
                statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedAggType,
                    statement.EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE));

            // assert no row
            env.SendEventBean(new SupportBean_S0(10));
            env.AssertPropsNew("s0", fields, new object[] { null, null, null, null });

            // create merge
            var eplMerge = "@name('merge') on SupportBean merge varaggMIU" +
                           " where IntPrimitive=key" +
                           " when not matched then" +
                           " insert select IntPrimitive as key, \"v1\" as p0, 1000 as p1, new object[] {1,2} as p2" +
                           " when matched and TheString like \"U%\" then" +
                           " update set p0=\"v2\", p1=2000, p2={3,4}" +
                           " when matched and TheString like \"D%\" then" +
                           " delete";
            env.CompileDeploy(soda, eplMerge, path).AddListener("merge");

            // merge for varagg[10]
            env.SendEventBean(new SupportBean("E1", 10));
            env.AssertPropsNew("merge", fieldsTable, new object[] { 10, "v1", 1000, new int?[] { 1, 2 }, null });

            // assert key "10"
            env.SendEventBean(new SupportBean_S0(10));
            env.AssertPropsNew("s0", fields, new object[] { "v1", 1000, new int?[] { 1, 2 }, null });

            // also aggregate-into the same key
            env.CompileDeploy(
                soda,
                "into table varaggMIU select sum(50) as sumint from SupportBean_S1 group by Id",
                path);
            env.SendEventBean(new SupportBean_S1(10));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(10));
            env.AssertPropsNew("s0", fields, new object[] { "v1", 1000, new int?[] { 1, 2 }, 50 });

            // update for varagg[10]
            env.SendEventBean(new SupportBean("U2", 10));
            env.AssertPropsIRPair(
                "merge",
                fieldsTable,
                new object[] { 10, "v2", 2000, new int?[] { 3, 4 }, 50 },
                new object[] { 10, "v1", 1000, new int?[] { 1, 2 }, 50 });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(10));
            env.AssertPropsNew("s0", fields, new object[] { "v2", 2000, new int?[] { 3, 4 }, 50 });

            // delete for varagg[10]
            env.SendEventBean(new SupportBean("D3", 10));
            env.AssertPropsOld("merge", fieldsTable, new object[] { 10, "v2", 2000, new int?[] { 3, 4 }, 50 });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(10));
            env.AssertPropsNew("s0", fields, new object[] { null, null, null, null });

            env.UndeployAll();
        }

        private static void RunOnMergeInsertUpdDeleteTwoKey(
            RegressionEnvironment env,
            bool soda,
            AtomicLong milestone)
        {
            var path = new RegressionPath();
            var eplDeclare =
                "@public create table varaggMIUD (keyOne int primary key, keyTwo string primary key, prop string)";
            env.CompileDeploy(soda, eplDeclare, path);

            var fields = "c0,c1,c2".SplitCsv();
            var eplRead =
                "@name('s0') select varaggMIUD[Id,P00].keyOne as c0, varaggMIUD[Id,P00].keyTwo as c1, varaggMIUD[Id,P00].prop as c2 from SupportBean_S0";
            env.CompileDeploy(soda, eplRead, path).AddListener("s0");

            // assert selected column types
            var expectedAggType = new object[][] {
                new object[] { "c0", typeof(int?) }, new object[] { "c1", typeof(string) },
                new object[] { "c2", typeof(string) }
            };
            env.AssertStatement(
                "s0",
                statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedAggType,
                    statement.EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE));

            // assert no row
            env.SendEventBean(new SupportBean_S0(10, "A"));
            env.AssertPropsNew("s0", fields, new object[] { null, null, null });

            // create merge
            var eplMerge = "@name('merge') on SupportBean merge varaggMIUD" +
                           " where IntPrimitive=keyOne and TheString=keyTwo" +
                           " when not matched then" +
                           " insert select IntPrimitive as keyOne, TheString as keyTwo, \"inserted\" as prop" +
                           " when matched and LongPrimitive>0 then" +
                           " update set prop=\"updated\"" +
                           " when matched and LongPrimitive<0 then" +
                           " delete";
            env.CompileDeploy(soda, eplMerge, path);
            var expectedType = new object[][] {
                new object[] { "keyOne", typeof(int?) }, new object[] { "keyTwo", typeof(string) },
                new object[] { "prop", typeof(string) }
            };
            env.AssertStatement(
                "merge",
                statement => SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                    expectedType,
                    statement.EventType,
                    SupportEventTypeAssertionEnum.NAME,
                    SupportEventTypeAssertionEnum.TYPE));

            // merge for varagg[10, "A"]
            env.SendEventBean(new SupportBean("A", 10));

            env.MilestoneInc(milestone);

            // assert key {"10", "A"}
            env.SendEventBean(new SupportBean_S0(10, "A"));
            env.AssertPropsNew("s0", fields, new object[] { 10, "A", "inserted" });

            // update for varagg[10, "A"]
            env.SendEventBean(MakeSupportBean("A", 10, 1));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(10, "A"));
            env.AssertPropsNew("s0", fields, new object[] { 10, "A", "updated" });

            // test typable output
            env.CompileDeploy(
                    "@name('convert') insert into LocalBean select varaggMIUD[10, 'A'] as val0 from SupportBean_S1",
                    path)
                .AddListener("convert");
            env.SendEventBean(new SupportBean_S1(2));
            env.AssertPropsNew("convert", "val0.keyOne".SplitCsv(), new object[] { 10 });

            // delete for varagg[10, "A"]
            env.SendEventBean(MakeSupportBean("A", 10, -1));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(10, "A"));
            env.AssertPropsNew("s0", fields, new object[] { null, null, null });

            env.UndeployAll();
        }

        private static void SendAssertEMA(
            RegressionEnvironment env,
            string id,
            double x,
            double? expected)
        {
            IDictionary<string, object> @event = new Dictionary<string, object>();
            @event.Put("Id", id);
            @event.Put("x", x);
            env.SendEventMap(@event, "MyEvent");
            env.AssertEventNew(
                "output",
                output => {
                    var burn = output.Get("burn").AsBoxedDouble();
                    if (expected == null) {
                        ClassicAssert.IsNull(burn);
                    }
                    else {
                        Assert.That(expected.Value, Is.EqualTo(burn).Within(1e-10));
                    }
                });
        }

        private static SupportBean MakeSupportBean(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        private static void AssertResultAggRead(
            RegressionEnvironment env,
            object[] objects,
            int total)
        {
            var fields = "eventset,Total".SplitCsv();
            env.SendEventBean(new SupportBean_S0(0));
            env.AssertEventNew(
                "s0",
                @event => {
                    EPAssertionUtil.AssertProps(@event, fields, new object[] { objects, total });
                    EPAssertionUtil.AssertEqualsExactOrder(
                        new object[] { objects[^1] },
                        ((ICollection<object>)@event.Get("c0")).ToArray());
                });
        }

        private static void AssertKeyFound(
            RegressionEnvironment env,
            string keyCsv,
            bool[] expected)
        {
            var split = keyCsv.SplitCsv();
            for (var i = 0; i < split.Length; i++) {
                var key = split[i];
                env.SendEventBean(new SupportBean_S0(0, key));
                var expectedString = expected[i] ? key : null;
                env.AssertEventNew(
                    "s0",
                    @event => ClassicAssert.AreEqual(expectedString, @event.Get("c0"), "failed for key '" + key + "'"));
            }
        }

        public class LocalSubBean
        {
            private int keyOne;
            private string keyTwo;
            private string prop;

            public int KeyOne {
                get => keyOne;
                set => keyOne = value;
            }

            public string KeyTwo {
                get => keyTwo;
                set => keyTwo = value;
            }

            public string Prop {
                get => prop;
                set => prop = value;
            }
        }

        public class LocalBean
        {
            private LocalSubBean val0;

            public LocalSubBean Val0 {
                get => val0;
                set => val0 = value;
            }
        }
    }
} // end of namespace