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
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.compat.logging;

using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    [TestFixture]
    public class TestFollowedByMaxOperator : SupportBeanConstants
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp() {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType<SupportBean_A>();
            config.AddEventType<SupportBean_B>();
            config.AddEventType<SupportBean_C>();
            config.EngineDefaults.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestInvalid() {
            TryInvalid("select * from pattern[a=SupportBean_A -[a.IntPrimitive]> SupportBean_B]",
                    "Invalid maximum expression in followed-by, event properties are not allowed within the expression [select * from pattern[a=SupportBean_A -[a.IntPrimitive]> SupportBean_B]]");
            TryInvalid("select * from pattern[a=SupportBean_A -[false]> SupportBean_B]",
                    "Invalid maximum expression in followed-by, the expression must return an integer value [select * from pattern[a=SupportBean_A -[false]> SupportBean_B]]");
        }
    
        public void TryInvalid(String text, String message) {
            try {
                _epService.EPAdministrator.CreateEPL(text);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        [Test]
        public void TestMultiple() {
            SupportConditionHandlerFactory.SupportConditionHandler handler = SupportConditionHandlerFactory.LastHandler;
    
            const string expression = "select a.id as a, b.id as b, c.id as c from pattern [" +
                                      "every a=SupportBean_A -[2]> b=SupportBean_B -[3]> c=SupportBean_C]";
            var stmt = _epService.EPAdministrator.CreateEPL(expression);
    
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new[] {"a", "b", "c"};
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
            
            _epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            AssertContext(_epService, stmt, handler.Contexts, 3);
    
            _epService.EPRuntime.SendEvent(new SupportBean_C("C1"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "A1", "B1", "C1" }, new Object[] { "A2", "B1", "C1" }, new Object[] { "A3", "B2", "C1" } });
        }
    
        [Test]
        public void TestMixed() {
            SupportConditionHandlerFactory.SupportConditionHandler handler = SupportConditionHandlerFactory.LastHandler;
    
            const string expression = "select a.id as a, b.id as b, c.id as c from pattern [" +
                                      "every a=SupportBean_A -> b=SupportBean_B -[2]> c=SupportBean_C]";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(expression);
    
            RunAssertionMixed(_epService, stmt, handler);
    
            // test SODA
            stmt.Dispose();
            EPStatementObjectModel model =  _epService.EPAdministrator.CompileEPL(expression);
            Assert.AreEqual(expression, model.ToEPL());
            stmt = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmt.Text, model.ToEPL());
            RunAssertionMixed(_epService, stmt, handler);
        }
    
        private static void RunAssertionMixed(EPServiceProvider epService, EPStatement stmt, SupportConditionHandlerFactory.SupportConditionHandler handler)
        {
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new String[] {"a", "b", "c"};
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
    
            handler.Contexts.Clear();
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            AssertContext(epService, stmt, handler.Contexts, 2);
    
            epService.EPRuntime.SendEvent(new SupportBean_C("C1"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "A1", "B1", "C1" }, new Object[] { "A2", "B1", "C1" } });
        }
    
        [Test]
        public void TestSinglePermFalseAndQuit() {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            ConditionHandlerFactoryContext context = SupportConditionHandlerFactory.FactoryContexts[0];
            Assert.AreEqual(_epService.URI, context.EngineURI);
            SupportConditionHandlerFactory.SupportConditionHandler handler = SupportConditionHandlerFactory.LastHandler;
            var listener = new SupportUpdateListener();
    
            // not-operator
            const string expression = "select a.id as a, b.id as b from pattern [every a=SupportBean_A -[2]> (b=SupportBean_B and not SupportBean_C)]";
            var stmt = _epService.EPAdministrator.CreateEPL(expression);
            stmt.Events += listener.Update;
            var fields = new[] {"a", "b"};
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            _epService.EPRuntime.SendEvent(new SupportBean_C("C1"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "A3", "B1" }, new Object[] { "A4", "B1" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A5"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A6"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A7"));
            AssertContext(_epService, stmt, handler.Contexts, 2);
            stmt.Dispose();
    
            // guard
            const string expressionTwo = "select a.id as a, b.id as b from pattern [every a=SupportBean_A -[2]> (b=SupportBean_B where timer:within(1))]";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(expressionTwo);
            stmtTwo.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000)); // expires sub-expressions
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            _epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "A3", "B1" }, new Object[] { "A4", "B1" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("A5"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A6"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("A7"));
            AssertContext(_epService, stmtTwo, handler.Contexts, 2);
    
            // every-operator
            stmtTwo.Dispose();
            const string expressionThree = "select a.id as a, b.id as b from pattern [every a=SupportBean_A -[2]> (every b=SupportBean_B(id=a.id) and not SupportBean_C(id=a.id))]";
            EPStatement stmtThree = _epService.EPAdministrator.CreateEPL(expressionThree);
            stmtThree.Events += listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("1"));
            _epService.EPRuntime.SendEvent(new SupportBean_A("2"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_B("1"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] {"1","1"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_B("2"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] {"2","2"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_C("1"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("3"));
            _epService.EPRuntime.SendEvent(new SupportBean_B("3"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] {"3","3"}});
        }
    
        [Test]
        public void TestSingleMaxSimple()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean_A>();
            config.AddEventType<SupportBean_B>();
            config.EngineDefaults.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));
            
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            ConditionHandlerFactoryContext context = SupportConditionHandlerFactory.FactoryContexts[0];
            Assert.AreEqual(_epService.URI, context.EngineURI);
            SupportConditionHandlerFactory.SupportConditionHandler handler = SupportConditionHandlerFactory.LastHandler;
    
            String expression = "select a.id as a, b.id as b from pattern [every a=SupportBean_A -[2]> b=SupportBean_B]";
            EPStatement statement = _epService.EPAdministrator.CreateEPL(expression);
            RunAssertionSingleMaxSimple(_epService, statement, handler);
            statement.Dispose();
    
            // test SODA
            EPStatementObjectModel model =  _epService.EPAdministrator.CompileEPL(expression);
            Assert.AreEqual(expression, model.ToEPL());
            statement = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(statement.Text, model.ToEPL());
            RunAssertionSingleMaxSimple(_epService, statement, handler);
            statement.Dispose();
            
            // test variable
            _epService.EPAdministrator.CreateEPL("create variable int myvar=3");
            expression = "select a.id as a, b.id as b from pattern [every a=SupportBean_A -[myvar-1]> b=SupportBean_B]";
            statement = _epService.EPAdministrator.CreateEPL(expression);
            RunAssertionSingleMaxSimple(_epService, statement, handler);
        }
    
        private static void RunAssertionSingleMaxSimple(EPServiceProvider epService, EPStatement stmt, SupportConditionHandlerFactory.SupportConditionHandler handler) {
    
            var fields = new String[] {"a", "b"};
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
    
            handler.Contexts.Clear();
            epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            AssertContext(epService, stmt, handler.Contexts, 2);
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "A1", "B1" }, new Object[] { "A2", "B1" } });
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] {"A4","B2"}});
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            for (int i = 5; i < 9; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_A("A" + i));
                if (i >= 7) {
                    AssertContext(epService, stmt, handler.Contexts, 2);
                }
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B3"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "A5", "B3" }, new Object[] { "A6", "B3" } });
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B4"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A20"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A21"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B5"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new Object[][] { new Object[] { "A20", "B5" }, new Object[] { "A21", "B5" } });
            Assert.IsTrue(handler.Contexts.IsEmpty());
        }
    
        private static void AssertContext(EPServiceProvider epService, EPStatement stmt, List<ConditionHandlerContext> contexts, int max) {
            Assert.AreEqual(1, contexts.Count);
            ConditionHandlerContext context = contexts[0];
            Assert.AreEqual(epService.URI, context.EngineURI);
            Assert.AreEqual(stmt.Text, context.Epl);
            Assert.AreEqual(stmt.Name, context.StatementName);
            var condition = (ConditionPatternSubexpressionMax) context.EngineCondition;
            Assert.AreEqual(max, condition.Max);
            contexts.Clear();
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
