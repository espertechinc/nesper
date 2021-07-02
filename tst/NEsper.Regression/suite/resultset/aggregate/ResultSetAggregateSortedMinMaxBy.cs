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

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateSortedMinMaxBy
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithGroupedSortedMinMax(execs);
            WithMultipleOverlappingCategories(execs);
            WithMinByMaxByOverWindow(execs);
            WithNoAlias(execs);
            WithMultipleCriteriaSimple(execs);
            WithMultipleCriteria(execs);
            WithNoDataWindow(execs);
            WithInvalid(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithNoDataWindow(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateNoDataWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithMultipleCriteria(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMultipleCriteria());
            return execs;
        }

        public static IList<RegressionExecution> WithMultipleCriteriaSimple(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMultipleCriteriaSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithNoAlias(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateNoAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithMinByMaxByOverWindow(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMinByMaxByOverWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithMultipleOverlappingCategories(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMultipleOverlappingCategories());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedSortedMinMax(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateGroupedSortedMinMax());
            return execs;
        }

        private static void TryAssertionGroupedSortedMinMax(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = new[] {"c0", "c1", "c2", "c3", "c4", "c5", "c6"};
            var eventOne = MakeEvent("E1", 1, 1);
            env.SendEventBean(eventOne);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {
                    new object[] {eventOne},
                    new object[] {eventOne},
                    new object[] {eventOne},
                    eventOne, eventOne, eventOne, eventOne
                });

            env.MilestoneInc(milestone);

            var eventTwo = MakeEvent("E2", 2, 1);
            env.SendEventBean(eventTwo);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {
                    new object[] {eventOne, eventTwo},
                    new object[] {eventTwo, eventOne},
                    new object[] {eventOne, eventTwo},
                    eventTwo, eventOne, eventTwo, eventOne
                });

            env.MilestoneInc(milestone);

            var eventThree = MakeEvent("E3", 0, 1);
            env.SendEventBean(eventThree);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {
                    new object[] {eventOne, eventTwo, eventThree},
                    new object[] {eventTwo, eventOne, eventThree},
                    new object[] {eventThree, eventOne, eventTwo},
                    eventTwo, eventThree, eventTwo, eventThree
                });

            env.MilestoneInc(milestone);

            var eventFour = MakeEvent("E4", 3, 1); // pushes out E1
            env.SendEventBean(eventFour);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {
                    new object[] {eventTwo, eventThree, eventFour},
                    new object[] {eventFour, eventTwo, eventThree},
                    new object[] {eventThree, eventTwo, eventFour},
                    eventFour, eventThree, eventFour, eventThree
                });

            var eventFive = MakeEvent("E5", -1, 2); // group 2
            env.SendEventBean(eventFive);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {
                    new object[] {eventFive},
                    new object[] {eventFive},
                    new object[] {eventFive},
                    eventFive, eventFive, eventFive, eventFive
                });

            var eventSix = MakeEvent("E6", -1, 1); // pushes out E2
            env.SendEventBean(eventSix);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {
                    new object[] {eventThree, eventFour, eventSix},
                    new object[] {eventFour, eventThree, eventSix},
                    new object[] {eventSix, eventThree, eventFour},
                    eventFour, eventSix, eventFour, eventSix
                });

            env.MilestoneInc(milestone);

            var eventSeven = MakeEvent("E7", 2, 2); // group 2
            env.SendEventBean(eventSeven);
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {
                    new object[] {eventFive, eventSeven},
                    new object[] {eventSeven, eventFive},
                    new object[] {eventFive, eventSeven},
                    eventSeven, eventFive, eventSeven, eventFive
                });
        }

        private static SupportBean MakeEvent(
            string @string,
            int intPrimitive,
            long longPrimitive)
        {
            var @event = new SupportBean(@string, intPrimitive);
            @event.LongPrimitive = longPrimitive;
            return @event;
        }

        private static void AssertExpected(
            RegressionEnvironment env,
            object[][] expected)
        {
            var und = (SupportBean[]) env.Listener("s0").AssertOneGetNewAndReset().Get("c0");
            for (var i = 0; i < und.Length; i++) {
                Assert.AreEqual(expected[i][0], und[i].TheString);
                Assert.AreEqual(expected[i][1], und[i].IntPrimitive);
            }
        }

        public class ResultSetAggregateMultipleCriteriaSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select sorted(TheString desc, IntPrimitive desc) as c0 from SupportBean#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                Assert.AreEqual(typeof(SupportBean[]), env.Statement("s0").EventType.GetPropertyType("c0"));
                
                env.SendEventBean(new SupportBean("C", 10));
                AssertExpected(
                    env,
                    new[] {new object[] {"C", 10}});

                env.Milestone(0);

                env.SendEventBean(new SupportBean("D", 20));
                AssertExpected(
                    env,
                    new[] {new object[] {"D", 20}, new object[] {"C", 10}});

                env.Milestone(1);

                env.SendEventBean(new SupportBean("C", 15));
                AssertExpected(
                    env,
                    new[] {new object[] {"D", 20}, new object[] {"C", 15}, new object[] {"C", 10}});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("D", 19));
                AssertExpected(
                    env,
                    new[] {
                        new object[] {"D", 20}, new object[] {"D", 19}, new object[] {"C", 15}, new object[] {"C", 10}
                    });

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateGroupedSortedMinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@Name('s0') select " +
                          "window(*) as c0, " +
                          "sorted(IntPrimitive desc) as c1, " +
                          "sorted(IntPrimitive asc) as c2, " +
                          "maxby(IntPrimitive) as c3, " +
                          "minby(IntPrimitive) as c4, " +
                          "maxbyever(IntPrimitive) as c5, " +
                          "minbyever(IntPrimitive) as c6 " +
                          "from SupportBean#groupwin(LongPrimitive)#length(3) " +
                          "group by LongPrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionGroupedSortedMinMax(env, milestone);

                env.UndeployAll();

                // test SODA
                env.EplToModelCompileDeploy(epl).AddListener("s0");
                TryAssertionGroupedSortedMinMax(env, milestone);
                env.UndeployAll();

                // test join
                var eplJoin = "@Name('s0') select " +
                              "window(sb.*) as c0, " +
                              "sorted(IntPrimitive desc) as c1, " +
                              "sorted(IntPrimitive asc) as c2, " +
                              "maxby(IntPrimitive) as c3, " +
                              "minby(IntPrimitive) as c4, " +
                              "maxbyever(IntPrimitive) as c5, " +
                              "minbyever(IntPrimitive) as c6 " +
                              "from SupportBean_S0#lastevent, SupportBean#groupwin(LongPrimitive)#length(3) as sb " +
                              "group by LongPrimitive";
                env.CompileDeploy(eplJoin).AddListener("s0");
                env.SendEventBean(new SupportBean_S0(1, "P00"));
                TryAssertionGroupedSortedMinMax(env, milestone);
                env.UndeployAll();

                // test join multirow
                var fields = new[] {"c0"};
                var joinMultirow =
                    "@Name('s0') select sorted(IntPrimitive desc) as c0 from SupportBean_S0#keepall, SupportBean#length(2)";
                env.CompileDeploy(joinMultirow).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "S1"));
                env.SendEventBean(new SupportBean_S0(2, "S2"));
                env.SendEventBean(new SupportBean_S0(3, "S3"));

                env.MilestoneInc(milestone);

                var eventOne = new SupportBean("E1", 1);
                env.SendEventBean(eventOne);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {new object[] {eventOne}});

                env.MilestoneInc(milestone);

                var eventTwo = new SupportBean("E2", 2);
                env.SendEventBean(eventTwo);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {new object[] {eventTwo, eventOne}});

                env.MilestoneInc(milestone);

                var eventThree = new SupportBean("E3", 0);
                env.SendEventBean(eventThree);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {new object[] {eventTwo, eventThree}});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMinByMaxByOverWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9"};
                var epl = "@Name('s0') select " +
                          "maxbyever(LongPrimitive) as c0, " +
                          "minbyever(LongPrimitive) as c1, " +
                          "maxby(LongPrimitive).LongPrimitive as c2, " +
                          "maxby(LongPrimitive).TheString as c3, " +
                          "maxby(LongPrimitive).IntPrimitive as c4, " +
                          "maxby(LongPrimitive) as c5, " +
                          "minby(LongPrimitive).LongPrimitive as c6, " +
                          "minby(LongPrimitive).TheString as c7, " +
                          "minby(LongPrimitive).IntPrimitive as c8, " +
                          "minby(LongPrimitive) as c9 " +
                          "from SupportBean#length(5)";
                env.CompileDeploy(epl).AddListener("s0");

                var eventOne = MakeEvent("E1", 1, 10);
                env.SendEventBean(eventOne);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {eventOne, eventOne, 10L, "E1", 1, eventOne, 10L, "E1", 1, eventOne});

                var eventTwo = MakeEvent("E2", 2, 20);
                env.SendEventBean(eventTwo);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {eventTwo, eventOne, 20L, "E2", 2, eventTwo, 10L, "E1", 1, eventOne});

                env.Milestone(0);

                var eventThree = MakeEvent("E3", 3, 5);
                env.SendEventBean(eventThree);
                object[] resultThree = {eventTwo, eventThree, 20L, "E2", 2, eventTwo, 5L, "E3", 3, eventThree};
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, resultThree);

                var eventFour = MakeEvent("E4", 4, 5);
                env.SendEventBean(eventFour); // same as E3
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, resultThree);

                env.Milestone(1);

                var eventFive = MakeEvent("E5", 5, 20);
                env.SendEventBean(eventFive); // same as E2
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, resultThree);

                var eventSix = MakeEvent("E6", 6, 10);
                env.SendEventBean(eventSix); // expires E1
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, resultThree);

                var eventSeven = MakeEvent("E7", 7, 20);
                env.SendEventBean(eventSeven); // expires E2
                object[] resultSeven = {eventTwo, eventThree, 20L, "E5", 5, eventFive, 5L, "E3", 3, eventThree};
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, resultSeven);

                env.Milestone(2);

                env.SendEventBean(MakeEvent("E8", 8, 20)); // expires E3
                object[] resultEight = {eventTwo, eventThree, 20L, "E5", 5, eventFive, 5L, "E4", 4, eventFour};
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, resultEight);

                env.SendEventBean(MakeEvent("E9", 9, 19)); // expires E4
                object[] resultNine = {eventTwo, eventThree, 20L, "E5", 5, eventFive, 10L, "E6", 6, eventSix};
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, resultNine);

                env.SendEventBean(MakeEvent("E10", 10, 12)); // expires E5
                object[] resultTen = {eventTwo, eventThree, 20L, "E7", 7, eventSeven, 10L, "E6", 6, eventSix};
                EPAssertionUtil.AssertProps(env.Listener("s0").AssertOneGetNewAndReset(), fields, resultTen);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select " +
                          "maxby(IntPrimitive).TheString, " +
                          "minby(IntPrimitive)," +
                          "maxbyever(IntPrimitive).TheString, " +
                          "minbyever(IntPrimitive)," +
                          "sorted(IntPrimitive asc, TheString desc)" +
                          " from SupportBean#time(10)";
                env.CompileDeploy(epl).AddListener("s0");

                var props = env.Statement("s0").EventType.PropertyDescriptors;
                Assert.AreEqual("maxby(IntPrimitive).TheString", props[0].PropertyName);
                Assert.AreEqual("minby(IntPrimitive)", props[1].PropertyName);
                Assert.AreEqual("maxbyever(IntPrimitive).TheString", props[2].PropertyName);
                Assert.AreEqual("minbyever(IntPrimitive)", props[3].PropertyName);
                Assert.AreEqual("sorted(IntPrimitive,TheString desc)", props[4].PropertyName);

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMultipleOverlappingCategories : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7"};
                var epl = "@Name('s0') select " +
                          "maxbyever(IntPrimitive).LongPrimitive as c0," +
                          "maxbyever(TheString).LongPrimitive as c1," +
                          "minbyever(IntPrimitive).LongPrimitive as c2," +
                          "minbyever(TheString).LongPrimitive as c3," +
                          "maxby(IntPrimitive).LongPrimitive as c4," +
                          "maxby(TheString).LongPrimitive as c5," +
                          "minby(IntPrimitive).LongPrimitive as c6," +
                          "minby(TheString).LongPrimitive as c7 " +
                          "from SupportBean#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(MakeEvent("C", 10, 1L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L});

                env.SendEventBean(MakeEvent("P", 5, 2L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L});

                env.Milestone(0);

                env.SendEventBean(MakeEvent("G", 7, 3L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L});

                env.SendEventBean(MakeEvent("A", 7, 4L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L, 2L, 2L, 4L, 1L, 2L, 2L, 4L});

                env.Milestone(1);

                env.SendEventBean(MakeEvent("G", 1, 5L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L, 2L, 5L, 4L, 1L, 2L, 5L, 4L});

                env.SendEventBean(MakeEvent("X", 7, 6L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L, 6L, 5L, 4L, 1L, 6L, 5L, 4L});

                env.Milestone(2);

                env.SendEventBean(MakeEvent("G", 100, 7L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {7L, 6L, 5L, 4L, 7L, 6L, 5L, 4L});

                env.SendEventBean(MakeEvent("Z", 1000, 8L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {8L, 8L, 5L, 4L, 8L, 8L, 5L, 4L});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateMultipleCriteria : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                string epl;

                // test sorted multiple criteria
                var fields = new[] {"c0", "c1", "c2", "c3"};
                epl = "@Name('s0') select " +
                      "sorted(TheString desc, IntPrimitive desc) as c0," +
                      "sorted(TheString, IntPrimitive) as c1," +
                      "sorted(TheString asc, IntPrimitive asc) as c2," +
                      "sorted(TheString desc, IntPrimitive asc) as c3 " +
                      "from SupportBean#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                var eventOne = new SupportBean("C", 10);
                env.SendEventBean(eventOne);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {
                        new object[] {eventOne},
                        new object[] {eventOne},
                        new object[] {eventOne},
                        new object[] {eventOne}
                    });

                env.MilestoneInc(milestone);

                var eventTwo = new SupportBean("D", 20);
                env.SendEventBean(eventTwo);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {
                        new object[] {eventTwo, eventOne},
                        new object[] {eventOne, eventTwo},
                        new object[] {eventOne, eventTwo},
                        new object[] {eventTwo, eventOne}
                    });

                var eventThree = new SupportBean("C", 15);
                env.SendEventBean(eventThree);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {
                        new object[] {eventTwo, eventThree, eventOne},
                        new object[] {eventOne, eventThree, eventTwo},
                        new object[] {eventOne, eventThree, eventTwo},
                        new object[] {eventTwo, eventOne, eventThree}
                    });

                env.MilestoneInc(milestone);

                var eventFour = new SupportBean("D", 19);
                env.SendEventBean(eventFour);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new[] {
                        new object[] {eventTwo, eventFour, eventThree, eventOne},
                        new object[] {eventOne, eventThree, eventFour, eventTwo},
                        new object[] {eventOne, eventThree, eventFour, eventTwo},
                        new object[] {eventFour, eventTwo, eventOne, eventThree}
                    });

                env.UndeployAll();

                // test min/max
                var fieldsTwo = new[] {"c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7"};
                epl = "@Name('s0') select " +
                      "maxbyever(IntPrimitive, TheString).LongPrimitive as c0," +
                      "minbyever(IntPrimitive, TheString).LongPrimitive as c1," +
                      "maxbyever(TheString, IntPrimitive).LongPrimitive as c2," +
                      "minbyever(TheString, IntPrimitive).LongPrimitive as c3," +
                      "maxby(IntPrimitive, TheString).LongPrimitive as c4," +
                      "minby(IntPrimitive, TheString).LongPrimitive as c5," +
                      "maxby(TheString, IntPrimitive).LongPrimitive as c6," +
                      "minby(TheString, IntPrimitive).LongPrimitive as c7 " +
                      "from SupportBean#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(MakeEvent("C", 10, 1L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L});

                env.SendEventBean(MakeEvent("P", 5, 2L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L});

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("C", 9, 3L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {1L, 2L, 2L, 3L, 1L, 2L, 2L, 3L});

                env.SendEventBean(MakeEvent("C", 11, 4L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {4L, 2L, 2L, 3L, 4L, 2L, 2L, 3L});

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("X", 11, 5L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {5L, 2L, 5L, 3L, 5L, 2L, 5L, 3L});

                env.SendEventBean(MakeEvent("X", 0, 6L));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fieldsTwo,
                    new object[] {5L, 6L, 5L, 3L, 5L, 6L, 5L, 3L});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateNoDataWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"c0", "c1", "c2", "c3"};
                var epl = "@Name('s0') select " +
                          "maxbyever(IntPrimitive).TheString as c0, " +
                          "minbyever(IntPrimitive).TheString as c1, " +
                          "maxby(IntPrimitive).TheString as c2, " +
                          "minby(IntPrimitive).TheString as c3 " +
                          "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", "E1", "E1", "E1"});

                env.SendEventBean(new SupportBean("E2", 2));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", "E1", "E2", "E1"});

                env.SendEventBean(new SupportBean("E3", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", "E3", "E2", "E3"});

                env.SendEventBean(new SupportBean("E4", 3));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4", "E3", "E4", "E3"});

                env.UndeployAll();
            }
        }

        internal class ResultSetAggregateInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                TryInvalidCompile(
                    env,
                    "select maxBy(P00||P10) from SupportBean_S0#lastevent, SupportBean_S1#lastevent",
                    "Failed to validate select-clause expression 'maxby(P00||P10)': The 'maxby' aggregation function requires that any parameter expressions evaluate properties of the same stream");

                TryInvalidCompile(
                    env,
                    "select sorted(P00) from SupportBean_S0",
                    "Failed to validate select-clause expression 'sorted(P00)': The 'sorted' aggregation function requires that a data window is declared for the stream");
            }
        }
    }
} // end of namespace