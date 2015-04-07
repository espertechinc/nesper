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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.rowregex;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    [TestFixture]
    public class TestRowPatternRecognitionOps
    {
        // Out-list
        //  - SUBSET
        //  - ORDER BY
        //  - MATCH_NUMBER
        //  - WINDOW clause is implied
        //  - aggregation support in DEFINE clauses, running aggregates
        //  - ^(begin partition), $(end partition), {n}, {n,}, {n,m}, {,m}, {n,m}?)
        //  - ALL ROWS PER MATCH: one row for each row of each match (is not the default) (then Measure selection can include all properties of EventType) and CLASSIFIER (4 hrs)
        //  - excluding portions of a pattern {-NOTA-}
        //  - PERMUTE
        //  - fire and forget query support
        //  - TO_TIMESTAMP
        //  - AFTER MATCH SKIP TO FIRST/LAST <variable>
    
        [Test]
        public void TestConcatenation()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }
    
            var fields = "a_string,b_string".Split(',');
            var text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, B.TheString as b_string " +
                    "  all matches " +
                    "  pattern (A B) " +
                    "  define B as B.value > A.value" +
                    ") " +
                    "order by a_string, b_string";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 3));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E2", "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 4));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E4", "E5"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", "E3"}, new Object[] {"E4", "E5"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 10));
            Assert.IsFalse(listener.IsInvoked);      // E5-E6 not a match since "skip past last row"
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", "E3"}, new Object[] {"E4", "E5"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 9));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 4));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", "E3"}, new Object[] {"E4", "E5"}});
    
            stmt.Stop();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestZeroToMany()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            var fields = "a_string,b0_string,b1_string,b2_string,c_string".Split(',');
            var text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, " +
                    "    B[0].TheString as b0_string, " +
                    "    B[1].TheString as b1_string, " +
                    "    B[2].TheString as b2_string, " +
                    "    C.TheString as c_string" +
                    "  all matches " +
                    "  pattern (A B* C) " +
                    "  define \n" +
                    "    A as A.value = 10,\n" +
                    "    B as B.value > 10,\n" +
                    "    C as C.value < 10\n" +
                    ") " +
                    "order by a_string, c_string";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 10));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E2", null, null, null, "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", null, null, null, "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 12));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", null, null, null, "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E4", "E5", null, null, "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", null, null, null, "E3"}, new Object[] {"E4", "E5", null, null, "E6"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E10", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 9));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E7", "E8", "E9", "E10", "E11"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", null, null, null, "E3"}, new Object[] {"E4", "E5", null, null, "E6"}, new Object[] {"E7", "E8", "E9", "E10", "E11"}});
    
            stmt.Stop();
    
            // Zero-to-many unfiltered
            var epl = "select * from MyEvent match_recognize (" +
                    "measures A as a, B as b, C as c " +
                    "pattern (A C*? B) " +
                    "define " +
                    "A as Typeof(A) = 'MyEventTypeA'," +
                    "B as Typeof(B) = 'MyEventTypeB'" +
                    ")";
            epService.EPAdministrator.CreateEPL(epl);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestOneToMany()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            var fields = "a_string,b0_string,b1_string,b2_string,c_string".Split(',');
            var text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, " +
                    "    B[0].TheString as b0_string, " +
                    "    B[1].TheString as b1_string, " +
                    "    B[2].TheString as b2_string, " +
                    "    C.TheString as c_string" +
                    "  all matches " +
                    "  pattern (A B+ C) " +
                    "  define \n" +
                    "    A as (A.value = 10),\n" +
                    "    B as (B.value > 10),\n" +
                    "    C as (C.value < 10)\n" +
                    ") " +
                    "order by a_string, c_string";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 8));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 12));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E4", "E5", null, null, "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E4", "E5", null, null, "E6"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E10", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 9));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E7", "E8", "E9", "E10", "E11"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E4", "E5", null, null, "E6"}, new Object[] {"E7", "E8", "E9", "E10", "E11"}});

            stmt.Stop();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestZeroToOne()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            var fields = "a_string,b_string,c_string".Split(',');
            var text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, B.TheString as b_string, " +
                    "    C.TheString as c_string" +
                    "  all matches " +
                    "  pattern (A B? C) " +
                    "  define \n" +
                    "    A as (A.value = 10),\n" +
                    "    B as (B.value > 10),\n" +
                    "    C as (C.value < 10)\n" +
                    ") " +
                    "order by a_string";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 10));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E2", null, "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", null, "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 12));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", null, "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E4", "E5", "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", null, "E3"}, new Object[] {"E4", "E5", "E6"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 9));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E2", null, "E3"}, new Object[] {"E4", "E5", "E6"}});
    
            stmt.Stop();
    
            // test optional event not defined
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportBean_B));
    
            var epl = "select * from A match_recognize (" +
                    "measures A.id as id, B.id as b_id " +
                    "pattern (A B?) " +
                    "define " +
                    " A as Typeof(A) = 'A'" +
                    ")";
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
            
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            Assert.IsTrue(listener.IsInvoked);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestPartitionBy()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            var fields = "a_string,a_value,b_value".Split(',');
            var text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  partition by TheString" +
                    "  measures A.TheString as a_string, A.value as a_value, B.value as b_value " +
                    "  all matches pattern (A B) " +
                    "  define B as (B.value > A.value)" +
                    ")" +
                    " order by a_string";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", 6));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S3", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", -1));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"S1", 4, 6}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"S1", 4, 6}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"S4", -1, 10}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"S1", 4, 6}, new Object[] {"S4", -1, 10}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", 11));
            Assert.IsFalse(listener.IsInvoked);      // since skip past last row
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"S1", 4, 6}, new Object[] {"S4", -1, 10}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S3", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", -2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S3", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 4));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"S1", 4, 6}, new Object[] {"S4", -1, 10}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 7));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"S1", 4, 7}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"S1", 4, 6}, new Object[] {"S1", 4, 7}, new Object[] {"S4", -1, 10}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", 12));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"S4", -2, 12}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"S1", 4, 6}, new Object[] {"S1", 4, 7}, new Object[] {"S4", -1, 10}, new Object[] {"S4", -2, 12}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 7));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 5));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"S2", 4, 5}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"S1", 4, 6}, new Object[] {"S1", 4, 7}, new Object[] {"S2", 4, 5}, new Object[] {"S4", -1, 10}, new Object[] {"S4", -2, 12}});

            stmt.Dispose();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestUnlimitedPartition()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            var text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  partition by value" +
                    "  measures A.TheString as a_string " +
                    "  pattern (A B) " +
                    "  define " +
                    "    A as (A.TheString = 'A')," +
                    "    B as (B.TheString = 'B')" +
                    ")";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            for (var i = 0; i < 5 * RegexPartitionStateRepoGroup.INITIAL_COLLECTION_MIN; i++)
            {
                epService.EPRuntime.SendEvent(new SupportRecogBean("A", i));
                epService.EPRuntime.SendEvent(new SupportRecogBean("B", i));
                Assert.IsTrue(listener.GetAndClearIsInvoked());
            }

            for (var i = 0; i < 5 * RegexPartitionStateRepoGroup.INITIAL_COLLECTION_MIN; i++)
            {
                epService.EPRuntime.SendEvent(new SupportRecogBean("A", i + 100000));
            }
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            for (var i = 0; i < 5 * RegexPartitionStateRepoGroup.INITIAL_COLLECTION_MIN; i++)
            {
                epService.EPRuntime.SendEvent(new SupportRecogBean("B", i + 100000));
                Assert.IsTrue(listener.GetAndClearIsInvoked());
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestConcatWithinAlter()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            var fields = "a_string,b_string,c_string,d_string".Split(',');
            var text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, B.TheString as b_string, C.TheString as c_string, D.TheString as d_string " +
                    "  all matches pattern ( A B | C D ) " +
                    "  define " +
                    "    A as (A.value = 1)," +
                    "    B as (B.value = 2)," +
                    "    C as (C.value = 3)," +
                    "    D as (D.value = 4)" +
                    ")";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 3));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 4));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {null, null, "E4", "E5"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {null, null, "E4", "E5"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E1", "E2", null, null}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {null, null, "E4", "E5"}, new Object[] {"E1", "E2", null, null}});

            stmt.Stop();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestAlterWithinConcat()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            var fields = "a_string,b_string,c_string,d_string".Split(',');
            var text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, B.TheString as b_string, C.TheString as c_string, D.TheString as d_string " +
                    "  all matches pattern ( (A | B) (C | D) ) " +
                    "  define " +
                    "    A as (A.value = 1)," +
                    "    B as (B.value = 2)," +
                    "    C as (C.value = 3)," +
                    "    D as (D.value = 4)" +
                    ")";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 1));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E5", null, "E6", null}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E5", null, "E6", null}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {null, "E7", "E8", null}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E5", null, "E6", null}, new Object[] {null, "E7", "E8", null}});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestVariableMoreThenOnce()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("MyEvent", typeof(SupportRecogBean));
            var epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName); }

            var fields = "a0,b,a1".Split(',');
            var text = "select * from MyEvent.win:keepall() " +
                    "match_recognize (" +
                    "  measures A[0].TheString as a0, B.TheString as b, A[1].TheString as a1 " +
                    "  all matches pattern ( A B A ) " +
                    "  define " +
                    "    A as (A.value = 1)," +
                    "    B as (B.value = 2)" +
                    ")";
    
            var stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 2));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E5", "E6", "E7"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E5", "E6", "E7"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", 1));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E5", "E6", "E7"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E10", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new Object[][]{new Object[] {"E9", "E10", "E11"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new Object[][]{new Object[] {"E5", "E6", "E7"}, new Object[] {"E9", "E10", "E11"}});

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestRegex()
        {
            Assert.IsTrue("aq".Matches("^aq|^id"));
            Assert.IsTrue("id".Matches("^aq|^id"));
            Assert.IsTrue("ad".Matches("a(q|i)?d"));
            Assert.IsTrue("aqd".Matches("a(q|i)?d"));
            Assert.IsTrue("aid".Matches("a(q|i)?d"));
            Assert.IsFalse("aed".Matches("a(q|i)?d"));
            Assert.IsFalse("a".Matches("(a(b?)c)?"));
        }
    }
}
