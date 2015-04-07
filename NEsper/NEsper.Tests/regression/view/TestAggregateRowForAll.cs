///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestAggregateRowForAll
    {
        private const String JOIN_KEY = "KEY";

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private EPStatement _selectTestView;

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
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
        public void TestSumOneView()
        {
            String viewExpr = "select irstream sum(LongBoxed) as mySum " +
                              "from " + typeof(SupportBean).FullName + ".win:time(10 sec)";
            _selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            _selectTestView.Events += _listener.Update;

            RunAssert();
        }

        [Test]
        public void TestSumJoin()
        {
            String viewExpr = "select irstream sum(LongBoxed) as mySum " +
                              "from " + typeof(SupportBeanString).FullName + ".win:time(10) as one, " +
                                        typeof(SupportBean).FullName + ".win:time(10 sec) as two " +
                              "where one.TheString = two.TheString";

            _selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            _selectTestView.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanString(JOIN_KEY));

            RunAssert();
        }

        private void RunAssert()
        {
            // assert select result type
            Assert.AreEqual(typeof(long?), _selectTestView.EventType.GetPropertyType("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new String[] { "mySum" }, new Object[][] { new Object[] { null } });

            SendTimerEvent(0);
            SendEvent(10);
            Assert.AreEqual(10L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new String[] { "mySum" }, new Object[][] { new Object[] { 10L } });

            SendTimerEvent(5000);
            SendEvent(15);
            Assert.AreEqual(25L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new String[] { "mySum" }, new Object[][] { new Object[] { 25L } });

            SendTimerEvent(8000);
            SendEvent(-5);
            Assert.AreEqual(20L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
            Assert.IsNull(_listener.LastOldData);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new String[] { "mySum" }, new Object[][] { new Object[] { 20L } });

            SendTimerEvent(10000);
            Assert.AreEqual(20L, _listener.LastOldData[0].Get("mySum"));
            Assert.AreEqual(10L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new String[] { "mySum" }, new Object[][] { new Object[] { 10L } });

            SendTimerEvent(15000);
            Assert.AreEqual(10L, _listener.LastOldData[0].Get("mySum"));
            Assert.AreEqual(-5L, _listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new String[] { "mySum" }, new Object[][] { new Object[] { -5L } });

            SendTimerEvent(18000);
            Assert.AreEqual(-5L, _listener.LastOldData[0].Get("mySum"));
            Assert.IsNull(_listener.GetAndResetLastNewData()[0].Get("mySum"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_selectTestView.GetEnumerator(), new String[] { "mySum" }, new Object[][] { new Object[] { null } });
        }

        [Test]
        public void TestAvgPerSym()
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream avg(Price) as avgp, sym from " + typeof(SupportPriceEvent).FullName + ".std:groupwin(sym).win:length(2)"
            );
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            _epService.EPRuntime.SendEvent(new SupportPriceEvent(1, "A"));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("A", theEvent.Get("sym"));
            Assert.AreEqual(1.0, theEvent.Get("avgp"));

            _epService.EPRuntime.SendEvent(new SupportPriceEvent(2, "B"));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("B", theEvent.Get("sym"));
            Assert.AreEqual(1.5, theEvent.Get("avgp"));

            _epService.EPRuntime.SendEvent(new SupportPriceEvent(9, "A"));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("A", theEvent.Get("sym"));
            Assert.AreEqual((1 + 2 + 9) / 3.0, theEvent.Get("avgp"));

            _epService.EPRuntime.SendEvent(new SupportPriceEvent(18, "B"));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("B", theEvent.Get("sym"));
            Assert.AreEqual((1 + 2 + 9 + 18) / 4.0, theEvent.Get("avgp"));

            _epService.EPRuntime.SendEvent(new SupportPriceEvent(5, "A"));
            theEvent = listener.LastNewData[0];
            Assert.AreEqual("A", theEvent.Get("sym"));
            Assert.AreEqual((2 + 9 + 18 + 5) / 4.0, theEvent.Get("avgp"));
            theEvent = listener.LastOldData[0];
            Assert.AreEqual("A", theEvent.Get("sym"));
            Assert.AreEqual((5 + 2 + 9 + 18) / 4.0, theEvent.Get("avgp"));
        }

        [Test]
        public void TestSelectStarStdGroupBy()
        {
            String stmtText = "select istream * from " + typeof(SupportMarketDataBean).FullName
                    + ".std:groupwin(symbol).win:length(2)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _listener.Update;

            SendEvent("A", 1);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1.0, _listener.LastNewData[0].Get("Price"));
            Assert.IsTrue(_listener.LastNewData[0].Underlying is SupportMarketDataBean);
        }

        [Test]
        public void TestSelectExprStdGroupBy()
        {
            String stmtText = "select istream Price from " + typeof(SupportMarketDataBean).FullName
                    + ".std:groupwin(symbol).win:length(2)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _listener.Update;

            SendEvent("A", 1);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1.0, _listener.LastNewData[0].Get("Price"));
        }

        [Test]
        public void TestSelectAvgExprStdGroupBy()
        {
            String stmtText = "select istream avg(Price) as aPrice from " + typeof(SupportMarketDataBean).FullName
                    + ".std:groupwin(symbol).win:length(2)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _listener.Update;

            SendEvent("A", 1);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1.0, _listener.LastNewData[0].Get("aPrice"));
            SendEvent("B", 3);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(2.0, _listener.LastNewData[0].Get("aPrice"));
        }

        [Test]
        public void TestSelectAvgStdGroupByUni()
        {
            String stmtText = "select istream average as aPrice from " + typeof(SupportMarketDataBean).FullName
                    + ".std:groupwin(symbol).win:length(2).stat:uni(Price)";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _listener.Update;

            SendEvent("A", 1);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(1.0, _listener.LastNewData[0].Get("aPrice"));
            SendEvent("B", 3);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(3.0, _listener.LastNewData[0].Get("aPrice"));
            SendEvent("A", 3);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(2.0, _listener.LastNewData[0].Get("aPrice"));
            SendEvent("A", 10);
            SendEvent("A", 20);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(15.0, _listener.LastNewData[0].Get("aPrice"));
        }

        [Test]
        public void TestSelectAvgExprGroupBy()
        {
            String stmtText = "select istream avg(Price) as aPrice, Symbol from " + typeof(SupportMarketDataBean).FullName
                    + ".win:length(2) group by Symbol";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            statement.Events += _listener.Update;

            SendEvent("A", 1);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1.0, _listener.LastNewData[0].Get("aPrice"));
            Assert.AreEqual("A", _listener.LastNewData[0].Get("Symbol"));
            SendEvent("B", 3);
            //there is no A->1 as we already got it out
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual(3.0, _listener.LastNewData[0].Get("aPrice"));
            Assert.AreEqual("B", _listener.LastNewData[0].Get("Symbol"));
            SendEvent("B", 5);
            // there is NOW a A->null entry
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(2, _listener.LastNewData.Length);
            Assert.AreEqual(4.0, _listener.LastNewData[0].Get("aPrice"));
            Assert.AreEqual("B", _listener.LastNewData[0].Get("Symbol"));
            Assert.AreEqual(null, _listener.LastNewData[1].Get("aPrice"));
            Assert.AreEqual("A", _listener.LastNewData[1].Get("Symbol"));
            SendEvent("A", 10);
            SendEvent("A", 20);
            Assert.IsTrue(_listener.GetAndClearIsInvoked());
            Assert.AreEqual(2, _listener.LastNewData.Length);
            Assert.AreEqual(15.0, _listener.LastNewData[0].Get("aPrice"));//A
            Assert.AreEqual(null, _listener.LastNewData[1].Get("aPrice"));//B
        }

        private Object SendEvent(String symbol, double price)
        {
            Object theEvent = new SupportMarketDataBean(symbol, price, null, null);
            _epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }

        private void SendEvent(long longBoxed, int intBoxed, short shortBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = JOIN_KEY;
            bean.LongBoxed = longBoxed;
            bean.IntBoxed = intBoxed;
            bean.ShortBoxed = shortBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendEvent(long longBoxed)
        {
            SendEvent(longBoxed, 0, (short)0);
        }

        private void SendEventInt(int intBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.IntBoxed = intBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendEventFloat(float floatBoxed)
        {
            SupportBean bean = new SupportBean();
            bean.FloatBoxed = floatBoxed;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendTimerEvent(long msec)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(msec));
        }
    }
}
