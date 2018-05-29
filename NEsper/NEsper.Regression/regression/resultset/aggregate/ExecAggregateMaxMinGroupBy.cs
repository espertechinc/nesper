///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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
    public class ExecAggregateMaxMinGroupBy : RegressionExecution {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionMinMax(epService);
            RunAssertionMinMax_OM(epService);
            RunAssertionMinMaxView_Compile(epService);
            RunAssertionMinMaxJoin(epService);
            RunAssertionMinNoGroupHaving(epService);
            RunAssertionMinNoGroupSelectHaving(epService);
        }
    
        private void RunAssertionMinMax(EPServiceProvider epService) {
            string epl = "select irstream symbol, " +
                    "min(all volume) as minVol," +
                    "max(all volume) as maxVol," +
                    "min(distinct volume) as minDistVol," +
                    "max(distinct volume) as maxDistVol" +
                    " from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                    "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionMinMax(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionMinMax_OM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create()
                    .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH)
                    .Add("symbol")
                    .Add(Expressions.Min("volume"), "minVol")
                    .Add(Expressions.Max("volume"), "maxVol")
                    .Add(Expressions.MinDistinct("volume"), "minDistVol")
                    .Add(Expressions.MaxDistinct("volume"), "maxDistVol");
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportMarketDataBean).FullName).AddView("length", Expressions.Constant(3)));
            model.WhereClause = Expressions.Or()
                    .Add(Expressions.Eq("symbol", "DELL"))
                    .Add(Expressions.Eq("symbol", "IBM"))
                    .Add(Expressions.Eq("symbol", "GE"));
            model.GroupByClause = GroupByClause.Create("symbol");
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string epl = "select irstream symbol, " +
                    "min(volume) as minVol, " +
                    "max(volume) as maxVol, " +
                    "min(distinct volume) as minDistVol, " +
                    "max(distinct volume) as maxDistVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                    "where symbol=\"DELL\" or symbol=\"IBM\" or symbol=\"GE\" " +
                    "group by symbol";
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionMinMax(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionMinMaxView_Compile(EPServiceProvider epService) {
            string epl = "select irstream symbol, " +
                    "min(volume) as minVol, " +
                    "max(volume) as maxVol, " +
                    "min(distinct volume) as minDistVol, " +
                    "max(distinct volume) as maxDistVol " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(3) " +
                    "where symbol=\"DELL\" or symbol=\"IBM\" or symbol=\"GE\" " +
                    "group by symbol";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionMinMax(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionMinMaxJoin(EPServiceProvider epService) {
            string epl = "select irstream symbol, " +
                    "min(volume) as minVol," +
                    "max(volume) as maxVol," +
                    "min(distinct volume) as minDistVol," +
                    "max(distinct volume) as maxDistVol" +
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
    
            TryAssertionMinMax(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionMinNoGroupHaving(EPServiceProvider epService) {
            string stmtText = "select symbol from " + typeof(SupportMarketDataBean).FullName + "#time(5 sec) " +
                    "having volume > min(volume) * 1.3";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "DELL", 100L);
            SendEvent(epService, "DELL", 105L);
            SendEvent(epService, "DELL", 100L);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "DELL", 131L);
            Assert.AreEqual("DELL", listener.AssertOneGetNewAndReset().Get("symbol"));
    
            SendEvent(epService, "DELL", 132L);
            Assert.AreEqual("DELL", listener.AssertOneGetNewAndReset().Get("symbol"));
    
            SendEvent(epService, "DELL", 129L);
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void RunAssertionMinNoGroupSelectHaving(EPServiceProvider epService) {
            string stmtText = "select symbol, min(volume) as mymin from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "having volume > min(volume) * 1.3";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendEvent(epService, "DELL", 100L);
            SendEvent(epService, "DELL", 105L);
            SendEvent(epService, "DELL", 100L);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "DELL", 131L);
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("DELL", theEvent.Get("symbol"));
            Assert.AreEqual(100L, theEvent.Get("mymin"));
    
            SendEvent(epService, "DELL", 132L);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("DELL", theEvent.Get("symbol"));
            Assert.AreEqual(100L, theEvent.Get("mymin"));
    
            SendEvent(epService, "DELL", 129L);
            SendEvent(epService, "DELL", 125L);
            SendEvent(epService, "DELL", 125L);
            Assert.IsFalse(listener.IsInvoked);
    
            SendEvent(epService, "DELL", 170L);
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("DELL", theEvent.Get("symbol"));
            Assert.AreEqual(125L, theEvent.Get("mymin"));
        }
    
        private void TryAssertionMinMax(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("minVol"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("maxVol"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("minDistVol"));
            Assert.AreEqual(typeof(long?), stmt.EventType.GetPropertyType("maxDistVol"));
    
            SendEvent(epService, SYMBOL_DELL, 50L);
            AssertEvents(listener, SYMBOL_DELL, null, null, null, null,
                    SYMBOL_DELL, 50L, 50L, 50L, 50L
            );
    
            SendEvent(epService, SYMBOL_DELL, 30L);
            AssertEvents(listener, SYMBOL_DELL, 50L, 50L, 50L, 50L,
                    SYMBOL_DELL, 30L, 50L, 30L, 50L
            );
    
            SendEvent(epService, SYMBOL_DELL, 30L);
            AssertEvents(listener, SYMBOL_DELL, 30L, 50L, 30L, 50L,
                    SYMBOL_DELL, 30L, 50L, 30L, 50L
            );
    
            SendEvent(epService, SYMBOL_DELL, 90L);
            AssertEvents(listener, SYMBOL_DELL, 30L, 50L, 30L, 50L,
                    SYMBOL_DELL, 30L, 90L, 30L, 90L
            );
    
            SendEvent(epService, SYMBOL_DELL, 100L);
            AssertEvents(listener, SYMBOL_DELL, 30L, 90L, 30L, 90L,
                    SYMBOL_DELL, 30L, 100L, 30L, 100L
            );
    
            SendEvent(epService, SYMBOL_IBM, 20L);
            SendEvent(epService, SYMBOL_IBM, 5L);
            SendEvent(epService, SYMBOL_IBM, 15L);
            SendEvent(epService, SYMBOL_IBM, 18L);
            AssertEvents(listener, SYMBOL_IBM, 5L, 20L, 5L, 20L,
                    SYMBOL_IBM, 5L, 18L, 5L, 18L
            );
    
            SendEvent(epService, SYMBOL_IBM, null);
            AssertEvents(listener, SYMBOL_IBM, 5L, 18L, 5L, 18L,
                    SYMBOL_IBM, 15L, 18L, 15L, 18L
            );
    
            SendEvent(epService, SYMBOL_IBM, null);
            AssertEvents(listener, SYMBOL_IBM, 15L, 18L, 15L, 18L,
                    SYMBOL_IBM, 18L, 18L, 18L, 18L
            );
    
            SendEvent(epService, SYMBOL_IBM, null);
            AssertEvents(listener, SYMBOL_IBM, 18L, 18L, 18L, 18L,
                    SYMBOL_IBM, null, null, null, null
            );
        }
    
        private void AssertEvents(
            SupportUpdateListener listener,
            string symbolOld,
            long? minVolOld, long? maxVolOld,
            long? minDistVolOld, long? maxDistVolOld,
            string symbolNew,
            long? minVolNew, long? maxVolNew, 
            long? minDistVolNew, long? maxDistVolNew)
        {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbolOld, oldData[0].Get("symbol"));
            Assert.AreEqual(minVolOld, oldData[0].Get("minVol"));
            Assert.AreEqual(maxVolOld, oldData[0].Get("maxVol"));
            Assert.AreEqual(minDistVolOld, oldData[0].Get("minDistVol"));
            Assert.AreEqual(maxDistVolOld, oldData[0].Get("maxDistVol"));
    
            Assert.AreEqual(symbolNew, newData[0].Get("symbol"));
            Assert.AreEqual(minVolNew, newData[0].Get("minVol"));
            Assert.AreEqual(maxVolNew, newData[0].Get("maxVol"));
            Assert.AreEqual(minDistVolNew, newData[0].Get("minDistVol"));
            Assert.AreEqual(maxDistVolNew, newData[0].Get("maxDistVol"));
    
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
