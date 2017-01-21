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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionDelete
    {
        [Test]
        public void TestNamedWindowInSequenceDelete()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportRecogBean", typeof(SupportRecogBean));
            config.AddEventType<SupportBean>();
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportRecogBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportRecogBean");
            epService.EPAdministrator.CreateEPL(
                "on SupportBean as s delete from MyWindow as w where s.TheString = w.TheString");

            String[] fields = "a0,a1,b".Split(',');
            String text = "select * from MyWindow " +
                          "match_recognize (" +
                          "  measures A[0].TheString as a0, A[1].TheString as a1, B.TheString as b" +
                          "  pattern ( A* B ) " +
                          "  define " +
                          "    A as (A.Value = 1)," +
                          "    B as (B.Value = 2)" +
                          ")";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0)); // deletes E1
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0)); // deletes E2
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 3));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());

            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0)); // deletes E4
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E5", "E6", "E7"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"E5", "E6", "E7"}});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        // This test is for
        //  (a) on-delete of events from a named window
        //  (b) a sorted window which also posts a remove stream that is out-of-order
        // ... also termed Out-Of-Sequence Delete (OOSD).
        //
        // The test is for out-of-sequence (and in-sequence) deletes:
        //  (1) Make sure that partial pattern matches get removed
        //  (2) Make sure that PREV is handled by order-of-arrival, and is not affected (by default) by delete (versus normal ordered remove stream).
        //      Since it is impossible to make guarantees as the named window could be entirely deleted, and "prev" depth is therefore unknown.
        //
        // Prev
        //    has OOSD
        //      Update          PREV operates on original order-of-arrival; OOSD impacts matching: resequence only when partial matches deleted
        //      iterate         PREV operates on original order-of-arrival; OOSD impacts matching: iterator may present unseen-before matches after delete
        //    no OOSD
        //      Update          PREV operates on original order-of-arrival; no resequencing when in-order deleted
        //      iterate         PREV operates on original order-of-arrival
        // No-Prev
        //    has OOSD
        //      Update
        //      iterate
        //    no OOSD
        //      Update
        //      iterate

        [Test]
        public void TestNamedWindowOnDeleteOutOfSeq()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportRecogBean>("MyEvent");
            config.AddEventType("MyDeleteEvent", typeof(SupportBean));
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            epService.EPAdministrator.CreateEPL("create window MyNamedWindow.win:keepall() as MyEvent");
            epService.EPAdministrator.CreateEPL("insert into MyNamedWindow select * from MyEvent");
            epService.EPAdministrator.CreateEPL(
                "on MyDeleteEvent as d delete from MyNamedWindow w where d.IntPrimitive = w.Value");

            String[] fields = "a_string,b_string".Split(',');
            String text = "select * from MyNamedWindow " +
                          "match_recognize (" +
                          "  measures A.TheString as a_string, B.TheString as b_string" +
                          "  all matches pattern (A B) " +
                          "  define " +
                          "    A as PREV(A.TheString, 3) = 'P3' and PREV(A.TheString, 2) = 'P2' and PREV(A.TheString, 4) = 'P4'," +
                          "    B as B.Value in (PREV(B.Value, 4), PREV(B.Value, 2))" +
                          ")";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P1", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P3", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P4", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 3));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());

            epService.EPRuntime.SendEvent(new SupportRecogBean("P4", 11));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P3", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", 13));
            epService.EPRuntime.SendEvent(new SupportRecogBean("xx", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", -4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 12));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E2", "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"E2", "E3"}});

            epService.EPRuntime.SendEvent(new SupportRecogBean("P4", 21));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P3", 22));
            epService.EPRuntime.SendEvent(new SupportRecogBean("P2", 23));
            epService.EPRuntime.SendEvent(new SupportRecogBean("xx", -2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", -1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", -2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E5", "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"E2", "E3"}, new[] {"E5", "E6"}});

            // delete an PREV-referenced event: no effect as PREV is an order-of-arrival operator
            epService.EPRuntime.SendEvent(new SupportBean("D1", 21)); // delete P4 of second batch
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"E2", "E3"}, new[] {"E5", "E6"}});

            // delete an partial-match event
            epService.EPRuntime.SendEvent(new SupportBean("D2", -1)); // delete E5 of second batch
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"E2", "E3"}});

            epService.EPRuntime.SendEvent(new SupportBean("D3", 12)); // delete P3 and E3 of first batch
            Assert.IsFalse(stmt.HasFirst());

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }

        [Test]
        public void TestNamedWindowOutOfSequenceDelete()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportRecogBean", typeof(SupportRecogBean));
            config.AddEventType<SupportBean>();
            EPServiceProvider epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportRecogBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportRecogBean");
            epService.EPAdministrator.CreateEPL(
                "on SupportBean as s delete from MyWindow as w where s.TheString = w.TheString");

            String[] fields = "a0,a1,b0,b1,c".Split(',');
            String text = "select * from MyWindow " +
                          "match_recognize (" +
                          "  measures A[0].TheString as a0, A[1].TheString as a1, B[0].TheString as b0, B[1].TheString as b1, C.TheString as c" +
                          "  pattern ( A+ B* C ) " +
                          "  define " +
                          "    A as (A.Value = 1)," +
                          "    B as (B.Value = 2)," +
                          "    C as (C.Value = 3)" +
                          ")";

            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0)); // deletes E2
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E1", null, null, null, "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"E1", null, null, null, "E3"}});

            epService.EPRuntime.SendEvent(new SupportBean("E1", 0)); // deletes E1
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0)); // deletes E4

            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0)); // deletes E4
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E5", null, null, null, "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new[] {new Object[] {"E5", null, null, null, "E6"}});

            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E10", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E9", 0)); // deletes E9
            epService.EPRuntime.SendEvent(new SupportRecogBean("E12", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new[] {new Object[] {"E7", "E8", "E10", "E11", "E12"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new Object[][]
                                                          {
                                                              new Object[] {"E5", null, null, null, "E6"},
                                                              new Object[] {"E7", "E8", "E10", "E11", "E12"}
                                                          });
                // note interranking among per-event result

            epService.EPRuntime.SendEvent(new SupportRecogBean("E13", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E14", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E15", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E16", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E14", 0)); // deletes E14
            epService.EPRuntime.SendEvent(new SupportBean("E15", 0)); // deletes E15
            epService.EPRuntime.SendEvent(new SupportBean("E16", 0)); // deletes E16
            epService.EPRuntime.SendEvent(new SupportBean("E13", 0)); // deletes E17
            epService.EPRuntime.SendEvent(new SupportRecogBean("E18", 3));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                      new Object[][]
                                                          {
                                                              new Object[] {"E5", null, null, null, "E6"},
                                                              new Object[] {"E7", "E8", "E10", "E11", "E12"}
                                                          });
                // note interranking among per-event result

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    }
}
