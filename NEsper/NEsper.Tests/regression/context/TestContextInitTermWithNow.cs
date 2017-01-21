///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
	public class TestContextInitTermWithNow
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var configuration = SupportConfigFactory.GetConfiguration();
	        configuration.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}

	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestNonOverlapping()
        {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        var contextExpr =  "create context MyContext " +
	                "as start @now end after 10 seconds";
	        _epService.EPAdministrator.CreateEPL(contextExpr);

	        var fields = new string[] {"cnt"};
	        var streamExpr = "context MyContext " +
	                "select count(*) as cnt from SupportBean output last when terminated";
	        var stream = _epService.EPAdministrator.CreateEPL(streamExpr);
	        stream.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(8000));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {3L});

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(19999));
	        Assert.IsFalse(_listener.IsInvoked);
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(20000));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1L});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(30000));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {0L});

	        SupportModelHelper.CompileCreate(_epService, streamExpr);

	        _epService.EPAdministrator.DestroyAllStatements();

	        SupportModelHelper.CompileCreate(_epService, contextExpr);
	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestOverlappingWithPattern()
        {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	        var contextExpr =  "create context MyContext " +
	                "initiated by @Now and pattern [every timer:interval(10)] terminated after 10 sec";
	        _epService.EPAdministrator.CreateEPL(contextExpr);

	        var fields = new string[] {"cnt"};
	        var streamExpr = "context MyContext " +
	                "select count(*) as cnt from SupportBean output last when terminated";
	        var stream = _epService.EPAdministrator.CreateEPL(streamExpr);
	        stream.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(8000));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(9999));
	        Assert.IsFalse(_listener.IsInvoked);
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {3L});

	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10100));
	        _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(19999));
	        Assert.IsFalse(_listener.IsInvoked);

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(20000));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {2L});

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(30000));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {0L});

	        _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(40000));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1L});

	        SupportModelHelper.CompileCreate(_epService, streamExpr);

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestInvalid() {
	        // for overlapping contexts, @now without condition is not allowed
	        TryInvalid("create context TimedImmediate initiated @now terminated after 10 seconds",
	                "Incorrect syntax near 'terminated' (a reserved keyword) expecting 'and' but found 'terminated' at line 1 column 45 [create context TimedImmediate initiated @now terminated after 10 seconds]");

	        // for non-overlapping contexts, @now with condition is not allowed
	        TryInvalid("create context TimedImmediate start @now and after 5 seconds end after 10 seconds",
	                "Incorrect syntax near 'and' (a reserved keyword) expecting 'end' but found 'and' at line 1 column 41 [create context TimedImmediate start @now and after 5 seconds end after 10 seconds]");

	        // for overlapping contexts, @now together with a filter condition is not allowed
	        TryInvalid("create context TimedImmediate initiated @now and SupportBean terminated after 10 seconds",
	                "Invalid use of 'now' with initiated-by stream, this combination is not supported [create context TimedImmediate initiated @now and SupportBean terminated after 10 seconds]");
	    }

	    private void TryInvalid(string epl, string expected) {
	        try {
	            _epService.EPAdministrator.CreateEPL(epl);
	            Assert.Fail();
	        }
	        catch (EPStatementException ex) {
	            Assert.AreEqual(expected, ex.Message);
	        }
	    }

	}
} // end of namespace
