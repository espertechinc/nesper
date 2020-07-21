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
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterJoinVarB3Stream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLJoinOuterInnerJoinRootS0());
            execs.Add(new EPLJoinOuterInnerJoinRootS1());
            execs.Add(new EPLJoinOuterInnerJoinRootS2());
            return execs;
        }

        private static void TryAssertion(
            RegressionEnvironment env,
            AtomicLong milestone)
        {
            object[] s0Events;
            object[] s1Events;
            object[] s2Events;

            // Test s0 ... s1 with 1 rows, s2 with 0 rows
            //
            s1Events = SupportBean_S1.MakeS1("A", new[] {"A-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("A", new[] {"A-s0-1"});
            SendEvent(env, s0Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s0 ... s1 with 0 rows, s2 with 1 rows
            //
            s2Events = SupportBean_S2.MakeS2("B", new[] {"B-s2-1"});
            SendEventsAndReset(env, s2Events);

            env.MilestoneInc(milestone);

            s0Events = SupportBean_S0.MakeS0("B", new[] {"B-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {new[] {s0Events[0], null, s2Events[0]}},
                GetAndResetNewEvents(env));

            // Test s0 ... s1 with 1 rows, s2 with 1 rows
            //
            s1Events = SupportBean_S1.MakeS1("C", new[] {"C-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.MilestoneInc(milestone);

            s2Events = SupportBean_S2.MakeS2("C", new[] {"C-s2-1"});
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("C", new[] {"C-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], s2Events[0]}},
                GetAndResetNewEvents(env));

            // Test s0 ... s1 with 2 rows, s2 with 1 rows
            //
            s1Events = SupportBean_S1.MakeS1("D", new[] {"D-s1-1", "D-s1-2"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("D", new[] {"D-s2-1"});
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("D", new[] {"D-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s0 ... s1 with 2 rows, s2 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("E", new[] {"E-s1-1", "E-s1-2"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("E", new[] {"E-s2-1", "E-s2-2"});
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("E", new[] {"E-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s0 ... s1 with 0 rows, s2 with 2 rows
            //
            s2Events = SupportBean_S2.MakeS2("F", new[] {"F-s2-1", "F-s2-2"});
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("F", new[] {"F-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0]},
                    new[] {s0Events[0], null, s2Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s1 ... s0 with 0 rows, s2 with 1 rows
            //
            s2Events = SupportBean_S2.MakeS2("H", new[] {"H-s2-1"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("H", new[] {"H-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s1 ... s0 with 1 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("I", new[] {"I-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("I", new[] {"I-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s1 ... s0 with 1 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("J", new[] {"J-s0-1"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("J", new[] {"J-s2-1"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("J", new[] {"J-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s1 ... s0 with 1 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("K", new[] {"K-s0-1"});
            SendEventsAndReset(env, s0Events);

            env.MilestoneInc(milestone);

            s2Events = SupportBean_S2.MakeS2("K", new[] {"K-s2-1", "K-s2-2"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("K", new[] {"K-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s1 ... s0 with 2 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("L", new[] {"L-s0-1", "L-s0-2"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("L", new[] {"L-s2-1", "L-s2-2"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("L", new[] {"L-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1]},
                    new[] {s0Events[1], s1Events[0], s2Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s2 ... s0 with 0 rows, s1 with 1 rows
            //
            s1Events = SupportBean_S1.MakeS1("P", new[] {"P-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("P", new[] {"P-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {null, null, s2Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s2 ... s1 with 0 rows, s0 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("Q", new[] {"Q-s0-1"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("Q", new[] {"Q-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s2 ... s1 with 1 rows, s0 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("R", new[] {"R-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("R", new[] {"R-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("R", new[] {"R-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s2 ... s1 with 2 rows, s0 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("S", new[] {"S-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("S", new[] {"S-s1-1", "S-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("S", new[] {"S-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s2 ... s1 with 0 rows, s0 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("T", new[] {"T-s0-1", "T-s0-1"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("T", new[] {"T-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0]},
                    new[] {s0Events[1], null, s2Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s2 ... s1 with 1 rows, s0 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("U", new[] {"U-s0-1", "U-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("U", new[] {"U-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("U", new[] {"U-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s2 ... s1 with 2 rows, s0 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("V", new[] {"V-s0-1", "V-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("V", new[] {"V-s1-1", "V-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("V", new[] {"V-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertEqualsAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[0]},
                    new[] {s0Events[1], s1Events[1], s2Events[0]}
                },
                GetAndResetNewEvents(env));

            env.UndeployAll();
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

        private static object[][] GetAndResetNewEvents(RegressionEnvironment env)
        {
            var newEvents = env.Listener("s0").LastNewData;
            Assert.IsNotNull(newEvents, "no events received");
            env.Listener("s0").Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new[] { "S0", "S1", "S2" });
        }

        internal class EPLJoinOuterInnerJoinRootS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Query:
                // s0
                var milestone = new AtomicLong();
                var epl = "@name('s0') select * from " +
                          "SupportBean_S0#length(1000) as S0 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 ";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertion(env, milestone);
            }
        }

        internal class EPLJoinOuterInnerJoinRootS1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Query:
                // s0
                var milestone = new AtomicLong();
                var epl = "@name('s0') select * from " +
                          "SupportBean_S1#length(1000) as S1 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 ";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertion(env, milestone);
            }
        }

        internal class EPLJoinOuterInnerJoinRootS2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // Query:
                // s0
                var milestone = new AtomicLong();
                var epl = "@name('s0') select * from " +
                          "SupportBean_S2#length(1000) as S2 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S2.P20 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 ";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertion(env, milestone);
            }
        }
    }
} // end of namespace