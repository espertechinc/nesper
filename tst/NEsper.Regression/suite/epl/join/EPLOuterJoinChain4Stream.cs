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


namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLOuterJoinChain4Stream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            With0(execs);
            With1(execs);
            With2(execs);
            With3(execs);
            return execs;
        }

        public static IList<RegressionExecution> With3(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoinRootS3());
            return execs;
        }

        public static IList<RegressionExecution> With2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoinRootS2());
            return execs;
        }

        public static IList<RegressionExecution> With1(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoinRootS1());
            return execs;
        }

        public static IList<RegressionExecution> With0(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinLeftOuterJoinRootS0());
            return execs;
        }

        private class EPLJoinLeftOuterJoinRootS0 : RegressionExecution
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
                var epl = "@name('s0') select * from " +
                          "SupportBean_S0#length(1000) as s0 " +
                          " left outer join SupportBean_S1#length(1000) as s1 on s0.P00 = s1.P10 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s1.P10 = s2.P20 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s2.P20 = s3.P30 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinLeftOuterJoinRootS1 : RegressionExecution
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
                var epl = "@name('s0') select * from " +
                          "SupportBean_S1#length(1000) as s1 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.P00 = s1.P10 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s1.P10 = s2.P20 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s2.P20 = s3.P30 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinLeftOuterJoinRootS2 : RegressionExecution
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
                var epl = "@name('s0') select * from " +
                          "SupportBean_S2#length(1000) as s2 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s2.P20 = s1.P10 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as s0 on s1.P10 = s0.P00 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s2.P20 = s3.P30 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinLeftOuterJoinRootS3 : RegressionExecution
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
                var epl = "@name('s0') select * from " +
                          "SupportBean_S3#length(1000) as s3 " +
                          " right outer join SupportBean_S2#length(1000) as s2 on s3.P30 = s2.P20 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s2.P20 = s1.P10 " +
                          " right outer join " +
                          "SupportBean_S0#length(1000) as s0 on s1.P10 = s0.P00 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            object[] s0Events, s1Events, s2Events, s3Events;

            // Test s0 and s1=1, s2=1, s3=1
            //
            s1Events = SupportBean_S1.MakeS1("A", new string[] { "A-s1-1" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s2Events = SupportBean_S2.MakeS2("A", new string[] { "A-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s3Events = SupportBean_S3.MakeS3("A", new string[] { "A-s3-1" });
            SendEvent(env, s3Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("A", new string[] { "A-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] { new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0] } });

            // Test s0 and s1=1, s2=0, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("B", new string[] { "B-s1-1" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("B", new string[] { "B-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] { new object[] { s0Events[0], s1Events[0], null, null } });

            // Test s0 and s1=1, s2=1, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("C", new string[] { "C-s1-1" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s2Events = SupportBean_S2.MakeS2("C", new string[] { "C-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("C", new string[] { "C-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] { new object[] { s0Events[0], s1Events[0], s2Events[0], null } });

            // Test s0 and s1=2, s2=0, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("D", new string[] { "D-s1-1", "D-s1-2" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s2Events = SupportBean_S2.MakeS2("D", new string[] { "D-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("D", new string[] { "D-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], null },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], null }
                });

            // Test s0 and s1=2, s2=2, s3=0
            //
            s1Events = SupportBean_S1.MakeS1("E", new string[] { "E-s1-1", "E-s1-2" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s2Events = SupportBean_S2.MakeS2("E", new string[] { "E-s2-1", "E-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("E", new string[] { "E-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], null },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], null },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], null },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], null }
                });

            // Test s0 and s1=2, s2=2, s3=1
            //
            s1Events = SupportBean_S1.MakeS1("F", new string[] { "F-s1-1", "F-s1-2" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s2Events = SupportBean_S2.MakeS2("F", new string[] { "F-s2-1", "F-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s3Events = SupportBean_S3.MakeS3("F", new string[] { "F-s3-1" });
            SendEvent(env, s3Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("F", new string[] { "F-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[0] }
                });

            // Test s0 and s1=2, s2=2, s3=2
            //
            s1Events = SupportBean_S1.MakeS1("G", new string[] { "G-s1-1", "G-s1-2" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s2Events = SupportBean_S2.MakeS2("G", new string[] { "G-s2-1", "G-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s3Events = SupportBean_S3.MakeS3("G", new string[] { "G-s3-1", "G-s3-2" });
            SendEvent(env, s3Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("G", new string[] { "G-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[1] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[1] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[1] }
                });

            // Test s0 and s1=1, s2=1, s3=3
            //
            s1Events = SupportBean_S1.MakeS1("H", new string[] { "H-s1-1" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s2Events = SupportBean_S2.MakeS2("H", new string[] { "H-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s3Events = SupportBean_S3.MakeS3("H", new string[] { "H-s3-1", "H-s3-2", "H-s3-3" });
            SendEvent(env, s3Events);
            env.AssertListenerNotInvoked("s0");

            s0Events = SupportBean_S0.MakeS0("H", new string[] { "H-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[2] }
                });

            // Test s3 and s0=0, s1=0, s2=0
            //
            s3Events = SupportBean_S3.MakeS3("I", new string[] { "I-s3-1" });
            SendEvent(env, s3Events);
            env.AssertListenerNotInvoked("s0");

            // Test s3 and s0=0, s1=0, s2=1
            //
            s2Events = SupportBean_S2.MakeS2("J", new string[] { "J-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s3Events = SupportBean_S3.MakeS3("J", new string[] { "J-s3-1" });
            SendEvent(env, s3Events);
            env.AssertListenerNotInvoked("s0");

            // Test s3 and s0=0, s1=1, s2=1
            //
            s2Events = SupportBean_S2.MakeS2("K", new string[] { "K-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            s1Events = SupportBean_S1.MakeS1("K", new string[] { "K-s1-1" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            s3Events = SupportBean_S3.MakeS3("K", new string[] { "K-s3-1" });
            SendEvent(env, s3Events);
            env.AssertListenerNotInvoked("s0");

            // Test s3 and s0=1, s1=1, s2=1
            //
            s0Events = SupportBean_S0.MakeS0("M", new string[] { "M-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("M", new string[] { "M-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("M", new string[] { "M-s2-1" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("M", new string[] { "M-s3-1" });
            SendEvent(env, s3Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0] }
                });

            // Test s3 and s0=1, s1=2, s2=1
            //
            s0Events = SupportBean_S0.MakeS0("N", new string[] { "N-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("N", new string[] { "N-s1-1", "N-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("N", new string[] { "N-s2-1" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("N", new string[] { "N-s3-1" });
            SendEvent(env, s3Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0] }
                });

            // Test s3 and s0=1, s1=2, s2=3
            //
            s0Events = SupportBean_S0.MakeS0("O", new string[] { "O-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("O", new string[] { "O-s1-1", "O-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("O", new string[] { "O-s2-1", "O-s2-2", "O-s2-3" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("O", new string[] { "O-s3-1" });
            SendEvent(env, s3Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[2], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[2], s3Events[0] }
                });

            // Test s3 and s0=2, s1=2, s2=3
            //
            s0Events = SupportBean_S0.MakeS0("P", new string[] { "P-s0-1", "P-s0-2" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("P", new string[] { "P-s1-1", "P-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("P", new string[] { "P-s2-1", "P-s2-2", "P-s2-3" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("P", new string[] { "P-s3-1" });
            SendEvent(env, s3Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[2], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[2], s3Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[0] },
                    new object[] { s0Events[1], s1Events[1], s2Events[0], s3Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[1], s3Events[0] },
                    new object[] { s0Events[1], s1Events[1], s2Events[1], s3Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[2], s3Events[0] },
                    new object[] { s0Events[1], s1Events[1], s2Events[2], s3Events[0] }
                });

            // Test s1 and s0=0, s2=1, s3=0
            //
            s2Events = SupportBean_S2.MakeS2("Q", new string[] { "Q-s2-1" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("Q", new string[] { "Q-s1-1" });
            SendEvent(env, s1Events);
            env.AssertListenerNotInvoked("s0");

            // Test s1 and s0=2, s2=1, s3=0
            //
            s0Events = SupportBean_S0.MakeS0("R", new string[] { "R-s0-1", "R-s0-2" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("R", new string[] { "R-s2-1" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("R", new string[] { "R-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], null },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], null }
                });

            // Test s1 and s0=2, s2=2, s3=2
            //
            s0Events = SupportBean_S0.MakeS0("S", new string[] { "S-s0-1", "S-s0-2" });
            SendEventsAndReset(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("S", new string[] { "S-s2-1" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("S", new string[] { "S-s3-1", "S-s3-1" });
            SendEventsAndReset(env, s3Events);

            s1Events = SupportBean_S1.MakeS1("S", new string[] { "S-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[1] }
                });

            // Test s2 and s0=0, s1=0, s3=1
            //
            s3Events = SupportBean_S3.MakeS3("T", new string[] { "T-s3-1" });
            SendEventsAndReset(env, s3Events);

            s2Events = SupportBean_S2.MakeS2("T", new string[] { "T-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            // Test s2 and s0=0, s1=1, s3=1
            //
            s3Events = SupportBean_S3.MakeS3("U", new string[] { "U-s3-1" });
            SendEventsAndReset(env, s3Events);

            s1Events = SupportBean_S1.MakeS1("U", new string[] { "U-s1-1" });
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("U", new string[] { "U-s2-1" });
            SendEvent(env, s2Events);
            env.AssertListenerNotInvoked("s0");

            // Test s2 and s0=1, s1=1, s3=1
            //
            s0Events = SupportBean_S0.MakeS0("V", new string[] { "V-s0-1" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("V", new string[] { "V-s1-1" });
            SendEvent(env, s1Events);

            s3Events = SupportBean_S3.MakeS3("V", new string[] { "V-s3-1" });
            SendEventsAndReset(env, s3Events);

            s2Events = SupportBean_S2.MakeS2("V", new string[] { "V-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0] }
                });

            // Test s2 and s0=2, s1=2, s3=0
            //
            s0Events = SupportBean_S0.MakeS0("W", new string[] { "W-s0-1", "W-s0-2" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("W", new string[] { "W-s1-1", "W-s1-2" });
            SendEvent(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("W", new string[] { "W-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], null },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], null },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], null },
                    new object[] { s0Events[1], s1Events[1], s2Events[0], null }
                });

            // Test s2 and s0=2, s1=2, s3=2
            //
            s0Events = SupportBean_S0.MakeS0("X", new string[] { "X-s0-1", "X-s0-2" });
            SendEventsAndReset(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("X", new string[] { "X-s1-1", "X-s1-2" });
            SendEvent(env, s1Events);

            s3Events = SupportBean_S3.MakeS3("X", new string[] { "X-s3-1", "X-s3-2" });
            SendEventsAndReset(env, s3Events);

            s2Events = SupportBean_S2.MakeS2("X", new string[] { "X-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[0] },
                    new object[] { s0Events[1], s1Events[1], s2Events[0], s3Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[1] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[1] },
                    new object[] { s0Events[1], s1Events[1], s2Events[0], s3Events[1] }
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
                        new string[] { "s0", "s1", "s2", "s3" });
                    EPAssertionUtil.AssertSameAnyOrder(expected, und);
                });
        }
    }
} // end of namespace