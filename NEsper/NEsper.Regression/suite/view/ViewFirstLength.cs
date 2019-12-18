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
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewFirstLength
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewFirstLengthSceneOne());
            execs.Add(new ViewFirstLengthMarketData());
            return execs;
        }

        private static SupportMarketDataBean MakeMarketDataEvent(string symbol)
        {
            return new SupportMarketDataBean(symbol, 0, 0L, null);
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string @string)
        {
            env.SendEventBean(new SupportBean(@string, 0));
        }

        public class ViewFirstLengthSceneOne : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.Milestone(0);

                var fields = new [] { "c0" };
                var epl = "@Name('s0') select irstream TheString as c0 from SupportBean#firstlength(2)";
                env.CompileDeploy(epl).AddListener("s0");

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);
                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}});
                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                SendSupportBean(env, "E3");
                Assert.IsFalse(env.Listener("s0").IsInvoked);

                env.Milestone(4);

                SendSupportBean(env, "E4");
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.UndeployAll();
            }
        }

        public class ViewFirstLengthMarketData : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var text = "@Name('s0') select irstream * from  SupportMarketDataBean#firstlength(3)";
                env.CompileDeployAddListenerMileZero(text, "s0");

                env.SendEventBean(MakeMarketDataEvent("E1"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E1"}}, null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new [] { "Symbol" },
                    new[] {new object[] {"E1"}});

                env.Milestone(1);

                env.SendEventBean(MakeMarketDataEvent("E2"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E2"}}, null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new [] { "Symbol" },
                    new[] {new object[] {"E1"}, new object[] {"E2"}});

                env.Milestone(2);

                env.SendEventBean(MakeMarketDataEvent("E3"));
                env.Listener("s0").AssertNewOldData(new[] {new object[] {"Symbol", "E3"}}, null);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new [] { "Symbol" },
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                env.Milestone(3);

                env.SendEventBean(MakeMarketDataEvent("E4"));
                Assert.IsFalse(env.Listener("s0").IsInvoked);
                EPAssertionUtil.AssertPropsPerRow(
                    env.GetEnumerator("s0"),
                    new [] { "Symbol" },
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

                env.UndeployAll();
            }
        }
    }
} // end of namespace