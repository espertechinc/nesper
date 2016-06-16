///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestCronParameter : SupportBeanConstants
    {
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            _listener = null;
        }
    
        [Test]
        public void TestCronLast() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsShareViews = false;
            config.EngineDefaults.ExecutionConfig.IsAllowIsolatedService = true;
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            //
            // LAST
            //
            // Last day of the month, at 5pm
            RunSequenceIsolated(epService, "2013-08-23 08:05:00.000",
                    "select * from pattern [ every timer:at(0, 17, last, *, *) ]",
                    new String[]{
                            "2013-08-31 17:00:00.000",
                            "2013-09-30 17:00:00.000",
                            "2013-10-31 17:00:00.000",
                            "2013-11-30 17:00:00.000",
                            "2013-12-31 17:00:00.000",
                            "2014-01-31 17:00:00.000",
                            "2014-02-28 17:00:00.000",
                            "2014-03-31 17:00:00.000",
                            "2014-04-30 17:00:00.000",
                            "2014-05-31 17:00:00.000",
                            "2014-06-30 17:00:00.000",
                    });
    
            // Last day of the month, at the earliest
            RunSequenceIsolated(epService, "2013-08-23 08:05:00.000",
                    "select * from pattern [ every timer:at(*, *, last, *, *) ]",
                    new String[]{
                            "2013-08-31 00:00:00.000",
                            "2013-09-30 00:00:00.000",
                            "2013-10-31 00:00:00.000",
                            "2013-11-30 00:00:00.000",
                            "2013-12-31 00:00:00.000",
                            "2014-01-31 00:00:00.000",
                            "2014-02-28 00:00:00.000",
                            "2014-03-31 00:00:00.000",
                            "2014-04-30 00:00:00.000",
                            "2014-05-31 00:00:00.000",
                            "2014-06-30 00:00:00.000",
                    });
    
            // Last Sunday of the month, at 5pm
            RunSequenceIsolated(epService, "2013-08-20 08:00:00.000",
                    "select * from pattern [ every timer:at(0, 17, *, *, 0 last, *) ]",
                    new String[] {
                            "2013-08-25 17:00:00.000",
                            "2013-09-29 17:00:00.000",
                            "2013-10-27 17:00:00.000",
                            "2013-11-24 17:00:00.000",
                            "2013-12-29 17:00:00.000",
                            "2014-01-26 17:00:00.000",
                            "2014-02-23 17:00:00.000",
                            "2014-03-30 17:00:00.000",
                            "2014-04-27 17:00:00.000",
                            "2014-05-25 17:00:00.000",
                            "2014-06-29 17:00:00.000",
                    });
    
            // Last Friday of the month, any time
            // 0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4= Thursday, 5=Friday, 6=Saturday
            RunSequenceIsolated(epService, "2013-08-20 08:00:00.000",
                    "select * from pattern [ every timer:at(*, *, *, *, 5 last, *) ]",
                    new String[] {
                            "2013-08-30 0:00:00.000",
                            "2013-09-27 0:00:00.000",
                            "2013-10-25 0:00:00.000",
                            "2013-11-29 0:00:00.000",
                            "2013-12-27 0:00:00.000",
                            "2014-01-31 0:00:00.000",
                            "2014-02-28 0:00:00.000",
                            "2014-03-28 0:00:00.000",
                    });
    
            // Last day of week (Saturday)
            RunSequenceIsolated(epService, "2013-08-01 08:00:00.000",
                    "select * from pattern [ every timer:at(*, *, *, *, last, *) ]",
                    new String[] {
                            "2013-08-03 00:00:00.000",
                            "2013-08-10 00:00:00.000",
                            "2013-08-17 00:00:00.000",
                            "2013-08-24 00:00:00.000",
                            "2013-08-31 00:00:00.000",
                            "2013-09-07 00:00:00.000",
                    });

            // Last day of month in August
            // For Java: January=0, February=1, March=2, April=3, May=4, June=5,
            //            July=6, August=7, September=8, November=9, October=10, December=11
            // For .NET: January=1, February=2, March=3, April=4, May=5, June=6,
            //            July=7, August=8, September=9, November=10, October=11, December=12
            // For Esper: January=1, February=2, March=3, April=4, May=5, June=6,
            //            July=7, August=8, September=9, November=10, October=11, December=12
            RunSequenceIsolated(epService, "2013-01-01 08:00:00.000",
                    "select * from pattern [ every timer:at(*, *, last, 8, *, *) ]",
                    new String[] {
                            "2013-08-31 00:00:00.000",
                            "2014-08-31 00:00:00.000",
                            "2015-08-31 00:00:00.000",
                            "2016-08-31 00:00:00.000",
                    });
    
            // Last day of month in Feb. (test leap year)
            RunSequenceIsolated(epService, "2007-01-01 08:00:00.000",
                    "select * from pattern [ every timer:at(*, *, last, 2, *, *) ]",
                    new String[] {
                            "2007-02-28 00:00:00.000",
                            "2008-02-29 00:00:00.000",
                            "2009-02-28 00:00:00.000",
                            "2010-02-28 00:00:00.000",
                            "2011-02-28 00:00:00.000",
                            "2012-02-29 00:00:00.000",
                            "2013-02-28 00:00:00.000",
                    });
    
            // Observer for last Friday of each June (month 6)
            RunSequenceIsolated(epService, "2007-01-01 08:00:00.000",
                    "select * from pattern [ every timer:at(*, *, *, 6, 5 last, *) ]",
                    new String[] {
                            "2007-06-29 00:00:00.000",
                            "2008-06-27 00:00:00.000",
                            "2009-06-26 00:00:00.000",
                            "2010-06-25 00:00:00.000",
                            "2011-06-24 00:00:00.000",
                            "2012-06-29 00:00:00.000",
                            "2013-06-28 00:00:00.000",
                    });
    
            //
            // LASTWEEKDAY
            //

            // Last weekday (last day that is not a weekend day)
            RunSequenceIsolated(epService, "2013-08-23 8:05:00.000",
                    "select * from pattern [ every timer:at(0, 17, lastweekday, *, *) ]",
                    new String[]{
                            "2013-08-30 17:00:00.000",
                            "2013-09-30 17:00:00.000",
                            "2013-10-31 17:00:00.000",
                            "2013-11-29 17:00:00.000",
                            "2013-12-31 17:00:00.000",
                            "2014-01-31 17:00:00.000",
                            "2014-02-28 17:00:00.000",
                            "2014-03-31 17:00:00.000",
                            "2014-04-30 17:00:00.000",
                            "2014-05-30 17:00:00.000",
                            "2014-06-30 17:00:00.000",
                    });
    
            // Last weekday, any time
            RunSequenceIsolated(epService, "2013-08-23 8:05:00.000",
                    "select * from pattern [ every timer:at(*, *, lastweekday, *, *, *) ]",
                    new String[]{
                            "2013-08-30 00:00:00.000",
                            "2013-09-30 00:00:00.000",
                            "2013-10-31 00:00:00.000",
                            "2013-11-29 00:00:00.000",
                            "2013-12-31 00:00:00.000",
                            "2014-01-31 00:00:00.000",
                    });
    
            // Observer for last weekday of September, for 2007 it's Friday September 28th
            RunSequenceIsolated(epService, "2007-08-23 8:05:00.000",
                    "select * from pattern [ every timer:at(*, *, lastweekday, 9, *, *) ]",
                    new String[]{
                            "2007-09-28 00:00:00.000",
                            "2008-09-30 00:00:00.000",
                            "2009-09-30 00:00:00.000",
                            "2010-09-30 00:00:00.000",
                            "2011-09-30 00:00:00.000",
                            "2012-09-28 00:00:00.000",
                    });
    
            // Observer for last weekday of February
            RunSequenceIsolated(epService, "2007-01-23 8:05:00.000",
                    "select * from pattern [ every timer:at(*, *, lastweekday, 2, *, *) ]",
                    new String[]{
                            "2007-02-28 00:00:00.000",
                            "2008-02-29 00:00:00.000",
                            "2009-02-27 00:00:00.000",
                            "2010-02-26 00:00:00.000",
                            "2011-02-28 00:00:00.000",
                            "2012-02-29 00:00:00.000",
                    });

            //
            // WEEKDAY
            //
            RunSequenceIsolated(epService, "2007-01-23 8:05:00.000",
                    "select * from pattern [ every timer:at(*, *, 1 weekday, 9, *, *) ]",
                    new String[]{
                            "2007-09-03 00:00:00.000",
                            "2008-09-01 00:00:00.000",
                            "2009-09-01 00:00:00.000",
                            "2010-09-01 00:00:00.000",
                            "2011-09-01 00:00:00.000",
                            "2012-09-03 00:00:00.000",
                            "2013-09-02 00:00:00.000",
                    });
    
            RunSequenceIsolated(epService, "2007-01-23 8:05:00.000",
                    "select * from pattern [ every timer:at(*, *, 30 weekday, 9, *, *) ]",
                    new String[]{
                            "2007-09-28 00:00:00.000",
                            "2008-09-30 00:00:00.000",
                            "2009-09-30 00:00:00.000",
                            "2010-09-30 00:00:00.000",
                            "2011-09-30 00:00:00.000",
                            "2012-09-28 00:00:00.000",
                            "2013-09-30 00:00:00.000",
                    });
    
            // nearest weekday for current month on the 10th
            RunSequenceIsolated(epService, "2013-01-23 8:05:00.000",
                    "select * from pattern [ every timer:at(*, *, 10 weekday, *, *, *) ]",
                    new String[]{
                            "2013-02-11 00:00:00.000",
                            "2013-03-11 00:00:00.000",
                            "2013-04-10 00:00:00.000",
                            "2013-05-10 00:00:00.000",
                            "2013-06-10 00:00:00.000",
                            "2013-07-10 00:00:00.000",
                            "2013-08-09 00:00:00.000",
                    });

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void RunSequenceIsolated(EPServiceProvider epService, String startTime, String epl, String[] times) {
            EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("i1");
            SendTime(isolated, startTime);
            isolated.EPAdministrator.CreateEPL(epl, "S0", null).Events += _listener.Update;
            RunSequence(isolated, times);
            epService.EPAdministrator.DestroyAllStatements();
            isolated.Dispose();
        }
    
        private void RunSequence(EPServiceProviderIsolated epService, String[] times) {
            foreach (String next in times) {
                // send right-before time
                long nextLong = DateTimeParser.ParseDefaultMSec(next);
                epService.EPRuntime.SendEvent(new CurrentTimeEvent(nextLong - 1001));
                Assert.IsFalse(_listener.IsInvoked, "unexpected callback at " + next);
    
                // send right-after time
                epService.EPRuntime.SendEvent(new CurrentTimeEvent(nextLong + 1000));
                Assert.IsTrue(_listener.GetAndClearIsInvoked(), "missing callback at " + next);
            }
        }
    
        private void SendTime(EPServiceProviderIsolated epService, String time) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }
    }
    
}
