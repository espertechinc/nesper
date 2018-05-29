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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.rowregex;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.rowrecog;


using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogOps : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("MyEvent", typeof(SupportRecogBean));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionConcatenation(epService);
            RunAssertionZeroToMany(epService);
            RunAssertionOneToMany(epService);
            RunAssertionZeroToOne(epService);
            RunAssertionPartitionBy(epService);
            RunAssertionUnlimitedPartition(epService);
            RunAssertionConcatWithinAlter(epService);
            RunAssertionAlterWithinConcat(epService);
            RunAssertionVariableMoreThenOnce(epService);
            RunAssertionRegex();
        }
    
        private void RunAssertionConcatenation(EPServiceProvider epService) {
            string[] fields = "a_string,b_string".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, B.TheString as b_string " +
                    "  all matches " +
                    "  pattern (A B) " +
                    "  define B as B.value > A.value" +
                    ") " +
                    "order by a_string, b_string";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 5));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 3));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E2", "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 4));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 6));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E4", "E5"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E3"}, new object[] {"E4", "E5"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 10));
            Assert.IsFalse(listener.IsInvoked);      // E5-E6 not a match since "skip past last row"
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E3"}, new object[] {"E4", "E5"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 9));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 4));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E3"}, new object[] {"E4", "E5"}});
    
            stmt.Stop();
        }
    
        private void RunAssertionZeroToMany(EPServiceProvider epService) {
            string[] fields = "a_string,b0_string,b1_string,b2_string,c_string".Split(',');
            string text = "select * from MyEvent#keepall " +
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
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 10));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E2", null, null, null, "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", null, null, null, "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 12));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", null, null, null, "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E4", "E5", null, null, "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", null, null, null, "E3"}, new object[] {"E4", "E5", null, null, "E6"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E10", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 9));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E7", "E8", "E9", "E10", "E11"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", null, null, null, "E3"}, new object[] {"E4", "E5", null, null, "E6"}, new object[] {"E7", "E8", "E9", "E10", "E11"}});
    
            stmt.Stop();
    
            // Zero-to-many unfiltered
            string epl = "select * from MyEvent match_recognize (" +
                    "measures A as a, B as b, C as c " +
                    "pattern (A C*? B) " +
                    "define " +
                    "A as typeof(A) = 'MyEventTypeA'," +
                    "B as typeof(B) = 'MyEventTypeB'" +
                    ")";
            stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Dispose();
        }
    
        private void RunAssertionOneToMany(EPServiceProvider epService) {
            string[] fields = "a_string,b0_string,b1_string,b2_string,c_string".Split(',');
            string text = "select * from MyEvent#keepall " +
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
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
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
                    new object[][]{new object[] {"E4", "E5", null, null, "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E4", "E5", null, null, "E6"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E10", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 9));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E7", "E8", "E9", "E10", "E11"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E4", "E5", null, null, "E6"}, new object[] {"E7", "E8", "E9", "E10", "E11"}});
    
            stmt.Stop();
        }
    
        private void RunAssertionZeroToOne(EPServiceProvider epService) {
            string[] fields = "a_string,b_string,c_string".Split(',');
            string text = "select * from MyEvent#keepall " +
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
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 10));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsFalse(stmt.HasFirst());
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E3", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E2", null, "E3"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", null, "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E4", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E5", 12));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", null, "E3"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E6", 8));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E4", "E5", "E6"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", null, "E3"}, new object[] {"E4", "E5", "E6"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 10));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 9));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", null, "E3"}, new object[] {"E4", "E5", "E6"}});
    
            stmt.Stop();
    
            // test optional event not defined
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportBean_B));
    
            string epl = "select * from A match_recognize (" +
                    "measures A.id as id, B.id as b_id " +
                    "pattern (A B?) " +
                    "define " +
                    " A as typeof(A) = 'A'" +
                    ")";
            epService.EPAdministrator.CreateEPL(epl).Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            Assert.IsTrue(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPartitionBy(EPServiceProvider epService) {
            string[] fields = "a_string,a_value,b_value".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  partition by TheString" +
                    "  measures A.TheString as a_string, A.value as a_value, B.value as b_value " +
                    "  all matches pattern (A B) " +
                    "  define B as (B.value > A.value)" +
                    ")" +
                    " order by a_string";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
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
                    new object[][]{new object[] {"S1", 4, 6}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"S1", 4, 6}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", 10));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"S4", -1, 10}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"S1", 4, 6}, new object[] {"S4", -1, 10}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", 11));
            Assert.IsFalse(listener.IsInvoked);      // since skip past last row
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"S1", 4, 6}, new object[] {"S4", -1, 10}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S3", 3));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", -2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S3", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 4));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"S1", 4, 6}, new object[] {"S4", -1, 10}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 7));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"S1", 4, 7}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"S1", 4, 6}, new object[] {"S1", 4, 7}, new object[] {"S4", -1, 10}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", 12));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"S4", -2, 12}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"S1", 4, 6}, new object[] {"S1", 4, 7}, new object[] {"S4", -1, 10}, new object[] {"S4", -2, 12}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S4", 12));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 7));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", 4));
            epService.EPRuntime.SendEvent(new SupportRecogBean("S1", 5));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("S2", 5));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"S2", 4, 5}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"S1", 4, 6}, new object[] {"S1", 4, 7}, new object[] {"S2", 4, 5}, new object[] {"S4", -1, 10}, new object[] {"S4", -2, 12}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnlimitedPartition(EPServiceProvider epService) {
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  partition by value" +
                    "  measures A.TheString as a_string " +
                    "  pattern (A B) " +
                    "  define " +
                    "    A as (A.TheString = 'A')," +
                    "    B as (B.TheString = 'B')" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            for (int i = 0; i < 5 * RegexPartitionStateRepoGroup.INITIAL_COLLECTION_MIN; i++) {
                epService.EPRuntime.SendEvent(new SupportRecogBean("A", i));
                epService.EPRuntime.SendEvent(new SupportRecogBean("B", i));
                Assert.IsTrue(listener.GetAndClearIsInvoked());
            }
    
            for (int i = 0; i < 5 * RegexPartitionStateRepoGroup.INITIAL_COLLECTION_MIN; i++) {
                epService.EPRuntime.SendEvent(new SupportRecogBean("A", i + 100000));
            }
            Assert.IsFalse(listener.GetAndClearIsInvoked());
            for (int i = 0; i < 5 * RegexPartitionStateRepoGroup.INITIAL_COLLECTION_MIN; i++) {
                epService.EPRuntime.SendEvent(new SupportRecogBean("B", i + 100000));
                Assert.IsTrue(listener.GetAndClearIsInvoked());
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionConcatWithinAlter(EPServiceProvider epService) {
            string[] fields = "a_string,b_string,c_string,d_string".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, B.TheString as b_string, C.TheString as c_string, D.TheString as d_string " +
                    "  all matches pattern ( A B | C D ) " +
                    "  define " +
                    "    A as (A.value = 1)," +
                    "    B as (B.value = 2)," +
                    "    C as (C.value = 3)," +
                    "    D as (D.value = 4)" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
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
                    new object[][]{new object[] {null, null, "E4", "E5"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {null, null, "E4", "E5"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E1", "E2", null, null}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {null, null, "E4", "E5"}, new object[] {"E1", "E2", null, null}});
    
            stmt.Stop();
        }
    
        private void RunAssertionAlterWithinConcat(EPServiceProvider epService) {
            string[] fields = "a_string,b_string,c_string,d_string".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A.TheString as a_string, B.TheString as b_string, C.TheString as c_string, D.TheString as d_string " +
                    "  all matches pattern ( (A | B) (C | D) ) " +
                    "  define " +
                    "    A as (A.value = 1)," +
                    "    B as (B.value = 2)," +
                    "    C as (C.value = 3)," +
                    "    D as (D.value = 4)" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
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
                    new object[][]{new object[] {"E5", null, "E6", null}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E5", null, "E6", null}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E7", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 3));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {null, "E7", "E8", null}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E5", null, "E6", null}, new object[] {null, "E7", "E8", null}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionVariableMoreThenOnce(EPServiceProvider epService) {
            string[] fields = "a0,b,a1".Split(',');
            string text = "select * from MyEvent#keepall " +
                    "match_recognize (" +
                    "  measures A[0].TheString as a0, B.TheString as b, A[1].TheString as a1 " +
                    "  all matches pattern ( A B A ) " +
                    "  define " +
                    "    A as (A.value = 1)," +
                    "    B as (B.value = 2)" +
                    ")";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text);
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
                    new object[][]{new object[] {"E5", "E6", "E7"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E5", "E6", "E7"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E8", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E9", 1));
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E5", "E6", "E7"}});
    
            epService.EPRuntime.SendEvent(new SupportRecogBean("E10", 2));
            epService.EPRuntime.SendEvent(new SupportRecogBean("E11", 1));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                    new object[][]{new object[] {"E9", "E10", "E11"}});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E5", "E6", "E7"}, new object[] {"E9", "E10", "E11"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionRegex() {
            Assert.IsTrue("aq".Matches("^aq|^id"));
            Assert.IsTrue("id".Matches("^aq|^id"));
            Assert.IsTrue("ad".Matches("a(q|i)?d"));
            Assert.IsTrue("aqd".Matches("a(q|i)?d"));
            Assert.IsTrue("aid".Matches("a(q|i)?d"));
            Assert.IsFalse("aed".Matches("a(q|i)?d"));
            Assert.IsFalse("a".Matches("(a(b?)c)?"));
        }
    }
} // end of namespace
