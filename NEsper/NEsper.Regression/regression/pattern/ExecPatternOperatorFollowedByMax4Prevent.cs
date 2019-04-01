///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternOperatorFollowedByMax4Prevent : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            configuration.AddEventType("SupportBean_B", typeof(SupportBean_B));
            configuration.EngineDefaults.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));
            configuration.EngineDefaults.Patterns.MaxSubexpressions = 4L;
            configuration.EngineDefaults.Patterns.IsMaxSubexpressionPreventStart = true;
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            if (SupportConfigFactory.SkipTest(typeof(ExecPatternOperatorFollowedByMax4Prevent))) {
                return;
            }
            ConditionHandlerFactoryContext context = SupportConditionHandlerFactory.FactoryContexts[0];
            Assert.AreEqual(epService.URI, context.EngineURI);
            SupportConditionHandlerFactory.SupportConditionHandler handler = SupportConditionHandlerFactory.LastHandler;
    
            RunAssertionFollowedWithMax(epService, handler);
            RunAssertionTwoStatementsAndStopDestroy(epService, handler);
        }
    
        private void RunAssertionFollowedWithMax(EPServiceProvider epService, SupportConditionHandlerFactory.SupportConditionHandler handler) {
            string expressionOne = "@Name('S1') select * from pattern [every a=SupportBean(TheString like 'A%') -[2]> b=SupportBean_A(id=a.TheString)]";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(expressionOne);
    
            string expressionTwo = "@Name('S2') select * from pattern [every a=SupportBean(TheString like 'B%') -> b=SupportBean_B(id=a.TheString)]";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(expressionTwo);
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 0));
            epService.EPRuntime.SendEvent(new SupportBean("B1", 0));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 0));
            AssertContextStatement(epService, stmtOne, handler.GetAndResetContexts(), 2);
    
            epService.EPRuntime.SendEvent(new SupportBean("B2", 0));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean("B3", 0));
            AssertContextEnginePool(epService, stmtTwo, handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 2, "S2", 2));
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            epService.EPRuntime.SendEvent(new SupportBean("B4", 0));   // now A1, B1, B2, B4
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean("A3", 0));
            AssertContextEnginePool(epService, stmtOne, handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 1, "S2", 3));
    
            stmtOne.Dispose();
    
            epService.EPRuntime.SendEvent(new SupportBean("B4", 0));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean("B5", 0));
            AssertContextEnginePool(epService, stmtTwo, handler.GetAndResetContexts(), 4, GetExpectedCountMap("S2", 4));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionTwoStatementsAndStopDestroy(EPServiceProvider epService, SupportConditionHandlerFactory.SupportConditionHandler handler) {
            string expressionOne = "@Name('S1') select * from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean_A(id=a.TheString)]";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(expressionOne);
    
            string expressionTwo = "@Name('S2') select * from pattern [every a=SupportBean(TheString like 'B%') -> b=SupportBean_B(id=a.TheString)]";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(expressionTwo);
    
            epService.EPRuntime.SendEvent(new SupportBean("A1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("A2", 0));
            epService.EPRuntime.SendEvent(new SupportBean("A3", 0));
            epService.EPRuntime.SendEvent(new SupportBean("B1", 0));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean("B2", 0));
            AssertContextEnginePool(epService, stmtTwo, handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 3, "S2", 1));
    
            epService.EPRuntime.SendEvent(new SupportBean("A4", 0));
            AssertContextEnginePool(epService, stmtOne, handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 3, "S2", 1));
    
            stmtOne.Stop();
    
            epService.EPRuntime.SendEvent(new SupportBean("B3", 0));
            epService.EPRuntime.SendEvent(new SupportBean("B4", 0));
            epService.EPRuntime.SendEvent(new SupportBean("B5", 0));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean("B6", 0));
            AssertContextEnginePool(epService, stmtTwo, handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 0, "S2", 4));
    
            stmtOne.Dispose();
    
            epService.EPRuntime.SendEvent(new SupportBean("B7", 0));
            AssertContextEnginePool(epService, stmtTwo, handler.GetAndResetContexts(), 4, GetExpectedCountMap("S2", 4));
        }
    
        internal static IDictionary<string, long> GetExpectedCountMap(string statementName, long count) {
            var result = new Dictionary<string, long>();
            result.Put(statementName, count);
            return result;
        }
    
        internal static IDictionary<string, long> GetExpectedCountMap(string stmtOne, long countOne, string stmtTwo, long countTwo) {
            var result = new Dictionary<string, long>();
            result.Put(stmtOne, countOne);
            result.Put(stmtTwo, countTwo);
            return result;
        }
    
        internal static void AssertContextEnginePool(EPServiceProvider epService, EPStatement stmt, List<ConditionHandlerContext> contexts, int max, IDictionary<string, long> counts) {
            Assert.AreEqual(1, contexts.Count);
            ConditionHandlerContext context = contexts[0];
            Assert.AreEqual(epService.URI, context.EngineURI);
            Assert.AreEqual(stmt.Text, context.Epl);
            Assert.AreEqual(stmt.Name, context.StatementName);
            ConditionPatternEngineSubexpressionMax condition = (ConditionPatternEngineSubexpressionMax) context.EngineCondition;
            Assert.AreEqual(max, condition.Max);
            Assert.AreEqual(counts.Count, condition.Counts.Count);
            foreach (var expected in counts) {
                Assert.AreEqual(expected.Value, condition.Counts.Get(expected.Key), "failed for key " + expected.Key);
            }
            contexts.Clear();
        }
    
        internal static void AssertContextStatement(EPServiceProvider epService, EPStatement stmt, List<ConditionHandlerContext> contexts, int max) {
            Assert.AreEqual(1, contexts.Count);
            ConditionHandlerContext context = contexts[0];
            Assert.AreEqual(epService.URI, context.EngineURI);
            Assert.AreEqual(stmt.Text, context.Epl);
            Assert.AreEqual(stmt.Name, context.StatementName);
            ConditionPatternSubexpressionMax condition = (ConditionPatternSubexpressionMax) context.EngineCondition;
            Assert.AreEqual(max, condition.Max);
            contexts.Clear();
        }
    }
} // end of namespace
