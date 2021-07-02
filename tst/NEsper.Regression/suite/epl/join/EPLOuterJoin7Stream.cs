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
    public class EPLOuterJoin7Stream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithKeyPerStream(execs);
            WithRootS0(execs);
            WithRootS1(execs);
            WithRootS2(execs);
            WithRootS3(execs);
            WithRootS4(execs);
            WithRootS5(execs);
            WithRootS6(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithRootS6(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS6());
            return execs;
        }

        public static IList<RegressionExecution> WithRootS5(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS5());
            return execs;
        }

        public static IList<RegressionExecution> WithRootS4(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS4());
            return execs;
        }

        public static IList<RegressionExecution> WithRootS3(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS3());
            return execs;
        }

        public static IList<RegressionExecution> WithRootS2(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS2());
            return execs;
        }

        public static IList<RegressionExecution> WithRootS1(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS1());
            return execs;
        }

        public static IList<RegressionExecution> WithRootS0(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS0());
            return execs;
        }

        public static IList<RegressionExecution> WithKeyPerStream(IList<RegressionExecution> execs = null)
        {
            execs ??= new List<RegressionExecution>();
            execs.Add(new EPLJoinKeyPerStream());
            return execs;
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            object[] s0Events, s1Events, s2Events, s3Events, s4Events, s5Events, s6Events;

            // Test s0 and s1=0, s2=0, s3=0, s4=0, s5=0, s6=0
            //
            s0Events = SupportBean_S0.MakeS0("A", new[] {"A-s0-1"});
            SendEvent(env, s0Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s0 and s1=0, s2=1, s3=0, s4=0, s5=0, s6=0
            //
            s2Events = SupportBean_S2.MakeS2("B", new[] {"B-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s0Events = SupportBean_S0.MakeS0("B", new[] {"B-s0-1"});
            SendEvent(env, s0Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s0 and s1=0, s2=1, s3=0, s4=0, s5=0, s6=1
            //
            s2Events = SupportBean_S2.MakeS2("C", new[] {"C-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s6Events = SupportBean_S6.MakeS6("C", new[] {"C-s6-1"});
            SendEvent(env, s6Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, s2Events[0], null, null, null, s6Events[0]}
                },
                GetAndResetNewEvents(env));

            s0Events = SupportBean_S0.MakeS0("C", new[] {"C-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0], null, null, null, s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=1, s2=1, s3=1, s4=0, s5=1, s6=1
            //
            s1Events = SupportBean_S1.MakeS1("D", new[] {"D-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("D", new[] {"D-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s3Events = SupportBean_S3.MakeS3("D", new[] {"D-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, null, s3Events[0], null, null, null}
                },
                GetAndResetNewEvents(env));

            s5Events = SupportBean_S5.MakeS5("D", new[] {"D-s5-1"});
            SendEvent(env, s5Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s6Events = SupportBean_S6.MakeS6("D", new[] {"D-s6-1"});
            SendEvent(env, s6Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, s2Events[0], null, null, s5Events[0], s6Events[0]}
                },
                GetAndResetNewEvents(env));

            s0Events = SupportBean_S0.MakeS0("D", new[] {"D-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, s5Events[0], s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=1, s2=1, s3=1, s4=2, s5=1, s6=1
            //
            s1Events = SupportBean_S1.MakeS1("E", new[] {"E-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("E", new[] {"E-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("E", new[] {"E-s2-1"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("E", new[] {"E-s4-1"});
            SendEvent(env, s4Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s5Events = SupportBean_S5.MakeS5("E", new[] {"E-s5-1"});
            SendEventsAndReset(env, s5Events);

            s6Events = SupportBean_S6.MakeS6("E", new[] {"E-s6-1"});
            SendEventsAndReset(env, s6Events);

            s0Events = SupportBean_S0.MakeS0("E", new[] {"E-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=2, s2=2, s3=1, s4=2, s5=1, s6=1
            //
            s1Events = SupportBean_S1.MakeS1("F", new[] {"F-s1-1", "F-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("F", new[] {"F-s2-1", "F-s2-2"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("F", new[] {"F-s3-1"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("F", new[] {"F-s4-1"});
            SendEvent(env, s4Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s5Events = SupportBean_S5.MakeS5("F", new[] {"F-s5-1"});
            SendEventsAndReset(env, s5Events);

            s6Events = SupportBean_S6.MakeS6("F", new[] {"F-s6-1"});
            SendEventsAndReset(env, s6Events);

            s0Events = SupportBean_S0.MakeS0("F", new[] {"F-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0], s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=1, s2=1, s3=2, s4=2, s5=1, s6=2
            //
            s1Events = SupportBean_S1.MakeS1("G", new[] {"G-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("G", new[] {"G-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("G", new[] {"G-s3-1", "G-s3-2"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("G", new[] {"G-s4-1"});
            SendEvent(env, s4Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s5Events = SupportBean_S5.MakeS5("G", new[] {"G-s5-1"});
            SendEventsAndReset(env, s5Events);

            s6Events = SupportBean_S6.MakeS6("G", new[] {"G-s6-1", "G-s6-2"});
            SendEventsAndReset(env, s6Events);

            s0Events = SupportBean_S0.MakeS0("G", new[] {"G-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[1]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0], s5Events[0], s6Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s0 and s1=2, s2=2, s3=1, s4=1, s5=2, s6=1
            //
            s1Events = SupportBean_S1.MakeS1("H", new[] {"H-s1-1", "H-s1-2"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("H", new[] {"H-s2-1", "H-s2-2"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("H", new[] {"H-s3-1"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("H", new[] {"H-s4-1"});
            SendEvent(env, s4Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s5Events = SupportBean_S5.MakeS5("H", new[] {"H-s5-1", "H-s5-2"});
            SendEventsAndReset(env, s5Events);

            s6Events = SupportBean_S6.MakeS6("H", new[] {"H-s6-1"});
            SendEventsAndReset(env, s6Events);

            s0Events = SupportBean_S0.MakeS0("H", new[] {"H-s0-1"});
            SendEvent(env, s0Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1], s6Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0], s5Events[1], s6Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0], s5Events[1], s6Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0], s5Events[1], s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s1 and s0=1, s2=1, s3=1, s4=0, s5=1, s6=0
            //
            s0Events = SupportBean_S0.MakeS0("I", new[] {"I-s0-1"});
            SendEvent(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("I", new[] {"I-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("I", new[] {"I-s3-1"});
            SendEventsAndReset(env, s3Events);

            s5Events = SupportBean_S5.MakeS5("I", new[] {"I-s5-1"});
            SendEventsAndReset(env, s5Events);

            s1Events = SupportBean_S1.MakeS1("I", new[] {"I-s1-1", "I-s1-2"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked); // no s6

            // Test s1 and s0=1, s2=1, s3=1, s4=0, s5=1, s6=1
            //
            s0Events = SupportBean_S0.MakeS0("J", new[] {"J-s0-1"});
            SendEvent(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("J", new[] {"J-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("J", new[] {"J-s3-1"});
            SendEventsAndReset(env, s3Events);

            s5Events = SupportBean_S5.MakeS5("J", new[] {"J-s5-1"});
            SendEventsAndReset(env, s5Events);

            s6Events = SupportBean_S6.MakeS6("J", new[] {"J-s6-1"});
            SendEventsAndReset(env, s6Events);

            s1Events = SupportBean_S1.MakeS1("J", new[] {"J-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, s5Events[0], s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s1 and s0=1, s2=1, s3=1, s4=1, s5=0, s6=1
            //
            s0Events = SupportBean_S0.MakeS0("K", new[] {"K-s0-1"});
            SendEvent(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("K", new[] {"K-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("K", new[] {"K-s3-1"});
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("K", new[] {"K-s4-1"});
            SendEventsAndReset(env, s4Events);

            s6Events = SupportBean_S6.MakeS6("K", new[] {"K-s6-1"});
            SendEventsAndReset(env, s6Events);

            s1Events = SupportBean_S1.MakeS1("K", new[] {"K-s1-1"});
            SendEvent(env, s1Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], null, s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s2 and s0=1, s1=0, s3=0, s4=0, s5=0, s6=1
            //
            s0Events = SupportBean_S0.MakeS0("L", new[] {"L-s0-1"});
            SendEventsAndReset(env, s0Events);

            s6Events = SupportBean_S6.MakeS6("L", new[] {"L-s6-1"});
            SendEventsAndReset(env, s6Events);

            s2Events = SupportBean_S2.MakeS2("L", new[] {"L-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0], null, null, null, s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s2 and s0=1, s1=1, s3=0, s4=0, s5=1, s6=1
            //
            s0Events = SupportBean_S0.MakeS0("M", new[] {"M-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("M", new[] {"M-s1-1"});
            SendEventsAndReset(env, s1Events);

            s5Events = SupportBean_S5.MakeS5("M", new[] {"M-s5-1"});
            SendEventsAndReset(env, s5Events);

            s6Events = SupportBean_S6.MakeS6("M", new[] {"M-s6-1"});
            SendEventsAndReset(env, s6Events);

            s2Events = SupportBean_S2.MakeS2("M", new[] {"M-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0], null, null, s5Events[0], s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s2 and s0=1, s1=1, s3=1, s4=0, s5=1, s6=1
            //
            s0Events = SupportBean_S0.MakeS0("N", new[] {"N-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("N", new[] {"N-s1-1"});
            SendEventsAndReset(env, s1Events);

            s3Events = SupportBean_S3.MakeS3("N", new[] {"N-s3-1"});
            SendEventsAndReset(env, s3Events);

            s5Events = SupportBean_S5.MakeS5("N", new[] {"N-s5-1"});
            SendEventsAndReset(env, s5Events);

            s6Events = SupportBean_S6.MakeS6("N", new[] {"N-s6-1"});
            SendEventsAndReset(env, s6Events);

            s2Events = SupportBean_S2.MakeS2("N", new[] {"N-s2-1"});
            SendEvent(env, s2Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, s5Events[0], s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=1, s1=1, s2=1, s4=0, s5=0, s6=0
            //
            s0Events = SupportBean_S0.MakeS0("O", new[] {"O-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("O", new[] {"O-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("O", new[] {"O-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("O", new[] {"O-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, null, s3Events[0], null, null, null}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=1, s1=1, s2=1, s4=0, s5=0, s6=0
            //
            s0Events = SupportBean_S0.MakeS0("O", new[] {"O-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("O", new[] {"O-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("O", new[] {"O-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("O", new[] {"O-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {null, null, null, s3Events[0], null, null, null}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=1, s1=1, s2=1, s4=0, s5=0, s6=1
            //
            s0Events = SupportBean_S0.MakeS0("P", new[] {"P-s0-1"});
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("P", new[] {"P-s1-1"});
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("P", new[] {"P-s2-1"});
            SendEventsAndReset(env, s2Events);

            s6Events = SupportBean_S6.MakeS6("P", new[] {"P-s6-1"});
            SendEventsAndReset(env, s6Events);

            s3Events = SupportBean_S3.MakeS3("P", new[] {"P-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, null, s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s3 and s0=1, s1=1, s2=1, s4=2, s5=2, s6=1
            //
            s0Events = SupportBean_S0.MakeS0("Q", new[] {"Q-s0-1"});
            SendEvent(env, s0Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s1Events = SupportBean_S1.MakeS1("Q", new[] {"Q-s1-1"});
            SendEvent(env, s1Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s2Events = SupportBean_S2.MakeS2("Q", new[] {"Q-s2-1"});
            SendEvent(env, s2Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s4Events = SupportBean_S4.MakeS4("Q", new[] {"Q-s4-1", "Q-s4-2"});
            SendEvent(env, s4Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s5Events = SupportBean_S5.MakeS5("Q", new[] {"Q-s5-1", "Q-s5-2"});
            SendEvent(env, s5Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            s6Events = SupportBean_S6.MakeS6("Q", new[] {"Q-s6-1"});
            SendEvent(env, s6Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0], null, null, s5Events[0], s6Events[0]},
                    new[] {s0Events[0], null, s2Events[0], null, null, s5Events[1], s6Events[0]}
                },
                GetAndResetNewEvents(env));

            s3Events = SupportBean_S3.MakeS3("Q", new[] {"Q-s3-1"});
            SendEvent(env, s3Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], s5Events[1], s6Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1], s5Events[1], s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s4 and s0=1, s1=1, s2=0, s4=0, s5=0, s6=0
            //
            s0Events = SupportBean_S0.MakeS0("R", new[] {"R-s0-1"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("R", new[] {"R-s1-1"});
            SendEvent(env, s1Events);

            s4Events = SupportBean_S4.MakeS4("R", new[] {"R-s4-1"});
            SendEvent(env, s4Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s4 and s0=2, s1=1, s2=1, s4=0, s5=0, s6=2
            //
            s0Events = SupportBean_S0.MakeS0("S", new[] {"S-s0-1", "S-s0-2"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("S", new[] {"S-s1-1"});
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("S", new[] {"S-s2-1"});
            SendEventsAndReset(env, s2Events);

            s6Events = SupportBean_S6.MakeS6("S", new[] {"S-s6-1"});
            SendEvent(env, s6Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0], null, null, null, s6Events[0]},
                    new[] {s0Events[1], null, s2Events[0], null, null, null, s6Events[0]}
                },
                GetAndResetNewEvents(env));

            s4Events = SupportBean_S4.MakeS4("S", new[] {"S-s4-1"});
            SendEvent(env, s4Events);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            // Test s4 and s0=1, s1=1, s2=1, s4=0, s5=0, s6=1
            //
            s0Events = SupportBean_S0.MakeS0("T", new[] {"T-s0-1"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("T", new[] {"T-s1-1"});
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("T", new[] {"T-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("T", new[] {"T-s3-1"});
            SendEventsAndReset(env, s3Events);

            s6Events = SupportBean_S6.MakeS6("T", new[] {"T-s6-1"});
            SendEvent(env, s6Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, null, s6Events[0]}
                },
                GetAndResetNewEvents(env));

            s4Events = SupportBean_S4.MakeS4("T", new[] {"T-s4-1"});
            SendEvent(env, s4Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0], null, s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s5 and s0=1, s1=0, s2=1, s3=0, s4=0, s6=1
            //
            s0Events = SupportBean_S0.MakeS0("U", new[] {"U-s0-1"});
            SendEvent(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("U", new[] {"U-s2-1"});
            SendEventsAndReset(env, s2Events);

            s6Events = SupportBean_S6.MakeS6("U", new[] {"U-s6-1"});
            SendEventsAndReset(env, s6Events);

            s5Events = SupportBean_S5.MakeS5("U", new[] {"U-s5-1"});
            SendEvent(env, s5Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0], null, null, s5Events[0], s6Events[0]}
                },
                GetAndResetNewEvents(env));

            // Test s6 and s0=1, s1=2, s2=1, s3=0, s4=0, s6=2
            //
            s0Events = SupportBean_S0.MakeS0("V", new[] {"V-s0-1"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("V", new[] {"V-s1-1"});
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("V", new[] {"V-s2-1"});
            SendEventsAndReset(env, s2Events);

            s6Events = SupportBean_S6.MakeS6("V", new[] {"V-s6-1", "V-s6-2"});
            SendEventsAndReset(env, s6Events);

            s5Events = SupportBean_S5.MakeS5("V", new[] {"V-s5-1"});
            SendEvent(env, s5Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], null, s2Events[0], null, null, s5Events[0], s6Events[0]},
                    new[] {s0Events[0], null, s2Events[0], null, null, s5Events[0], s6Events[1]}
                },
                GetAndResetNewEvents(env));

            // Test s5 and s0=1, s1=2, s2=1, s3=1, s4=0, s6=1
            //
            s0Events = SupportBean_S0.MakeS0("W", new[] {"W-s0-1"});
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("W", new[] {"W-s1-1", "W-s1-2"});
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("W", new[] {"W-s2-1"});
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("W", new[] {"W-s3-1"});
            SendEventsAndReset(env, s3Events);

            s6Events = SupportBean_S6.MakeS6("W", new[] {"W-s6-1"});
            SendEventsAndReset(env, s6Events);

            s5Events = SupportBean_S5.MakeS5("W", new[] {"W-s5-1"});
            SendEvent(env, s5Events);
            EPAssertionUtil.AssertSameAnyOrder(
                new[] {
                    new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], null, s5Events[0], s6Events[0]},
                    new[] {s0Events[0], s1Events[0], s2Events[0], s3Events[0], null, s5Events[0], s6Events[0]}
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
            return ArrayHandlingUtil.GetUnderlyingEvents(newEvents, new[] {"S0", "S1", "S2", "S3", "S4", "S5", "S6"});
        }

        internal class EPLJoinKeyPerStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0 -&gt; s1
                /// -&gt; s4
                /// -&gt; s5
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S0#length(1000) as S0 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S2#length(1000) as S2 on S0.P01 = S2.P20 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P11 = S3.P30 " +
                          " left outer join SupportBean_S4#length(1000) as S4 on S1.P11 = S4.P40 " +
                          " left outer join SupportBean_S5#length(1000) as S5 on S2.P21 = S5.P50 " +
                          " right outer join SupportBean_S6#length(1000) as S6 on S2.P21 = S6.P60 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertsKeysPerStream(env);
            }

            private static void TryAssertsKeysPerStream(RegressionEnvironment env)
            {
                object[] s0Events, s1Events, s2Events, s3Events, s4Events, s5Events, s6Events;

                // Test s0
                //
                s2Events = SupportBean_S2.MakeS2("A-s0-1", new[] {"A-s2-1", "A-s2-2", "A-s2-3"});
                SendEventsAndReset(env, s2Events);

                var s6Events1 = SupportBean_S6.MakeS6("A-s2-1", new[] {"A-s6-1", "A-s6-2"});
                SendEventsAndReset(env, s6Events1);
                var s6Events2 = SupportBean_S6.MakeS6("A-s2-3", new[] {"A-s6-1"});
                SendEventsAndReset(env, s6Events2);

                s0Events = SupportBean_S0.MakeS0("A", new[] {"A-s0-1"});
                SendEvent(env, s0Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[0], null, s2Events[0], null, null, null, s6Events1[0]},
                        new[] {s0Events[0], null, s2Events[0], null, null, null, s6Events1[1]},
                        new[] {s0Events[0], null, s2Events[2], null, null, null, s6Events2[0]}
                    },
                    GetAndResetNewEvents(env));

                // Test s0
                //
                s1Events = SupportBean_S1.MakeS1("B", new[] {"B-s1-1", "B-s1-2", "B-s1-3"});
                SendEventsAndReset(env, s1Events);

                s2Events = SupportBean_S2.MakeS2("B-s0-1", new[] {"B-s2-1", "B-s2-2", "B-s2-3", "B-s2-4"});
                SendEventsAndReset(env, s2Events);

                var s5Events1 = SupportBean_S5.MakeS5("B-s2-3", new[] {"B-s6-1"});
                SendEventsAndReset(env, s5Events1);
                var s5Events2 = SupportBean_S5.MakeS5("B-s2-4", new[] {"B-s5-1", "B-s5-2"});
                SendEventsAndReset(env, s5Events2);

                s6Events = SupportBean_S6.MakeS6("B-s2-4", new[] {"B-s6-1"});
                SendEventsAndReset(env, s6Events);

                s0Events = SupportBean_S0.MakeS0("B", new[] {"B-s0-1"});
                SendEvent(env, s0Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[0], null, s2Events[3], null, null, s5Events2[1], s6Events[0]},
                        new[] {s0Events[0], null, s2Events[3], null, null, s5Events2[0], s6Events[0]}
                    },
                    GetAndResetNewEvents(env));

                // Test s0
                //
                s1Events = SupportBean_S1.MakeS1("C", new[] {"C-s1-1", "C-s1-2", "C-s1-3"});
                SendEventsAndReset(env, s1Events);

                s2Events = SupportBean_S2.MakeS2("C-s0-1", new[] {"C-s2-1", "C-s2-2"});
                SendEventsAndReset(env, s2Events);

                s3Events = SupportBean_S3.MakeS3("C-s1-2", new[] {"C-s3-1"});
                SendEventsAndReset(env, s3Events);

                s5Events1 = SupportBean_S5.MakeS5("C-s2-1", new[] {"C-s5-1"});
                SendEventsAndReset(env, s5Events1);
                s5Events2 = SupportBean_S5.MakeS5("C-s2-2", new[] {"C-s5-1", "C-s5-2"});
                SendEventsAndReset(env, s5Events2);

                s6Events1 = SupportBean_S6.MakeS6("C-s2-1", new[] {"C-s6-1"});
                SendEventsAndReset(env, s6Events1);
                s6Events2 = SupportBean_S6.MakeS6("C-s2-2", new[] {"C-s6-2"});
                SendEventsAndReset(env, s6Events2);

                s0Events = SupportBean_S0.MakeS0("C", new[] {"C-s0-1"});
                SendEvent(env, s0Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[0], s1Events[1], s2Events[0], s3Events[0], null, s5Events1[0], s6Events1[0]},
                        new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], null, s5Events2[0], s6Events2[0]},
                        new[] {s0Events[0], s1Events[1], s2Events[1], s3Events[0], null, s5Events2[1], s6Events2[0]}
                    },
                    GetAndResetNewEvents(env));

                // Test s0
                //
                s1Events = SupportBean_S1.MakeS1("D", new[] {"D-s1-3", "D-s1-2", "D-s1-1"});
                SendEventsAndReset(env, s1Events);

                s2Events = SupportBean_S2.MakeS2("D-s0-1", new[] {"D-s2-2", "D-s2-1"});
                SendEventsAndReset(env, s2Events);

                var s3Events1 = SupportBean_S3.MakeS3("D-s1-1", new[] {"D-s3-1", "D-s3-2"});
                SendEventsAndReset(env, s3Events1);
                var s3Events2 = SupportBean_S3.MakeS3("D-s1-3", new[] {"D-s3-3", "D-s3-4"});
                SendEventsAndReset(env, s3Events2);

                s4Events = SupportBean_S4.MakeS4("D-s1-2", new[] {"D-s4-1", "D-s4-2"});
                SendEventsAndReset(env, s4Events);
                s4Events = SupportBean_S4.MakeS4("D-s1-3", new[] {"D-s4-3", "D-s4-4"});
                SendEventsAndReset(env, s4Events);

                s5Events = SupportBean_S5.MakeS5("D-s2-1", new[] {"D-s5-1", "D-s5-2"});
                SendEventsAndReset(env, s5Events);

                s6Events = SupportBean_S6.MakeS6("D-s2-2", new[] {"D-s6-1"});
                SendEventsAndReset(env, s6Events);

                s0Events = SupportBean_S0.MakeS0("D", new[] {"D-s0-1"});
                SendEvent(env, s0Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[0], s1Events[0], s2Events[0], s3Events2[0], s4Events[0], null, s6Events[0]},
                        new[] {s0Events[0], s1Events[0], s2Events[0], s3Events2[0], s4Events[1], null, s6Events[0]},
                        new[] {s0Events[0], s1Events[0], s2Events[0], s3Events2[1], s4Events[0], null, s6Events[0]},
                        new[] {s0Events[0], s1Events[0], s2Events[0], s3Events2[1], s4Events[1], null, s6Events[0]},
                        new[] {s0Events[0], s1Events[2], s2Events[0], s3Events1[0], null, null, s6Events[0]},
                        new[] {s0Events[0], s1Events[2], s2Events[0], s3Events1[1], null, null, s6Events[0]}
                    },
                    GetAndResetNewEvents(env));

                // Test s1
                //
                s3Events = SupportBean_S3.MakeS3("E-s1-1", new[] {"E-s3-1"});
                SendEventsAndReset(env, s3Events);

                s4Events = SupportBean_S4.MakeS4("E-s1-1", new[] {"E-s4-1"});
                SendEventsAndReset(env, s4Events);

                s0Events = SupportBean_S0.MakeS0("E", new[] {"E-s0-1", "E-s0-2", "E-s0-3", "E-s0-4"});
                SendEvent(env, s0Events);

                var s2Events1 = SupportBean_S2.MakeS2("E-s0-1", new[] {"E-s2-1", "E-s2-2"});
                SendEventsAndReset(env, s2Events1);
                var s2Events2 = SupportBean_S2.MakeS2("E-s0-3", new[] {"E-s2-3", "E-s2-4"});
                SendEventsAndReset(env, s2Events2);
                var s2Events3 = SupportBean_S2.MakeS2("E-s0-4", new[] {"E-s2-5", "E-s2-6"});
                SendEventsAndReset(env, s2Events3);

                s5Events1 = SupportBean_S5.MakeS5("E-s2-2", new[] {"E-s5-1", "E-s5-2"});
                SendEventsAndReset(env, s5Events1);
                s5Events2 = SupportBean_S5.MakeS5("E-s2-4", new[] {"E-s5-3"});
                SendEventsAndReset(env, s5Events2);

                s6Events1 = SupportBean_S6.MakeS6("E-s2-2", new[] {"E-s6-1"});
                SendEventsAndReset(env, s6Events1);
                s6Events2 = SupportBean_S6.MakeS6("E-s2-5", new[] {"E-s6-2"});
                SendEventsAndReset(env, s6Events2);

                s1Events = SupportBean_S1.MakeS1("E", new[] {"E-s1-1"});
                SendEvent(env, s1Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {
                            s0Events[0], s1Events[0], s2Events1[1], s3Events[0], s4Events[0], s5Events1[0], s6Events1[0]
                        },
                        new[] {
                            s0Events[0], s1Events[0], s2Events1[1], s3Events[0], s4Events[0], s5Events1[1], s6Events1[0]
                        },
                        new[] {s0Events[3], s1Events[0], s2Events3[0], s3Events[0], s4Events[0], null, s6Events2[0]}
                    },
                    GetAndResetNewEvents(env));

                // Test s2
                //
                s5Events = SupportBean_S5.MakeS5("F-s2-1", new[] {"F-s5-1"});
                SendEventsAndReset(env, s5Events);

                s6Events = SupportBean_S6.MakeS6("F-s2-1", new[] {"F-s6-1"});
                SendEventsAndReset(env, s6Events);

                s0Events = SupportBean_S0.MakeS0("F", new[] {"F-s2-1", "F-s2-2"});
                SendEventsAndReset(env, s0Events);

                s3Events1 = SupportBean_S3.MakeS3("F-s1-1", new[] {"F-s3-1"});
                SendEventsAndReset(env, s3Events1);
                s3Events2 = SupportBean_S3.MakeS3("F-s1-3", new[] {"F-s3-2"});
                SendEventsAndReset(env, s3Events2);

                s4Events = SupportBean_S4.MakeS4("F-s1-1", new[] {"F-s4-1"});
                SendEventsAndReset(env, s4Events);

                var s1Events1 = SupportBean_S1.MakeS1("F", new[] {"F-s1-1"});
                SendEventsAndReset(env, s1Events1);
                var s1Events2 = SupportBean_S1.MakeS1("F", new[] {"F-s1-2"});
                SendEventsAndReset(env, s1Events2);
                var s1Events3 = SupportBean_S1.MakeS1("F", new[] {"F-s1-3"});
                SendEventsAndReset(env, s1Events3);

                s2Events = SupportBean_S2.MakeS2("F-s2-1", new[] {"F-s2-1"});
                SendEvent(env, s2Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {
                            s0Events[0], s1Events1[0], s2Events[0], s3Events1[0], s4Events[0], s5Events[0], s6Events[0]
                        },
                        new[] {s0Events[0], s1Events3[0], s2Events[0], s3Events2[0], null, s5Events[0], s6Events[0]}
                    },
                    GetAndResetNewEvents(env));

                // Test s3
                //
                s1Events = SupportBean_S1.MakeS1("G", new[] {"G-s1-3", "G-s1-2", "G-s1-3"});
                SendEventsAndReset(env, s1Events);

                s0Events = SupportBean_S0.MakeS0("G", new[] {"G-s2-1", "G-s2-2"});
                SendEventsAndReset(env, s0Events);

                s6Events = SupportBean_S6.MakeS6("G-s2-2", new[] {"G-s6-1"});
                SendEventsAndReset(env, s6Events);

                s2Events = SupportBean_S2.MakeS2("G-s2-2", new[] {"G-s2-2"});
                SendEvent(env, s2Events);

                s4Events = SupportBean_S4.MakeS4("G-s1-2", new[] {"G-s4-1"});
                SendEventsAndReset(env, s4Events);

                s3Events = SupportBean_S3.MakeS3("G-s1-2", new[] {"G-s3-1"});
                SendEvent(env, s3Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0], null, s6Events[0]}
                    },
                    GetAndResetNewEvents(env));

                // Test s3
                //
                s1Events = SupportBean_S1.MakeS1("H", new[] {"H-s1-3", "H-s1-2", "H-s1-3"});
                SendEventsAndReset(env, s1Events);

                s4Events = SupportBean_S4.MakeS4("H-s1-2", new[] {"H-s4-1"});
                SendEventsAndReset(env, s4Events);

                s0Events = SupportBean_S0.MakeS0("H", new[] {"H-s2-1", "H-s2-2", "H-s2-3"});
                SendEventsAndReset(env, s0Events);

                s2Events1 = SupportBean_S2.MakeS2("H-s2-2", new[] {"H-s2-20"});
                SendEvent(env, s2Events1);
                s2Events2 = SupportBean_S2.MakeS2("H-s2-3", new[] {"H-s2-30", "H-s2-31"});
                SendEvent(env, s2Events2);

                s6Events1 = SupportBean_S6.MakeS6("H-s2-20", new[] {"H-s6-1"});
                SendEventsAndReset(env, s6Events1);
                s6Events2 = SupportBean_S6.MakeS6("H-s2-31", new[] {"H-s6-3", "H-s6-4"});
                SendEventsAndReset(env, s6Events2);

                s3Events = SupportBean_S3.MakeS3("H-s1-2", new[] {"H-s3-1"});
                SendEvent(env, s3Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[1], s1Events[1], s2Events1[0], s3Events[0], s4Events[0], null, s6Events1[0]},
                        new[] {s0Events[2], s1Events[1], s2Events2[1], s3Events[0], s4Events[0], null, s6Events2[0]},
                        new[] {s0Events[2], s1Events[1], s2Events2[1], s3Events[0], s4Events[0], null, s6Events2[1]}
                    },
                    GetAndResetNewEvents(env));

                // Test s4
                //
                s3Events = SupportBean_S3.MakeS3("I-s1-3", new[] {"I-s3-1"});
                SendEvent(env, s3Events);

                s1Events = SupportBean_S1.MakeS1("I", new[] {"I-s1-1", "I-s1-2", "I-s1-3"});
                SendEventsAndReset(env, s1Events);

                s0Events = SupportBean_S0.MakeS0("I", new[] {"I-s2-1", "I-s2-2", "I-s2-3"});
                SendEventsAndReset(env, s0Events);

                s2Events1 = SupportBean_S2.MakeS2("I-s2-1", new[] {"I-s2-20"});
                SendEvent(env, s2Events1);
                s2Events2 = SupportBean_S2.MakeS2("I-s2-2", new[] {"I-s2-30", "I-s2-31"});
                SendEvent(env, s2Events2);

                s5Events = SupportBean_S5.MakeS5("I-s2-30", new[] {"I-s5-1", "I-s5-2"});
                SendEventsAndReset(env, s5Events);

                s6Events = SupportBean_S6.MakeS6("I-s2-30", new[] {"I-s6-1", "I-s6-2"});
                SendEventsAndReset(env, s6Events);

                s4Events = SupportBean_S4.MakeS4("I-s1-3", new[] {"I-s4-1"});
                SendEvent(env, s4Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {
                            s0Events[1], s1Events[2], s2Events2[0], s3Events[0], s4Events[0], s5Events[0], s6Events[0]
                        },
                        new[] {
                            s0Events[1], s1Events[2], s2Events2[0], s3Events[0], s4Events[0], s5Events[1], s6Events[0]
                        },
                        new[] {
                            s0Events[1], s1Events[2], s2Events2[0], s3Events[0], s4Events[0], s5Events[0], s6Events[1]
                        },
                        new[] {
                            s0Events[1], s1Events[2], s2Events2[0], s3Events[0], s4Events[0], s5Events[1], s6Events[1]
                        }
                    },
                    GetAndResetNewEvents(env));

                // Test s5
                //
                s6Events = SupportBean_S6.MakeS6("J-s2-30", new[] {"J-s6-1", "J-s6-2"});
                SendEventsAndReset(env, s6Events);

                s2Events = SupportBean_S2.MakeS2("J-s2-1", new[] {"J-s2-30", "J-s2-31"});
                SendEvent(env, s2Events);

                s5Events = SupportBean_S5.MakeS5("J-s2-30", new[] {"J-s5-1"});
                SendEvent(env, s5Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {null, null, s2Events[0], null, null, s5Events[0], s6Events[0]},
                        new[] {null, null, s2Events[0], null, null, s5Events[0], s6Events[1]}
                    },
                    GetAndResetNewEvents(env));

                // Test s5
                //
                s6Events = SupportBean_S6.MakeS6("K-s2-31", new[] {"K-s6-1", "K-s6-2"});
                SendEventsAndReset(env, s6Events);

                s0Events = SupportBean_S0.MakeS0("K", new[] {"K-s2-1"});
                SendEventsAndReset(env, s0Events);

                s2Events = SupportBean_S2.MakeS2("K-s2-1", new[] {"K-s2-30", "K-s2-31"});
                SendEvent(env, s2Events);

                s5Events = SupportBean_S5.MakeS5("K-s2-31", new[] {"K-s5-1"});
                SendEvent(env, s5Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[0], null, s2Events[1], null, null, s5Events[0], s6Events[0]},
                        new[] {s0Events[0], null, s2Events[1], null, null, s5Events[0], s6Events[1]}
                    },
                    GetAndResetNewEvents(env));

                // Test s5
                //
                s6Events = SupportBean_S6.MakeS6("L-s2-31", new[] {"L-s6-1", "L-s6-2"});
                SendEventsAndReset(env, s6Events);

                s2Events = SupportBean_S2.MakeS2("L-s2-1", new[] {"L-s2-30", "L-s2-31"});
                SendEvent(env, s2Events);

                s0Events = SupportBean_S0.MakeS0("L", new[] {"L-s2-1"});
                SendEventsAndReset(env, s0Events);

                s1Events = SupportBean_S1.MakeS1("L", new[] {"L-s1-1"});
                SendEventsAndReset(env, s1Events);

                s5Events = SupportBean_S5.MakeS5("L-s2-31", new[] {"L-s5-1"});
                SendEvent(env, s5Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[0], null, s2Events[1], null, null, s5Events[0], s6Events[0]},
                        new[] {s0Events[0], null, s2Events[1], null, null, s5Events[0], s6Events[1]}
                    },
                    GetAndResetNewEvents(env));

                // Test s5
                //
                s6Events = SupportBean_S6.MakeS6("M-s2-31", new[] {"M-s6-1", "M-s6-2"});
                SendEventsAndReset(env, s6Events);

                s2Events = SupportBean_S2.MakeS2("M-s2-1", new[] {"M-s2-30", "M-s2-31"});
                SendEvent(env, s2Events);

                s0Events = SupportBean_S0.MakeS0("M", new[] {"M-s2-1"});
                SendEventsAndReset(env, s0Events);

                s1Events = SupportBean_S1.MakeS1("M", new[] {"M-s1-1"});
                SendEventsAndReset(env, s1Events);

                s3Events = SupportBean_S3.MakeS3("M-s1-1", new[] {"M-s3-1"});
                SendEventsAndReset(env, s3Events);

                s5Events = SupportBean_S5.MakeS5("M-s2-31", new[] {"M-s5-1"});
                SendEvent(env, s5Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], null, s5Events[0], s6Events[0]},
                        new[] {s0Events[0], s1Events[0], s2Events[1], s3Events[0], null, s5Events[0], s6Events[1]}
                    },
                    GetAndResetNewEvents(env));

                // Test s5
                //
                s6Events = SupportBean_S6.MakeS6("N-s2-31", new[] {"N-s6-1", "N-s6-2"});
                SendEventsAndReset(env, s6Events);

                s2Events = SupportBean_S2.MakeS2("N-s2-1", new[] {"N-s2-30", "N-s2-31"});
                SendEvent(env, s2Events);

                s0Events = SupportBean_S0.MakeS0("N", new[] {"N-s2-1"});
                SendEventsAndReset(env, s0Events);

                s1Events = SupportBean_S1.MakeS1("N", new[] {"N-s1-1", "N-s1-2", "N-s1-3"});
                SendEventsAndReset(env, s1Events);

                s3Events = SupportBean_S3.MakeS3("N-s1-3", new[] {"N-s3-1"});
                SendEventsAndReset(env, s3Events);

                s5Events = SupportBean_S5.MakeS5("N-s2-31", new[] {"N-s5-1"});
                SendEvent(env, s5Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[0], s1Events[2], s2Events[1], s3Events[0], null, s5Events[0], s6Events[0]},
                        new[] {s0Events[0], s1Events[2], s2Events[1], s3Events[0], null, s5Events[0], s6Events[1]}
                    },
                    GetAndResetNewEvents(env));

                // Test s5
                //
                s6Events = SupportBean_S6.MakeS6("O-s2-31", new[] {"O-s6-1", "O-s6-2"});
                SendEventsAndReset(env, s6Events);

                s2Events = SupportBean_S2.MakeS2("O-s2-1", new[] {"O-s2-30", "O-s2-31"});
                SendEvent(env, s2Events);

                s0Events = SupportBean_S0.MakeS0("O", new[] {"O-s2-1"});
                SendEventsAndReset(env, s0Events);

                s1Events = SupportBean_S1.MakeS1("O", new[] {"O-s1-1", "O-s1-2", "O-s1-3"});
                SendEventsAndReset(env, s1Events);

                s3Events1 = SupportBean_S3.MakeS3("O-s1-2", new[] {"O-s3-1", "O-s3-2"});
                SendEventsAndReset(env, s3Events1);
                s3Events2 = SupportBean_S3.MakeS3("O-s1-3", new[] {"O-s3-3"});
                SendEventsAndReset(env, s3Events2);

                s5Events = SupportBean_S5.MakeS5("O-s2-31", new[] {"O-s5-1"});
                SendEvent(env, s5Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[0], s1Events[1], s2Events[1], s3Events1[0], null, s5Events[0], s6Events[0]},
                        new[] {s0Events[0], s1Events[1], s2Events[1], s3Events1[1], null, s5Events[0], s6Events[0]},
                        new[] {s0Events[0], s1Events[2], s2Events[1], s3Events2[0], null, s5Events[0], s6Events[0]},
                        new[] {s0Events[0], s1Events[1], s2Events[1], s3Events1[0], null, s5Events[0], s6Events[1]},
                        new[] {s0Events[0], s1Events[1], s2Events[1], s3Events1[1], null, s5Events[0], s6Events[1]},
                        new[] {s0Events[0], s1Events[2], s2Events[1], s3Events2[0], null, s5Events[0], s6Events[1]}
                    },
                    GetAndResetNewEvents(env));

                // Test s6
                //
                s5Events = SupportBean_S5.MakeS5("P-s2-31", new[] {"P-s5-1"});
                SendEvent(env, s5Events);

                s2Events = SupportBean_S2.MakeS2("P-s2-1", new[] {"P-s2-30", "P-s2-31"});
                SendEvent(env, s2Events);

                s0Events = SupportBean_S0.MakeS0("P", new[] {"P-s2-1"});
                SendEventsAndReset(env, s0Events);

                s1Events = SupportBean_S1.MakeS1("P", new[] {"P-s1-1", "P-s1-2", "P-s1-3"});
                SendEventsAndReset(env, s1Events);

                s3Events1 = SupportBean_S3.MakeS3("P-s1-2", new[] {"P-s3-1", "P-s3-2"});
                SendEventsAndReset(env, s3Events1);
                s3Events2 = SupportBean_S3.MakeS3("P-s1-3", new[] {"P-s3-3"});
                SendEventsAndReset(env, s3Events2);

                s6Events = SupportBean_S6.MakeS6("P-s2-31", new[] {"P-s6-1"});
                SendEvent(env, s6Events);
                EPAssertionUtil.AssertSameAnyOrder(
                    new[] {
                        new[] {s0Events[0], s1Events[1], s2Events[1], s3Events1[0], null, s5Events[0], s6Events[0]},
                        new[] {s0Events[0], s1Events[1], s2Events[1], s3Events1[1], null, s5Events[0], s6Events[0]},
                        new[] {s0Events[0], s1Events[2], s2Events[1], s3Events2[0], null, s5Events[0], s6Events[0]}
                    },
                    GetAndResetNewEvents(env));

                env.UndeployAll();
            }
        }

        internal class EPLJoinRootS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0 -&gt; s1
                /// -&gt; s4
                /// -&gt; s5
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S0#length(1000) as S0 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P10 = S3.P30 " +
                          " left outer join SupportBean_S4#length(1000) as S4 on S1.P10 = S4.P40 " +
                          " left outer join SupportBean_S5#length(1000) as S5 on S2.P20 = S5.P50 " +
                          " right outer join SupportBean_S6#length(1000) as S6 on S2.P20 = S6.P60 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRootS1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0 -&gt; s1
                /// -&gt; s4
                /// -&gt; s5
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S1#length(1000) as S1 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P10 = S3.P30 " +
                          " left outer join SupportBean_S4#length(1000) as S4 on S1.P10 = S4.P40 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 " +
                          " right outer join SupportBean_S6#length(1000) as S6 on S2.P20 = S6.P60 " +
                          " left outer join SupportBean_S5#length(1000) as S5 on S2.P20 = S5.P50 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRootS2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0 -&gt; s1
                /// -&gt; s4
                /// -&gt; s5
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S2#length(1000) as S2 " +
                          " right outer join SupportBean_S6#length(1000) as S6 on S2.P20 = S6.P60 " +
                          " left outer join SupportBean_S5#length(1000) as S5 on S2.P20 = S5.P50 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S2.P20 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P10 = S3.P30 " +
                          " left outer join SupportBean_S4#length(1000) as S4 on S1.P10 = S4.P40 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRootS3 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0 -&gt; s1
                /// -&gt; s4
                /// -&gt; s5
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S3#length(1000) as S3 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S3.P30 = S1.P10 " +
                          " left outer join SupportBean_S4#length(1000) as S4 on S1.P10 = S4.P40 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 " +
                          " left outer join SupportBean_S5#length(1000) as S5 on S2.P20 = S5.P50 " +
                          " right outer join SupportBean_S6#length(1000) as S6 on S2.P20 = S6.P60 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRootS4 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0 -&gt; s1
                /// -&gt; s4
                /// -&gt; s5
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S4#length(1000) as S4 " +
                          " right outer join SupportBean_S1#length(1000) as S1 on S4.P40 = S1.P10 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P10 = S3.P30 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S2#length(1000) as S2 on S0.P00 = S2.P20 " +
                          " left outer join SupportBean_S5#length(1000) as S5 on S2.P20 = S5.P50 " +
                          " right outer join SupportBean_S6#length(1000) as S6 on S2.P20 = S6.P60 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRootS5 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0 -&gt; s1
                /// -&gt; s4
                /// -&gt; s5
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S5#length(1000) as S5 " +
                          " right outer join SupportBean_S2#length(1000) as S2 on S2.P20 = S5.P50 " +
                          " right outer join SupportBean_S6#length(1000) as S6 on S2.P20 = S6.P60 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S2.P20 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P10 = S3.P30 " +
                          " left outer join SupportBean_S4#length(1000) as S4 on S1.P10 = S4.P40 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        internal class EPLJoinRootS6 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// s0 -&gt; s1
                /// -&gt; s4
                /// -&gt; s5
                /// </summary>
                var epl = "@Name('s0') select * from " +
                          "SupportBean_S6#length(1000) as S6 " +
                          " left outer join SupportBean_S2#length(1000) as S2 on S2.P20 = S6.P60 " +
                          " left outer join SupportBean_S5#length(1000) as S5 on S2.P20 = S5.P50 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as S0 on S0.P00 = S2.P20 " +
                          " left outer join SupportBean_S1#length(1000) as S1 on S0.P00 = S1.P10 " +
                          " right outer join SupportBean_S3#length(1000) as S3 on S1.P10 = S3.P30 " +
                          " left outer join SupportBean_S4#length(1000) as S4 on S1.P10 = S4.P40 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }
    }
} // end of namespace