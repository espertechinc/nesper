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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.type;
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestPreviousFunction
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
        public void TestExprNameAndTypeAndSODA()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            String epl = "select " +
                    "prev(1,IntPrimitive), " +
                    "prev(1,sb), " +
                    "prevtail(1,IntPrimitive), " +
                    "prevtail(1,sb), " +
                    "prevwindow(IntPrimitive), " +
                    "prevwindow(sb), " +
                    "prevcount(IntPrimitive), " +
                    "prevcount(sb) " +
                    "from SupportBean.win:time(1 minutes) as sb";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EventBean resultBean = _listener.GetNewDataListFlattened()[1];

            Object[][] rows = new Object[][]{
                    new Object[] {"prev(1,IntPrimitive)", typeof(int?)},
                    new Object[] {"prev(1,sb)", typeof(SupportBean)},
                    new Object[] {"prevtail(1,IntPrimitive)", typeof(int?)},
                    new Object[] {"prevtail(1,sb)", typeof(SupportBean)},
                    new Object[] {"prevwindow(IntPrimitive)", typeof(int?[])},
                    new Object[] {"prevwindow(sb)", typeof(SupportBean[])},
                    new Object[] {"prevcount(IntPrimitive)", typeof(long?)},
                    new Object[] {"prevcount(sb)", typeof(long?)}
            };
            for (int i = 0; i < rows.Length; i++)
            {
                String message = "For prop '" + rows[i][0] + "'";
                EventPropertyDescriptor prop = stmt.EventType.PropertyDescriptors[i];
                Assert.AreEqual(rows[i][0], prop.PropertyName, message);
                Assert.AreEqual(rows[i][1], prop.PropertyType, message);
                Object result = resultBean.Get(prop.PropertyName);
                Assert.AreEqual(prop.PropertyType, result.GetType().GetBoxedType(), message);
            }

            stmt.Dispose();
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(model.ToEPL(), epl);
            stmt = _epService.EPAdministrator.CreateEPL(epl);
            Assert.AreEqual(stmt.Text, epl);
        }

        [Test]
        public void TestPrevStream()
        {
            _epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            String text = "select prev(1, s0) as result, " +
                    "prevtail(0, s0) as tailresult," +
                    "prevwindow(s0) as windowresult," +
                    "prevcount(s0) as countresult " +
                    "from S0.win:length(2) as s0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;

            String[] fields = "result,tailresult,windowresult,countresult".Split(',');

            SupportBean_S0 e1 = new SupportBean_S0(1);
            _epService.EPRuntime.SendEvent(e1);

            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { null, e1, new Object[] { e1 }, 1L });

            SupportBean_S0 e2 = new SupportBean_S0(2);
            _epService.EPRuntime.SendEvent(e2);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { e1, e1, new Object[] { e2, e1 }, 2L });
            Assert.AreEqual(typeof(SupportBean_S0), stmt.EventType.GetPropertyType("result"));

            SupportBean_S0 e3 = new SupportBean_S0(3);
            _epService.EPRuntime.SendEvent(e3);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields,
                    new Object[] { e2, e2, new Object[] { e3, e2 }, 2L });
        }

        [Test]
        public void TestPrevCountStarWithStaticMethod()
        {
            String text = "select irstream count(*) as total, " +
                    "prev(" + typeof(TestPreviousFunction).FullName + ".IntToLong(count(*)) - 1, Price) as firstPrice from " + typeof(SupportMarketDataBean).FullName + ".win:time(60)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;

            AssertPrevCount();
        }

        [Test]
        public void TestPrevCountStar()
        {
            String text = "select irstream count(*) as total, " +
                    "prev(count(*) - 1, Price) as firstPrice from " + typeof(SupportMarketDataBean).FullName + ".win:time(60)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text);
            stmt.Events += _listener.Update;

            AssertPrevCount();
        }

        private void AssertPrevCount()
        {
            SendTimer(0);
            SendMarketEvent("IBM", 75);
            AssertCountAndPrice(_listener.AssertOneGetNewAndReset(), 1L, 75D);

            SendMarketEvent("IBM", 76);
            AssertCountAndPrice(_listener.AssertOneGetNewAndReset(), 2L, 75D);

            SendTimer(10000);
            SendMarketEvent("IBM", 77);
            AssertCountAndPrice(_listener.AssertOneGetNewAndReset(), 3L, 75D);

            SendTimer(20000);
            SendMarketEvent("IBM", 78);
            AssertCountAndPrice(_listener.AssertOneGetNewAndReset(), 4L, 75D);

            SendTimer(50000);
            SendMarketEvent("IBM", 79);
            AssertCountAndPrice(_listener.AssertOneGetNewAndReset(), 5L, 75D);

            SendTimer(60000);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EventBean[] oldData = _listener.LastOldData;
            Assert.AreEqual(2, oldData.Length);
            AssertCountAndPrice(oldData[0], 3L, null);
            _listener.Reset();

            SendMarketEvent("IBM", 80);
            AssertCountAndPrice(_listener.AssertOneGetNewAndReset(), 4L, 77D);

            SendTimer(65000);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(70000);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            oldData = _listener.LastOldData;
            Assert.AreEqual(1, oldData.Length);
            AssertCountAndPrice(oldData[0], 3L, null);
            _listener.Reset();

            SendTimer(80000);
            _listener.Reset();

            SendMarketEvent("IBM", 81);
            AssertCountAndPrice(_listener.AssertOneGetNewAndReset(), 3L, 79D);

            SendTimer(120000);
            _listener.Reset();

            SendMarketEvent("IBM", 82);
            AssertCountAndPrice(_listener.AssertOneGetNewAndReset(), 2L, 81D);

            SendTimer(300000);
            _listener.Reset();

            SendMarketEvent("IBM", 83);
            AssertCountAndPrice(_listener.AssertOneGetNewAndReset(), 1L, 83D);
        }

        [Test]
        public void TestPerGroupTwoCriteria()
        {
            _epService.EPAdministrator.Configuration.AddEventType("MDBean", typeof(SupportMarketDataBean));
            String viewExpr = "select Symbol, Feed, " +
                    "prev(1, Price) as prevPrice, " +
                    "prevtail(Price) as tailPrice, " +
                    "prevcount(Price) as countPrice, " +
                    "prevwindow(Price) as windowPrice " +
                    "from MDBean.std:groupwin(Symbol, Feed).win:length(2)";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;
            String[] fields = "Symbol,Feed,prevPrice,tailPrice,countPrice,windowPrice".Split(',');

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 10, 0L, "F1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "IBM", "F1", null, 10d, 1L, SplitDoubles("10d") });

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 11, 0L, "F1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "IBM", "F1", 10d, 10d, 2L, SplitDoubles("11d,10d") });

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("MSFT", 100, 0L, "F2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "MSFT", "F2", null, 100d, 1L, SplitDoubles("100d") });

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 12, 0L, "F2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "IBM", "F2", null, 12d, 1L, SplitDoubles("12d") });

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 13, 0L, "F1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "IBM", "F1", 11d, 11d, 2L, SplitDoubles("13d,11d") });

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("MSFT", 101, 0L, "F2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "MSFT", "F2", 100d, 100d, 2L, SplitDoubles("101d,100d") });

            _epService.EPRuntime.SendEvent(new SupportMarketDataBean("IBM", 17, 0L, "F2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "IBM", "F2", 12d, 12d, 2L, SplitDoubles("17d,12d") });

            // test length window overflow
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            _epService.EPAdministrator.CreateEPL("select prev(5,IntPrimitive) as val0 from SupportBean.std:groupwin(TheString).win:length(5)").Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("A", 11));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("A", 12));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("A", 13));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("A", 14));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("A", 15));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("C", 20));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("C", 21));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("C", 22));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("C", 23));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("C", 24));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("B", 31));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("C", 25));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));

            _epService.EPRuntime.SendEvent(new SupportBean("A", 16));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("val0"));
        }

        [Test]
        public void TestSortWindowPerGroup()
        {
            // descending sort
            String viewExpr = "select " +
                    "Symbol, " +
                    "prev(1, Price) as prevPrice, " +
                    "prev(2, Price) as prevPrevPrice, " +
                    "prevtail(0, Price) as prevTail0Price, " +
                    "prevtail(1, Price) as prevTail1Price, " +
                    "prevcount(Price) as countPrice, " +
                    "prevwindow(Price) as windowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".std:groupwin(Symbol).ext:sort(10, Price asc) ";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrice"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrevPrice"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevTail0Price"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevTail1Price"));
            Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("countPrice"));
            Assert.AreEqual(typeof(double?[]), selectTestView.EventType.GetPropertyType("windowPrice"));

            SendMarketEvent("IBM", 75);
            AssertReceived("IBM", null, null, 75d, null, 1L, SplitDoubles("75d"));
            SendMarketEvent("IBM", 80);
            AssertReceived("IBM", 80d, null, 80d, 75d, 2L, SplitDoubles("75d,80d"));
            SendMarketEvent("IBM", 79);
            AssertReceived("IBM", 79d, 80d, 80d, 79d, 3L, SplitDoubles("75d,79d,80d"));
            SendMarketEvent("IBM", 81);
            AssertReceived("IBM", 79d, 80d, 81d, 80d, 4L, SplitDoubles("75d,79d,80d,81d"));
            SendMarketEvent("IBM", 79.5);
            AssertReceived("IBM", 79d, 79.5d, 81d, 80d, 5L, SplitDoubles("75d,79d,79.5,80d,81d"));    // 75, 79, 79.5, 80, 81

            SendMarketEvent("MSFT", 10);
            AssertReceived("MSFT", null, null, 10d, null, 1L, SplitDoubles("10d"));
            SendMarketEvent("MSFT", 20);
            AssertReceived("MSFT", 20d, null, 20d, 10d, 2L, SplitDoubles("10d,20d"));
            SendMarketEvent("MSFT", 21);
            AssertReceived("MSFT", 20d, 21d, 21d, 20d, 3L, SplitDoubles("10d,20d,21d")); // 10, 20, 21

            SendMarketEvent("IBM", 74d);
            AssertReceived("IBM", 75d, 79d, 81d, 80d, 6L, SplitDoubles("74d,75d,79d,79.5,80d,81d"));  // 74, 75, 79, 79.5, 80, 81

            SendMarketEvent("MSFT", 19);
            AssertReceived("MSFT", 19d, 20d, 21d, 20d, 4L, SplitDoubles("10d,19d,20d,21d")); // 10, 19, 20, 21
        }

        [Test]
        public void TestTimeBatchPerGroup()
        {
            String viewExpr = "select " +
                    "Symbol, " +
                    "prev(1, Price) as prevPrice, " +
                    "prev(2, Price) as prevPrevPrice, " +
                    "prevtail(0, Price) as prevTail0Price, " +
                    "prevtail(1, Price) as prevTail1Price, " +
                    "prevcount(Price) as countPrice, " +
                    "prevwindow(Price) as windowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".std:groupwin(Symbol).win:time_batch(1 sec) ";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrice"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrevPrice"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevTail0Price"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevTail1Price"));

            SendTimer(0);
            SendMarketEvent("IBM", 75);
            SendMarketEvent("MSFT", 40);
            SendMarketEvent("IBM", 76);
            SendMarketEvent("CIC", 1);
            SendTimer(1000);

            EventBean[] events = _listener.LastNewData;
            // order not guaranteed as timed batch, however for testing the order is reliable as schedule buckets are created
            // in a predictable order
            // Previous is looking at the same batch, doesn't consider outside of window
            AssertReceived(events[0], "IBM", null, null, 75d, 76d, 2L, SplitDoubles("76d,75d"));
            AssertReceived(events[1], "IBM", 75d, null, 75d, 76d, 2L, SplitDoubles("76d,75d"));
            AssertReceived(events[2], "MSFT", null, null, 40d, null, 1L, SplitDoubles("40d"));
            AssertReceived(events[3], "CIC", null, null, 1d, null, 1L, SplitDoubles("1d"));

            // Next batch, previous is looking only within the same batch
            SendMarketEvent("MSFT", 41);
            SendMarketEvent("IBM", 77);
            SendMarketEvent("IBM", 78);
            SendMarketEvent("CIC", 2);
            SendMarketEvent("MSFT", 42);
            SendMarketEvent("CIC", 3);
            SendMarketEvent("CIC", 4);
            SendTimer(2000);

            events = _listener.LastNewData;
            AssertReceived(events[0], "IBM", null, null, 77d, 78d, 2L, SplitDoubles("78d,77d"));
            AssertReceived(events[1], "IBM", 77d, null, 77d, 78d, 2L, SplitDoubles("78d,77d"));
            AssertReceived(events[2], "MSFT", null, null, 41d, 42d, 2L, SplitDoubles("42d,41d"));
            AssertReceived(events[3], "MSFT", 41d, null, 41d, 42d, 2L, SplitDoubles("42d,41d"));
            AssertReceived(events[4], "CIC", null, null, 2d, 3d, 3L, SplitDoubles("4d,3d,2d"));
            AssertReceived(events[5], "CIC", 2d, null, 2d, 3d, 3L, SplitDoubles("4d,3d,2d"));
            AssertReceived(events[6], "CIC", 3d, 2d, 2d, 3d, 3L, SplitDoubles("4d,3d,2d"));

            // test for memory leak - comment in and run with large number
            /*
            for (int i = 0; i < 10000; i++)
            {
                SendMarketEvent("MSFT", 41);
                SendTimer(1000 * i);
                listener.Reset();
            }
            */
        }

        [Test]
        public void TestLengthBatchPerGroup()
        {
            // Also testing the alternative syntax here of "prev(property)" and "prev(property, index)" versus "prev(index, property)"
            String viewExpr = "select irstream " +
                    "Symbol, " +
                    "prev(Price) as prevPrice, " +
                    "prev(Price, 2) as prevPrevPrice, " +
                    "prevtail(Price, 0) as prevTail0Price, " +
                    "prevtail(Price, 1) as prevTail1Price, " +
                    "prevcount(Price) as countPrice, " +
                    "prevwindow(Price) as windowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".std:groupwin(Symbol).win:length_batch(3) ";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrice"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrevPrice"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevTail0Price"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevTail1Price"));

            SendMarketEvent("IBM", 75);
            SendMarketEvent("MSFT", 50);
            SendMarketEvent("IBM", 76);
            SendMarketEvent("CIC", 1);
            Assert.IsFalse(_listener.IsInvoked);
            SendMarketEvent("IBM", 77);

            EventBean[] eventsNew = _listener.LastNewData;
            Assert.AreEqual(3, eventsNew.Length);
            AssertReceived(eventsNew[0], "IBM", null, null, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));
            AssertReceived(eventsNew[1], "IBM", 75d, null, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));
            AssertReceived(eventsNew[2], "IBM", 76d, 75d, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));
            _listener.Reset();

            // Next batch, previous is looking only within the same batch
            SendMarketEvent("MSFT", 51);
            SendMarketEvent("IBM", 78);
            SendMarketEvent("IBM", 79);
            SendMarketEvent("CIC", 2);
            SendMarketEvent("CIC", 3);

            eventsNew = _listener.LastNewData;
            Assert.AreEqual(3, eventsNew.Length);
            AssertReceived(eventsNew[0], "CIC", null, null, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));
            AssertReceived(eventsNew[1], "CIC", 1d, null, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));
            AssertReceived(eventsNew[2], "CIC", 2d, 1d, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));
            _listener.Reset();

            SendMarketEvent("MSFT", 52);

            eventsNew = _listener.LastNewData;
            Assert.AreEqual(3, eventsNew.Length);
            AssertReceived(eventsNew[0], "MSFT", null, null, 50d, 51d, 3L, SplitDoubles("52d,51d,50d"));
            AssertReceived(eventsNew[1], "MSFT", 50d, null, 50d, 51d, 3L, SplitDoubles("52d,51d,50d"));
            AssertReceived(eventsNew[2], "MSFT", 51d, 50d, 50d, 51d, 3L, SplitDoubles("52d,51d,50d"));
            _listener.Reset();

            SendMarketEvent("IBM", 80);

            eventsNew = _listener.LastNewData;
            EventBean[] eventsOld = _listener.LastOldData;
            Assert.AreEqual(3, eventsNew.Length);
            Assert.AreEqual(3, eventsOld.Length);
            AssertReceived(eventsNew[0], "IBM", null, null, 78d, 79d, 3L, SplitDoubles("80d,79d,78d"));
            AssertReceived(eventsNew[1], "IBM", 78d, null, 78d, 79d, 3L, SplitDoubles("80d,79d,78d"));
            AssertReceived(eventsNew[2], "IBM", 79d, 78d, 78d, 79d, 3L, SplitDoubles("80d,79d,78d"));
            AssertReceived(eventsOld[0], "IBM", null, null, null, null, null, null);
            AssertReceived(eventsOld[1], "IBM", null, null, null, null, null, null);
            AssertReceived(eventsOld[2], "IBM", null, null, null, null, null, null);
        }

        [Test]
        public void TestTimeWindowPerGroup()
        {
            String viewExpr = "select " +
                    "Symbol, " +
                    "prev(1, Price) as prevPrice, " +
                    "prev(2, Price) as prevPrevPrice, " +
                    "prevtail(0, Price) as prevTail0Price, " +
                    "prevtail(1, Price) as prevTail1Price, " +
                    "prevcount(Price) as countPrice, " +
                    "prevwindow(Price) as windowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".std:groupwin(Symbol).win:time(20 sec) ";
            AssertPerGroup(viewExpr);
        }

        [Test]
        public void TestExtTimeWindowPerGroup()
        {
            String viewExpr = "select " +
                    "Symbol, " +
                    "prev(1, Price) as prevPrice, " +
                    "prev(2, Price) as prevPrevPrice, " +
                    "prevtail(0, Price) as prevTail0Price, " +
                    "prevtail(1, Price) as prevTail1Price, " +
                    "prevcount(Price) as countPrice, " +
                    "prevwindow(Price) as windowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".std:groupwin(Symbol).win:ext_timed(Volume, 20 sec) ";
            AssertPerGroup(viewExpr);
        }

        [Test]
        public void TestLengthWindowPerGroup()
        {
            String viewExpr =
                    "select Symbol, " +
                            "prev(1, Price) as prevPrice, " +
                            "prev(2, Price) as prevPrevPrice, " +
                            "prevtail(Price, 0) as prevTail0Price, " +
                            "prevtail(Price, 1) as prevTail1Price, " +
                            "prevcount(Price) as countPrice, " +
                            "prevwindow(Price) as windowPrice " +
                            "from " + typeof(SupportMarketDataBean).FullName + ".std:groupwin(Symbol).win:length(10) ";
            AssertPerGroup(viewExpr);
        }

        [Test]
        public void TestPreviousTimeWindow()
        {
            String viewExpr = "select irstream Symbol as currSymbol, " +
                    " prev(2, Symbol) as prevSymbol, " +
                    " prev(2, Price) as prevPrice, " +
                    " prevtail(0, Symbol) as prevTailSymbol, " +
                    " prevtail(0, Price) as prevTailPrice, " +
                    " prevtail(1, Symbol) as prevTail1Symbol, " +
                    " prevtail(1, Price) as prevTail1Price, " +
                    " prevcount(Price) as prevCountPrice, " +
                    " prevwindow(Price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:time(1 min) ";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("prevSymbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrice"));

            SendTimer(0);
            Assert.IsFalse(_listener.IsInvoked);

            SendMarketEvent("D1", 1);
            AssertNewEventWTail("D1", null, null, "D1", 1d, null, null, 1L, SplitDoubles("1d"));

            SendTimer(1000);
            Assert.IsFalse(_listener.IsInvoked);

            SendMarketEvent("D2", 2);
            AssertNewEventWTail("D2", null, null, "D1", 1d, "D2", 2d, 2L, SplitDoubles("2d,1d"));

            SendTimer(2000);
            Assert.IsFalse(_listener.IsInvoked);

            SendMarketEvent("D3", 3);
            AssertNewEventWTail("D3", "D1", 1d, "D1", 1d, "D2", 2d, 3L, SplitDoubles("3d,2d,1d"));

            SendTimer(3000);
            Assert.IsFalse(_listener.IsInvoked);

            SendMarketEvent("D4", 4);
            AssertNewEventWTail("D4", "D2", 2d, "D1", 1d, "D2", 2d, 4L, SplitDoubles("4d,3d,2d,1d"));

            SendTimer(4000);
            Assert.IsFalse(_listener.IsInvoked);

            SendMarketEvent("D5", 5);
            AssertNewEventWTail("D5", "D3", 3d, "D1", 1d, "D2", 2d, 5L, SplitDoubles("5d,4d,3d,2d,1d"));

            SendTimer(30000);
            Assert.IsFalse(_listener.IsInvoked);

            SendMarketEvent("D6", 6);
            AssertNewEventWTail("D6", "D4", 4d, "D1", 1d, "D2", 2d, 6L, SplitDoubles("6d,5d,4d,3d,2d,1d"));

            // Test remove stream, always returns null as previous function
            // returns null for remove stream for time windows
            SendTimer(60000);
            AssertOldEventWTail("D1", null, null, null, null, null, null, null, null);
            SendTimer(61000);
            AssertOldEventWTail("D2", null, null, null, null, null, null, null, null);
            SendTimer(62000);
            AssertOldEventWTail("D3", null, null, null, null, null, null, null, null);
            SendTimer(63000);
            AssertOldEventWTail("D4", null, null, null, null, null, null, null, null);
            SendTimer(64000);
            AssertOldEventWTail("D5", null, null, null, null, null, null, null, null);
            SendTimer(90000);
            AssertOldEventWTail("D6", null, null, null, null, null, null, null, null);
        }

        [Test]
        public void TestPreviousExtTimedWindow()
        {
            String viewExpr = "select irstream Symbol as currSymbol, " +
                    " prev(2, Symbol) as prevSymbol, " +
                    " prev(2, Price) as prevPrice, " +
                    " prevtail(0, Symbol) as prevTailSymbol, " +
                    " prevtail(0, Price) as prevTailPrice, " +
                    " prevtail(1, Symbol) as prevTail1Symbol, " +
                    " prevtail(1, Price) as prevTail1Price, " +
                    " prevcount(Price) as prevCountPrice, " +
                    " prevwindow(Price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:ext_timed(Volume, 1 min) ";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("prevSymbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrice"));
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("prevTailSymbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevTailPrice"));

            SendMarketEvent("D1", 1, 0);
            AssertNewEventWTail("D1", null, null, "D1", 1d, null, null, 1L, SplitDoubles("1d"));

            SendMarketEvent("D2", 2, 1000);
            AssertNewEventWTail("D2", null, null, "D1", 1d, "D2", 2d, 2L, SplitDoubles("2d,1d"));

            SendMarketEvent("D3", 3, 3000);
            AssertNewEventWTail("D3", "D1", 1d, "D1", 1d, "D2", 2d, 3L, SplitDoubles("3d,2d,1d"));

            SendMarketEvent("D4", 4, 4000);
            AssertNewEventWTail("D4", "D2", 2d, "D1", 1d, "D2", 2d, 4L, SplitDoubles("4d,3d,2d,1d"));

            SendMarketEvent("D5", 5, 5000);
            AssertNewEventWTail("D5", "D3", 3d, "D1", 1d, "D2", 2d, 5L, SplitDoubles("5d,4d,3d,2d,1d"));

            SendMarketEvent("D6", 6, 30000);
            AssertNewEventWTail("D6", "D4", 4d, "D1", 1d, "D2", 2d, 6L, SplitDoubles("6d,5d,4d,3d,2d,1d"));

            SendMarketEvent("D7", 7, 60000);
            AssertEventWTail(_listener.LastNewData[0], "D7", "D5", 5d, "D2", 2d, "D3", 3d, 6L, SplitDoubles("7d,6d,5d,4d,3d,2d"));
            AssertEventWTail(_listener.LastOldData[0], "D1", null, null, null, null, null, null, null, null);
            _listener.Reset();

            SendMarketEvent("D8", 8, 61000);
            AssertEventWTail(_listener.LastNewData[0], "D8", "D6", 6d, "D3", 3d, "D4", 4d, 6L, SplitDoubles("8d,7d,6d,5d,4d,3d"));
            AssertEventWTail(_listener.LastOldData[0], "D2", null, null, null, null, null, null, null, null);
            _listener.Reset();
        }

        [Test]
        public void TestPreviousTimeBatchWindow()
        {
            String viewExpr = "select irstream Symbol as currSymbol, " +
                    " prev(2, Symbol) as prevSymbol, " +
                    " prev(2, Price) as prevPrice, " +
                    " prevtail(0, Symbol) as prevTailSymbol, " +
                    " prevtail(0, Price) as prevTailPrice, " +
                    " prevtail(1, Symbol) as prevTail1Symbol, " +
                    " prevtail(1, Price) as prevTail1Price, " +
                    " prevcount(Price) as prevCountPrice, " +
                    " prevwindow(Price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:time_batch(1 min) ";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("prevSymbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrice"));

            SendTimer(0);
            Assert.IsFalse(_listener.IsInvoked);

            SendMarketEvent("A", 1);
            SendMarketEvent("B", 2);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(60000);
            Assert.AreEqual(2, _listener.LastNewData.Length);
            AssertEventWTail(_listener.LastNewData[0], "A", null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
            AssertEventWTail(_listener.LastNewData[1], "B", null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendTimer(80000);
            SendMarketEvent("C", 3);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(120000);
            Assert.AreEqual(1, _listener.LastNewData.Length);
            AssertEventWTail(_listener.LastNewData[0], "C", null, null, "C", 3d, null, null, 1L, SplitDoubles("3d"));
            Assert.AreEqual(2, _listener.LastOldData.Length);
            AssertEventWTail(_listener.LastOldData[0], "A", null, null, null, null, null, null, null, null);
            _listener.Reset();

            SendTimer(300000);
            SendMarketEvent("D", 4);
            SendMarketEvent("E", 5);
            SendMarketEvent("F", 6);
            SendMarketEvent("G", 7);
            SendTimer(360000);
            Assert.AreEqual(4, _listener.LastNewData.Length);
            AssertEventWTail(_listener.LastNewData[0], "D", null, null, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));
            AssertEventWTail(_listener.LastNewData[1], "E", null, null, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));
            AssertEventWTail(_listener.LastNewData[2], "F", "D", 4d, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));
            AssertEventWTail(_listener.LastNewData[3], "G", "E", 5d, "D", 4d, "E", 5d, 4L, SplitDoubles("7d,6d,5d,4d"));
        }

        [Test]
        public void TestPreviousTimeBatchWindowJoin()
        {
            String viewExpr = "select TheString as currSymbol, " +
                    " prev(2, Symbol) as prevSymbol, " +
                    " prev(1, Price) as prevPrice, " +
                    " prevtail(0, Symbol) as prevTailSymbol, " +
                    " prevtail(0, Price) as prevTailPrice, " +
                    " prevtail(1, Symbol) as prevTail1Symbol, " +
                    " prevtail(1, Price) as prevTail1Price, " +
                    " prevcount(Price) as prevCountPrice, " +
                    " prevwindow(Price) as prevWindowPrice " +
                    "from " + typeof(SupportBean).FullName + ".win:keepall(), " +
                    typeof(SupportMarketDataBean).FullName + ".win:time_batch(1 min)";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("prevSymbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrice"));

            SendTimer(0);
            Assert.IsFalse(_listener.IsInvoked);

            SendMarketEvent("A", 1);
            SendMarketEvent("B", 2);
            SendBeanEvent("X1");
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(60000);
            Assert.AreEqual(2, _listener.LastNewData.Length);
            AssertEventWTail(_listener.LastNewData[0], "X1", null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
            AssertEventWTail(_listener.LastNewData[1], "X1", null, 1d, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
            Assert.IsNull(_listener.LastOldData);
            _listener.Reset();

            SendMarketEvent("C1", 11);
            SendMarketEvent("C2", 12);
            SendMarketEvent("C3", 13);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(120000);
            Assert.AreEqual(3, _listener.LastNewData.Length);
            AssertEventWTail(_listener.LastNewData[0], "X1", null, null, "C1", 11d, "C2", 12d, 3L, SplitDoubles("13d,12d,11d"));
            AssertEventWTail(_listener.LastNewData[1], "X1", null, 11d, "C1", 11d, "C2", 12d, 3L, SplitDoubles("13d,12d,11d"));
            AssertEventWTail(_listener.LastNewData[2], "X1", "C1", 12d, "C1", 11d, "C2", 12d, 3L, SplitDoubles("13d,12d,11d"));
        }

        [Test]
        public void TestPreviousLengthWindow()
        {
            String viewExpr = "select irstream Symbol as currSymbol, " +
                    "prev(0, Symbol) as prev0Symbol, " +
                    "prev(1, Symbol) as prev1Symbol, " +
                    "prev(2, Symbol) as prev2Symbol, " +
                    "prev(0, Price) as prev0Price, " +
                    "prev(1, Price) as prev1Price, " +
                    "prev(2, Price) as prev2Price," +
                    "prevtail(0, Symbol) as prevTail0Symbol, " +
                    "prevtail(0, Price) as prevTail0Price, " +
                    "prevtail(1, Symbol) as prevTail1Symbol, " +
                    "prevtail(1, Price) as prevTail1Price, " +
                    "prevcount(Price) as prevCountPrice, " +
                    "prevwindow(Price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) ";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("prev0Symbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prev0Price"));

            SendMarketEvent("A", 1);
            AssertNewEvents("A", "A", 1d, null, null, null, null, "A", 1d, null, null, 1L, SplitDoubles("1d"));
            SendMarketEvent("B", 2);
            AssertNewEvents("B", "B", 2d, "A", 1d, null, null, "A", 1d, "B", 2d, 2L, SplitDoubles("2d,1d"));
            SendMarketEvent("C", 3);
            AssertNewEvents("C", "C", 3d, "B", 2d, "A", 1d, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));
            SendMarketEvent("D", 4);
            EventBean newEvent = _listener.LastNewData[0];
            EventBean oldEvent = _listener.LastOldData[0];
            AssertEventProps(newEvent, "D", "D", 4d, "C", 3d, "B", 2d, "B", 2d, "C", 3d, 3L, SplitDoubles("4d,3d,2d"));
            AssertEventProps(oldEvent, "A", null, null, null, null, null, null, null, null, null, null, null, null);
        }

        [Test]
        public void TestPreviousLengthBatch()
        {
            String viewExpr = "select irstream Symbol as currSymbol, " +
                    "prev(0, Symbol) as prev0Symbol, " +
                    "prev(1, Symbol) as prev1Symbol, " +
                    "prev(2, Symbol) as prev2Symbol, " +
                    "prev(0, Price) as prev0Price, " +
                    "prev(1, Price) as prev1Price, " +
                    "prev(2, Price) as prev2Price, " +
                    "prevtail(0, Symbol) as prevTail0Symbol, " +
                    "prevtail(0, Price) as prevTail0Price, " +
                    "prevtail(1, Symbol) as prevTail1Symbol, " +
                    "prevtail(1, Price) as prevTail1Price, " +
                    "prevcount(Price) as prevCountPrice, " +
                    "prevwindow(Price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:length_batch(3) ";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("prev0Symbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prev0Price"));

            SendMarketEvent("A", 1);
            SendMarketEvent("B", 2);
            Assert.IsFalse(_listener.IsInvoked);

            SendMarketEvent("C", 3);
            EventBean[] newEvents = _listener.LastNewData;
            Assert.AreEqual(3, newEvents.Length);
            AssertEventProps(newEvents[0], "A", "A", 1d, null, null, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));
            AssertEventProps(newEvents[1], "B", "B", 2d, "A", 1d, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));
            AssertEventProps(newEvents[2], "C", "C", 3d, "B", 2d, "A", 1d, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d"));
            _listener.Reset();

            SendMarketEvent("D", 4);
            SendMarketEvent("E", 5);
            Assert.IsFalse(_listener.IsInvoked);

            SendMarketEvent("F", 6);
            newEvents = _listener.LastNewData;
            EventBean[] oldEvents = _listener.LastOldData;
            Assert.AreEqual(3, newEvents.Length);
            Assert.AreEqual(3, oldEvents.Length);
            AssertEventProps(newEvents[0], "D", "D", 4d, null, null, null, null, "D", 4d, "E", 5d, 3L, SplitDoubles("6d,5d,4d"));
            AssertEventProps(newEvents[1], "E", "E", 5d, "D", 4d, null, null, "D", 4d, "E", 5d, 3L, SplitDoubles("6d,5d,4d"));
            AssertEventProps(newEvents[2], "F", "F", 6d, "E", 5d, "D", 4d, "D", 4d, "E", 5d, 3L, SplitDoubles("6d,5d,4d"));
            AssertEventProps(oldEvents[0], "A", null, null, null, null, null, null, null, null, null, null, null, null);
            AssertEventProps(oldEvents[1], "B", null, null, null, null, null, null, null, null, null, null, null, null);
            AssertEventProps(oldEvents[2], "C", null, null, null, null, null, null, null, null, null, null, null, null);
        }

        [Test]
        public void TestPreviousLengthWindowWhere()
        {
            String viewExpr = "select prev(2, Symbol) as currSymbol " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:length(100) " +
                    "where prev(2, Price) > 100";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            SendMarketEvent("A", 1);
            SendMarketEvent("B", 130);
            SendMarketEvent("C", 10);
            Assert.IsFalse(_listener.IsInvoked);
            SendMarketEvent("D", 5);
            Assert.AreEqual("B", _listener.AssertOneGetNewAndReset().Get("currSymbol"));
        }

        [Test]
        public void TestPreviousLengthWindowDynamic()
        {
            String viewExpr = "select prev(IntPrimitive, TheString) as sPrev " +
                    "from " + typeof(SupportBean).FullName + ".win:length(100)";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            SendBeanEvent("A", 1);
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("sPrev"));

            SendBeanEvent("B", 0);
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("B", theEvent.Get("sPrev"));

            SendBeanEvent("C", 2);
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("A", theEvent.Get("sPrev"));

            SendBeanEvent("D", 1);
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("C", theEvent.Get("sPrev"));

            SendBeanEvent("E", 4);
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("A", theEvent.Get("sPrev"));
        }

        [Test]
        public void TestPreviousSortWindow()
        {
            String viewExpr = "select Symbol as currSymbol, " +
                    " prev(0, Symbol) as prev0Symbol, " +
                    " prev(1, Symbol) as prev1Symbol, " +
                    " prev(2, Symbol) as prev2Symbol, " +
                    " prev(0, Price) as prev0Price, " +
                    " prev(1, Price) as prev1Price, " +
                    " prev(2, Price) as prev2Price, " +
                    " prevtail(0, Symbol) as prevTail0Symbol, " +
                    " prevtail(0, Price) as prevTail0Price, " +
                    " prevtail(1, Symbol) as prevTail1Symbol, " +
                    " prevtail(1, Price) as prevTail1Price, " +
                    " prevcount(Price) as prevCountPrice, " +
                    " prevwindow(Price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".ext:sort(100, Symbol asc)";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("prev0Symbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prev0Price"));

            SendMarketEvent("COX", 30);
            AssertNewEvents("COX", "COX", 30d, null, null, null, null, "COX", 30d, null, null, 1L, SplitDoubles("30d"));

            SendMarketEvent("IBM", 45);
            AssertNewEvents("IBM", "COX", 30d, "IBM", 45d, null, null, "IBM", 45d, "COX", 30d, 2L, SplitDoubles("30d,45d"));

            SendMarketEvent("MSFT", 33);
            AssertNewEvents("MSFT", "COX", 30d, "IBM", 45d, "MSFT", 33d, "MSFT", 33d, "IBM", 45d, 3L, SplitDoubles("30d,45d,33d"));

            SendMarketEvent("XXX", 55);
            AssertNewEvents("XXX", "COX", 30d, "IBM", 45d, "MSFT", 33d, "XXX", 55d, "MSFT", 33d, 4L, SplitDoubles("30d,45d,33d,55d"));

            SendMarketEvent("CXX", 56);
            AssertNewEvents("CXX", "COX", 30d, "CXX", 56d, "IBM", 45d, "XXX", 55d, "MSFT", 33d, 5L, SplitDoubles("30d,56d,45d,33d,55d"));

            SendMarketEvent("GE", 1);
            AssertNewEvents("GE", "COX", 30d, "CXX", 56d, "GE", 1d, "XXX", 55d, "MSFT", 33d, 6L, SplitDoubles("30d,56d,1d,45d,33d,55d"));

            SendMarketEvent("AAA", 1);
            AssertNewEvents("AAA", "AAA", 1d, "COX", 30d, "CXX", 56d, "XXX", 55d, "MSFT", 33d, 7L, SplitDoubles("1d,30d,56d,1d,45d,33d,55d"));
        }

        [Test]
        public void TestPreviousExtTimedBatch()
        {
            String[] fields = 
                    "currSymbol,prev0Symbol,prev0Price,prev1Symbol,prev1Price,prev2Symbol,prev2Price,prevTail0Symbol,prevTail0Price,prevTail1Symbol,prevTail1Price,prevCountPrice,prevWindowPrice".Split(',');
            String viewExpr =   "select irstream symbol as currSymbol, " +
                    "prev(0, symbol) as prev0Symbol, " +
                    "prev(0, price) as prev0Price, " +
                    "prev(1, symbol) as prev1Symbol, " +
                    "prev(1, price) as prev1Price, " +
                    "prev(2, symbol) as prev2Symbol, " +
                    "prev(2, price) as prev2Price," +
                    "prevtail(0, symbol) as prevTail0Symbol, " +
                    "prevtail(0, price) as prevTail0Price, " +
                    "prevtail(1, symbol) as prevTail1Symbol, " +
                    "prevtail(1, price) as prevTail1Price, " +
                    "prevcount(price) as prevCountPrice, " +
                    "prevwindow(price) as prevWindowPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:ext_timed_batch(volume, 10, 0L) ";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            SendMarketEvent("A", 1, 1000);
            SendMarketEvent("B", 2, 1001);
            SendMarketEvent("C", 3, 1002);
            SendMarketEvent("D", 4, 10000);

            EPAssertionUtil.AssertPropsPerRow(_listener.AssertInvokedAndReset(), fields,
                    new Object[][] {
                            new object[] {"A", "A", 1d, null, null, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d")},
                            new object[] {"B", "B", 2d, "A", 1d, null, null, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d")},
                            new object[] {"C", "C", 3d, "B", 2d, "A", 1d, "A", 1d, "B", 2d, 3L, SplitDoubles("3d,2d,1d")}
                    },
                    null);

            SendMarketEvent("E", 5, 20000);

            EPAssertionUtil.AssertPropsPerRow(_listener.AssertInvokedAndReset(), fields,
                    new Object[][] {
                            new object[] {"D", "D", 4d, null, null, null, null, "D", 4d, null, null, 1L, SplitDoubles("4d")},
                    },
                    new Object[][] {
                            new object[] {"A", null, null, null, null, null, null, null, null, null, null, null, null},
                            new object[] {"B", null, null, null, null, null, null, null, null, null, null, null, null},
                            new object[] {"C", null, null, null, null, null, null, null, null, null, null, null, null},
                    }
                    );
        }

        [Test]
        public void TestInvalid()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));

            TryInvalid("select prev(0, average) " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:length(100).stat:uni(Price)",
                    "Error starting statement: Previous function requires a single data window view onto the stream [select prev(0, average) from com.espertech.esper.support.bean.SupportMarketDataBean.win:length(100).stat:uni(Price)]");

            TryInvalid("select count(*) from SupportBean.win:keepall() where prev(0, IntPrimitive) = 5",
                    "Error starting statement: The 'prev' function may not occur in the where-clause or having-clause of a statement with aggregations as 'previous' does not provide remove stream data; Use the 'first','last','window' or 'count' aggregation functions instead [select count(*) from SupportBean.win:keepall() where prev(0, IntPrimitive) = 5]");

            TryInvalid("select count(*) from SupportBean.win:keepall() having prev(0, IntPrimitive) = 5",
                    "Error starting statement: The 'prev' function may not occur in the where-clause or having-clause of a statement with aggregations as 'previous' does not provide remove stream data; Use the 'first','last','window' or 'count' aggregation functions instead [select count(*) from SupportBean.win:keepall() having prev(0, IntPrimitive) = 5]");
        }

        private void TryInvalid(String statement, String expectedError)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(statement);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                // expected
                Assert.AreEqual(expectedError, ex.Message);
            }
        }

        private void AssertEventWTail(EventBean eventBean,
                                      string currSymbol,
                                      string prevSymbol,
                                      double? prevPrice,
                                      string prevTailSymbol,
                                      double? prevTailPrice,
                                      string prevTail1Symbol,
                                      double? prevTail1Price,
                                      long? prevcount,
                                      object[] prevwindow)
        {
            Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
            Assert.AreEqual(prevSymbol, eventBean.Get("prevSymbol"));
            Assert.AreEqual(prevPrice, eventBean.Get("prevPrice"));
            Assert.AreEqual(prevTailSymbol, eventBean.Get("prevTailSymbol"));
            Assert.AreEqual(prevTailPrice, eventBean.Get("prevTailPrice"));
            Assert.AreEqual(prevTail1Symbol, eventBean.Get("prevTail1Symbol"));
            Assert.AreEqual(prevTail1Price, eventBean.Get("prevTail1Price"));
            Assert.AreEqual(prevcount, eventBean.Get("prevCountPrice"));
            EPAssertionUtil.AssertEqualsExactOrder(eventBean.Get("prevWindowPrice").UnwrapIntoArray<object>(), prevwindow);
        }

        private void AssertNewEvents(string currSymbol,
                                     string prev0Symbol,
                                     double? prev0Price,
                                     string prev1Symbol,
                                     double? prev1Price,
                                     string prev2Symbol,
                                     double? prev2Price,
                                     string prevTail0Symbol,
                                     double? prevTail0Price,
                                     string prevTail1Symbol,
                                     double? prevTail1Price,
                                     long? prevCount,
                                     object[] prevWindow)
        {
            EventBean[] oldData = _listener.LastOldData;
            EventBean[] newData = _listener.LastNewData;

            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);
            AssertEventProps(newData[0], currSymbol, prev0Symbol, prev0Price, prev1Symbol, prev1Price, prev2Symbol, prev2Price,
                    prevTail0Symbol, prevTail0Price, prevTail1Symbol, prevTail1Price, prevCount, prevWindow);

            _listener.Reset();
        }

        private void AssertEventProps(EventBean eventBean,
                                      string currSymbol,
                                      string prev0Symbol,
                                      double? prev0Price,
                                      string prev1Symbol,
                                      double? prev1Price,
                                      string prev2Symbol,
                                      double? prev2Price,
                                      string prevTail0Symbol,
                                      double? prevTail0Price,
                                      string prevTail1Symbol,
                                      double? prevTail1Price,
                                      long? prevCount,
                                      object[] prevWindow)
        {
            Assert.AreEqual(currSymbol, eventBean.Get("currSymbol"));
            Assert.AreEqual(prev0Symbol, eventBean.Get("prev0Symbol"));
            Assert.AreEqual(prev0Price, eventBean.Get("prev0Price"));
            Assert.AreEqual(prev1Symbol, eventBean.Get("prev1Symbol"));
            Assert.AreEqual(prev1Price, eventBean.Get("prev1Price"));
            Assert.AreEqual(prev2Symbol, eventBean.Get("prev2Symbol"));
            Assert.AreEqual(prev2Price, eventBean.Get("prev2Price"));
            Assert.AreEqual(prevTail0Symbol, eventBean.Get("prevTail0Symbol"));
            Assert.AreEqual(prevTail0Price, eventBean.Get("prevTail0Price"));
            Assert.AreEqual(prevTail1Symbol, eventBean.Get("prevTail1Symbol"));
            Assert.AreEqual(prevTail1Price, eventBean.Get("prevTail1Price"));
            Assert.AreEqual(prevCount, eventBean.Get("prevCountPrice"));
            EPAssertionUtil.AssertEqualsExactOrder(eventBean.Get("prevWindowPrice").UnwrapIntoArray<object>(), prevWindow);

            _listener.Reset();
        }

        private void SendTimer(long timeInMSec)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendMarketEvent(String symbol, double price)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, 0L, null);
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendMarketEvent(String symbol, double price, long volume)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, volume, null);
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendBeanEvent(String stringValue)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendBeanEvent(String stringValue, int intPrimitive)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = stringValue;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }

        private void AssertNewEventWTail(string currSymbol,
                                         string prevSymbol,
                                         double? prevPrice,
                                         string prevTailSymbol,
                                         double? prevTailPrice,
                                         string prevTail1Symbol,
                                         double? prevTail1Price,
                                         long? prevcount,
                                         object[] prevwindow)
        {
            EventBean[] oldData = _listener.LastOldData;
            EventBean[] newData = _listener.LastNewData;

            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);

            AssertEventWTail(newData[0], currSymbol, prevSymbol, prevPrice, prevTailSymbol, prevTailPrice, prevTail1Symbol, prevTail1Price, prevcount, prevwindow);

            _listener.Reset();
        }

        private void AssertOldEventWTail(string currSymbol,
                                         string prevSymbol,
                                         double? prevPrice,
                                         string prevTailSymbol,
                                         double? prevTailPrice,
                                         string prevTail1Symbol,
                                         double? prevTail1Price,
                                         long? prevcount,
                                         object[] prevwindow)
        {
            EventBean[] oldData = _listener.LastOldData;
            EventBean[] newData = _listener.LastNewData;

            Assert.IsNull(newData);
            Assert.AreEqual(1, oldData.Length);

            AssertEventWTail(oldData[0], currSymbol, prevSymbol, prevPrice, prevTailSymbol, prevTailPrice, prevTail1Symbol, prevTail1Price, prevcount, prevwindow);

            _listener.Reset();
        }

        private void AssertPerGroup(String statement)
        {
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(statement);
            selectTestView.Events += _listener.Update;

            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrice"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevPrevPrice"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevTail0Price"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("prevTail1Price"));
            Assert.AreEqual(typeof(long?), selectTestView.EventType.GetPropertyType("countPrice"));
            Assert.AreEqual(typeof(double?[]), selectTestView.EventType.GetPropertyType("windowPrice"));

            SendMarketEvent("IBM", 75);
            AssertReceived("IBM", null, null, 75d, null, 1L, SplitDoubles("75d"));

            SendMarketEvent("MSFT", 40);
            AssertReceived("MSFT", null, null, 40d, null, 1L, SplitDoubles("40d"));

            SendMarketEvent("IBM", 76);
            AssertReceived("IBM", 75d, null, 75d, 76d, 2L, SplitDoubles("76d,75d"));

            SendMarketEvent("CIC", 1);
            AssertReceived("CIC", null, null, 1d, null, 1L, SplitDoubles("1d"));

            SendMarketEvent("MSFT", 41);
            AssertReceived("MSFT", 40d, null, 40d, 41d, 2L, SplitDoubles("41d,40d"));

            SendMarketEvent("IBM", 77);
            AssertReceived("IBM", 76d, 75d, 75d, 76d, 3L, SplitDoubles("77d,76d,75d"));

            SendMarketEvent("IBM", 78);
            AssertReceived("IBM", 77d, 76d, 75d, 76d, 4L, SplitDoubles("78d,77d,76d,75d"));

            SendMarketEvent("CIC", 2);
            AssertReceived("CIC", 1d, null, 1d, 2d, 2L, SplitDoubles("2d,1d"));

            SendMarketEvent("MSFT", 42);
            AssertReceived("MSFT", 41d, 40d, 40d, 41d, 3L, SplitDoubles("42d,41d,40d"));

            SendMarketEvent("CIC", 3);
            AssertReceived("CIC", 2d, 1d, 1d, 2d, 3L, SplitDoubles("3d,2d,1d"));
        }

        private void AssertReceived(string symbol,
                                    double? prevPrice,
                                    double? prevPrevPrice,
                                    double? prevTail1Price,
                                    double? prevTail2Price,
                                    long? countPrice,
                                    object[] windowPrice)
        {
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            AssertReceived(theEvent, symbol, prevPrice, prevPrevPrice, prevTail1Price, prevTail2Price, countPrice, windowPrice);
        }

        private void AssertReceived(EventBean theEvent,
                                    string symbol,
                                    double? prevPrice,
                                    double? prevPrevPrice,
                                    double? prevTail0Price,
                                    double? prevTail1Price,
                                    long? countPrice,
                                    object[] windowPrice)
        {
            Assert.AreEqual(symbol, theEvent.Get("Symbol"));
            Assert.AreEqual(prevPrice, theEvent.Get("prevPrice"));
            Assert.AreEqual(prevPrevPrice, theEvent.Get("prevPrevPrice"));
            Assert.AreEqual(prevTail0Price, theEvent.Get("prevTail0Price"));
            Assert.AreEqual(prevTail1Price, theEvent.Get("prevTail1Price"));
            Assert.AreEqual(countPrice, theEvent.Get("countPrice"));
            EPAssertionUtil.AssertEqualsExactOrder(windowPrice, theEvent.Get("windowPrice").UnwrapIntoArray<object>());
        }

        private void AssertCountAndPrice(EventBean theEvent, long? total, double? price)
        {
            Assert.AreEqual(total, theEvent.Get("total"));
            Assert.AreEqual(price, theEvent.Get("firstPrice"));
        }

        // Don't remove me, I'm dynamically referenced by EPL
        public static int? IntToLong(long? longValue)
        {
            if (longValue == null)
            {
                return null;
            }
            else
            {
                return (int)longValue.GetValueOrDefault();
            }
        }

        private static Object[] SplitDoubles(string doubleList)
        {
            var doubles = doubleList.Split(',');
            var result = new Object[doubles.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = DoubleValue.ParseString(doubles[i]);
            }
            return result;
        }
    }
}
