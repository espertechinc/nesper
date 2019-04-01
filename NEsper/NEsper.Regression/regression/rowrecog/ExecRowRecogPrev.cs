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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.rowrecog;


using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogPrev : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MyEvent", typeof(SupportRecogBean));
            configuration.AddEventType("MyDeleteEvent", typeof(SupportBean));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionTimeWindowUnpartitioned(epService);
            RunAssertionTimeWindowPartitioned(epService);
            RunAssertionTimeWindowPartitionedSimple(epService);
            RunAssertionPartitionBy2FieldsKeepall(epService);
            RunAssertionUnpartitionedKeepAll(epService);
        }
    
        private void RunAssertionTimeWindowUnpartitioned(EPServiceProvider epService) {
            SendTimer(0, epService);
            string[] fields = "a_string,b_string".Split(',');
            string text = "select * from MyEvent#time(5) " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, B.TheString as b_string" +
                    "  all matches pattern (A B) " +
                    "  define " +
                    "    A as PREV(A.TheString, 3) = 'P3' and PREV(A.TheString, 2) = 'P2' and PREV(A.TheString, 4) = 'P4' and Math.Abs(Prev(A.value, 0)) >= 0," +
                    "    B as B.value in (PREV(B.value, 4), PREV(B.value, 2))" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(1000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P1", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P3", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P4", 4));
            SendTimer(2000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 3));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            SendTimer(3000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P4", 11));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P3", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", 13));
            SendTimer(4000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("xx", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", -1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 12));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E2", "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E3"}});
    
            SendTimer(5000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P4", 21));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P3", 22));
            SendTimer(6000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", 23));
            epService.EPRuntime.SendEvent(new SupportRecogBean("xx", -2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", -1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", -2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E5", "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E3"}, new object[] {"E5", "E6"}});
    
            SendTimer(8500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E3"}, new object[] {"E5", "E6"}});
    
            SendTimer(9500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E5", "E6"}});
    
            SendTimer(10500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E5", "E6"}});
    
            SendTimer(11500, epService);
            Assert.IsFalse(stmt.HasFirst());
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeWindowPartitioned(EPServiceProvider epService) {
            SendTimer(0, epService);
            string[] fields = "cat,a_string,b_string".Split(',');
            string text = "select * from MyEvent#time(5) " +
                    "match_recognize (" +
                    "  partition by cat" +
                    "  measures A.cat as cat, A.TheString as a_string, B.TheString as b_string" +
                    "  all matches pattern (A B) " +
                    "  define " +
                    "    A as PREV(A.TheString, 3) = 'P3' and PREV(A.TheString, 2) = 'P2' and PREV(A.TheString, 4) = 'P4'," +
                    "    B as B.value in (PREV(B.value, 4), PREV(B.value, 2))" +
                    ") order by cat";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(1000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P4", "c2", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P3", "c1", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", "c2", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("xx", "c1", 4));
            SendTimer(2000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", "c1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", "c1", 3));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            SendTimer(3000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P4", "c1", 11));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P3", "c1", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", "c1", 13));
            SendTimer(4000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("xx", "c1", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", "c1", -1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", "c1", 12));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"c1", "E2", "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"c1", "E2", "E3"}});
    
            SendTimer(5000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P4", "c2", 21));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P3", "c2", 22));
            SendTimer(6000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", "c2", 23));
            epService.EPRuntime.SendEvent(new SupportRecogBean("xx", "c2", -2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", "c2", -1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", "c2", -2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"c2", "E5", "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"c1", "E2", "E3"}, new object[] {"c2", "E5", "E6"}});
    
            SendTimer(8500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"c1", "E2", "E3"}, new object[] {"c2", "E5", "E6"}});
    
            SendTimer(9500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"c2", "E5", "E6"}});
    
            SendTimer(10500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"c2", "E5", "E6"}});
    
            SendTimer(11500, epService);
            Assert.IsFalse(stmt.HasFirst());
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeWindowPartitionedSimple(EPServiceProvider epService) {
            SendTimer(0, epService);
            string[] fields = "a_string".Split(',');
            string text = "select * from MyEvent#time(5 sec) " +
                    "match_recognize (" +
                    "  partition by cat " +
                    "  measures A.cat as cat, A.TheString as a_string" +
                    "  all matches pattern (A) " +
                    "  define " +
                    "    A as PREV(A.value) = (A.value - 1)" +
                    ") order by a_string";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(1000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", "S1", 100));
    
            SendTimer(2000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", "S3", 100));
    
            SendTimer(2500, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", "S2", 102));
    
            SendTimer(6200, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", "S1", 101));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E4"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E4"}});
    
            SendTimer(6500, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", "S3", 101));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E5"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E4"}, new object[] {"E5"}});
    
            SendTimer(7000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", "S1", 102));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}});
    
            SendTimer(10000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", "S2", 103));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E7"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", "S2", 102));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", "S1", 101));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", "S2", 104));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", "S1", 105));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(11199, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E4"}, new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}});
    
            SendTimer(11200, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E5"}, new object[] {"E6"}, new object[] {"E7"}});
    
            SendTimer(11600, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E6"}, new object[] {"E7"}});
    
            SendTimer(16000, epService);
            Assert.IsFalse(stmt.HasFirst());
    
            stmt.Dispose();
        }
    
        private void RunAssertionPartitionBy2FieldsKeepall(EPServiceProvider epService) {
            string[] fields = "a_string,a_cat,a_value,b_value".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  partition by TheString, cat" +
                    "  measures A.TheString as a_string, A.cat as a_cat, A.value as a_value, B.value as b_value " +
                    "  all matches pattern (A B) " +
                    "  define " +
                    "    A as (A.value > PREV(A.value))," +
                    "    B as (B.value > PREV(B.value))" +
                    ") order by a_string, a_cat";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", "T1", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T1", 110));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", "T2", 21));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", "T1", 7));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T1", 111));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", "T2", 20));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T1", 110));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T2", 1000));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T2", 1001));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", null, 9));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", "T1", 9));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"S1", "T1", 7, 9}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"S1", "T1", 7, 9}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T2", 1001));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T1", 109));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", "T2", 25));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"S1", "T1", 7, 9}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T2", 1002));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T2", 1003));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"S2", "T2", 1002, 1003}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"S1", "T1", 7, 9}, new object[] {"S2", "T2", 1002, 1003}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", "T2", 28));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"S1", "T2", 25, 28}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"S1", "T1", 7, 9}, new object[] {"S1", "T2", 25, 28}, new object[] {"S2", "T2", 1002, 1003}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnpartitionedKeepAll(EPServiceProvider epService) {
            string[] fields = "a_string".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string" +
                    "  all matches pattern (A) " +
                    "  define A as (A.value > PREV(A.value))" +
                    ") " +
                    "order by a_string";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 3));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 4));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E5"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3"}, new object[] {"E5"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3"}, new object[] {"E5"}, new object[] {"E6"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 9));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 4));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3"}, new object[] {"E5"}, new object[] {"E6"}});
    
            stmt.Stop();
    
            text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string" +
                    "  all matches pattern (A) " +
                    "  define A as (PREV(A.value, 2) = 5)" +
                    ") " +
                    "order by a_string";
    
            stmt = epService.EPAdministrator.CreateEPL(text);
            listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 4));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 5));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E7"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3"}, new object[] {"E7"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E8"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3"}, new object[] {"E7"}, new object[] {"E8"}});
    
            stmt.Dispose();
        }
    
        private void SendTimer(long time, EPServiceProvider epService) {
            var theEvent = new CurrentTimeEvent(time);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
} // end of namespace
