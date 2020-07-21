///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.client.extension
{
    public class ClientExtendView : RegressionExecution
    {
        public void Run(RegressionEnvironment env)
        {
            RunAssertionPlugInViewTrend(env);
            RunAssertionPlugInViewFlushed(env);
            RunAssertionInvalid(env);
        }

        private void RunAssertionPlugInViewFlushed(RegressionEnvironment env)
        {
            var text = "@name('s0') select * from SupportMarketDataBean.mynamespace:flushedsimple(Price)";
            env.CompileDeploy(text).AddListener("s0");

            SendEvent(env, 1);
            SendEvent(env, 2);
            Assert.IsFalse(env.Listener("s0").IsInvoked);

            env.UndeployAll();
        }

        private void RunAssertionPlugInViewTrend(RegressionEnvironment env)
        {
            var text = "@name('s0') select irstream * from SupportMarketDataBean.mynamespace:trendspotter(Price)";
            env.CompileDeploy(text).AddListener("s0");

            SendEvent(env, 10);
            AssertReceived(env, 1L, null);

            SendEvent(env, 11);
            AssertReceived(env, 2L, 1L);

            SendEvent(env, 12);
            AssertReceived(env, 3L, 2L);

            SendEvent(env, 11);
            AssertReceived(env, 0L, 3L);

            SendEvent(env, 12);
            AssertReceived(env, 1L, 0L);

            SendEvent(env, 0);
            AssertReceived(env, 0L, 1L);

            SendEvent(env, 0);
            AssertReceived(env, 0L, 0L);

            SendEvent(env, 1);
            AssertReceived(env, 1L, 0L);

            SendEvent(env, 1);
            AssertReceived(env, 1L, 1L);

            SendEvent(env, 2);
            AssertReceived(env, 2L, 1L);

            SendEvent(env, 2);
            AssertReceived(env, 2L, 2L);

            env.UndeployAll();
        }

        private void RunAssertionInvalid(RegressionEnvironment env)
        {
            TryInvalidCompile(
                env,
                "select * from SupportMarketDataBean.mynamespace:xxx()",
                "Failed to validate data window declaration: View name 'mynamespace:xxx' is not a known view name");
            TryInvalidCompile(
                env,
                "select * from SupportMarketDataBean.mynamespace:invalid()",
                "Failed to validate data window declaration: Error instantiating view factory instance to " +
                typeof(ViewFactoryForge).FullName +
                " interface for view 'invalid'");
        }

        private void SendEvent(
            RegressionEnvironment env,
            double price)
        {
            env.SendEventBean(new SupportMarketDataBean("", price, null, null));
        }

        private void AssertReceived(
            RegressionEnvironment env,
            long? newTrendCount,
            long? oldTrendCount)
        {
            EPAssertionUtil.AssertPropsPerRow(
                env.Listener("s0").AssertInvokedAndReset(),
                "trendcount",
                new object[] {newTrendCount},
                new object[] {oldTrendCount});
        }
    }
} // end of namespace