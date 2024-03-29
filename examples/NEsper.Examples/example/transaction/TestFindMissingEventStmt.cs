///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace NEsper.Examples.Transaction
{
    [TestFixture]
    public class TestFindMissingEventStmt : TestStmtBase
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            base.SetUp();

            listener = new SupportUpdateListener();
            EPStatement stmt = FindMissingEventStmt.Create(_runtime);
            stmt.Events += listener.Update;

            // Use external clocking for the test
            _runtime.EventService.ClockExternal();
        }

        #endregion

        private const int TIME_WINDOW_SIZE_MSEC = 1800*1000;

        private SupportUpdateListener listener;

        private void AssertReceivedEvent(TxnEventA expectedA, TxnEventB expectedB)
        {
            ClassicAssert.AreEqual(1, listener.OldDataList.Count);
            ClassicAssert.AreEqual(1, listener.LastOldData.Length);
            EventBean combinedEvent = listener.LastOldData[0];
            Compare(combinedEvent, expectedA, expectedB);
            listener.Reset();
        }

        private void AssertReceivedTwoEvents(TxnEventA expectedA, TxnEventB expectedB)
        {
            ClassicAssert.AreEqual(1, listener.OldDataList.Count);
            ClassicAssert.AreEqual(2, listener.LastOldData.Length);

            // The order is not guaranteed
            if (listener.LastOldData[0].Get("A") == expectedA) {
                Compare(listener.LastOldData[0], expectedA, null);
                Compare(listener.LastOldData[1], null, expectedB);
            }
            else {
                Compare(listener.LastOldData[1], expectedA, null);
                Compare(listener.LastOldData[0], null, expectedB);
            }

            listener.Reset();
        }

        private static void Compare(EventBean combinedEvent, TxnEventA expectedA, TxnEventB expectedB)
        {
            ClassicAssert.AreSame(expectedA, combinedEvent.Get("A"));
            ClassicAssert.AreSame(expectedB, combinedEvent.Get("B"));
            ClassicAssert.IsNull(combinedEvent.Get("C"));
        }

        [Test]
        public void TestFlow()
        {
            var a = new TxnEventA[20];
            var b = new TxnEventB[20];
            var c = new TxnEventC[20];

            int seconds = 1*60; // after 1 minutes
            _runtime.EventService.AdvanceTime(seconds*1000);
            SendEvent(a[0] = new TxnEventA("id0", seconds*1000, "c1"));
            SendEvent(b[0] = new TxnEventB("id0", seconds*1000));
            SendEvent(c[0] = new TxnEventC("id0", seconds*1000, "s1"));

            seconds = 2*60;
            _runtime.EventService.AdvanceTime(seconds*1000);
            SendEvent(a[1] = new TxnEventA("id1", seconds*1000, "c1"));

            seconds = 3*60;
            _runtime.EventService.AdvanceTime(seconds*1000);
            SendEvent(b[2] = new TxnEventB("id2", seconds*1000));

            seconds = 4*60;
            _runtime.EventService.AdvanceTime(seconds*1000);
            SendEvent(b[3] = new TxnEventB("id3", seconds*1000));
            SendEvent(c[3] = new TxnEventC("id3", seconds*1000, "s1"));

            seconds = 5*60;
            _runtime.EventService.AdvanceTime(seconds*1000);
            SendEvent(a[4] = new TxnEventA("id4", seconds*1000, "c1"));
            SendEvent(c[4] = new TxnEventC("id4", seconds*1000, "s1"));

            seconds = 6*60;
            _runtime.EventService.AdvanceTime(seconds*1000);
            SendEvent(a[5] = new TxnEventA("id5", seconds*1000, "c1"));
            SendEvent(b[5] = new TxnEventB("id5", seconds*1000));

            listener.Reset();

            _runtime.EventService.AdvanceTime(TIME_WINDOW_SIZE_MSEC + 1*60*1000); // Expire "id0" from window
            ClassicAssert.IsFalse(listener.IsInvoked);

            _runtime.EventService.AdvanceTime(TIME_WINDOW_SIZE_MSEC + 2*60*1000); // Expire "id1" from window
            AssertReceivedEvent(a[1], null);

            _runtime.EventService.AdvanceTime(TIME_WINDOW_SIZE_MSEC + 3*60*1000);
            AssertReceivedEvent(null, b[2]);

            _runtime.EventService.AdvanceTime(TIME_WINDOW_SIZE_MSEC + 4*60*1000);
            ClassicAssert.IsFalse(listener.IsInvoked);

            _runtime.EventService.AdvanceTime(TIME_WINDOW_SIZE_MSEC + 5*60*1000);
            ClassicAssert.IsFalse(listener.IsInvoked);

            _runtime.EventService.AdvanceTime(TIME_WINDOW_SIZE_MSEC + 6*60*1000);
            AssertReceivedTwoEvents(a[5], b[5]);
        }
    }
}
