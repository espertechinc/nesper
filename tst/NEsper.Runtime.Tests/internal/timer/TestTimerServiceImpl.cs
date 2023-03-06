///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;
using System.Threading;

using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.timers;
using com.espertech.esper.container;

using NUnit.Framework;

namespace com.espertech.esper.runtime.@internal.timer
{
    [TestFixture]
    public class TestTimerServiceImpl : AbstractRuntimeTest
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [SetUp]
        public void SetUp()
        {
            callback = new SupportTimerCallback();
            service = new TimerServiceImpl(base.Container, null, 100);
            service.Callback = callback;
        }

        private SupportTimerCallback callback;
        private TimerServiceImpl service;

        protected override IContainer CreateContainer()
        {
            var container = ContainerExtensions.CreateDefaultContainer(false);
            container.Register<ITimerFactory>(ic => new SystemTimerFactory(), Lifespan.Singleton);
            container
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            return container;
        }

        private void Sleep(long msec)
        {
            try
            {
                Thread.Sleep((int) msec);
            }
            catch (ThreadInterruptedException e)
            {
                Log.Error("Interrupted: {}", e.Message, e);
            }
        }

        [Test, RunInApplicationDomain]
        public void TestClocking()
        {
            var RESOLUTION = (int) service.MsecTimerResolution;

            // Wait .55 sec
            Assert.That(callback.GetAndResetCount(), Is.Zero);
            service.StartInternalClock();
            Sleep(RESOLUTION * 5 + RESOLUTION / 2);
            service.StopInternalClock(true);
            Assert.That(callback.GetAndResetCount(), Is.EqualTo(6));

            // Check if truely stopped
            Sleep(RESOLUTION);
            Assert.That(callback.GetAndResetCount(), Is.Zero);

            // Loop for some clock cycles
            service.StartInternalClock();
            Sleep(RESOLUTION / 10);
            Assert.IsTrue(callback.GetAndResetCount() == 1);
            Sleep(service.MsecTimerResolution * 20);
            var count = callback.GetAndResetCount();
            Log.Debug(".testClocking count=" + count);
            Assert.That(count, Is.GreaterThanOrEqualTo(19L));

            // Stop and check again
            service.StopInternalClock(true);
            Sleep(RESOLUTION);
            Assert.That(callback.Count, Is.LessThanOrEqualTo(1));

            // Try some starts and stops to see
            service.StartInternalClock();
            Sleep(RESOLUTION / 5);
            service.StartInternalClock();
            Sleep(RESOLUTION / 5);
            service.StartInternalClock();
            Assert.That(callback.GetAndResetCount(), Is.GreaterThanOrEqualTo(1));

            Sleep(RESOLUTION / 5);
            Assert.That(callback.Count, Is.Zero);
            Sleep(RESOLUTION);
            Assert.That(callback.Count, Is.GreaterThanOrEqualTo(1));
            Sleep(RESOLUTION);
            Assert.That(callback.Count, Is.GreaterThanOrEqualTo(1));

            Sleep(RESOLUTION * 5);
            Assert.That(callback.GetAndResetCount(), Is.GreaterThanOrEqualTo(7));

            service.StopInternalClock(true);
            callback.GetAndResetCount();
            service.StopInternalClock(true);
            Sleep(RESOLUTION * 2);
            Assert.That(callback.Count, Is.Zero);
        }
    }
} // end of namespace
