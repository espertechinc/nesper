///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.epl.join
{
    public class EPLJoin2StreamSimple : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            var text =
                "@name('s0') select irstream s0.price, s1.price from SupportMarketDataBean(symbol='S0')#length(3) as s0," +
                "SupportMarketDataBean(symbol='S1')#length(3) as s1 " +
                " where s0.volume = s1.volume";
            env.CompileDeployAddListenerMileZero(text, "s0");

            env.SendEventBean(MakeMarketDataEvent("S0", 100, 1));
            env.AssertListenerNotInvoked("s0");

            env.Milestone(1);

            env.SendEventBean(MakeMarketDataEvent("S1", 20, 1));
            env.AssertPropsPerRowIRPairFlattened(
                "s0",
                new string[] { "s0.price", "s1.price" },
                new object[][] { new object[] { 100.0, 20.0 } },
                null);

            env.Milestone(2);

            env.SendEventBean(MakeMarketDataEvent("S1", 21, 1));
            env.AssertPropsPerRowIRPairFlattened(
                "s0",
                new string[] { "s0.price", "s1.price" },
                new object[][] { new object[] { 100.0, 21.0 } },
                null);

            env.Milestone(3);

            env.SendEventBean(MakeMarketDataEvent("S1", 22, 2));
            env.AssertListenerNotInvoked("s0");

            env.Milestone(4);

            env.SendEventBean(MakeMarketDataEvent("S1", 23, 3));
            env.AssertPropsPerRowIRPairFlattened(
                "s0",
                new string[] { "s0.price", "s1.price" },
                null,
                new object[][] { new object[] { 100.0, 20.0 } });

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