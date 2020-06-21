///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.client.soda;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterJoinVarA3Stream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithMapLeftJoinUnsortedProps(execs);
            WithLeftJoin2SidesMulticolumn(execs);
            WithLeftOuterJoinRootS0OM(execs);
            WithLeftOuterJoinRootS0Compiled(execs);
            WithLeftOuterJoinRootS0(execs);
            WithRightOuterJoinS2RootS2(execs);
            WithRightOuterJoinS1RootS1(execs);
            WithInvalidMulticolumn(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInvalidMulticolumn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinInvalidMulticolumn());
            return execs;
        }

        public static IList<RegressionExecution> WithRightOuterJoinS1RootS1(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRightOuterJoinS1RootS1());
            return execs;
        }

        public static IList<RegressionExecution> WithRightOuterJoinS2RootS2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRightOuterJoinS2RootS2());
            return execs;
        }

        public static IList<RegressionExecution> WithLeftOuterJoinRootS0(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoinRootS0());
            return execs;
        }

        public static IList<RegressionExecution> WithLeftOuterJoinRootS0Compiled(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoinRootS0Compiled());
            return execs;
        }

        public static IList<RegressionExecution> WithLeftOuterJoinRootS0OM(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoinRootS0OM());
            return execs;
        }

        public static IList<RegressionExecution> WithLeftJoin2SidesMulticolumn(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftJoin2SidesMulticolumn());
            return execs;
        }

        public static IList<RegressionExecution> WithMapLeftJoinUnsortedProps(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinMapLeftJoinUnsortedProps());
            return execs;
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            // Test s0 outer join to 2 streams, 2 results for each (cartesian product)
            //
            var s1Events = SupportBean_S1.MakeS1("A", new[] {"A-s1-1", "A-s1-2"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            var s2Events = SupportBean_S2.MakeS2("A", new[] {"A-s2-1", "A-s2-2"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            var s0Events = SupportBean_S0.MakeS0("A", new[] {"A-s0-1"});
            SendEvent(env, s0Events);
            object[][] expected = {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[1], s2Events[0]},
                new[] {s0Events[0], s1Events[0], s2Events[1]},
                new[] {s0Events[0], s1Events[1], s2Events[1]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, no results for each s1 and s2
            //
            s0Events = SupportBean_S0.MakeS0("B", new[] {"B-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {s0Events[0], null, null}}, GetAndResetNewEvents(env));

            s0Events = SupportBean_S0.MakeS0("B", new[] {"B-s0-2"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {s0Events[0], null, null}}, GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, one row for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("C", new[] {"C-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("C", new[] {"C-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], null}},
                GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, two rows for s1 and no results for s2
            //
            s1Events = SupportBean_S1.MakeS1("D", new[] {"D-s1-1", "D-s1-2"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("D", new[] {"D-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], null},
                    new[] {s0Events[0], s1Events[1], null}
                },
                GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, one row for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("E", new[] {"E-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("E", new[] {"E-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], null, s2Events[0]}},
                GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, two rows for s2 and no results for s1
            //
            s2Events = SupportBean_S2.MakeS2("F", new[] {"F-s2-1", "F-s2-2"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("F", new[] {"F-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0]},
                    new[] {s0Events[0], null, s2Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, one row for s1 and two rows s2
            //
            s1Events = SupportBean_S1.MakeS1("G", new[] {"G-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("G", new[] {"G-s2-1", "G-s2-2"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("G", new[] {"G-s0-2"});
            SendEvent(env, s0Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[0], s2Events[1]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, one row for s2 and two rows s1
            //
            s1Events = SupportBean_S1.MakeS1("H", new[] {"H-s1-1", "H-s1-2"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("H", new[] {"H-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("H", new[] {"H-s0-2"});
            SendEvent(env, s0Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[1], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s0 outer join to s1 and s2, one row for each s1 and s2
            //
            s1Events = SupportBean_S1.MakeS1("I", new[] {"I-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("I", new[] {"I-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("I", new[] {"I-s0-2"});
            SendEvent(env, s0Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("Q", new[] {"Q-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {s0Events[0], null, null}}, GetAndResetNewEvents(env));

            s2Events = SupportBean_S2.MakeS2("Q", new[] {"Q-s2-1", "Q-s2-2"});
            SendEvent(env, s2Events[0]);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], null, s2Events[0]}},
                GetAndResetNewEvents(env));
            SendEvent(env, s2Events[1]);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], null, s2Events[1]}},
                GetAndResetNewEvents(env));

            s1Events = SupportBean_S1.MakeS1("Q", new[] {"Q-s1-1"});
            SendEvent(env, s1Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[0], s2Events[1]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 0 rows, s2 with 2 rows
            //
            s2Events = SupportBean_S2.MakeS2("R", new[] {"R-s2-1", "R-s2-2"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("R", new[] {"R-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("S", new[] {"S-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {s0Events[0], null, null}}, GetAndResetNewEvents(env));

            s1Events = SupportBean_S1.MakeS1("S", new[] {"S-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], null}},
                GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 1 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("T", new[] {"T-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(new[] {new[] {s0Events[0], null, null}}, GetAndResetNewEvents(env));

            s2Events = SupportBean_S2.MakeS2("T", new[] {"T-s2-1"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("T", new[] {"T-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], s2Events[0]}},
                GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("U", new[] {"U-s0-1", "U-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("U", new[] {"U-s1-1"});
            SendEvent(env, s1Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], null},
                new[] {s0Events[1], s1Events[0], null}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("V", new[] {"V-s0-1", "V-s0-1"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("V", new[] {"V-s2-1"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("V", new[] {"V-s1-1"});
            SendEvent(env, s1Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[1], s1Events[0], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s1 inner join to s0 and outer to s2:  s0 with 2 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("W", new[] {"W-s0-1", "W-s0-2"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("W", new[] {"W-s2-1", "W-s2-2"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("W", new[] {"W-s1-1"});
            SendEvent(env, s1Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[1], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[0], s2Events[1]},
                new[] {s0Events[1], s1Events[0], s2Events[1]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("J", new[] {"J-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("J", new[] {"J-s1-1", "J-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("J", new[] {"J-s2-1"});
            SendEvent(env, s2Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[1], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 0 rows, s1 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("K", new[] {"K-s1-1", "K-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("K", new[] {"K-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("L", new[] {"L-s0-1"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("L", new[] {"L-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], null, s2Events[0]}},
                GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 1 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("M", new[] {"M-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("M", new[] {"M-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("M", new[] {"M-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], s2Events[0]}},
                GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("N", new[] {"N-s0-1", "N-s0-1"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("N", new[] {"N-s2-1"});
            SendEvent(env, s2Events);
            expected = new[] {
                new[] {s0Events[0], null, s2Events[0]},
                new[] {s0Events[1], null, s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("O", new[] {"O-s0-1", "O-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("O", new[] {"O-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("O", new[] {"O-s2-1"});
            SendEvent(env, s2Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[1], s1Events[0], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            // Test s2 inner join to s0 and outer to s1:  s0 with 2 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("P", new[] {"P-s0-1", "P-s0-2"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("P", new[] {"P-s1-1", "P-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("P", new[] {"P-s2-1"});
            SendEvent(env, s2Events);
            expected = new[] {
                new[] {s0Events[0], s1Events[0], s2Events[0]},
                new[] {s0Events[1], s1Events[0], s2Events[0]},
                new[] {s0Events[0], s1Events[1], s2Events[0]},
                new[] {s0Events[1], s1Events[1], s2Events[0]}
            };
            EPAssertionUtil.AssertSameAnyOrder(expected, GetAndResetNewEvents(env));

            env.UndeployAll();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object theEvent)
        {
            env.SendEventBean(theEvent);
        }

        private static void SendEventsAndReset(
            RegressionEnvironment env,
            object[] events)
        {
            SendEvent(env, events);
            env.Listener("s0").Reset();
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object[] events)
        {
            for (var i = 0; i < events.Length; i++) {
                env.SendEventBean(events[i]);
            }
        }

        private static void SendMapEvent(
            RegressionEnvironment env,
            string type,
            string col1,
            string col2)
        {
            IDictionary<string, object> mapEvent = new Dictionary<string, object>();
            mapEvent.Put("col1", col1);
            mapEvent.Put("col2", col2);
            env.SendEventMap(mapEvent, type);
        }

        private static object[][] GetAndResetNewEvents(RegressionEnvironment env)
        {
            var newEvents = env.Listener("s0").LastNewData;
            env.Listener("s0").Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new[] {"S0", "S1", "S2"});
        }

        internal class EPLJoinMapLeftJoinUnsortedProps : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select t1.col1, t1.col2, t2.col1, t2.col2, t3.col1, t3.col2 from Type1#keepall as t1" +
                    " left outer join Type2#keepall as t2" +
                    " on t1.col2 = t2.col2 and t1.col1 = t2.col1" +
                    " left outer join Type3#keepall as t3" +
                    " on t1.col1 = t3.col1";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                string[] fields = {"t1.col1", "t1.col2", "t2.col1", "t2.col2", "t3.col1", "t3.col2"};

                SendMapEvent(env, "Type2", "a1", "b1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendMapEvent(env, "Type1", "b1", "a1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"b1", "a1", null, null, null, null});

                SendMapEvent(env, "Type1", "a1", "a1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"a1", "a1", null, null, null, null});

                SendMapEvent(env, "Type1", "b1", "b1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"b1", "b1", null, null, null, null});

                SendMapEvent(env, "Type1", "a1", "b1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"a1", "b1", "a1", "b1", null, null});

                SendMapEvent(env, "Type3", "c1", "b1");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                SendMapEvent(env, "Type1", "d1", "b1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"d1", "b1", null, null, null, null});

                SendMapEvent(env, "Type3", "d1", "bx");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"d1", "b1", null, null, "d1", "bx"});

                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.UndeployAll();
            }
        }

        internal class EPLJoinLeftJoin2SidesMulticolumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new[] {"S0.Id", " S0.P00", " S0.P01", " S1.Id", " S1.P10", " S1.P11", " S2.Id", " S2.P20", " S2.P21"};

                var epl = "@Name('s0') select * from " +
                          "SupportBean_S0#length(1000) as S0 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 and S0.P01 = S1.P11" +
                          " left outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 and S0.P01 = S2.P21";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean_S1(10, "A_1", "B_1"));
                env.SendEventBean(new SupportBean_S1(11, "A_2", "B_1"));
                env.SendEventBean(new SupportBean_S1(12, "A_1", "B_2"));
                env.SendEventBean(new SupportBean_S1(13, "A_2", "B_2"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S2(20, "A_1", "B_1"));
                env.SendEventBean(new SupportBean_S2(21, "A_2", "B_1"));
                env.SendEventBean(new SupportBean_S2(22, "A_1", "B_2"));
                env.SendEventBean(new SupportBean_S2(23, "A_2", "B_2"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.SendEventBean(new SupportBean_S0(1, "A_3", "B_3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1, "A_3", "B_3", null, null, null, null, null, null});

                env.SendEventBean(new SupportBean_S0(2, "A_1", "B_3"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2, "A_1", "B_3", null, null, null, null, null, null});

                env.SendEventBean(new SupportBean_S0(3, "A_3", "B_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3, "A_3", "B_1", null, null, null, null, null, null});

                env.SendEventBean(new SupportBean_S0(4, "A_2", "B_2"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {4, "A_2", "B_2", 13, "A_2", "B_2", 23, "A_2", "B_2"});

                env.SendEventBean(new SupportBean_S0(5, "A_2", "B_1"));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {5, "A_2", "B_1", 11, "A_2", "B_1", 21, "A_2", "B_1"});

                env.UndeployAll();
            }
        }

        internal class EPLJoinLeftOuterJoinRootS0OM : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var model = new EPStatementObjectModel();
                model.SelectClause = SelectClause.CreateWildcard();
                var fromClause = FromClause.Create(
                    FilterStream.Create("SupportBean_S0", "S0").AddView("keepall"),
                    FilterStream.Create("SupportBean_S1", "S1").AddView("keepall"),
                    FilterStream.Create("SupportBean_S2", "S2").AddView("keepall"));
                fromClause.Add(OuterJoinQualifier.Create("S0.P00", OuterJoinType.LEFT, "S1.P10"));
                fromClause.Add(OuterJoinQualifier.Create("S0.P00", OuterJoinType.LEFT, "S2.P20"));
                model.FromClause = fromClause;
                model = env.CopyMayFail(model);

                Assert.AreEqual(
                    "select * from SupportBean_S0#keepall as S0 " +
                    "left outer join SupportBean_S1#keepall as S1 on S0.P00 = S1.P10 " +
                    "left outer join SupportBean_S2#keepall as S2 on S0.P00 = S2.P20",
                    model.ToEPL());

                model.Annotations = Collections.SingletonList(AnnotationPart.NameAnnotation("s0"));
                env.CompileDeploy(model).AddListener("s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinLeftOuterJoinRootS0Compiled : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S0#length(1000) as S0 " +
                          "left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          "left outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20";
                env.EplToModelCompileDeploy(epl).AddListener("s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinLeftOuterJoinRootS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S0#length(1000) as S0 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          " left outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRightOuterJoinS2RootS2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query: right other join is eliminated/translated
                /// s0
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S2#length(1000) as S2 " +
                          " right outer join SupportBean_S0#length(1000) as S0 on S0.P00 = S2.P20 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRightOuterJoinS1RootS1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query: right other join is eliminated/translated
                /// s0
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S1#length(1000) as S1 " +
                          " right outer join SupportBean_S0#length(1000) as S0 on S0.P00 = S1.P10 " +
                          " left outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinInvalidMulticolumn : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl = "@Name('s0') select * from " +
                      "SupportBean_S0#length(1000) as S0 " +
                      " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 and S0.P01 = S1.P11" +
                      " left outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 and S1.P11 = S2.P21";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate outer-join expression: Outer join ON-clause columns must refer to properties of the same joined streams when using multiple columns in the on-clause");

                epl = "@Name('s0') select * from " +
                      "SupportBean_S0#length(1000) as S0 " +
                      " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 and S0.P01 = S1.P11" +
                      " left outer join SupportBean_S2#length(1000) as S2 on S2.P20 = S0.P00 and S2.P20 = S1.P11";
                TryInvalidCompile(
                    env,
                    epl,
                    "Failed to validate outer-join expression: Outer join ON-clause columns must refer to properties of the same joined streams when using multiple columns in the on-clause [");
            }
        }
    }
} // end of namespace