///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.view;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestOutputLimitChangeSetOpt 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType<SupportBean>();
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
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
	    public void TestInvalid() {
	        SupportMessageAssertUtil.TryInvalid(_epService,
	                "@Hint('enable_outputlimit_opt') select sum(IntPrimitive) " +
	                        "from SupportBean output last every 4 events order by TheString",
	                "Error starting statement: Error in the output rate limiting clause: The ENABLE_OUTPUTLIMIT_OPT hint is not supported with order-by");
	    }

        [Test]
	    public void TestCases() {
	        var currentTime = new AtomicLong(0);
	        SendTime(currentTime.Get());

	        // unaggregated and ungrouped
	        //
	        RunAssertion(currentTime, 0, false, "IntPrimitive", null, null, "last", null);
	        RunAssertion(currentTime, 0, false, "IntPrimitive", null, null, "last", "order by IntPrimitive");

	        RunAssertion(currentTime, 5, false, "IntPrimitive", null, null, "all", null);
	        RunAssertion(currentTime, 0, true, "IntPrimitive", null, null, "all", null);

	        RunAssertion(currentTime, 0, false, "IntPrimitive", null, null, "first", null);

	        // fully-aggregated and ungrouped
	        RunAssertion(currentTime, 5, false, "count(*)", null, null, "last", null);
	        RunAssertion(currentTime, 0, true, "count(*)", null, null, "last", null);

	        RunAssertion(currentTime, 5, false, "count(*)", null, null, "all", null);
	        RunAssertion(currentTime, 0, true, "count(*)", null, null, "all", null);

	        RunAssertion(currentTime, 0, false, "count(*)", null, null, "first", null);
	        RunAssertion(currentTime, 0, false, "count(*)", null, "having count(*) > 0", "first", null);

	        // aggregated and ungrouped
	        RunAssertion(currentTime, 5, false, "TheString, count(*)", null, null, "last", null);
	        RunAssertion(currentTime, 0, true, "TheString, count(*)", null, null, "last", null);

	        RunAssertion(currentTime, 5, false, "TheString, count(*)", null, null, "all", null);
	        RunAssertion(currentTime, 0, true, "TheString, count(*)", null, null, "all", null);

	        RunAssertion(currentTime, 0, true, "TheString, count(*)", null, null, "first", null);
	        RunAssertion(currentTime, 0, true, "TheString, count(*)", null, "having count(*) > 0", "first", null);

	        // fully-aggregated and grouped
	        RunAssertion(currentTime, 5, false, "TheString, count(*)", "group by TheString", null, "last", null);
	        RunAssertion(currentTime, 0, true, "TheString, count(*)", "group by TheString", null, "last", null);

	        RunAssertion(currentTime, 5, false, "TheString, count(*)", "group by TheString", null, "all", null);
	        RunAssertion(currentTime, 0, true, "TheString, count(*)", "group by TheString", null, "all", null);

	        RunAssertion(currentTime, 0, false, "TheString, count(*)", "group by TheString", null, "first", null);

	        // aggregated and grouped
	        RunAssertion(currentTime, 5, false, "TheString, IntPrimitive, count(*)", "group by TheString", null, "last", null);
	        RunAssertion(currentTime, 0, true, "TheString, IntPrimitive, count(*)", "group by TheString", null, "last", null);

	        RunAssertion(currentTime, 5, false, "TheString, IntPrimitive, count(*)", "group by TheString", null, "all", null);

	        RunAssertion(currentTime, 0, false, "TheString, IntPrimitive, count(*)", "group by TheString", null, "first", null);
	    }

        private void RunAssertion(
            AtomicLong currentTime,
            int expected,
            bool withHint,
            string selectClause,
            string groupBy,
            string having,
            string outputKeyword,
            string orderBy)
        {
	        var epl = string.Format("{0}select irstream {1} " + "from SupportBean#length(2) {2}{3}output {4} every 1 seconds {5}", (withHint ? "@Hint('enable_outputlimit_opt') " : ""), selectClause, (groupBy == null ? "" : groupBy + " "), (having == null ? "" : having + " "), outputKeyword, (orderBy ?? ""));
	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        for (var i = 0; i < 5; i++) {
	            _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
	        }

	        AssertResourcesOutputRate(stmt, expected);

	        SendTime(currentTime.IncrementAndGet(1000));

	        AssertResourcesOutputRate(stmt, 0);
	        stmt.Dispose();
	        _listener.Reset();
	    }

	    private void AssertResourcesOutputRate(EPStatement stmt, int numExpectedChangeset) {
	        var spi = (EPStatementSPI) stmt;
	        var resources = spi.StatementContext.StatementExtensionServicesContext.StmtResources.ResourcesUnpartitioned;
	        var outputProcessViewBase = (OutputProcessViewBase) resources.EventStreamViewables[0].Views[0].Views[0];
	        try {
	            Assert.AreEqual(numExpectedChangeset, outputProcessViewBase.NumChangesetRows);
	        }
	        catch (UnsupportedOperationException) {
	            // allowed
	        }
	    }

	    private void SendTime(long currentTime) {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime));
	    }
	}
} // end of namespace
