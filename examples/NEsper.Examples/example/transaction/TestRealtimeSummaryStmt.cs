///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;

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
	        CombinedEventStmt.Create(_runtime);

	        // Establish listeners for testing
	        listenerTotals = new SupportUpdateListener();
	        listenerByCustomer = new SupportUpdateListener();
	        listenerBySupplier = new SupportUpdateListener();
	        RealtimeSummaryStmt realtimeStmt = new RealtimeSummaryStmt(_runtime);
	        realtimeStmt.TotalsStatement.Events += listenerTotals.Update;
	        realtimeStmt.CustomerStatement.Events += listenerByCustomer.Update;
	        realtimeStmt.SupplierStatement.Events += listenerBySupplier.Update;

	        // Use external clocking for the test
	        _runtime.EventService.ClockExternal();
	    }

	    [Test]
	    public void TestFlow()
	    {
		    _runtime.EventService.AdvanceTime(1000); // Set the time to 1 seconds

	        SendEvent(new TxnEventA("id1", 1000, "c1"));
	        SendEvent(new TxnEventB("id1", 2000));
	        SendEvent(new TxnEventC("id1", 3500, "s1"));
	        AssertTotals(2500L, 2500L, 2500D, 1500L, 1500L, 1500D, 1000L, 1000L, 1000D);
	        AssertTotalsByCustomer("c1", 2500L, 2500L, 2500D);
	        AssertTotalsBySupplier("s1", 2500L, 2500L, 2500D);

	        _runtime.EventService.AdvanceTime(10000); // Set the time to 10 seconds

	        SendEvent(new TxnEventB("id2", 10000));
	        SendEvent(new TxnEventC("id2", 10600, "s2"));
	        SendEvent(new TxnEventA("id2", 11200, "c2"));
	        AssertTotals(-600L, 2500L, (2500 - 600)/2.0D, 600L, 1500L, (1500 + 600)/2.0D, -1200L, 1000L, (1000 - 1200)/2.0);
	        AssertTotalsByCustomer("c2", -600L, -600L, -600D);
	        AssertTotalsBySupplier("s2", -600L, -600L, -600D);

	        _runtime.EventService.AdvanceTime(20000); // Set the time to 20 seconds

	        SendEvent(new TxnEventC("id3", 20000, "s1"));
	        SendEvent(new TxnEventA("id3", 20100, "c1"));
	        SendEvent(new TxnEventB("id3", 20200));
	        AssertTotals(-600L, 2500L, (2500 - 600 - 100)/3.0D, -200L, 1500L, (1500 + 600 - 200)/3.0, -1200L, 1000L, (1000 - 1200 + 100)/3.0);
	        AssertTotalsByCustomer("c1", -100L, 2500L, (2500 - 100) / 2.0);
	        AssertTotalsBySupplier("s1", -100L, 2500L, (2500 - 100) / 2.0);

	        // Set the time to 30 minutes and 5 seconds later expelling latencies for "id1"
	        int seconds = 30 * 60 + 5;
	        _runtime.EventService.AdvanceTime(seconds * 1000);
	        AssertTotals(-600L, -100L, (-600-100)/2.0, -200L, 600L, (600 - 200)/2.0, -1200L, 100L, (-1200 + 100)/2.0);
	        AssertTotalsByCustomer("c1", -100L, -100L, -100D);
	        AssertTotalsBySupplier("s1", -100L, -100L, -100D);

	        // Set the time to 30 minutes and 10 seconds later expelling latencies for "id2"
	        seconds = 30 * 60 + 10;
	        _runtime.EventService.AdvanceTime(seconds * 1000);
	        AssertTotals(-100L, -100L, -100D, -200L, -200L, - 200D, 100L, 100L, 100D);
	        AssertTotalsByCustomer("c2", null, null, null);
	        AssertTotalsBySupplier("s2", null, null, null);

	        // Set the time to 30 minutes and 20 seconds later expelling remaining latencies "id3"
	        seconds = 30 * 60 + 20;
	        _runtime.EventService.AdvanceTime(seconds * 1000);
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
	        ClassicAssert.AreEqual(1, listenerTotals.NewDataList.Count);
	        ClassicAssert.AreEqual(1, listenerTotals.LastNewData.Length);
	        EventBean @event = listenerTotals.LastNewData[0];
	        ClassicAssert.AreEqual(minAC, @event.Get("minLatencyAC"));
	        ClassicAssert.AreEqual(maxAC, @event.Get("maxLatencyAC"));
	        ClassicAssert.AreEqual(avgAC, @event.Get("avgLatencyAC"));
	        ClassicAssert.AreEqual(minBC, @event.Get("minLatencyBC"));
	        ClassicAssert.AreEqual(maxBC, @event.Get("maxLatencyBC"));
	        ClassicAssert.AreEqual(avgBC, @event.Get("avgLatencyBC"));
	        ClassicAssert.AreEqual(minAB, @event.Get("minLatencyAB"));
	        ClassicAssert.AreEqual(maxAB, @event.Get("maxLatencyAB"));
	        ClassicAssert.AreEqual(avgAB, @event.Get("avgLatencyAB"));
	        listenerTotals.Reset();
	    }

        private void AssertTotalsByCustomer(string customerId, long? minAC, long? maxAC, double? avgAC)
	    {
            ClassicAssert.AreEqual(1, listenerByCustomer.NewDataList.Count);
            ClassicAssert.AreEqual(1, listenerByCustomer.LastNewData.Length);
	        EventBean @event = listenerByCustomer.LastNewData[0];
	        ClassicAssert.AreEqual(customerId, @event.Get("customerId"));
	        ClassicAssert.AreEqual(minAC, @event.Get("minLatency"));
	        ClassicAssert.AreEqual(maxAC, @event.Get("maxLatency"));
	        ClassicAssert.AreEqual(avgAC, @event.Get("avgLatency"));
	        listenerByCustomer.Reset();
	    }

        private void AssertTotalsBySupplier(string supplierId, long? minAC, long? maxAC, double? avgAC)
	    {
	        ClassicAssert.AreEqual(1, listenerBySupplier.NewDataList.Count);
	        ClassicAssert.AreEqual(1, listenerBySupplier.LastNewData.Length);
	        EventBean @event = listenerBySupplier.LastNewData[0];
	        ClassicAssert.AreEqual(supplierId, @event.Get("supplierId"));
	        ClassicAssert.AreEqual(minAC, @event.Get("minLatency"));
	        ClassicAssert.AreEqual(maxAC, @event.Get("maxLatency"));
	        ClassicAssert.AreEqual(avgAC, @event.Get("avgLatency"));
	        listenerBySupplier.Reset();
	    }
	}
} // End of namespace
