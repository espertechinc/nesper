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
using com.espertech.esper.collection;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestGroupByEventPerGroup
    {
        private const String SYMBOL_DELL = "DELL";
        private const String SYMBOL_IBM = "IBM";

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = true;
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
        public void TestCriteriaByDotMethod()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            String epl = "select sb.get_TheString() as c0, sum(intPrimitive) as c1 from SupportBean.win:length_batch(2) as sb group by sb.get_TheString()";
            _epService.EPAdministrator.CreateEPL(epl).AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "c0,c1".Split(','), new Object[] {"E1", 30});
        }

        [Test]
        public void TestUnboundStreamIterate()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();

            // with output snapshot
            String[] fields = "c0,c1".Split(',');
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString output snapshot every 3 events");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { "E1", 10 } });
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { "E1", 10 }, new Object[] { "E2", 20 } });
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { "E1", 21 }, new Object[] { "E2", 20 } });
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "E1", 21 }, new Object[] { "E2", 20 } });

            _epService.EPRuntime.SendEvent(new SupportBean("E0", 30));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { "E1", 21 }, new Object[] { "E2", 20 }, new Object[] { "E0", 30 } });
            Assert.IsFalse(_listener.IsInvoked);

            stmt.Dispose();

            // with order-by
            stmt = _epService.EPAdministrator.CreateEPL("select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
                    "output snapshot every 3 events order by TheString asc");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { "E1", 21 }, new Object[] { "E2", 20 } });
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "E1", 21 }, new Object[] { "E2", 20 } });

            _epService.EPRuntime.SendEvent(new SupportBean("E0", 30));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { "E0", 30 }, new Object[] { "E1", 21 }, new Object[] { "E2", 20 } });
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("E3", 40));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { "E0", 30 }, new Object[] { "E1", 21 }, new Object[] { "E2", 20 }, new Object[] { "E3", 40 } });
            Assert.IsFalse(_listener.IsInvoked);

            stmt.Dispose();

            // test un-grouped case
            stmt = _epService.EPAdministrator.CreateEPL("select null as c0, sum(IntPrimitive) as c1 from SupportBean output snapshot every 3 events");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { null, 10 } });
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { null, 30 } });
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 11));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { null, 41 } });
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { null, 41 } });

            stmt.Dispose();

            // test reclaim
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            stmt = _epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=1,reclaim_group_freq=1') select TheString as c0, sum(IntPrimitive) as c1 from SupportBean group by TheString " +
                    "output snapshot every 3 events");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1500));
            _epService.EPRuntime.SendEvent(new SupportBean("E0", 11));

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1800));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 12));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "E1", 10 }, new Object[] { "E0", 11 }, new Object[] { "E2", 12 } });
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { "E1", 10 }, new Object[] { "E0", 11 }, new Object[] { "E2", 12 } });

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2200));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 13));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][] { new Object[] { "E0", 11 }, new Object[] { "E2", 25 } });
        }

        [Test]
        public void TestNamedWindowDelete()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_B", typeof(SupportBean_B));
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("on SupportBean_A a delete from MyWindow w where w.TheString = a.id");
            _epService.EPAdministrator.CreateEPL("on SupportBean_B delete from MyWindow");

            String[] fields = "TheString,mysum".Split(',');
            String viewExpr = "@Hint('DISABLE_RECLAIM_GROUP') select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            RunAssertion(selectTestView, fields);

            selectTestView.Dispose();
            _epService.EPRuntime.SendEvent(new SupportBean_B("delete"));

            viewExpr = "select TheString, sum(IntPrimitive) as mysum from MyWindow group by TheString order by TheString";
            selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            RunAssertion(selectTestView, fields);
        }

        [Test]
        public void TestUnboundStreamUnlimitedKey()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }

            // ESPER-396 Unbound stream and aggregating/grouping by unlimited key (i.e. timestamp) configurable state drop
            SendTimer(0);

            // After the oldest group is 60 second old, reclaim group older then  30 seconds
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=30,reclaim_group_freq=5') select LongPrimitive, count(*) from SupportBean group by LongPrimitive");
            stmtOne.Events += _listener.Update;

            for (int i = 0; i < 1000; i++)
            {
                SendTimer(1000 + i * 1000); // reduce factor if sending more events
                SupportBean theEvent = new SupportBean();
                theEvent.LongPrimitive = (i * 1000);
                _epService.EPRuntime.SendEvent(theEvent);

                //if (i % 100000 == 0)
                //{
                //    Console.WriteLine("Sending event number " + i);
                //}
            }

            _listener.Reset();

            for (int i = 0; i < 964; i++)
            {
                SupportBean theEvent = new SupportBean();
                theEvent.LongPrimitive = (i * 1000);
                _epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(1L, _listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
            }

            for (int i = 965; i < 1000; i++)
            {
                SupportBean theEvent = new SupportBean();
                theEvent.LongPrimitive = (i * 1000);
                _epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(2L, _listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
            }

            // no frequency provided
            _epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive");
            _epService.EPRuntime.SendEvent(new SupportBean());

            _epService.EPAdministrator.CreateEPL("create variable int myAge = 10");
            _epService.EPAdministrator.CreateEPL("create variable int myFreq = 10");

            stmtOne.Dispose();
            stmtOne = _epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=myAge,reclaim_group_freq=myFreq') select LongPrimitive, count(*) from SupportBean group by LongPrimitive");
            stmtOne.Events += _listener.Update;

            for (int i = 0; i < 1000; i++)
            {
                SendTimer(2000000 + 1000 + i * 1000); // reduce factor if sending more events
                SupportBean theEvent = new SupportBean();
                theEvent.LongPrimitive = (i * 1000);
                _epService.EPRuntime.SendEvent(theEvent);

                if (i == 500)
                {
                    _epService.EPRuntime.SetVariableValue("myAge", 60);
                    _epService.EPRuntime.SetVariableValue("myFreq", 90);
                }

                /*
                if (i % 100000 == 0)
                {
                    Console.WriteLine("Sending event number " + i);
                }
                */
            }

            _listener.Reset();

            for (int i = 0; i < 900; i++)
            {
                SupportBean theEvent = new SupportBean();
                theEvent.LongPrimitive = (i * 1000);
                _epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(1L, _listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
            }

            for (int i = 900; i < 1000; i++)
            {
                SupportBean theEvent = new SupportBean();
                theEvent.LongPrimitive = (i * 1000);
                _epService.EPRuntime.SendEvent(theEvent);
                Assert.AreEqual(2L, _listener.AssertOneGetNewAndReset().Get("count(*)"), "Failed at " + i);
            }

            stmtOne.Dispose();

            // invalid tests
            TryInvalid("@Hint('reclaim_group_aged=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Error starting statement: Failed to parse hint parameter value 'xyz' as a double-typed seconds value or variable name [@Hint('reclaim_group_aged=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
            TryInvalid("@Hint('reclaim_group_aged=30,reclaim_group_freq=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Error starting statement: Failed to parse hint parameter value 'xyz' as a double-typed seconds value or variable name [@Hint('reclaim_group_aged=30,reclaim_group_freq=xyz') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
            _epService.EPAdministrator.Configuration.AddVariable("MyVar", typeof(string), "");
            TryInvalid("@Hint('reclaim_group_aged=MyVar') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Error starting statement: Variable type of variable 'MyVar' is not numeric [@Hint('reclaim_group_aged=MyVar') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");
            TryInvalid("@Hint('reclaim_group_aged=-30,reclaim_group_freq=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive",
                    "Error starting statement: Hint parameter value '-30' is an invalid value, expecting a double-typed seconds value or variable name [@Hint('reclaim_group_aged=-30,reclaim_group_freq=30') select LongPrimitive, count(*) from SupportBean group by LongPrimitive]");

            /*
            epService = EPServiceProviderManager.GetProvider(this.GetType().FullName);
            epService.EPAdministrator.Configuration.AddEventType("SupportBean", typeof(SupportBean));
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL("@Hint('reclaim_group_aged=1,reclaim_group_freq=1') select LongPrimitive, count(*) from SupportBean group by LongPrimitive");
    
            int count = 0;
            while(true)
            {
                SupportBean event = new SupportBean();
                event.LongPrimitive = Environment.TickCount;
                epService.EPRuntime.SendEvent(new SupportBean());
                count++;
                if (count % 100000 == 0)
                {
                    Console.WriteLine("Sending event number " + count);
                }
            }
            */
        }

        private void RunAssertion(EPStatement selectTestView, String[] fields)
        {
            _epService.EPRuntime.SendEvent(new SupportBean("A", 100));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "A", 100 });

            _epService.EPRuntime.SendEvent(new SupportBean("B", 20));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "B", 20 });

            _epService.EPRuntime.SendEvent(new SupportBean("A", 101));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "A", 201 });

            _epService.EPRuntime.SendEvent(new SupportBean("B", 21));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "B", 41 });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "A", 201 }, new Object[] { "B", 41 } });

            _epService.EPRuntime.SendEvent(new SupportBean_A("A"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "A", null });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "B", 41 } });

            _epService.EPRuntime.SendEvent(new SupportBean("A", 102));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "A", 102 });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "A", 102 }, new Object[] { "B", 41 } });

            _epService.EPRuntime.SendEvent(new SupportBean_A("B"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "B", null });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "A", 102 } });

            _epService.EPRuntime.SendEvent(new SupportBean("B", 22));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "B", 22 });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "A", 102 }, new Object[] { "B", 22 } });
        }

        [Test]
        public void TestAggregateGroupedProps()
        {
            // test for ESPER-185
            String[] fields = "mycount".Split(',');
            String viewExpr = "select irstream count(Price) as mycount " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
                    "group by Price";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            SendEvent(SYMBOL_DELL, 10);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { 0L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { 1L } });
            _listener.Reset();

            SendEvent(SYMBOL_DELL, 11);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { 0L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { 1L }, new Object[] { 1L } });
            _listener.Reset();

            SendEvent(SYMBOL_IBM, 10);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { 2L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { 1L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { 2L }, new Object[] { 1L } });
            _listener.Reset();
        }

        [Test]
        public void TestAggregateGroupedPropsPerGroup()
        {
            // test for ESPER-185
            String[] fields = "mycount".Split(',');
            String viewExpr = "select irstream count(Price) as mycount " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
                    "group by Symbol, Price";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            SendEvent(SYMBOL_DELL, 10);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { 0L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { 1L } });
            _listener.Reset();

            SendEvent(SYMBOL_DELL, 11);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { 0L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { 1L }, new Object[] { 1L } });
            _listener.Reset();

            SendEvent(SYMBOL_DELL, 10);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { 2L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { 1L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { 2L }, new Object[] { 1L } });
            _listener.Reset();

            SendEvent(SYMBOL_IBM, 10);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { 0L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { 2L }, new Object[] { 1L }, new Object[] { 1L } });
            _listener.Reset();
        }

        [Test]
        public void TestAggregationOverGroupedProps()
        {
            // test for ESPER-185
            String[] fields = "Symbol,Price,mycount".Split(',');
            String viewExpr = "select irstream Symbol,Price,count(Price) as mycount " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:length(5) " +
                    "group by Symbol, Price order by Symbol asc";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            SendEvent(SYMBOL_DELL, 10);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "DELL", 10.0, 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "DELL", 10.0, 0L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 10.0, 1L } });
            _listener.Reset();

            SendEvent(SYMBOL_DELL, 11);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "DELL", 11.0, 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "DELL", 11.0, 0L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 10.0, 1L }, new Object[] { "DELL", 11.0, 1L } });
            _listener.Reset();

            SendEvent(SYMBOL_DELL, 10);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "DELL", 10.0, 2L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "DELL", 10.0, 1L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 10.0, 2L }, new Object[] { "DELL", 11.0, 1L } });
            _listener.Reset();

            SendEvent(SYMBOL_IBM, 5);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "IBM", 5.0, 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "IBM", 5.0, 0L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 10.0, 2L }, new Object[] { "DELL", 11.0, 1L }, new Object[] { "IBM", 5.0, 1L } });
            _listener.Reset();

            SendEvent(SYMBOL_IBM, 5);
            Assert.AreEqual(1, _listener.LastNewData.Length);
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "IBM", 5.0, 2L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "IBM", 5.0, 1L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 10.0, 2L }, new Object[] { "DELL", 11.0, 1L }, new Object[] { "IBM", 5.0, 2L } });
            _listener.Reset();

            SendEvent(SYMBOL_IBM, 5);
            Assert.AreEqual(2, _listener.LastNewData.Length);
            EPAssertionUtil.AssertProps(_listener.LastNewData[1], fields, new Object[] { "IBM", 5.0, 3L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[1], fields, new Object[] { "IBM", 5.0, 2L });
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "DELL", 10.0, 1L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "DELL", 10.0, 2L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 11.0, 1L }, new Object[] { "DELL", 10.0, 1L }, new Object[] { "IBM", 5.0, 3L } });
            _listener.Reset();

            SendEvent(SYMBOL_IBM, 5);
            Assert.AreEqual(2, _listener.LastNewData.Length);
            EPAssertionUtil.AssertProps(_listener.LastNewData[1], fields, new Object[] { "IBM", 5.0, 4L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[1], fields, new Object[] { "IBM", 5.0, 3L });
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[] { "DELL", 11.0, 0L });
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[] { "DELL", 11.0, 1L });
            EPAssertionUtil.AssertPropsPerRow(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 10.0, 1L }, new Object[] { "IBM", 5.0, 4L } });
            _listener.Reset();
        }

        [Test]
        public void TestSumOneView()
        {
            String viewExpr = "select irstream Symbol," +
                    "sum(Price) as mySum," +
                    "avg(Price) as myAvg " +
                    "from " + typeof(SupportMarketDataBean).FullName + ".win:length(3) " +
                    "where Symbol='DELL' or Symbol='IBM' or Symbol='GE' " +
                    "group by Symbol";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            RunAssertion(selectTestView);
        }

        [Test]
        public void TestSumJoin()
        {
            String viewExpr = "select irstream Symbol," +
                    "sum(Price) as mySum," +
                    "avg(Price) as myAvg " +
                    "from " + typeof(SupportBeanString).FullName + ".win:length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + ".win:length(3) as two " +
                    "where (Symbol='DELL' or Symbol='IBM' or Symbol='GE') " +
                    "       and one.TheString = two.Symbol " +
                    "group by Symbol";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            _epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
            _epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));

            RunAssertion(selectTestView);
        }

        [Test]
        public void TestUniqueInBatch()
        {
            String stmtOne = "insert into MyStream select Symbol, Price from " +
                    typeof(SupportMarketDataBean).FullName + ".win:time_batch(1 sec)";
            _epService.EPAdministrator.CreateEPL(stmtOne);
            SendTimer(0);

            String viewExpr = "select Symbol " +
                    "from MyStream.win:time_batch(1 sec).std:unique(Symbol) " +
                    "group by Symbol";

            EPStatement selectTestView = _epService.EPAdministrator.CreateEPL(viewExpr);
            selectTestView.Events += _listener.Update;

            SendEvent("IBM", 100);
            SendEvent("IBM", 101);
            SendEvent("IBM", 102);
            SendTimer(1000);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(2000);
            UniformPair<EventBean[]> received = _listener.GetDataListsFlattened();
            Assert.AreEqual("IBM", received.First[0].Get("Symbol"));
        }

        private void RunAssertion(EPStatement selectTestView)
        {
            String[] fields = new String[] { "Symbol", "mySum", "myAvg" };
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, null);

            // assert select result type
            Assert.AreEqual(typeof(string), selectTestView.EventType.GetPropertyType("Symbol"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("mySum"));
            Assert.AreEqual(typeof(double?), selectTestView.EventType.GetPropertyType("myAvg"));

            SendEvent(SYMBOL_DELL, 10);
            AssertEvents(SYMBOL_DELL,
                    null, null,
                    10d, 10d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 10d, 10d } });

            SendEvent(SYMBOL_DELL, 20);
            AssertEvents(SYMBOL_DELL,
                    10d, 10d,
                    30d, 15d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 30d, 15d } });

            SendEvent(SYMBOL_DELL, 100);
            AssertEvents(SYMBOL_DELL,
                    30d, 15d,
                    130d, 130d / 3d);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 130d, 130d / 3d } });

            SendEvent(SYMBOL_DELL, 50);
            AssertEvents(SYMBOL_DELL,
                    130d, 130 / 3d,
                    170d, 170 / 3d);    // 20 + 100 + 50
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 170d, 170d / 3d } });

            SendEvent(SYMBOL_DELL, 5);
            AssertEvents(SYMBOL_DELL,
                    170d, 170 / 3d,
                    155d, 155 / 3d);    // 100 + 50 + 5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 155d, 155d / 3d } });

            SendEvent("AAA", 1000);
            AssertEvents(SYMBOL_DELL,
                    155d, 155d / 3,
                    55d, 55d / 2);    // 50 + 5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][] { new Object[] { "DELL", 55d, 55d / 2d } });

            SendEvent(SYMBOL_IBM, 70);
            AssertEvents(SYMBOL_DELL,
                    55d, 55 / 2d,
                    5, 5,
                    SYMBOL_IBM,
                    null, null,
                    70, 70);    // Dell:5
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][]{
                    new Object[] {"DELL", 5d, 5d}, new Object[] {"IBM", 70d, 70d}
            });

            SendEvent("AAA", 2000);
            AssertEvents(SYMBOL_DELL,
                    5d, 5d,
                    null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, new Object[][]{
                    new Object[] {"IBM", 70d, 70d}
            });

            SendEvent("AAA", 3000);
            Assert.IsFalse(_listener.IsInvoked);

            SendEvent("AAA", 4000);
            AssertEvents(SYMBOL_IBM,
                    70d, 70d,
                    null, null);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(selectTestView.GetEnumerator(), fields, null);
        }

        private void AssertEvents(string symbol,
                                  double? oldSum, double? oldAvg,
                                  double? newSum, double? newAvg)
        {
            EventBean[] oldData = _listener.LastOldData;
            EventBean[] newData = _listener.LastNewData;

            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);

            Assert.AreEqual(symbol, oldData[0].Get("Symbol"));
            Assert.AreEqual(oldSum, oldData[0].Get("mySum"));
            Assert.AreEqual(oldAvg, oldData[0].Get("myAvg"));

            Assert.AreEqual(symbol, newData[0].Get("Symbol"));
            Assert.AreEqual(newSum, newData[0].Get("mySum"));
            Assert.AreEqual(newAvg, newData[0].Get("myAvg"), "newData myAvg wrong");

            _listener.Reset();
            Assert.IsFalse(_listener.IsInvoked);
        }

        private void AssertEvents(string symbolOne,
                                  double? oldSumOne, double? oldAvgOne,
                                  double newSumOne, double newAvgOne,
                                  string symbolTwo,
                                  double? oldSumTwo, double? oldAvgTwo,
                                  double newSumTwo, double newAvgTwo)
        {
            EventBean[] oldData = _listener.LastOldData;
            EventBean[] newData = _listener.LastNewData;

            Assert.AreEqual(2, oldData.Length);
            Assert.AreEqual(2, newData.Length);

            int indexOne = 0;
            int indexTwo = 1;
            if (oldData[0].Get("Symbol").Equals(symbolTwo))
            {
                indexTwo = 0;
                indexOne = 1;
            }
            Assert.AreEqual(newSumOne, newData[indexOne].Get("mySum"));
            Assert.AreEqual(newSumTwo, newData[indexTwo].Get("mySum"));
            Assert.AreEqual(oldSumOne, oldData[indexOne].Get("mySum"));
            Assert.AreEqual(oldSumTwo, oldData[indexTwo].Get("mySum"));

            Assert.AreEqual(newAvgOne, newData[indexOne].Get("myAvg"));
            Assert.AreEqual(newAvgTwo, newData[indexTwo].Get("myAvg"));
            Assert.AreEqual(oldAvgOne, oldData[indexOne].Get("myAvg"));
            Assert.AreEqual(oldAvgTwo, oldData[indexTwo].Get("myAvg"));

            _listener.Reset();
            Assert.IsFalse(_listener.IsInvoked);
        }

        private void SendEvent(String symbol, double price)
        {
            SupportMarketDataBean bean = new SupportMarketDataBean(symbol, price, 0L, null);
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendTimer(long timeInMSec)
        {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        private void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    }
}
