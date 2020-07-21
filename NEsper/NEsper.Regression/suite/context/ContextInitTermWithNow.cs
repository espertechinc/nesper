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

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextInitTermWithNow
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextStartStopWNow());
            execs.Add(new ContextInitTermWithPattern());
            execs.Add(new ContextInitTermWNowInvalid());
            return execs;
        }

        internal class ContextStartStopWNow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var path = new RegressionPath();
                var contextExpr = "create context MyContext " +
                                  "as start @now end after 10 seconds";
                env.CompileDeploy(contextExpr, path);

                string[] fields = {"cnt"};
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
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3L});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 4));
                env.AdvanceTime(19999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(20000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});

                env.Milestone(3);

                env.AdvanceTime(30000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {0L});

                env.EplToModelCompileDeploy(streamExpr, path);

                env.UndeployAll();

                env.EplToModelCompileDeploy(contextExpr);
                env.UndeployAll();
            }
        }

        internal class ContextInitTermWithPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);
                var path = new RegressionPath();
                var contextExpr = "create context MyContext " +
                                  "initiated by @Now and pattern [every timer:interval(10)] terminated after 10 sec";
                env.CompileDeploy(contextExpr, path);

                string[] fields = {"cnt"};
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
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                env.AdvanceTime(10000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {3L});

                env.Milestone(2);

                env.SendEventBean(new SupportBean("E4", 4));
                env.AdvanceTime(10100);

                env.Milestone(3);

                env.SendEventBean(new SupportBean("E5", 5));
                env.AdvanceTime(19999);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.AdvanceTime(20000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {2L});

                env.Milestone(4);

                env.AdvanceTime(30000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {0L});

                env.Milestone(5);

                env.SendEventBean(new SupportBean("E6", 6));

                env.AdvanceTime(40000);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {1L});

                env.EplToModelCompileDeploy(streamExpr, path);

                env.UndeployAll();
            }
        }

        internal class ContextInitTermWNowInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // for overlapping contexts, @now without condition is not allowed
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context TimedImmediate initiated @now terminated after 10 seconds",
                    "Incorrect syntax near 'terminated' (a reserved keyword) expecting 'and' but found 'terminated' at line 1 column 45 [create context TimedImmediate initiated @now terminated after 10 seconds]");

                // for non-overlapping contexts, @now with condition is not allowed
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context TimedImmediate start @now and after 5 seconds end after 10 seconds",
                    "Incorrect syntax near 'and' (a reserved keyword) at line 1 column 41 [create context TimedImmediate start @now and after 5 seconds end after 10 seconds]");

                // for overlapping contexts, @now together with a filter condition is not allowed
                SupportMessageAssertUtil.TryInvalidCompile(
                    env,
                    "create context TimedImmediate initiated @now and SupportBean terminated after 10 seconds",
                    "Invalid use of 'now' with initiated-by stream, this combination is not supported [create context TimedImmediate initiated @now and SupportBean terminated after 10 seconds]");
            }
        }
    }
} // end of namespace