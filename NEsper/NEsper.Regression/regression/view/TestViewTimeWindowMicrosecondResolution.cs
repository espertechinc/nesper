///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestViewTimeWindowMicrosecondResolution
    {
	    private IDictionary<TimeUnit, EPServiceProvider> _epServices;

        [SetUp]
	    public void SetUp() {
	        _epServices = SupportEngineFactory.SetupEnginesByTimeUnit();
	    }

        [TearDown]
	    public void TearDown() {
	        _epServices = null;
	    }

        [Test]
	    public void TestMicrosecond() {
	        foreach (EPServiceProvider epService in _epServices.Values) {
	            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
	        }
	        EPServiceProvider engineMillis = _epServices.Get(TimeUnit.MILLISECONDS);
	        EPServiceProvider engineMicros = _epServices.Get(TimeUnit.MICROSECONDS);

            RunAssertionTimeWindow(engineMillis, 0, "1", 1000);
	        RunAssertionTimeWindow(engineMicros, 0, "1", 1000000);
	        RunAssertionTimeWindow(engineMicros, 0, "10 milliseconds", 10000);
	        RunAssertionTimeWindow(engineMicros, 0, "10 microseconds", 10);
	        RunAssertionTimeWindow(engineMicros, 0, "1 seconds 10 microseconds", 1000010);

	        RunAssertionTimeWindow(engineMillis, 123456789, "10", 123456789 + 10 * 1000);
	        RunAssertionTimeWindow(engineMicros, 123456789, "10", 123456789 + 10 * 1000000);

	        RunAssertionTimeWindow(engineMillis, 0, "1 months 10 milliseconds", TimePlusMonth(0, 1) + 10);
	        RunAssertionTimeWindow(engineMicros, 0, "1 months 10 microseconds", TimePlusMonth(0, 1) * 1000 + 10);

            long currentTime = DateTimeHelper.CurrentTimeMillis;
	        RunAssertionTimeWindow(engineMillis, currentTime, "1 months 50 milliseconds", TimePlusMonth(currentTime, 1) + 50);
	        RunAssertionTimeWindow(engineMicros, currentTime*1000+33, "3 months 100 microseconds", TimePlusMonth(currentTime, 3)*1000 + 33 + 100);
	    }

	    private void RunAssertionTimeWindow(EPServiceProvider epService, long startTime, string size, long flipTime) {
	        EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("isolated");
	        isolated.EPRuntime.SendEvent(new CurrentTimeEvent(startTime));

	        string[] fields = "theString".SplitCsv();
	        EPStatement stmt = isolated.EPAdministrator.CreateEPL("select * from SupportBean#time(" + size + ")", "s0", 0);

	        isolated.EPRuntime.SendEvent(new SupportBean("E1", 1));

	        isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime - 1));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, new object[][] { new object[] {"E1"}});

	        isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime));
	        EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), fields, null);

	        isolated.Dispose();
	    }

	    private static long TimePlusMonth(long timeInMillis, int monthToAdd) {
	        DateTimeEx dtx = DateTimeEx.GetInstance(TimeZoneInfo.Local, timeInMillis);
	        dtx.AddMonths(monthToAdd, DateTimeMathStyle.Java);
	        return dtx.TimeInMillis;
	    }
	}
} // end of namespace
