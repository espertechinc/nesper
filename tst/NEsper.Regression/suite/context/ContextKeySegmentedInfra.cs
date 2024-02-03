///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
using NUnit.Framework.Legacy;


namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextKeySegmentedInfra
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSegmentedInfraAggregatedSubquery(execs);
            WithSegmentedInfraOnDeleteAndUpdate(execs);
            WithSegmentedInfraCreateIndex(execs);
            WithSegmentedInfraOnSelect(execs);
            WithSegmentedInfraNWConsumeAll(execs);
            WithSegmentedInfraNWConsumeSameContext(execs);
            WithSegmentedInfraOnMergeUpdateSubq(execs);
            WithedSegmentedTable(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithedSegmentedTable(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeyedSegmentedTable());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedInfraOnMergeUpdateSubq(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedInfraOnMergeUpdateSubq());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedInfraNWConsumeSameContext(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedInfraNWConsumeSameContext());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedInfraNWConsumeAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedInfraNWConsumeAll());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedInfraOnSelect(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedInfraOnSelect());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedInfraCreateIndex(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedInfraCreateIndex());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedInfraOnDeleteAndUpdate(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedInfraOnDeleteAndUpdate());
            return execs;
        }

        public static IList<RegressionExecution> WithSegmentedInfraAggregatedSubquery(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedInfraAggregatedSubquery());
            return execs;
        }

        private class ContextKeySegmentedInfraAggregatedSubquery : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionAggregatedSubquery(env, milestone, true);
                TryAssertionAggregatedSubquery(env, milestone, false);
            }
        }

        private class ContextKeySegmentedInfraOnDeleteAndUpdate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionOnDeleteAndUpdate(env, milestone, true);
                TryAssertionOnDeleteAndUpdate(env, milestone, false);
            }
        }

        private class ContextKeySegmentedInfraCreateIndex : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionCreateIndex(env, milestone, true);
                TryAssertionCreateIndex(env, milestone, false);
            }
        }

        private class ContextKeySegmentedInfraOnSelect : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                TryAssertionSegmentedOnSelect(env, milestone, true);
                TryAssertionSegmentedOnSelect(env, milestone, false);
            }
        }

        private class ContextKeySegmentedInfraNWConsumeAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                env.CompileDeploy(
                    "@name('named window') @public context SegmentedByString create window MyWindow#lastevent as SupportBean",
                    path);
                env.AddListener("named window");
                env.CompileDeploy("@name('insert') insert into MyWindow select * from SupportBean", path);

                env.CompileDeploy("@name('s0') select * from MyWindow", path).AddListener("s0");

                var fields = new string[] { "TheString", "IntPrimitive" };
                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("named window", fields, new object[] { "G1", 10 });
                env.AssertPropsNew("s0", fields, new object[] { "G1", 10 });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 20));
                env.AssertPropsNew("named window", fields, new object[] { "G2", 20 });
                env.AssertPropsNew("s0", fields, new object[] { "G2", 20 });

                env.Milestone(1);

                env.UndeployModuleContaining("s0");

                // Out-of-context consumer not initialized
                env.CompileDeploy("@name('s0') select count(*) as cnt from MyWindow", path);
                env.AssertIterator(
                    "s0",
                    iterator => EPAssertionUtil.AssertProps(iterator.Advance(), "cnt".SplitCsv(), new object[] { 0L }));

                env.UndeployAll();
            }
        }

        private class ContextKeySegmentedInfraNWConsumeSameContext : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                env.CompileDeploy(
                    "@name('named window') @public context SegmentedByString create window MyWindow#keepall as SupportBean",
                    path);
                env.AddListener("named window");
                env.CompileDeploy("@name('insert') insert into MyWindow select * from SupportBean", path);

                var fieldsNW = new string[] { "TheString", "IntPrimitive" };
                var fieldsCnt = new string[] { "TheString", "cnt" };
                env.CompileDeploy(
                    "@name('select') context SegmentedByString select TheString, count(*) as cnt from MyWindow group by TheString",
                    path);
                env.AddListener("select");

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("named window", fieldsNW, new object[] { "G1", 10 });
                env.AssertPropsNew("select", fieldsCnt, new object[] { "G1", 1L });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 20));
                env.AssertPropsNew("named window", fieldsNW, new object[] { "G2", 20 });
                env.AssertPropsNew("select", fieldsCnt, new object[] { "G2", 1L });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G1", 11));
                env.AssertPropsNew("named window", fieldsNW, new object[] { "G1", 11 });
                env.AssertPropsNew("select", fieldsCnt, new object[] { "G1", 2L });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G2", 21));
                env.AssertPropsNew("named window", fieldsNW, new object[] { "G2", 21 });
                env.AssertPropsNew("select", fieldsCnt, new object[] { "G2", 2L });

                env.UndeployModuleContaining("select");

                // In-context consumer not initialized
                env.CompileDeploy(
                    "@name('select') context SegmentedByString select count(*) as cnt from MyWindow",
                    path);
                env.AddListener("select");
                env.AssertThat(
                    () => {
                        try {
                            env.Statement("select").GetEnumerator();
                        }
                        catch (UnsupportedOperationException ex) {
                            ClassicAssert.AreEqual(
                                "Iterator not supported on statements that have a context attached",
                                ex.Message);
                        }
                    });
                env.UndeployAll();
            }
        }

        private class ContextKeySegmentedInfraOnMergeUpdateSubq : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('context') @public create context SegmentedByString " +
                          "partition by TheString from SupportBean, P00 from SupportBean_S0, P10 from SupportBean_S1;\n";
                epl +=
                    "@name('named window') @public context SegmentedByString create window MyWindow#keepall as SupportBean;\n";
                epl += "@name('insert') insert into MyWindow select * from SupportBean;\n";
                epl += "@name('on-merge') context SegmentedByString " +
                       "on SupportBean_S0 " +
                       "merge MyWindow " +
                       "when matched then " +
                       "  update set IntPrimitive = (select Id from SupportBean_S1#lastevent)";
                env.CompileDeploy(epl).AddListener("named window").AddListener("on-merge");

                var fieldsNW = new string[] { "TheString", "IntPrimitive" };

                env.SendEventBean(new SupportBean("G1", 1));
                env.AssertPropsNew("named window", fieldsNW, new object[] { "G1", 1 });

                env.SendEventBean(new SupportBean_S1(99, "G1"));
                env.SendEventBean(new SupportBean_S0(0, "G1"));
                env.AssertPropsIRPair("named window", fieldsNW, new object[] { "G1", 99 }, new object[] { "G1", 1 });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 2));
                env.AssertPropsNew("named window", fieldsNW, new object[] { "G2", 2 });

                env.SendEventBean(new SupportBean_S1(98, "Gx"));
                env.SendEventBean(new SupportBean_S0(0, "G2"));
                env.AssertPropsIRPair("named window", fieldsNW, new object[] { "G2", 2 }, new object[] { "G2", 2 });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G3", 3));
                env.AssertPropsNew("named window", fieldsNW, new object[] { "G3", 3 });

                env.SendEventBean(new SupportBean_S0(0, "Gx"));
                env.AssertListenerNotInvoked("named window");

                env.UndeployAll();
            }
        }

        public class ContextKeyedSegmentedTable : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);
                env.CompileDeploy(
                    "@name('table') @public context SegmentedByString " +
                    "create table MyTable(TheString string, IntPrimitive int primary key)",
                    path);
                env.CompileDeploy(
                    "@name('insert') context SegmentedByString insert into MyTable select TheString, IntPrimitive from SupportBean",
                    path);

                env.SendEventBean(new SupportBean("G1", 10));
                AssertValues(env, "G1", new object[][] { new object[] { "G1", 10 } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G2", 20));
                AssertValues(env, "G1", new object[][] { new object[] { "G1", 10 } });
                AssertValues(env, "G2", new object[][] { new object[] { "G2", 20 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G1", 11));

                env.Milestone(2);

                AssertValues(env, "G1", new object[][] { new object[] { "G1", 10 }, new object[] { "G1", 11 } });
                AssertValues(env, "G2", new object[][] { new object[] { "G2", 20 } });

                env.SendEventBean(new SupportBean("G2", 21));

                env.Milestone(3);

                AssertValues(env, "G1", new object[][] { new object[] { "G1", 10 }, new object[] { "G1", 11 } });
                AssertValues(env, "G2", new object[][] { new object[] { "G2", 20 }, new object[] { "G2", 21 } });

                env.UndeployAll();
            }

            private void AssertValues(
                RegressionEnvironment env,
                string group,
                object[][] expected)
            {
                env.AssertStatement(
                    "table",
                    statement => {
                        var it =
                            statement.GetEnumerator(
                                new ProxyContextPartitionSelectorSegmented(
                                    () => Collections.SingletonList(new object[] { group })));
                        EPAssertionUtil.AssertPropsPerRowAnyOrder(it, "TheString,IntPrimitive".SplitCsv(), expected);
                    });
            }
        }

        private static void TryAssertionSegmentedOnSelect(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool namedWindow)
        {
            var path = new RegressionPath();
            env.CompileDeploy(
                "@name('context') @public create context SegmentedByString " +
                "partition by TheString from SupportBean, P00 from SupportBean_S0",
                path);

            var eplCreate = namedWindow
                ? "@name('named window') @public context SegmentedByString create window MyInfra#keepall as SupportBean"
                : "@name('table') @public context SegmentedByString create table MyInfra(TheString string primary key, IntPrimitive int primary key)";
            env.CompileDeploy(eplCreate, path);
            env.CompileDeploy(
                "@name('insert') context SegmentedByString insert into MyInfra select TheString, IntPrimitive from SupportBean",
                path);

            var fieldsNW = new string[] { "TheString", "IntPrimitive" };
            env.CompileDeploy(
                "@name('s0') context SegmentedByString " +
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
            env.AssertPropsPerRowLastNewAnyOrder(
                "s0",
                fieldsNW,
                new object[][] { new object[] { "G1", 1 }, new object[] { "G1", 3 } });

            env.SendEventBean(new SupportBean_S0(0, "G2"));
            env.AssertPropsPerRowLastNewAnyOrder("s0", fieldsNW, new object[][] { new object[] { "G2", 2 } });

            env.UndeployAll();
        }

        private static void TryAssertionCreateIndex(
            RegressionEnvironment env,
            AtomicLong milestone,
            bool namedWindow)
        {
            var path = new RegressionPath();
            var epl = "@name('create-ctx') @public create context SegmentedByCustomer " +
                      "  initiated by SupportBean_S0 s0 " +
                      "  terminated by SupportBean_S1(P00 = P10);" +
                      "" +
                      "@name('create-infra') @public context SegmentedByCustomer\n" +
                      (namedWindow
                          ? "create window MyInfra#keepall as SupportBean;"
                          : "create table MyInfra(TheString string primary key, IntPrimitive int);") +
                      "" +
                      (namedWindow
                          ? "@name('insert-into-window') insert into MyInfra select TheString, IntPrimitive from SupportBean;"
                          : "@name('insert-into-table') context SegmentedByCustomer insert into MyInfra select TheString, IntPrimitive from SupportBean;") +
                      "" +
                      "@name('create-index') context SegmentedByCustomer create index MyIndex on MyInfra(IntPrimitive);";
            env.CompileDeploy(epl, path);

            env.SendEventBean(new SupportBean_S0(1, "A"));
            env.SendEventBean(new SupportBean_S0(2, "B"));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E1", 1));

            env.AssertThat(
                () => {
                    var faf = env.CompileFAF("select * from MyInfra where IntPrimitive = 1", path);
                    var result = env.Runtime.FireAndForgetService.ExecuteQuery(
                        faf,
                        new ContextPartitionSelector[] { new SupportSelectorById(1) });
                    EPAssertionUtil.AssertPropsPerRow(
                        result.Array,
                        "TheString,IntPrimitive".SplitCsv(),
                        new object[][] { new object[] { "E1", 1 } });
                });

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
                "@name('context') @public create context SegmentedByString " +
                "partition by TheString from SupportBean, P00 from SupportBean_S0, P10 from SupportBean_S1",
                path);

            var fieldsNW = new string[] { "TheString", "IntPrimitive" };
            var eplCreate = namedWindow
                ? "@name('named window') @public context SegmentedByString create window MyInfra#keepall as SupportBean"
                : "@name('named window') @public context SegmentedByString create table MyInfra(TheString string primary key, IntPrimitive int primary key)";
            env.CompileDeploy(eplCreate, path);
            var eplInsert = namedWindow
                ? "@name('insert') insert into MyInfra select TheString, IntPrimitive from SupportBean"
                : "@name('insert') context SegmentedByString insert into MyInfra select TheString, IntPrimitive from SupportBean";
            env.CompileDeploy(eplInsert, path);

            env.CompileDeploy("@name('s0') context SegmentedByString select irstream * from MyInfra", path)
                .AddListener("s0");

            // Delete testing
            env.CompileDeploy(
                "@name('on-delete') context SegmentedByString on SupportBean_S0 delete from MyInfra",
                path);

            env.SendEventBean(new SupportBean("G1", 1));
            if (namedWindow) {
                env.AssertPropsNew("s0", fieldsNW, new object[] { "G1", 1 });
            }
            else {
                env.AssertListenerNotInvoked("s0");
            }

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "G0"));
            env.SendEventBean(new SupportBean_S0(0, "G2"));
            env.AssertListenerNotInvoked("s0");

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "G1"));
            if (namedWindow) {
                env.AssertPropsOld("s0", fieldsNW, new object[] { "G1", 1 });
            }

            env.SendEventBean(new SupportBean("G2", 20));
            if (namedWindow) {
                env.AssertPropsNew("s0", fieldsNW, new object[] { "G2", 20 });
            }

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("G3", 3));
            if (namedWindow) {
                env.AssertPropsNew("s0", fieldsNW, new object[] { "G3", 3 });
            }

            env.SendEventBean(new SupportBean("G2", 21));
            if (namedWindow) {
                env.AssertPropsNew("s0", fieldsNW, new object[] { "G2", 21 });
            }

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "G2"));
            if (namedWindow) {
                env.AssertListener(
                    "s0",
                    listener => EPAssertionUtil.AssertPropsPerRow(
                        listener.LastOldData,
                        fieldsNW,
                        new object[][] { new object[] { "G2", 20 }, new object[] { "G2", 21 } }));
            }

            env.ListenerReset("s0");

            env.UndeployModuleContaining("on-delete");

            // update testing
            env.CompileDeploy(
                "@name('on-merge') context SegmentedByString on SupportBean_S0 update MyInfra set IntPrimitive = IntPrimitive + 1",
                path);

            env.SendEventBean(new SupportBean("G4", 4));
            if (namedWindow) {
                env.AssertPropsNew("s0", fieldsNW, new object[] { "G4", 4 });
            }

            env.SendEventBean(new SupportBean_S0(0, "G0"));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "G1"));
            env.SendEventBean(new SupportBean_S0(0, "G2"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(0, "G4"));
            if (namedWindow) {
                env.AssertPropsIRPair("s0", fieldsNW, new object[] { "G4", 5 }, new object[] { "G4", 4 });
            }

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("G5", 5));
            if (namedWindow) {
                env.AssertPropsNew("s0", fieldsNW, new object[] { "G5", 5 });
            }

            env.SendEventBean(new SupportBean_S0(0, "G5"));
            if (namedWindow) {
                env.AssertPropsIRPair("s0", fieldsNW, new object[] { "G5", 6 }, new object[] { "G5", 5 });
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
                ? "@public context SegmentedByString create window MyInfra#keepall as SupportBean;\n"
                : "@public context SegmentedByString create table MyInfra (TheString string primary key, IntPrimitive int);\n";
            epl +=
                "@name('insert') context SegmentedByString insert into MyInfra select TheString, IntPrimitive from SupportBean;\n";
            epl += "@Audit @name('s0') context SegmentedByString " +
                   "select *, (select max(IntPrimitive) from MyInfra) as mymax from SupportBean_S0;\n";
            env.CompileDeploy(epl).AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 10));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean("E2", 20));

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "E2"));
            env.AssertPropsNew("s0", "mymax".SplitCsv(), new object[] { 20 });

            env.SendEventBean(new SupportBean_S0(0, "E1"));
            env.AssertPropsNew("s0", "mymax".SplitCsv(), new object[] { 10 });

            env.MilestoneInc(milestone);

            env.SendEventBean(new SupportBean_S0(0, "E3"));
            env.AssertPropsNew("s0", "mymax".SplitCsv(), new object[] { null });

            env.UndeployAll();
        }
    }
} // end of namespace