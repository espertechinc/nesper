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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.view
{
    public class ViewKeepAll
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ViewKeepAllSimple());
            execs.Add(new ViewKeepAllIterator());
            execs.Add(new ViewKeepAllWindowStats());
            return execs;
        }

        private static void SendSupportBean(
            RegressionEnvironment env,
            string @string)
        {
            env.SendEventBean(new SupportBean(@string, 0));
        }

        private static void SendEvent(
            RegressionEnvironment env,
            string symbol,
            double price)
        {
            var theEvent = new SupportMarketDataBean(symbol, price, 0L, "");
            env.SendEventBean(theEvent);
        }

        public class ViewKeepAllSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var fields = "c0".SplitCsv();

                var epl = "@Name('s0') select irstream theString as c0 from SupportBean#keepall()";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                EPAssertionUtil.AssertPropsPerRow(env.GetEnumerator("s0"), fields, new object[0][]);
                SendSupportBean(env, "E1");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E1"});

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}});
                SendSupportBean(env, "E2");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E2"});

                env.Milestone(2);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}});
                SendSupportBean(env, "E3");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E3"});

                env.Milestone(3);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
                SendSupportBean(env, "E4");
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {"E4"});
                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.GetEnumerator("s0"),
                    fields,
                    new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}, new object[] {"E4"}});

                env.Milestone(4);

                env.UndeployAll();
            }
        }

        internal class ViewKeepAllIterator : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('s0') select symbol, price from SupportMarketDataBean#keepall";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "ABC", 20);
                SendEvent(env, "DEF", 100);

                // check iterator results
                var events = env.Statement("s0").GetEnumerator();
                var theEvent = events.Advance();
                Assert.AreEqual("ABC", theEvent.Get("symbol"));
                Assert.AreEqual(20d, theEvent.Get("price"));

                theEvent = events.Advance();
                Assert.AreEqual("DEF", theEvent.Get("symbol"));
                Assert.AreEqual(100d, theEvent.Get("price"));
                Assert.IsFalse(events.MoveNext());

                SendEvent(env, "EFG", 50);

                // check iterator results
                events = env.Statement("s0").GetEnumerator();
                theEvent = events.Advance();
                Assert.AreEqual("ABC", theEvent.Get("symbol"));
                Assert.AreEqual(20d, theEvent.Get("price"));

                theEvent = events.Advance();
                Assert.AreEqual("DEF", theEvent.Get("symbol"));
                Assert.AreEqual(100d, theEvent.Get("price"));

                theEvent = events.Advance();
                Assert.AreEqual("EFG", theEvent.Get("symbol"));
                Assert.AreEqual(50d, theEvent.Get("price"));

                env.UndeployAll();
            }
        }

        internal class ViewKeepAllWindowStats : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@Name('s0') select irstream symbol, count(*) as cnt, sum(price) as mysum from SupportMarketDataBean#keepall group by Symbol";
                env.CompileDeployAddListenerMileZero(epl, "s0");

                SendEvent(env, "S1", 100);
                string[] fields = {"symbol", "cnt", "mysum"};
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"S1", 1L, 100d});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"S1", 0L, null});
                env.Listener("s0").Reset();

                SendEvent(env, "S2", 50);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"S2", 1L, 50d});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"S2", 0L, null});
                env.Listener("s0").Reset();

                SendEvent(env, "S1", 5);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"S1", 2L, 105d});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"S1", 1L, 100d});
                env.Listener("s0").Reset();

                SendEvent(env, "S2", -1);
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastNewData[0],
                    fields,
                    new object[] {"S2", 2L, 49d});
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").LastOldData[0],
                    fields,
                    new object[] {"S2", 1L, 50d});
                env.Listener("s0").Reset();

                env.UndeployAll();
            }
        }
    }
} // end of namespace