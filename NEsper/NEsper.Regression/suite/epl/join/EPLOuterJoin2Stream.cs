///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterJoin2Stream
    {
        private static readonly string[] FIELDS = {"s0.id", "s0.p00", "s1.id", "s1.p10"};

        private static readonly SupportBean_S0[] EVENTS_S0;
        private static readonly SupportBean_S1[] EVENTS_S1;

        static EPLOuterJoin2Stream()
        {
            EVENTS_S0 = new SupportBean_S0[15];
            EVENTS_S1 = new SupportBean_S1[15];
            var count = 100;
            for (var i = 0; i < EVENTS_S0.Length; i++) {
                EVENTS_S0[i] = new SupportBean_S0(count++, Convert.ToString(i));
            }

            count = 200;
            for (var i = 0; i < EVENTS_S1.Length; i++) {
                EVENTS_S1[i] = new SupportBean_S1(count++, Convert.ToString(i));
            }
        }

        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLJoinRangeOuterJoin());
            execs.Add(new EPLJoinFullOuterIteratorGroupBy());
            execs.Add(new EPLJoinFullOuterJoin());
            execs.Add(new EPLJoinMultiColumnLeftOM());
            execs.Add(new EPLJoinMultiColumnLeft());
            execs.Add(new EPLJoinMultiColumnRight());
            execs.Add(new EPLJoinMultiColumnRightCoercion());
            execs.Add(new EPLJoinRightOuterJoin());
            execs.Add(new EPLJoinLeftOuterJoin());
            execs.Add(new EPLJoinEventType());
            return execs;
        }

        private static void CompareEvent(
            EventBean receivedEvent,
            int? idS0,
            string p00,
            int? idS1,
            string p10)
        {
            Assert.AreEqual(idS0, receivedEvent.Get("s0.id"));
            Assert.AreEqual(idS1, receivedEvent.Get("s1.id"));
            Assert.AreEqual(p00, receivedEvent.Get("s0.p00"));
            Assert.AreEqual(p10, receivedEvent.Get("s1.p10"));
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
            var fields = "s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11".SplitCsv();
            env.SendEventBean(new SupportBean_S0(1, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {1, "A_1", "B_1", null, null, null});

            env.SendEventBean(new SupportBean_S1(2, "A_1", "B_1"));
            EPAssertionUtil.AssertProps(
                env.Listener("s0").AssertOneGetNewAndReset(),
                fields,
                new object[] {1, "A_1", "B_1", 2, "A_1", "B_1"});

            env.SendEventBean(new SupportBean_S1(3, "A_2", "B_1"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.SendEventBean(new SupportBean_S1(4, "A_1", "B_2"));
            Assert.IsFalse(env.Listener("s0").IsInvoked);
        }

        private static void SetupStatement(
            RegressionEnvironment env,
            string outerJoinType)
        {
            var joinStatement = "@Name('s0') select irstream s0.id, s0.p00, s1.id, s1.p10 from " +
                                "SupportBean_S0#length(3) as s0 " +
                                outerJoinType +
                                " outer join " +
                                "SupportBean_S1#length(5) as s1" +
                                " on s0.p00 = s1.p10";
            env.CompileDeployAddListenerMileZero(joinStatement, "s0");
        }

        private static void SendEvent(
            object theEvent,
            RegressionEnvironment env)
        {
            env.SendEventBean(theEvent);
        }

        internal class EPLJoinRangeOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var milestone = new AtomicLong();

                var stmtOne =
                    "@Name('s0') select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.key as sbrk, sbr.rangeStart as sbrs, sbr.rangeEnd as sbre " +
                    "from SupportBean#keepall sb " +
                    "full outer join " +
                    "SupportBeanRange#keepall sbr " +
                    "on theString = key " +
                    "where IntPrimitive between rangeStart and rangeEnd " +
                    "order by rangeStart asc, IntPrimitive asc";
                TryAssertion(env, stmtOne, milestone);

                var stmtTwo =
                    "@Name('s0') select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.key as sbrk, sbr.rangeStart as sbrs, sbr.rangeEnd as sbre " +
                    "from SupportBeanRange#keepall sbr " +
                    "full outer join " +
                    "SupportBean#keepall sb " +
                    "on theString = key " +
                    "where IntPrimitive between rangeStart and rangeEnd " +
                    "order by rangeStart asc, IntPrimitive asc";
                TryAssertion(env, stmtTwo, milestone);

                var stmtThree =
                    "@Name('s0') select sb.TheString as sbstr, sb.IntPrimitive as sbint, sbr.key as sbrk, sbr.rangeStart as sbrs, sbr.rangeEnd as sbre " +
                    "from SupportBeanRange#keepall sbr " +
                    "full outer join " +
                    "SupportBean#keepall sb " +
                    "on theString = key " +
                    "where intPrimitive >= rangeStart and IntPrimitive <= rangeEnd " +
                    "order by rangeStart asc, IntPrimitive asc";
                TryAssertion(env, stmtThree, milestone);
            }

            private static void TryAssertion(
                RegressionEnvironment env,
                string epl,
                AtomicLong milestone)
            {
                var fields = "sbstr,sbint,sbrk,sbrs,sbre".SplitCsv();
                env.CompileDeployAddListenerMile(epl, "s0", milestone.GetAndIncrement());

                env.SendEventBean(new SupportBean("K1", 10));
                env.SendEventBean(new SupportBeanRange("R1", "K1", 20, 30));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("K1", 30));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"K1", 30, "K1", 20, 30}
                    });

                env.SendEventBean(new SupportBean("K1", 40));
                env.SendEventBean(new SupportBean("K1", 31));
                env.SendEventBean(new SupportBean("K1", 19));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBeanRange("R2", "K1", 39, 41));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"K1", 40, "K1", 39, 41}
                    });

                env.SendEventBean(new SupportBeanRange("R2", "K1", 38, 40));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"K1", 40, "K1", 38, 40}
                    });

                env.SendEventBean(new SupportBeanRange("R2", "K1", 40, 42));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"K1", 40, "K1", 40, 42}
                    });

                env.SendEventBean(new SupportBeanRange("R2", "K1", 41, 42));
                env.SendEventBean(new SupportBeanRange("R2", "K1", 38, 39));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean("K1", 41));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"K1", 41, "K1", 39, 41},
                        new object[] {"K1", 41, "K1", 40, 42},
                        new object[] {"K1", 41, "K1", 41, 42}
                    });

                env.SendEventBean(new SupportBeanRange("R2", "K1", 35, 42));
                EPAssertionUtil.AssertPropsPerRow(
                    env.Listener("s0").GetAndResetLastNewData(),
                    fields,
                    new[] {
                        new object[] {"K1", 40, "K1", 35, 42},
                        new object[] {"K1", 41, "K1", 35, 42}
                    });

                env.UndeployAll();
            }
        }

        internal class EPLJoinFullOuterIteratorGroupBy : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select TheString, IntPrimitive, symbol, volume " +
                          "from SupportMarketDataBean#keepall " +
                          "full outer join SupportBean#groupwin(TheString, IntPrimitive)#length(2) " +
                          "on theString = symbol " +
                          "group by TheString, IntPrimitive, symbol " +
                          "order by theString, IntPrimitive, symbol, volume";
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

                var iterator = env.Statement("s0").GetEnumerator();
                var events = EPAssertionUtil.EnumeratorToArray(iterator);
                Assert.AreEqual(10, events.Length);

                /* For debugging, comment in
                for (int i = 0; i < events.length; i++)
                {
                    System.out.println(
                           "string=" + events[i].get("string") +
                           "  int=" + events[i].get("IntPrimitive") +
                           "  symbol=" + events[i].get("Symbol") +
                           "  volume="  + events[i].get("Volume")
                        );
                }
                */

                EPAssertionUtil.AssertPropsPerRow(
                    events,
                    "theString,intPrimitive,symbol,volume".SplitCsv(),
                    new[] {
                        new object[] {null, null, "c3", 400L},
                        new object[] {"c0", 0, "c0", 200L},
                        new object[] {"c0", 1, "c0", 200L},
                        new object[] {"c0", 2, "c0", 200L},
                        new object[] {"c1", 0, null, null},
                        new object[] {"c1", 1, null, null},
                        new object[] {"c1", 2, null, null},
                        new object[] {"c2", 0, null, null},
                        new object[] {"c2", 1, null, null},
                        new object[] {"c2", 2, null, null}
                    });

                env.UndeployAll();
            }
        }

        internal class EPLJoinFullOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupStatement(env, "full");

                // Send S0[0]
                SendEvent(EVENTS_S0[0], env);
                CompareEvent(env.Listener("s0").AssertOneGetNewAndReset(), 100, "0", null, null);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {new object[] {100, "0", null, null}});

                // Send S1[1]
                SendEvent(EVENTS_S1[1], env);
                CompareEvent(env.Listener("s0").AssertOneGetNewAndReset(), null, null, 201, "1");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {100, "0", null, null},
                        new object[] {null, null, 201, "1"}
                    });

                // Send S1[2] and S0[2]
                SendEvent(EVENTS_S1[2], env);
                CompareEvent(env.Listener("s0").AssertOneGetNewAndReset(), null, null, 202, "2");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {100, "0", null, null},
                        new object[] {null, null, 201, "1"},
                        new object[] {null, null, 202, "2"}
                    });

                SendEvent(EVENTS_S0[2], env);
                CompareEvent(env.Listener("s0").AssertOneGetNewAndReset(), 102, "2", 202, "2");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {100, "0", null, null},
                        new object[] {null, null, 201, "1"},
                        new object[] {102, "2", 202, "2"}
                    });

                // Send S0[3] and S1[3]
                SendEvent(EVENTS_S0[3], env);
                CompareEvent(env.Listener("s0").AssertOneGetNewAndReset(), 103, "3", null, null);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {100, "0", null, null},
                        new object[] {null, null, 201, "1"},
                        new object[] {102, "2", 202, "2"},
                        new object[] {103, "3", null, null}
                    });
                SendEvent(EVENTS_S1[3], env);
                CompareEvent(env.Listener("s0").AssertOneGetNewAndReset(), 103, "3", 203, "3");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {100, "0", null, null},
                        new object[] {null, null, 201, "1"},
                        new object[] {102, "2", 202, "2"},
                        new object[] {103, "3", 203, "3"}
                    });

                // Send S0[4], pushes S0[0] out of window
                SendEvent(EVENTS_S0[4], env);
                var oldEvent = env.Listener("s0").LastOldData[0];
                var newEvent = env.Listener("s0").LastNewData[0];
                CompareEvent(oldEvent, 100, "0", null, null);
                CompareEvent(newEvent, 104, "4", null, null);
                env.Listener("s0").Reset();
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {null, null, 201, "1"},
                        new object[] {102, "2", 202, "2"},
                        new object[] {103, "3", 203, "3"},
                        new object[] {104, "4", null, null}
                    });

                // Send S1[4]
                SendEvent(EVENTS_S1[4], env);
                CompareEvent(env.Listener("s0").AssertOneGetNewAndReset(), 104, "4", 204, "4");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {null, null, 201, "1"},
                        new object[] {102, "2", 202, "2"},
                        new object[] {103, "3", 203, "3"},
                        new object[] {104, "4", 204, "4"}
                    });

                // Send S1[5]
                SendEvent(EVENTS_S1[5], env);
                CompareEvent(env.Listener("s0").AssertOneGetNewAndReset(), null, null, 205, "5");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {null, null, 201, "1"},
                        new object[] {102, "2", 202, "2"},
                        new object[] {103, "3", 203, "3"},
                        new object[] {104, "4", 204, "4"},
                        new object[] {null, null, 205, "5"}
                    });

                // Send S1[6], pushes S1[1] out of window
                SendEvent(EVENTS_S1[5], env);
                oldEvent = env.Listener("s0").LastOldData[0];
                newEvent = env.Listener("s0").LastNewData[0];
                CompareEvent(oldEvent, null, null, 201, "1");
                CompareEvent(newEvent, null, null, 205, "5");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {102, "2", 202, "2"},
                        new object[] {103, "3", 203, "3"},
                        new object[] {104, "4", 204, "4"},
                        new object[] {null, null, 205, "5"},
                        new object[] {null, null, 205, "5"}
                    });

                env.UndeployAll();
            }
        }

        internal class EPLJoinMultiColumnLeftOM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.Create("s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11".SplitCsv());
                var fromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportBean_S0).Name, "s0").AddView("keepall"),
                    FilterStream.Create(typeof(SupportBean_S1).Name, "s1").AddView("keepall"));
                fromClause.Add(
                    OuterJoinQualifier.Create("s0.p00", OuterJoinType.LEFT, "s1.p10").Add("s1.p11", "s0.p01"));
                model.FromClause = fromClause;
                model = env.CopyMayFail(model);

                var stmtText =
                    "select s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11 from SupportBean_S0#keepall as s0 left outer join SupportBean_S1#keepall as s1 on s0.p00 = s1.p10 and s1.p11 = s0.p01";
                Assert.AreEqual(stmtText, model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                AssertMultiColumnLeft(env);

                var modelReverse = env.EplToModel(stmtText);
                Assert.AreEqual(stmtText, modelReverse.ToEPL());

                env.UndeployAll();
            }
        }

        internal class EPLJoinMultiColumnLeft : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11 from " +
                          "SupportBean_S0#length(3) as s0 " +
                          "left outer join " +
                          "SupportBean_S1#length(5) as s1" +
                          " on s0.p00 = s1.p10 and s0.p01 = s1.p11";
                env.CompileDeploy(epl).AddListener("s0");

                AssertMultiColumnLeft(env);

                env.UndeployAll();
            }
        }

        internal class EPLJoinMultiColumnRight : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11".SplitCsv();
                var epl = "@Name('s0') select s0.id, s0.p00, s0.p01, s1.id, s1.p10, s1.p11 from " +
                          "SupportBean_S0#length(3) as s0 " +
                          "right outer join " +
                          "SupportBean_S1#length(5) as s1" +
                          " on s0.p00 = s1.p10 and s1.p11 = s0.p01";
                env.CompileDeploy(epl).AddListener("s0");

                env.SendEventBean(new SupportBean_S0(1, "A_1", "B_1"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S1(2, "A_1", "B_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, "A_1", "B_1", 2, "A_1", "B_1"});

                env.SendEventBean(new SupportBean_S1(3, "A_2", "B_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, 3, "A_2", "B_1"});

                env.SendEventBean(new SupportBean_S1(4, "A_1", "B_2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, null, null, 4, "A_1", "B_2"});

                env.UndeployAll();
            }
        }

        internal class EPLJoinMultiColumnRightCoercion : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "s0.TheString, s1.TheString".SplitCsv();
                var epl = "@Name('s0') select s0.TheString, s1.TheString from " +
                          "SupportBean(theString like 'S0%')#keepall as s0 " +
                          "right outer join " +
                          "SupportBean(theString like 'S1%')#keepall as s1" +
                          " on s0.IntPrimitive = s1.doublePrimitive and s1.IntPrimitive = s0.doublePrimitive";
                env.CompileDeploy(epl).AddListener("s0");

                SendEvent(env, "S1_1", 10, 20d);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, "S1_1"});

                SendEvent(env, "S0_2", 11, 22d);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "S0_3", 11, 21d);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "S0_4", 12, 21d);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendEvent(env, "S1_2", 11, 22d);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null, "S1_2"});

                SendEvent(env, "S1_3", 22, 11d);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0_2", "S1_3"});

                SendEvent(env, "S0_5", 22, 11d);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"S0_5", "S1_2"});

                env.UndeployAll();
            }
        }

        internal class EPLJoinRightOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupStatement(env, "right");

                // Send S0 events, no events expected
                SendEvent(EVENTS_S0[0], env);
                SendEvent(EVENTS_S0[1], env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), FIELDS, null);

                // Send S1[2]
                SendEvent(EVENTS_S1[2], env);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                CompareEvent(theEvent, null, null, 202, "2");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {new object[] {null, null, 202, "2"}});

                // Send S0[2] events, joined event expected
                SendEvent(EVENTS_S0[2], env);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                CompareEvent(theEvent, 102, "2", 202, "2");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {new object[] {102, "2", 202, "2"}});

                // Send S1[3]
                SendEvent(EVENTS_S1[3], env);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                CompareEvent(theEvent, null, null, 203, "3");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {102, "2", 202, "2"},
                        new object[] {null, null, 203, "3"}
                    });

                // Send some more S0 events
                SendEvent(EVENTS_S0[3], env);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                CompareEvent(theEvent, 103, "3", 203, "3");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {102, "2", 202, "2"},
                        new object[] {103, "3", 203, "3"}
                    });

                // Send some more S0 events
                SendEvent(EVENTS_S0[4], env);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {102, "2", 202, "2"},
                        new object[] {103, "3", 203, "3"}
                    });

                // Push S0[2] out of the window
                SendEvent(EVENTS_S0[5], env);
                theEvent = env.Listener("s0").AssertOneGetOldAndReset();
                CompareEvent(theEvent, 102, "2", 202, "2");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {null, null, 202, "2"},
                        new object[] {103, "3", 203, "3"}
                    });

                // Some more S1 events
                SendEvent(EVENTS_S1[6], env);
                CompareEvent(env.Listener("s0").AssertOneGetNewAndReset(), null, null, 206, "6");
                SendEvent(EVENTS_S1[7], env);
                CompareEvent(env.Listener("s0").AssertOneGetNewAndReset(), null, null, 207, "7");
                SendEvent(EVENTS_S1[8], env);
                CompareEvent(env.Listener("s0").AssertOneGetNewAndReset(), null, null, 208, "8");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {null, null, 202, "2"},
                        new object[] {103, "3", 203, "3"},
                        new object[] {null, null, 206, "6"},
                        new object[] {null, null, 207, "7"},
                        new object[] {null, null, 208, "8"}
                    });

                // Push S1[2] out of the window
                SendEvent(EVENTS_S1[9], env);
                var oldEvent = env.Listener("s0").LastOldData[0];
                var newEvent = env.Listener("s0").LastNewData[0];
                CompareEvent(oldEvent, null, null, 202, "2");
                CompareEvent(newEvent, null, null, 209, "9");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {103, "3", 203, "3"},
                        new object[] {null, null, 206, "6"},
                        new object[] {null, null, 207, "7"},
                        new object[] {null, null, 208, "8"},
                        new object[] {null, null, 209, "9"}
                    });

                env.UndeployAll();
            }
        }

        internal class EPLJoinLeftOuterJoin : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupStatement(env, "left");

                // Send S1 events, no events expected
                SendEvent(EVENTS_S1[0], env);
                SendEvent(EVENTS_S1[1], env);
                SendEvent(EVENTS_S1[3], env);
                Assert.IsNull(env.Listener("s0").LastNewData); // No events expected
                EPAssertionUtil.AssertPropsPerRowAnyOrder(env.GetEnumerator("s0"), FIELDS, null);

                // Send S0 event, expect event back from outer join
                SendEvent(EVENTS_S0[2], env);
                var theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                CompareEvent(theEvent, 102, "2", null, null);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {new object[] {102, "2", null, null}});

                // Send S1 event matching S0, expect event back
                SendEvent(EVENTS_S1[2], env);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                CompareEvent(theEvent, 102, "2", 202, "2");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {new object[] {102, "2", 202, "2"}});

                // Send some more unmatched events
                SendEvent(EVENTS_S1[4], env);
                SendEvent(EVENTS_S1[5], env);
                SendEvent(EVENTS_S1[6], env);
                Assert.IsNull(env.Listener("s0").LastNewData); // No events expected
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {new object[] {102, "2", 202, "2"}});

                // Send event, expect a join result
                SendEvent(EVENTS_S0[5], env);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                CompareEvent(theEvent, 105, "5", 205, "5");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {102, "2", 202, "2"},
                        new object[] {105, "5", 205, "5"}
                    });

                // Let S1[2] go out of the window (lenght 5), expected old join event
                SendEvent(EVENTS_S1[7], env);
                SendEvent(EVENTS_S1[8], env);
                theEvent = env.Listener("s0").AssertOneGetOldAndReset();
                CompareEvent(theEvent, 102, "2", 202, "2");
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {102, "2", null, null},
                        new object[] {105, "5", 205, "5"}
                    });

                // S0[9] should generate an outer join event
                SendEvent(EVENTS_S0[9], env);
                theEvent = env.Listener("s0").AssertOneGetNewAndReset();
                CompareEvent(theEvent, 109, "9", null, null);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {102, "2", null, null},
                        new object[] {109, "9", null, null},
                        new object[] {105, "5", 205, "5"}
                    });

                // S0[2] Should leave the window (length 3), should get OLD and NEW event
                SendEvent(EVENTS_S0[10], env);
                var oldEvent = env.Listener("s0").LastOldData[0];
                var newEvent = env.Listener("s0").LastNewData[0];
                CompareEvent(oldEvent, 102, "2", null, null); // S1[2] has left the window already
                CompareEvent(newEvent, 110, "10", null, null);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    FIELDS,
                    new[] {
                        new object[] {110, "10", null, null},
                        new object[] {109, "9", null, null},
                        new object[] {105, "5", 205, "5"}
                    });

                env.UndeployAll();
            }
        }

        internal class EPLJoinEventType : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SetupStatement(env, "left");

                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("s0.p00"));
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("s0.id"));
                Assert.AreEqual(typeof(string), env.Statement("s0").EventType.GetPropertyType("s1.p10"));
                Assert.AreEqual(typeof(int?), env.Statement("s0").EventType.GetPropertyType("s1.id"));
                Assert.AreEqual(4, env.Statement("s0").EventType.PropertyNames.Length);

                env.UndeployAll();
            }
        }
    }
} // end of namespace