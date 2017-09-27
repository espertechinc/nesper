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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionDataWin 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        [Test]
        public void TestUnboundStreamNoIterator()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            var fields = "TheString,value".Split(',');
            var text = "select * from MyEvent " +
                    "match_recognize (" +
                    "  measures A.TheString as TheString, A.Value as value" +
                    "  all matches pattern (A) " +
                    "  define " +
                    "    A as PREV(A.TheString, 1) = TheString" +
                    ")";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("s1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("s2", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("s1", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("s3", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("s2", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("s1", 6));
            Assert.IsFalse(stmt.HasFirst());
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("s1", 7));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][] { new Object[] {"s1", 7}});
            Assert.IsFalse(stmt.HasFirst());
    
            /*
              Optionally send some more events.
    
            for (int i = 0; i < 100000; i++)
            {
                epService.EPRuntime.SendEvent(new SupportRecogBean("P2", 1));
            }
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", 1));
             */

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestTimeWindow()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            SendTimer(0, epService);
            var fields = "a_string,b_string,c_string".Split(',');
            var text = "select * from MyEvent#time(5 sec) " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, B.TheString as b_string, C.TheString as c_string" +
                    "  all matches pattern ( A B C ) " +
                    "  define " +
                    "    A as (A.Value = 1)," +
                    "    B as (B.Value = 2)," +
                    "    C as (C.Value = 3)" +
                    ")";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(50, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
    
            SendTimer(1000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 2));
            Assert.IsFalse(stmt.HasFirst());
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(6000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 3));
            Assert.IsFalse(stmt.HasFirst());
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(7000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 1));
    
            SendTimer(8000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 2));
    
            SendTimer(11500, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][] { new Object[] {"E4", "E5", "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][] { new Object[] {"E4", "E5", "E6"}});
    
            SendTimer(11999, epService);
            Assert.IsTrue(stmt.HasFirst());
    
            SendTimer(12000, epService);
            Assert.IsFalse(stmt.HasFirst());
            Assert.IsFalse(listener.IsInvoked);
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestTimeBatchWindow()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            SendTimer(0, epService);
            var fields = "a_string,b_string,c_string".Split(',');
            var text = "select * from MyEvent#time_batch(5 sec) " +
                    "match_recognize (" +
                    "  partition by Cat " +
                    "  measures A.TheString as a_string, B.TheString as b_string, C.TheString as c_string" +
                    "  all matches pattern ( (A | B) C ) " +
                    "  define " +
                    "    A as A.TheString like 'A%'," +
                    "    B as B.TheString like 'B%'," +
                    "    C as C.TheString like 'C%' and C.Value in (A.Value, B.Value)" +
                    ") order by a_string";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(50, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A1", "001", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B1", "002", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B2", "002", 4));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            SendTimer(4000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("C1", "002", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("C2", "002", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B3", "003", -1));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][] { new Object[] {null, "B2", "C1"}});
    
            SendTimer(5050, epService);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][] { new Object[] {null, "B2", "C1"}});
            Assert.IsFalse(stmt.HasFirst());
    
            SendTimer(6000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("C3", "003", -1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("C4", "001", 1));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            SendTimer(10050, epService);
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            SendTimer(14000, epService);
            epService.EPRuntime.SendEvent(new SupportRecogBean("A2", "002", 0));
            epService.EPRuntime.SendEvent(new SupportRecogBean("B4", "003", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("C5", "002", 0));
            epService.EPRuntime.SendEvent(new SupportRecogBean("C6", "003", 10));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][] { new Object[] { null, "B4", "C6" }, new Object[] { "A2", null, "C5" } });
    
            SendTimer(15050, epService);
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][] { new Object[] {null, "B4", "C6"}, new Object[] {"A2", null, "C5"}});
            Assert.IsFalse(stmt.HasFirst());

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void SendTimer(long time, EPServiceProvider epService)
        {
            var theEvent = new CurrentTimeEvent(time);
            var runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    }
}
