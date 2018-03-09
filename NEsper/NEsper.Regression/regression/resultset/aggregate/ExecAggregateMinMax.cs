///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

namespace com.espertech.esper.regression.resultset.aggregate
{
    public class ExecAggregateMinMax : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
    
            RunAssertionMinMaxNamedWindowWEver(epService, false);
            RunAssertionMinMaxNamedWindowWEver(epService, true);
            RunAssertionMinMaxNoDataWindowSubquery(epService);
            if (!InstrumentationHelper.ENABLED) {
                RunAssertionMemoryMinHaving(epService);
            }
        }
    
        private void RunAssertionMinMaxNamedWindowWEver(EPServiceProvider epService, bool soda) {
            string[] fields = "lower,upper,lowerever,upperever".Split(',');
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "create window NamedWindow5m#length(2) as select * from SupportBean");
            SupportModelHelper.CreateByCompileOrParse(epService, soda, "insert into NamedWindow5m select * from SupportBean");
            EPStatement stmt = SupportModelHelper.CreateByCompileOrParse(epService, soda, "select " +
                    "min(IntPrimitive) as lower, " +
                    "max(IntPrimitive) as upper, " +
                    "minever(IntPrimitive) as lowerever, " +
                    "maxever(IntPrimitive) as upperever from NamedWindow5m");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, 1, 1, 1});
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{1, 5, 1, 5});
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, 5, 1, 5});
    
            epService.EPRuntime.SendEvent(new SupportBean(null, 6));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, 6, 1, 6});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMinMaxNoDataWindowSubquery(EPServiceProvider epService) {
    
            string[] fields = "maxi,mini,max0,min0".Split(',');
            string epl = "select max(IntPrimitive) as maxi, min(IntPrimitive) as mini," +
                    "(select max(id) from S0#lastevent) as max0, (select min(id) from S0#lastevent) as min0" +
                    " from SupportBean";
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, 3, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4, 3, null, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 4));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{4, 3, 2, 2});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{5, 3, 1, 1});
    
            epService.EPAdministrator.DestroyAllStatements();
            /// <summary>
            /// Comment out here for sending many more events.
            /// 
            ///          for (int i = 0; i < 10000000; i++) {
            ///          epService.EPRuntime.SendEvent(new SupportBean(null, i));
            ///          if (i % 10000 == 0) {
            ///          Log.Info("Sent " + i + " events");
            ///          }
            ///          }
            /// </summary>
        }
    
        private void RunAssertionMemoryMinHaving(EPServiceProvider epService) {
            string statementText = "select price, min(price) as minPrice " +
                    "from " + typeof(SupportMarketDataBean).FullName + "#time(30)" +
                    "having price >= min(price) * (1.02)";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var random = new Random();
            // Change to perform a long-running tests, each loop is 1 second
            int loopcount = 2;
            int loopCount = 0;
    
            while (true) {
                Log.Info("Sending batch " + loopCount);
    
                // send events
                long startTime = DateTimeHelper.CurrentTimeMillis;
                for (int i = 0; i < 5000; i++) {
                    double price = 50 + 49 * random.Next(100) / 100.0;
                    SendEvent(epService, price);
                }
                long endTime = DateTimeHelper.CurrentTimeMillis;
    
                // sleep remainder of 1 second
                long delta = startTime - endTime;
                if (delta < 950) {
                    Thread.Sleep((int) (950 - delta));
                }
    
                listener.Reset();
                loopCount++;
                if (loopCount > loopcount) {
                    break;
                }
            }
        }
    
        private void SendEvent(EPServiceProvider epService, double price) {
            var bean = new SupportMarketDataBean("DELL", price, -1L, null);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
