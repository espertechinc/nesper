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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;
using NUnit.Framework;

namespace com.espertech.esper.regression.pattern
{
    public class ExecPatternOperatorFollowedByMax : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            configuration.AddEventType("SupportBean_B", typeof(SupportBean_B));
            configuration.AddEventType("SupportBean_C", typeof(SupportBean_C));
            configuration.EngineDefaults.ConditionHandling.AddClass(typeof(SupportConditionHandlerFactory));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionMultiple(epService);
            RunAssertionMixed(epService);
            RunAssertionSinglePermFalseAndQuit(epService);
            RunAssertionSingleMaxSimple(epService);
    
            TryInvalid(epService, "select * from pattern[a=SupportBean_A -[a.IntPrimitive]> SupportBean_B]",
                    "Invalid maximum expression in followed-by, event properties are not allowed within the expression [select * from pattern[a=SupportBean_A -[a.IntPrimitive]> SupportBean_B]]");
            TryInvalid(epService, "select * from pattern[a=SupportBean_A -[false]> SupportBean_B]",
                    "Invalid maximum expression in followed-by, the expression must return an integer value [select * from pattern[a=SupportBean_A -[false]> SupportBean_B]]");
        }
    
        private void RunAssertionMultiple(EPServiceProvider epService) {
            SupportConditionHandlerFactory.SupportConditionHandler handler = SupportConditionHandlerFactory.LastHandler;
    
            string expression = "select a.id as a, b.id as b, c.id as c from pattern [" +
                    "every a=SupportBean_A -[2]> b=SupportBean_B -[3]> c=SupportBean_C]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
    
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new string[]{"a", "b", "c"};
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            AssertContext(epService, stmt, handler.Contexts, 3);
    
            epService.EPRuntime.SendEvent(new SupportBean_C("C1"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A1", "B1", "C1"}, new object[] {"A2", "B1", "C1"}, new object[] {"A3", "B2", "C1"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionMixed(EPServiceProvider epService) {
            SupportConditionHandlerFactory.SupportConditionHandler handler = SupportConditionHandlerFactory.LastHandler;
    
            string expression = "select a.id as a, b.id as b, c.id as c from pattern [" +
                    "every a=SupportBean_A -> b=SupportBean_B -[2]> c=SupportBean_C]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
    
            TryAssertionMixed(epService, stmt, handler);
    
            // test SODA
            stmt.Dispose();
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(expression);
            Assert.AreEqual(expression, model.ToEPL());
            stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(stmt.Text, model.ToEPL());
            TryAssertionMixed(epService, stmt, handler);
    
            stmt.Dispose();
        }
    
        private static void TryAssertionMixed(EPServiceProvider epService, EPStatement stmt, SupportConditionHandlerFactory.SupportConditionHandler handler) {
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            var fields = new string[]{"a", "b", "c"};
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
    
            handler.Contexts.Clear();
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            AssertContext(epService, stmt, handler.Contexts, 2);
    
            epService.EPRuntime.SendEvent(new SupportBean_C("C1"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A1", "B1", "C1"}, new object[] {"A2", "B1", "C1"}});
        }
    
        private void RunAssertionSinglePermFalseAndQuit(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            ConditionHandlerFactoryContext context = SupportConditionHandlerFactory.FactoryContexts[0];
            Assert.AreEqual(epService.URI, context.EngineURI);
            SupportConditionHandlerFactory.SupportConditionHandler handler = SupportConditionHandlerFactory.LastHandler;
            handler.GetAndResetContexts();
            var listener = new SupportUpdateListener();
    
            // not-operator
            string expression = "select a.id as a, b.id as b from pattern [every a=SupportBean_A -[2]> (b=SupportBean_B and not SupportBean_C)]";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(expression);
            stmt.Events += listener.Update;
            var fields = new string[]{"a", "b"};
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            epService.EPRuntime.SendEvent(new SupportBean_C("C1"));
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A3", "B1"}, new object[] {"A4", "B1"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A5"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A6"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A7"));
            AssertContext(epService, stmt, handler.Contexts, 2);
            stmt.Dispose();
    
            // guard
            string expressionTwo = "select a.id as a, b.id as b from pattern [every a=SupportBean_A -[2]> (b=SupportBean_B where timer:within(1))]";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(expressionTwo);
            stmtTwo.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000)); // expires sub-expressions
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            Assert.IsTrue(handler.Contexts.IsEmpty());
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A3", "B1"}, new object[] {"A4", "B1"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A5"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A6"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A7"));
            AssertContext(epService, stmtTwo, handler.Contexts, 2);
    
            // every-operator
            stmtTwo.Dispose();
            string expressionThree = "select a.id as a, b.id as b from pattern [every a=SupportBean_A -[2]> (every b=SupportBean_B(id=a.id) and not SupportBean_C(id=a.id))]";
            EPStatement stmtThree = epService.EPAdministrator.CreateEPL(expressionThree);
            stmtThree.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("2"));
    
            epService.EPRuntime.SendEvent(new SupportBean_B("1"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"1", "1"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_B("2"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"2", "2"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_C("1"));
    
            epService.EPRuntime.SendEvent(new SupportBean_A("3"));
            epService.EPRuntime.SendEvent(new SupportBean_B("3"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"3", "3"}});
    
            stmtThree.Dispose();
        }
    
        private void RunAssertionSingleMaxSimple(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_B", typeof(SupportBean_B));
    
            ConditionHandlerFactoryContext context = SupportConditionHandlerFactory.FactoryContexts[0];
            Assert.AreEqual(epService.URI, context.EngineURI);
            SupportConditionHandlerFactory.SupportConditionHandler handler = SupportConditionHandlerFactory.LastHandler;
    
            string expression = "select a.id as a, b.id as b from pattern [every a=SupportBean_A -[2]> b=SupportBean_B]";
            EPStatement statement = epService.EPAdministrator.CreateEPL(expression);
            RunAssertionSingleMaxSimple(epService, statement, handler);
            statement.Dispose();
    
            // test SODA
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(expression);
            Assert.AreEqual(expression, model.ToEPL());
            statement = epService.EPAdministrator.Create(model);
            Assert.AreEqual(statement.Text, model.ToEPL());
            RunAssertionSingleMaxSimple(epService, statement, handler);
            statement.Dispose();
    
            // test variable
            epService.EPAdministrator.CreateEPL("create variable int myvar=3");
            expression = "select a.id as a, b.id as b from pattern [every a=SupportBean_A -[myvar-1]> b=SupportBean_B]";
            statement = epService.EPAdministrator.CreateEPL(expression);
            RunAssertionSingleMaxSimple(epService, statement, handler);
    
            statement.Dispose();
        }
    
        private static void RunAssertionSingleMaxSimple(EPServiceProvider epService, EPStatement stmt, SupportConditionHandlerFactory.SupportConditionHandler handler) {
    
            var fields = new string[]{"a", "b"};
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A2"));
    
            handler.Contexts.Clear();
            epService.EPRuntime.SendEvent(new SupportBean_A("A3"));
            AssertContext(epService, stmt, handler.Contexts, 2);
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B1"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A1", "B1"}, new object[] {"A2", "B1"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A4"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B2"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A4", "B2"}});
            Assert.IsTrue(handler.Contexts.IsEmpty());
    
            for (int i = 5; i < 9; i++) {
                epService.EPRuntime.SendEvent(new SupportBean_A("A" + i));
                if (i >= 7) {
                    AssertContext(epService, stmt, handler.Contexts, 2);
                }
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B3"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A5", "B3"}, new object[] {"A6", "B3"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_B("B4"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_A("A20"));
            epService.EPRuntime.SendEvent(new SupportBean_A("A21"));
            epService.EPRuntime.SendEvent(new SupportBean_B("B5"));
            EPAssertionUtil.AssertPropsPerRow(listener.GetAndResetLastNewData(), fields, new object[][]{new object[] {"A20", "B5"}, new object[] {"A21", "B5"}});
            Assert.IsTrue(handler.Contexts.IsEmpty());
        }
    
        private static void AssertContext(EPServiceProvider epService, EPStatement stmt, List<ConditionHandlerContext> contexts, int max) {
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
} // end of namespace
