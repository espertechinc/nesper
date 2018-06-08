///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewExternallyBatched : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType(typeof(MyEvent));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionExtBatchedNoReference(epService);
            RunAssertionExtBatchedWithRefTime(epService);
        }
    
        private void RunAssertionExtBatchedNoReference(EPServiceProvider epService) {
            string[] fields = "id".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select irstream * from MyEvent#ext_timed_batch(mytimestamp, 1 minute)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E1", "8:00:00.000"));
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E2", "8:00:30.000"));
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E3", "8:00:59.999"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E4", "8:01:00.000"));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}}, (object[][]) null);
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E5", "8:01:02.000"));
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E6", "8:01:05.000"));
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E7", "8:02:00.000"));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}}, new object[][] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E8", "8:03:59.000"));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{new object[] {"E7"}}, new object[][] {new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E9", "8:03:59.000"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E10", "8:04:00.000"));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{new object[] {"E8"}, new object[] {"E9"}}, new object[][] {new object[] {"E7"}});
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E11", "8:06:30.000"));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{new object[] {"E10"}}, new object[][] {new object[] {"E8"}, new object[] {"E9"}});
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E12", "8:06:59.999"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E13", "8:07:00.001"));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{new object[] {"E11"}, new object[] {"E12"}}, new object[][] {new object[] {"E10"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionExtBatchedWithRefTime(EPServiceProvider epService) {
    
            string epl = "select irstream * from MyEvent#ext_timed_batch(mytimestamp, 1 minute, 5000)";
            TryAssertionWithRefTime(epService, epl);
    
            epl = "select irstream * from MyEvent#ext_timed_batch(mytimestamp, 1 minute, 65000)";
            TryAssertionWithRefTime(epService, epl);
        }
    
        private void TryAssertionWithRefTime(EPServiceProvider epService, string epl) {
            string[] fields = "id".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E1", "8:00:00.000"));
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E2", "8:00:04.999"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E3", "8:00:05.000"));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{new object[] {"E1"}, new object[] {"E2"}}, null);
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E4", "8:00:04.000"));
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E5", "7:00:00.000"));
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E6", "8:01:04.999"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E7", "8:01:05.000"));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}}, new object[][] {new object[] {"E1"}, new object[] {"E2"}});
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E8", "8:03:55.000"));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{new object[] {"E7"}}, new object[][] {new object[] {"E3"}, new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});
    
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E9", "0:00:00.000"));
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E10", "8:04:04.999"));
            epService.EPRuntime.SendEvent(MyEvent.MakeTime("E11", "8:04:05.000"));
            EPAssertionUtil.AssertPropsPerRow(listener.AssertInvokedAndReset(), fields,
                    new object[][]{new object[] {"E8"}, new object[] {"E9"}, new object[] {"E10"}}, new object[][] {new object[] {"E7"}});
    
            stmt.Dispose();
        }
    
        [Serializable]
        public class MyEvent  {
            private string id;
            private long mytimestamp;
    
            public MyEvent(string id, long mytimestamp) {
                this.id = id;
                this.mytimestamp = mytimestamp;
            }
    
            public static MyEvent MakeTime(string id, string mytime) {
                long msec = DateTimeParser.ParseDefaultMSec("2002-05-1T" + mytime);
                return new MyEvent(id, msec);
            }
    
            public string GetId() {
                return id;
            }
    
            public long GetMytimestamp() {
                return mytimestamp;
            }
        }
    }
} // end of namespace
