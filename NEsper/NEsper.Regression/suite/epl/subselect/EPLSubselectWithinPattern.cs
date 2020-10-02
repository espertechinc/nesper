///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.epl.subselect
{
    public class EPLSubselectWithinPattern
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithInvalid(execs);
            WithCorrelated(execs);
            WithAggregation(execs);
            WithSubqueryAgainstNamedWindowInUDFInPattern(execs);
            WithFilterPatternNamedWindowNoAlias(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithFilterPatternNamedWindowNoAlias(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectFilterPatternNamedWindowNoAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryAgainstNamedWindowInUDFInPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectSubqueryAgainstNamedWindowInUDFInPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithAggregation(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectAggregation());
            return execs;
        }

        public static IList<RegressionExecution> WithCorrelated(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectCorrelated());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectInvalid());
            return execs;
        }

        private static void TryAssertionCorrelated(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean_S0(1, "A"));
            env.SendEventBean(new SupportBean_S1(2, "A"));
            env.SendEventBean(new SupportBean_S0(3, "B"));
            env.SendEventBean(new SupportBean_S1(4, "C"));
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            env.SendEventBean(new SupportBean_S0(5, "C"));
            Assert.AreEqual(5, env.Listener("s0").AssertOneGetNewAndReset().Get("myId"));

            env.SendEventBean(new SupportBean_S0(6, "A"));
            Assert.AreEqual(6, env.Listener("s0").AssertOneGetNewAndReset().Get("myId"));

            env.SendEventBean(new SupportBean_S0(7, "D"));
            env.SendEventBean(new SupportBean_S1(8, "E"));
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            env.SendEventBean(new SupportBean_S0(9, "C"));
            Assert.AreEqual(9, env.Listener("s0").AssertOneGetNewAndReset().Get("myId"));
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean_S0(1, "A"));
            env.SendEventBean(new SupportBean_S1(2, "A"));
            env.SendEventBean(new SupportBean_S0(3, "B"));
            env.SendEventBean(new SupportBean_S1(4, "C"));
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            env.SendEventBean(new SupportBean_S0(5, "C"));
            Assert.AreEqual(5, env.Listener("s0").AssertOneGetNewAndReset().Get("myId"));

            env.SendEventBean(new SupportBean_S0(6, "A"));
            env.SendEventBean(new SupportBean_S0(7, "D"));
            env.SendEventBean(new SupportBean_S1(8, "E"));
            env.SendEventBean(new SupportBean_S0(9, "C"));
            Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

            env.SendEventBean(new SupportBean_S0(10, "E"));
            Assert.AreEqual(10, env.Listener("s0").AssertOneGetNewAndReset().Get("myId"));
        }

        public static bool SupportSingleRowFunction(params object[] v)
        {
            return true;
        }

        internal class EPLSubselectInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create window MyWindowInvalid#lastevent as select * from SupportBean_S0", path);

                TryInvalidCompile(
                    env,
                    "select * from SupportBean_S0(exists (select * from SupportBean_S1))",
                    "Failed to validate subquery number 1 querying SupportBean_S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window [select * from SupportBean_S0(exists (select * from SupportBean_S1))]");

                TryInvalidCompile(
                    env,
                    path,
                    "select * from SupportBean_S0(exists (select * from MyWindowInvalid#lastevent))",
                    "Failed to validate subquery number 1 querying MyWindowInvalid: Consuming statements to a named window cannot declare a data window view onto the named window [select * from SupportBean_S0(exists (select * from MyWindowInvalid#lastevent))]");

                TryInvalidCompile(
                    env,
                    path,
                    "select * from SupportBean_S0(Id in ((select P00 from MyWindowInvalid)))",
                    "Failed to validate filter expression 'Id in (subselect_1)': Implicit conversion not allowed: Cannot coerce types System.Nullable<System.Int32> and System.String [select * from SupportBean_S0(Id in ((select P00 from MyWindowInvalid)))]");

                env.UndeployAll();
            }
        }

        internal class EPLSubselectSubqueryAgainstNamedWindowInUDFInPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindowSNW#unique(P00)#keepall as SupportBean_S0;\n" +
                          "@Name('s0') select * from pattern[SupportBean_S1(supportSingleRowFunction((select * from MyWindowSNW)))];\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean_S1(1));
                env.Listener("s0").AssertInvokedAndReset();

                env.UndeployAll();
            }
        }

        internal class EPLSubselectFilterPatternNamedWindowNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // subselect in pattern
                var stmtTextOne =
                    "@Name('s0') select s.Id as myId from pattern [every s=SupportBean_S0(P00 in (select P10 from SupportBean_S1#lastevent))]";
                env.CompileDeployAddListenerMileZero(stmtTextOne, "s0");

                TryAssertion(env);
                env.UndeployAll();

                // subselect in filter
                var stmtTextTwo =
                    "@Name('s0') select Id as myId from SupportBean_S0(P00 in (select P10 from SupportBean_S1#lastevent))";
                env.CompileDeployAddListenerMile(stmtTextTwo, "s0", 1);
                TryAssertion(env);
                env.UndeployAll();

                // subselect in filter with named window
                var epl = "create window MyS1Window#lastevent as select * from SupportBean_S1;\n" +
                          "insert into MyS1Window select * from SupportBean_S1;\n" +
                          "@Name('s0') select Id as myId from SupportBean_S0(P00 in (select P10 from MyS1Window))";
                env.CompileDeployAddListenerMile(epl, "s0", 2);
                TryAssertion(env);
                env.UndeployAll();

                // subselect in pattern with named window
                epl = "create window MyS1Window#lastevent as select * from SupportBean_S1;\n" +
                      "insert into MyS1Window select * from SupportBean_S1;\n" +
                      "@Name('s0') select s.Id as myId from pattern [every s=SupportBean_S0(P00 in (select P10 from MyS1Window))];\n";
                env.CompileDeployAddListenerMile(epl, "s0", 3);
                TryAssertion(env);
                env.UndeployAll();
            }
        }

        internal class EPLSubselectCorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl =
                    "@Name('s0') select sp1.Id as myId from pattern[every sp1=SupportBean_S0(exists (select * from SupportBean_S1#keepall as stream1 where stream1.P10 = sp1.P00))]";
                env.CompileDeployAddListenerMile(epl, "s0", 0);
                TryAssertionCorrelated(env);
                env.UndeployAll();

                epl =
                    "@Name('s0') select Id as myId from SupportBean_S0(exists (select stream1.Id from SupportBean_S1#keepall as stream1 where stream1.P10 = stream0.P00)) as stream0";
                env.CompileDeployAddListenerMile(epl, "s0", 1);
                TryAssertionCorrelated(env);
                env.UndeployAll();

                epl = "@Name('s0') select sp0.P00||'+'||sp1.P10 as myId from pattern[" +
                      "every sp0=SupportBean_S0 -> sp1=SupportBean_S1(P11 = (select stream2.P21 from SupportBean_S2#keepall as stream2 where stream2.P20 = sp0.P00))]";
                env.CompileDeployAddListenerMile(epl, "s0", 2);

                env.SendEventBean(new SupportBean_S2(21, "X", "A"));
                env.SendEventBean(new SupportBean_S2(22, "Y", "B"));
                env.SendEventBean(new SupportBean_S2(23, "Z", "C"));

                env.SendEventBean(new SupportBean_S0(1, "A"));
                env.SendEventBean(new SupportBean_S0(2, "Y"));
                env.SendEventBean(new SupportBean_S0(3, "C"));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean_S1(4, "B", "B"));
                Assert.AreEqual("Y+B", env.Listener("s0").AssertOneGetNewAndReset().Get("myId"));

                env.SendEventBean(new SupportBean_S1(4, "B", "C"));
                env.SendEventBean(new SupportBean_S1(5, "C", "B"));
                env.SendEventBean(new SupportBean_S1(6, "X", "A"));
                env.SendEventBean(new SupportBean_S1(7, "A", "C"));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }

        internal class EPLSubselectAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@Name('s0') select * from SupportBean_S0(Id = (select sum(Id) from SupportBean_S1#length(2)))";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S1(1));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean_S0(1));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean_S1(3)); // now at 4
                env.SendEventBean(new SupportBean_S0(3));
                env.SendEventBean(new SupportBean_S0(5));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean_S0(4));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean_S1(10)); // now at 13 (length window 2)
                env.SendEventBean(new SupportBean_S0(10));
                env.SendEventBean(new SupportBean_S0(3));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean_S0(13));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());

                env.UndeployAll();
            }
        }
    }
} // end of namespace