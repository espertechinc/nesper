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
	public class TestViewGroupWinReclaimMicrosecondResolution  {
	    private IDictionary<TimeUnit, EPServiceProvider> epServices;

        [SetUp]
	    public void SetUp() {
	        epServices = SupportEngineFactory.SetupEnginesByTimeUnit();
	    }

        [TearDown]
	    public void TearDown() {
	        epServices = null;
	    }

        [Test]
	    public void TestReclaim() {
	        foreach (EPServiceProvider epService in epServices.Values) {
	            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
	        }

	        RunAssertionEventTime(epServices.Get(TimeUnit.MILLISECONDS), 5000);
	        RunAssertionEventTime(epServices.Get(TimeUnit.MICROSECONDS), 5000000);
	    }

	    private static void RunAssertionEventTime(EPServiceProvider epService, long flipTime) {

	        EPServiceProviderIsolated isolated = epService.GetEPServiceIsolated("isolated");
	        isolated.EPRuntime.SendEvent(new CurrentTimeEvent(0));

	        string epl = "@Hint('reclaim_group_aged=1,reclaim_group_freq=5') select * from SupportBean#groupwin(theString)#keepall";
	        EPStatement stmt = isolated.EPAdministrator.CreateEPL(epl, "s0", null);

	        isolated.EPRuntime.SendEvent(new SupportBean("E1", 0));
	        AssertCount(stmt, 1);

	        isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime - 1));
	        isolated.EPRuntime.SendEvent(new SupportBean("E2", 0));
	        AssertCount(stmt, 2);

	        isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime));
	        isolated.EPRuntime.SendEvent(new SupportBean("E3", 0));
	        AssertCount(stmt, 2);

	        isolated.Dispose();
	    }

	    private static void AssertCount(EPStatement stmt, long count) {
	        Assert.AreEqual(count, EPAssertionUtil.EnumeratorCount(stmt.GetEnumerator()));
	    }
	}
} // end of namespace
