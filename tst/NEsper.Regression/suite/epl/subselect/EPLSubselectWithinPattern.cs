///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.framework;

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

        public static IList<RegressionExecution> WithFilterPatternNamedWindowNoAlias(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new EPLSubselectFilterPatternNamedWindowNoAlias());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryAgainstNamedWindowInUDFInPattern(
            IList<RegressionExecution> execs = null)
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

        private class EPLSubselectInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create window MyWindowInvalid#lastevent as select * from SupportBean_S0",
                    path);

                env.TryInvalidCompile(
                    "select * from SupportBean_S0(exists (select * from SupportBean_S1))",
                    "Failed to validate subquery number 1 querying SupportBean_S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window [select * from SupportBean_S0(exists (select * from SupportBean_S1))]");

                env.TryInvalidCompile(
                    path,
                    "select * from SupportBean_S0(exists (select * from MyWindowInvalid#lastevent))",
                    "Failed to validate subquery number 1 querying MyWindowInvalid: Consuming statements to a named window cannot declare a data window view onto the named window [select * from SupportBean_S0(exists (select * from MyWindowInvalid#lastevent))]");

                env.TryInvalidCompile(
                    path,
                    "select * from SupportBean_S0(Id in ((select P00 from MyWindowInvalid)))",
                    "Failed to validate filter expression 'Id in (subselect_1)': Implicit conversion not allowed: Cannot coerce types System.Nullable<System.Int32> and System.String [select * from SupportBean_S0(Id in ((select P00 from MyWindowInvalid)))]");

                env.UndeployAll();
            }
        }

        private class EPLSubselectSubqueryAgainstNamedWindowInUDFInPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create window MyWindowSNW#unique(P00)#keepall as SupportBean_S0;\n" +
                          "@name('s0') select * from pattern[SupportBean_S1(supportSingleRowFunction((select * from MyWindowSNW)))];\n";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.SendEventBean(new SupportBean_S1(1));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLSubselectFilterPatternNamedWindowNoAlias : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // subselect in pattern
                var stmtTextOne =
                    "@name('s0') select s.Id as myid from pattern [every s=SupportBean_S0(P00 in (select P10 from SupportBean_S1#lastevent))]";
                env.CompileDeployAddListenerMileZero(stmtTextOne, "s0");

                TryAssertion(env);
                env.UndeployAll();

                // subselect in filter
                var stmtTextTwo =
                    "@name('s0') select Id as myid from SupportBean_S0(P00 in (select P10 from SupportBean_S1#lastevent))";
                env.CompileDeployAddListenerMile(stmtTextTwo, "s0", 1);
                TryAssertion(env);
                env.UndeployAll();

                // subselect in filter with named window
                var epl = "create window MyS1Window#lastevent as select * from SupportBean_S1;\n" +
                          "insert into MyS1Window select * from SupportBean_S1;\n" +
                          "@name('s0') select Id as myid from SupportBean_S0(P00 in (select P10 from MyS1Window))";
                env.CompileDeployAddListenerMile(epl, "s0", 2);
                TryAssertion(env);
                env.UndeployAll();

                // subselect in pattern with named window
                epl = "create window MyS1Window#lastevent as select * from SupportBean_S1;\n" +
                      "insert into MyS1Window select * from SupportBean_S1;\n" +
                      "@name('s0') select s.Id as myid from pattern [every s=SupportBean_S0(P00 in (select P10 from MyS1Window))];\n";
                env.CompileDeployAddListenerMile(epl, "s0", 3);
                TryAssertion(env);
                env.UndeployAll();
            }
        }

        private class EPLSubselectCorrelated : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                string epl;

                epl =
                    "@name('s0') select sp1.Id as myid from pattern[every sp1=SupportBean_S0(exists (select * from SupportBean_S1#keepall as stream1 where stream1.P10 = sp1.P00))]";
                env.CompileDeployAddListenerMile(epl, "s0", 0);
                TryAssertionCorrelated(env);
                env.UndeployAll();

                epl =
                    "@name('s0') select Id as myid from SupportBean_S0(exists (select stream1.Id from SupportBean_S1#keepall as stream1 where stream1.P10 = stream0.P00)) as stream0";
                env.CompileDeployAddListenerMile(epl, "s0", 1);
                TryAssertionCorrelated(env);
                env.UndeployAll();

                epl = "@name('s0') select sp0.P00||'+'||sp1.P10 as myid from pattern[" +
                      "every sp0=SupportBean_S0 -> sp1=SupportBean_S1(P11 = (select stream2.P21 from SupportBean_S2#keepall as stream2 where stream2.P20 = sp0.P00))]";
                env.CompileDeployAddListenerMile(epl, "s0", 2);

                env.SendEventBean(new SupportBean_S2(21, "X", "A"));
                env.SendEventBean(new SupportBean_S2(22, "Y", "B"));
                env.SendEventBean(new SupportBean_S2(23, "Z", "C"));

                env.SendEventBean(new SupportBean_S0(1, "A"));
                env.SendEventBean(new SupportBean_S0(2, "Y"));
                env.SendEventBean(new SupportBean_S0(3, "C"));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S1(4, "B", "B"));
                env.AssertEqualsNew("s0", "myid", "Y+B");

                env.SendEventBean(new SupportBean_S1(4, "B", "C"));
                env.SendEventBean(new SupportBean_S1(5, "C", "B"));
                env.SendEventBean(new SupportBean_S1(6, "X", "A"));
                env.SendEventBean(new SupportBean_S1(7, "A", "C"));
                env.AssertListenerNotInvoked("s0");

                env.UndeployAll();
            }
        }

        private class EPLSubselectAggregation : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var stmtText =
                    "@name('s0') select * from SupportBean_S0(Id = (select sum(Id) from SupportBean_S1#length(2)))";
                env.CompileDeployAddListenerMileZero(stmtText, "s0");

                env.SendEventBean(new SupportBean_S0(1));
                env.SendEventBean(new SupportBean_S1(1));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S0(1));
                env.AssertListenerInvoked("s0");

                env.SendEventBean(new SupportBean_S1(3)); // now at 4
                env.SendEventBean(new SupportBean_S0(3));
                env.SendEventBean(new SupportBean_S0(5));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S0(4));
                env.AssertListenerInvoked("s0");

                env.SendEventBean(new SupportBean_S1(10)); // now at 13 (length window 2)
                env.SendEventBean(new SupportBean_S0(10));
                env.SendEventBean(new SupportBean_S0(3));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean_S0(13));
                env.AssertListenerInvoked("s0");

                env.UndeployAll();
            }
        }

        private static void TryAssertionCorrelated(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean_S0(1, "A"));
            env.SendEventBean(new SupportBean_S1(2, "A"));
            env.SendEventBean(new SupportBean_S0(3, "B"));
            env.SendEventBean(new SupportBean_S1(4, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(5, "C"));
            env.AssertEqualsNew("s0", "myid", 5);

            env.SendEventBean(new SupportBean_S0(6, "A"));
            env.AssertEqualsNew("s0", "myid", 6);

            env.SendEventBean(new SupportBean_S0(7, "D"));
            env.SendEventBean(new SupportBean_S1(8, "E"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(9, "C"));
            env.AssertEqualsNew("s0", "myid", 9);
        }

        private static void TryAssertion(RegressionEnvironment env)
        {
            env.SendEventBean(new SupportBean_S0(1, "A"));
            env.SendEventBean(new SupportBean_S1(2, "A"));
            env.SendEventBean(new SupportBean_S0(3, "B"));
            env.SendEventBean(new SupportBean_S1(4, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(5, "C"));
            env.AssertEqualsNew("s0", "myid", 5);

            env.SendEventBean(new SupportBean_S0(6, "A"));
            env.SendEventBean(new SupportBean_S0(7, "D"));
            env.SendEventBean(new SupportBean_S1(8, "E"));
            env.SendEventBean(new SupportBean_S0(9, "C"));
            env.AssertListenerNotInvoked("s0");

            env.SendEventBean(new SupportBean_S0(10, "E"));
            env.AssertEqualsNew("s0", "myid", 10);
        }

        public static bool SupportSingleRowFunction(params object[] v)
        {
            return true;
        }
    }
} // end of namespace