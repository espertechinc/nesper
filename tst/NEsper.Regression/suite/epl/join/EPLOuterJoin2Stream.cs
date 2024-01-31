///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterJoin2Stream
    {
        private static readonly string[] FIELDS = new string[] { "s0.Id", "s0.P00", "s1.Id", "s1.P10" };

        private static readonly SupportBean_S0[] EVENTS_S0;
        private static readonly SupportBean_S1[] EVENTS_S1;

        static EPLOuterJoin2Stream()
        {
            EVENTS_S0 = new SupportBean_S0[15];
            EVENTS_S1 = new SupportBean_S1[15];
            var count = 100;
            for (var i = 0; i < EVENTS_S0.Length; i++) {
                EVENTS_S0[i] = new SupportBean_S0(count++, i.ToString());
            }

            count = 200;
            for (var i = 0; i < EVENTS_S1.Length; i++) {
                EVENTS_S1[i] = new SupportBean_S1(count++, i.ToString());
            }
        }

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithRangeOuterJoin(execs);
            WithFullOuterIteratorGroupBy(execs);
            WithFullOuterJoin(execs);
            WithMultiColumnLeftOM(execs);
            WithMultiColumnLeft(execs);
            WithMultiColumnRight(execs);
            WithMultiColumnRightCoercion(execs);
            WithRightOuterJoin(execs);
            WithLeftOuterJoin(execs);
            WithEventType(execs);
            WithFullOuterMultikeyWArrayPrimitive(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithFullOuterMultikeyWArrayPrimitive(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullOuterMultikeyWArrayPrimitive());
            return execs;
        }

        public static IList<RegressionExecution> WithEventType(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinEventType());
            return execs;
        }

        public static IList<RegressionExecution> WithLeftOuterJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithRightOuterJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRightOuterJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiColumnRightCoercion(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinMultiColumnRightCoercion());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiColumnRight(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinMultiColumnRight());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiColumnLeft(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinMultiColumnLeft());
            return execs;
        }

        public static IList<RegressionExecution> WithMultiColumnLeftOM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinMultiColumnLeftOM());
            return execs;
        }

        public static IList<RegressionExecution> WithFullOuterJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullOuterJoin());
            return execs;
        }

        public static IList<RegressionExecution> WithFullOuterIteratorGroupBy(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinFullOuterIteratorGroupBy());
            return execs;
        }

        public static IList<RegressionExecution> WithRangeOuterJoin(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRangeOuterJoin());
            return execs;
        }

        private class EPLJoinFullOuterMultikeyWArrayPrimitive : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select * " +
                          "from SupportEventWithIntArray#keepall one " +
                          "full outer join " +
                          "SupportEventWithManyArray#keepall two " +
                          "on Array = IntOne";
                env.CompileDeploy(epl).AddListener("s0");

                SendIntArrayAssert(env, "IA1", new int[] { 1, 2 }, new object[][] { new object[] { "IA1", null } });
                SendManyArrayAssert(env, "MA1", new int[] { 3, 4 }, new object[][] { new object[] { null, "MA1" } });
                SendIntArrayAssert(env, "IA2", new int[] { 1 }, new object[][] { new object[] { "IA2", null } });
                SendManyArrayAssert(env, "MA2", new int[] { 2 }, new object[][] { new object[] { null, "MA2" } });

                env.Milestone(0);

                SendManyArrayAssert(env, "MA3", new int[] { 1 }, new object[][] { new object[] { "IA2", "MA3" } });
                SendIntArrayAssert(env, "IA3", new int[] { 3, 4 }, new object[][] { new object[] { "IA3", "MA1" } });
                SendManyArrayAssert(env, "MA4", new int[] { 3, 4 }, new object[][] { new object[] { "IA3", "MA4" } });
                SendIntArrayAssert(
                    env,
                    "IA4",
                    new int[] { 3, 4 },
                    new object[][] { new object[] { "IA4", "MA1" }, new object[] { "IA4", "MA4" } });

                env.UndeployAll();
            }

            private void SendIntArrayAssert(
                RegressionEnvironment env,
                string id,
                int[] array,
                object[][] expected)
            {
                env.SendEventBean(new SupportEventWithIntArray(id, array));
                AssertEvents(env, expected);
            }

            private void SendManyArrayAssert(
                RegressionEnvironment env,
                string id,
                int[] intOne,
                object[][] expected)
            {
                env.SendEventBean(new SupportEventWithManyArray(id).WithIntOne(intOne));
                AssertEvents(env, expected);
            }

            private void AssertEvents(
                RegressionEnvironment env,
                object[][] expected)
            {
                env.AssertPropsPerRowLastNewAnyOrder("s0", "one.Id,two.Id".Split(","), expected);
            }
        }

        private class EPLJoinRangeOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var stmtOne =
                    "@name('s0') select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.Key as sbrk, sbr.RangeStart as sbrs, sbr.RangeEnd as sbre " +
                    "from SupportBean#keepall sb " +
                    "full outer join " +
                    "SupportBeanRange#keepall sbr " +
                    "on TheString = Key " +
                    "where IntPrimitive between RangeStart and RangeEnd " +
                    "order by RangeStart asc, IntPrimitive asc";
                TryAssertion(env, stmtOne, milestone);

                var stmtTwo =
                    "@name('s0') select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.Key as sbrk, sbr.RangeStart as sbrs, sbr.RangeEnd as sbre " +
                    "from SupportBeanRange#keepall sbr " +
                    "full outer join " +
                    "SupportBean#keepall sb " +
                    "on TheString = Key " +
                    "where IntPrimitive between RangeStart and RangeEnd " +
                    "order by RangeStart asc, IntPrimitive asc";
                TryAssertion(env, stmtTwo, milestone);

                var stmtThree =
                    "@name('s0') select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.Key as sbrk, sbr.RangeStart as sbrs, sbr.RangeEnd as sbre " +
                    "from SupportBeanRange#keepall sbr " +
                    "full outer join " +
                    "SupportBean#keepall sb " +
                    "on TheString = Key " +
                    "where IntPrimitive >= RangeStart and IntPrimitive <= RangeEnd " +
                    "order by RangeStart asc, IntPrimitive asc";
                TryAssertion(env, stmtThree, milestone);
            }

            private static void TryAssertion(
                RegressionEnvironment env,
                string epl,
                AtomicLong milestone)
            {
                var fields = "sbstr,sbint,sbrk,sbrs,sbre".Split(",");
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean("K1", 10));
                env.SendEventBean(new SupportBeanRange("R1", "K1", 20, 30));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("K1", 30));
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "K1", 30, "K1", 20, 30 } });

                env.SendEventBean(new SupportBean("K1", 40));
                env.SendEventBean(new SupportBean("K1", 31));
                env.SendEventBean(new SupportBean("K1", 19));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBeanRange("R2", "K1", 39, 41));
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "K1", 40, "K1", 39, 41 } });

                env.SendEventBean(new SupportBeanRange("R2", "K1", 38, 40));
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "K1", 40, "K1", 38, 40 } });

                env.SendEventBean(new SupportBeanRange("R2", "K1", 40, 42));
                env.AssertPropsPerRowLastNew("s0", fields, new object[][] { new object[] { "K1", 40, "K1", 40, 42 } });

                env.SendEventBean(new SupportBeanRange("R2", "K1", 41, 42));
                env.SendEventBean(new SupportBeanRange("R2", "K1", 38, 39));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("K1", 41));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][] {
                        new object[] { "K1", 41, "K1", 39, 41 }, new object[] { "K1", 41, "K1", 40, 42 },
                        new object[] { "K1", 41, "K1", 41, 42 }
                    });

                env.SendEventBean(new SupportBeanRange("R2", "K1", 35, 42));
                env.AssertPropsPerRowLastNew(
                    "s0",
                    fields,
                    new object[][]
                        { new object[] { "K1", 40, "K1", 35, 42 }, new object[] { "K1", 41, "K1", 35, 42 } });

                env.UndeployAll();
            }
        }

        private class EPLJoinFullOuterIteratorGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select TheString, IntPrimitive, Symbol, Volume " +
                          "from SupportMarketDataBean#keepall " +
                          "full outer join SupportBean#groupwin(TheString, IntPrimitive)#length(2) " +
                          "on TheString = Symbol " +
                          "group by TheString, IntPrimitive, Symbol " +
                          "order by TheString, IntPrimitive, Symbol, Volume";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEventMD(env, "c0", 200L);
                SendEventMD(env, "c3", 400L);

                SendEvent(env, "c0", 0);
                SendEvent(env, "c0", 1);
                SendEvent(env, "c0", 2);
                SendEvent(env, "c1", 0);
                SendEvent(env, "c1", 1);
                SendEvent(env, "c1", 2);
                SendEvent(env, "c2", 0);
                SendEvent(env, "c2", 1);
                SendEvent(env, "c2", 2);

                env.AssertIterator(
                    "s0",
                    iterator => {
                        var events = EPAssertionUtil.EnumeratorToArray(iterator);
                        ClassicAssert.AreEqual(10, events.Length);
                        EPAssertionUtil.AssertPropsPerRow(
                            events,
                            "TheString,IntPrimitive,Symbol,Volume".Split(","),
                            new object[][] {
                                new object[] { null, null, "c3", 400L }, new object[] { "c0", 0, "c0", 200L },
                                new object[] { "c0", 1, "c0", 200L }, new object[] { "c0", 2, "c0", 200L },
                                new object[] { "c1", 0, null, null }, new object[] { "c1", 1, null, null },
                                new object[] { "c1", 2, null, null }, new object[] { "c2", 0, null, null },
                                new object[] { "c2", 1, null, null }, new object[] { "c2", 2, null, null }
                            });
                    });

                /* For debugging, comment in
                for (int i = 0; i < events.length; i++)
                new object[] {
                    Console.WriteLine(
                           "string=" + events[i].get("string") +
                           "  int=" + events[i].get("intPrimitive") +
                           "  symbol=" + events[i].get("symbol") +
                           "  volume="  + events[i].get("volume")
                        );
                }
                */
                env.UndeployAll();
            }
        }

        private class EPLJoinFullOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupStatement(env, "full");

                // Send S0[0]
                SendEvent(EVENTS_S0[0], env);
                CompareEventNew(env, 100, "0", null, null);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] { new object[] { 100, "0", null, null } });

                // Send S1[1]
                SendEvent(EVENTS_S1[1], env);
                CompareEventNew(env, null, null, 201, "1");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 100, "0", null, null }, new object[] { null, null, 201, "1" }
                    });

                // Send S1[2] and S0[2]
                SendEvent(EVENTS_S1[2], env);
                CompareEventNew(env, null, null, 202, "2");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 100, "0", null, null }, new object[] { null, null, 201, "1" },
                        new object[] { null, null, 202, "2" }
                    });

                SendEvent(EVENTS_S0[2], env);
                CompareEventNew(env, 102, "2", 202, "2");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 100, "0", null, null }, new object[] { null, null, 201, "1" },
                        new object[] { 102, "2", 202, "2" }
                    });

                // Send S0[3] and S1[3]
                SendEvent(EVENTS_S0[3], env);
                CompareEventNew(env, 103, "3", null, null);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 100, "0", null, null }, new object[] { null, null, 201, "1" },
                        new object[] { 102, "2", 202, "2" }, new object[] { 103, "3", null, null }
                    });
                SendEvent(EVENTS_S1[3], env);
                CompareEventNew(env, 103, "3", 203, "3");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 100, "0", null, null }, new object[] { null, null, 201, "1" },
                        new object[] { 102, "2", 202, "2" }, new object[] { 103, "3", 203, "3" }
                    });

                // Send S0[4], pushes S0[0] out of window
                SendEvent(EVENTS_S0[4], env);
                env.AssertListener(
                    "s0",
                    listener => {
                        var oldEvent = listener.LastOldData[0];
                        var newEvent = listener.LastNewData[0];
                        CompareEvent(oldEvent, 100, "0", null, null);
                        CompareEvent(newEvent, 104, "4", null, null);
                        listener.Reset();
                    });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { null, null, 201, "1" }, new object[] { 102, "2", 202, "2" },
                        new object[] { 103, "3", 203, "3" }, new object[] { 104, "4", null, null }
                    });

                // Send S1[4]
                SendEvent(EVENTS_S1[4], env);
                CompareEventNew(env, 104, "4", 204, "4");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { null, null, 201, "1" }, new object[] { 102, "2", 202, "2" },
                        new object[] { 103, "3", 203, "3" }, new object[] { 104, "4", 204, "4" }
                    });

                // Send S1[5]
                SendEvent(EVENTS_S1[5], env);
                CompareEventNew(env, null, null, 205, "5");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { null, null, 201, "1" }, new object[] { 102, "2", 202, "2" },
                        new object[] { 103, "3", 203, "3" }, new object[] { 104, "4", 204, "4" },
                        new object[] { null, null, 205, "5" }
                    });

                // Send S1[6], pushes S1[1] out of window
                SendEvent(EVENTS_S1[5], env);
                env.AssertListener(
                    "s0",
                    listener => {
                        var oldEvent = listener.LastOldData[0];
                        var newEvent = listener.LastNewData[0];
                        CompareEvent(oldEvent, null, null, 201, "1");
                        CompareEvent(newEvent, null, null, 205, "5");
                    });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 102, "2", 202, "2" }, new object[] { 103, "3", 203, "3" },
                        new object[] { 104, "4", 204, "4" }, new object[] { null, null, 205, "5" },
                        new object[] { null, null, 205, "5" }
                    });

                env.UndeployAll();
            }
        }

        private class EPLJoinMultiColumnLeftOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("s0.Id, s0.P00, s0.P01, s1.Id, s1.P10, s1.P11".Split(","));
                var fromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportBean_S0).Name, "s0").AddView("keepall"),
                    FilterStream.Create(typeof(SupportBean_S1).Name, "s1").AddView("keepall"));
                fromClause.Add(
                    OuterJoinQualifier.Create("s0.P00", OuterJoinType.LEFT, "s1.P10").Add("s1.P11", "s0.P01"));
                model.FromClause = fromClause;
                model = env.CopyMayFail(model);

                var stmtText =
                    "select s0.Id, s0.P00, s0.P01, s1.Id, s1.P10, s1.P11 from SupportBean_S0#keepall as s0 left outer join SupportBean_S1#keepall as s1 on s0.P00 = s1.P10 and s1.P11 = s0.P01";
                ClassicAssert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                AssertMultiColumnLeft(env);

                var modelReverse = env.EplToModel(stmtText);
                ClassicAssert.AreEqual(stmtText, modelReverse.ToEPL());

                env.UndeployAll();
            }
        }

        private class EPLJoinMultiColumnLeft : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@name('s0') select s0.Id, s0.P00, s0.P01, s1.Id, s1.P10, s1.P11 from " +
                          "SupportBean_S0#length(3) as s0 " +
                          "left outer join " +
                          "SupportBean_S1#length(5) as s1" +
                          " on s0.P00 = s1.P10 and s0.P01 = s1.P11";
                env.CompileDeploy(epl).AddListener("s0");

                AssertMultiColumnLeft(env);

                env.UndeployAll();
            }
        }

        private class EPLJoinMultiColumnRight : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "s0.Id, s0.P00, s0.P01, s1.Id, s1.P10, s1.P11".Split(",");
                var epl = "@name('s0') select s0.Id, s0.P00, s0.P01, s1.Id, s1.P10, s1.P11 from " +
                          "SupportBean_S0#length(3) as s0 " +
                          "right outer join " +
                          "SupportBean_S1#length(5) as s1" +
                          " on s0.P00 = s1.P10 and s1.P11 = s0.P01";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "A_1", "B_1"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(2, "A_1", "B_1"));
                env.AssertPropsNew("s0", fields, new object[] { 1, "A_1", "B_1", 2, "A_1", "B_1" });

                env.SendEventBean(new SupportBean_S1(3, "A_2", "B_1"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, 3, "A_2", "B_1" });

                env.SendEventBean(new SupportBean_S1(4, "A_1", "B_2"));
                env.AssertPropsNew("s0", fields, new object[] { null, null, null, 4, "A_1", "B_2" });

                env.UndeployAll();
            }
        }

        private class EPLJoinMultiColumnRightCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "s0.TheString, s1.TheString".Split(",");
                var epl = "@name('s0') select s0.TheString, s1.TheString from " +
                          "SupportBean(TheString like 'S0%')#keepall as s0 " +
                          "right outer join " +
                          "SupportBean(TheString like 'S1%')#keepall as s1" +
                          " on s0.IntPrimitive = s1.DoublePrimitive and s1.IntPrimitive = s0.DoublePrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "S1_1", 10, 20d);
                env.AssertPropsNew("s0", fields, new object[] { null, "S1_1" });

                SendEvent(env, "S0_2", 11, 22d);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "S0_3", 11, 21d);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "S0_4", 12, 21d);
                env.AssertListenerNotInvoked("s0");

                SendEvent(env, "S1_2", 11, 22d);
                env.AssertPropsNew("s0", fields, new object[] { null, "S1_2" });

                SendEvent(env, "S1_3", 22, 11d);
                env.AssertPropsNew("s0", fields, new object[] { "S0_2", "S1_3" });

                SendEvent(env, "S0_5", 22, 11d);
                env.AssertPropsNew("s0", fields, new object[] { "S0_5", "S1_2" });

                env.UndeployAll();
            }
        }

        private class EPLJoinRightOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupStatement(env, "right");

                // Send S0 events, no events expected
                SendEvent(EVENTS_S0[0], env);
                SendEvent(EVENTS_S0[1], env);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIteratorAnyOrder("s0", FIELDS, null);

                // Send S1[2]
                SendEvent(EVENTS_S1[2], env);
                CompareEventNew(env, null, null, 202, "2");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] { new object[] { null, null, 202, "2" } });

                // Send S0[2] events, joined event expected
                SendEvent(EVENTS_S0[2], env);
                CompareEventNew(env, 102, "2", 202, "2");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] { new object[] { 102, "2", 202, "2" } });

                // Send S1[3]
                SendEvent(EVENTS_S1[3], env);
                CompareEventNew(env, null, null, 203, "3");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 102, "2", 202, "2" }, new object[] { null, null, 203, "3" }
                    });

                // Send some more S0 events
                SendEvent(EVENTS_S0[3], env);
                CompareEventNew(env, 103, "3", 203, "3");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 102, "2", 202, "2" }, new object[] { 103, "3", 203, "3" }
                    });

                // Send some more S0 events
                SendEvent(EVENTS_S0[4], env);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 102, "2", 202, "2" }, new object[] { 103, "3", 203, "3" }
                    });

                // Push S0[2] out of the window
                SendEvent(EVENTS_S0[5], env);
                CompareEventOld(env, 102, "2", 202, "2");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { null, null, 202, "2" }, new object[] { 103, "3", 203, "3" }
                    });

                // Some more S1 events
                SendEvent(EVENTS_S1[6], env);
                CompareEventNew(env, null, null, 206, "6");
                SendEvent(EVENTS_S1[7], env);
                CompareEventNew(env, null, null, 207, "7");
                SendEvent(EVENTS_S1[8], env);
                CompareEventNew(env, null, null, 208, "8");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { null, null, 202, "2" }, new object[] { 103, "3", 203, "3" },
                        new object[] { null, null, 206, "6" }, new object[] { null, null, 207, "7" },
                        new object[] { null, null, 208, "8" }
                    });

                // Push S1[2] out of the window
                SendEvent(EVENTS_S1[9], env);
                env.AssertListener(
                    "s0",
                    listener => {
                        var oldEvent = listener.LastOldData[0];
                        var newEvent = listener.LastNewData[0];
                        CompareEvent(oldEvent, null, null, 202, "2");
                        CompareEvent(newEvent, null, null, 209, "9");
                    });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 103, "3", 203, "3" }, new object[] { null, null, 206, "6" },
                        new object[] { null, null, 207, "7" }, new object[] { null, null, 208, "8" },
                        new object[] { null, null, 209, "9" }
                    });

                env.UndeployAll();
            }
        }

        private class EPLJoinLeftOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupStatement(env, "left");

                // Send S1 events, no events expected
                SendEvent(EVENTS_S1[0], env);
                SendEvent(EVENTS_S1[1], env);
                SendEvent(EVENTS_S1[3], env);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIteratorAnyOrder("s0", FIELDS, null);

                // Send S0 event, expect event back from outer join
                SendEvent(EVENTS_S0[2], env);
                CompareEventNew(env, 102, "2", null, null);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] { new object[] { 102, "2", null, null } });

                // Send S1 event matching S0, expect event back
                SendEvent(EVENTS_S1[2], env);
                CompareEventNew(env, 102, "2", 202, "2");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] { new object[] { 102, "2", 202, "2" } });

                // Send some more unmatched events
                SendEvent(EVENTS_S1[4], env);
                SendEvent(EVENTS_S1[5], env);
                SendEvent(EVENTS_S1[6], env);
                env.AssertListenerNotInvoked("s0");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] { new object[] { 102, "2", 202, "2" } });

                // Send event, expect a join result
                SendEvent(EVENTS_S0[5], env);
                CompareEventNew(env, 105, "5", 205, "5");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 102, "2", 202, "2" }, new object[] { 105, "5", 205, "5" }
                    });

                // Let S1[2] go out of the window (lenght 5), expected old join event
                SendEvent(EVENTS_S1[7], env);
                SendEvent(EVENTS_S1[8], env);
                CompareEventOld(env, 102, "2", 202, "2");
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 102, "2", null, null }, new object[] { 105, "5", 205, "5" }
                    });

                // S0[9] should generate an outer join event
                SendEvent(EVENTS_S0[9], env);
                CompareEventNew(env, 109, "9", null, null);
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 102, "2", null, null }, new object[] { 109, "9", null, null },
                        new object[] { 105, "5", 205, "5" }
                    });

                // S0[2] Should leave the window (length 3), should get OLD and NEW event
                SendEvent(EVENTS_S0[10], env);
                env.AssertListener(
                    "s0",
                    listener => {
                        var oldEvent = listener.LastOldData[0];
                        var newEvent = listener.LastNewData[0];
                        CompareEvent(oldEvent, 102, "2", null, null); // S1[2] has left the window already
                        CompareEvent(newEvent, 110, "10", null, null);
                    });
                env.AssertPropsPerRowIteratorAnyOrder(
                    "s0",
                    FIELDS,
                    new object[][] {
                        new object[] { 110, "10", null, null }, new object[] { 109, "9", null, null },
                        new object[] { 105, "5", 205, "5" }
                    });

                env.UndeployAll();
            }
        }

        private class EPLJoinEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupStatement(env, "left");

                env.AssertStatement(
                    "s0",
                    statement => {
                        ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("s0.P00"));
                        ClassicAssert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("s0.Id"));
                        ClassicAssert.AreEqual(typeof(string), statement.EventType.GetPropertyType("s1.P10"));
                        ClassicAssert.AreEqual(typeof(int?), statement.EventType.GetPropertyType("s1.Id"));
                        ClassicAssert.AreEqual(4, statement.EventType.PropertyNames.Length);
                    });

                env.UndeployAll();
            }
        }

        private static void CompareEventNew(
            RegressionEnvironment env,
            int? idS0,
            string p00,
            int? idS1,
            string p10)
        {
            env.AssertEventNew("s0", @event => CompareEvent(@event, idS0, p00, idS1, p10));
        }

        private static void CompareEventOld(
            RegressionEnvironment env,
            int? idS0,
            string p00,
            int? idS1,
            string p10)
        {
            env.AssertEventOld("s0", @event => CompareEvent(@event, idS0, p00, idS1, p10));
        }

        private static void CompareEvent(
            EventBean receivedEvent,
            int? idS0,
            string p00,
            int? idS1,
            string p10)
        {
            ClassicAssert.AreEqual(idS0, receivedEvent.Get("s0.Id"));
            ClassicAssert.AreEqual(idS1, receivedEvent.Get("s1.Id"));
            ClassicAssert.AreEqual(p00, receivedEvent.Get("s0.P00"));
            ClassicAssert.AreEqual(p10, receivedEvent.Get("s1.P10"));
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string s,
            int intPrimitive,
            double doublePrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            bean.DoublePrimitive = doublePrimitive;
            env.SendEventBean(bean);
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string s,
            int intPrimitive)
        {
            var bean = new SupportBean();
            bean.TheString = s;
            bean.IntPrimitive = intPrimitive;
            env.SendEventBean(bean);
        }

        private static void SendEventMD(
            RegressionEnvironment env,
            string symbol,
            long volume)
        {
            var bean = new SupportMarketDataBean(symbol, 0, volume, "");
            env.SendEventBean(bean);
        }

        private static void AssertMultiColumnLeft(RegressionEnvironment env)
        {
            var fields = "s0.Id, s0.P00, s0.P01, s1.Id, s1.P10, s1.P11".Split(",");
            env.SendEventBean(new SupportBean_S0(1, "A_1", "B_1"));
            env.AssertPropsNew("s0", fields, new object[] { 1, "A_1", "B_1", null, null, null });

            env.SendEventBean(new SupportBean_S1(2, "A_1", "B_1"));
            env.AssertPropsNew("s0", fields, new object[] { 1, "A_1", "B_1", 2, "A_1", "B_1" });

            env.SendEventBean(new SupportBean_S1(3, "A_2", "B_1"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S1(4, "A_1", "B_2"));
            env.AssertListenerNotInvoked("s0");
        }

        private static void SetupStatement(
            RegressionEnvironment env,
            string outerJoinType)
        {
            var joinStatement =
                $"@name('s0') select irstream s0.Id, s0.P00, s1.Id, s1.P10 from SupportBean_S0#length(3) as s0 {outerJoinType} outer join SupportBean_S1#length(5) as s1 on s0.P00 = s1.P10";
            env.CompileDeployAddListenerMileZero(joinStatement, "s0");
        }

        private static void SendEvent(
            object theEvent,
            RegressionEnvironment env)
        {
            env.SendEventBean(theEvent);
        }
    }
} // end of namespace