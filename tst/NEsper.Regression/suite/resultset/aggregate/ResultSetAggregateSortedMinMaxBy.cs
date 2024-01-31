///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.resultset.aggregate
{
    public class ResultSetAggregateSortedMinMaxBy
    {
        public static ICollection<RegressionExecution> Executions()
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
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithNoDataWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateNoDataWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithMultipleCriteria(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMultipleCriteria());
            return execs;
        }

        public static IList<RegressionExecution> WithMultipleCriteriaSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMultipleCriteriaSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithNoAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateNoAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithMinByMaxByOverWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMinByMaxByOverWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithMultipleOverlappingCategories(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateMultipleOverlappingCategories());
            return execs;
        }

        public static IList<RegressionExecution> WithGroupedSortedMinMax(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ResultSetAggregateGroupedSortedMinMax());
            return execs;
        }

        public class ResultSetAggregateMultipleCriteriaSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select sorted(TheString desc, IntPrimitive desc) as c0 from SupportBean#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => ClassicAssert.AreEqual(typeof(SupportBean[]), statement.EventType.GetPropertyType("c0")));

                env.SendEventBean(new SupportBean("C", 10));
                AssertExpected(env, new object[][] { new object[] { "C", 10 } });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("D", 20));
                AssertExpected(env, new object[][] { new object[] { "D", 20 }, new object[] { "C", 10 } });

                env.Milestone(1);

                env.SendEventBean(new SupportBean("C", 15));
                AssertExpected(
                    env,
                    new object[][] { new object[] { "D", 20 }, new object[] { "C", 15 }, new object[] { "C", 10 } });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("D", 19));
                AssertExpected(
                    env,
                    new object[][] {
                        new object[] { "D", 20 }, new object[] { "D", 19 }, new object[] { "C", 15 },
                        new object[] { "C", 10 }
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateGroupedSortedMinMax : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var epl = "@name('s0') select " +
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
                var eplJoin = "@name('s0') select " +
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
                var fields = "c0".SplitCsv();
                var joinMultirow =
                    "@name('s0') select sorted(IntPrimitive desc) as c0 from SupportBean_S0#keepall, SupportBean#length(2)";
                env.CompileDeploy(joinMultirow).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "S1"));
                env.SendEventBean(new SupportBean_S0(2, "S2"));
                env.SendEventBean(new SupportBean_S0(3, "S3"));

                env.MilestoneInc(milestone);

                var eventOne = new SupportBean("E1", 1);
                env.SendEventBean(eventOne);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { new object[] { eventOne } });

                env.MilestoneInc(milestone);

                var eventTwo = new SupportBean("E2", 2);
                env.SendEventBean(eventTwo);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { new object[] { eventTwo, eventOne } });

                env.MilestoneInc(milestone);

                var eventThree = new SupportBean("E3", 0);
                env.SendEventBean(eventThree);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { new object[] { eventTwo, eventThree } });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateMinByMaxByOverWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7,c8,c9".SplitCsv();
                var epl = "@name('s0') select " +
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
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { eventOne, eventOne, 10L, "E1", 1, eventOne, 10L, "E1", 1, eventOne });

                var eventTwo = MakeEvent("E2", 2, 20);
                env.SendEventBean(eventTwo);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { eventTwo, eventOne, 20L, "E2", 2, eventTwo, 10L, "E1", 1, eventOne });

                env.Milestone(0);

                var eventThree = MakeEvent("E3", 3, 5);
                env.SendEventBean(eventThree);
                var resultThree = new object[]
                    { eventTwo, eventThree, 20L, "E2", 2, eventTwo, 5L, "E3", 3, eventThree };
                env.AssertPropsNew("s0", fields, resultThree);

                var eventFour = MakeEvent("E4", 4, 5);
                env.SendEventBean(eventFour); // same as E3
                env.AssertPropsNew("s0", fields, resultThree);

                env.Milestone(1);

                var eventFive = MakeEvent("E5", 5, 20);
                env.SendEventBean(eventFive); // same as E2
                env.AssertPropsNew("s0", fields, resultThree);

                var eventSix = MakeEvent("E6", 6, 10);
                env.SendEventBean(eventSix); // expires E1
                env.AssertPropsNew("s0", fields, resultThree);

                var eventSeven = MakeEvent("E7", 7, 20);
                env.SendEventBean(eventSeven); // expires E2
                var resultSeven = new object[]
                    { eventTwo, eventThree, 20L, "E5", 5, eventFive, 5L, "E3", 3, eventThree };
                env.AssertPropsNew("s0", fields, resultSeven);

                env.Milestone(2);

                env.SendEventBean(MakeEvent("E8", 8, 20)); // expires E3
                var resultEight = new object[]
                    { eventTwo, eventThree, 20L, "E5", 5, eventFive, 5L, "E4", 4, eventFour };
                env.AssertPropsNew("s0", fields, resultEight);

                env.SendEventBean(MakeEvent("E9", 9, 19)); // expires E4
                var resultNine = new object[] { eventTwo, eventThree, 20L, "E5", 5, eventFive, 10L, "E6", 6, eventSix };
                env.AssertPropsNew("s0", fields, resultNine);

                env.SendEventBean(MakeEvent("E10", 10, 12)); // expires E5
                var resultTen = new object[] { eventTwo, eventThree, 20L, "E7", 7, eventSeven, 10L, "E6", 6, eventSix };
                env.AssertPropsNew("s0", fields, resultTen);

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select " +
                          "maxby(IntPrimitive).TheString, " +
                          "minby(IntPrimitive)," +
                          "maxbyever(IntPrimitive).TheString, " +
                          "minbyever(IntPrimitive)," +
                          "sorted(IntPrimitive asc, TheString desc)" +
                          " from SupportBean#time(10)";
                env.CompileDeploy(epl).AddListener("s0");

                env.AssertStatement(
                    "s0",
                    statement => {
                        var props = statement.EventType.PropertyDescriptors;
                        ClassicAssert.AreEqual("maxby(IntPrimitive).TheString", props[0].PropertyName);
                        ClassicAssert.AreEqual("minby(IntPrimitive)", props[1].PropertyName);
                        ClassicAssert.AreEqual("maxbyever(IntPrimitive).TheString", props[2].PropertyName);
                        ClassicAssert.AreEqual("minbyever(IntPrimitive)", props[3].PropertyName);
                        ClassicAssert.AreEqual("sorted(IntPrimitive,TheString desc)", props[4].PropertyName);
                    });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateMultipleOverlappingCategories : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
                var epl = "@name('s0') select " +
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
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L });

                env.SendEventBean(MakeEvent("P", 5, 2L));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L });

                env.Milestone(0);

                env.SendEventBean(MakeEvent("G", 7, 3L));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L });

                env.SendEventBean(MakeEvent("A", 7, 4L));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 1L, 2L, 2L, 4L, 1L, 2L, 2L, 4L });

                env.Milestone(1);

                env.SendEventBean(MakeEvent("G", 1, 5L));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 1L, 2L, 5L, 4L, 1L, 2L, 5L, 4L });

                env.SendEventBean(MakeEvent("X", 7, 6L));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 1L, 6L, 5L, 4L, 1L, 6L, 5L, 4L });

                env.Milestone(2);

                env.SendEventBean(MakeEvent("G", 100, 7L));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 7L, 6L, 5L, 4L, 7L, 6L, 5L, 4L });

                env.SendEventBean(MakeEvent("Z", 1000, 8L));
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[] { 8L, 8L, 5L, 4L, 8L, 8L, 5L, 4L });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateMultipleCriteria : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();
                string epl;

                // test sorted multiple criteria
                var fields = "c0,c1,c2,c3".SplitCsv();
                epl = "@name('s0') select " +
                      "sorted(TheString desc, IntPrimitive desc) as c0," +
                      "sorted(TheString, IntPrimitive) as c1," +
                      "sorted(TheString asc, IntPrimitive asc) as c2," +
                      "sorted(TheString desc, IntPrimitive asc) as c3 " +
                      "from SupportBean#keepall";
                env.CompileDeploy(epl).AddListener("s0");

                var eventOne = new SupportBean("C", 10);
                env.SendEventBean(eventOne);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { eventOne },
                        new object[] { eventOne },
                        new object[] { eventOne },
                        new object[] { eventOne }
                    });

                env.MilestoneInc(milestone);

                var eventTwo = new SupportBean("D", 20);
                env.SendEventBean(eventTwo);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { eventTwo, eventOne },
                        new object[] { eventOne, eventTwo },
                        new object[] { eventOne, eventTwo },
                        new object[] { eventTwo, eventOne }
                    });

                var eventThree = new SupportBean("C", 15);
                env.SendEventBean(eventThree);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { eventTwo, eventThree, eventOne },
                        new object[] { eventOne, eventThree, eventTwo },
                        new object[] { eventOne, eventThree, eventTwo },
                        new object[] { eventTwo, eventOne, eventThree }
                    });

                env.MilestoneInc(milestone);

                var eventFour = new SupportBean("D", 19);
                env.SendEventBean(eventFour);
                env.AssertPropsNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { eventTwo, eventFour, eventThree, eventOne },
                        new object[] { eventOne, eventThree, eventFour, eventTwo },
                        new object[] { eventOne, eventThree, eventFour, eventTwo },
                        new object[] { eventFour, eventTwo, eventOne, eventThree }
                    });

                env.UndeployAll();

                // test min/max
                var fieldsTwo = "c0,c1,c2,c3,c4,c5,c6,c7".SplitCsv();
                epl = "@name('s0') select " +
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
                env.AssertPropsNew(
                    "s0",
                    fieldsTwo,
                    new object[] { 1L, 1L, 1L, 1L, 1L, 1L, 1L, 1L });

                env.SendEventBean(MakeEvent("P", 5, 2L));
                env.AssertPropsNew(
                    "s0",
                    fieldsTwo,
                    new object[] { 1L, 2L, 2L, 1L, 1L, 2L, 2L, 1L });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("C", 9, 3L));
                env.AssertPropsNew(
                    "s0",
                    fieldsTwo,
                    new object[] { 1L, 2L, 2L, 3L, 1L, 2L, 2L, 3L });

                env.SendEventBean(MakeEvent("C", 11, 4L));
                env.AssertPropsNew(
                    "s0",
                    fieldsTwo,
                    new object[] { 4L, 2L, 2L, 3L, 4L, 2L, 2L, 3L });

                env.MilestoneInc(milestone);

                env.SendEventBean(MakeEvent("X", 11, 5L));
                env.AssertPropsNew(
                    "s0",
                    fieldsTwo,
                    new object[] { 5L, 2L, 5L, 3L, 5L, 2L, 5L, 3L });

                env.SendEventBean(MakeEvent("X", 0, 6L));
                env.AssertPropsNew(
                    "s0",
                    fieldsTwo,
                    new object[] { 5L, 6L, 5L, 3L, 5L, 6L, 5L, 3L });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateNoDataWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0,c1,c2,c3".SplitCsv();
                var epl = "@name('s0') select " +
                          "maxbyever(IntPrimitive).TheString as c0, " +
                          "minbyever(IntPrimitive).TheString as c1, " +
                          "maxby(IntPrimitive).TheString as c2, " +
                          "minby(IntPrimitive).TheString as c3 " +
                          "from SupportBean";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { "E1", "E1", "E1", "E1" });

                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertPropsNew("s0", fields, new object[] { "E2", "E1", "E2", "E1" });

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E3", 0));
                env.AssertPropsNew("s0", fields, new object[] { "E2", "E3", "E2", "E3" });

                env.SendEventBean(new SupportBean("E4", 3));
                env.AssertPropsNew("s0", fields, new object[] { "E4", "E3", "E4", "E3" });

                env.UndeployAll();
            }
        }

        private class ResultSetAggregateInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.TryInvalidCompile(
                    "select maxBy(P00||P10) from SupportBean_S0#lastevent, SupportBean_S1#lastevent",
                    "Failed to validate select-clause expression 'maxby(P00||P10)': The 'maxby' aggregation function requires that any parameter expressions evaluate properties of the same stream");

                env.TryInvalidCompile(
                    "select sorted(P00) from SupportBean_S0",
                    "Failed to validate select-clause expression 'sorted(P00)': The 'sorted' aggregation function requires that a data window is declared for the stream");
            }
        }

        private static void TryAssertionGroupedSortedMinMax(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            var fields = "c0,c1,c2,c3,c4,c5,c6".SplitCsv();
            var eventOne = MakeEvent("E1", 1, 1);
            env.SendEventBean(eventOne);
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] {
                    new object[] { eventOne },
                    new object[] { eventOne },
                    new object[] { eventOne },
                    eventOne, eventOne, eventOne, eventOne
                });

            env.MilestoneInc(milestone);

            var eventTwo = MakeEvent("E2", 2, 1);
            env.SendEventBean(eventTwo);
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] {
                    new object[] { eventOne, eventTwo },
                    new object[] { eventTwo, eventOne },
                    new object[] { eventOne, eventTwo },
                    eventTwo, eventOne, eventTwo, eventOne
                });

            env.MilestoneInc(milestone);

            var eventThree = MakeEvent("E3", 0, 1);
            env.SendEventBean(eventThree);
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] {
                    new object[] { eventOne, eventTwo, eventThree },
                    new object[] { eventTwo, eventOne, eventThree },
                    new object[] { eventThree, eventOne, eventTwo },
                    eventTwo, eventThree, eventTwo, eventThree
                });

            env.MilestoneInc(milestone);

            var eventFour = MakeEvent("E4", 3, 1); // pushes out E1
            env.SendEventBean(eventFour);
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] {
                    new object[] { eventTwo, eventThree, eventFour },
                    new object[] { eventFour, eventTwo, eventThree },
                    new object[] { eventThree, eventTwo, eventFour },
                    eventFour, eventThree, eventFour, eventThree
                });

            var eventFive = MakeEvent("E5", -1, 2); // group 2
            env.SendEventBean(eventFive);
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] {
                    new object[] { eventFive },
                    new object[] { eventFive },
                    new object[] { eventFive },
                    eventFive, eventFive, eventFive, eventFive
                });

            var eventSix = MakeEvent("E6", -1, 1); // pushes out E2
            env.SendEventBean(eventSix);
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] {
                    new object[] { eventThree, eventFour, eventSix },
                    new object[] { eventFour, eventThree, eventSix },
                    new object[] { eventSix, eventThree, eventFour },
                    eventFour, eventSix, eventFour, eventSix
                });

            env.MilestoneInc(milestone);

            var eventSeven = MakeEvent("E7", 2, 2); // group 2
            env.SendEventBean(eventSeven);
            env.AssertPropsNew(
                "s0",
                fields,
                new object[] {
                    new object[] { eventFive, eventSeven },
                    new object[] { eventSeven, eventFive },
                    new object[] { eventFive, eventSeven },
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
            env.AssertListener(
                "s0",
                listener => {
                    var und = (SupportBean[])listener.AssertOneGetNewAndReset().Get("c0");
                    for (var i = 0; i < und.Length; i++) {
                        ClassicAssert.AreEqual(expected[i][0], und[i].TheString);
                        ClassicAssert.AreEqual(expected[i][1], und[i].IntPrimitive);
                    }
                });
        }
    }
} // end of namespace