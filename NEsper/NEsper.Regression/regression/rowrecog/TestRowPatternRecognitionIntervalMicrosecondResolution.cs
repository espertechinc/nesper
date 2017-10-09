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

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
	public class TestRowPatternRecognitionIntervalMicrosecondResolution
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

	        RunAssertionWithTime(_epServices.Get(TimeUnit.MILLISECONDS), 0, 10000);
	        RunAssertionWithTime(_epServices.Get(TimeUnit.MICROSECONDS), 0, 10000000);
	    }

	    private void RunAssertionWithTime(EPServiceProvider epService, long startTime, long flipTime) {
	        var isolated = epService.GetEPServiceIsolated("isolated");
	        isolated.EPRuntime.SendEvent(new CurrentTimeEvent(startTime));

	        var text = "select * from SupportBean " +
	                      "match_recognize (" +
	                      " measures A as a" +
	                      " pattern (A*)" +
	                      " interval 10 seconds" +
	                      ")";

	        var stmt = isolated.EPAdministrator.CreateEPL(text, "s0", null);
	        var listener = new SupportUpdateListener();
	        stmt.AddListener(listener);

	        isolated.EPRuntime.SendEvent(new SupportBean("E1", 1));

	        isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime - 1));
            Assert.IsFalse(listener.IsInvokedAndReset());

	        isolated.EPRuntime.SendEvent(new CurrentTimeEvent(flipTime));
	        Assert.IsTrue(listener.IsInvokedAndReset());

	        isolated.Dispose();
	    }
	}
} // end of namespace
