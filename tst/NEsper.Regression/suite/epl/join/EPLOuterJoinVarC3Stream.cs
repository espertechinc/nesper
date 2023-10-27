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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;


namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterJoinVarC3Stream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            With0(execs);
            With1(execs);
            With2(execs);
            return execs;
        }

        public static IList<RegressionExecution> With2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinOuterInnerJoinRootS2());
            return execs;
        }

        public static IList<RegressionExecution> With1(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinOuterInnerJoinRootS1());
            return execs;
        }

        public static IList<RegressionExecution> With0(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinOuterInnerJoinRootS0());
            return execs;
        }

        private class EPLJoinOuterInnerJoinRootS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S0#length(1000) as s0 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s0.P00 = s1.P10 " +
                          " right outer join SupportBean_S2#length(1000) as s2 on s0.P00 = s2.P20 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinOuterInnerJoinRootS1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S1#length(1000) as s1 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.P00 = s1.P10 " +
                          " right outer join SupportBean_S2#length(1000) as s2 on s0.P00 = s2.P20 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinOuterInnerJoinRootS2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S2#length(1000) as s2 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.P00 = s2.P20 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s0.P00 = s1.P10 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            // Test s0 ... s1 with 0 rows, s2 with 0 rows
            //
            var s0Events = SupportBean_S0.MakeS0("A", new string[] { "A-s0-1" });
            SendEvent(env, s0Events);
            env.AssertListenerNotInvoked("s0");

            // Test s0 ... s1 with 1 rows, s2 with 0 rows
            //
            var s1Events = SupportBean_S1.MakeS1("B", new string[] { "B-s1-1" });
            SendEventsAndReset(env, s1Events);

            s0Events = SupportBean_S0.MakeS0("B", new string[] { "B-s0-1" });
            SendEvent(env, s0Events);
            env.AssertListenerNotInvoked("s0");

            // Test s0 ... s1 with 0 rows, s2 with 1 rows
            //
            var s2Events = SupportBean_S2.MakeS2("C", new string[] { "C-s2-1" });
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("C", new string[] { "C-s0-1" });
            SendEvent(env, s0Events);
            env.AssertListenerNotInvoked("s0");

            // Test s0 ... s1 with 1 rows, s2 with 1 rows
            //
            s1Events = SupportBean_S1.MakeS1("D", new string[] { "D-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("D", new string[] { "D-s2-1" });
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("D", new string[] { "D-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] }
                });

            // Test s0 ... s1 with 1 rows, s2 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("E", new string[] { "E-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("E", new string[] { "E-s2-1", "E-s2-2" });
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("E", new string[] { "E-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1] }
                });

            // Test s0 ... s1 with 2 rows, s2 with 1 rows
            //
            s1Events = SupportBean_S1.MakeS1("F", new string[] { "F-s1-1", "F-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("F", new string[] { "F-s2-1" });
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("F", new string[] { "F-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0] }
                });

            // Test s0 ... s1 with 2 rows, s2 with 2 rows
            //
            s1Events = SupportBean_S1.MakeS1("G", new string[] { "G-s1-1", "G-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("G", new string[] { "G-s2-1", "G-s2-2" });
            SendEventsAndReset(env, s2Events);

            s0Events = SupportBean_S0.MakeS0("G", new string[] { "G-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1] }
                });

            // Test s1 ... s0 with 0 rows, s2 with 0 rows
            //
            s1Events = SupportBean_S1.MakeS1("H", new string[] { "H-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], null }
                });

            // Test s1 ... s0 with 1 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("I", new string[] { "I-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("I", new string[] { "I-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], null }
                });
            // s0 is not expected in this case since s0 requires results in s2 which didn't exist

            // Test s1 ... s0 with 1 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("J", new string[] { "J-s0-1" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("J", new string[] { "J-s2-1" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("J", new string[] { "J-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] }
                });

            // Test s1 ... s0 with 1 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("K", new string[] { "K-s0-1" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("K", new string[] { "K-s2-1", "K-s2-1" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("K", new string[] { "K-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1] }
                });

            // Test s1 ... s0 with 2 rows, s2 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("L", new string[] { "L-s0-1", "L-s0-2" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("L", new string[] { "L-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], null }
                });
            // s0 is not expected in this case since s0 requires results in s2 which didn't exist

            // Test s1 ... s0 with 2 rows, s2 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("M", new string[] { "M-s0-1", "M-s0-2" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("M", new string[] { "M-s2-1" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("M", new string[] { "M-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0] }
                });

            // Test s1 ... s0 with 2 rows, s2 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("N", new string[] { "N-s0-1", "N-s0-2" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("N", new string[] { "N-s2-1", "N-s2-2" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("N", new string[] { "N-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[1] }
                });

            // Test s2 ... s0 with 0 rows, s1 with 0 rows
            //
            s2Events = SupportBean_S2.MakeS2("P", new string[] { "P-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, null, s2Events[0] }
                });

            // Test s2 ... s0 with 1 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("Q", new string[] { "Q-s0-1" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("Q", new string[] { "Q-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, null, s2Events[0] }
                });

            // Test s2 ... s0 with 1 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("R", new string[] { "R-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("R", new string[] { "R-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("R", new string[] { "R-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] }
                });

            // Test s2 ... s0 with 1 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("S", new string[] { "S-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("S", new string[] { "S-s1-1", "S-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("S", new string[] { "S-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0] }
                });

            // Test s2 ... s0 with 2 rows, s1 with 0 rows
            //
            s0Events = SupportBean_S0.MakeS0("T", new string[] { "T-s0-1", "T-s0-2" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("T", new string[] { "T-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, null, s2Events[0] }
                }); // no s0 events as they depend on s1

            // Test s2 ... s0 with 2 rows, s1 with 1 rows
            //
            s0Events = SupportBean_S0.MakeS0("U", new string[] { "U-s0-1", "U-s0-2" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("U", new string[] { "U-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("U", new string[] { "U-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0] }
                });

            // Test s2 ... s0 with 2 rows, s1 with 2 rows
            //
            s0Events = SupportBean_S0.MakeS0("V", new string[] { "V-s0-1", "V-s0-2" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("V", new string[] { "V-s1-1", "V-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("V", new string[] { "V-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0] },
                    new object[] { s0Events[1], s1Events[1], s2Events[0] }
                });

            env.UndeployAll();
        }

        private static void SendEventsAndReset(
            RegressionEnvironment env,
            object[] events)
        {
            SendEvent(env, events);
            env.ListenerReset("s0");
        }

        private static void SendEvent(
            RegressionEnvironment env,
            object[] events)
        {
            for (var i = 0; i < events.Length; i++) {
                env.SendEventBean(events[i]);
            }
        }

        private static void AssertListenerUnd(
            RegressionEnvironment env,
            object[][] expected)
        {
            env.AssertListener(
                "s0",
                listener => {
                    var und = ArrayHandlingUtil.GetUnderlyingEvents(
                        listener.GetAndResetLastNewData(),
                        new string[] { "s0", "s1", "s2" });
                    EPAssertionUtil.AssertSameAnyOrder(expected, und);
                });
        }
    }
} // end of namespace