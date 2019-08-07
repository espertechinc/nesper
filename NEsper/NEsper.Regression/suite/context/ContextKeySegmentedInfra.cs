///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextKeySegmentedInfra
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedInfraAggregatedSubquery());
            execs.Add(new ContextKeySegmentedInfraOnDeleteAndUpdate());
            execs.Add(new ContextKeySegmentedInfraCreateIndex());
            execs.Add(new ContextKeySegmentedInfraOnSelect());
            execs.Add(new ContextKeySegmentedInfraNWConsumeAll());
            execs.Add(new ContextKeySegmentedInfraNWConsumeSameContext());
            execs.Add(new ContextKeySegmentedInfraOnMergeUpdateSubq());
            execs.Add(new ContextKeyedSegmentedTable());
            return execs;
        }

        private static void TryAssertionSegmentedOnSelect(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool namedWindow)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@Name('context') create context SegmentedByString " +
                "partition by TheString from SupportBean, P00 from SupportBean_S0",
                path);

            var eplCreate = namedWindow
                ? "@Name('named window') context SegmentedByString create window MyInfra#keepall as SupportBean"
                : "@Name('table') context SegmentedByString create table MyInfra(TheString string primary key, IntPrimitive int primary key)";
            env.CompileDeploy(eplCreate, path);
            env.CompileDeploy(
                "@Name('insert') context SegmentedByString insert into MyInfra select TheString, IntPrimitive from SupportBean",
                path);

            string[] fieldsNW = {"TheString", "IntPrimitive"};
            env.CompileDeploy(
                "@Name('s0') context SegmentedByString " +
                "on SupportBean_S0 select mywin.* from MyInfra as mywin",
                path);
            env.AddListener("s0");

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("G1", 1));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("G2", 2));
            env.SendEventBean(new SupportBean("G1", 3));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "G1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Listener("s0").GetAndResetLastNewData(),
                fieldsNW,
                new[] {new object[] {"G1", 1}, new object[] {"G1", 3}});

            env.SendEventBean(new SupportBean_S0(0, "G2"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(
                env.Listener("s0").GetAndResetLastNewData(),
                fieldsNW,
                new[] {new object[] {"G2", 2}});

            env.UndeployAll();
        }

        private static void TryAssertionCreateIndex(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool namedWindow)
        {
            var path = new RegressionPath();
            var epl = "@Name('create-ctx') create context SegmentedByCustomer " +
                      "  initiated by SupportBean_S0 s0 " +
                      "  terminated by SupportBean_S1(P00 = P10);" +
                      "" +
                      "@Name('create-infra') context SegmentedByCustomer\n" +
                      (namedWindow
                          ? "create window MyInfra#keepall as SupportBean;"
                          : "create table MyInfra(TheString string primary key, IntPrimitive int);") +
                      "" +
                      (namedWindow
                          ? "@Name('insert-into-window') insert into MyInfra select TheString, IntPrimitive from SupportBean;"
                          : "@Name('insert-into-table') context SegmentedByCustomer insert into MyInfra select TheString, IntPrimitive from SupportBean;"
                      ) +
                      "" +
                      "@Name('create-index') context SegmentedByCustomer create index MyIndex on MyInfra(IntPrimitive);";
            env.CompileDeploy(epl, path);

            env.SendEventBean(new SupportBean_S0(1, "A"));
            env.SendEventBean(new SupportBean_S0(2, "B"));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E1", 1));

            var faf = env.CompileFAF("select * from MyInfra where IntPrimitive = 1", path);
            var result = env.Runtime.FireAndForgetService.ExecuteQuery(
                faf,
                new ContextPartitionSelector[] {new SupportSelectorById(1)});
            EPAssertionUtil.AssertPropsPerRow(
                result.Array,
                "theString,IntPrimitive".SplitCsv(),
                new[] {new object[] {"E1", 1}});

            env.SendEventBean(new SupportBean_S1(3, "A"));

            env.UndeployAll();
        }

        private static void TryAssertionOnDeleteAndUpdate(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool namedWindow)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@Name('context') create context SegmentedByString " +
                "partition by TheString from SupportBean, P00 from SupportBean_S0, P10 from SupportBean_S1",
                path);

            string[] fieldsNW = {"TheString", "IntPrimitive"};
            var eplCreate = namedWindow
                ? "@Name('named window') context SegmentedByString create window MyInfra#keepall as SupportBean"
                : "@Name('named window') context SegmentedByString create table MyInfra(TheString string primary key, IntPrimitive int primary key)";
            env.CompileDeploy(eplCreate, path);
            var eplInsert = namedWindow
                ? "@Name('insert') insert into MyInfra select TheString, IntPrimitive from SupportBean"
                : "@Name('insert') context SegmentedByString insert into MyInfra select TheString, IntPrimitive from SupportBean";
            env.CompileDeploy(eplInsert, path);

            env.CompileDeploy("@Name('s0') context SegmentedByString select irstream * from MyInfra", path)
                .AddListener("s0");

            // Delete testing
            env.CompileDeploy(
                "@Name('on-delete') context SegmentedByString on SupportBean_S0 delete from MyInfra",
                path);

            env.SendEventBean(new SupportBean("G1", 1));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G1", 1});
            }
            else {
                Assert.IsFalse(env.Listener("s0").IsInvoked);
            }

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "G0"));
            env.SendEventBean(new SupportBean_S0(0, "G2"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "G1"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetOldAndReset(),
                    fieldsNW,
                    new object[] {"G1", 1});
            }

            env.SendEventBean(new SupportBean("G2", 20));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G2", 20});
            }

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("G3", 3));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G3", 3});
            }

            env.SendEventBean(new SupportBean("G2", 21));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G2", 21});
            }

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "G2"));
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").LastOldData,
                    fieldsNW,
                    new[] {new object[] {"G2", 20}, new object[] {"G2", 21}});
            }

            env.Listener("s0").Reset();

            env.UndeployModuleContaining("on-delete");

            // update testing
            env.CompileDeploy(
                "@Name('on-merge') context SegmentedByString on SupportBean_S0 update MyInfra set IntPrimitive = IntPrimitive + 1",
                path);

            env.SendEventBean(new SupportBean("G4", 4));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G4", 4});
            }

            env.SendEventBean(new SupportBean_S0(0, "G0"));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "G1"));
            env.SendEventBean(new SupportBean_S0(0, "G2"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S0(0, "G4"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsNW,
                    new object[] {"G4", 5});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fieldsNW,
                    new object[] {"G4", 4});
                env.Listener("s0").Reset();
            }

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("G5", 5));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G5", 5});
            }

            env.SendEventBean(new SupportBean_S0(0, "G5"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsNW,
                    new object[] {"G5", 6});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fieldsNW,
                    new object[] {"G5", 5});
                env.Listener("s0").Reset();
            }

            env.UndeployModuleContaining("on-merge");
            env.UndeployAll();
        }

        private static void TryAssertionAggregatedSubquery(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool namedWindow)
        {
            var epl = "";
            epl +=
                "create context SegmentedByString partition by TheString from SupportBean, P00 from SupportBean_S0;\n";
            epl += namedWindow
                ? "context SegmentedByString create window MyInfra#keepall as SupportBean;\n"
                : "context SegmentedByString create table MyInfra (TheString string primary key, IntPrimitive int);\n";
            epl +=
                "@Name('insert') context SegmentedByString insert into MyInfra select TheString, IntPrimitive from SupportBean;\n";
            epl += "@Audit @Name('s0') context SegmentedByString " +
                   "select *, (select max(IntPrimitive) from MyInfra) as mymax from SupportBean_S0;\n";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 20));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "E2"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "mymax".SplitCsv(),
                new object[] {20});

            env.SendEventBean(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "mymax".SplitCsv(),
                new object[] {10});

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "E3"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                "mymax".SplitCsv(),
                new object[] {null});

            env.UndeployAll();
        }

        internal class ContextKeySegmentedInfraAggregatedSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionAggregatedSubquery(env, milestone, true);
                TryAssertionAggregatedSubquery(env, milestone, false);
            }
        }

        internal class ContextKeySegmentedInfraOnDeleteAndUpdate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionOnDeleteAndUpdate(env, milestone, true);
                TryAssertionOnDeleteAndUpdate(env, milestone, false);
            }
        }

        internal class ContextKeySegmentedInfraCreateIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionCreateIndex(env, milestone, true);
                TryAssertionCreateIndex(env, milestone, false);
            }
        }

        internal class ContextKeySegmentedInfraOnSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionSegmentedOnSelect(env, milestone, true);
                TryAssertionSegmentedOnSelect(env, milestone, false);
            }
        }

        internal class ContextKeySegmentedInfraNWConsumeAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by TheString from SupportBean",
                    path);

                env.CompileDeploy(
                    "@Name('named window') context SegmentedByString create window MyWindow#lastevent as SupportBean",
                    path);
                env.AddListener("named window");
                env.CompileDeploy("@Name('insert') insert into MyWindow select * from SupportBean", path);

                env.CompileDeploy("@Name('s0') select * from MyWindow", path).AddListener("s0");

                string[] fields = {"TheString", "IntPrimitive"};
                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 20});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 20});

                env.Milestone(1);

                env.UndeployModuleContaining("s0");

                // Out-of-context consumer not initialized
                env.CompileDeploy("@Name('s0') select count(*) as cnt from MyWindow", path);
                EPAssertionUtil.AssertProps(
                    env.GetEnumerator("s0").Advance(),
                    "cnt".SplitCsv(),
                    new object[] {0L});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedInfraNWConsumeSameContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by TheString from SupportBean",
                    path);

                env.CompileDeploy(
                    "@Name('named window') context SegmentedByString create window MyWindow#keepall as SupportBean",
                    path);
                env.AddListener("named window");
                env.CompileDeploy("@Name('insert') insert into MyWindow select * from SupportBean", path);

                string[] fieldsNW = {"TheString", "IntPrimitive"};
                string[] fieldsCnt = {"TheString", "cnt"};
                env.CompileDeploy(
                    "@Name('select') context SegmentedByString select TheString, count(*) as cnt from MyWindow group by TheString",
                    path);
                env.AddListener("select");

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G1", 10});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fieldsCnt,
                    new object[] {"G1", 1L});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G2", 20});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fieldsCnt,
                    new object[] {"G2", 1L});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G1", 11});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fieldsCnt,
                    new object[] {"G1", 2L});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G2", 21));
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G2", 21});
                EPAssertionUtil.AssertProps(
                    env.Listener("select").AssertOneGetNewAndReset(),
                    fieldsCnt,
                    new object[] {"G2", 2L});

                env.UndeployModuleContaining("select");

                // In-context consumer not initialized
                env.CompileDeploy(
                    "@Name('select') context SegmentedByString select count(*) as cnt from MyWindow",
                    path);
                env.AddListener("select");
                try {
                    env.Statement("select").GetEnumerator();
                }
                catch (UnsupportedOperationException ex) {
                    Assert.AreEqual("Iterator not supported on statements that have a context attached", ex.Message);
                }

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedInfraOnMergeUpdateSubq : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('context') create context SegmentedByString " +
                          "partition by TheString from SupportBean, P00 from SupportBean_S0, P10 from SupportBean_S1;\n";
                epl +=
                    "@Name('named window') context SegmentedByString create window MyWindow#keepall as SupportBean;\n";
                epl += "@Name('insert') insert into MyWindow select * from SupportBean;\n";
                epl += "@Name('on-merge') context SegmentedByString " +
                       "on SupportBean_S0 " +
                       "merge MyWindow " +
                       "when matched then " +
                       "  update set IntPrimitive = (select Id from SupportBean_S1#lastevent)";
                env.CompileDeploy(epl).AddListener("named window").AddListener("on-merge");

                string[] fieldsNW = {"TheString", "IntPrimitive"};

                env.SendEventBean(new SupportBean("G1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G1", 1});

                env.SendEventBean(new SupportBean_S1(99, "G1"));
                env.SendEventBean(new SupportBean_S0(0, "G1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").LastNewData[0],
                    fieldsNW,
                    new object[] {"G1", 99});
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").LastOldData[0],
                    fieldsNW,
                    new object[] {"G1", 1});
                env.Listener("named window").Reset();

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G2", 2});

                env.SendEventBean(new SupportBean_S1(98, "Gx"));
                env.SendEventBean(new SupportBean_S0(0, "G2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").LastNewData[0],
                    fieldsNW,
                    new object[] {"G2", 2});
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").LastOldData[0],
                    fieldsNW,
                    new object[] {"G2", 2});
                env.Listener("named window").Reset();

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("named window").AssertOneGetNewAndReset(),
                    fieldsNW,
                    new object[] {"G3", 3});

                env.SendEventBean(new SupportBean_S0(0, "Gx"));
                Assert.IsFalse(env.Listener("named window").IsInvoked);

                env.UndeployAll();
            }
        }

        public class ContextKeyedSegmentedTable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by TheString from SupportBean",
                    path);
                env.CompileDeploy(
                    "@Name('table') context SegmentedByString " +
                    "create table MyTable(TheString string, IntPrimitive int primary key)",
                    path);
                env.CompileDeploy(
                    "@Name('insert') context SegmentedByString insert into MyTable select TheString, IntPrimitive from SupportBean",
                    path);

                env.SendEventBean(new SupportBean("G1", 10));
                AssertValues(
                    env,
                    "G1",
                    new[] {new object[] {"G1", 10}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 20));
                AssertValues(
                    env,
                    "G1",
                    new[] {new object[] {"G1", 10}});
                AssertValues(
                    env,
                    "G2",
                    new[] {new object[] {"G2", 20}});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G1", 11));

                env.Milestone(2);

                AssertValues(
                    env,
                    "G1",
                    new[] {new object[] {"G1", 10}, new object[] {"G1", 11}});
                AssertValues(
                    env,
                    "G2",
                    new[] {new object[] {"G2", 20}});

                env.SendEventBean(new SupportBean("G2", 21));

                env.Milestone(3);

                AssertValues(
                    env,
                    "G1",
                    new[] {new object[] {"G1", 10}, new object[] {"G1", 11}});
                AssertValues(
                    env,
                    "G2",
                    new[] {new object[] {"G2", 20}, new object[] {"G2", 21}});

                env.UndeployAll();
            }

            private void AssertValues(
                RegressionEnvironment env,
                string group,
                object[][] expected)
            {
                var it = env.Statement("table")
                    .GetEnumerator(
                        new ProxyContextPartitionSelectorSegmented {
                            ProcPartitionKeys = () => { return Collections.SingletonList(new object[] {group}); }
                        });
                EPAssertionUtil.AssertPropsPerRowAnyOrder(it, "theString,IntPrimitive".SplitCsv(), expected);
            }
        }
    }
} // end of namespace