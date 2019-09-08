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
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectIndex : IndexBackingTableInfo
    {
        private const int SUBQUERY_NUM_FIRST = 0;

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLSubselectIndexChoicesOverdefinedWhere());
            execs.Add(new EPLSubselectUniqueIndexCorrelated());
            return execs;
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            bool disableImplicitUniqueIdx,
            string uniqueFields,
            string whereClause,
            string backingTable,
            IndexAssertionEventSend assertion)
        {
            SupportQueryPlanIndexHook.Reset();
            var eplUnique = "@Name('s0')" +
                            INDEX_CALLBACK_HOOK +
                            "select s1 as c0, " +
                            "(select S2 from SupportSimpleBeanTwo#unique(" +
                            uniqueFields +
                            ") as ssb2 " +
                            whereClause +
                            ") as c1 " +
                            "from SupportSimpleBeanOne as ssb1";
            if (disableImplicitUniqueIdx) {
                eplUnique = "@Hint('DISABLE_UNIQUE_IMPLICIT_IDX')" + eplUnique;
            }

            env.CompileDeploy(eplUnique).AddListener("s0");

            SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(SUBQUERY_NUM_FIRST, null, backingTable);

            assertion.Invoke();

            env.UndeployAll();
        }

        private static SupportBean MakeBean(
            string theString,
            int intPrimitive,
            long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }

        private static void SendAssert(
            RegressionEnvironment env,
            string sbOneS1,
            int sbOneI1,
            string[] fields,
            object[] expected)
        {
            env.SendEventBean(new SupportSimpleBeanOne(sbOneS1, sbOneI1));
            EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, expected);
        }

        internal class EPLSubselectIndexChoicesOverdefinedWhere : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                // test no where clause with unique
                IndexAssertionEventSend assertNoWhere = () => {
                    var fields = new [] { "c0", "c1" };
                    env.SendEventBean(new SupportSimpleBeanTwo("E1", 1, 2, 3));

                    env.MilestoneInc(milestone);

                    env.SendEventBean(new SupportSimpleBeanOne("EX", 10, 11, 12));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"EX", "E1"});
                    env.SendEventBean(new SupportSimpleBeanTwo("E2", 1, 2, 3));

                    env.MilestoneInc(milestone);

                    env.SendEventBean(new SupportSimpleBeanOne("EY", 10, 11, 12));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"EY", null});
                };

                TryAssertion(env, false, "s2,i2", "", BACKING_UNINDEXED, assertNoWhere);

                // test no where clause with unique on multiple props, exact specification of where-clause
                IndexAssertionEventSend assertSendEvents = () => {
                    var fields = new [] { "c0", "c1" };
                    env.SendEventBean(new SupportSimpleBeanTwo("E1", 1, 3, 10));
                    env.SendEventBean(new SupportSimpleBeanTwo("E2", 1, 2, 0));
                    env.SendEventBean(new SupportSimpleBeanTwo("E3", 1, 3, 9));

                    env.MilestoneInc(milestone);

                    env.SendEventBean(new SupportSimpleBeanOne("EX", 1, 3, 9));
                    EPAssertionUtil.AssertProps(
                        env.Listener("s0").AssertOneGetNewAndReset(),
                        fields,
                        new object[] {"EX", "E3"});
                };

                TryAssertion(
                    env,
                    false,
                    "d2,i2",
                    "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1",
                    BACKING_MULTI_UNIQUE,
                    assertSendEvents);
                TryAssertion(
                    env,
                    false,
                    "d2,i2",
                    "where ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1",
                    BACKING_MULTI_UNIQUE,
                    assertSendEvents);
                TryAssertion(
                    env,
                    false,
                    "d2,i2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1 and ssb2.i2 = ssb1.i1",
                    BACKING_MULTI_UNIQUE,
                    assertSendEvents);
                TryAssertion(
                    env,
                    false,
                    "d2,i2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1",
                    BACKING_MULTI_DUPS,
                    assertSendEvents);
                TryAssertion(env, false, "d2,i2", "where ssb2.d2 = ssb1.d1", BACKING_SINGLE_DUPS, assertSendEvents);
                TryAssertion(
                    env,
                    false,
                    "d2,i2",
                    "where ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000",
                    BACKING_MULTI_UNIQUE,
                    assertSendEvents);
                TryAssertion(
                    env,
                    false,
                    "d2,i2",
                    "where ssb2.d2 = ssb1.d1 and ssb2.l2 between 1 and 1000",
                    BACKING_COMPOSITE,
                    assertSendEvents);
                TryAssertion(
                    env,
                    false,
                    "i2,d2,l2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.d2 = ssb1.d1",
                    BACKING_MULTI_DUPS,
                    assertSendEvents);
                TryAssertion(
                    env,
                    false,
                    "i2,d2,l2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1",
                    BACKING_MULTI_UNIQUE,
                    assertSendEvents);
                TryAssertion(
                    env,
                    false,
                    "d2,l2,i2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1",
                    BACKING_MULTI_UNIQUE,
                    assertSendEvents);
                TryAssertion(
                    env,
                    false,
                    "d2,l2,i2",
                    "where ssb2.l2 = ssb1.l1 and ssb2.i2 = ssb1.i1 and ssb2.d2 = ssb1.d1 and ssb2.s2 between 'E3' and 'E4'",
                    BACKING_MULTI_UNIQUE,
                    assertSendEvents);
                TryAssertion(env, false, "l2", "where ssb2.l2 = ssb1.l1", BACKING_SINGLE_UNIQUE, assertSendEvents);
                TryAssertion(env, true, "l2", "where ssb2.l2 = ssb1.l1", BACKING_SINGLE_DUPS, assertSendEvents);
                TryAssertion(
                    env,
                    false,
                    "l2",
                    "where ssb2.l2 = ssb1.l1 and ssb1.i1 between 1 and 20",
                    BACKING_SINGLE_UNIQUE,
                    assertSendEvents);

                // greater
                IndexAssertionEventSend assertGreater = () => {
                    var fields = new [] { "c0", "c1" };
                    env.SendEventBean(new SupportSimpleBeanTwo("E1", 1));
                    env.SendEventBean(new SupportSimpleBeanTwo("E2", 2));

                    env.MilestoneInc(milestone);

                    SendAssert(
                        env,
                        "A",
                        1,
                        fields,
                        new object[] {"A", null});
                    SendAssert(
                        env,
                        "B",
                        2,
                        fields,
                        new object[] {"B", "E1"});
                    SendAssert(
                        env,
                        "C",
                        3,
                        fields,
                        new object[] {"C", null});
                    SendAssert(
                        env,
                        "D",
                        4,
                        fields,
                        new object[] {"D", null});
                    SendAssert(
                        env,
                        "E",
                        5,
                        fields,
                        new object[] {"E", null});
                };
                TryAssertion(env, false, "s2", "where ssb1.i1 > ssb2.i2", BACKING_SORTED, assertGreater);

                // greater-equals
                IndexAssertionEventSend assertGreaterEquals = () => {
                    var fields = new [] { "c0", "c1" };
                    env.SendEventBean(new SupportSimpleBeanTwo("E1", 2));
                    env.SendEventBean(new SupportSimpleBeanTwo("E2", 4));

                    env.MilestoneInc(milestone);

                    SendAssert(
                        env,
                        "A",
                        1,
                        fields,
                        new object[] {"A", null});
                    SendAssert(
                        env,
                        "B",
                        2,
                        fields,
                        new object[] {"B", "E1"});
                    SendAssert(
                        env,
                        "C",
                        3,
                        fields,
                        new object[] {"C", "E1"});
                    SendAssert(
                        env,
                        "D",
                        4,
                        fields,
                        new object[] {"D", null});
                    SendAssert(
                        env,
                        "E",
                        5,
                        fields,
                        new object[] {"E", null});
                };
                TryAssertion(env, false, "s2", "where ssb1.i1 >= ssb2.i2", BACKING_SORTED, assertGreaterEquals);

                // less
                IndexAssertionEventSend assertLess = () => {
                    var fields = new [] { "c0", "c1" };
                    env.SendEventBean(new SupportSimpleBeanTwo("E1", 2));
                    env.SendEventBean(new SupportSimpleBeanTwo("E2", 3));

                    env.MilestoneInc(milestone);

                    SendAssert(
                        env,
                        "A",
                        1,
                        fields,
                        new object[] {"A", null});
                    SendAssert(
                        env,
                        "B",
                        2,
                        fields,
                        new object[] {"B", "E2"});
                    SendAssert(
                        env,
                        "C",
                        3,
                        fields,
                        new object[] {"C", null});
                    SendAssert(
                        env,
                        "D",
                        4,
                        fields,
                        new object[] {"D", null});
                    SendAssert(
                        env,
                        "E",
                        5,
                        fields,
                        new object[] {"E", null});
                };
                TryAssertion(env, false, "s2", "where ssb1.i1 < ssb2.i2", BACKING_SORTED, assertLess);

                // less-equals
                IndexAssertionEventSend assertLessEquals = () => {
                    var fields = new [] { "c0", "c1" };
                    env.SendEventBean(new SupportSimpleBeanTwo("E1", 1));
                    env.SendEventBean(new SupportSimpleBeanTwo("E2", 3));

                    env.MilestoneInc(milestone);

                    SendAssert(
                        env,
                        "A",
                        1,
                        fields,
                        new object[] {"A", null});
                    SendAssert(
                        env,
                        "B",
                        2,
                        fields,
                        new object[] {"B", "E2"});
                    SendAssert(
                        env,
                        "C",
                        3,
                        fields,
                        new object[] {"C", "E2"});
                    SendAssert(
                        env,
                        "D",
                        4,
                        fields,
                        new object[] {"D", null});
                    SendAssert(
                        env,
                        "E",
                        5,
                        fields,
                        new object[] {"E", null});
                };
                TryAssertion(env, false, "s2", "where ssb1.i1 <= ssb2.i2", BACKING_SORTED, assertLessEquals);
            }
        }

        internal class EPLSubselectUniqueIndexCorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1" };
                var milestone = new AtomicLong();

                // test std:unique
                SupportQueryPlanIndexHook.Reset();
                var eplUnique = INDEX_CALLBACK_HOOK +
                                "@Name('s0') select Id as c0, " +
                                "(select IntPrimitive from SupportBean#unique(TheString) where TheString = S0.P00) as c1 " +
                                "from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMile(eplUnique, "s0", milestone.GetAndIncrement());

                SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(
                    SUBQUERY_NUM_FIRST,
                    null,
                    BACKING_SINGLE_UNIQUE);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E1", 3));
                env.SendEventBean(new SupportBean("E2", 4));

                env.SendEventBean(new SupportBean_S0(10, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 4});

                env.SendEventBean(new SupportBean_S0(11, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11, 3});

                env.UndeployAll();

                // test std:firstunique
                SupportQueryPlanIndexHook.Reset();
                var eplFirstUnique = INDEX_CALLBACK_HOOK +
                                     "@Name('s0') select Id as c0, " +
                                     "(select IntPrimitive from SupportBean#firstunique(TheString) where TheString = S0.P00) as c1 " +
                                     "from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMile(eplFirstUnique, "s0", milestone.GetAndIncrement());

                SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(
                    SUBQUERY_NUM_FIRST,
                    null,
                    BACKING_SINGLE_UNIQUE);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.SendEventBean(new SupportBean("E1", 3));
                env.SendEventBean(new SupportBean("E2", 4));

                env.SendEventBean(new SupportBean_S0(10, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 2});

                env.SendEventBean(new SupportBean_S0(11, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11, 1});

                env.UndeployAll();

                // test intersection std:firstunique
                SupportQueryPlanIndexHook.Reset();
                var eplIntersection = INDEX_CALLBACK_HOOK +
                                      "@Name('s0') select Id as c0, " +
                                      "(select IntPrimitive from SupportBean#time(1)#unique(TheString) where TheString = S0.P00) as c1 " +
                                      "from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMile(eplIntersection, "s0", milestone.GetAndIncrement());

                SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(
                    SUBQUERY_NUM_FIRST,
                    null,
                    BACKING_SINGLE_UNIQUE);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E1", 2));
                env.SendEventBean(new SupportBean("E1", 3));
                env.SendEventBean(new SupportBean("E2", 4));

                env.SendEventBean(new SupportBean_S0(10, "E2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {10, 4});

                env.SendEventBean(new SupportBean_S0(11, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {11, 3});

                env.UndeployAll();

                // test grouped unique
                SupportQueryPlanIndexHook.Reset();
                var eplGrouped = INDEX_CALLBACK_HOOK +
                                 "@Name('s0') select Id as c0, " +
                                 "(select LongPrimitive from SupportBean#groupwin(TheString)#unique(IntPrimitive) where TheString = S0.P00 and IntPrimitive = S0.Id) as c1 " +
                                 "from SupportBean_S0 as S0";
                env.CompileDeployAddListenerMile(eplGrouped, "s0", milestone.GetAndIncrement());

                SupportQueryPlanIndexHook.AssertSubqueryBackingAndReset(SUBQUERY_NUM_FIRST, null, BACKING_MULTI_UNIQUE);

                env.SendEventBean(MakeBean("E1", 1, 100));
                env.SendEventBean(MakeBean("E1", 2, 101));
                env.SendEventBean(MakeBean("E1", 1, 102));

                env.SendEventBean(new SupportBean_S0(1, "E1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, 102L});

                env.UndeployAll();
            }
        }
    }
} // end of namespace