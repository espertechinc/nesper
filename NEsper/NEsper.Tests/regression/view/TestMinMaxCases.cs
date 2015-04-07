///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestMinMaxCases 
	{
	    private EPServiceProvider epService;
	    private SupportUpdateListener listener;
	    private Random random = new Random();

        [SetUp]
	    public void SetUp()
	    {
	        listener = new SupportUpdateListener();
	        epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName);}
	        epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
	        epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        listener = null;
	    }

        [Test]
	    public void TestMinMaxNamedWindowWEver() {
	        RunAssertionMinMaxNamedWindowWEver(false);
	        RunAssertionMinMaxNamedWindowWEver(true);
	    }

	    public void RunAssertionMinMaxNamedWindowWEver(bool soda) {
	        string[] fields = "lower,upper,lowerever,upperever".Split(',');
	        SupportModelHelper.CreateByCompileOrParse(epService, soda, "create window NamedWindow5m.win:length(2) as select * from SupportBean");
	        SupportModelHelper.CreateByCompileOrParse(epService, soda, "insert into NamedWindow5m select * from SupportBean");
	        EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, "select " +
	                "min(IntPrimitive) as lower, " +
	                "max(IntPrimitive) as upper, " +
	                "minever(IntPrimitive) as lowerever, " +
	                "maxever(IntPrimitive) as upperever from NamedWindow5m");
	        stmt.AddListener(listener);

	        epService.EPRuntime.SendEvent(new SupportBean(null, 1));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, 1, 1, 1});

	        epService.EPRuntime.SendEvent(new SupportBean(null, 5));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, 5, 1, 5});

	        epService.EPRuntime.SendEvent(new SupportBean(null, 3));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, 5, 1, 5});

	        epService.EPRuntime.SendEvent(new SupportBean(null, 6));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, 6, 1, 6});

	        epService.EPAdministrator.DestroyAllStatements();
	    }

        [Test]
	    public void TestMinMaxNoDataWindowSubquery() {

	        string[] fields = "maxi,mini,max0,min0".Split(',');
	        string epl = "select max(IntPrimitive) as maxi, min(IntPrimitive) as mini," +
	                     "(select max(id) from S0.std:lastevent()) as max0, (select min(id) from S0.std:lastevent()) as min0" +
	                     " from SupportBean";
	        epService.EPAdministrator.CreateEPL(epl).AddListener(listener);

	        epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, 3, null, null});

	        epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4, 3, null, null});

	        epService.EPRuntime.SendEvent(new SupportBean_S0(2));
	        epService.EPRuntime.SendEvent(new SupportBean("E3", 4));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4, 3, 2, 2});

	        epService.EPRuntime.SendEvent(new SupportBean_S0(1));
	        epService.EPRuntime.SendEvent(new SupportBean("E4", 5));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5, 3, 1, 1});

	        // Comment out here for sending many more events.
	        // epService.EPRuntime.SendEvent(new SupportBean(null, i));
	        // if (i % 10000 == 0) {
	        //     Console.WriteLine("Sent " + i + " events");
	        // }
	    }

        [Test]
	    public void TestMemoryMinHaving()
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();} // not instrumented

	        string statementText = "select price, min(price) as minPrice " +
	                "from " + typeof(SupportMarketDataBean).FullName + ".win:time(30)" +
	                "having price >= min(price) * (1.02)";

	        EPStatement testView = epService.EPAdministrator.CreateEPL(statementText);
	        testView.AddListener(listener);

	        SendClockingInternal();

	        //SendClockingExternal();
	    }

	    private void SendClockingInternal()
	    {
	        // Change to perform a long-running tests, each loop is 1 second
	        int LOOP_COUNT = 2;
	        int loopCount = 0;

	        while(true)
	        {
	            log.Info("Sending batch " + loopCount);

	            // send events
	            var delta = PerformanceObserver.TimeMillis(() =>
	            {
	                for (int i = 0; i < 5000; i++)
	                {
	                    double price = 50 + 49 * random.Next(100) / 100.0;
	                    SendEvent(price);
	                }
                });

	            // sleep remainder of 1 second
	            if (delta < 950)
	            {
	                Thread.Sleep(950 - (int) delta);
	            }

	            listener.Reset();
	            loopCount++;
	            if (loopCount > LOOP_COUNT)
	            {
	                break;
	            }
	        }
	    }

	    private void SendEvent(double price)
	    {
	        SupportMarketDataBean bean = new SupportMarketDataBean("DELL", price, -1L, null);
	        epService.EPRuntime.SendEvent(bean);
	    }

        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace
