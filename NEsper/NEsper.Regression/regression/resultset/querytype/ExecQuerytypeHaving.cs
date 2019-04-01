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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeHaving : RegressionExecution {
        private const string SYMBOL_DELL = "DELL";

        public override void Configure(Configuration configuration) {
            base.Configure(configuration);
            configuration.AddImport(typeof(Math));
        }

        public override void Run(EPServiceProvider epService) {
            RunAssertionHavingWildcardSelect(epService);
            RunAssertionStatementOM(epService);
            RunAssertionStatement(epService);
            RunAssertionStatementJoin(epService);
            RunAssertionSumHavingNoAggregatedProp(epService);
            RunAssertionNoAggregationJoinHaving(epService);
            RunAssertionNoAggregationJoinWhere(epService);
            RunAssertionSubstreamSelectHaving(epService);
            RunAssertionHavingSum(epService);
            RunAssertionHavingSumIStream(epService);
        }
    
        private void RunAssertionHavingWildcardSelect(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string epl = "select * " +
                    "from SupportBean#length_batch(2) " +
                    "where IntPrimitive>0 " +
                    "having count(*)=2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            stmt.Dispose();
        }
    
        private void RunAssertionStatementOM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("symbol", "price")
                .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
                .Add(Expressions.Avg("price"), "avgPrice");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName)
                .AddView("length", Expressions.Constant(5)));
            model.HavingClause = Expressions.Lt(Expressions.Property("price"), Expressions.Avg("price"));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string epl = "select irstream symbol, price, avg(price) as avgPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "having price<avg(price)";
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertion(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionStatement(EPServiceProvider epService) {
            string epl = "select irstream symbol, price, avg(price) as avgPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "having price < avg(price)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertion(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionStatementJoin(EPServiceProvider epService) {
            string epl = "select irstream symbol, price, avg(price) as avgPrice " +
                    "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) as two " +
                    "where one.TheString = two.symbol " +
                    "having price < avg(price)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
    
            TryAssertion(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSumHavingNoAggregatedProp(EPServiceProvider epService) {
            string epl = "select irstream symbol, price, avg(price) as avgPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) as two " +
                    "having volume < avg(price)";
            epService.EPAdministrator.CreateEPL(epl).Dispose();
        }
    
        private void RunAssertionNoAggregationJoinHaving(EPServiceProvider epService) {
            RunNoAggregationJoin(epService, "having");
        }
    
        private void RunAssertionNoAggregationJoinWhere(EPServiceProvider epService) {
            RunNoAggregationJoin(epService, "where");
        }
    
        private void RunAssertionSubstreamSelectHaving(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string stmtText = "insert into MyStream select quote.* from SupportBean#length(14) quote having avg(IntPrimitive) >= 3\n";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("abc", 2));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("abc", 2));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("abc", 3));
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean("abc", 5));
            Assert.IsTrue(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunNoAggregationJoin(EPServiceProvider epService, string filterClause) {
            string epl = "select irstream a.price as aPrice, b.price as bPrice, Math.Max(a.price, b.price) - Math.Min(a.price, b.price) as spread " +
                    "from " + typeof(SupportMarketDataBean).FullName + "(symbol='SYM1')#length(1) as a, " +
                    typeof(SupportMarketDataBean).FullName + "(symbol='SYM2')#length(1) as b " +
                    filterClause + " Math.Max(a.price, b.price) - Math.Min(a.price, b.price) >= 1.4";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendPriceEvent(epService, "SYM1", 20);
            Assert.IsFalse(listener.IsInvoked);
    
            SendPriceEvent(epService, "SYM2", 10);
            AssertNewSpreadEvent(listener, 20, 10, 10);
    
            SendPriceEvent(epService, "SYM2", 20);
            AssertOldSpreadEvent(listener, 20, 10, 10);
    
            SendPriceEvent(epService, "SYM2", 20);
            SendPriceEvent(epService, "SYM2", 20);
            SendPriceEvent(epService, "SYM1", 20);
            Assert.IsFalse(listener.IsInvoked);
    
            SendPriceEvent(epService, "SYM1", 18.7);
            Assert.IsFalse(listener.IsInvoked);
    
            SendPriceEvent(epService, "SYM2", 20);
            Assert.IsFalse(listener.IsInvoked);
    
            SendPriceEvent(epService, "SYM1", 18.5);
            AssertNewSpreadEvent(listener, 18.5, 20, 1.5d);
    
            SendPriceEvent(epService, "SYM2", 16);
            AssertOldNewSpreadEvent(listener, 18.5, 20, 1.5d, 18.5, 16, 2.5d);
    
            SendPriceEvent(epService, "SYM1", 12);
            AssertOldNewSpreadEvent(listener, 18.5, 16, 2.5d, 12, 16, 4);
    
            stmt.Dispose();
        }
    
        private void AssertOldNewSpreadEvent(SupportUpdateListener listener, double oldaprice, double oldbprice, double oldspread,
                                             double newaprice, double newbprice, double newspread) {
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual(1, listener.NewDataList.Count);   // since event null is put into the list
            Assert.AreEqual(1, listener.LastNewData.Length);
    
            EventBean oldEvent = listener.LastOldData[0];
            EventBean newEvent = listener.LastNewData[0];
    
            CompareSpreadEvent(oldEvent, oldaprice, oldbprice, oldspread);
            CompareSpreadEvent(newEvent, newaprice, newbprice, newspread);
    
            listener.Reset();
        }
    
        private void AssertOldSpreadEvent(SupportUpdateListener listener, double aprice, double bprice, double spread) {
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.AreEqual(1, listener.LastOldData.Length);
            Assert.AreEqual(1, listener.NewDataList.Count);   // since event null is put into the list
            Assert.IsNull(listener.LastNewData);
    
            EventBean theEvent = listener.LastOldData[0];
    
            CompareSpreadEvent(theEvent, aprice, bprice, spread);
            listener.Reset();
        }
    
        private void AssertNewSpreadEvent(SupportUpdateListener listener, double aprice, double bprice, double spread) {
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.AreEqual(1, listener.LastNewData.Length);
            Assert.AreEqual(1, listener.OldDataList.Count);
            Assert.IsNull(listener.LastOldData);
    
            EventBean theEvent = listener.LastNewData[0];
            CompareSpreadEvent(theEvent, aprice, bprice, spread);
            listener.Reset();
        }
    
        private void CompareSpreadEvent(EventBean theEvent, double aprice, double bprice, double spread) {
            Assert.AreEqual(aprice, theEvent.Get("aPrice"));
            Assert.AreEqual(bprice, theEvent.Get("bPrice"));
            Assert.AreEqual(spread, theEvent.Get("spread"));
        }
    
        private void SendPriceEvent(EPServiceProvider epService, string symbol, double price) {
            epService.EPRuntime.SendEvent(new SupportMarketDataBean(symbol, price, -1L, null));
        }
    
        private void TryAssertion(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("price").GetBoxedType());
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("avgPrice").GetBoxedType());
    
            SendEvent(epService, SYMBOL_DELL, 10);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, SYMBOL_DELL, 5);
            AssertNewEvents(listener, SYMBOL_DELL, 5d, 7.5d);
    
            SendEvent(epService, SYMBOL_DELL, 15);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, SYMBOL_DELL, 8);  // avg = (10 + 5 + 15 + 8) / 4 = 38/4=9.5
            AssertNewEvents(listener, SYMBOL_DELL, 8d, 9.5d);
    
            SendEvent(epService, SYMBOL_DELL, 10);  // avg = (10 + 5 + 15 + 8 + 10) / 5 = 48/5=9.5
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, SYMBOL_DELL, 6);  // avg = (5 + 15 + 8 + 10 + 6) / 5 = 44/5=8.8
            // no old event posted, old event falls above current avg price
            AssertNewEvents(listener, SYMBOL_DELL, 6d, 8.8d);
    
            SendEvent(epService, SYMBOL_DELL, 12);  // avg = (15 + 8 + 10 + 6 + 12) / 5 = 51/5=10.2
            AssertOldEvents(listener, SYMBOL_DELL, 5d, 10.2d);
        }
    
        private void RunAssertionHavingSum(EPServiceProvider epService) {
            string epl = "select irstream sum(myEvent.IntPrimitive) as mysum from pattern [every myEvent=" + typeof(SupportBean).FullName +
                    "] having sum(myEvent.IntPrimitive) = 2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, 1);
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("mysum"));
    
            SendEvent(epService, 1);
            Assert.AreEqual(2, listener.AssertOneGetOldAndReset().Get("mysum"));
        }
    
        private void RunAssertionHavingSumIStream(EPServiceProvider epService) {
            string epl = "select istream sum(myEvent.IntPrimitive) as mysum from pattern [every myEvent=" + typeof(SupportBean).FullName +
                    "] having sum(myEvent.IntPrimitive) = 2";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, 1);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, 1);
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("mysum"));
    
            SendEvent(epService, 1);
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void AssertNewEvents(SupportUpdateListener listener, string symbol,
                                     double? newPrice, double? newAvgPrice
        ) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(oldData);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbol, newData[0].Get("symbol"));
            Assert.AreEqual(newPrice, newData[0].Get("price"));
            Assert.AreEqual(newAvgPrice, newData[0].Get("avgPrice"));
    
            listener.Reset();
        }
    
        private void AssertOldEvents(SupportUpdateListener listener, string symbol,
                                     double? oldPrice, double? oldAvgPrice
        ) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.IsNull(newData);
            Assert.AreEqual(1, oldData.Length);
    
            Assert.AreEqual(symbol, oldData[0].Get("symbol"));
            Assert.AreEqual(oldPrice, oldData[0].Get("price"));
            Assert.AreEqual(oldAvgPrice, oldData[0].Get("avgPrice"));
    
            listener.Reset();
        }
    
        private void SendEvent(EPServiceProvider epService, int intPrimitive) {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
