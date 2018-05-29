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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.view;


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewLengthWindowStats : RegressionExecution {
        private const string SYMBOL = "CSCO.O";
        private const string FEED = "feed1";
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionIterator(epService);
            RunAssertionWindowStats(epService);
        }
    
        private void RunAssertionIterator(EPServiceProvider epService) {
            string epl = "select symbol, price from " + typeof(SupportMarketDataBean).FullName + "#length(2)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, "ABC", 20);
            SendEvent(epService, "DEF", 100);
    
            // check iterator results
            IEnumerator<EventBean> events = statement.GetEnumerator();
            Assert.IsTrue(events.MoveNext());
            EventBean theEvent = events.Current;
            Assert.AreEqual("ABC", theEvent.Get("symbol"));
            Assert.AreEqual(20d, theEvent.Get("price"));

            Assert.IsTrue(events.MoveNext());
            theEvent = events.Current;
            Assert.AreEqual("DEF", theEvent.Get("symbol"));
            Assert.AreEqual(100d, theEvent.Get("price"));
            Assert.IsFalse(events.MoveNext());
    
            SendEvent(epService, "EFG", 50);
    
            // check iterator results
            events = statement.GetEnumerator();
            Assert.IsTrue(events.MoveNext());
            theEvent = events.Current;
            Assert.AreEqual("DEF", theEvent.Get("symbol"));
            Assert.AreEqual(100d, theEvent.Get("price"));

            Assert.IsTrue(events.MoveNext());
            theEvent = events.Current;
            Assert.AreEqual("EFG", theEvent.Get("symbol"));
            Assert.AreEqual(50d, theEvent.Get("price"));
        }
    
        private void RunAssertionWindowStats(EPServiceProvider epService) {
            string epl = "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                    "(symbol='" + SYMBOL + "')#length(3)#uni(price, symbol, feed)";
            EPStatement statement = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            listener.Reset();
    
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("average"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("variance"));
            Assert.AreEqual(typeof(long?), statement.EventType.GetPropertyType("datapoints"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("total"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("stddev"));
            Assert.AreEqual(typeof(double?), statement.EventType.GetPropertyType("stddevpa"));

            SendEvent(epService, SYMBOL, 100);
            CheckOld(listener, true, 0, 0, Double.NaN, Double.NaN, Double.NaN, Double.NaN);
            CheckNew(statement, 1, 100, 100, 0, Double.NaN, Double.NaN, listener);
    
            SendEvent(epService, SYMBOL, 100.5);
            CheckOld(listener, false, 1, 100, 100, 0, Double.NaN, Double.NaN);
            CheckNew(statement, 2, 200.5, 100.25, 0.25, 0.353553391, 0.125, listener);
    
            SendEvent(epService, "DUMMY", 100.5);
            Assert.IsTrue(listener.LastNewData == null);
            Assert.IsTrue(listener.LastOldData == null);
    
            SendEvent(epService, SYMBOL, 100.7);
            CheckOld(listener, false, 2, 200.5, 100.25, 0.25, 0.353553391, 0.125);
            CheckNew(statement, 3, 301.2, 100.4, 0.294392029, 0.360555128, 0.13, listener);
    
            SendEvent(epService, SYMBOL, 100.6);
            CheckOld(listener, false, 3, 301.2, 100.4, 0.294392029, 0.360555128, 0.13);
            CheckNew(statement, 3, 301.8, 100.6, 0.081649658, 0.1, 0.01, listener);
    
            SendEvent(epService, SYMBOL, 100.9);
            CheckOld(listener, false, 3, 301.8, 100.6, 0.081649658, 0.1, 0.01);
            CheckNew(statement, 3, 302.2, 100.733333333, 0.124721913, 0.152752523, 0.023333333, listener);
            statement.Dispose();
    
            // Test copying all properties
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            string eplWildcard = "select * from SupportBean#length(3)#uni(IntPrimitive, *)";
            statement = epService.EPAdministrator.CreateEPL(eplWildcard);
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1.0, theEvent.Get("average"));
            Assert.AreEqual("E1", theEvent.Get("TheString"));
            Assert.AreEqual(1, theEvent.Get("IntPrimitive"));
        }
    
        private void SendEvent(EPServiceProvider epService, string symbol, double price) {
            var theEvent = new SupportMarketDataBean(symbol, price, 0L, FEED);
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void CheckNew(EPStatement statement, long countE, double sumE, double avgE, double stdevpaE, double stdevE, double varianceE, SupportUpdateListener listener) {
            IEnumerator<EventBean> iterator = statement.GetEnumerator();
            Assert.IsTrue(iterator.MoveNext());
            CheckValues(iterator.Current, false, false, countE, sumE, avgE, stdevpaE, stdevE, varianceE);
            Assert.IsFalse(iterator.MoveNext());
    
            Assert.IsTrue(listener.LastNewData.Length == 1);
            EventBean childViewValues = listener.LastNewData[0];
            CheckValues(childViewValues, false, false, countE, sumE, avgE, stdevpaE, stdevE, varianceE);
    
            listener.Reset();
        }
    
        private void CheckOld(SupportUpdateListener listener, bool isFirst, long countE, double sumE, double avgE, double stdevpaE, double stdevE, double varianceE) {
            Assert.IsTrue(listener.LastOldData.Length == 1);
            EventBean childViewValues = listener.LastOldData[0];
            CheckValues(childViewValues, isFirst, false, countE, sumE, avgE, stdevpaE, stdevE, varianceE);
        }
    
        private void CheckValues(EventBean values, bool isFirst, bool isNewData, long countE, double sumE, double avgE, double stdevpaE, double stdevE, double varianceE) {
            long count = GetLongValue(ViewFieldEnum.UNIVARIATE_STATISTICS__DATAPOINTS, values);
            double sum = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__TOTAL, values);
            double avg = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__AVERAGE, values);
            double stdevpa = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEVPA, values);
            double stdev = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__STDDEV, values);
            double variance = GetDoubleValue(ViewFieldEnum.UNIVARIATE_STATISTICS__VARIANCE, values);
    
            Assert.AreEqual(count, countE);
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(sum, sumE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(avg, avgE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stdevpa, stdevpaE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(stdev, stdevE, 6));
            Assert.IsTrue(DoubleValueAssertionUtil.Equals(variance, varianceE, 6));
            if (isFirst && !isNewData) {
                Assert.AreEqual(null, values.Get("symbol"));
                Assert.AreEqual(null, values.Get("feed"));
            } else {
                Assert.AreEqual(SYMBOL, values.Get("symbol"));
                Assert.AreEqual(FEED, values.Get("feed"));
            }
        }
    
        private double GetDoubleValue(ViewFieldEnum field, EventBean values) {
            return values.Get(field.GetName()).AsDouble();
        }
    
        private long GetLongValue(ViewFieldEnum field, EventBean values) {
            return (long) values.Get(field.GetName()).AsLong();
        }
    }
} // end of namespace
