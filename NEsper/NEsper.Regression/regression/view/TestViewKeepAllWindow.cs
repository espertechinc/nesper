///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewKeepAllWindow
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestIterator()
        {
            String viewExpr = "select Symbol, Price from " + typeof(SupportMarketDataBean).FullName + "#keepall";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(viewExpr);
            statement.Events += _listener.Update;

            SendEvent("ABC", 20);
            SendEvent("DEF", 100);

            // check iterator results
            IEnumerator<EventBean> events = statement.GetEnumerator();
            EventBean theEvent = events.Advance();
            Assert.AreEqual("ABC", theEvent.Get("Symbol"));
            Assert.AreEqual(20d, theEvent.Get("Price"));

            theEvent = events.Advance();
            Assert.AreEqual("DEF", theEvent.Get("Symbol"));
            Assert.AreEqual(100d, theEvent.Get("Price"));
            Assert.IsFalse(events.MoveNext());

            SendEvent("EFG", 50);

            // check iterator results
            events = statement.GetEnumerator();
            theEvent = events.Advance();
            Assert.AreEqual("ABC", theEvent.Get("Symbol"));
            Assert.AreEqual(20d, theEvent.Get("Price"));

            theEvent = events.Advance();
            Assert.AreEqual("DEF", theEvent.Get("Symbol"));
            Assert.AreEqual(100d, theEvent.Get("Price"));

            theEvent = events.Advance();
            Assert.AreEqual("EFG", theEvent.Get("Symbol"));
            Assert.AreEqual(50d, theEvent.Get("Price"));
        }

        [Test]
        public void TestWindowStats()
        {
            String viewExpr = "select irstream Symbol, count(*) as cnt, sum(Price) as mysum from " + typeof(SupportMarketDataBean).FullName +
                    "#keepall group by Symbol";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(viewExpr);
            statement.Events += _listener.Update;
            _listener.Reset();

            SendEvent("S1", 100);
            String[] fields = new String[] { "Symbol", "cnt", "mysum" };
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "S1", 1L, 100d });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "S1", 0L, null });
            _listener.Reset();

            SendEvent("S2", 50);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "S2", 1L, 50d });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "S2", 0L, null });
            _listener.Reset();

            SendEvent("S1", 5);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "S1", 2L, 105d });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "S1", 1L, 100d });
            _listener.Reset();

            SendEvent("S2", -1);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "S2", 2L, 49d });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "S2", 1L, 50d });
            _listener.Reset();
        }

        private void SendEvent(String symbol, double price)
        {
            SupportMarketDataBean theEvent = new SupportMarketDataBean(symbol, price, 0L, "");
            _epService.EPRuntime.SendEvent(theEvent);
        }
    }
}
