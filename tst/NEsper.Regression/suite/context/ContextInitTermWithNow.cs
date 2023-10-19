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

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextInitTermWithNow
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithStartStopWNow(execs);
            WithInitTermWithPattern(execs);
            WithInitTermWNowInvalid(execs);
            WithInitTermWNowNoEnd(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInitTermWNowNoEnd(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextInitTermWNowNoEnd());
            return execs;
        }

        public static IList<RegressionExecution> WithInitTermWNowInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextInitTermWNowInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithInitTermWithPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextInitTermWithPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithStartStopWNow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextStartStopWNow());
            return execs;
        }

        private class ContextInitTermWNowNoEnd : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context StartNowAndNeverEnd start @now;\n" +
                          "@name('s0') context StartNowAndNeverEnd select count(*) as c0 from SupportBean;\n";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(0);

                AssertSendCount(env, 1);
                AssertSendCount(env, 2);

                env.Milestone(1);

                AssertSendCount(env, 3);

                env.Milestone(2);

                AssertSendCount(env, 4);

                env.UndeployAll();
            }

            private void AssertSendCount(
                RegressionEnvironment env,
                long expected)
            {
                env.SendEventBean(new SupportBean());
                env.AssertEqualsNew("s0", "c0", expected);
            }
        }

        private class ContextStartStopWNow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var path = new RegressionPath();
                var contextExpr = "@public create context MyContext " +
                                  "as start @now end after 10 seconds";
                env.CompileDeploy(contextExpr, path);

                var fields = new string[] { "cnt" };
                var streamExpr = "@name('s0') context MyContext " +
                                 "select count(*) as cnt from SupportBean output last when terminated";
                env.CompileDeploy(streamExpr, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));

                env.Milestone(0);

                env.SendEventBean(new SupportBean("E2", 2));
                env.AdvanceTime(8000);

                env.Milestone(1);

                env.SendEventBean(new SupportBean("E3", 3));
                env.AdvanceTime(10000);
                env.AssertPropsNew("s0", fields, new object[] { 3L });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 4));
                env.AdvanceTime(19999);
                env.AssertListenerNotInvoked("s0");
                env.AdvanceTime(20000);
                env.AssertPropsNew("s0", fields, new object[] { 1L });

                env.Milestone(3);

                env.AdvanceTime(30000);
                env.AssertPropsNew("s0", fields, new object[] { 0L });

                env.EplToModelCompileDeploy(streamExpr, path);

                env.UndeployAll();

                env.EplToModelCompileDeploy(contextExpr);
                env.UndeployAll();
            }
        }

        private class ContextInitTermWithPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var path = new RegressionPath();
                var contextExpr = "@public create context MyContext " +
                                  "initiated by @Now and pattern [every timer:interval(10)] terminated after 10 sec";
                env.CompileDeploy(contextExpr, path);

                var fields = new string[] { "cnt" };
                var streamExpr = "@name('s0') context MyContext " +
                                 "select count(*) as cnt from SupportBean output last when terminated";
                env.CompileDeploy(streamExpr, path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));

                env.Milestone(0);

                env.AdvanceTime(8000);
                env.SendEventBean(new SupportBean("E3", 3));

                env.Milestone(1);

                env.AdvanceTime(9999);
                env.AssertListenerNotInvoked("s0");
                env.AdvanceTime(10000);
                env.AssertPropsNew("s0", fields, new object[] { 3L });

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 4));
                env.AdvanceTime(10100);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E5", 5));
                env.AdvanceTime(19999);
                env.AssertListenerNotInvoked("s0");

                env.AdvanceTime(20000);
                env.AssertPropsNew("s0", fields, new object[] { 2L });

                env.Milestone(4);

                env.AdvanceTime(30000);
                env.AssertPropsNew("s0", fields, new object[] { 0L });

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E6", 6));

                env.AdvanceTime(40000);
                env.AssertPropsNew("s0", fields, new object[] { 1L });

                env.EplToModelCompileDeploy(streamExpr, path);

                env.UndeployAll();
            }
        }

        private class ContextInitTermWNowInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // for overlapping contexts, @now without condition is not allowed
                env.TryInvalidCompile(
                    "create context TimedImmediate initiated @now terminated after 10 seconds",
                    "Incorrect syntax near 'terminated' (a reserved keyword) expecting 'and' but found 'terminated' at line 1 column 45 [create context TimedImmediate initiated @now terminated after 10 seconds]");

                // for non-overlapping contexts, @now with condition is not allowed
                env.TryInvalidCompile(
                    "create context TimedImmediate start @now and after 5 seconds end after 10 seconds",
                    "Incorrect syntax near 'and' (a reserved keyword) at line 1 column 41 [create context TimedImmediate start @now and after 5 seconds end after 10 seconds]");

                // for overlapping contexts, @now together with a filter condition is not allowed
                env.TryInvalidCompile(
                    "create context TimedImmediate initiated @now and SupportBean terminated after 10 seconds",
                    "Invalid use of 'now' with initiated-by stream, this combination is not supported [create context TimedImmediate initiated @now and SupportBean terminated after 10 seconds]");
            }
        }
    }
} // end of namespace