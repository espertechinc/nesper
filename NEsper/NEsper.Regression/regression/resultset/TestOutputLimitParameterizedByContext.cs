///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOutputLimitParameterizedByContext
    {
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp() {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);
	        }
	        _listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        _listener = null;
	    }

        [Test]
	    public void TestCrontabFromContext() {
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(MySimpleScheduleEvent));
	        _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec("2002-05-01T09:00:00.000")));
	        _epService.EPAdministrator.CreateEPL("create context MyCtx start MySimpleScheduleEvent as sse");
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL("context MyCtx\n" +
	                           "select count(*) as c \n" +
	                           "from SupportBean_S0\n" +
	                           "output last at(context.sse.atminute, context.sse.athour, *, *, *, *) and when terminated\n");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new MySimpleScheduleEvent(10, 15));
	        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec("2002-05-01T10:14:59.000")));
	        Assert.IsFalse(_listener.GetAndClearIsInvoked());

	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec("2002-05-01T10:15:00.000")));
	        Assert.IsTrue(_listener.GetAndClearIsInvoked());
	    }

	    public class MySimpleScheduleEvent
        {
	        public MySimpleScheduleEvent(int athour, int atminute)
            {
	            Athour = athour;
	            Atminute = atminute;
	        }

	        public int Athour { get; private set; }

	        public int Atminute { get; private set; }
	    }
	}
} // end of namespace
