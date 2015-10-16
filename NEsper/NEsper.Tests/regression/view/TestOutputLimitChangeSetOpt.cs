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
using com.espertech.esper.core.service;
using com.espertech.esper.core.service.resource;
using com.espertech.esper.epl.view;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestOutputLimitChangeSetOpt 
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;

        [SetUp]
	    public void SetUp()
	    {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("SupportBean", typeof(SupportBean));
	        _epService = EPServiceProviderManager.GetDefaultProvider(config);
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName); }
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
	                "@Hint('enable_outputlimit_opt') select sum(intPrimitive) " +
	                        "from SupportBean output last every 4 events order by theString",
	                "Error starting statement: Error in the output rate limiting clause: The ENABLE_OUTPUTLIMIT_OPT hint is not supported with order-by");
	    }

        [Test]
	    public void TestCases() {
	        AtomicLong currentTime = new AtomicLong(0);
	        SendTime(currentTime.Get());

	        // unaggregated and ungrouped
	        //
	        RunAssertion(currentTime, 0, false, "intPrimitive", null, null, "last", null);
	        RunAssertion(currentTime, 0, false, "intPrimitive", null, null, "last", "order by intPrimitive");

	        RunAssertion(currentTime, 5, false, "intPrimitive", null, null, "all", null);
	        RunAssertion(currentTime, 0, true, "intPrimitive", null, null, "all", null);

	        RunAssertion(currentTime, 0, false, "intPrimitive", null, null, "first", null);

	        // fully-aggregated and ungrouped
	        RunAssertion(currentTime, 5, false, "count(*)", null, null, "last", null);
	        RunAssertion(currentTime, 0, true, "count(*)", null, null, "last", null);

	        RunAssertion(currentTime, 5, false, "count(*)", null, null, "all", null);
	        RunAssertion(currentTime, 0, true, "count(*)", null, null, "all", null);

	        RunAssertion(currentTime, 0, false, "count(*)", null, null, "first", null);
	        RunAssertion(currentTime, 0, false, "count(*)", null, "having count(*) > 0", "first", null);

	        // aggregated and ungrouped
	        RunAssertion(currentTime, 5, false, "theString, count(*)", null, null, "last", null);
	        RunAssertion(currentTime, 0, true, "theString, count(*)", null, null, "last", null);

	        RunAssertion(currentTime, 5, false, "theString, count(*)", null, null, "all", null);
	        RunAssertion(currentTime, 0, true, "theString, count(*)", null, null, "all", null);

	        RunAssertion(currentTime, 0, true, "theString, count(*)", null, null, "first", null);
	        RunAssertion(currentTime, 0, true, "theString, count(*)", null, "having count(*) > 0", "first", null);

	        // fully-aggregated and grouped
	        RunAssertion(currentTime, 5, false, "theString, count(*)", "group by theString", null, "last", null);
	        RunAssertion(currentTime, 0, true, "theString, count(*)", "group by theString", null, "last", null);

	        RunAssertion(currentTime, 5, false, "theString, count(*)", "group by theString", null, "all", null);
	        RunAssertion(currentTime, 0, true, "theString, count(*)", "group by theString", null, "all", null);

	        RunAssertion(currentTime, 0, false, "theString, count(*)", "group by theString", null, "first", null);

	        // aggregated and grouped
	        RunAssertion(currentTime, 5, false, "theString, intPrimitive, count(*)", "group by theString", null, "last", null);
	        RunAssertion(currentTime, 0, true, "theString, intPrimitive, count(*)", "group by theString", null, "last", null);

	        RunAssertion(currentTime, 5, false, "theString, intPrimitive, count(*)", "group by theString", null, "all", null);

	        RunAssertion(currentTime, 0, false, "theString, intPrimitive, count(*)", "group by theString", null, "first", null);
	    }

	    private void RunAssertion(AtomicLong currentTime,
	                              int expected,
	                              bool withHint,
	                              string selectClause,
	                              string groupBy,
	                              string having,
	                              string outputKeyword,
	                              string orderBy) {
	        string epl = (withHint ? "@Hint('enable_outputlimit_opt') " : "") +
	                     "select irstream " + selectClause + " " +
	                     "from SupportBean.win:length(2) " +
	                     (groupBy == null ? "" : groupBy + " ") +
	                     (having == null ? "" : having + " ") +
	                     "output " + outputKeyword + " every 1 seconds " +
	                    (orderBy == null ? "" : orderBy);
	        EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.AddListener(_listener);

	        for (int i = 0; i < 5; i++) {
	            _epService.EPRuntime.SendEvent(new SupportBean("E" + i, i));
	        }

	        AssertResourcesOutputRate(stmt, expected);

            SendTime(currentTime.IncrementAndGet(1000));

	        AssertResourcesOutputRate(stmt, 0);
	        stmt.Dispose();
	        _listener.Reset();
	    }

	    private void AssertResourcesOutputRate(EPStatement stmt, int numExpectedChangeset) {
	        EPStatementSPI spi = (EPStatementSPI) stmt;
	        StatementResourceHolder resources = spi.StatementContext.StatementExtensionServicesContext.StmtResources.ResourcesUnpartitioned;
	        OutputProcessViewBase outputProcessViewBase = (OutputProcessViewBase) resources.EventStreamViewables[0].Views[0].Views[0];
	        Assert.AreEqual(numExpectedChangeset, outputProcessViewBase.NumChangesetRows);
	    }

	    private void SendTime(long currentTime) {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime));
	    }
	}
} // end of namespace
