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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextKeySegmentedAggregate
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextKeySegmentedRowForAll());
            execs.Add(new ContextKeySegmentedAccessOnly());
            execs.Add(new ContextKeySegmentedSubqueryWithAggregation());
            execs.Add(new ContextKeySegmentedRowPerGroupStream());
            execs.Add(new ContextKeySegmentedRowPerGroupBatchContextProp());
            execs.Add(new ContextKeySegmentedRowPerGroupWithAccess());
            execs.Add(new ContextKeySegmentedRowPerGroupUnidirectionalJoin());
            execs.Add(new ContextKeySegmentedRowPerEvent());
            execs.Add(new ContextKeySegmentedRowPerGroup3Stmts());
            return execs;
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

        public static object ToArray(ICollection<EventBean> input)
        {
            return input.ToArray();
        }

        internal class ContextKeySegmentedAccessOnly : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContext =
                    "@Name('CTX') create context SegmentedByString partition by TheString from SupportBean";
                env.CompileDeploy(eplContext, path);

                var fieldsGrouped = new [] { "TheString","IntPrimitive","col1" };
                var eplGroupedAccess =
                    "@Name('s0') context SegmentedByString select TheString,IntPrimitive,window(LongPrimitive) as col1 from SupportBean#keepall sb group by IntPrimitive";
                env.CompileDeploy(eplGroupedAccess, path);

                env.AddListener("s0");

                env.SendEventBean(MakeEvent("G1", 1, 10L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsGrouped,
                    new object[] {
                        "G1", 1,
                        new object[] {10L}
                    });

                env.Milestone(0);

                env.SendEventBean(MakeEvent("G1", 2, 100L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsGrouped,
                    new object[] {
                        "G1", 2,
                        new object[] {100L}
                    });

                env.SendEventBean(MakeEvent("G2", 1, 200L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsGrouped,
                    new object[] {
                        "G2", 1,
                        new object[] {200L}
                    });

                env.Milestone(1);

                env.SendEventBean(MakeEvent("G1", 1, 11L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsGrouped,
                    new object[] {
                        "G1", 1,
                        new object[] {10L, 11L}
                    });

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedSubqueryWithAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by TheString from SupportBean",
                    path);

                string[] fields = {"TheString", "IntPrimitive", "val0"};
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select TheString, IntPrimitive, (select count(*) from SupportBean_S0#keepall as S0 where sb.IntPrimitive = S0.Id) as val0 " +
                    "from SupportBean as sb",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean_S0(10, "s1"));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 10, 0L});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedRowPerGroupStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fieldsOne = new [] { "IntPrimitive","count(*)" };
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString select IntPrimitive, count(*) from SupportBean group by IntPrimitive",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {10, 1L});

                env.SendEventBean(new SupportBean("G2", 200));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {200, 1L});

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {10, 2L});

                env.SendEventBean(new SupportBean("G1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {11, 1L});

                env.SendEventBean(new SupportBean("G2", 200));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {200, 2L});

                env.SendEventBean(new SupportBean("G2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {10, 1L});

                env.UndeployModuleContaining("s0");

                // add "string" : a context property
                var fieldsTwo = new [] { "TheString","IntPrimitive","count(*)" };
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString select TheString, IntPrimitive, count(*) from SupportBean group by IntPrimitive",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {"G1", 10, 1L});

                env.SendEventBean(new SupportBean("G2", 200));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {"G2", 200, 1L});

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {"G1", 10, 2L});

                env.SendEventBean(new SupportBean("G1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {"G1", 11, 1L});

                env.SendEventBean(new SupportBean("G2", 200));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {"G2", 200, 2L});

                env.SendEventBean(new SupportBean("G2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {"G2", 10, 1L});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedRowPerGroupBatchContextProp : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fieldsOne = new [] { "IntPrimitive","count(*)" };
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString select IntPrimitive, count(*) from SupportBean#length_batch(2) group by IntPrimitive order by IntPrimitive asc",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G2", 200));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsOne,
                    new object[] {10, 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").GetAndResetLastNewData()[1],
                    fieldsOne,
                    new object[] {11, 1L});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G1", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G2", 200));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {200, 2L});

                env.Milestone(3);

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsOne,
                    new object[] {10, 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").GetAndResetLastNewData()[1],
                    fieldsOne,
                    new object[] {11, 0L});

                env.Milestone(4);

                env.SendEventBean(new SupportBean("G2", 10));
                env.SendEventBean(new SupportBean("G2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsOne,
                    new object[] {10, 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").GetAndResetLastNewData()[1],
                    fieldsOne,
                    new object[] {200, 0L});

                env.UndeployModuleContaining("s0");

                // add "string" : add context property
                var fieldsTwo = new [] { "TheString","IntPrimitive","count(*)" };
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString select TheString, IntPrimitive, count(*) from SupportBean#length_batch(2) group by IntPrimitive order by TheString, IntPrimitive asc",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean("G2", 200));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(5);

                env.SendEventBean(new SupportBean("G1", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsTwo,
                    new object[] {"G1", 10, 1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").GetAndResetLastNewData()[1],
                    fieldsTwo,
                    new object[] {"G1", 11, 1L});

                env.SendEventBean(new SupportBean("G1", 10));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(6);

                env.SendEventBean(new SupportBean("G2", 200));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {"G2", 200, 2L});

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsTwo,
                    new object[] {"G1", 10, 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").GetAndResetLastNewData()[1],
                    fieldsTwo,
                    new object[] {"G1", 11, 0L});

                env.Milestone(7);

                env.SendEventBean(new SupportBean("G2", 10));
                env.SendEventBean(new SupportBean("G2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fieldsTwo,
                    new object[] {"G2", 10, 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").GetAndResetLastNewData()[1],
                    fieldsTwo,
                    new object[] {"G2", 200, 0L});

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedRowPerGroupWithAccess : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fieldsOne = new [] { "IntPrimitive","col1","col2","col3" };
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select IntPrimitive, count(*) as col1, toArray(window(*).selectFrom(v->v.LongPrimitive)) as col2, first().LongPrimitive as col3 " +
                    "from SupportBean#keepall as sb " +
                    "group by IntPrimitive order by IntPrimitive asc",
                    path);
                env.AddListener("s0");

                env.SendEventBean(MakeEvent("G1", 10, 200L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {
                        10, 1L,
                        new object[] {200L}, 200L
                    });

                env.SendEventBean(MakeEvent("G1", 10, 300L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {
                        10, 2L,
                        new object[] {200L, 300L}, 200L
                    });

                env.Milestone(0);

                env.SendEventBean(MakeEvent("G2", 10, 1000L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {
                        10, 1L,
                        new object[] {1000L}, 1000L
                    });

                env.Milestone(1);

                env.SendEventBean(MakeEvent("G2", 10, 1010L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {
                        10, 2L,
                        new object[] {1000L, 1010L}, 1000L
                    });

                env.UndeployModuleContaining("s0");
                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedRowForAll : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                var fieldsOne = new [] { "col1" };
                var path = new RegressionPath();

                var eplCtx =
                    "@Name('context') create context SegmentedByString partition by TheString from SupportBean";
                env.CompileDeploy(eplCtx, path);

                var epl = "@Name('s0') context SegmentedByString select sum(IntPrimitive) as col1 from SupportBean;\n";
                env.CompileDeploy(epl, path).AddListener("s0");

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("G1", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {3});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("G2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {2});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("G1", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {7});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("G2", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {3});

                env.MilestoneInc(milestone);

                env.SendEventBean(new SupportBean("G3", -1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {-1});

                env.MilestoneInc(milestone);

                env.UndeployModuleContaining("s0");

                // test mixed with access
                var fieldsTwo = new [] { "col1","col2" };
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select sum(IntPrimitive) as col1, toArray(window(*).selectFrom(v->v.IntPrimitive)) as col2 " +
                    "from SupportBean#keepall",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 8));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {
                        8,
                        new object[] {8}
                    });

                env.SendEventBean(new SupportBean("G2", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {
                        5,
                        new object[] {5}
                    });

                env.SendEventBean(new SupportBean("G1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {
                        9,
                        new object[] {8, 1}
                    });

                env.SendEventBean(new SupportBean("G2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {
                        7,
                        new object[] {5, 2}
                    });

                env.UndeployModuleContaining("s0");

                // test only access
                var fieldsThree = new [] { "col1" };
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select toArray(window(*).selectFrom(v->v.IntPrimitive)) as col1 " +
                    "from SupportBean#keepall",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 8));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsThree,
                    new object[] {new object[] {8}});

                env.SendEventBean(new SupportBean("G2", 5));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsThree,
                    new object[] {new object[] {5}});

                env.SendEventBean(new SupportBean("G1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsThree,
                    new object[] {new object[] {8, 1}});

                env.SendEventBean(new SupportBean("G2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsThree,
                    new object[] {new object[] {5, 2}});

                env.UndeployModuleContaining("s0");

                // test subscriber
                var stmtFour = env.CompileDeploy(
                        "@Name('s0') context SegmentedByString " +
                        "select count(*) as col1 " +
                        "from SupportBean",
                        path)
                    .Statement("s0");
                var subs = new SupportSubscriber();
                stmtFour.Subscriber = subs;

                env.SendEventBean(new SupportBean("G1", 1));
                Assert.AreEqual(1L, subs.AssertOneGetNewAndReset());

                env.SendEventBean(new SupportBean("G1", 1));
                Assert.AreEqual(2L, subs.AssertOneGetNewAndReset());

                env.SendEventBean(new SupportBean("G2", 2));
                Assert.AreEqual(1L, subs.AssertOneGetNewAndReset());

                env.UndeployAll();
            }
        }

        internal class ContextKeySegmentedRowPerGroupUnidirectionalJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByString partition by TheString from SupportBean",
                    path);

                var fieldsOne = new [] { "IntPrimitive","col1" };
                env.CompileDeploy(
                    "@Name('s0') context SegmentedByString " +
                    "select IntPrimitive, count(*) as col1 " +
                    "from SupportBean unidirectional, SupportBean_S0#keepall " +
                    "group by IntPrimitive order by IntPrimitive asc",
                    path);
                env.AddListener("s0");

                env.SendEventBean(new SupportBean("G1", 10));
                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S0(2));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {10, 2L});

                env.Milestone(0);

                env.SendEventBean(new SupportBean_S0(3));

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {10, 3L});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("G2", 20));
                env.SendEventBean(new SupportBean_S0(4));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("G2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {20, 1L});

                env.SendEventBean(new SupportBean_S0(5));

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G2", 20));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {20, 2L});

                env.SendEventBean(new SupportBean("G1", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsOne,
                    new object[] {10, 5L});

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
                    "@Name('CTX') create context SegmentedByString partition by TheString from SupportBean";
                env.CompileDeploy(eplContext, path);

                var fields = new [] { "TheString","col1" };
                var eplUngrouped =
                    "@Name('S1') context SegmentedByString select TheString,sum(IntPrimitive) as col1 from SupportBean";
                env.CompileDeploy(eplUngrouped, path).AddListener("S1");

                var eplGroupedAccess =
                    "@Name('S2') context SegmentedByString select TheString,window(IntPrimitive) as col1 from SupportBean#keepall() sb";
                env.CompileDeploy(eplGroupedAccess, path).AddListener("S2");

                env.Milestone(0);

                env.SendEventBean(new SupportBean("G1", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 2});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G1",
                        new object[] {2}
                    });

                env.SendEventBean(new SupportBean("G1", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 5});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G1",
                        new object[] {2, 3}
                    });
                AssertPartitionInfo(env);

                env.Milestone(1);

                AssertPartitionInfo(env);
                env.SendEventBean(new SupportBean("G2", 10));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 10});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G2",
                        new object[] {10}
                    });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("G2", 11));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 21});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G2",
                        new object[] {10, 11}
                    });

                env.Milestone(3);

                env.SendEventBean(new SupportBean("G1", 4));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 9});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G1",
                        new object[] {2, 3, 4}
                    });

                env.Milestone(4);

                env.SendEventBean(new SupportBean("G3", 100));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 100});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G3",
                        new object[] {100}
                    });

                env.Milestone(5);

                env.SendEventBean(new SupportBean("G3", 101));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 201});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G3",
                        new object[] {100, 101}
                    });

                env.UndeployModuleContaining("S1");
                env.UndeployModuleContaining("S2");
                env.UndeployModuleContaining("CTX");
            }

            private void AssertPartitionInfo(RegressionEnvironment env)
            {
                var partitionAdmin = env.Runtime.ContextPartitionService;
                var partitions = partitionAdmin.GetContextPartitions(
                    env.DeploymentId("CTX"),
                    "SegmentedByString",
                    ContextPartitionSelectorAll.INSTANCE);
                Assert.AreEqual(1, partitions.Identifiers.Count);
                var ident = (ContextPartitionIdentifierPartitioned) partitions.Identifiers.Values.First();
                EPAssertionUtil.AssertEqualsExactOrder(new[] {"G1"}, ident.Keys);
            }
        }

        public class ContextKeySegmentedRowPerGroup3Stmts : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplContext =
                    "@Name('CTX') create context SegmentedByString partition by TheString from SupportBean";
                env.CompileDeploy(eplContext, path);

                var fields = new [] { "TheString","IntPrimitive","col1" };
                var eplGrouped =
                    "@Name('S1') context SegmentedByString select TheString,IntPrimitive,sum(LongPrimitive) as col1 from SupportBean group by IntPrimitive";
                env.CompileDeploy(eplGrouped, path).AddListener("S1");

                var eplGroupedAccess =
                    "@Name('S2') context SegmentedByString select TheString,IntPrimitive,window(LongPrimitive) as col1 from SupportBean.win:keepall() sb group by IntPrimitive";
                env.CompileDeploy(eplGroupedAccess, path).AddListener("S2");

                var eplGroupedDistinct =
                    "@Name('S3') context SegmentedByString select TheString,IntPrimitive,sum(distinct LongPrimitive) as col1 from SupportBean.win:keepall() sb group by IntPrimitive";
                env.CompileDeploy(eplGroupedDistinct, path).AddListener("S3");

                env.SendEventBean(MakeEvent("G1", 1, 10L));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 1, 10L});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G1", 1,
                        new object[] {10L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 1, 10L});

                env.Milestone(0);

                env.SendEventBean(MakeEvent("G2", 1, 25L));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 1, 25L});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G2", 1,
                        new object[] {25L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 1, 25L});

                env.Milestone(1);

                env.SendEventBean(MakeEvent("G1", 2, 2L));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 2, 2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G1", 2,
                        new object[] {2L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 2, 2L});

                env.Milestone(2);

                env.SendEventBean(MakeEvent("G2", 2, 100L));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 2, 100L});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G2", 2,
                        new object[] {100L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 2, 100L});

                env.Milestone(3);

                env.SendEventBean(MakeEvent("G1", 1, 10L));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 1, 20L});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G1", 1,
                        new object[] {10L, 10L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 1, 10L});

                env.Milestone(4);

                env.SendEventBean(MakeEvent("G1", 2, 3L));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 2, 5L});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G1", 2,
                        new object[] {2L, 3L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 2, 5L});

                env.Milestone(5);

                env.SendEventBean(MakeEvent("G2", 2, 101L));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 2, 201L});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G2", 2,
                        new object[] {100L, 101L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G2", 2, 201L});

                env.Milestone(6);

                env.SendEventBean(MakeEvent("G3", 1, -1L));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 1, -1L});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G3", 1,
                        new object[] {-1L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 1, -1L});

                env.Milestone(7);

                env.SendEventBean(MakeEvent("G3", 2, -2L));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 2, -2L});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G3", 2,
                        new object[] {-2L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 2, -2L});

                env.Milestone(8);

                env.SendEventBean(MakeEvent("G3", 1, -3L));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 1, -4L});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G3", 1,
                        new object[] {-1L, -3L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G3", 1, -4L});

                env.Milestone(9);

                env.SendEventBean(MakeEvent("G1", 2, 3L));
                EPAssertionUtil.AssertProps(
                    env.Listener("S1").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 2, 8L});
                EPAssertionUtil.AssertProps(
                    env.Listener("S2").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {
                        "G1", 2,
                        new object[] {2L, 3L, 3L}
                    });
                EPAssertionUtil.AssertProps(
                    env.Listener("S3").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"G1", 2, 5L});

                env.UndeployAll();
            }
        }
    }
} // end of namespace