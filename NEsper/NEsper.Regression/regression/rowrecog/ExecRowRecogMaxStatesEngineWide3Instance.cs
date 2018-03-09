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
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.bean.SupportBeanConstants;

using NUnit.Framework;

namespace com.espertech.esper.regression.rowrecog
{
    public class ExecRowRecogMaxStatesEngineWide3Instance : RegressionExecution
    {
        private SupportConditionHandlerFactory.SupportConditionHandler handler;
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType(typeof(SupportBean_S1));
            configuration.EngineDefaults.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));
            configuration.EngineDefaults.MatchRecognize.MaxStates = 3L;
            configuration.EngineDefaults.MatchRecognize.IsMaxStatesPreventStart = true;
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            handler = SupportConditionHandlerFactory.LastHandler;
    
            RunAssertionTwoStatementNoDelete(epService);
            RunAssertionContextPartitionAndOverflow(epService);
            RunAssertionNamedWindowInSequenceRemoveEvent(epService);
            RunAssertionNamedWindowOutOfSequenceRemoveEvent(epService);
        }
    
        private void RunAssertionTwoStatementNoDelete(EPServiceProvider epService) {
            string[] fields = "c0".Split(',');
            string eplOne = "@Name('S1') select * from SupportBean(TheString='A') " +
                    "match_recognize (" +
                    "  measures P1.LongPrimitive as c0" +
                    "  pattern (P1 P2 P3) " +
                    "  define " +
                    "    P1 as P1.IntPrimitive = 1," +
                    "    P2 as P2.IntPrimitive = 1," +
                    "    P3 as P3.IntPrimitive = 2 and P3.LongPrimitive = P1.LongPrimitive" +
                    ")";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(eplOne);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            string eplTwo = "@Name('S2') select * from SupportBean(TheString='B') " +
                    "match_recognize (" +
                    "  measures P1.LongPrimitive as c0" +
                    "  pattern (P1 P2 P3) " +
                    "  define " +
                    "    P1 as P1.IntPrimitive = 1," +
                    "    P2 as P2.IntPrimitive = 1," +
                    "    P3 as P3.IntPrimitive = 2 and P3.LongPrimitive = P1.LongPrimitive" +
                    ")";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(eplTwo);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            epService.EPRuntime.SendEvent(MakeBean("A", 1, 10)); // A(10):P1->P2
            epService.EPRuntime.SendEvent(MakeBean("B", 1, 11)); // A(10):P1->P2, B(11):P1->P2
            epService.EPRuntime.SendEvent(MakeBean("A", 1, 12)); // A(10):P2->P3, A(12):P1->P2, B(11):P1->P2
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // overflow
            epService.EPRuntime.SendEvent(MakeBean("B", 1, 13)); // would be: A(10):P2->P3, A(12):P1->P2, B(11):P2->P3, B(13):P1->P2
            AssertContextEnginePool(epService, stmtTwo, handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 2, "S2", 1));
    
            // terminate B
            epService.EPRuntime.SendEvent(MakeBean("B", 2, 11)); // we have no more B-state
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new object[]{11L});
    
            // should not overflow
            epService.EPRuntime.SendEvent(MakeBean("B", 1, 15));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // overflow
            epService.EPRuntime.SendEvent(MakeBean("B", 1, 16));
            AssertContextEnginePool(epService, stmtTwo, handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 2, "S2", 1));
    
            // terminate A
            epService.EPRuntime.SendEvent(MakeBean("A", 2, 10)); // we have no more A-state
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{10L});
    
            // should not overflow
            epService.EPRuntime.SendEvent(MakeBean("B", 1, 17));
            epService.EPRuntime.SendEvent(MakeBean("B", 1, 18));
            epService.EPRuntime.SendEvent(MakeBean("A", 1, 19));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // overflow
            epService.EPRuntime.SendEvent(MakeBean("A", 1, 20));
            AssertContextEnginePool(epService, stmtOne, handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 1, "S2", 2));
    
            // terminate B
            epService.EPRuntime.SendEvent(MakeBean("B", 2, 17));
            EPAssertionUtil.AssertProps(listenerTwo.AssertOneGetNewAndReset(), fields, new object[]{17L});
    
            // terminate A
            epService.EPRuntime.SendEvent(MakeBean("A", 2, 19));
            EPAssertionUtil.AssertProps(listenerOne.AssertOneGetNewAndReset(), fields, new object[]{19L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionContextPartitionAndOverflow(EPServiceProvider epService) {
            string[] fields = "c0".Split(',');
            string eplCtx = "create context MyCtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(p10 = s0.p00)";
            epService.EPAdministrator.CreateEPL(eplCtx);
    
            string epl = "@Name('S1') context MyCtx select * from SupportBean(TheString = context.s0.p00) " +
                    "match_recognize (" +
                    "  measures P2.TheString as c0" +
                    "  pattern (P1 P2) " +
                    "  define " +
                    "    P1 as P1.IntPrimitive = 1," +
                    "    P2 as P2.IntPrimitive = 2" +
                    ")";
            var listener = new SupportUpdateListener();
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "B"));
            epService.EPRuntime.SendEvent(new SupportBean("B", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "C"));
            epService.EPRuntime.SendEvent(new SupportBean("C", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "D"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean("D", 1));
            AssertContextEnginePool(epService, stmt, handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));
    
            // terminate a context partition
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "D"));
            epService.EPRuntime.SendEvent(new SupportBean("D", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean("E", 1));
            AssertContextEnginePool(epService, stmt, handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));
    
            epService.EPRuntime.SendEvent(new SupportBean("A", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"A"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNamedWindowInSequenceRemoveEvent(EPServiceProvider epService) {
            string[] fields = "c0,c1".Split(',');
    
            string namedWindow = "create window MyWindow#keepall as SupportBean";
            epService.EPAdministrator.CreateEPL(namedWindow);
            string insert = "insert into MyWindow select * from SupportBean";
            epService.EPAdministrator.CreateEPL(insert);
            string delete = "on SupportBean_S0 delete from MyWindow where TheString = p00";
            epService.EPAdministrator.CreateEPL(delete);
    
            string epl = "@Name('S1') select * from MyWindow " +
                    "match_recognize (" +
                    "  partition by TheString " +
                    "  measures P1.LongPrimitive as c0, P2.LongPrimitive as c1" +
                    "  pattern (P1 P2) " +
                    "  define " +
                    "    P1 as P1.IntPrimitive = 0," +
                    "    P2 as P2.IntPrimitive = 1" +
                    ")";
            var listener = new SupportUpdateListener();
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeBean("A", 0, 1));
            epService.EPRuntime.SendEvent(MakeBean("B", 0, 2));
            epService.EPRuntime.SendEvent(MakeBean("C", 0, 3));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // overflow
            epService.EPRuntime.SendEvent(MakeBean("D", 0, 4));
            AssertContextEnginePool(epService, stmt, handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));
    
            // delete A (in-sequence remove)
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            epService.EPRuntime.SendEvent(MakeBean("D", 0, 5)); // now 3 states: B, C, D
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // test matching
            epService.EPRuntime.SendEvent(MakeBean("B", 1, 6)); // now 2 states: C, D
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{2L, 6L});
    
            // no overflows
            epService.EPRuntime.SendEvent(MakeBean("E", 0, 7));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // overflow
            epService.EPRuntime.SendEvent(MakeBean("F", 0, 9));
            AssertContextEnginePool(epService, stmt, handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));
    
            // no match expected
            epService.EPRuntime.SendEvent(MakeBean("F", 1, 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNamedWindowOutOfSequenceRemoveEvent(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
    
            string namedWindow = "create window MyWindow#keepall as SupportBean";
            epService.EPAdministrator.CreateEPL(namedWindow);
            string insert = "insert into MyWindow select * from SupportBean";
            epService.EPAdministrator.CreateEPL(insert);
            string delete = "on SupportBean_S0 delete from MyWindow where TheString = p00 and IntPrimitive = id";
            epService.EPAdministrator.CreateEPL(delete);
    
            string epl = "@Name('S1') select * from MyWindow " +
                    "match_recognize (" +
                    "  partition by TheString " +
                    "  measures P1.LongPrimitive as c0, P2.LongPrimitive as c1, P3.LongPrimitive as c2" +
                    "  pattern (P1 P2 P3) " +
                    "  define " +
                    "    P1 as P1.IntPrimitive = 0," +
                    "    P2 as P2.IntPrimitive = 1," +
                    "    P3 as P3.IntPrimitive = 2" +
                    ")";
            var listener = new SupportUpdateListener();
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeBean("A", 0, 1));
            epService.EPRuntime.SendEvent(MakeBean("A", 1, 2));
            epService.EPRuntime.SendEvent(MakeBean("B", 0, 3));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // delete A-1 (out-of-sequence remove)
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "A"));
            epService.EPRuntime.SendEvent(MakeBean("A", 2, 4));
            Assert.IsFalse(listener.IsInvoked);
            Assert.IsTrue(handler.Contexts.IsEmpty()); // states: B
    
            // test overflow
            epService.EPRuntime.SendEvent(MakeBean("C", 0, 5));
            epService.EPRuntime.SendEvent(MakeBean("D", 0, 6));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // overflow
            epService.EPRuntime.SendEvent(MakeBean("E", 0, 7));
            AssertContextEnginePool(epService, stmt, handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));
    
            // assert nothing matches for overflowed and deleted
            epService.EPRuntime.SendEvent(MakeBean("E", 1, 8));
            epService.EPRuntime.SendEvent(MakeBean("E", 2, 9));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "C")); // delete c
            epService.EPRuntime.SendEvent(MakeBean("C", 1, 10));
            epService.EPRuntime.SendEvent(MakeBean("C", 2, 11));
            Assert.IsFalse(listener.IsInvoked);
    
            // assert match found for B
            epService.EPRuntime.SendEvent(MakeBean("B", 1, 12));
            epService.EPRuntime.SendEvent(MakeBean("B", 2, 13));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3L, 12L, 13L});
    
            // no overflow
            epService.EPRuntime.SendEvent(MakeBean("F", 0, 14));
            epService.EPRuntime.SendEvent(MakeBean("G", 0, 15));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            // overflow
            epService.EPRuntime.SendEvent(MakeBean("H", 0, 16));
            AssertContextEnginePool(epService, stmt, handler.GetAndResetContexts(), 3, GetExpectedCountMap("S1", 3));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        internal static void AssertContextEnginePool(EPServiceProvider epService, EPStatement stmt, List<ConditionHandlerContext> contexts, int max, IDictionary<string, long> counts) {
            Assert.AreEqual(1, contexts.Count);
            ConditionHandlerContext context = contexts[0];
            Assert.AreEqual(epService.URI, context.EngineURI);
            Assert.AreEqual(stmt.Text, context.Epl);
            Assert.AreEqual(stmt.Name, context.StatementName);
            ConditionMatchRecognizeStatesMax condition = (ConditionMatchRecognizeStatesMax) context.EngineCondition;
            Assert.AreEqual(max, condition.Max);
            Assert.AreEqual(counts.Count, condition.Counts.Count);
            foreach (var expected in counts) {
                Assert.AreEqual(expected.Value, condition.Counts.Get(expected.Key), "failed for key " + expected.Key);
            }
            contexts.Clear();
        }
    
        internal static IDictionary<string, long> GetExpectedCountMap(string stmtOne, long countOne, string stmtTwo, long countTwo) {
            var result = new Dictionary<string, long>();
            result.Put(stmtOne, countOne);
            result.Put(stmtTwo, countTwo);
            return result;
        }
    
        internal static IDictionary<string, long> GetExpectedCountMap(string stmtOne, long countOne) {
            var result = new Dictionary<string, long>();
            result.Put(stmtOne, countOne);
            return result;
        }
    
        internal static SupportBean MakeBean(string theString, int intPrimitive, long longPrimitive) {
            var supportBean = new SupportBean(theString, intPrimitive);
            supportBean.LongPrimitive = longPrimitive;
            return supportBean;
        }
    }
} // end of namespace
