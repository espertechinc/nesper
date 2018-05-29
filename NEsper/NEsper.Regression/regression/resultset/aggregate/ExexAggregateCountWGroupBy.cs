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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExexAggregateCountWGroupBy : RegressionExecution {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionCountOneViewOM(epService);
            RunAssertionGroupByCountNestedAggregationAvg(epService);
            RunAssertionCountOneViewCompile(epService);
            RunAssertionCountOneView(epService);
            RunAssertionCountJoin(epService);
        }
    
        private void RunAssertionCountOneViewOM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
                .Add("symbol")
                .Add(Expressions.CountStar(), "countAll")
                .Add(Expressions.CountDistinct("volume"), "countDistVol")
                .Add(Expressions.Count("volume"), "countVol");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("length", Expressions.Constant(3)));
            model.WhereClause = Expressions.Or()
                    .Add(Expressions.Eq("symbol", "DELL"))
                    .Add(Expressions.Eq("symbol", "IBM"))
                    .Add(Expressions.Eq("symbol", "GE"));
            model.GroupByClause = GroupByClause.Create("symbol");
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string epl = "select irstream symbol, " +
                    "count(*) as countAll, " +
                    "count(distinct volume) as countDistVol, " +
                    "count(volume) as countVol" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                    "where symbol=\"DELL\" or symbol=\"IBM\" or symbol=\"GE\" " +
                    "group by symbol";
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionCount(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionGroupByCountNestedAggregationAvg(EPServiceProvider epService) {
            // test for ESPER-328
            string epl = "select symbol, count(*) as cnt, avg(count(*)) as val from " + typeof(SupportMarketDataBean).FullName + "#length(3)" +
                    "group by symbol order by symbol asc";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, SYMBOL_DELL, 50L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "symbol,cnt,val".Split(','), new object[]{"DELL", 1L, 1d});
    
            SendEvent(epService, SYMBOL_DELL, 51L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "symbol,cnt,val".Split(','), new object[]{"DELL", 2L, 1.5d});
    
            SendEvent(epService, SYMBOL_DELL, 52L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "symbol,cnt,val".Split(','), new object[]{"DELL", 3L, 2d});
    
            SendEvent(epService, "IBM", 52L);
            EventBean[] events = listener.LastNewData;
            EPAssertionUtil.AssertProps(events[0], "symbol,cnt,val".Split(','), new object[]{"DELL", 2L, 2d});
            EPAssertionUtil.AssertProps(events[1], "symbol,cnt,val".Split(','), new object[]{"IBM", 1L, 1d});
            listener.Reset();
    
            SendEvent(epService, SYMBOL_DELL, 53L);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "symbol,cnt,val".Split(','), new object[]{"DELL", 2L, 2.5d});
    
            stmt.Dispose();
        }
    
        private void RunAssertionCountOneViewCompile(EPServiceProvider epService) {
            string epl = "select irstream symbol, " +
                    "count(*) as countAll, " +
                    "count(distinct volume) as countDistVol, " +
                    "count(volume) as countVol" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                    "where symbol=\"DELL\" or symbol=\"IBM\" or symbol=\"GE\" " +
                    "group by symbol";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionCount(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCountOneView(EPServiceProvider epService) {
            string epl = "select irstream symbol, " +
                    "count(*) as countAll," +
                    "count(distinct volume) as countDistVol," +
                    "count(all volume) as countVol" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                    "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionCount(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionCountJoin(EPServiceProvider epService) {
            string epl = "select irstream symbol, " +
                    "count(*) as countAll," +
                    "count(distinct volume) as countDistVol," +
                    "count(volume) as countVol " +
                    " from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + "#length(3) as two " +
                    "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                    "  and one.TheString = two.symbol " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
    
            TryAssertionCount(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void TryAssertionCount(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(long), stmt.EventType.GetPropertyType("countAll"));
            Assert.AreEqual(typeof(long), stmt.EventType.GetPropertyType("countDistVol"));
            Assert.AreEqual(typeof(long), stmt.EventType.GetPropertyType("countVol"));
    
            SendEvent(epService, SYMBOL_DELL, 50L);
            AssertEvents(listener, SYMBOL_DELL, 0L, 0L, 0L,
                    SYMBOL_DELL, 1L, 1L, 1L
            );
    
            SendEvent(epService, SYMBOL_DELL, null);
            AssertEvents(listener, SYMBOL_DELL, 1L, 1L, 1L,
                    SYMBOL_DELL, 2L, 1L, 1L
            );
    
            SendEvent(epService, SYMBOL_DELL, 25L);
            AssertEvents(listener, SYMBOL_DELL, 2L, 1L, 1L,
                    SYMBOL_DELL, 3L, 2L, 2L
            );
    
            SendEvent(epService, SYMBOL_DELL, 25L);
            AssertEvents(listener, SYMBOL_DELL, 3L, 2L, 2L,
                    SYMBOL_DELL, 3L, 1L, 2L
            );
    
            SendEvent(epService, SYMBOL_DELL, 25L);
            AssertEvents(listener, SYMBOL_DELL, 3L, 1L, 2L,
                    SYMBOL_DELL, 3L, 1L, 3L
            );
    
            SendEvent(epService, SYMBOL_IBM, 1L);
            SendEvent(epService, SYMBOL_IBM, null);
            SendEvent(epService, SYMBOL_IBM, null);
            SendEvent(epService, SYMBOL_IBM, null);
            AssertEvents(listener, SYMBOL_IBM, 3L, 1L, 1L,
                    SYMBOL_IBM, 3L, 0L, 0L
            );
        }
    
        private void AssertEvents(SupportUpdateListener listener, string symbolOld, long countAllOld, long countDistVolOld, long countVolOld,
                                  string symbolNew, long countAllNew, long countDistVolNew, long countVolNew) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbolOld, oldData[0].Get("symbol"));
            Assert.AreEqual(countAllOld, oldData[0].Get("countAll"));
            Assert.AreEqual(countDistVolOld, oldData[0].Get("countDistVol"));
            Assert.AreEqual(countVolOld, oldData[0].Get("countVol"));
    
            Assert.AreEqual(symbolNew, newData[0].Get("symbol"));
            Assert.AreEqual(countAllNew, newData[0].Get("countAll"));
            Assert.AreEqual(countDistVolNew, newData[0].Get("countDistVol"));
            Assert.AreEqual(countVolNew, newData[0].Get("countVol"));
    
            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, long? volume) {
            var bean = new SupportMarketDataBean(symbol, 0, volume, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
