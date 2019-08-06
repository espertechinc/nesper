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
using com.espertech.esper.regressionlib.support.bean;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterJoinChain4Stream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoinRootS0());
            execs.Add(new EPLJoinLeftOuterJoinRootS1());
            execs.Add(new EPLJoinLeftOuterJoinRootS2());
            execs.Add(new EPLJoinLeftOuterJoinRootS3());
            return execs;
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            object[] s0Events, s1Events, s2Events, s3Events;

            // Test s0 and s1=1, s2=1, s3=1
            //
            s1Events = SupportBean_S1.MakeS1("A", new[] {"A-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("A", new[] {"A-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s3Events = SupportBean_S3.MakeS3("A", new[] {"A-s3-1"});
            SendEvent(env, s3Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("A", new[] {"A-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]}},
                GetAndResetNewEvents(env));

            // Test s0 and s1=1, s2=0, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("B", new[] {"B-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("B", new[] {"B-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], null, null}},
                GetAndResetNewEvents(env));

            // Test s0 and s1=1, s2=1, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("C", new[] {"C-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("C", new[] {"C-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("C", new[] {"C-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {new[] {s0Events[0], s1Events[0], s2Events[0], null}},
                GetAndResetNewEvents(env));

            // Test s0 and s1=2, s2=0, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("D", new[] {"D-s1-1", "D-s1-2"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("D", new[] {"D-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("D", new[] {"D-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], null},
                    new[] {s0Events[0], s1Events[1], s2Events[0], null}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=2, s2=2, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("E", new[] {"E-s1-1", "E-s1-2"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("E", new[] {"E-s2-1", "E-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("E", new[] {"E-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], null},
                    new[] {s0Events[0], s1Events[1], s2Events[0], null},
                    new[] {s0Events[0], s1Events[0], s2Events[1], null},
                    new[] {s0Events[0], s1Events[1], s2Events[1], null}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=2, s2=2, s3=1
            //
            s1Events = SupportBean_S1.MakeS1("F", new[] {"F-s1-1", "F-s1-2"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("F", new[] {"F-s2-1", "F-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s3Events = SupportBean_S3.MakeS3("F", new[] {"F-s3-1"});
            SendEvent(env, s3Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("F", new[] {"F-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=2, s2=2, s3=2
            //
            s1Events = SupportBean_S1.MakeS1("G", new[] {"G-s1-1", "G-s1-2"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("G", new[] {"G-s2-1", "G-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s3Events = SupportBean_S3.MakeS3("G", new[] {"G-s3-1", "G-s3-2"});
            SendEvent(env, s3Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("G", new[] {"G-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=1, s2=1, s3=3
            //
            s1Events = SupportBean_S1.MakeS1("H", new[] {"H-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("H", new[] {"H-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s3Events = SupportBean_S3.MakeS3("H", new[] {"H-s3-1", "H-s3-2", "H-s3-3"});
            SendEvent(env, s3Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("H", new[] {"H-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[2]}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=0, s1=0, s2=0
            //
            s3Events = SupportBean_S3.MakeS3("I", new[] {"I-s3-1"});
            SendEvent(env, s3Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s3 and s0=0, s1=0, s2=1
            //
            s2Events = SupportBean_S2.MakeS2("J", new[] {"J-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s3Events = SupportBean_S3.MakeS3("J", new[] {"J-s3-1"});
            SendEvent(env, s3Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s3 and s0=0, s1=1, s2=1
            //
            s2Events = SupportBean_S2.MakeS2("K", new[] {"K-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s1Events = SupportBean_S1.MakeS1("K", new[] {"K-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s3Events = SupportBean_S3.MakeS3("K", new[] {"K-s3-1"});
            SendEvent(env, s3Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s3 and s0=1, s1=1, s2=1
            //
            s0Events = SupportBean_S0.MakeS0("M", new[] {"M-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("M", new[] {"M-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("M", new[] {"M-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("M", new[] {"M-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=1, s1=2, s2=1
            //
            s0Events = SupportBean_S0.MakeS0("N", new[] {"N-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("N", new[] {"N-s1-1", "N-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("N", new[] {"N-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("N", new[] {"N-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=1, s1=2, s2=3
            //
            s0Events = SupportBean_S0.MakeS0("O", new[] {"O-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("O", new[] {"O-s1-1", "O-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("O", new[] {"O-s2-1", "O-s2-2", "O-s2-3"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("O", new[] {"O-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[2], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[2], s3Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=2, s1=2, s2=3
            //
            s0Events = SupportBean_S0.MakeS0("P", new[] {"P-s0-1", "P-s0-2"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("P", new[] {"P-s1-1", "P-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("P", new[] {"P-s2-1", "P-s2-2", "P-s2-3"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("P", new[] {"P-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[2], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[2], s3Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0]},
                    new[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[1], s3Events[0]},
                    new[] {s0Events[1], s1Events[1], s2Events[1], s3Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[2], s3Events[0]},
                    new[] {s0Events[1], s1Events[1], s2Events[2], s3Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s1 and s0=0, s2=1, s3=0
            //
            s2Events = SupportBean_S2.MakeS2("Q", new[] {"Q-s2-1"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("Q", new[] {"Q-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s1 and s0=2, s2=1, s3=0
            //
            s0Events = SupportBean_S0.MakeS0("R", new[] {"R-s0-1", "R-s0-2"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("R", new[] {"R-s2-1"});
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("R", new[] {"R-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], null},
                    new[] {s0Events[1], s1Events[0], s2Events[0], null}
                },
                GetAndResetNewEvents(env));

            // Test s1 and s0=2, s2=2, s3=2
            //
            s0Events = SupportBean_S0.MakeS0("S", new[] {"S-s0-1", "S-s0-2"});
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("S", new[] {"S-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("S", new[] {"S-s3-1", "S-s3-1"});
            SendEventsAndReset(env, s3Events);

            s1Events = SupportBean_S1.MakeS1("S", new[] {"S-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
                    new[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s2 and s0=0, s1=0, s3=1
            //
            s3Events = SupportBean_S3.MakeS3("T", new[] {"T-s3-1"});
            SendEventsAndReset(env, s3Events);

            s2Events = SupportBean_S2.MakeS2("T", new[] {"T-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s2 and s0=0, s1=1, s3=1
            //
            s3Events = SupportBean_S3.MakeS3("U", new[] {"U-s3-1"});
            SendEventsAndReset(env, s3Events);

            s1Events = SupportBean_S1.MakeS1("U", new[] {"U-s1-1"});
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("U", new[] {"U-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s2 and s0=1, s1=1, s3=1
            //
            s0Events = SupportBean_S0.MakeS0("V", new[] {"V-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("V", new[] {"V-s1-1"});
            SendEvent(env, s1Events);

            s3Events = SupportBean_S3.MakeS3("V", new[] {"V-s3-1"});
            SendEventsAndReset(env, s3Events);

            s2Events = SupportBean_S2.MakeS2("V", new[] {"V-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s2 and s0=2, s1=2, s3=0
            //
            s0Events = SupportBean_S0.MakeS0("W", new[] {"W-s0-1", "W-s0-2"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("W", new[] {"W-s1-1", "W-s1-2"});
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("W", new[] {"W-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], null},
                    new[] {s0Events[0], s1Events[1], s2Events[0], null},
                    new[] {s0Events[1], s1Events[0], s2Events[0], null},
                    new[] {s0Events[1], s1Events[1], s2Events[0], null}
                },
                GetAndResetNewEvents(env));

            // Test s2 and s0=2, s1=2, s3=2
            //
            s0Events = SupportBean_S0.MakeS0("X", new[] {"X-s0-1", "X-s0-2"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("X", new[] {"X-s1-1", "X-s1-2"});
            SendEvent(env, s1Events);

            s3Events = SupportBean_S3.MakeS3("X", new[] {"X-s3-1", "X-s3-2"});
            SendEventsAndReset(env, s3Events);

            s2Events = SupportBean_S2.MakeS2("X", new[] {"X-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0]},
                    new[] {s0Events[1], s1Events[0], s2Events[0], s3Events[0]},
                    new[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[1]},
                    new[] {s0Events[1], s1Events[0], s2Events[0], s3Events[1]},
                    new[] {s0Events[1], s1Events[1], s2Events[0], s3Events[1]}
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
            env.Listener("s0").Reset();
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new[] {"s0", "s1", "s2", "s3"});
        }

        internal class EPLJoinLeftOuterJoinRootS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0
                /// -&gt; s1
                /// -&gt; s2
                /// -&gt; s3
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S0#length(1000) as s0 " +
                          " left outer join SupportBean_S1#length(1000) as s1 on s0.P00 = s1.P10 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s1.P10 = s2.P20 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s2.P20 = s3.P30 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinLeftOuterJoinRootS1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0
                /// -&gt; s1
                /// -&gt; s2
                /// -&gt; s3
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S1#length(1000) as s1 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.P00 = s1.P10 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s1.P10 = s2.P20 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s2.P20 = s3.P30 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinLeftOuterJoinRootS2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0
                /// -&gt; s1
                /// -&gt; s2
                /// -&gt; s3
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S2#length(1000) as s2 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s2.P20 = s1.P10 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as s0 on s1.P10 = s0.P00 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s2.P20 = s3.P30 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinLeftOuterJoinRootS3 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0
                /// -&gt; s1
                /// -&gt; s2
                /// -&gt; s3
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S3#length(1000) as s3 " +
                          " right outer join SupportBean_S2#length(1000) as s2 on s3.P30 = s2.P20 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s2.P20 = s1.P10 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as s0 on s1.P10 = s0.P00 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }
    }
} // end of namespace