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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateMedianAndDeviation : RegressionExecution {
        private const string SYMBOL_DELL = "DELL";
        private const string SYMBOL_IBM = "IBM";
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionStmt(epService);
            RunAssertionStmtJoin_OM(epService);
            RunAssertionStmtJoin(epService);
            RunAssertionStmt(epService);
        }
    
        private void RunAssertionStmt(EPServiceProvider epService) {
            string epl = "select irstream symbol," +
                    "median(all price) as myMedian," +
                    "median(distinct price) as myDistMedian," +
                    "stddev(all price) as myStdev," +
                    "avedev(all price) as myAvedev " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#length(5) " +
                    "where symbol='DELL' or symbol='IBM' or symbol='GE' " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            TryAssertionStmt(epService, listener, stmt);
    
            // Test NaN sensitivity
            stmt.Dispose();
            stmt = epService.EPAdministrator.CreateEPL("select stddev(price) as val from " + typeof(SupportMarketDataBean).FullName + "#length(3)");
            stmt.Events += listener.Update;
    
            SendEvent(epService, "A", Double.NaN);
            SendEvent(epService, "B", Double.NaN);
            SendEvent(epService, "C", Double.NaN);
            SendEvent(epService, "D", 1d);
            SendEvent(epService, "E", 2d);
            listener.Reset();
            SendEvent(epService, "F", 3d);
            var result = listener.AssertOneGetNewAndReset().Get("val").AsDouble();
            Assert.IsTrue(double.IsNaN(result));
    
            stmt.Dispose();
        }
    
        private void RunAssertionStmtJoin_OM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create("symbol")
                .Add(Expressions.Median("price"), "myMedian")
                .Add(Expressions.MedianDistinct("price"), "myDistMedian")
                .Add(Expressions.Stddev("price"), "myStdev")
                .Add(Expressions.Avedev("price"), "myAvedev")
                .SetStreamSelector(StreamSelector.RSTREAM_ISTREAM_BOTH);

            FromClause fromClause = FromClause.Create(
                    FilterStream.Create(typeof(SupportBeanString).FullName, "one").AddView(View.Create("length", Expressions.Constant(100))),
                    FilterStream.Create(typeof(SupportMarketDataBean).FullName, "two").AddView(View.Create("length", Expressions.Constant(5))));
            model.FromClause = fromClause;
            model.WhereClause = Expressions.And().Add(
                    Expressions.Or()
                        .Add(Expressions.Eq("symbol", "DELL"))
                        .Add(Expressions.Eq("symbol", "IBM"))
                        .Add(Expressions.Eq("symbol", "GE"))
                )
                .Add(Expressions.EqProperty("one.TheString", "two.symbol"));

            model.GroupByClause = GroupByClause.Create("symbol");
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string epl = "select irstream symbol, " +
                    "median(price) as myMedian, " +
                    "median(distinct price) as myDistMedian, " +
                    "stddev(price) as myStdev, " +
                    "avedev(price) as myAvedev " +
                    "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) as two " +
                    "where (symbol=\"DELL\" or symbol=\"IBM\" or symbol=\"GE\") " +
                    "and one.TheString=two.symbol " +
                    "group by symbol";
            Assert.AreEqual(epl, model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
            epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));
    
            TryAssertionStmt(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void RunAssertionStmtJoin(EPServiceProvider epService) {
            string epl = "select irstream symbol," +
                    "median(price) as myMedian," +
                    "median(distinct price) as myDistMedian," +
                    "stddev(price) as myStdev," +
                    "avedev(price) as myAvedev " +
                    "from " + typeof(SupportBeanString).FullName + "#length(100) as one, " +
                    typeof(SupportMarketDataBean).FullName + "#length(5) as two " +
                    "where (symbol='DELL' or symbol='IBM' or symbol='GE') " +
                    "       and one.TheString = two.symbol " +
                    "group by symbol";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_DELL));
            epService.EPRuntime.SendEvent(new SupportBeanString(SYMBOL_IBM));
            epService.EPRuntime.SendEvent(new SupportBeanString("AAA"));
    
            TryAssertionStmt(epService, listener, stmt);
    
            stmt.Dispose();
        }
    
        private void TryAssertionStmt(EPServiceProvider epService, SupportUpdateListener listener, EPStatement stmt) {
            // assert select result type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("symbol"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("myMedian"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("myDistMedian"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("myStdev"));
            Assert.AreEqual(typeof(double), stmt.EventType.GetPropertyType("myAvedev"));
    
            SendEvent(epService, SYMBOL_DELL, 10);
            AssertEvents(listener, SYMBOL_DELL,
                    null, null, null, null,
                    10d, 10d, null, 0d);
    
            SendEvent(epService, SYMBOL_DELL, 20);
            AssertEvents(listener, SYMBOL_DELL,
                    10d, 10d, null, 0d,
                    15d, 15d, 7.071067812d, 5d);
    
            SendEvent(epService, SYMBOL_DELL, 20);
            AssertEvents(listener, SYMBOL_DELL,
                    15d, 15d, 7.071067812d, 5d,
                    20d, 15d, 5.773502692, 4.444444444444444);
    
            SendEvent(epService, SYMBOL_DELL, 90);
            AssertEvents(listener, SYMBOL_DELL,
                    20d, 15d, 5.773502692, 4.444444444444444,
                    20d, 20d, 36.96845502d, 27.5d);
    
            SendEvent(epService, SYMBOL_DELL, 5);
            AssertEvents(listener, SYMBOL_DELL,
                    20d, 20d, 36.96845502d, 27.5d,
                    20d, 15d, 34.71310992d, 24.4d);
    
            SendEvent(epService, SYMBOL_DELL, 90);
            AssertEvents(listener, SYMBOL_DELL,
                    20d, 15d, 34.71310992d, 24.4d,
                    20d, 20d, 41.53311931d, 36d);
    
            SendEvent(epService, SYMBOL_DELL, 30);
            AssertEvents(listener, SYMBOL_DELL,
                    20d, 20d, 41.53311931d, 36d,
                    30d, 25d, 40.24922359d, 34.4d);
        }
    
        private void AssertEvents(SupportUpdateListener listener, string symbol,
                                  double? oldMedian, double? oldDistMedian, double? oldStdev, double? oldAvedev,
                                  double? newMedian, double? newDistMedian, double? newStdev, double? newAvedev
        ) {
            EventBean[] oldData = listener.LastOldData;
            EventBean[] newData = listener.LastNewData;
    
            Assert.AreEqual(1, oldData.Length);
            Assert.AreEqual(1, newData.Length);
    
            Assert.AreEqual(symbol, oldData[0].Get("symbol"));
            Assert.AreEqual(oldMedian, oldData[0].Get("myMedian"), "oldData.myMedian wrong");
            Assert.AreEqual(oldDistMedian, oldData[0].Get("myDistMedian"), "oldData.myDistMedian wrong");
            Assert.AreEqual(oldAvedev, oldData[0].Get("myAvedev"), "oldData.myAvedev wrong");
    
            double? oldStdevResult = (double?) oldData[0].Get("myStdev");
            if (oldStdevResult == null) {
                Assert.IsNull(oldStdev);
            } else {
                Assert.AreEqual(
                    Math.Round(oldStdev.Value * 1000), 
                    Math.Round(oldStdevResult.Value * 1000),
                    "oldData.myStdev wrong");
            }
    
            Assert.AreEqual(symbol, newData[0].Get("symbol"));
            Assert.AreEqual(newMedian, newData[0].Get("myMedian"), "newData.myMedian wrong");
            Assert.AreEqual(newDistMedian, newData[0].Get("myDistMedian"), "newData.myDistMedian wrong");
            Assert.AreEqual(newAvedev, newData[0].Get("myAvedev"), "newData.myAvedev wrong");
    
            double? newStdevResult = (double?) newData[0].Get("myStdev");
            if (newStdevResult == null) {
                Assert.IsNull(newStdev);
            } else {
                Assert.AreEqual( 
                    Math.Round(newStdev.Value * 1000), 
                    Math.Round(newStdevResult.Value * 1000),
                    "newData.myStdev wrong");
            }
    
            listener.Reset();
            Assert.IsFalse(listener.IsInvoked);
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var bean = new SupportMarketDataBean(symbol, price, 0L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
