///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.timer;

using NUnit.Framework;

namespace com.espertech.esper.regression.timer
{
    [TestFixture]
    public class TestTimeSource
    {
        [Test]
        public void TestInternalTimeSource()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Threading.IsInternalTimerEnabled = true;

            var serviceProvider = EPServiceProviderManager.GetDefaultProvider(config);
            serviceProvider.Initialize();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(serviceProvider, GetType(), GetType().FullName); }

            var administrator = serviceProvider.EPAdministrator;

            var timeZone = "US Eastern Standard TimeInMillis";

            var counter = new Mutable<int>(0);

            // Create a crontab that fires in one minute
            var crontab = string.Format("*, *, *, *, *, */10, '{0}'", timeZone);

            var statement = administrator.CreatePattern(
                "@Name('MarketMakerNonContinuousAlert:Update') every timer:at(" + crontab + ")");
            statement.Events += (sender, e) => counter.Value++;

            // now go to sleep for the next 1 minute
            SleepForSpan(TimeSpan.FromMinutes(1));

            Assert.That(counter.Value, Is.EqualTo(6));
        }

        private static void SleepForSpan(TimeSpan timeSpan)
        {
            var timeCur = DateTime.Now;
            var timeEnd = timeCur + timeSpan;

            while (timeCur < timeEnd)
            {
                Thread.Sleep(timeEnd - timeCur);
                timeCur = DateTime.Now;
            }
        }
    }
}