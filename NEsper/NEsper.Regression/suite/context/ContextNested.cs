///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;
using com.espertech.esper.regressionlib.support.filter;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextNested
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextNestedWithFilterUDF());
            execs.Add(new ContextNestedIterateTargetedCP());
            execs.Add(new ContextNestedInvalid());
            execs.Add(new ContextNestedIterator());
            execs.Add(new ContextNestedPartitionedWithFilterOverlap());
            execs.Add(new ContextNestedPartitionedWithFilterNonOverlap());
            execs.Add(new ContextNestedNestingFilterCorrectness());
            execs.Add(new ContextNestedCategoryOverPatternInitiated());
            execs.Add(new ContextNestedSingleEventTriggerNested());
            execs.Add(new ContextNestedFourContextsNested());
            execs.Add(new ContextNestedTemporalOverCategoryOverPartition());
            execs.Add(new ContextNestedTemporalFixedOverHash());
            execs.Add(new ContextNestedCategoryOverTemporalOverlapping());
            execs.Add(new ContextNestedFixedTemporalOverPartitioned());
            execs.Add(new ContextNestedPartitionedOverFixedTemporal());
            execs.Add(new ContextNestedContextProps());
            execs.Add(new ContextNestedLateComingStatement());
            execs.Add(new ContextNestedPartitionWithMultiPropsAndTerm());
            execs.Add(new ContextNestedOverlappingAndPattern());
            execs.Add(new ContextNestedNonOverlapping());
            execs.Add(new ContextNestedPartitionedOverPatternInitiated());
            execs.Add(new ContextNestedInitWStartNow());
            execs.Add(new ContextNestedInitWStartNowSceneTwo());
            execs.Add(new ContextNestedKeyedStartStop());
            execs.Add(new ContextNestedKeyedFilter());
            execs.Add(new ContextNestedNonOverlapOverNonOverlapNoEndCondition(false));
            execs.Add(new ContextNestedNonOverlapOverNonOverlapNoEndCondition(true));
            execs.Add(new ContextNestedInitTermWCategoryWHash());
            execs.Add(new ContextNestedInitTermOverHashIterate(true));
            execs.Add(new ContextNestedInitTermOverHashIterate(false));
            execs.Add(new ContextNestedInitTermOverPartitionedIterate());
            execs.Add(new ContextNestedInitTermOverCategoryIterate());
            execs.Add(new ContextNestedInitTermOverInitTermIterate());
            execs.Add(new ContextNestedCategoryOverInitTermDistinct());
            return execs;
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            RegressionPath path)
        {
            env.AdvanceTime(0);
            var fields = "c0,c1".SplitCsv();
            env.CompileDeploy(
                "@Name('s0') context NestedContext " +
                "select TheString as c0, sum(IntPrimitive) as c1 from SupportBean \n" +
                "output last when terminated",
                path);
            env.AddListener("s0");

            env.SendEventBean(new SupportBean("E1", 1));
            env.SendEventBean(new SupportBean("E2", 2));

            env.Milestone(0);

            env.AdvanceTime(10000);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").DataListsFlattened,
                fields,
                new[] {new object[] {"E1", 1}, new object[] {"E2", 2}},
                null);
            env.Listener("s0").Reset();

            env.SendEventBean(new SupportBean("E1", 3));
            env.SendEventBean(new SupportBean("E3", 4));

            env.Milestone(1);

            env.AdvanceTime(20000);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").DataListsFlattened,
                fields,
                new[] {new object[] {"E1", 3}, new object[] {"E3", 4}},
                null);

            env.UndeployAll();
        }

        private static void TryAssertion3Contexts(
            RegressionEnvironment env,
            AtomicLong milestone,
            string[] fields,
            string startTime,
            string subsequentTime)
        {
            env.SendEventBean(MakeEvent("E1", 0, 10));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"g2", "E1", 10L});

            AssertPartitionInfo(env, startTime);

            env.SendEventBean(MakeEvent("E2", 0, 11));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"g2", "E2", 11L});

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E1", 0, 12));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"g2", "E1", 22L});

            env.SendEventBean(MakeEvent("E1", 1, 13));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"g3", "E1", 13L});

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E1", -1, 14));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"g1", "E1", 14L});

            env.SendEventBean(MakeEvent("E2", -1, 15));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {"g1", "E2", 15L});

            SendTimeEvent(env, subsequentTime);

            env.MilestoneInc(milestone);

            env.SendEventBean(MakeEvent("E2", -1, 15));
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            env.SendEventBean(bean);
        }

        public static bool CustomMatch(
            string theString,
            string p00,
            int intPrimitive,
            int s1id)
        {
            Assert.AreEqual("X", theString);
            Assert.AreEqual("S0", p00);
            Assert.AreEqual(-1, intPrimitive);
            Assert.AreEqual(2, s1id);
            return true;
        }

        private static object MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        private static object MakeEvent(
            string theString,
            int intPrimitive,
            long longPrimitive,
            bool boolPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            bean.BoolPrimitive = boolPrimitive;
            return bean;
        }

        private static void SendTimeEvent(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void AssertPartitionInfo(
            RegressionEnvironment env,
            string startTime)
        {
            var partitionAdmin = env.Runtime.ContextPartitionService;
            var deploymentId = env.Statement("ctx").DeploymentId;
            var partitions = partitionAdmin.GetContextPartitions(
                deploymentId,
                "NestedContext",
                ContextPartitionSelectorAll.INSTANCE);
            Assert.AreEqual(1, partitions.Identifiers.Count);
            var nested = (ContextPartitionIdentifierNested) partitions.Identifiers.Values.First();
            AssertNested(nested, startTime);
        }

        private static void AssertNested(
            ContextPartitionIdentifierNested nested,
            string startTime)
        {
            Assert.AreEqual(
                DateTimeParsingFunctions.ParseDefaultMSec(startTime),
                ((ContextPartitionIdentifierInitiatedTerminated) nested.Identifiers[0]).StartTime);
            Assert.AreEqual("g2", ((ContextPartitionIdentifierCategory) nested.Identifiers[1]).Label);
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[] {"E1"},
                ((ContextPartitionIdentifierPartitioned) nested.Identifiers[2]).Keys);
        }

        private static SupportBean SendBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive,
            long longPrimitive,
            bool boolPrimitive)
        {
            var sb = new SupportBean(theString, intPrimitive);
            sb.BoolPrimitive = boolPrimitive;
            sb.LongPrimitive = longPrimitive;
            env.SendEventBean(sb);
            return sb;
        }

        private static void AssertFilterCount(
            RegressionEnvironment env,
            int count,
            string stmtName)
        {
            var statement = env.Statement(stmtName);
            Assert.AreEqual(count, SupportFilterHelper.GetFilterCount(statement, "SupportBean"));
        }

        internal class ContextNestedInitWStartNow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.AdvanceTime(0);
                env.CompileDeploy(
                    "create context Ctx " +
                    "context C0 initiated by SupportBean as criteria terminated by SupportBean(theString='x'), " +
                    "context C1 start @now end (*,*,*,*,*,*/5)",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context Ctx select context.C0.criteria as c0, event, count(*) as cnt from SupportBean_S0(p00=context.C0.criteria.TheString) as event",
                    path);
                env.AddListener("s0");

                var criteriaA = new SupportBean("A", 0);
                env.SendEventBean(criteriaA);
                env.SendEventBean(new SupportBean_S0(1, "B"));
                env.SendEventBean(new SupportBean("B", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                var s0 = new SupportBean_S0(2, "A");
                env.SendEventBean(s0);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,event,cnt".SplitCsv(),
                    new object[] {criteriaA, s0, 1L});

                env.SendEventBean(new SupportBean_S0(3, "A"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,cnt".SplitCsv(),
                    new object[] {criteriaA, 2L});

                env.Milestone(1);

                env.AdvanceTime(5000000);

                env.SendEventBean(new SupportBean_S0(4, "A"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "c0,cnt".SplitCsv(),
                    new object[] {criteriaA, 1L});

                env.UndeployAll();
            }
        }

        public class ContextNestedInitWStartNowSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var path = new RegressionPath();

                env.CompileDeploy(
                    "@Name('ctx') create context MyContext \n" +
                    "context C0 initiated by SupportBean(intPrimitive=0) AS criteria terminated by SupportBean(intPrimitive=1), \n" +
                    "context C1 start @now end (*,*,*,*,*,*/5)",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context MyContext select count(*) as cnt from SupportBean(theString = context.C0.criteria.TheString)",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("A", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "cnt".SplitCsv(),
                    new object[] {1L});

                env.Milestone(0);

                var cpc = env.Runtime.ContextPartitionService.GetContextPartitions(
                    env.DeploymentId("ctx"),
                    "MyContext",
                    ContextPartitionSelectorAll.INSTANCE);
                var nested = (ContextPartitionIdentifierNested) cpc.Identifiers[0];
                var first = (ContextPartitionIdentifierInitiatedTerminated) nested.Identifiers[0];
                Assert.IsFalse(first.Properties.IsEmpty());
                var second = (ContextPartitionIdentifierInitiatedTerminated) nested.Identifiers[1];

                env.SendEventBean(new SupportBean("A", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "cnt".SplitCsv(),
                    new object[] {2L});

                env.Milestone(1);

                env.AdvanceTime(100000);

                env.SendEventBean(new SupportBean("A", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "cnt".SplitCsv(),
                    new object[] {1L});

                env.UndeployAll();
            }
        }

        internal class ContextNestedPartitionedOverPatternInitiated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context TheContext " +
                    "context C0 partition by theString from SupportBean," +
                    "context C1 initiated by SupportBean(intPrimitive=1) terminated by SupportBean(intPrimitive=2)",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context TheContext select TheString, sum(longPrimitive) as theSum from SupportBean output last when terminated",
                    path);
                env.AddListener("s0");

                SendSupportBean(env, "A", 0, 1);
                SendSupportBean(env, "B", 0, 2);

                env.Milestone(0);

                SendSupportBean(env, "C", 1, 3);
                SendSupportBean(env, "D", 1, 4);

                env.Milestone(1);

                SendSupportBean(env, "A", 0, 5);
                SendSupportBean(env, "C", 0, 6);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendSupportBean(env, "C", 2, -10);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "theString,theSum".SplitCsv(),
                    new object[] {"C", -1L});

                env.Milestone(2);

                SendSupportBean(env, "D", 2, 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    "theString,theSum".SplitCsv(),
                    new object[] {"D", 9L});

                env.UndeployAll();
            }
        }

        internal class ContextNestedWithFilterUDF : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context NestedContext " +
                    "context ACtx initiated by SupportBean_S0 as s0 terminated after 24 hours, " +
                    "context BCtx initiated by SupportBean_S1 as s1 terminated after 1 hour",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select * " +
                    "from SupportBean(" +
                    "customEnabled(theString, context.ACtx.s0.p00, intPrimitive, context.BCtx.s1.id)" +
                    " and " +
                    "customDisabled(theString, context.ACtx.s0.p00, intPrimitive, context.BCtx.s1.id))",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "S0"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S1(2, "S1"));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("X", -1));
                Assert.IsTrue(env.Listener("s0").IsInvoked);

                env.UndeployAll();

                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
            }
        }

        internal class ContextNestedIterateTargetedCP : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context NestedContext " +
                    "context ACtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(id=s0.id), " +
                    "context BCtx group by IntPrimitive < 0 as grp1, group by IntPrimitive = 0 as grp2, group by IntPrimitive > 0 as grp3 from SupportBean",
                    path);

                var fields = "c0,c1,c2,c3".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext " +
                    "select context.ACtx.s0.p00 as c0, context.BCtx.label as c1, theString as c2, sum(IntPrimitive) as c3 from SupportBean#length(5) group by TheString",
                    path);

                env.SendEventBean(new SupportBean_S0(1, "S0_1"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", -1));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 5));
                env.SendEventBean(new SupportBean_S0(2, "S0_2"));
                env.SendEventBean(new SupportBean("E1", 2));

                env.MilestoneInc(milestone);

                object[][] expectedAll = {
                    new object[] {"S0_1", "grp1", "E2", -1},
                    new object[] {"S0_1", "grp3", "E3", 5},
                    new object[] {"S0_1", "grp3", "E1", 3},
                    new object[] {"S0_2", "grp3", "E1", 2}
                };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    env.Statement("s0").GetSafeEnumerator(),
                    fields,
                    expectedAll);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(ContextPartitionSelectorAll.INSTANCE),
                    env.Statement("s0").GetSafeEnumerator(ContextPartitionSelectorAll.INSTANCE),
                    fields,
                    expectedAll);
                var allIds = new SupportSelectorById(new HashSet<int>(Arrays.AsList(0, 1, 2, 3, 4, 5)));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(allIds),
                    env.Statement("s0").GetSafeEnumerator(allIds),
                    fields,
                    expectedAll);

                // test iterator targeted
                ContextPartitionSelector firstOne = new SupportSelectorFilteredInitTerm("S0_2");
                ContextPartitionSelector secondOne = new SupportSelectorCategory(Collections.SingletonSet("grp3"));
                var nestedSelector = new SupportSelectorNested(Collections.SingletonList(new[] {firstOne, secondOne}));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(nestedSelector),
                    env.Statement("s0").GetSafeEnumerator(nestedSelector),
                    fields,
                    new[] {new object[] {"S0_2", "grp3", "E1", 2}});

                ContextPartitionSelector firstTwo = new SupportSelectorFilteredInitTerm("S0_1");
                ContextPartitionSelector secondTwo = new SupportSelectorCategory(Collections.SingletonSet("grp1"));
                var nestedSelectorTwo = new SupportSelectorNested(
                    Arrays.AsList(new[] {firstOne, secondOne}, new[] {firstTwo, secondTwo}));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(nestedSelectorTwo),
                    env.Statement("s0").GetSafeEnumerator(nestedSelectorTwo),
                    fields,
                    new[] {new object[] {"S0_2", "grp3", "E1", 2}, new object[] {"S0_1", "grp1", "E2", -1}});

                // test iterator filtered : not supported for nested
                try {
                    var filtered = new MySelectorFilteredNested(new object[] {"S0_2", "grp3"});
                    env.Statement("s0").GetEnumerator(filtered);
                    Assert.Fail();
                }
                catch (InvalidContextPartitionSelector ex) {
                    Assert.IsTrue(
                        ex.Message.StartsWith(
                            "Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorById, ContextPartitionSelectorNested] interfaces but received com."),
                        "message: " + ex.Message);
                }

                env.UndeployAll();
                path.Clear();

                // test 3 nesting levels and targeted
                env.CompileDeploy(
                    "create context NestedContext " +
                    "context ACtx group by IntPrimitive < 0 as i1, group by IntPrimitive = 0 as i2, group by IntPrimitive > 0 as i3 from SupportBean," +
                    "context BCtx group by LongPrimitive < 0 as l1, group by LongPrimitive = 0 as l2, group by LongPrimitive > 0 as l3 from SupportBean," +
                    "context CCtx group by boolPrimitive = true as b1, group by boolPrimitive = false as b2 from SupportBean",
                    path);

                var fieldsSelect = "c0,c1,c2,c3".SplitCsv();
                env.CompileDeploy(
                    "@Name('StmtOne') context NestedContext " +
                    "select context.ACtx.label as c0, context.BCtx.label as c1, context.CCtx.label as c2, count(*) as c3 from SupportBean#length(5) having count(*) > 0",
                    path);

                env.SendEventBean(MakeEvent("E1", -1, 10L, true));
                env.SendEventBean(MakeEvent("E2", 2, -10L, false));

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("E3", 1, 11L, false));
                env.SendEventBean(MakeEvent("E4", 0, 0L, true));

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("E5", -1, 10L, false));
                env.SendEventBean(MakeEvent("E6", -1, 10L, true));

                env.MilestoneInc(milestone);

                object[][] expectedRows = {
                    new object[] {"i1", "l3", "b1", 2L},
                    new object[] {"i3", "l1", "b2", 1L},
                    new object[] {"i1", "l3", "b2", 1L},
                    new object[] {"i2", "l2", "b1", 1L},
                    new object[] {"i3", "l3", "b2", 1L}
                };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("StmtOne").GetEnumerator(),
                    env.Statement("StmtOne").GetSafeEnumerator(),
                    fieldsSelect,
                    expectedRows);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("StmtOne").GetEnumerator(ContextPartitionSelectorAll.INSTANCE),
                    env.Statement("StmtOne").GetSafeEnumerator(ContextPartitionSelectorAll.INSTANCE),
                    fields,
                    expectedRows);

                // test iterator targeted
                ContextPartitionSelector[] selectors = {
                    new SupportSelectorCategory(Collections.SingletonSet("i3")),
                    new SupportSelectorCategory(Collections.SingletonSet("l1")),
                    new SupportSelectorCategory(Collections.SingletonSet("b2"))
                };
                var nestedSelectorSelect = new SupportSelectorNested(Collections.SingletonList(selectors));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("StmtOne").GetEnumerator(nestedSelectorSelect),
                    env.Statement("StmtOne").GetSafeEnumerator(nestedSelectorSelect),
                    fieldsSelect,
                    new[] {new object[] {"i3", "l1", "b2", 1L}});

                env.UndeployAll();
            }
        }

        internal class ContextNestedInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                // invalid same sub-context name twice
                epl =
                    "create context ABC context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *)";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    epl,
                    "Context by name 'EightToNine' has already been declared within nested context 'ABC' [");

                // validate statement added to nested context
                var path = new RegressionPath();
                epl =
                    "create context ABC context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), context PartCtx as partition by theString from SupportBean";
                env.CompileDeploy(epl, path);
                epl = "context ABC select * from SupportBean_S0";
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    path,
                    epl,
                    "Segmented context 'ABC' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");

                env.UndeployAll();
            }
        }

        internal class ContextNestedIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var path = new RegressionPath();

                env.CompileDeploy(
                    "create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegByString partition by theString from SupportBean",
                    path);

                var fields = "c0,c1,c2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.EightToNine.startTime as c0, context.SegByString.key1 as c1, intPrimitive as c2 from SupportBean#keepall",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                object[][] expected = {
                    new object[] {DateTimeParsingFunctions.ParseDefaultMSec("2002-05-1T08:00:00.000"), "E1", 1}
                };
                EPAssertionUtil.AssertPropsPerRow(env.Listener("s0").GetAndResetLastNewData(), fields, expected);
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), fields, expected);
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetSafeEnumerator(), fields, expected);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 2));
                expected = new[] {
                    new object[] {DateTimeParsingFunctions.ParseDefaultMSec("2002-05-1T08:00:00.000"), "E1", 1},
                    new object[] {DateTimeParsingFunctions.ParseDefaultMSec("2002-05-1T08:00:00.000"), "E1", 2}
                };
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetEnumerator(), fields, expected);
                EPAssertionUtil.AssertPropsPerRow(env.Statement("s0").GetSafeEnumerator(), fields, expected);

                env.UndeployAll();
            }
        }

        internal class ContextNestedNestingFilterCorrectness : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var path = new RegressionPath();
                string eplContext;
                var eplSelect = "@Name('s0') context TheContext select count(*) from SupportBean";
                SupportBean bean;

                // category over partition
                eplContext = "@Name('ctx') create context TheContext " +
                             "context CtxCategory as group intPrimitive < 0 as negative, group intPrimitive > 0 as positive from SupportBean, " +
                             "context CtxPartition as partition by theString from SupportBean";
                env.CompileDeploy(eplContext, path);
                env.CompileDeploy(eplSelect, path);

                AssertFilters(env, "[SupportBean(intPrimitive<0), SupportBean(intPrimitive>0)]", "ctx");
                env.SendEventBean(new SupportBean("E1", -1));

                env.MilestoneInc(milestone);

                AssertFilters(env, "[SupportBean(intPrimitive<0,theStringisE1)]", "s0");
                env.UndeployAll();
                path.Clear();
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                // category over partition over category
                eplContext = "@Name('ctx') create context TheContext " +
                             "context CtxCategoryOne as group intPrimitive < 0 as negative, group intPrimitive > 0 as positive from SupportBean, " +
                             "context CtxPartition as partition by theString from SupportBean," +
                             "context CtxCategoryTwo as group longPrimitive < 0 as negative, group longPrimitive > 0 as positive from SupportBean";
                env.CompileDeploy(eplContext, path);
                env.CompileDeploy(eplSelect, path);

                AssertFilters(env, "[SupportBean(intPrimitive<0), SupportBean(intPrimitive>0)]", "ctx");
                bean = new SupportBean("E1", -1);
                bean.LongPrimitive = 1;
                env.SendEventBean(bean);

                env.MilestoneInc(milestone);

                AssertFilters(
                    env,
                    "[SupportBean(intPrimitive<0,theStringisE1,longPrimitive<0), SupportBean(intPrimitive<0,theStringisE1,longPrimitive>0)]",
                    "s0");
                AssertFilters(env, "[SupportBean(intPrimitive<0), SupportBean(intPrimitive>0)]", "ctx");
                env.UndeployAll();
                path.Clear();
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                // partition over partition over partition
                eplContext = "@Name('ctx') create context TheContext " +
                             "context CtxOne as partition by theString from SupportBean, " +
                             "context CtxTwo as partition by intPrimitive from SupportBean," +
                             "context CtxThree as partition by longPrimitive from SupportBean";
                env.CompileDeploy(eplContext, path);
                env.CompileDeploy(eplSelect, path);

                AssertFilters(env, "[SupportBean()]", "ctx");
                bean = new SupportBean("E1", 2);
                bean.LongPrimitive = 3;
                env.SendEventBean(bean);
                env.MilestoneInc(milestone);

                AssertFilters(env, "[SupportBean(theStringisE1,intPrimitiveis2,longPrimitiveis3)]", "s0");
                AssertFilters(
                    env,
                    "[SupportBean(), SupportBean(theStringisE1), SupportBean(theStringisE1,intPrimitiveis2)]",
                    "ctx");

                env.UndeployAll();
                path.Clear();
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                // category over hash
                eplContext = "@Name('ctx') create context TheContext " +
                             "context CtxCategoryOne as group intPrimitive < 0 as negative, group intPrimitive > 0 as positive from SupportBean, " +
                             "context CtxTwo as coalesce by consistent_hash_crc32(TheString) from SupportBean granularity 100";
                env.CompileDeploy(eplContext, path);
                env.CompileDeploy(eplSelect, path);

                AssertFilters(env, "[SupportBean(intPrimitive<0), SupportBean(intPrimitive>0)]", "ctx");
                bean = new SupportBean("E1", 2);
                bean.LongPrimitive = 3;
                env.SendEventBean(bean);

                env.MilestoneInc(milestone);

                AssertFilters(env, "[SupportBean(intPrimitive>0,consistent_hash_crc32(TheString)=33)]", "s0");
                AssertFilters(env, "[SupportBean(intPrimitive<0), SupportBean(intPrimitive>0)]", "ctx");
                env.UndeployAll();
                path.Clear();
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                eplContext = "@Name('ctx') create context TheContext " +
                             "context CtxOne as partition by theString from SupportBean, " +
                             "context CtxTwo as start pattern [SupportBean_S0] end pattern[SupportBean_S1]";
                env.CompileDeploy(eplContext, path);
                env.CompileDeploy(eplSelect, path);

                AssertFilters(env, "[SupportBean()]", "ctx");
                env.SendEventBean(new SupportBean("E1", 2));

                env.MilestoneInc(milestone);

                AssertFilters(env, "[]", "s0");
                AssertFilters(env, "[SupportBean(), SupportBean_S0()]", "ctx");
                env.UndeployAll();
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
            }

            private static void AssertFilters(
                RegressionEnvironment env,
                string expected,
                string name)
            {
                Assert.AreEqual(expected, SupportFilterHelper.GetFilterToString(env, name));
            }
        }

        internal class ContextNestedPartitionedWithFilterOverlap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Audit('pattern-instances') create context TheContext" +
                    " context CtxSession partition by id from SupportBean_S0, " +
                    " context CtxStartEnd start SupportBean_S0 as te end SupportBean_S1(id=te.id)",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context TheContext select firstEvent from SupportBean_S0#firstevent() as firstEvent" +
                    " inner join SupportBean_S0#lastevent as lastEvent",
                    path);
                var supportSubscriber = new SupportSubscriber();
                env.Statement("s0").Subscriber = supportSubscriber;
                var milestone = new AtomicLong();

                for (var i = 0; i < 2; i++) {
                    env.SendEventBean(new SupportBean_S0(1, "A"));

                    env.MilestoneInc(milestone);
                    env.Statement("s0").Subscriber = supportSubscriber;

                    env.SendEventBean(new SupportBean_S0(2, "B"));
                    env.SendEventBean(new SupportBean_S1(1));

                    env.MilestoneInc(milestone);
                    env.Statement("s0").Subscriber = supportSubscriber;

                    supportSubscriber.Reset();
                    env.SendEventBean(new SupportBean_S0(2, "C"));
                    Assert.AreEqual("B", ((SupportBean_S0) supportSubscriber.AssertOneGetNewAndReset()).P00);

                    env.SendEventBean(new SupportBean_S1(1));

                    env.MilestoneInc(milestone);
                    env.Statement("s0").Subscriber = supportSubscriber;

                    env.SendEventBean(new SupportBean_S1(2));
                }

                env.UndeployAll();
            }
        }

        internal class ContextNestedCategoryOverPatternInitiated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var path = new RegressionPath();

                var eplCtx = "@Name('ctx') create context NestedContext as " +
                             "context ByCat as group intPrimitive < 0 as g1, group intPrimitive > 0 as g2, group intPrimitive = 0 as g3 from SupportBean, " +
                             "context InitCtx as initiated by pattern [every a=SupportBean_S0 => b=SupportBean_S1(id = a.id)] terminated after 10 sec";
                env.CompileDeploy(eplCtx, path);

                var fields = "c0,c1,c2,c3".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.ByCat.label as c0, context.InitCtx.a.p00 as c1, context.InitCtx.b.p10 as c2, sum(IntPrimitive) as c3 from SupportBean group by TheString",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(100, "S0_1"));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S1(101, "S1_1"));

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 2));

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S1(100, "S1_2"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"g2", "S0_1", "S1_2", 3}});

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E4", -2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"g1", "S0_1", "S1_2", -2}});

                env.SendEventBean(new SupportBean("E5", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"g3", "S0_1", "S1_2", 0}});

                env.SendEventBean(new SupportBean("E3", 5));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"g2", "S0_1", "S1_2", 8}});

                env.SendEventBean(new SupportBean("E6", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"g2", "S0_1", "S1_2", 6}});

                env.Milestone(6);

                env.SendEventBean(new SupportBean_S0(102, "S0_3"));

                env.Milestone(7);

                env.SendEventBean(new SupportBean_S1(102, "S1_3"));

                env.SendEventBean(new SupportBean("E3", 7));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"g2", "S0_1", "S1_2", 15}, new object[] {"g2", "S0_3", "S1_3", 7}});

                env.Milestone(8);

                SendTimeEvent(env, "2002-05-1T08:00:10.000");

                env.SendEventBean(new SupportBean("E3", 8));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(104, "S0_4"));
                env.SendEventBean(new SupportBean_S1(104, "S1_4"));

                env.Milestone(9);

                env.SendEventBean(new SupportBean("E3", 9));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"g2", "S0_4", "S1_4", 9}});

                env.UndeployAll();
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
            }
        }

        internal class ContextNestedPartitionedWithFilterNonOverlap : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var path = new RegressionPath();

                var eplCtx = "@Name('ctx') create context NestedContext as " +
                             "context SegByString as partition by theString from SupportBean(intPrimitive > 0), " +
                             "context InitCtx initiated by SupportBean_S0 as s0 terminated after 60 seconds";
                env.CompileDeploy(eplCtx, path);

                var fields = "c0,c1,c2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.InitCtx.s0.p00 as c0, theString as c1, sum(IntPrimitive) as c2 from SupportBean group by TheString",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                var s0Bean1 = new SupportBean_S0(1, "S0_1");
                env.SendEventBean(s0Bean1);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", -5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                SupportContextPropUtil.AssertContextPropsNested(
                    env,
                    "ctx",
                    "NestedContext",
                    new[] {0},
                    "SegByString,InitCtx".SplitCsv(),
                    new[] {"key1", "s0"},
                    new[] {
                        new[] {
                            new object[] {"E1"},
                            new object[] {s0Bean1}
                        }
                    });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E1", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_1", "E1", 2}});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E2", 3));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                var s0Bean2 = new SupportBean_S0(2, "S0_2");
                env.SendEventBean(s0Bean2);

                env.Milestone(4);

                SupportContextPropUtil.AssertContextPropsNested(
                    env,
                    "ctx",
                    "NestedContext",
                    new[]
                        {0, 1, 2},
                    "SegByString,InitCtx".SplitCsv(),
                    new[] {"key1", "s0"},
                    new[] {
                        new[] {
                            new object[] {"E1"},
                            new object[] {s0Bean1}
                        },
                        new[] {
                            new object[] {"E1"},
                            new object[] {s0Bean2}
                        },
                        new[] {
                            new object[] {"E2"},
                            new object[] {s0Bean2}
                        }
                    });

                env.SendEventBean(new SupportBean("E2", 4));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_2", "E2", 4}});

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E1", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_1", "E1", 8}, new object[] {"S0_2", "E1", 6}});

                env.UndeployAll();
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
            }
        }

        internal class ContextNestedSingleEventTriggerNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Test partitioned context
                //
                var path = new RegressionPath();
                var milestone = new AtomicLong();

                var eplCtxOne = "create context NestedContext as " +
                                "context SegByString as partition by theString from SupportBean, " +
                                "context SegByInt as partition by intPrimitive from SupportBean, " +
                                "context SegByLong as partition by longPrimitive from SupportBean ";
                env.CompileDeploy(eplCtxOne, path);

                var fieldsOne = "c0,c1,c2,c3".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.SegByString.key1 as c0, context.SegByInt.key1 as c1, context.SegByLong.key1 as c2, count(*) as c3 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(MakeEvent("E1", 10, 100));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsOne,
                    new[] {new object[] {"E1", 10, 100L, 1L}});

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("E2", 10, 100));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsOne,
                    new[] {new object[] {"E2", 10, 100L, 1L}});

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("E1", 11, 100));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsOne,
                    new[] {new object[] {"E1", 11, 100L, 1L}});

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("E1", 10, 101));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsOne,
                    new[] {new object[] {"E1", 10, 101L, 1L}});

                env.SendEventBean(MakeEvent("E1", 10, 100));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsOne,
                    new[] {new object[] {"E1", 10, 100L, 2L}});

                env.UndeployAll();
                path.Clear();
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                // Test partitioned context
                //
                var eplCtxTwo = "@Name('ctx') create context NestedContext as " +
                                "context HashOne coalesce by hash_code(TheString) from SupportBean granularity 10, " +
                                "context HashTwo coalesce by hash_code(IntPrimitive) from SupportBean granularity 10";
                env.CompileDeploy(eplCtxTwo, path);

                var fieldsTwo = "c1,c2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "theString as c1, count(*) as c2 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsTwo,
                    new[] {new object[] {"E1", 1L}});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsTwo,
                    new[] {new object[] {"E2", 1L}});

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsTwo,
                    new[] {new object[] {"E1", 2L}});

                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("ctx");
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
                path.Clear();

                // Test partitioned context
                //
                var eplCtxThree = "create context NestedContext as " +
                                  "context InitOne initiated by SupportBean(theString like 'I%') as sb0 terminated after 10 sec, " +
                                  "context InitTwo initiated by SupportBean(intPrimitive > 0) as sb1 terminated after 10 sec";
                env.CompileDeploy(eplCtxThree, path);

                var fieldsThree = "c1,c2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select TheString as c1, count(*) as c2 from SupportBean",
                    path);
                env.AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("I1", 1));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fieldsThree,
                    new[] {new object[] {"I1", 1L}});

                env.UndeployModuleContaining("s0");
                env.UndeployAll();
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
            }
        }

        internal class ContextNestedFourContextsNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T07:00:00.000");
                var path = new RegressionPath();
                var milestone = new AtomicLong();

                var eplCtx = "@Name('ctx') create context NestedContext as " +
                             "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                             "context InitCtx0 initiated by SupportBean_S0 as s0 terminated after 60 seconds, " +
                             "context InitCtx1 initiated by SupportBean_S1 as s1 terminated after 30 seconds, " +
                             "context InitCtx2 initiated by SupportBean_S2 as s2 terminated after 10 seconds";
                env.CompileDeploy(eplCtx, path);

                var fields = "c1,c2,c3,c4".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.InitCtx0.s0.p00 as c1, context.InitCtx1.s1.p10 as c2, context.InitCtx2.s2.p20 as c3, sum(IntPrimitive) as c4 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "S0_1"));
                env.SendEventBean(new SupportBean_S1(100, "S1_1"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S2(200, "S2_1"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                SendTimeEvent(env, "2002-05-1T08:00:00.000");

                env.SendEventBean(new SupportBean_S0(1, "S0_2"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S1(100, "S1_2"));
                env.SendEventBean(new SupportBean_S2(200, "S2_2"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_2", "S1_2", "S2_2", 2}});

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_2", "S1_2", "S2_2", 5}});

                SendTimeEvent(env, "2002-05-1T08:00:05.000");

                env.SendEventBean(new SupportBean_S1(101, "S1_3"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_2", "S1_2", "S2_2", 9}});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S2(201, "S2_3"));
                env.SendEventBean(new SupportBean("E5", 5));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"S0_2", "S1_2", "S2_2", 14}, new object[] {"S0_2", "S1_2", "S2_3", 5},
                        new object[] {"S0_2", "S1_3", "S2_3", 5}
                    });

                SendTimeEvent(env, "2002-05-1T08:00:10.000"); // terminate S2_2 leaf

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E6", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_2", "S1_2", "S2_3", 11}, new object[] {"S0_2", "S1_3", "S2_3", 11}});

                SendTimeEvent(env, "2002-05-1T08:00:15.000"); // terminate S0_2/S1_2/S2_3 and S0_2/S1_3/S2_3 leafs

                env.SendEventBean(new SupportBean("E7", 7));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S2(201, "S2_4"));
                env.SendEventBean(new SupportBean("E8", 8));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_2", "S1_2", "S2_4", 8}, new object[] {"S0_2", "S1_3", "S2_4", 8}});

                env.MilestoneInc(milestone);

                SendTimeEvent(env, "2002-05-1T08:00:30.000"); // terminate S1_2 branch

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E9", 9));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(105, "S1_5"));
                env.SendEventBean(new SupportBean_S2(205, "S2_5"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E10", 10));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_2", "S1_3", "S2_5", 10}, new object[] {"S0_2", "S1_5", "S2_5", 10}});

                SendTimeEvent(env, "2002-05-1T08:00:60.000"); // terminate S0_2 branch, only the "8to9" is left

                env.SendEventBean(new SupportBean("E11", 11));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(6, "S0_6"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S1(106, "S1_6"));
                env.SendEventBean(new SupportBean_S2(206, "S2_6"));
                env.SendEventBean(new SupportBean("E2", 12));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_6", "S1_6", "S2_6", 12}});

                env.SendEventBean(new SupportBean_S0(7, "S0_7"));
                env.SendEventBean(new SupportBean_S1(107, "S1_7"));
                env.SendEventBean(new SupportBean_S2(207, "S2_7"));

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("E3", 13));
                Assert.AreEqual(4, env.Listener("s0").GetAndResetLastNewData().Length);

                env.MilestoneInc(milestone);

                SendTimeEvent(env, "2002-05-1T10:00:00.000"); // terminate all

                env.SendEventBean(new SupportBean("E14", 14));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendTimeEvent(env, "2002-05-2T08:00:00.000"); // start next day

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean_S0(8, "S0_8"));
                env.SendEventBean(new SupportBean_S1(108, "S1_8"));
                env.SendEventBean(new SupportBean_S2(208, "S2_8"));
                env.SendEventBean(new SupportBean("E15", 15));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_8", "S1_8", "S2_8", 15}});

                var listener = env.Listener("s0");
                env.UndeployModuleContaining("s0");

                env.SendEventBean(new SupportBean("E16", 16));
                Assert.IsFalse(listener.IsInvoked);
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));

                env.UndeployAll();
            }
        }

        internal class ContextNestedTemporalOverlapOverPartition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var fields = "c1,c2,c3".SplitCsv();

                var epl = "@Name('ctx') create context NestedContext as " +
                          "context InitCtx initiated by SupportBean_S0(id > 0) as s0 terminated after 10 seconds, " +
                          "context SegmCtx as partition by theString from SupportBean(intPrimitive > 0);\n" +
                          "@Name('s0') context NestedContext select " +
                          "context.InitCtx.s0.p00 as c1, context.SegmCtx.key1 as c2, sum(IntPrimitive) as c3 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", -1));
                env.SendEventBean(new SupportBean_S0(-1, "S0_1"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 1));
                env.SendEventBean(new SupportBean_S0(1, "S0_2"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0_2", "E3", 3});

                env.SendEventBean(new SupportBean("E4", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0_2", "E4", 4});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E3", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0_2", "E3", 8});

                SendTimeEvent(env, "2002-05-1T08:00:05.000");

                env.SendEventBean(new SupportBean_S0(-2, "S0_3"));

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S0(1, "S0_4"));

                env.SendEventBean(new SupportBean("E3", 6));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_2", "E3", 14}, new object[] {"S0_4", "E3", 6}});

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E4", 7));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {new object[] {"S0_2", "E4", 11}, new object[] {"S0_4", "E4", 7}});

                SendTimeEvent(env, "2002-05-1T08:00:10.000"); // expires first context

                env.SendEventBean(new SupportBean("E3", 8));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0_4", "E3", 14});

                env.SendEventBean(new SupportBean("E4", 9));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0_4", "E4", 16});

                env.Milestone(5);

                SendTimeEvent(env, "2002-05-1T08:00:15.000"); // expires second context

                env.SendEventBean(new SupportBean("Ex", 1));
                env.SendEventBean(new SupportBean_S0(1, "S0_5"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(6);

                env.SendEventBean(new SupportBean("E4", 10));
                env.SendEventBean(new SupportBean("E4", -10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0_5", "E4", 10});

                SendTimeEvent(env, "2002-05-1T08:00:25.000"); // expires second context

                env.SendEventBean(new SupportBean("E4", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        internal class ContextNestedTemporalOverCategoryOverPartition : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                var path = new RegressionPath();
                var milestone = new AtomicLong();

                var eplCtx = "@Name('ctx') create context NestedContext as " +
                             "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                             "context ByCat as group intPrimitive<0 as g1, group intPrimitive=0 as g2, group intPrimitive>0 as g3 from SupportBean, " +
                             "context SegmentedByString as partition by theString from SupportBean";
                env.CompileDeploy(eplCtx, path);

                var fields = "c1,c2,c3".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.ByCat.label as c1, context.SegmentedByString.key1 as c2, sum(longPrimitive) as c3 from SupportBean",
                    path);
                env.AddListener("s0");

                TryAssertion3Contexts(env, milestone, fields, "2002-05-1T08:00:00.000", "2002-05-1T09:00:00.000");

                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("ctx");
                path.Clear();

                SendTimeEvent(env, "2002-05-2T08:00:00.000");

                // test SODA
                env.EplToModelCompileDeploy(eplCtx, path);
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.ByCat.label as c1, context.SegmentedByString.key1 as c2, sum(longPrimitive) as c3 from SupportBean",
                    path);
                env.AddListener("s0");

                TryAssertion3Contexts(env, milestone, fields, "2002-05-2T08:00:00.000", "2002-05-2T09:00:00.000");

                env.UndeployAll();
            }
        }

        /// <summary>
        ///     Root: Temporal
        ///     Sub: Hash
        ///     }
        /// </summary>
        internal class ContextNestedTemporalFixedOverHash : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendTimeEvent(env, "2002-05-1T07:00:00.000");

                env.CompileDeploy(
                    "create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context HashedCtx coalesce hash_code(IntPrimitive) from SupportBean granularity 10 preallocate",
                    path);
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));

                var fields = "c1,c2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "theString as c1, count(*) as c2 from SupportBean group by TheString",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                SendTimeEvent(env, "2002-05-1T08:00:00.000"); // start context

                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 1L});

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 2L});

                SendTimeEvent(env, "2002-05-1T09:00:00.000"); // terminate

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                SendTimeEvent(env, "2002-05-2T08:00:00.000"); // start context

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 1L});

                env.UndeployAll();
            }
        }

        /// <summary>
        ///     Root: Category
        ///     Sub: Initiated
        ///     }
        /// </summary>
        internal class ContextNestedCategoryOverTemporalOverlapping : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendTimeEvent(env, "2002-05-1T08:00:00.000");

                env.CompileDeploy(
                    "create context NestedContext " +
                    "context ByCat " +
                    "  group intPrimitive < 0 and intPrimitive != -9999 as g1, " +
                    "  group intPrimitive = 0 as g2, " +
                    "  group intPrimitive > 0 as g3 from SupportBean, " +
                    "context InitGrd initiated by SupportBean(theString like 'init%') as sb terminated after 10 seconds",
                    path);
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));

                var fields = "c1,c2,c3".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.ByCat.label as c1, context.InitGrd.sb.TheString as c2, count(*) as c3 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 5));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("init_1", -9999));
                env.SendEventBean(new SupportBean("X100", 0));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("X101", 10));
                env.SendEventBean(new SupportBean("X102", -10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("init_2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"g2", "init_2", 1L});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E3", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"g2", "init_2", 2L});

                env.SendEventBean(new SupportBean("E4", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("init_3", -2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"g1", "init_3", 1L});

                env.SendEventBean(new SupportBean("E5", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"g1", "init_3", 2L});

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E6", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"g1", "init_3", 3L});

                SendTimeEvent(env, "2002-05-1T08:11:00.000"); // terminates all

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E7", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }
        }

        /// <summary>
        ///     Root: Fixed temporal
        ///     Sub: Partition by string
        ///     <para />
        ///     - Root starts deactivated.
        ///     - With context destroy before statement destroy
        ///     }
        /// </summary>
        internal class ContextNestedFixedTemporalOverPartitioned : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendTimeEvent(env, "2002-05-1T07:00:00.000");

                env.CompileDeploy(
                    "create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegmentedByAString partition by theString from SupportBean",
                    path);
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));

                var fields = "c1".SplitCsv();
                env.CompileDeploy("@Name('s0') context NestedContext select count(*) as c1 from SupportBean", path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean());
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env));

                env.Milestone(0);

                // starts EightToNine context
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                Assert.AreEqual(1, SupportFilterHelper.GetFilterCountApprox(env));

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});
                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});
                Assert.AreEqual(3, SupportFilterHelper.GetFilterCountApprox(env));

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2L});
                Assert.AreEqual(3, SupportFilterHelper.GetFilterCountApprox(env));
                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env));

                env.Milestone(2);

                // ends EightToNine context
                SendTimeEvent(env, "2002-05-1T09:00:00.000");
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                // starts EightToNine context
                SendTimeEvent(env, "2002-05-2T08:00:00.000");
                Assert.AreEqual(1, SupportFilterHelper.GetFilterCountApprox(env));

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2L});

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 2, null, null, null);

                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2L});

                var listener = env.Listener("s0");
                env.UndeployAll();

                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 0));
                Assert.IsFalse(listener.IsInvoked);

                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));
            }
        }

        /// <summary>
        ///     Root: Partition by string
        ///     Sub: Fixed temporal
        ///     <para />
        ///     - Sub starts deactivated.
        ///     - With statement destroy before context destroy
        ///     }
        /// </summary>
        internal class ContextNestedPartitionedOverFixedTemporal : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendTimeEvent(env, "2002-05-1T07:00:00.000");

                env.CompileDeploy(
                    "create context NestedContext " +
                    "context SegmentedByAString partition by theString from SupportBean, " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *)",
                    path);
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));

                var fields = "c1".SplitCsv();
                env.CompileDeploy("@Name('s0') context NestedContext select count(*) as c1 from SupportBean", path);
                env.AddListener("s0");
                Assert.AreEqual(1, SupportFilterHelper.GetFilterCountApprox(env));
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));

                env.SendEventBean(new SupportBean("E1", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.AreEqual(1, SupportFilterHelper.GetFilterCountApprox(env));
                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env));

                // starts EightToNine context
                SendTimeEvent(env, "2002-05-1T08:00:00.000");
                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});
                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));
                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env));

                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});
                Assert.AreEqual(3, SupportFilterHelper.GetFilterCountApprox(env));
                Assert.AreEqual(2, SupportScheduleHelper.ScheduleCountOverall(env));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2L});
                Assert.AreEqual(3, SupportFilterHelper.GetFilterCountApprox(env));

                // ends EightToNine context
                SendTimeEvent(env, "2002-05-1T09:00:00.000");
                Assert.AreEqual(1, SupportFilterHelper.GetFilterCountApprox(env));

                env.SendEventBean(new SupportBean("E1", 0));
                env.SendEventBean(new SupportBean("E2", 0));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                Assert.AreEqual(2, SupportScheduleHelper.ScheduleCountOverall(env));

                env.Milestone(2);

                // starts EightToNine context
                SendTimeEvent(env, "2002-05-2T08:00:00.000");
                Assert.AreEqual(3, SupportFilterHelper.GetFilterCountApprox(env));

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2L});

                env.SendEventBean(new SupportBean("E2", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 2, null, null, null);
                Assert.AreEqual(2, SupportScheduleHelper.ScheduleCountOverall(env));

                var listener = env.Listener("s0");
                env.UndeployAll();

                env.SendEventBean(new SupportBean("E1", 0));
                Assert.IsFalse(listener.IsInvoked);
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));
            }
        }

        /// <summary>
        ///     Test nested context properties.
        ///     <para />
        ///     Root: Fixed temporal
        ///     Sub: Partition by string
        ///     <para />
        ///     - fixed temportal starts active
        ///     - starting and stopping statement
        ///     }
        /// </summary>
        internal class ContextNestedContextProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendTimeEvent(env, "2002-05-1T08:30:00.000");

                env.CompileDeploy(
                    "@Name('ctx') create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegmentedByAString partition by theString from SupportBean",
                    path);

                var fields = "c0,c1,c2,c3,c4,c5,c6".SplitCsv();
                var epl = "@Name('s0') context NestedContext select " +
                          "context.EightToNine.name as c0, " +
                          "context.EightToNine.startTime as c1, " +
                          "context.SegmentedByAString.name as c2, " +
                          "context.SegmentedByAString.key1 as c3, " +
                          "context.name as c4, " +
                          "intPrimitive as c5," +
                          "count(*) as c6 " +
                          "from SupportBean";
                env.CompileDeploy(epl, path).AddListener("s0");
                Assert.AreEqual(1, SupportFilterHelper.GetFilterCountApprox(env));

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "EightToNine", DateTimeParsingFunctions.ParseDefaultMSec("2002-05-1T08:30:00.000"),
                        "SegmentedByAString", "E1",
                        "NestedContext",
                        10, 1L
                    });
                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "EightToNine", DateTimeParsingFunctions.ParseDefaultMSec("2002-05-1T08:30:00.000"),
                        "SegmentedByAString", "E2",
                        "NestedContext",
                        20, 1L
                    });
                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env));
                Assert.AreEqual(3, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 2);

                env.Milestone(1);

                env.UndeployModuleContaining("s0");
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));

                env.Milestone(2);

                env.CompileDeploy(epl, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E2", 30));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "EightToNine", DateTimeParsingFunctions.ParseDefaultMSec("2002-05-1T08:30:00.000"),
                        "SegmentedByAString", "E2",
                        "NestedContext",
                        30, 1L
                    });
                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env));
                Assert.AreEqual(2, SupportFilterHelper.GetFilterCountApprox(env));
                AgentInstanceAssertionUtil.AssertInstanceCounts(env, "s0", 1);

                env.Milestone(3);

                var listener = env.Listener("s0");
                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("ctx");

                env.Milestone(4);

                env.SendEventBean(new SupportBean("E2", 30));
                Assert.IsFalse(listener.IsInvoked);
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));
                Assert.AreEqual(0, SupportFilterHelper.GetFilterCountApprox(env));
            }
        }

        /// <summary>
        ///     Test late-coming env.statement("s0").
        ///     <para />
        ///     Root: Fixed temporal
        ///     Sub: Partition by string
        ///     }
        /// </summary>
        internal class ContextNestedLateComingStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                SendTimeEvent(env, "2002-05-1T08:30:00.000");

                env.CompileDeploy(
                    "create context NestedContext " +
                    "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                    "context SegmentedByAString partition by theString from SupportBean",
                    path);

                var fields = "c0,c1".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select TheString as c0, count(*) as c1 from SupportBean",
                    path);
                env.AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1L});

                env.CompileDeploy(
                    "@Name('s2') context NestedContext select TheString as c0, sum(IntPrimitive) as c1 from SupportBean",
                    path);
                env.AddListener("s2");

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 20});

                env.SendEventBean(new SupportBean("E2", 30));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 30});

                env.Milestone(2);

                env.CompileDeploy(
                    "@Name('s3') context NestedContext select TheString as c0, min(IntPrimitive) as c1 from SupportBean",
                    path);
                env.AddListener("s3");

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E1", 40));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 3L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 60});
                EPAssertionUtil.AssertProps(
                    env.Listener("s3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 40});

                env.Milestone(4);

                var s2Listener = env.Listener("s2");
                env.UndeployModuleContaining("s2");

                env.SendEventBean(new SupportBean("E1", 50));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 4L});
                Assert.IsFalse(s2Listener.IsInvoked);
                EPAssertionUtil.AssertProps(
                    env.Listener("s3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 40});

                var s0Listener = env.Listener("s0");
                env.UndeployModuleContaining("s0");

                env.SendEventBean(new SupportBean("E1", -60));
                Assert.IsFalse(s0Listener.IsInvoked);
                Assert.IsFalse(s2Listener.IsInvoked);
                EPAssertionUtil.AssertProps(
                    env.Listener("s3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", -60});

                env.UndeployModuleContaining("s3");

                env.UndeployAll();
            }
        }

        internal class ContextNestedPartitionWithMultiPropsAndTerm : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context NestedContext " +
                    "context PartitionedByKeys partition by theString, intPrimitive from SupportBean, " +
                    "context InitiateAndTerm start SupportBean as e1 " +
                    "end SupportBean_S0(id=e1.IntPrimitive and p00=e1.TheString)",
                    path);

                var fields = "c0,c1,c2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext " +
                    "select TheString as c0, intPrimitive as c1, count(longPrimitive) as c2 from SupportBean \n" +
                    "output last when terminated",
                    path);
                env.AddListener("s0");

                env.SendEventBean(MakeEvent("E1", 0, 10));
                env.SendEventBean(MakeEvent("E1", 0, 10));

                env.Milestone(0);

                env.SendEventBean(MakeEvent("E2", 1, 1));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S0(0, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 0, 2L});

                env.UndeployAll();
            }
        }

        internal class ContextNestedOverlappingAndPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context NestedContext " +
                    "context PartitionedByKeys partition by theString from SupportBean, " +
                    "context TimedImmediate initiated @now and pattern[every timer:interval(10)] terminated after 10 seconds",
                    path);
                TryAssertion(env, path);
            }
        }

        internal class ContextNestedNonOverlapping : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context NestedContext " +
                    "context PartitionedByKeys partition by theString from SupportBean, " +
                    "context TimedImmediate start @now end after 10 seconds",
                    path);
                TryAssertion(env, path);
            }
        }

        internal class ContextNestedKeyedStartStop : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.AdvanceTime(0);
                env.CompileDeploy(
                    "@Name('ctx') create context NestedCtxWTime " +
                    "context OuterCtx initiated @now and pattern[timer:interval(10000000)] terminated after 1 second, " +
                    "context InnerCtx partition by theString from SupportBean(intPrimitive=0) terminated by SupportBean(intPrimitive=1)",
                    path);
                env.CompileDeploy("context NestedCtxWTime select TheString, count(*) as cnt from SupportBean", path);

                env.SendEventBean(new SupportBean("A", 0));
                env.SendEventBean(new SupportBean("B", 0));

                env.Milestone(0);

                env.AdvanceTime(100000);
                AssertFilterCount(env, 0, "ctx");

                env.UndeployAll();
            }
        }

        internal class ContextNestedKeyedFilter : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('ctx') create context NestedCtxWPartition " +
                    "context ByString partition by theString from SupportBean, " +
                    "context ByInt partition by intPrimitive from SupportBean terminated by SupportBean(boolPrimitive=false)",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context NestedCtxWPartition select TheString, intPrimitive, sum(longPrimitive) as thesum from SupportBean output last when terminated",
                    path);
                env.AddListener("s0");
                var fields = "theString,intPrimitive,thesum".SplitCsv();

                SendBean(env, "A", 1, 10, true);
                SendBean(env, "B", 1, 11, true);
                SendBean(env, "A", 2, 12, true);

                env.Milestone(0);

                SendBean(env, "B", 2, 13, true);
                SendBean(env, "B", 1, 20, true);
                SendBean(env, "A", 1, 30, true);

                env.Milestone(1);

                SendBean(env, "A", 2, 40, true);
                SendBean(env, "B", 2, 50, true);

                SendBean(env, "A", 1, 0, false);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 1, 40L});

                env.Milestone(2);

                SendBean(env, "B", 2, 0, false);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 2, 63L});

                SendBean(env, "A", 2, 0, false);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"A", 2, 52L});

                env.Milestone(3);

                SendBean(env, "B", 1, 0, false);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"B", 1, 31L});

                AssertFilterCount(env, 3, "ctx");
                env.UndeployModuleContaining("s0");
                AssertFilterCount(env, 0, "ctx");
                env.UndeployAll();
            }
        }

        public class ContextNestedNonOverlapOverNonOverlapNoEndCondition : RegressionExecution
        {
            private readonly bool soda;

            public ContextNestedNonOverlapOverNonOverlapNoEndCondition(bool soda)
            {
                this.soda = soda;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    soda,
                    "create context MyCtx as " +
                    "context Lvl1Ctx as start SupportBean_S0 as s0, " +
                    "context Lvl2Ctx as start SupportBean_S1 as s1",
                    path);
                env.CompileDeploy(
                    "@Name('s0') context MyCtx " +
                    "select TheString, context.Lvl1Ctx.s0.p00 as p00, context.Lvl2Ctx.s1.p10 as p10 from SupportBean",
                    path);
                env.AddListener("s0");
                var fields = "theString,p00,p10".SplitCsv();

                env.SendEventBean(new SupportBean("P1", 100));

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1, "A"));
                env.SendEventBean(new SupportBean("P1", 100));

                env.Milestone(1);

                env.SendEventBean(new SupportBean_S1(2, "B"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "A", "B"});

                env.UndeployAll();
            }
        }

        public class ContextNestedInitTermWCategoryWHash : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendTimeEvent(env, "2002-05-1T8:00:00.000");
                var path = new RegressionPath();

                var eplCtx = "@Name('ctx') create context NestedContext as " +
                             "context EightToNine as start (0, 8, *, *, *) end (0, 9, *, *, *), " +
                             "context ByCat as group intPrimitive < 0 as g1, group intPrimitive = 0 as g2, group intPrimitive > 0 as g3 from SupportBean, " +
                             "context SegmentedByString as partition by theString from SupportBean";
                env.CompileDeploy(eplCtx, path);

                var fields = "c1,c2,c3".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.ByCat.label as c1, context.SegmentedByString.key1 as c2, sum(longPrimitive) as c3 from SupportBean",
                    path);
                env.AddListener("s0");

                env.SendEventBean(MakeEvent("E1", 0, 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"g2", "E1", 10L});
                AssertPartitionInfo(env);

                env.Milestone(0);

                AssertPartitionInfo(env);
                env.SendEventBean(MakeEvent("E2", 0, 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"g2", "E2", 11L});

                env.Milestone(1);

                env.SendEventBean(MakeEvent("E1", 0, 12));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"g2", "E1", 22L});
                AssertPartitionInfoMulti(env, 2);

                env.Milestone(2);

                AssertPartitionInfoMulti(env, 2);
                env.SendEventBean(MakeEvent("E1", 1, 13));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"g3", "E1", 13L});
                AssertPartitionInfoMulti(env, 3);

                env.Milestone(3);

                AssertPartitionInfoMulti(env, 3);
                env.SendEventBean(MakeEvent("E1", -1, 14));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"g1", "E1", 14L});

                env.Milestone(4);

                env.SendEventBean(MakeEvent("E2", -1, 15));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"g1", "E2", 15L});

                env.Milestone(5);

                SendTimeEvent(env, "2002-05-1T9:01:00.000");

                env.Milestone(6);

                env.SendEventBean(MakeEvent("E2", -1, 15));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.UndeployAll();
            }

            private static object MakeEvent(
                string theString,
                int intPrimitive,
                long longPrimitive)
            {
                var bean = new SupportBean(theString, intPrimitive);
                bean.LongPrimitive = longPrimitive;
                return bean;
            }

            private void AssertPartitionInfoMulti(
                RegressionEnvironment env,
                int size)
            {
                var partitionAdmin = env.Runtime.ContextPartitionService;
                var partitions = partitionAdmin.GetContextPartitions(
                    env.DeploymentId("ctx"),
                    "NestedContext",
                    ContextPartitionSelectorAll.INSTANCE);
                Assert.AreEqual(size, partitions.Identifiers.Count);
            }

            private void AssertPartitionInfo(RegressionEnvironment env)
            {
                var partitionAdmin = env.Runtime.ContextPartitionService;
                var partitions = partitionAdmin.GetContextPartitions(
                    env.DeploymentId("ctx"),
                    "NestedContext",
                    ContextPartitionSelectorAll.INSTANCE);
                Assert.AreEqual(1, partitions.Identifiers.Count);
                var nested =
                    (ContextPartitionIdentifierNested) partitions.Identifiers.Values.First();
                AssertNested(nested);
            }

            private void AssertNested(ContextPartitionIdentifierNested nested)
            {
                Assert.IsTrue(((ContextPartitionIdentifierInitiatedTerminated) nested.Identifiers[0]).StartTime >= 0);
                Assert.AreEqual("g2", ((ContextPartitionIdentifierCategory) nested.Identifiers[1]).Label);
                EPAssertionUtil.AssertEqualsExactOrder(
                    new object[] {"E1"},
                    ((ContextPartitionIdentifierPartitioned) nested.Identifiers[2]).Keys);
            }

            private string GetLblLvl1(ContextPartitionIdentifierNested ident)
            {
                return ((ContextPartitionIdentifierCategory) ident.Identifiers[1]).Label;
            }
        }

        public class ContextNestedInitTermOverHashIterate : RegressionExecution
        {
            private readonly bool preallocate;

            public ContextNestedInitTermOverHashIterate(bool preallocate)
            {
                this.preallocate = preallocate;
            }

            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('ctx') create context NestedContext " +
                    "context FirstCtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(id = s0.id), " +
                    "context SecondCtx coalesce by consistent_hash_crc32(TheString) from SupportBean granularity 4 " +
                    (preallocate ? "preallocate" : ""),
                    path);

                var fields = "c0,c1".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.FirstCtx.s0.id as c0, theString as c1 from SupportBean#keepall",
                    path);
                env.AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S0(2));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {1, "E1"}, new object[] {2, "E1"}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 1));
                object[][] expectedAll = {
                    new object[] {1, "E1"},
                    new object[] {2, "E1"},
                    new object[] {1, "E2"},
                    new object[] {2, "E2"}
                };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), fields, expectedAll);

                // all-selector
                var selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    new ContextPartitionSelectorAll());
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);
                // hash-specific-selector
                selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    SupportSelectorByHashCode.FromSetOfAll(4));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);
                // filter-specific-selector
                selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    new SupportSelectorFilteredPassAll());
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);
                // id-specific-selector
                selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    SupportSelectorById.FromSetOfAll(100));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S1(2));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {1, "E1"}, new object[] {1, "E2"}});

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S1(1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new object[0][]);

                env.UndeployAll();
            }
        }

        public class ContextNestedInitTermOverPartitionedIterate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('ctx') create context NestedContext " +
                    "context FirstCtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(id = s0.id), " +
                    "context SecondCtx partition by theString from SupportBean",
                    path);

                var fields = "c0,c1".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.FirstCtx.s0.id as c0, theString as c1 from SupportBean#keepall",
                    path);
                env.AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S0(2));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1, "E1"}, new object[] {2, "E1"}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 1));
                object[][] expectedAll = {
                    new object[] {1, "E1"},
                    new object[] {2, "E1"},
                    new object[] {1, "E2"},
                    new object[] {2, "E2"}
                };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, expectedAll);

                // all-selector
                var selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    new ContextPartitionSelectorAll());
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);
                // segmented-specific-selector
                selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    new SupportSelectorPartitioned(
                        Arrays.AsList(
                            new object[] {"E1"},
                            new object[] {"E2"})));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);
                // filter-specific-selector
                selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    new SupportSelectorFilteredPassAll());
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S1(2));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1, "E1"}, new object[] {1, "E2"}});

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S1(1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new object[0][]);

                env.UndeployAll();
            }
        }

        public class ContextNestedInitTermOverCategoryIterate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('ctx') create context NestedContext " +
                    "context FirstCtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(id = s0.id), " +
                    "context SecondCtx group by TheString = 'E1' as cat1, group by TheString = 'E2' as cat2 from SupportBean",
                    path);

                var fields = "c0,c1".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.FirstCtx.s0.id as c0, theString as c1 from SupportBean#keepall",
                    path);
                env.AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S0(2));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1, "E1"}, new object[] {2, "E1"}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 1));
                object[][] expectedAll = {
                    new object[] {1, "E1"},
                    new object[] {2, "E1"},
                    new object[] {1, "E2"},
                    new object[] {2, "E2"}
                };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, expectedAll);

                // all-selector
                var selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    new ContextPartitionSelectorAll());
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);
                // category-specific-selector
                selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    new SupportSelectorCategory(
                        new HashSet<string>(Arrays.AsList("cat1,cat2".SplitCsv()))));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);
                // filter-specific-selector
                selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    new SupportSelectorFilteredPassAll());
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S1(2));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1, "E1"}, new object[] {1, "E2"}});

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S1(1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new object[0][]);

                env.UndeployAll();
            }
        }

        public class ContextNestedInitTermOverInitTermIterate : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('ctx') create context NestedContext " +
                    "context FirstCtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(id = s0.id), " +
                    "context SecondCtx initiated by SupportBean_S2 as s2 terminated after 24 hours",
                    path);

                var fields = "c0,c1,c2".SplitCsv();
                env.CompileDeploy(
                    "@Name('s0') context NestedContext select " +
                    "context.FirstCtx.s0.id as c0, context.SecondCtx.s2.id as c1, theString as c2 from SupportBean#keepall",
                    path);
                env.AddListener("s0");

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S0(2));
                env.SendEventBean(new SupportBean_S2(10));

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1, 10, "E1"}, new object[] {2, 10, "E1"}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E2", 1));
                object[][] expectedAll = {
                    new object[] {1, 10, "E1"},
                    new object[] {2, 10, "E1"},
                    new object[] {1, 10, "E2"},
                    new object[] {2, 10, "E2"}
                };
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.Statement("s0").GetEnumerator(), fields, expectedAll);

                // all-selector
                var selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    new ContextPartitionSelectorAll());
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);
                // filter-specific-selector
                selectorNested = new SupportSelectorNested(
                    new ContextPartitionSelectorAll(),
                    new SupportSelectorFilteredPassAll());
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(selectorNested),
                    fields,
                    expectedAll);

                env.Milestone(3);

                env.SendEventBean(new SupportBean_S1(2));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new[] {new object[] {1, 10, "E1"}, new object[] {1, 10, "E2"}});

                env.Milestone(4);

                env.SendEventBean(new SupportBean_S1(1));
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Statement("s0").GetEnumerator(),
                    fields,
                    new object[0][]);

                env.UndeployAll();
            }
        }

        public class ContextNestedCategoryOverInitTermDistinct : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "create context NestedContext " +
                    "context ACtx group by IntPrimitive < 0 as grp1, group by IntPrimitive = 0 as grp2, group by IntPrimitive > 0 as grp3 from SupportBean, " +
                    "context BCtx initiated by distinct(a.IntPrimitive) SupportBean(theString='A') as a terminated by SupportBean(theString='B') ",
                    path);
                env.CompileDeploy(
                        "@Name('s0') context NestedContext select count(*) as cnt from SupportBean(IntPrimitive = context.BCtx.a.IntPrimitive and theString != 'B')",
                        path)
                    .AddListener("s0");

                SendBeanAssertCount(env, "A", 10, 1);
                SendBeanAssertCount(env, "A", 10, 2);

                env.Milestone(0);

                SendBeanAssertCount(env, "A", -5, 1);
                SendBeanAssertCount(env, "A", -4, 1);
                env.SendEventBean(new SupportBean("B", 10));

                env.Milestone(1);

                SendBeanAssertCount(env, "A", 10, 1);
                SendBeanAssertCount(env, "A", -5, 2);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("B", -5));
                SendBeanAssertCount(env, "A", -5, 1);

                env.UndeployAll();
            }

            private void SendBeanAssertCount(
                RegressionEnvironment env,
                string theString,
                int intPrimitive,
                long count)
            {
                env.SendEventBean(new SupportBean(theString, intPrimitive));
                Assert.AreEqual(count, env.Listener("s0").AssertOneGetNewAndReset().Get("cnt"));
            }
        }

        public class MySelectorFilteredNested : ContextPartitionSelectorFiltered
        {
            private readonly object[] pathMatch;
            private readonly LinkedHashSet<int?> cpids = new LinkedHashSet<int?>();

            private readonly IList<object[]> paths = new List<object[]>();

            public MySelectorFilteredNested(object[] pathMatch)
            {
                this.pathMatch = pathMatch;
            }

            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
            {
                var nested = (ContextPartitionIdentifierNested) contextPartitionIdentifier;
                if (pathMatch == null && cpids.Contains(nested.ContextPartitionId)) {
                    throw new EPException("Already exists context id: " + nested.ContextPartitionId);
                }

                cpids.Add(nested.ContextPartitionId);

                var first = (ContextPartitionIdentifierInitiatedTerminated) nested.Identifiers[0];
                var second = (ContextPartitionIdentifierCategory) nested.Identifiers[1];

                var extract = new object[2];
                extract[0] = ((EventBean) first.Properties.Get("s0")).Get("p00");
                extract[1] = second.Label;
                paths.Add(extract);

                return paths != null && Equals(pathMatch, extract);
            }
        }
    }
} // end of namespace