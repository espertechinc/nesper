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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestMinMaxCases 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;
	    private readonly Random _random = new Random();

        [SetUp]
	    public void SetUp()
	    {
	        _listener = new SupportUpdateListener();
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _listener = null;
	    }

        [Test]
	    public void TestMinMaxNamedWindowWEver()
        {
	        RunAssertionMinMaxNamedWindowWEver(false);
	        RunAssertionMinMaxNamedWindowWEver(true);
	    }

	    public void RunAssertionMinMaxNamedWindowWEver(bool soda)
        {
	        var fields = "lower,upper,lowerever,upperever".Split(',');
	        SupportModelHelper.CreateByCompileOrParse(_epService, soda, "create window NamedWindow5m.win:length(2) as select * from SupportBean");
	        SupportModelHelper.CreateByCompileOrParse(_epService, soda, "insert into NamedWindow5m select * from SupportBean");
	        var stmt = SupportModelHelper.CreateByCompileOrParse(_epService, soda, "select " +
	                "min(IntPrimitive) as lower, " +
	                "max(IntPrimitive) as upper, " +
	                "minever(IntPrimitive) as lowerever, " +
	                "maxever(IntPrimitive) as upperever from NamedWindow5m");
	        stmt.AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean(null, 1));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1, 1, 1, 1});

	        _epService.EPRuntime.SendEvent(new SupportBean(null, 5));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{1, 5, 1, 5});

	        _epService.EPRuntime.SendEvent(new SupportBean(null, 3));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{3, 5, 1, 5});

	        _epService.EPRuntime.SendEvent(new SupportBean(null, 6));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{3, 6, 1, 6});

	        _epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestMinMaxNoDataWindowSubquery()
        {
	        var fields = "maxi,mini,max0,min0".Split(',');
	        var epl = "select max(IntPrimitive) as maxi, min(IntPrimitive) as mini," +
	                     "(select max(id) from S0.std:lastevent()) as max0, (select min(id) from S0.std:lastevent()) as min0" +
	                     " from SupportBean";
	        _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

	        _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{3, 3, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{4, 3, null, null});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
	        _epService.EPRuntime.SendEvent(new SupportBean("E3", 4));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{4, 3, 2, 2});

	        _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        _epService.EPRuntime.SendEvent(new SupportBean("E4", 5));
	        EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{5, 3, 1, 1});
	    }

        [Test]
	    public void TestMemoryMinHaving()
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();} // not instrumented

	        var statementText = "select Price, min(Price) as minPrice " +
	                "from " + typeof(SupportMarketDataBean).FullName + ".win:time(30)" +
	                "having Price >= min(Price) * (1.02)";

	        var testView = _epService.EPAdministrator.CreateEPL(statementText);
	        testView.AddListener(_listener);

	        SendClockingInternal();

	        //sendClockingExternal();
	    }

	    private void SendClockingInternal()
	    {
	        // Change to perform a long-running tests, each loop is 1 second
	        var LOOP_COUNT = 2;
	        var loopCount = 0;

	        while(true)
	        {
	            Log.Info("Sending batch " + loopCount);

	            // send events
	            long startTime = DateTimeHelper.CurrentTimeMillis;
	            for (var i = 0; i < 5000; i++)
	            {
	                var price = 50 + 49 * _random.Next(0, 100) / 100.0;
	                SendEvent(price);
	            }
                long endTime = DateTimeHelper.CurrentTimeMillis;

	            // sleep remainder of 1 second
	            var delta = startTime - endTime;
	            if (delta < 950)
	            {
	                Thread.Sleep((int) (950 - delta));
	            }

	            _listener.Reset();
	            loopCount++;
	            if (loopCount > LOOP_COUNT)
	            {
	                break;
	            }
	        }
	    }

	    private void SendEvent(double price)
	    {
	        var bean = new SupportMarketDataBean("DELL", price, -1L, null);
	        _epService.EPRuntime.SendEvent(bean);
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
