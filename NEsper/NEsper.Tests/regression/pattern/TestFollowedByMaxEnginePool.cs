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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestFollowedByMaxEnginePool : SupportBeanConstants
    {
        private EPServiceProvider _epService;
        private SupportConditionHandlerFactory.SupportConditionHandler _handler;
    
        [TearDown]
        public void TearDown()
        {
            _handler = null;
        }
    
        [Test]
        public void TestFollowedWithMax()
        {
            InitService(4L, true);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            const string expressionOne = "@Name('S1') select * from pattern [every a=SupportBean(TheString like 'A%') -[2]> b=SupportBean_A(id=a.TheString)]";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(expressionOne);
    
            const string expressionTwo = "@Name('S2') select * from pattern [every a=SupportBean(TheString like 'B%') -> b=SupportBean_B(id=a.TheString)]";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(expressionTwo);
    
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("B1", 0));
            Assert.IsTrue(_handler.Contexts.IsEmpty());
    
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 0));
            AssertContextStatement(_epService, stmtOne, _handler.GetAndResetContexts(), 2);
    
            _epService.EPRuntime.SendEvent(new SupportBean("B2", 0));
            Assert.IsTrue(_handler.Contexts.IsEmpty());
    
            _epService.EPRuntime.SendEvent(new SupportBean("B3", 0));
            AssertContextEnginePool(_epService, stmtTwo, _handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 2, "S2", 2));
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            _epService.EPRuntime.SendEvent(new SupportBean("B4", 0));   // now A1, B1, B2, B4
            Assert.IsTrue(_handler.Contexts.IsEmpty());
    
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 0));
            AssertContextEnginePool(_epService, stmtOne, _handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 1, "S2", 3));
    
            stmtOne.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean("B4", 0));
            Assert.IsTrue(_handler.Contexts.IsEmpty());
            
            _epService.EPRuntime.SendEvent(new SupportBean("B5", 0));
            AssertContextEnginePool(_epService, stmtTwo, _handler.GetAndResetContexts(), 4, GetExpectedCountMap("S2", 4));
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestTwoStatementsAndStopDestroy()
        {
            InitService(4, true);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            String expressionOne = "@Name('S1') select * from pattern [every a=SupportBean(TheString like 'A%') -> b=SupportBean_A(id=a.TheString)]";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(expressionOne);

            String expressionTwo = "@Name('S2') select * from pattern [every a=SupportBean(TheString like 'B%') -> b=SupportBean_B(id=a.TheString)]";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(expressionTwo);
    
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("B1", 0));
            Assert.IsTrue(_handler.Contexts.IsEmpty());
            
            _epService.EPRuntime.SendEvent(new SupportBean("B2", 0));
            AssertContextEnginePool(_epService, stmtTwo, _handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 3, "S2", 1));
    
            _epService.EPRuntime.SendEvent(new SupportBean("A4", 0));
            AssertContextEnginePool(_epService, stmtOne, _handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 3, "S2", 1));
    
            stmtOne.Stop();
    
            _epService.EPRuntime.SendEvent(new SupportBean("B3", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("B4", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("B5", 0));
            Assert.IsTrue(_handler.Contexts.IsEmpty());
    
            _epService.EPRuntime.SendEvent(new SupportBean("B6", 0));
            AssertContextEnginePool(_epService, stmtTwo, _handler.GetAndResetContexts(), 4, GetExpectedCountMap("S1", 0, "S2", 4));
    
            stmtOne.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean("B7", 0));
            AssertContextEnginePool(_epService, stmtTwo, _handler.GetAndResetContexts(), 4, GetExpectedCountMap("S2", 4));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestSingleNoOperatorMax()
        {
            InitService(2L, true);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            String expression = "@Name('A') select a.id as a, b.id as b from pattern [every a=SupportBean_A -> b=SupportBean_B]";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
    
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
    
            _handler.Contexts.Clear();
            _epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            AssertContextEnginePool(_epService, stmt, _handler.Contexts, 2, GetExpectedCountMap("A", 2));
    
            String[] fields = new String[] {"a", "b"};
            _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "A1", "B1" }, new Object[] { "A2", "B1" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            _epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "A4", "B2" } });
            Assert.IsTrue(_handler.Contexts.IsEmpty());
    
            for (int i = 5; i < 9; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean_A("A" + i));
                if (i >= 7) {
                    AssertContextEnginePool(_epService, stmt, _handler.Contexts, 2, GetExpectedCountMap("A", 2));
                }
            }
    
            _epService.EPRuntime.SendEvent(new SupportBean_B("B3"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "A5", "B3" }, new Object[] { "A6", "B3" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean_B("B4"));
            Assert.IsFalse(listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A20"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A21"));
            _epService.EPRuntime.SendEvent(new SupportBean_B("B5"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "A20", "B5" }, new Object[] { "A21", "B5" } });
            Assert.IsTrue(_handler.Contexts.IsEmpty());
    
            stmt.Dispose();
        
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestNoPreventRuntimeConfig()
        {
            InitService(2L, false);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            String expression = "@Name('A') select a.id as a, b.id as b from pattern [every a=SupportBean_A -> b=SupportBean_B]";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
    
            SupportUpdateListener listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
    
            _handler.Contexts.Clear();
            _epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            AssertContextEnginePool(_epService, stmt, _handler.Contexts, 2, GetExpectedCountMap("A", 2));
    
            _handler.Contexts.Clear();
            _epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            AssertContextEnginePool(_epService, stmt, _handler.Contexts, 2, GetExpectedCountMap("A", 3));
    
            String[] fields = new String[] {"a", "b"};
            _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields,
                                                 new Object[][]
                                                 {
                                                     new Object[] {"A1", "B1"}, new Object[] {"A2", "B1"},
                                                     new Object[] {"A3", "B1"}, new Object[] {"A4", "B1"}
                                                 });
    
            // set new max
            _epService.EPAdministrator.Configuration.PatternMaxSubexpressions = 1L;
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A5"));
    
            _handler.Contexts.Clear();
            _epService.EPRuntime.SendEvent(new SupportBean_A("A6"));
            AssertContextEnginePool(_epService, stmt, _handler.Contexts, 1, GetExpectedCountMap("A", 1));
    
            stmt.Dispose();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        private void InitService(long max, bool preventStart)
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType<SupportBean_A>();
            config.AddEventType<SupportBean_B>();
            config.EngineDefaults.ConditionHandlingConfig.AddClass(typeof(SupportConditionHandlerFactory));
            config.EngineDefaults.PatternsConfig.MaxSubexpressions = max;
            config.EngineDefaults.PatternsConfig.IsMaxSubexpressionPreventStart = preventStart;
            config.EngineDefaults.Logging.IsEnableExecutionDebug = true;
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
    
            ConditionHandlerFactoryContext context = SupportConditionHandlerFactory.FactoryContexts[0];
            Assert.AreEqual(_epService.URI, context.EngineURI);
            _handler = SupportConditionHandlerFactory.LastHandler;
        }

        private static IDictionary<String, long?> GetExpectedCountMap(String statementName, long count)
        {
            IDictionary<String, long?> result = new Dictionary<String, long?>();
            result.Put(statementName, count);
            return result;
        }
    
        private static IDictionary<String, long?> GetExpectedCountMap(String stmtOne, long countOne, String stmtTwo, long countTwo)
        {
            IDictionary<String, long?> result = new Dictionary<String, long?>();
            result.Put(stmtOne, countOne);
            result.Put(stmtTwo, countTwo);
            return result;
        }
    
        private static void AssertContextEnginePool(EPServiceProvider epService, EPStatement stmt, List<ConditionHandlerContext> contexts, int max, IDictionary<String, long?> counts)
        {
            Assert.AreEqual(1, contexts.Count);
            ConditionHandlerContext context = contexts[0];
            Assert.AreEqual(epService.URI, context.EngineURI);
            Assert.AreEqual(stmt.Text, context.Epl);
            Assert.AreEqual(stmt.Name, context.StatementName);
            ConditionPatternEngineSubexpressionMax condition = (ConditionPatternEngineSubexpressionMax) context.EngineCondition;
            Assert.AreEqual(max, condition.Max);
            Assert.AreEqual(counts.Count, condition.Counts.Count);
            foreach (var expected in counts) {
                Assert.AreEqual(expected.Value, condition.Counts.Get(expected.Key).GetValueOrDefault(),
                    "failed for key " + expected.Key);
            }
            contexts.Clear();
        }
    
        private static void AssertContextStatement(EPServiceProvider epService, EPStatement stmt, List<ConditionHandlerContext> contexts, int max)
        {
            Assert.AreEqual(1, contexts.Count);
            ConditionHandlerContext context = contexts[0];
            Assert.AreEqual(epService.URI, context.EngineURI);
            Assert.AreEqual(stmt.Text, context.Epl);
            Assert.AreEqual(stmt.Name, context.StatementName);
            ConditionPatternSubexpressionMax condition = (ConditionPatternSubexpressionMax) context.EngineCondition;
            Assert.AreEqual(max, condition.Max);
            contexts.Clear();
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
