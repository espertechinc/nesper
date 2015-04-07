///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionPrev 
    {
        [Test]
        public void TestTimeWindowUnpartitioned()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            SendTimer(0, epService);
            var fields = "a_string,b_string".Split(',');
            var text = "select * from MyEvent.win:time(5) " +
                          "match_recognize (" +
                          "  measures A.TheString as a_string, B.TheString as b_string" +
                          "  all matches pattern (A B) " +
                          "  define " +
                          "    A as PREV(A.TheString, 3) = 'P3' and PREV(A.TheString, 2) = 'P2' and PREV(A.TheString, 4) = 'P4' and Math.Abs(prev(A.Value, 0)) >= 0," +
                          "    B as B.Value in (PREV(B.Value, 4), PREV(B.Value, 2))" +
                          ")";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
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
                    new [] { new Object[] {"E2", "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E2", "E3"}});
    
            SendTimer(5000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P4", 21));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P3", 22));
            SendTimer(6000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", 23));
            epService.EPRuntime.SendEvent(new SupportRecogBean("xx", -2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", -1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", -2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"E5", "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E2", "E3"}, new Object[] {"E5", "E6"}});
    
            SendTimer(8500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E2", "E3"}, new Object[] {"E5", "E6"}});
    
            SendTimer(9500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E5", "E6"}});
    
            SendTimer(10500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E5", "E6"}});
    
            SendTimer(11500, epService);
            Assert.IsFalse(stmt.HasFirst());

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestTimeWindowPartitioned()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            config.AddEventType("MyDeleteEvent", typeof(SupportBean));
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            SendTimer(0, epService);
            var fields = "Cat,a_string,b_string".Split(',');
            var text = "select * from MyEvent.win:time(5) " +
                    "match_recognize (" +
                    "  partition by Cat" +
                    "  measures A.Cat as Cat, A.TheString as a_string, B.TheString as b_string" +
                    "  all matches pattern (A B) " +
                    "  define " +
                    "    A as PREV(A.TheString, 3) = 'P3' and PREV(A.TheString, 2) = 'P2' and PREV(A.TheString, 4) = 'P4'," +
                    "    B as B.Value in (PREV(B.Value, 4), PREV(B.Value, 2))" +
                    ") order by Cat";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
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
                    new [] { new Object[] {"c1", "E2", "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"c1", "E2", "E3"}});
    
            SendTimer(5000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P4", "c2", 21));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P3", "c2", 22));
            SendTimer(6000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", "c2", 23));
            epService.EPRuntime.SendEvent(new SupportRecogBean("xx", "c2", -2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", "c2", -1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", "c2", -2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"c2", "E5", "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"c1", "E2", "E3"}, new Object[] {"c2", "E5", "E6"}});
    
            SendTimer(8500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"c1", "E2", "E3"}, new Object[] {"c2", "E5", "E6"}});
    
            SendTimer(9500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"c2", "E5", "E6"}});
    
            SendTimer(10500, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"c2", "E5", "E6"}});
    
            SendTimer(11500, epService);
            Assert.IsFalse(stmt.HasFirst());

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestTimeWindowPartitionedSimple()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            SendTimer(0, epService);
            var fields = "a_string".Split(',');
            var text = "select * from MyEvent.win:time(5 sec) " +
                    "match_recognize (" +
                    "  partition by Cat " +
                    "  measures A.Cat as Cat, A.TheString as a_string" +
                    "  all matches pattern (A) " +
                    "  define " +
                    "    A as PREV(A.Value) = (A.Value - 1)" +
                    ") order by a_string";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
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
                    new [] { new Object[] {"E4"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E4"}});
    
            SendTimer(6500, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", "S3", 101));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"E5"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E4"}, new Object[] {"E5"}});
    
            SendTimer(7000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", "S1", 102));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E4"}, new Object[] {"E5"}, new Object[] {"E6"}});
    
            SendTimer(10000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", "S2", 103));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"E7"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E4"}, new Object[] {"E5"}, new Object[] {"E6"}, new Object[] {"E7"}});
                    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", "S2", 102));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", "S1", 101));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", "S2", 104));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", "S1", 105));
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(11199, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E4"}, new Object[] {"E5"}, new Object[] {"E6"}, new Object[] {"E7"}});
    
            SendTimer(11200, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E5"}, new Object[] {"E6"}, new Object[] {"E7"}});
    
            SendTimer(11600, epService);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E6"}, new Object[] {"E7"}});
    
            SendTimer(16000, epService);
            Assert.IsFalse(stmt.HasFirst());

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestPartitionBy2FieldsKeepall()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            var fields = "a_string,a_cat,a_value,b_value".Split(',');
            var text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  partition by TheString, Cat" +
                    "  measures A.TheString as a_string, A.Cat as a_cat, A.Value as a_value, B.Value as b_value " +
                    "  all matches pattern (A B) " +
                    "  define " +
                    "    A as (A.Value > PREV(A.Value))," +
                    "    B as (B.Value > PREV(B.Value))" +
                    ") order by a_string, a_cat";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
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
                    new [] { new Object[] {"S1", "T1", 7, 9}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"S1", "T1", 7, 9}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T2", 1001));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T1", 109));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", "T2", 25));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"S1", "T1", 7, 9}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T2", 1002));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", "T2", 1003));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"S2", "T2", 1002, 1003}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"S1", "T1", 7, 9}, new Object[] {"S2", "T2", 1002, 1003}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", "T2", 28));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"S1", "T2", 25, 28}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"S1", "T1", 7, 9}, new Object[] {"S1", "T2", 25, 28}, new Object[] {"S2", "T2", 1002, 1003}});
    
            stmt.Dispose();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestUnpartitionedKeepAll()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            var fields = "a_string".Split(',');
            var text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string" +
                    "  all matches pattern (A) " +
                    "  define A as (A.Value > PREV(A.Value))" +
                    ") " +
                    "order by a_string";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 3));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 4));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"E5"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E3"}, new Object[] {"E5"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E3"}, new Object[] {"E5"}, new Object[] {"E6"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 9));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 4));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E3"}, new Object[] {"E5"}, new Object[] {"E6"}});
    
            stmt.Stop();
    
            text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string" +
                    "  all matches pattern (A) " +
                    "  define A as (PREV(A.Value, 2) = 5)" +
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
                    new [] { new Object[] {"E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 5));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"E7"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E3"}, new Object[] {"E7"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new [] { new Object[] {"E8"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new [] { new Object[] {"E3"}, new Object[] {"E7"}, new Object[] {"E8"}});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private static void SendTimer(long time, EPServiceProvider epService)
        {
            var theEvent = new CurrentTimeEvent(time);
            var runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }    
    }
}
