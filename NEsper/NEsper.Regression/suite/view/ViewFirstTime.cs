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
using com.espertech.esper.compat.datetime;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewFirstTime
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewFirstTimeSimple());
            execs.Add(new ViewFirstTimeSceneOne());
            execs.Add(new ViewFirstTimeSceneTwo());
            return execs;
        }

        private static void SendCurrentTime(
            RegressionEnvironment env,
            string time)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time));
        }

        private static void SendCurrentTimeWithMinus(
            RegressionEnvironment env,
            string time,
            long minus)
        {
            env.AdvanceTime(DateTimeParsingFunctions.ParseDefaultMSec(time) - minus);
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string theString,
            int intPrimitive)
        {
            env.SendEventBean(new SupportBean(theString, intPrimitive));
        }

        public class ViewFirstTimeSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = new [] { "c0", "c1" };
                env.AdvanceTime(0);
                var epl =
                    "@Name('s0') select irstream TheString as c0, IntPrimitive as c1 from SupportBean#firsttime(10 sec)";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);
                SendSupportBean(env, "E1", 1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1", 1});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 1}});
                env.AdvanceTime(2000);
                SendSupportBean(env, "E2", 20);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2", 20});

                env.Milestone(3);

                env.AdvanceTime(9999);

                env.Milestone(4);

                env.AdvanceTime(10000);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 20}});
                SendSupportBean(env, "E3", 30);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(5);

                SendSupportBean(env, "E4", 40);
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1", 1}, new object[] {"E2", 20}});

                env.UndeployAll();
            }
        }

        public class ViewFirstTimeSceneTwo : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.AdvanceTime(0);

                var text = "@Name('s0') select irstream * from SupportMarketDataBean#firsttime(1 sec)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.AdvanceTime(500);
                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E1"}}, null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}});

                env.Milestone(1);

                env.AdvanceTime(600);
                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E2"}}, null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new[] {"Symbol"},
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(2);

                env.AdvanceTime(1500);
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(3);

                env.AdvanceTime(1600);
                env.SendEventBean(MakeMarketDataEvent("E3"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                env.AdvanceTime(2000);
                env.SendEventBean(MakeMarketDataEvent("E4"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(5);

                env.UndeployAll();
            }
        }

        internal class ViewFirstTimeSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SendCurrentTime(env, "2002-02-01T09:00:00.000");
                env.CompileDeployAddListenerMileZero("@Name('s0') select * from SupportBean#firsttime(1 month)", "s0");

                SendCurrentTime(env, "2002-02-15T09:00:00.000");
                env.SendEventBean(new SupportBean("E1", 1));

                SendCurrentTimeWithMinus(env, "2002-03-01T09:00:00.000", 1);
                env.SendEventBean(new SupportBean("E2", 2));

                SendCurrentTime(env, "2002-03-01T09:00:00.000");
                env.SendEventBean(new SupportBean("E3", 3));

                EPAssertionUtil.AssertPropsPerRow(
                    env.Statement("s0").GetEnumerator(),
                    new [] { "TheString" },
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace