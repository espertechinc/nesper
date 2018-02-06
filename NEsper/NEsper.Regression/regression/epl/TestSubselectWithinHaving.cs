///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestSubselectWithinHaving
    {
	    private EPServiceProvider epService;
	    private SupportUpdateListener listener;

        [SetUp]
	    public void SetUp()
        {
	        Configuration config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("SupportBean", typeof(SupportBean));
	        config.AddEventType("S0", typeof(SupportBean_S0));
	        config.AddEventType("S1", typeof(SupportBean_S1));
	        epService = EPServiceProviderManager.GetDefaultProvider(config);
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.StartTest(epService, this.GetType(), this.GetType().FullName);
	        }
	        listener = new SupportUpdateListener();
	    }

        [TearDown]
	    public void TearDown()
        {
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.EndTest();
	        }
	        listener = null;
	    }

        [Test]
	    public void TestHavingSubselectWithGroupBy()
        {
	        epService.EPAdministrator.Configuration.AddEventType(typeof(MaxAmountEvent));
	        RunAssertionHavingSubselectWithGroupBy(true);
	        RunAssertionHavingSubselectWithGroupBy(false);
	    }

	    private void RunAssertionHavingSubselectWithGroupBy(bool namedWindow)
        {
	        string eplCreate = namedWindow ?
	                           "create window MyInfra#unique(key) as MaxAmountEvent" :
	                           "create table MyInfra(key string primary key, maxAmount double)";
	        epService.EPAdministrator.CreateEPL(eplCreate);
	        epService.EPAdministrator.CreateEPL("insert into MyInfra select * from MaxAmountEvent");

	        string stmtText = "select theString as c0, sum(intPrimitive) as c1 " +
	                          "from SupportBean#groupwin(theString)#length(2) as sb " +
	                          "group by theString " +
	                          "having sum(intPrimitive) > (select maxAmount from MyInfra as mw where sb.theString = mw.key)";
	        EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
	        stmt.AddListener(listener);
	        string[] fields = "c0,c1".SplitCsv();

	        // set some amounts
	        epService.EPRuntime.SendEvent(new MaxAmountEvent("G1", 10));
	        epService.EPRuntime.SendEvent(new MaxAmountEvent("G2", 20));
	        epService.EPRuntime.SendEvent(new MaxAmountEvent("G3", 30));

	        // send some events
	        epService.EPRuntime.SendEvent(new SupportBean("G1", 5));
	        epService.EPRuntime.SendEvent(new SupportBean("G2", 19));
	        epService.EPRuntime.SendEvent(new SupportBean("G3", 28));
	        Assert.IsFalse(listener.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"G2", 21});

	        epService.EPRuntime.SendEvent(new SupportBean("G2", 18));
	        epService.EPRuntime.SendEvent(new SupportBean("G1", 4));
	        epService.EPRuntime.SendEvent(new SupportBean("G3", 2));
	        Assert.IsFalse(listener.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean("G3", 29));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"G3", 31});

	        epService.EPRuntime.SendEvent(new SupportBean("G3", 4));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"G3", 33});

	        epService.EPRuntime.SendEvent(new SupportBean("G1", 6));
	        epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
	        epService.EPRuntime.SendEvent(new SupportBean("G3", 26));
	        Assert.IsFalse(listener.IsInvoked);

	        epService.EPRuntime.SendEvent(new SupportBean("G1", 99));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"G1", 105});

	        epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
	        EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"G1", 100});

	        epService.EPAdministrator.DestroyAllStatements();
	        epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
	    }

        internal class MaxAmountEvent
        {
            internal MaxAmountEvent(string key, double maxAmount)
            {
                this.Key = key;
                this.MaxAmount = maxAmount;
            }

            [PropertyName("key")]
            public string Key { get; private set; }

            [PropertyName("maxAmount")]
            public double MaxAmount { get; private set; }
        }
	}
} // end of namespace
