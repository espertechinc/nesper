///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeThreadedConfigOutbound : RegressionExecutionWithConfigure
    {
        public bool EnableHATest => true;
        public bool HAWithCOnly => false;

        public void Configure(Configuration configuration)
        {
            configuration.Runtime.Threading.IsInternalTimerEnabled = false;
            configuration.Compiler.Expression.UdfCache = false;
            configuration.Runtime.Threading.IsThreadPoolOutbound = true;
            configuration.Runtime.Threading.ThreadPoolOutboundNumThreads = 5;
            configuration.Common.AddEventType("SupportBean", typeof(SupportBean));
        }

        public void Run(RegressionEnvironment env)
        {
            var listener = new SupportListenerSleeping(200);
            env.CompileDeploy("@name('s0') select * from SupportBean").Statement("s0").AddListener(listener);

            var start = PerformanceObserver.NanoTime;
            for (var i = 0; i < 5; i++) {
                env.SendEventBean(new SupportBean());
            }

            var end = PerformanceObserver.NanoTime;
            var delta = (end - start) / 1000000;
            Assert.That(delta, Is.LessThan(100), "Delta is " + delta);

            try {
                Thread.Sleep(1000);
            }
            catch (ThreadInterruptedException e) {
                throw new EPException(e);
            }

            Assert.AreEqual(5, listener.NewEvents.Count);

            env.UndeployAll();
        }
    }
} // end of namespace