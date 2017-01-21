///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestThreadedConfigOutbound
    {
        [Test]
        public void TestOp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ThreadingConfig.IsInternalTimerEnabled = false;
            config.EngineDefaults.ExpressionConfig.IsUdfCache = false;
            config.EngineDefaults.ThreadingConfig.IsThreadPoolOutbound = true;
            config.EngineDefaults.ThreadingConfig.ThreadPoolOutboundNumThreads = 5;
            config.AddEventType<SupportBean>();

            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();

            var listener = new SupportListenerSleeping(200);
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean");
            stmt.Events += listener.Update;

            long delta = PerformanceObserver.TimeMillis(
                             delegate {
                                 for (int i = 0; i < 5; i++) {
                                     epService.EPRuntime.SendEvent(new SupportBean());
                                 }
                             });

            Assert.IsTrue(delta < 100, "Delta is " + delta);

            Thread.Sleep(1000);
            Assert.AreEqual(5, listener.NewEvents.Count);

            epService.Dispose();
        }
    }
}
