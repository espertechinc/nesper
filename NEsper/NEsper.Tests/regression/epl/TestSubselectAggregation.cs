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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSubselectAggregation 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();        
            config.AddEventType<SupportBean>();
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            config.AddEventType("MarketData", typeof(SupportMarketDataBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestFromFilterNoDataWindow()
        {
            RunAssertionNoDataWindowUncorrelatedFullyAggregatedNoGroupBy();
            RunAssertionNoDataWindowUncorrelatedFullyAggregatedWithGroupBy();
        }

        private void RunAssertionNoDataWindowUncorrelatedFullyAggregatedNoGroupBy()
        {
            String stmtText = "select p00 as c0, (select sum(intPrimitive) from SupportBean) as c1 from S0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(_listener);
            String[] fields = "c0,c1".Split(',');

            var runtime = _epService.EPRuntime;
            runtime.SendEvent(new SupportBean_S0(1, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", null });

            runtime.SendEvent(new SupportBean("", 10));
            runtime.SendEvent(new SupportBean_S0(2, "E2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 10 });

            runtime.SendEvent(new SupportBean("", 20));
            runtime.SendEvent(new SupportBean_S0(3, "E3"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E3", 30 });

            stmt.Dispose();
        }

        private void RunAssertionNoDataWindowUncorrelatedFullyAggregatedWithGroupBy()
        {
            String stmtText = "select (select theString as c0, sum(intPrimitive) as c1 from SupportBean group by theString).take(10) as subq from S0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.AddListener(_listener);
            String[] fields = "c0,c1".Split(',');

            var runtime = _epService.EPRuntime;
            runtime.SendEvent(new SupportBean_S0(1, "E1"));
            TestSubselectAggregationGroupBy.AssertMapMultiRow("subq", _listener.AssertOneGetNewAndReset(), "c0", fields, null);

            runtime.SendEvent(new SupportBean("G1", 10));
            runtime.SendEvent(new SupportBean_S0(2, "E2"));
            TestSubselectAggregationGroupBy.AssertMapMultiRow("subq", _listener.AssertOneGetNewAndReset(), "c0", fields, new Object[][] { new Object[] { "G1", 10 } });

            runtime.SendEvent(new SupportBean("G2", 20));
            runtime.SendEvent(new SupportBean_S0(3, "E3"));
            TestSubselectAggregationGroupBy.AssertMapMultiRow("subq", _listener.AssertOneGetNewAndReset(), "c0", fields, new Object[][] { new Object[] { "G1", 10 }, new Object[] { "G2", 20 } });

            stmt.Dispose();
        }
    
        [Test]
        public void TestCorrelatedAggregationSelectEquals()
        {
            String stmtText = "select p00, " +
                    "(select sum(IntPrimitive) from SupportBean.win:keepall() where TheString = s0.P00) as sump00 " +
                    "from S0 as s0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            String[] fields = "p00,sump00".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "T1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"T1", null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("T1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "T1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"T1", 10});
    
            _epService.EPRuntime.SendEvent(new SupportBean("T1", 11));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "T1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"T1", 21});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "T2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"T2", null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("T2", -2));
            _epService.EPRuntime.SendEvent(new SupportBean("T2", -7));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "T2"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"T2", -9});
            stmt.Dispose();

            // test distinct
            fields = "TheString,c0,c1,c2,c3".Split(',');
            String viewExpr = "select TheString, " +
                    "(select count(sb.IntPrimitive) from SupportBean().win:keepall() as sb where bean.TheString = sb.TheString) as c0, " +
                    "(select count(distinct sb.IntPrimitive) from SupportBean().win:keepall() as sb where bean.TheString = sb.TheString) as c1, " +
                    "(select count(sb.IntPrimitive, true) from SupportBean().win:keepall() as sb where bean.TheString = sb.TheString) as c2, " +
                    "(select count(distinct sb.IntPrimitive, true) from SupportBean().win:keepall() as sb where bean.TheString = sb.TheString) as c3 " +
                    "from SupportBean as bean";
            stmt = _epService.EPAdministrator.CreateEPL(viewExpr);
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", 1L, 1L, 1L, 1L });

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 1L, 1L, 1L, 1L });

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 2L, 2L, 2L, 2L });

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E2", 3L, 2L, 3L, 2L });
        }
    
        [Test]
        public void TestCorrelatedAggregationWhereGreater()
        {
            String stmtText = "select p00 from S0 as s0 where id > " +
                    "(select sum(IntPrimitive) from SupportBean.win:keepall() where TheString = s0.P00)";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            RunAssertionCorrAggWhereGreater();
    
            stmtText = "select p00 from S0 as s0 where id > " +
                    "(select sum(IntPrimitive) from SupportBean.win:keepall() where TheString||'X' = s0.P00||'X')";
            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            RunAssertionCorrAggWhereGreater();
        }
    
        private void RunAssertionCorrAggWhereGreater() {
            String[] fields = "p00".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "T1"));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("T1", 10));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "T1"));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "T1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"T1"});
    
            _epService.EPRuntime.SendEvent(new SupportBean("T1", 11));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(21, "T1"));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(22, "T1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"T1"});
        }
    
        [Test]
        public void TestPriceMap()
        {
            String stmtText = "select * from MarketData " +
                    "where Price > (select max(Price) from MarketData(symbol='GOOG').std:lastevent()) ";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendEventMD("GOOG", 1);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEventMD("GOOG", 2);
            Assert.IsFalse(_listener.IsInvoked);
    
            Object theEvent = SendEventMD("IBM", 3);
            Assert.AreEqual(theEvent, _listener.AssertOneGetNewAndReset().Underlying);
        }
    
        [Test]
        public void TestCorrelatedPropertiesSelected()
        {
            String stmtText = "select (select s0.id + max(s1.id) from S1.win:length(3) as s1) as value from S0 as s0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendEventS0(1);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(100);
            SendEventS0(2);
            Assert.AreEqual(102, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(30);
            SendEventS0(3);
            Assert.AreEqual(103, _listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        [Test]
        public void TestExists()
        {
            String stmtText = "select id from S0 where exists (select max(id) from S1.win:length(3))";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendEventS0(1);
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("id"));
    
            SendEventS1(100);
            SendEventS0(2);
            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("id"));
        }
    
        [Test]
        public void TestIn()
        {
            String stmtText = "select id from S0 where id in (select max(id) from S1.win:length(2))";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendEventS0(1);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEventS1(100);
            SendEventS0(2);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEventS0(100);
            Assert.AreEqual(100, _listener.AssertOneGetNewAndReset().Get("id"));
    
            SendEventS0(200);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEventS1(-1);
            SendEventS1(-1);
            SendEventS0(-1);
            Assert.AreEqual(-1, _listener.AssertOneGetNewAndReset().Get("id"));
        }
    
        [Test]
        public void TestMaxUnfiltered()
        {
            String stmtText = "select (select max(id) from S1.win:length(3)) as value from S0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
            
            SendEventS0(1);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(100);
            SendEventS0(2);
            Assert.AreEqual(100, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(200);
            SendEventS0(3);
            Assert.AreEqual(200, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(190);
            SendEventS0(4);
            Assert.AreEqual(200, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(180);
            SendEventS0(5);
            Assert.AreEqual(200, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(170);   // note event leaving window
            SendEventS0(6);
            Assert.AreEqual(190, _listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        [Test]
        public void TestAvgMaxStopStart()
        {
            String stmtText = "select (select avg(id) + max(id) from S1.win:length(3)) as value from S0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            SendEventS0(1);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(100);
            SendEventS0(2);
            Assert.AreEqual(200.0, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(200);
            SendEventS0(3);
            Assert.AreEqual(350.0, _listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Stop();
            SendEventS1(10000);
            SendEventS0(4);
            Assert.IsFalse(_listener.IsInvoked);
            stmt.Start();
    
            SendEventS1(10);
            SendEventS0(5);
            Assert.AreEqual(20.0, _listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        [Test]
        public void TestSumFilteredEvent()
        {
            String stmtText = "select (select sum(id) from S1(id < 0).win:length(3)) as value from S0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            RunAssertionSumFilter();
        }
    
        [Test]
        public void TestSumFilteredWhere()
        {
            String stmtText = "select (select sum(id) from S1.win:length(3) where id < 0) as value from S0";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            RunAssertionSumFilter();
        }
    
        [Test]
        public void TestInvalid()
        {
            TryInvalid("", "Unexpected end-of-input []");
    
            String stmtText = "select (select sum(s0.id) from S1.win:length(3) as s1) as value from S0 as s0";
            TryInvalid(stmtText, "Error starting statement: Failed to plan subquery number 1 querying S1: Subselect aggregation functions cannot aggregate across correlated properties [select (select sum(s0.id) from S1.win:length(3) as s1) as value from S0 as s0]");
    
            stmtText = "select (select s1.id + sum(s1.id) from S1.win:length(3) as s1) as value from S0 as s0";
            TryInvalid(stmtText, "Error starting statement: Failed to plan subquery number 1 querying S1: Subselect properties must all be within aggregation functions [select (select s1.id + sum(s1.id) from S1.win:length(3) as s1) as value from S0 as s0]");
    
            stmtText = "select (select sum(s0.id + s1.id) from S1.win:length(3) as s1) as value from S0 as s0";
            TryInvalid(stmtText, "Error starting statement: Failed to plan subquery number 1 querying S1: Subselect aggregation functions cannot aggregate across correlated properties [select (select sum(s0.id + s1.id) from S1.win:length(3) as s1) as value from S0 as s0]");
        }
    
        private void TryInvalid(String stmtText, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                SupportMessageAssertUtil.AssertMessage(ex, message);
            }
        }
        
        private void RunAssertionSumFilter()
        {
            SendEventS0(1);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(1);
            SendEventS0(2);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(0);
            SendEventS0(3);
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(-1);
            SendEventS0(4);
            Assert.AreEqual(-1, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(-3);
            SendEventS0(5);
            Assert.AreEqual(-4, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(-5);
            SendEventS0(6);
            Assert.AreEqual(-9, _listener.AssertOneGetNewAndReset().Get("value"));
    
            SendEventS1(-2);   // note event leaving window
            SendEventS0(6);
            Assert.AreEqual(-10, _listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        private void SendEventS0(int id)
        {
            _epService.EPRuntime.SendEvent(new SupportBean_S0(id));
        }
    
        private void SendEventS1(int id)
        {
            _epService.EPRuntime.SendEvent(new SupportBean_S1(id));
        }
    
        private Object SendEventMD(String symbol, double price)
        {
            Object theEvent = new SupportMarketDataBean(symbol, price, 0L, "");
            _epService.EPRuntime.SendEvent(theEvent);
            return theEvent;
        }
    }
}
