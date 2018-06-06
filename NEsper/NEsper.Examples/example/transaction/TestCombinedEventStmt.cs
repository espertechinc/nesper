///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;

using NUnit.Framework;

namespace NEsper.Examples.Transaction
{
    [TestFixture]
	public class TestCombinedEventStmt : TestStmtBase
	{
	    private SupportUpdateListener listener;

	    [SetUp]
	    public override void SetUp()
	    {
	        base.SetUp();

	        listener = new SupportUpdateListener();
	        var stmt = CombinedEventStmt.Create(epService.EPAdministrator);
	        stmt.Events += listener.Update;
	    }

	    [Test]
	    public void TestFlow()
	    {
	        TxnEventA a = new TxnEventA("id1", 1, "c1");
	        TxnEventB b = new TxnEventB("id1", 2);
	        TxnEventC c = new TxnEventC("id1", 3, "s1");

	        // send 3 events with C last
	        SendEvent(a);
	        SendEvent(b);
	        Assert.IsFalse(listener.IsInvoked);
	        SendEvent(c);
	        AssertCombinedEvent(a, b, c);

	        // send events not matching id
	        a = new TxnEventA("id4", 4, "c2");
	        b = new TxnEventB("id2", 5);
	        c = new TxnEventC("id3", 6, "s2");
	        SendEvent(a);
	        SendEvent(b);
	        Assert.IsFalse(listener.IsInvoked);
	        SendEvent(c);

	        // send events with B last
	        a = new TxnEventA("id3", 7, "c2");
	        b = new TxnEventB("id3", 8);
	        SendEvent(a);
            Assert.IsFalse(listener.IsInvoked);
	        SendEvent(b);
	        AssertCombinedEvent(a, b, c);

	        // send events with A last
	        a = new TxnEventA("id6", 9, "c2");
	        b = new TxnEventB("id6", 10);
	        c = new TxnEventC("id6", 11, "s2");
	        SendEvent(b);
	        SendEvent(c);
            Assert.IsFalse(listener.IsInvoked);
	        SendEvent(a);
	        AssertCombinedEvent(a, b, c);
	    }

	    private void AssertCombinedEvent(TxnEventA expectedA, TxnEventB expectedB, TxnEventC expectedC)
	    {
            Assert.AreEqual(1, listener.NewDataList.Count);
	        Assert.AreEqual(1, listener.LastNewData.Length);
	        EventBean combinedEvent = listener.LastNewData[0];
	        Assert.AreSame(expectedC.TransactionId, combinedEvent.Get("transactionId"));
            Assert.AreSame(expectedB.TransactionId, combinedEvent.Get("transactionId"));
            Assert.AreSame(expectedA.TransactionId, combinedEvent.Get("transactionId"));
            Assert.AreSame(expectedA.CustomerId, combinedEvent.Get("customerId"));
            Assert.AreSame(expectedC.SupplierId, combinedEvent.Get("supplierId"));
            Assert.AreEqual(expectedC.Timestamp - expectedA.Timestamp, combinedEvent.Get("latencyAC"));
            Assert.AreEqual(expectedB.Timestamp - expectedA.Timestamp, combinedEvent.Get("latencyAB"));
            Assert.AreEqual(expectedC.Timestamp - expectedB.Timestamp, combinedEvent.Get("latencyBC"));
	        listener.Reset();
	    }
	}
} // End of namespace
