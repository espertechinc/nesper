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
    public class EPLOuterJoinCart5Stream
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            With0(execs);
            With1(execs);
            With1Order2(execs);
            With2(execs);
            With2Order2(execs);
            With3(execs);
            With3Order2(execs);
            With4(execs);
            With4Order2(execs);
            return execs;
        }

        public static IList<RegressionExecution> With4Order2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS4Order2());
            return execs;
        }

        public static IList<RegressionExecution> With4(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS4());
            return execs;
        }

        public static IList<RegressionExecution> With3Order2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS3Order2());
            return execs;
        }

        public static IList<RegressionExecution> With3(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS3());
            return execs;
        }

        public static IList<RegressionExecution> With2Order2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS2Order2());
            return execs;
        }

        public static IList<RegressionExecution> With2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS2());
            return execs;
        }

        public static IList<RegressionExecution> With1Order2(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS1Order2());
            return execs;
        }

        public static IList<RegressionExecution> With1(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS1());
            return execs;
        }

        public static IList<RegressionExecution> With0(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLJoinRootS0());
            return execs;
        }

        private class EPLJoinRootS0 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// -&gt; s2
                /// -&gt; s3
                /// -&gt; s4
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S0#length(1000) as s0 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s0.p00 = s1.p10 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s1.p10 = s2.p20 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 " +
                          " left outer join SupportBean_S4#length(1000) as s4 on s1.p10 = s4.p40 ";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinRootS1 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// -&gt; s2
                /// -&gt; s3
                /// -&gt; s4
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S1#length(1000) as s1 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s1.p10 = s2.p20 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 " +
                          " left outer join SupportBean_S4#length(1000) as s4 on s1.p10 = s4.p40 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinRootS1Order2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// -&gt; s2
                /// -&gt; s3
                /// -&gt; s4
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S1#length(1000) as s1 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s1.p10 = s2.p20 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10 " +
                          " left outer join SupportBean_S4#length(1000) as s4 on s1.p10 = s4.p40 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinRootS2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// -&gt; s2
                /// -&gt; s3
                /// -&gt; s4
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S2#length(1000) as s2 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s1.p10 = s2.p20 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 " +
                          " left outer join SupportBean_S4#length(1000) as s4 on s1.p10 = s4.p40 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinRootS2Order2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// -&gt; s2
                /// -&gt; s3
                /// -&gt; s4
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S2#length(1000) as s2 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s1.p10 = s2.p20 " +
                          " left outer join SupportBean_S4#length(1000) as s4 on s1.p10 = s4.p40 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinRootS3 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// -&gt; s2
                /// -&gt; s3
                /// -&gt; s4
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S3#length(1000) as s3 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s1.p10 = s3.p30 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s1.p10 = s2.p20 " +
                          " left outer join SupportBean_S4#length(1000) as s4 on s1.p10 = s4.p40 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinRootS3Order2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// -&gt; s2
                /// -&gt; s3
                /// -&gt; s4
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S3#length(1000) as s3 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s1.p10 = s3.p30 " +
                          " left outer join SupportBean_S4#length(1000) as s4 on s1.p10 = s4.p40 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s1.p10 = s2.p20 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinRootS4 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// -&gt; s2
                /// -&gt; s3
                /// -&gt; s4
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S4#length(1000) as s4 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s1.p10 = s4.p40 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s1.p10 = s2.p20 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private class EPLJoinRootS4Order2 : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                /// <summary>
                /// Query:
                /// -&gt; s2
                /// -&gt; s3
                /// -&gt; s4
                /// </summary>
                var epl = "@name('s0') select * from " +
                          "SupportBean_S4#length(1000) as s4 " +
                          " right outer join SupportBean_S1#length(1000) as s1 on s1.p10 = s4.p40 " +
                          " left outer join " +
                          "SupportBean_S0#length(1000) as s0 on s0.p00 = s1.p10 " +
                          " left outer join SupportBean_S2#length(1000) as s2 on s1.p10 = s2.p20 " +
                          " left outer join SupportBean_S3#length(1000) as s3 on s1.p10 = s3.p30 ";

                env.CompileDeployAddListenerMileZero(epl, "s0");

                TryAssertion(env);
            }
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            object[] s0Events;
            object[] s1Events;
            object[] s2Events;
            object[] s3Events;
            object[] s4Events;

            // Test s0 and s1=0, s2=0, s3=0, s4=0
            //
            s0Events = SupportBean_S0.MakeS0("A", new string[] { "A-s0-1" });
            SendEvent(env, s0Events);
            env.AssertListenerNotInvoked("s0");

            // Test s0 and s1=1, s2=0, s3=0, s4=0
            //
            s1Events = SupportBean_S1.MakeS1("B", new string[] { "B-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] { new object[] { null, s1Events[0], null, null, null } });

            s0Events = SupportBean_S0.MakeS0("B", new string[] { "B-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] { new object[] { s0Events[0], s1Events[0], null, null, null } });

            // Test s0 and s1=1, s2=1, s3=0, s4=0
            //
            s1Events = SupportBean_S1.MakeS1("C", new string[] { "C-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("C", new string[] { "C-s2-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] { new object[] { null, s1Events[0], s2Events[0], null, null } });

            s0Events = SupportBean_S0.MakeS0("C", new string[] { "C-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] { new object[] { s0Events[0], s1Events[0], s2Events[0], null, null } });

            // Test s0 and s1=1, s2=1, s3=1, s4=0
            //
            s1Events = SupportBean_S1.MakeS1("D", new string[] { "D-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("D", new string[] { "D-s2-1" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("D", new string[] { "D-s2-1" });
            SendEvent(env, s3Events);
            AssertListenerUnd(
                env,
                new object[][] { new object[] { null, s1Events[0], s2Events[0], s3Events[0], null } });

            s0Events = SupportBean_S0.MakeS0("D", new string[] { "D-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], null }
                });

            // Test s0 and s1=1, s2=1, s3=1, s4=1
            //
            s1Events = SupportBean_S1.MakeS1("E", new string[] { "E-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("E", new string[] { "E-s2-1" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("E", new string[] { "E-s2-1" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("E", new string[] { "E-s2-1" });
            SendEvent(env, s4Events);
            AssertListenerUnd(
                env,
                new object[][] { new object[] { null, s1Events[0], s2Events[0], s3Events[0], s4Events[0] } });

            s0Events = SupportBean_S0.MakeS0("E", new string[] { "E-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0] }
                });

            // Test s0 and s1=2, s2=1, s3=1, s4=1
            //
            s1Events = SupportBean_S1.MakeS1("F", new string[] { "F-s1-1", "F-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("F", new string[] { "F-s2-1" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("F", new string[] { "F-s3-1" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("F", new string[] { "F-s2-1" });
            SendEventsAndReset(env, s4Events);

            s0Events = SupportBean_S0.MakeS0("F", new string[] { "F-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0] }
                });

            // Test s0 and s1=2, s2=2, s3=1, s4=1
            //
            s1Events = SupportBean_S1.MakeS1("G", new string[] { "G-s1-1", "G-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("G", new string[] { "G-s2-1", "G-s2-2" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("G", new string[] { "G-s3-1" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("G", new string[] { "G-s2-1" });
            SendEventsAndReset(env, s4Events);

            s0Events = SupportBean_S0.MakeS0("G", new string[] { "G-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0] }
                });

            // Test s0 and s1=2, s2=2, s3=2, s4=1
            //
            s1Events = SupportBean_S1.MakeS1("H", new string[] { "H-s1-1", "H-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("H", new string[] { "H-s2-1", "H-s2-2" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("H", new string[] { "H-s3-1", "H-s3-2" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("H", new string[] { "H-s2-1" });
            SendEventsAndReset(env, s4Events);

            s0Events = SupportBean_S0.MakeS0("H", new string[] { "H-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0] }
                });

            // Test s0 and s1=2, s2=2, s3=2, s4=2
            //
            s1Events = SupportBean_S1.MakeS1("I", new string[] { "I-s1-1", "I-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("I", new string[] { "I-s2-1", "I-s2-2" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("I", new string[] { "I-s3-1", "I-s3-2" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("I", new string[] { "I-s4-1", "I-s4-2" });
            SendEventsAndReset(env, s4Events);

            s0Events = SupportBean_S0.MakeS0("I", new string[] { "I-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[1] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[1] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[1] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[1] }
                });

            // Test s0 and s1=1, s2=1, s3=2, s4=3
            //
            s1Events = SupportBean_S1.MakeS1("J", new string[] { "J-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("J", new string[] { "J-s2-1" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("J", new string[] { "J-s3-1", "J-s3-2" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("J", new string[] { "J-s4-1", "J-s4-2", "J-s4-3" });
            SendEventsAndReset(env, s4Events);

            s0Events = SupportBean_S0.MakeS0("J", new string[] { "J-s0-1" });
            SendEvent(env, s0Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[2] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[2] }
                });

            // Test s1 and s0=0, s2=1, s3=1, s4=1
            //
            s2Events = SupportBean_S2.MakeS2("K", new string[] { "K-s2-1" });
            SendEventsAndReset(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("K", new string[] { "K-s3-1" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("K", new string[] { "K-s4-1" });
            SendEventsAndReset(env, s4Events);

            s1Events = SupportBean_S1.MakeS1("K", new string[] { "K-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], s2Events[0], s3Events[0], s4Events[0] }
                });

            // Test s1 and s0=0, s2=1, s3=0, s4=1
            //
            s2Events = SupportBean_S2.MakeS2("L", new string[] { "L-s2-1" });
            SendEventsAndReset(env, s2Events);

            s4Events = SupportBean_S4.MakeS4("L", new string[] { "L-s4-1" });
            SendEventsAndReset(env, s4Events);

            s1Events = SupportBean_S1.MakeS1("L", new string[] { "L-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], s2Events[0], null, s4Events[0] }
                });

            // Test s1 and s0=2, s2=1, s3=0, s4=1
            //
            s0Events = SupportBean_S0.MakeS0("M", new string[] { "M-s0-1", "M-s0-2" });
            SendEvent(env, s0Events);

            s2Events = SupportBean_S2.MakeS2("M", new string[] { "M-s2-1" });
            SendEventsAndReset(env, s2Events);

            s4Events = SupportBean_S4.MakeS4("M", new string[] { "M-s4-1" });
            SendEventsAndReset(env, s4Events);

            s1Events = SupportBean_S1.MakeS1("M", new string[] { "M-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], null, s4Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], null, s4Events[0] }
                });

            // Test s1 and s0=1, s2=0, s3=0, s4=0
            //
            s0Events = SupportBean_S0.MakeS0("N", new string[] { "N-s0-1" });
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("N", new string[] { "N-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], null, null, null }
                });

            // Test s1 and s0=0, s2=0, s3=1, s4=0
            //
            s3Events = SupportBean_S3.MakeS3("O", new string[] { "O-s3-1" });
            SendEventsAndReset(env, s3Events);

            s1Events = SupportBean_S1.MakeS1("O", new string[] { "O-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], null, s3Events[0], null }
                });

            // Test s1 and s0=0, s2=0, s3=0, s4=1
            //
            s4Events = SupportBean_S4.MakeS4("P", new string[] { "P-s4-1" });
            SendEventsAndReset(env, s4Events);

            s1Events = SupportBean_S1.MakeS1("P", new string[] { "P-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], null, null, s4Events[0] }
                });

            // Test s1 and s0=0, s2=0, s3=0, s4=2
            //
            s4Events = SupportBean_S4.MakeS4("Q", new string[] { "Q-s4-1", "Q-s4-2" });
            SendEventsAndReset(env, s4Events);

            s1Events = SupportBean_S1.MakeS1("Q", new string[] { "Q-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], null, null, s4Events[0] },
                    new object[] { null, s1Events[0], null, null, s4Events[1] }
                });

            // Test s1 and s0=0, s2=0, s3=2, s4=2
            //
            s3Events = SupportBean_S3.MakeS3("R", new string[] { "R-s3-1", "R-s3-2" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("R", new string[] { "R-s4-1", "R-s4-2" });
            SendEventsAndReset(env, s4Events);

            s1Events = SupportBean_S1.MakeS1("R", new string[] { "R-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], null, s3Events[0], s4Events[0] },
                    new object[] { null, s1Events[0], null, s3Events[1], s4Events[0] },
                    new object[] { null, s1Events[0], null, s3Events[0], s4Events[1] },
                    new object[] { null, s1Events[0], null, s3Events[1], s4Events[1] }
                });

            // Test s1 and s0=0, s2=2, s3=0, s4=2
            //
            s4Events = SupportBean_S4.MakeS4("S", new string[] { "S-s4-1", "S-s4-2" });
            SendEventsAndReset(env, s4Events);

            s2Events = SupportBean_S2.MakeS2("S", new string[] { "S-s2-1", "S-s2-1" });
            SendEventsAndReset(env, s2Events);

            s1Events = SupportBean_S1.MakeS1("S", new string[] { "S-s1-1" });
            SendEvent(env, s1Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], s2Events[0], null, s4Events[0] },
                    new object[] { null, s1Events[0], s2Events[0], null, s4Events[1] },
                    new object[] { null, s1Events[0], s2Events[1], null, s4Events[0] },
                    new object[] { null, s1Events[0], s2Events[1], null, s4Events[1] }
                });

            // Test s2 and s0=1, s1=2, s3=0, s4=2
            //
            s0Events = SupportBean_S0.MakeS0("U", new string[] { "U-s0-1" });
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("U", new string[] { "U-s1-1" });
            SendEventsAndReset(env, s1Events);

            s4Events = SupportBean_S4.MakeS4("U", new string[] { "U-s4-1", "U-s4-2" });
            SendEventsAndReset(env, s4Events);

            s2Events = SupportBean_S2.MakeS2("U", new string[] { "U-s1-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], null, s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], null, s4Events[1] }
                });

            // Test s2 and s0=3, s1=1, s3=2, s4=1
            //
            s0Events = SupportBean_S0.MakeS0("V", new string[] { "V-s0-1", "V-s0-2", "V-s0-3" });
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("V", new string[] { "V-s1-1" });
            SendEventsAndReset(env, s1Events);

            s3Events = SupportBean_S3.MakeS3("V", new string[] { "V-s3-1", "V-s3-2" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("V", new string[] { "V-s4-1" });
            SendEventsAndReset(env, s4Events);

            s2Events = SupportBean_S2.MakeS2("V", new string[] { "V-s1-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[2], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[2], s1Events[0], s2Events[0], s3Events[1], s4Events[0] }
                });

            // Test s2 and s0=2, s1=2, s3=2, s4=1
            //
            s0Events = SupportBean_S0.MakeS0("W", new string[] { "W-s0-1", "W-s0-2" });
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("W", new string[] { "W-s1-1", "W-s1-2" });
            SendEventsAndReset(env, s1Events);

            s3Events = SupportBean_S3.MakeS3("W", new string[] { "W-s3-1", "W-s3-2" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("W", new string[] { "W-s4-1", "W-s4-2" });
            SendEventsAndReset(env, s4Events);

            s2Events = SupportBean_S2.MakeS2("W", new string[] { "W-s1-1" });
            SendEvent(env, s2Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[1], s1Events[1], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[1] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[1] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[1] },
                    new object[] { s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[1] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[1] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[1] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[1] },
                    new object[] { s0Events[1], s1Events[1], s2Events[0], s3Events[1], s4Events[1] }
                });

            // Test s4 and s0=2, s1=2, s2=2, s3=2
            //
            s0Events = SupportBean_S0.MakeS0("X", new string[] { "X-s0-1", "X-s0-2" });
            SendEvent(env, s0Events);

            s1Events = SupportBean_S1.MakeS1("X", new string[] { "X-s1-1", "X-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("X", new string[] { "X-s2-1", "X-s2-2" });
            SendEvent(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("X", new string[] { "X-s3-1", "X-s3-2" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("X", new string[] { "X-s4-1" });
            SendEvent(env, s4Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[1], s1Events[1], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[1], s1Events[1], s2Events[0], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[0], s4Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[1], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[0], s4Events[0] },
                    new object[] { s0Events[1], s1Events[1], s2Events[1], s3Events[0], s4Events[0] },
                    new object[] { s0Events[0], s1Events[0], s2Events[1], s3Events[1], s4Events[0] },
                    new object[] { s0Events[1], s1Events[0], s2Events[1], s3Events[1], s4Events[0] },
                    new object[] { s0Events[0], s1Events[1], s2Events[1], s3Events[1], s4Events[0] },
                    new object[] { s0Events[1], s1Events[1], s2Events[1], s3Events[1], s4Events[0] }
                });

            // Test s4 and s0=0, s1=1, s2=1, s3=1
            //
            s1Events = SupportBean_S1.MakeS1("Y", new string[] { "Y-s1-1" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("Y", new string[] { "Y-s2-1" });
            SendEvent(env, s2Events);

            s3Events = SupportBean_S3.MakeS3("Y", new string[] { "Y-s3-1" });
            SendEventsAndReset(env, s3Events);

            s4Events = SupportBean_S4.MakeS4("Y", new string[] { "Y-s4-1" });
            SendEvent(env, s4Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], s2Events[0], s3Events[0], s4Events[0] }
                });

            // Test s3 and s0=0, s1=2, s2=1, s4=1
            //
            s1Events = SupportBean_S1.MakeS1("Z", new string[] { "Z-s1-1", "Z-s1-2" });
            SendEventsAndReset(env, s1Events);

            s2Events = SupportBean_S2.MakeS2("Z", new string[] { "Z-s2-1" });
            SendEventsAndReset(env, s2Events);

            s4Events = SupportBean_S4.MakeS4("Z", new string[] { "Z-s4-1" });
            SendEventsAndReset(env, s4Events);

            s3Events = SupportBean_S3.MakeS3("Z", new string[] { "Z-s3-1" });
            SendEvent(env, s3Events);
            AssertListenerUnd(
                env,
                new object[][] {
                    new object[] { null, s1Events[0], s2Events[0], s3Events[0], s4Events[0] },
                    new object[] { null, s1Events[1], s2Events[0], s3Events[0], s4Events[0] }
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
                        new string[] { "s0", "s1", "s2", "s3", "s4" });
                    EPAssertionUtil.AssertSameAnyOrder(expected, und);
                });
        }
    }
} // end of namespace