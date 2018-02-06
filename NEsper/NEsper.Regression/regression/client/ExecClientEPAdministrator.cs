///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.spec;
using com.espertech.esper.pattern;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientEPAdministrator : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableTimerDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionGetStmtByName(epService);
            RunAssertionStatementArray(epService);
            RunAssertionCreateEPLByName(epService);
            RunAssertionCreatePatternByName(epService);
            RunAssertionDestroyAll(epService);
            RunAssertionStopStartAll(epService);
            RunAssertionStopStartSome(epService);
            RunAssertionSPI(epService);
        }
    
        private void RunAssertionGetStmtByName(EPServiceProvider epService) {
            var names = new string[]{"s1", "s2", "s3--0", "s3", "s3"};
            var expected = new string[]{"s1", "s2", "s3--0", "s3", "s3--1"};
            EPStatement[] stmts = CreateStmts(epService, names);
            for (int i = 0; i < stmts.Length; i++) {
                Assert.AreSame(stmts[i], epService.EPAdministrator.GetStatement(expected[i]), "failed for " + names[i]);
                Assert.AreEqual(expected[i], epService.EPAdministrator.GetStatement(expected[i]).Name, "failed for " + names[i]);
            }
    
            // test statement name trim
            epService.EPAdministrator.Configuration.AddEventType("S0", typeof(SupportBean_S0));
            epService.EPAdministrator.CreateEPL("@Name(' stmt0  ') select * from S0");
            epService.EPAdministrator.CreateEPL("select * from S0", "  stmt1  ");
            foreach (string name in Collections.List("stmt0", "stmt1")) {
                EPStatement stmt = epService.EPAdministrator.GetStatement(name);
                Assert.AreEqual(name, stmt.Name);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStatementArray(EPServiceProvider epService) {
            Assert.AreEqual(0, epService.EPAdministrator.StatementNames.Count);
    
            var names = new string[]{"s1"};
            CreateStmts(epService, names);
            EPAssertionUtil.AssertEqualsAnyOrder(names, epService.EPAdministrator.StatementNames);
    
            names = new string[]{"s1", "s2"};
            CreateStmts(epService, names);
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"s1", "s1--0", "s2"}, epService.EPAdministrator.StatementNames);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCreateEPLByName(EPServiceProvider epService) {
            string stmt = "select * from " + typeof(SupportBean).FullName;
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(stmt, "s1");
            var testListener = new SupportUpdateListener();
            stmtOne.Events += testListener.Update;
            Assert.AreEqual("s1", stmtOne.Name);
            Assert.AreEqual(stmt, stmtOne.Text);
    
            // check working
            SendEvent(epService);
            testListener.AssertOneGetNewAndReset();
    
            // create a second with the same name
            stmt = "select IntPrimitive from " + typeof(SupportBean).FullName;
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(stmt, "s1");
            Assert.AreEqual("s1--0", stmtTwo.Name);
            Assert.AreEqual(stmt, stmtTwo.Text);
    
            // create a third invalid statement with the same name
            stmt = "select xxx from " + typeof(SupportBean).FullName;
            try {
                epService.EPAdministrator.CreateEPL(stmt, "s1");
                Assert.Fail();
            } catch (Exception) {
                // expected
            }
    
            // create a forth statement with the same name
            stmt = "select TheString from " + typeof(SupportBean).FullName;
            EPStatement stmtFour = epService.EPAdministrator.CreateEPL(stmt, "s1");
            Assert.AreEqual("s1--1", stmtFour.Name);
            Assert.AreEqual(stmt, stmtFour.Text);
    
            // create a fifth pattern statement with the same name
            stmt = typeof(SupportBean).FullName;
            EPStatement stmtFive = epService.EPAdministrator.CreatePattern(stmt, "s1");
            Assert.AreEqual("s1--2", stmtFive.Name);
            Assert.AreEqual(stmt, stmtFive.Text);
    
            // should allow null statement name
            epService.EPAdministrator.CreatePattern(stmt, null);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionCreatePatternByName(EPServiceProvider epService) {
            string stmt = typeof(SupportBean).FullName;
            EPStatement stmtOne = epService.EPAdministrator.CreatePattern(stmt, "s1");
            var testListener = new SupportUpdateListener();
            stmtOne.Events += testListener.Update;
            Assert.AreEqual("s1", stmtOne.Name);
            Assert.AreEqual(stmt, stmtOne.Text);
    
            // check working
            SendEvent(epService);
            testListener.AssertOneGetNewAndReset();
    
            // create a second with the same name
            stmt = typeof(SupportMarketDataBean).FullName;
            EPStatement stmtTwo = epService.EPAdministrator.CreatePattern(stmt, "s1");
            Assert.AreEqual("s1--0", stmtTwo.Name);
            Assert.AreEqual(stmt, stmtTwo.Text);
    
            // create a third invalid statement with the same name
            stmt = "xxx" + typeof(SupportBean).FullName;
            try {
                epService.EPAdministrator.CreatePattern(stmt, "s1");
                Assert.Fail();
            } catch (Exception) {
                // expected
            }
    
            // create a forth statement with the same name
            stmt = typeof(SupportBean).FullName;
            EPStatement stmtFour = epService.EPAdministrator.CreatePattern(stmt, "s1");
            Assert.AreEqual("s1--1", stmtFour.Name);
            Assert.AreEqual(stmt, stmtFour.Text);
    
            // create a fifth pattern statement with the same name
            stmt = "select * from " + typeof(SupportBean).FullName;
            EPStatement stmtFive = epService.EPAdministrator.CreateEPL(stmt, "s1");
            Assert.AreEqual("s1--2", stmtFive.Name);
            Assert.AreEqual(stmt, stmtFive.Text);
    
            // Null statement names should be allowed
            epService.EPAdministrator.CreatePattern("every " + typeof(SupportBean).FullName, null);
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDestroyAll(EPServiceProvider epService) {
            EPStatement[] stmts = CreateStmts(epService, new string[]{"s1", "s2", "s3"});
            var testListener = new SupportUpdateListener();
            stmts[0].Events += testListener.Update;
            stmts[1].Events += testListener.Update;
            stmts[2].Events += testListener.Update;
            SendEvent(epService);
            Assert.AreEqual(3, testListener.NewDataList.Count);
            testListener.Reset();
    
            epService.EPAdministrator.DestroyAllStatements();
            AssertDestroyed(epService, stmts, testListener);
        }
    
        private void RunAssertionStopStartAll(EPServiceProvider epService) {
            EPStatement[] stmts = CreateStmts(epService, new string[]{"s1", "s2", "s3"});
            var testListener = new SupportUpdateListener();
            stmts[0].Events += testListener.Update;
            stmts[1].Events += testListener.Update;
            stmts[2].Events += testListener.Update;
    
            AssertStarted(epService, stmts, testListener);
    
            epService.EPAdministrator.StopAllStatements();
            AssertStopped(epService, stmts, testListener);
    
            epService.EPAdministrator.StartAllStatements();
            AssertStarted(epService, stmts, testListener);
    
            epService.EPAdministrator.DestroyAllStatements();
            AssertDestroyed(epService, stmts, testListener);
        }
    
        private void RunAssertionStopStartSome(EPServiceProvider epService) {
            EPStatement[] stmts = CreateStmts(epService, new string[]{"s1", "s2", "s3"});
            var testListener = new SupportUpdateListener();
            stmts[0].Events += testListener.Update;
            stmts[1].Events += testListener.Update;
            stmts[2].Events += testListener.Update;
            AssertStarted(epService, stmts, testListener);
    
            stmts[0].Stop();
            SendEvent(epService);
            Assert.AreEqual(2, testListener.NewDataList.Count);
            testListener.Reset();
    
            epService.EPAdministrator.StopAllStatements();
            AssertStopped(epService, stmts, testListener);
    
            stmts[1].Start();
            SendEvent(epService);
            Assert.AreEqual(1, testListener.NewDataList.Count);
            testListener.Reset();
    
            epService.EPAdministrator.StartAllStatements();
            AssertStarted(epService, stmts, testListener);
    
            epService.EPAdministrator.DestroyAllStatements();
            AssertDestroyed(epService, stmts, testListener);
        }
    
        private void RunAssertionSPI(EPServiceProvider epService) {
    
            EPAdministratorSPI spi = (EPAdministratorSPI) epService.EPAdministrator;
    
            ExprDotNode funcnode = (ExprDotNode) spi.CompileExpression("Func()");
            Assert.IsFalse(funcnode.ChainSpec[0].IsProperty);
    
            ExprNode node = spi.CompileExpression("value=5 and /* comment */ true");
            Assert.AreEqual("value=5 and true", ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(node));
    
            Expression expr = spi.CompileExpressionToSODA("value=5 and true");
            var buf = new StringWriter();
            expr.ToEPL(buf, ExpressionPrecedenceEnum.MINIMUM);
            Assert.AreEqual("value=5 and true", buf.ToString());
    
            expr = spi.CompileExpressionToSODA("5 sec");
            buf = new StringWriter();
            expr.ToEPL(buf, ExpressionPrecedenceEnum.MINIMUM);
            Assert.AreEqual("5 seconds", buf.ToString());
    
            EvalFactoryNode pattern = spi.CompilePatternToNode("every A -> B");
            Assert.IsTrue(pattern is EvalFollowedByFactoryNode);
    
            PatternExpr patternExpr = spi.CompilePatternToSODA("every A -> B");
            Assert.AreEqual(typeof(PatternFollowedByExpr), patternExpr.GetType());
    
            EPStatementObjectModel modelPattern = spi.CompilePatternToSODAModel("@Name('test') every A -> B");
            Assert.AreEqual("Name", modelPattern.Annotations[0].Name);
            Assert.AreEqual(typeof(PatternFollowedByExpr), ((PatternStream) modelPattern.FromClause.Streams[0]).Expression.GetType());
    
            AnnotationPart part = spi.CompileAnnotationToSODA("@Somevalue(a='test', b=5)");
            Assert.AreEqual("Somevalue", part.Name);
            Assert.AreEqual(2, part.Attributes.Count);
            Assert.AreEqual("a", part.Attributes[0].Name);
            Assert.AreEqual("test", part.Attributes[0].Value);
            Assert.AreEqual("b", part.Attributes[1].Name);
            Assert.AreEqual(5, part.Attributes[1].Value);
    
            MatchRecognizeRegEx regex = spi.CompileMatchRecognizePatternToSODA("a b* c+ d? e?");
            Assert.AreEqual(5, regex.Children.Count);
    
            // test fail cases
            string expected = "Incorrect syntax near 'in' (a reserved keyword) at line 1 column 42 [goofy in in]";
            string compiled = "goofy in in";
            try {
                spi.CompileExpression(compiled);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual(expected, ex.Message);
            }
    
            try {
                spi.CompileExpressionToSODA(compiled);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual(expected, ex.Message);
            }
    
            expected = "Incorrect syntax near 'in' (a reserved keyword) at line 1 column 6 [goofy in in]";
            try {
                spi.CompilePatternToNode(compiled);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual(expected, ex.Message);
            }
    
            try {
                spi.CompilePatternToSODA(compiled);
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual(expected, ex.Message);
            }
    
            try {
                spi.CompileAnnotationToSODA("not an annotation");
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Incorrect syntax near 'not' (a reserved keyword) [not an annotation]", ex.Message);
            }
    
            try {
                spi.CompileMatchRecognizePatternToSODA("a b???");
                Assert.Fail();
            } catch (EPException ex) {
                Assert.AreEqual("Incorrect syntax near '?' expecting a closing parenthesis ')' but found a questionmark '?' at line 1 column 76 [a b???]", ex.Message);
            }
    
            StatementSpecRaw raw = spi.CompileEPLToRaw("select * from System.Object");
            Assert.IsNotNull(raw);
            EPStatementObjectModel model = spi.MapRawToSODA(raw);
            Assert.IsNotNull(model);
    
            // try control characters
            TryInvalidControlCharacters(epService);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryInvalidControlCharacters(EPServiceProvider epService) {
            string epl = "select * \u008F from " + typeof(SupportBean).FullName;
            try {
                epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Unrecognized control characters found in text at line 1 column 8 [");
            }
        }
    
        private void AssertStopped(EPServiceProvider epService, EPStatement[] stmts, SupportUpdateListener testListener) {
            foreach (EPStatement stmt in stmts) {
                Assert.AreEqual(EPStatementState.STOPPED, stmt.State);
            }
            SendEvent(epService);
            Assert.AreEqual(0, testListener.NewDataList.Count);
            testListener.Reset();
        }
    
        private void AssertStarted(EPServiceProvider epService, EPStatement[] stmts, SupportUpdateListener testListener) {
            foreach (EPStatement stmt in stmts) {
                Assert.AreEqual(EPStatementState.STARTED, stmt.State);
            }
            SendEvent(epService);
            Assert.AreEqual(stmts.Length, testListener.NewDataList.Count);
            testListener.Reset();
        }
    
        private void AssertDestroyed(EPServiceProvider epService, EPStatement[] stmts, SupportUpdateListener testListener) {
            foreach (EPStatement stmt in stmts) {
                Assert.AreEqual(EPStatementState.DESTROYED, stmt.State);
            }
            SendEvent(epService);
            Assert.AreEqual(0, testListener.NewDataList.Count);
            testListener.Reset();
        }
    
        private EPStatement[] CreateStmts(EPServiceProvider epService, string[] statementNames) {
            var statements = new EPStatement[statementNames.Length];
            for (int i = 0; i < statementNames.Length; i++) {
                statements[i] = epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportBean).FullName, statementNames[i]);
            }
            return statements;
        }
    
        private void SendEvent(EPServiceProvider epService) {
            var bean = new SupportBean();
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace
