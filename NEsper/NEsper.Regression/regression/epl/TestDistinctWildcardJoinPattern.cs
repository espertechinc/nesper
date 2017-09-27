///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
	public class TestDistinctWildcardJoinPattern 
	{
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    private EPServiceProvider _epService;
	    private SupportSubscriberMRD _subscriber;

        [SetUp]
	    public void SetUp()
	    {
	        _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
	        _epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), this.GetType().FullName);}
	        _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
	        _subscriber = new SupportSubscriberMRD();
	    }

        [TearDown]
	    public void TearDown() {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        _subscriber = null;
	    }

        [Test]
	    public void TestWildcardJoinPattern()
        {
	        var epl = "select distinct * from " +
	                "SupportBean(IntPrimitive=0) as fooB unidirectional " +
	                "inner join " +
	                "pattern [" +
	                "every-distinct(fooA.TheString) fooA=SupportBean(IntPrimitive=1)" +
	                "->" +
	                "every-distinct(wooA.TheString) wooA=SupportBean(IntPrimitive=2)" +
	                " where timer:within(1 hour)" +
	                "]#time(1 hour) as fooWooPair " +
	                "on fooB.LongPrimitive = fooWooPair.fooA.LongPrimitive" +
	                " order by fooWooPair.wooA.TheString asc";

	        var stmt = _epService.EPAdministrator.CreateEPL(epl);
	        stmt.Subscriber = _subscriber;

	        Log.Info("Sending event (fooA) starting pattern subexpression...");
	        SendEvent("E1", 1, 10L);

	        Log.Info("Sending event (wooA 1) matching pattern subexpression...");
	        SendEvent("E2", 2, 10L);

	        Log.Info("Sending event (wooA 2) matching pattern subexpression...");
	        SendEvent("E3", 2, 10L);

	        Log.Info("Sending event (fooB) causing join with matched patterns...");
	        SendEvent("Query", 0, 10L);

	        Assert.IsTrue(_subscriber.IsInvoked());
	        Assert.AreEqual(1, _subscriber.InsertStreamList.Count);
	        var inserted = _subscriber.InsertStreamList[0];
	        Assert.AreEqual(2, inserted.Length);
	        Assert.AreEqual("Query", ((SupportBean)inserted[0][0]).TheString);
	        Assert.AreEqual("Query", ((SupportBean)inserted[1][0]).TheString);
	        var mapOne = (IDictionary<string, object>) inserted[0][1];
	        Assert.AreEqual("E2", ((EventBean)mapOne.Get("wooA")).Get("TheString"));
	        Assert.AreEqual("E1", ((EventBean)mapOne.Get("fooA")).Get("TheString"));
	        var mapTwo = (IDictionary<string, object>) inserted[1][1];
	        Assert.AreEqual("E3", ((EventBean)mapTwo.Get("wooA")).Get("TheString"));
	        Assert.AreEqual("E1", ((EventBean)mapTwo.Get("fooA")).Get("TheString"));
	    }

	    private void SendEvent(string theString, int intPrimitive, long longPrimitive)
        {
	        var bean = new SupportBean(theString, intPrimitive);
	        bean.LongPrimitive = longPrimitive;
	        _epService.EPRuntime.SendEvent(bean);
	    }
	}
} // end of namespace
