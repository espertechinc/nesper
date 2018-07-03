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

using NUnit.Framework;

namespace NEsper.Examples.Transaction
{
    [TestFixture]
	public class TestRealtimeSummaryStmt : TestStmtBase
	{
	    private SupportUpdateListener listenerTotals;
	    private SupportUpdateListener listenerByCustomer;
	    private SupportUpdateListener listenerBySupplier;

	    [SetUp]
	    public override void SetUp()
	    {
	        base.SetUp();

	        // Establish feed for combined events, which contains the latency values
	        CombinedEventStmt.Create(epService.EPAdministrator);

	        // Establish listeners for testing
	        listenerTotals = new SupportUpdateListener();
	        listenerByCustomer = new SupportUpdateListener();
	        listenerBySupplier = new SupportUpdateListener();
	        RealtimeSummaryStmt realtimeStmt = new RealtimeSummaryStmt(epService.EPAdministrator);
	        realtimeStmt.TotalsStatement.Events += listenerTotals.Update;
	        realtimeStmt.CustomerStatement.Events += listenerByCustomer.Update;
	        realtimeStmt.SupplierStatement.Events += listenerBySupplier.Update;

	        // Use external clocking for the test
	        epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));
	    }

	    [Test]
	    public void TestFlow()
	    {
	        SendEvent(new CurrentTimeEvent(1000)); // Set the time to 1 seconds

	        SendEvent(new TxnEventA("id1", 1000, "c1"));
	        SendEvent(new TxnEventB("id1", 2000));
	        SendEvent(new TxnEventC("id1", 3500, "s1"));
	        AssertTotals(2500L, 2500L, 2500D, 1500L, 1500L, 1500D, 1000L, 1000L, 1000D);
	        AssertTotalsByCustomer("c1", 2500L, 2500L, 2500D);
	        AssertTotalsBySupplier("s1", 2500L, 2500L, 2500D);

	        SendEvent(new CurrentTimeEvent(10000)); // Set the time to 10 seconds

	        SendEvent(new TxnEventB("id2", 10000));
	        SendEvent(new TxnEventC("id2", 10600, "s2"));
	        SendEvent(new TxnEventA("id2", 11200, "c2"));
	        AssertTotals(-600L, 2500L, (2500 - 600)/2.0D, 600L, 1500L, (1500 + 600)/2.0D, -1200L, 1000L, (1000 - 1200)/2.0);
	        AssertTotalsByCustomer("c2", -600L, -600L, -600D);
	        AssertTotalsBySupplier("s2", -600L, -600L, -600D);

	        SendEvent(new CurrentTimeEvent(20000)); // Set the time to 20 seconds

	        SendEvent(new TxnEventC("id3", 20000, "s1"));
	        SendEvent(new TxnEventA("id3", 20100, "c1"));
	        SendEvent(new TxnEventB("id3", 20200));
	        AssertTotals(-600L, 2500L, (2500 - 600 - 100)/3.0D, -200L, 1500L, (1500 + 600 - 200)/3.0, -1200L, 1000L, (1000 - 1200 + 100)/3.0);
	        AssertTotalsByCustomer("c1", -100L, 2500L, (2500 - 100) / 2.0);
	        AssertTotalsBySupplier("s1", -100L, 2500L, (2500 - 100) / 2.0);

	        // Set the time to 30 minutes and 5 seconds later expelling latencies for "id1"
	        int seconds = 30 * 60 + 5;
	        SendEvent(new CurrentTimeEvent(seconds * 1000));
	        AssertTotals(-600L, -100L, (-600-100)/2.0, -200L, 600L, (600 - 200)/2.0, -1200L, 100L, (-1200 + 100)/2.0);
	        AssertTotalsByCustomer("c1", -100L, -100L, -100D);
	        AssertTotalsBySupplier("s1", -100L, -100L, -100D);

	        // Set the time to 30 minutes and 10 seconds later expelling latencies for "id2"
	        seconds = 30 * 60 + 10;
	        SendEvent(new CurrentTimeEvent(seconds * 1000));
	        AssertTotals(-100L, -100L, -100D, -200L, -200L, - 200D, 100L, 100L, 100D);
	        AssertTotalsByCustomer("c2", null, null, null);
	        AssertTotalsBySupplier("s2", null, null, null);

	        // Set the time to 30 minutes and 20 seconds later expelling remaining latencies "id3"
	        seconds = 30 * 60 + 20;
	        SendEvent(new CurrentTimeEvent(seconds * 1000));
	        AssertTotals(null, null, null, null, null, null, null, null, null);
	        AssertTotalsByCustomer("c1", null, null, null);
	        AssertTotalsBySupplier("s1", null, null, null);

	        // Send some more events crossing supplier and customer ids
	        seconds = 30 * 60 + 30;
	        SendEvent(new TxnEventA("id4", seconds * 1000, "cA"));
	        SendEvent(new TxnEventB("id4", seconds * 1000 + 500));
	        SendEvent(new TxnEventC("id4", seconds * 1000 + 1000, "sB"));
	        AssertTotalsByCustomer("cA", 1000L, 1000L, 1000D);
	        AssertTotalsBySupplier("sB", 1000L, 1000L, 1000D);

	        seconds = 30 * 60 + 40;
	        SendEvent(new TxnEventA("id5", seconds * 1000, "cB"));
	        SendEvent(new TxnEventB("id5", seconds * 1000 + 1500));
	        SendEvent(new TxnEventC("id5", seconds * 1000 + 2000, "sA"));
	        AssertTotalsByCustomer("cB", 2000L, 2000L, 2000D);
	        AssertTotalsBySupplier("sA", 2000L, 2000L, 2000D);

	        seconds = 30 * 60 + 50;
	        SendEvent(new TxnEventA("id6", seconds * 1000, "cA"));
	        SendEvent(new TxnEventB("id6", seconds * 1000 + 2500));
	        SendEvent(new TxnEventC("id6", seconds * 1000 + 3000, "sA"));
	        AssertTotalsByCustomer("cA", 1000L, 3000L, 2000D);
	        AssertTotalsBySupplier("sA", 2000L, 3000L, 2500D);
	    }

	    private void AssertTotals(long? minAC, long? maxAC, double? avgAC,
                                  long? minBC, long? maxBC, double? avgBC,
                                  long? minAB, long? maxAB, double? avgAB)
	    {
	        Assert.AreEqual(1, listenerTotals.NewDataList.Count);
	        Assert.AreEqual(1, listenerTotals.LastNewData.Length);
	        EventBean @event = listenerTotals.LastNewData[0];
	        Assert.AreEqual(minAC, @event.Get("minLatencyAC"));
	        Assert.AreEqual(maxAC, @event.Get("maxLatencyAC"));
	        Assert.AreEqual(avgAC, @event.Get("avgLatencyAC"));
	        Assert.AreEqual(minBC, @event.Get("minLatencyBC"));
	        Assert.AreEqual(maxBC, @event.Get("maxLatencyBC"));
	        Assert.AreEqual(avgBC, @event.Get("avgLatencyBC"));
	        Assert.AreEqual(minAB, @event.Get("minLatencyAB"));
	        Assert.AreEqual(maxAB, @event.Get("maxLatencyAB"));
	        Assert.AreEqual(avgAB, @event.Get("avgLatencyAB"));
	        listenerTotals.Reset();
	    }

        private void AssertTotalsByCustomer(String customerId, long? minAC, long? maxAC, double? avgAC)
	    {
            Assert.AreEqual(1, listenerByCustomer.NewDataList.Count);
            Assert.AreEqual(1, listenerByCustomer.LastNewData.Length);
	        EventBean @event = listenerByCustomer.LastNewData[0];
	        Assert.AreEqual(customerId, @event.Get("customerId"));
	        Assert.AreEqual(minAC, @event.Get("minLatency"));
	        Assert.AreEqual(maxAC, @event.Get("maxLatency"));
	        Assert.AreEqual(avgAC, @event.Get("avgLatency"));
	        listenerByCustomer.Reset();
	    }

        private void AssertTotalsBySupplier(String supplierId, long? minAC, long? maxAC, double? avgAC)
	    {
	        Assert.AreEqual(1, listenerBySupplier.NewDataList.Count);
	        Assert.AreEqual(1, listenerBySupplier.LastNewData.Length);
	        EventBean @event = listenerBySupplier.LastNewData[0];
	        Assert.AreEqual(supplierId, @event.Get("supplierId"));
	        Assert.AreEqual(minAC, @event.Get("minLatency"));
	        Assert.AreEqual(maxAC, @event.Get("maxLatency"));
	        Assert.AreEqual(avgAC, @event.Get("avgLatency"));
	        listenerBySupplier.Reset();
	    }
	}
} // End of namespace
