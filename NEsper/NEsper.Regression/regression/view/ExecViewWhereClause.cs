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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewWhereClause : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionWhere(epService);
            RunAssertionWhereNumericType(epService);
        }
    
        private void RunAssertionWhere(EPServiceProvider epService) {
            string epl = "select * from " + typeof(SupportMarketDataBean).FullName + "#length(3) where symbol='CSCO'";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendMarketDataEvent(epService, "IBM");
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendMarketDataEvent(epService, "CSCO");
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            SendMarketDataEvent(epService, "IBM");
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendMarketDataEvent(epService, "CSCO");
            Assert.IsTrue(listener.GetAndClearIsInvoked());
    
            // invalid return type for filter during compilation time
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            try {
                epService.EPAdministrator.CreateEPL("select TheString From SupportBean#time(30 seconds) where IntPrimitive group by TheString");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error validating expression: The where-clause filter expression must return a boolean value [select TheString From SupportBean#time(30 seconds) where IntPrimitive group by TheString]", ex.Message);
            }
    
            // invalid return type for filter at runtime
            var dict = new Dictionary<string, Object>();
            dict.Put("criteria", typeof(bool?));
            epService.EPAdministrator.Configuration.AddEventType("MapEvent", dict);
            stmt = epService.EPAdministrator.CreateEPL("select * From MapEvent#time(30 seconds) where criteria");
    
            try {
                epService.EPRuntime.SendEvent(Collections.SingletonDataMap("criteria", 15), "MapEvent");
                Assert.Fail(); // ensure exception handler rethrows
            } catch (EPException) {
                // fine
            }
            stmt.Dispose();
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionWhereNumericType(EPServiceProvider epService) {
            string epl = "select " +
                    " IntPrimitive + LongPrimitive as p1," +
                    " IntPrimitive * DoublePrimitive as p2," +
                    " FloatPrimitive / DoublePrimitive as p3" +
                    " from " + typeof(SupportBean).FullName + "#length(3) where " +
                    "IntPrimitive=LongPrimitive and IntPrimitive=DoublePrimitive and FloatPrimitive=DoublePrimitive";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSupportBeanEvent(epService, 1, 2, 3, 4);
            Assert.IsFalse(listener.GetAndClearIsInvoked());
    
            SendSupportBeanEvent(epService, 2, 2, 2, 2);
            EventBean theEvent = listener.GetAndResetLastNewData()[0];
            Assert.AreEqual(typeof(long?), theEvent.EventType.GetPropertyType("p1"));
            Assert.AreEqual(4L, theEvent.Get("p1"));
            Assert.AreEqual(typeof(double?), theEvent.EventType.GetPropertyType("p2"));
            Assert.AreEqual(4d, theEvent.Get("p2"));
            Assert.AreEqual(typeof(double?), theEvent.EventType.GetPropertyType("p3"));
            Assert.AreEqual(1d, theEvent.Get("p3"));
    
            stmt.Dispose();
        }
    
        private void SendMarketDataEvent(EPServiceProvider epService, string symbol) {
            var theEvent = new SupportMarketDataBean(symbol, 0, 0L, "");
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, int intPrimitive, long longPrimitive, float floatPrimitive, double doublePrimitive) {
            var theEvent = new SupportBean();
            theEvent.IntPrimitive = intPrimitive;
            theEvent.LongPrimitive = longPrimitive;
            theEvent.FloatPrimitive = floatPrimitive;
            theEvent.DoublePrimitive = doublePrimitive;
            epService.EPRuntime.SendEvent(theEvent);
        }
    }
} // end of namespace
