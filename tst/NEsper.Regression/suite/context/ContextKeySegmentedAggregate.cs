///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;


namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextKeySegmentedAggregate
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithRowForAll(execs);
            WithAccessOnly(execs);
            WithSubqueryWithAggregation(execs);
            WithRowPerGroupStream(execs);
            WithRowPerGroupBatchContextProp(execs);
            WithRowPerGroupWithAccess(execs);
            WithRowPerGroupUnidirectionalJoin(execs);
            WithRowPerEvent(execs);
            WithRowPerGroup3Stmts(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroup3Stmts(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedRowPerGroup3Stmts());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedRowPerEvent());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupUnidirectionalJoin(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedRowPerGroupUnidirectionalJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupWithAccess(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedRowPerGroupWithAccess());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupBatchContextProp(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedRowPerGroupBatchContextProp());
            return execs;
        }

        public static IList<RegressionExecution> WithRowPerGroupStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedRowPerGroupStream());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryWithAggregation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedSubqueryWithAggregation());
            return execs;
        }

        public static IList<RegressionExecution> WithAccessOnly(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedAccessOnly());
            return execs;
        }

        public static IList<RegressionExecution> WithRowForAll(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedRowForAll());
            return execs;
        }

        private class ContextKeySegmentedAccessOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContext =
                    "@name('CTX') @public create context SegmentedByString partition by TheString from SupportBean";
                env.CompileDeploy(eplContext, path);

                var fieldsGrouped = "TheString,IntPrimitive,col1".SplitCsv();
                var eplGroupedAccess =
                    "@name('s0') context SegmentedByString select TheString,IntPrimitive,window(LongPrimitive) as col1 from SupportBean#keepall sb group by IntPrimitive";
                env.CompileDeploy(eplGroupedAccess, path);

                env.AddListener("s0");

                env.SendEventBean(MakeEvent("G1", 1, 10L));
                env.AssertPropsNew("s0", fieldsGrouped, new object[] { "G1", 1, new long?[] { 10L } });

                env.Milestone(0);

                env.SendEventBean(MakeEvent("G1", 2, 100L));
                env.AssertPropsNew("s0", fieldsGrouped, new object[] { "G1", 2, new long?[] { 100L } });

                env.SendEventBean(MakeEvent("G2", 1, 200L));
                env.AssertPropsNew("s0", fieldsGrouped, new object[] { "G2", 1, new long?[] { 200L } });

                env.Milestone(1);

                env.SendEventBean(MakeEvent("G1", 1, 11L));
                env.AssertPropsNew("s0", fieldsGrouped, new object[] { "G1", 1, new long?[] { 10L, 11L } });

                env.UndeployAll();
            }
        }

        private class ContextKeySegmentedSubqueryWithAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fields = new string[] { "TheString", "IntPrimitive", "val0" };
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select TheString, IntPrimitive, (select count(*) from SupportBean_S0#keepall as s0 where sb.IntPrimitive = s0.Id) as val0 " +
                    "from SupportBean as sb",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10, "s1"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fields, new object[] { "G1", 10, 0L });

                env.UndeployAll();
            }
        }

        private class ContextKeySegmentedRowPerGroupStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fieldsOne = "IntPrimitive,count(*)".SplitCsv();
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString select IntPrimitive, count(*) from SupportBean group by IntPrimitive",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 10, 1L });

                env.SendEventBean(new SupportBean("G2", 200));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 200, 1L });

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 10, 2L });

                env.SendEventBean(new SupportBean("G1", 11));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 11, 1L });

                env.SendEventBean(new SupportBean("G2", 200));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 200, 2L });

                env.SendEventBean(new SupportBean("G2", 10));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 10, 1L });

                env.UndeployModuleContaining("s0");

                // add "string" : a context property
                var fieldsTwo = "TheString,IntPrimitive,count(*)".SplitCsv();
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString select TheString, IntPrimitive, count(*) from SupportBean group by IntPrimitive",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "G1", 10, 1L });

                env.SendEventBean(new SupportBean("G2", 200));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "G2", 200, 1L });

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "G1", 10, 2L });

                env.SendEventBean(new SupportBean("G1", 11));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "G1", 11, 1L });

                env.SendEventBean(new SupportBean("G2", 200));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "G2", 200, 2L });

                env.SendEventBean(new SupportBean("G2", 10));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "G2", 10, 1L });

                env.UndeployAll();
            }
        }

        private class ContextKeySegmentedRowPerGroupBatchContextProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fieldsOne = "IntPrimitive,count(*)".SplitCsv();
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString select IntPrimitive, count(*) from SupportBean#length_batch(2) group by IntPrimitive order by IntPrimitive asc",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G2", 200));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 11));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fieldsOne,
                    new object[][] { new object[] { 10, 1L }, new object[] { 11, 1L } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G2", 200));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 200, 2L });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fieldsOne,
                    new object[][] { new object[] { 10, 2L }, new object[] { 11, 0L } });

                env.Milestone(4);

                env.SendEventBean(new SupportBean("G2", 10));
                env.SendEventBean(new SupportBean("G2", 10));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fieldsOne,
                    new object[][] { new object[] { 10, 2L }, new object[] { 200, 0L } });

                env.UndeployModuleContaining("s0");

                // add "string" : add context property
                var fieldsTwo = "TheString,IntPrimitive,count(*)".SplitCsv();
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString select TheString, IntPrimitive, count(*) from SupportBean#length_batch(2) group by IntPrimitive order by TheString, IntPrimitive asc",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G2", 200));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(5);

                env.SendEventBean(new SupportBean("G1", 11));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fieldsTwo,
                    new object[][] { new object[] { "G1", 10, 1L }, new object[] { "G1", 11, 1L } });

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertListenerNotInvoked("s0");

                env.Milestone(6);

                env.SendEventBean(new SupportBean("G2", 200));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { "G2", 200, 2L });

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fieldsTwo,
                    new object[][] { new object[] { "G1", 10, 2L }, new object[] { "G1", 11, 0L } });

                env.Milestone(7);

                env.SendEventBean(new SupportBean("G2", 10));
                env.SendEventBean(new SupportBean("G2", 10));
                env.AssertPropsPerRowNewOnly(
                    "s0",
                    fieldsTwo,
                    new object[][] { new object[] { "G2", 10, 2L }, new object[] { "G2", 200, 0L } });

                env.UndeployAll();
            }
        }

        private class ContextKeySegmentedRowPerGroupWithAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fieldsOne = "IntPrimitive,col1,col2,col3".SplitCsv();
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select IntPrimitive, count(*) as col1, toArray(window(*).selectFrom(v=>v.LongPrimitive)) as col2, first().LongPrimitive as col3 " +
                    "from SupportBean#keepall as sb " +
                    "group by IntPrimitive order by IntPrimitive asc",
                    path);
                env.AddListener("s0");

                env.SendEventBean(MakeEvent("G1", 10, 200L));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 10, 1L, new object[] { 200L }, 200L });

                env.SendEventBean(MakeEvent("G1", 10, 300L));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 10, 2L, new object[] { 200L, 300L }, 200L });

                env.Milestone(0);

                env.SendEventBean(MakeEvent("G2", 10, 1000L));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 10, 1L, new object[] { 1000L }, 1000L });

                env.Milestone(1);

                env.SendEventBean(MakeEvent("G2", 10, 1010L));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 10, 2L, new object[] { 1000L, 1010L }, 1000L });

                env.UndeployModuleContaining("s0");
                env.UndeployAll();
            }
        }

        private class ContextKeySegmentedRowForAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var fieldsOne = "col1".SplitCsv();
                var path = new RegressionPath();

                var eplCtx =
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean";
                env.CompileDeploy(eplCtx, path);

                var epl = "@name('s0') context SegmentedByString select sum(IntPrimitive) as col1 from SupportBean;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("G1", 3));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 3 });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("G2", 2));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 2 });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("G1", 4));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 7 });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("G2", 1));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 3 });

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("G3", -1));
                env.AssertPropsNew("s0", fieldsOne, new object[] { -1 });

                env.MilestoneInc(milestone);

                env.UndeployModuleContaining("s0");

                // test mixed with access
                var fieldsTwo = "col1,col2".SplitCsv();
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select sum(IntPrimitive) as col1, toArray(window(*).selectFrom(v=>v.IntPrimitive)) as col2 " +
                    "from SupportBean#keepall",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 8));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { 8, new object[] { 8 } });

                env.SendEventBean(new SupportBean("G2", 5));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { 5, new object[] { 5 } });

                env.SendEventBean(new SupportBean("G1", 1));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { 9, new object[] { 8, 1 } });

                env.SendEventBean(new SupportBean("G2", 2));
                env.AssertPropsNew("s0", fieldsTwo, new object[] { 7, new object[] { 5, 2 } });

                env.UndeployModuleContaining("s0");

                // test only access
                var fieldsThree = "col1".SplitCsv();
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select toArray(window(*).selectFrom(v=>v.IntPrimitive)) as col1 " +
                    "from SupportBean#keepall",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 8));
                env.AssertPropsNew("s0", fieldsThree, new object[] { new object[] { 8 } });

                env.SendEventBean(new SupportBean("G2", 5));
                env.AssertPropsNew("s0", fieldsThree, new object[] { new object[] { 5 } });

                env.SendEventBean(new SupportBean("G1", 1));
                env.AssertPropsNew("s0", fieldsThree, new object[] { new object[] { 8, 1 } });

                env.SendEventBean(new SupportBean("G2", 2));
                env.AssertPropsNew("s0", fieldsThree, new object[] { new object[] { 5, 2 } });

                env.UndeployModuleContaining("s0");

                // test subscriber
                env.CompileDeploy(
                        "@name('s0') context SegmentedByString " +
                        "select count(*) as col1 " +
                        "from SupportBean",
                        path)
                    .SetSubscriber("s0");

                env.SendEventBean(new SupportBean("G1", 1));
                env.AssertSubscriber("s0", subs => ClassicAssert.AreEqual(1L, subs.AssertOneGetNewAndReset()));

                env.SendEventBean(new SupportBean("G1", 1));
                env.AssertSubscriber("s0", subs => ClassicAssert.AreEqual(2L, subs.AssertOneGetNewAndReset()));

                env.SendEventBean(new SupportBean("G2", 2));
                env.AssertSubscriber("s0", subs => ClassicAssert.AreEqual(1L, subs.AssertOneGetNewAndReset()));

                env.UndeployAll();
            }
        }

        private class ContextKeySegmentedRowPerGroupUnidirectionalJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fieldsOne = "IntPrimitive,col1".SplitCsv();
                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select IntPrimitive, count(*) as col1 " +
                    "from SupportBean unidirectional, SupportBean_S0#keepall " +
                    "group by IntPrimitive order by IntPrimitive asc",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S0(2));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 10, 2L });

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(3));

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 10, 3L });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G2", 20));
                env.SendEventBean(new SupportBean_S0(4));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("G2", 20));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 20, 1L });

                env.SendEventBean(new SupportBean_S0(5));

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G2", 20));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 20, 2L });

                env.SendEventBean(new SupportBean("G1", 10));
                env.AssertPropsNew("s0", fieldsOne, new object[] { 10, 5L });

                env.UndeployModuleContaining("s0");
                env.UndeployAll();
            }
        }

        public class ContextKeySegmentedRowPerEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContext =
                    "@name('CTX') @public create context SegmentedByString partition by TheString from SupportBean";
                env.CompileDeploy(eplContext, path);

                var fields = "TheString,col1".SplitCsv();
                var eplUngrouped =
                    "@name('S1') context SegmentedByString select TheString,sum(IntPrimitive) as col1 from SupportBean";
                env.CompileDeploy(eplUngrouped, path).AddListener("S1");

                var eplGroupedAccess =
                    "@name('S2') context SegmentedByString select TheString,window(IntPrimitive) as col1 from SupportBean#keepall() sb";
                env.CompileDeploy(eplGroupedAccess, path).AddListener("S2");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 2));
                env.AssertPropsNew("S1", fields, new object[] { "G1", 2 });
                env.AssertPropsNew("S2", fields, new object[] { "G1", new int?[] { 2 } });

                env.SendEventBean(new SupportBean("G1", 3));
                env.AssertPropsNew("S1", fields, new object[] { "G1", 5 });
                env.AssertPropsNew("S2", fields, new object[] { "G1", new int?[] { 2, 3 } });
                AssertPartitionInfo(env);

                env.Milestone(1);

                AssertPartitionInfo(env);
                env.SendEventBean(new SupportBean("G2", 10));
                env.AssertPropsNew("S1", fields, new object[] { "G2", 10 });
                env.AssertPropsNew("S2", fields, new object[] { "G2", new int?[] { 10 } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G2", 11));
                env.AssertPropsNew("S1", fields, new object[] { "G2", 21 });
                env.AssertPropsNew("S2", fields, new object[] { "G2", new int?[] { 10, 11 } });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("G1", 4));
                env.AssertPropsNew("S1", fields, new object[] { "G1", 9 });
                env.AssertPropsNew("S2", fields, new object[] { "G1", new int?[] { 2, 3, 4 } });

                env.Milestone(4);

                env.SendEventBean(new SupportBean("G3", 100));
                env.AssertPropsNew("S1", fields, new object[] { "G3", 100 });
                env.AssertPropsNew("S2", fields, new object[] { "G3", new int?[] { 100 } });

                env.Milestone(5);

                env.SendEventBean(new SupportBean("G3", 101));
                env.AssertPropsNew("S1", fields, new object[] { "G3", 201 });
                env.AssertPropsNew("S2", fields, new object[] { "G3", new int?[] { 100, 101 } });

                env.UndeployModuleContaining("S1");
                env.UndeployModuleContaining("S2");
                env.UndeployModuleContaining("CTX");
            }

            private void AssertPartitionInfo(RegressionEnvironment env)
            {
                env.AssertThat(
                    () => {
                        var partitionAdmin = env.Runtime.ContextPartitionService;
                        var partitions = partitionAdmin.GetContextPartitions(
                            env.DeploymentId("CTX"),
                            "SegmentedByString",
                            ContextPartitionSelectorAll.INSTANCE);
                        ClassicAssert.AreEqual(1, partitions.Identifiers.Count);
                        var ident = (ContextPartitionIdentifierPartitioned)partitions.Identifiers.Values.First();
                        EPAssertionUtil.AssertEqualsExactOrder(new string[] { "G1" }, ident.Keys);
                    });
            }
        }

        public class ContextKeySegmentedRowPerGroup3Stmts : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContext =
                    "@name('CTX') @public create context SegmentedByString partition by TheString from SupportBean";
                env.CompileDeploy(eplContext, path);

                var fields = "TheString,IntPrimitive,col1".SplitCsv();
                var eplGrouped =
                    "@name('S1') context SegmentedByString select TheString,IntPrimitive,sum(LongPrimitive) as col1 from SupportBean group by IntPrimitive";
                env.CompileDeploy(eplGrouped, path).AddListener("S1");

                var eplGroupedAccess =
                    "@name('S2') context SegmentedByString select TheString,IntPrimitive,window(LongPrimitive) as col1 from SupportBean.win:keepall() sb group by IntPrimitive";
                env.CompileDeploy(eplGroupedAccess, path).AddListener("S2");

                var eplGroupedDistinct =
                    "@name('S3') context SegmentedByString select TheString,IntPrimitive,sum(distinct LongPrimitive) as col1 from SupportBean.win:keepall() sb group by IntPrimitive";
                env.CompileDeploy(eplGroupedDistinct, path).AddListener("S3");

                env.SendEventBean(MakeEvent("G1", 1, 10L));
                env.AssertPropsNew("S1", fields, new object[] { "G1", 1, 10L });
                env.AssertPropsNew("S2", fields, new object[] { "G1", 1, new long?[] { 10L } });
                env.AssertPropsNew("S3", fields, new object[] { "G1", 1, 10L });

                env.Milestone(0);

                env.SendEventBean(MakeEvent("G2", 1, 25L));
                env.AssertPropsNew("S1", fields, new object[] { "G2", 1, 25L });
                env.AssertPropsNew("S2", fields, new object[] { "G2", 1, new long?[] { 25L } });
                env.AssertPropsNew("S3", fields, new object[] { "G2", 1, 25L });

                env.Milestone(1);

                env.SendEventBean(MakeEvent("G1", 2, 2L));
                env.AssertPropsNew("S1", fields, new object[] { "G1", 2, 2L });
                env.AssertPropsNew("S2", fields, new object[] { "G1", 2, new long?[] { 2L } });
                env.AssertPropsNew("S3", fields, new object[] { "G1", 2, 2L });

                env.Milestone(2);

                env.SendEventBean(MakeEvent("G2", 2, 100L));
                env.AssertPropsNew("S1", fields, new object[] { "G2", 2, 100L });
                env.AssertPropsNew("S2", fields, new object[] { "G2", 2, new long?[] { 100L } });
                env.AssertPropsNew("S3", fields, new object[] { "G2", 2, 100L });

                env.Milestone(3);

                env.SendEventBean(MakeEvent("G1", 1, 10L));
                env.AssertPropsNew("S1", fields, new object[] { "G1", 1, 20L });
                env.AssertPropsNew("S2", fields, new object[] { "G1", 1, new long?[] { 10L, 10L } });
                env.AssertPropsNew("S3", fields, new object[] { "G1", 1, 10L });

                env.Milestone(4);

                env.SendEventBean(MakeEvent("G1", 2, 3L));
                env.AssertPropsNew("S1", fields, new object[] { "G1", 2, 5L });
                env.AssertPropsNew("S2", fields, new object[] { "G1", 2, new long?[] { 2L, 3L } });
                env.AssertPropsNew("S3", fields, new object[] { "G1", 2, 5L });

                env.Milestone(5);

                env.SendEventBean(MakeEvent("G2", 2, 101L));
                env.AssertPropsNew("S1", fields, new object[] { "G2", 2, 201L });
                env.AssertPropsNew("S2", fields, new object[] { "G2", 2, new long?[] { 100L, 101L } });
                env.AssertPropsNew("S3", fields, new object[] { "G2", 2, 201L });

                env.Milestone(6);

                env.SendEventBean(MakeEvent("G3", 1, -1L));
                env.AssertPropsNew("S1", fields, new object[] { "G3", 1, -1L });
                env.AssertPropsNew("S2", fields, new object[] { "G3", 1, new long?[] { -1L } });
                env.AssertPropsNew("S3", fields, new object[] { "G3", 1, -1L });

                env.Milestone(7);

                env.SendEventBean(MakeEvent("G3", 2, -2L));
                env.AssertPropsNew("S1", fields, new object[] { "G3", 2, -2L });
                env.AssertPropsNew("S2", fields, new object[] { "G3", 2, new long?[] { -2L } });
                env.AssertPropsNew("S3", fields, new object[] { "G3", 2, -2L });

                env.Milestone(8);

                env.SendEventBean(MakeEvent("G3", 1, -3L));
                env.AssertPropsNew("S1", fields, new object[] { "G3", 1, -4L });
                env.AssertPropsNew("S2", fields, new object[] { "G3", 1, new long?[] { -1L, -3L } });
                env.AssertPropsNew("S3", fields, new object[] { "G3", 1, -4L });

                env.Milestone(9);

                env.SendEventBean(MakeEvent("G1", 2, 3L));
                env.AssertPropsNew("S1", fields, new object[] { "G1", 2, 8L });
                env.AssertPropsNew("S2", fields, new object[] { "G1", 2, new long?[] { 2L, 3L, 3L } });
                env.AssertPropsNew("S3", fields, new object[] { "G1", 2, 5L });

                env.UndeployAll();
            }
        }

        private static SupportBean MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        public static object ToArray(ICollection<int?> input)
        {
            return input.ToArray();
        }

        public static object ToArray(ICollection<long?> input)
        {
            return input.ToArray();
        }
    }
} // end of namespace