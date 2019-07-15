///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin2StreamSimple : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var text =
                "@Name('s0') select irstream s0.price, s1.price from SupportMarketDataBean(Symbol='S0')#length(3) as s0," +
                "SupportMarketDataBean(Symbol='S1')#length(3) as s1 " +
                " where s0.Volume = s1.Volume";
            env.CompileDeployAddListenerMileZero(text, "s0");

            env.SendEventBean(MakeMarketDataEvent("S0", 100, 1));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.Milestone(1);

            env.SendEventBean(MakeMarketDataEvent("S1", 20, 1));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").NewDataListFlattened,
                new[] {"s0.price", "s1.price"},
                new[] {new object[] {100.0, 20.0}});
            Assert.AreEqual(0, env.Listener("s0").OldDataListFlattened.Length);
            env.Listener("s0").Reset();

            env.Milestone(2);

            env.SendEventBean(MakeMarketDataEvent("S1", 21, 1));
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").NewDataListFlattened,
                new[] {"s0.price", "s1.price"},
                new[] {new object[] {100.0, 21.0}});
            Assert.AreEqual(0, env.Listener("s0").OldDataListFlattened.Length);
            env.Listener("s0").Reset();

            env.Milestone(3);

            env.SendEventBean(MakeMarketDataEvent("S1", 22, 2));
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.Milestone(4);

            env.SendEventBean(MakeMarketDataEvent("S1", 23, 3));
            Assert.AreEqual(0, env.Listener("s0").NewDataListFlattened.Length);
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").OldDataListFlattened,
                new[] {"s0.price", "s1.price"},
                new[] {new object[] {100.0, 20.0}});
            env.Listener("s0").Reset();

            env.Milestone(5);

            env.UndeployAll();
        }

        private static SupportMarketDataBean MakeMarketDataEvent(
            string symbol,
            double price,
            long volume)
        {
            return new SupportMarketDataBean(symbol, price, volume, null);
        }
    }
} // end of namespace