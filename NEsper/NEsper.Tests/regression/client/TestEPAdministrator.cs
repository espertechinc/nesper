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
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.pattern;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestEPAdministrator
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;

        [SetUp]
        public void SetUp()
        {
            _testListener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.LoggingConfig.IsEnableTimerDebug = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _testListener = null;
        }

        [Test]
        public void TestGetStmtByName()
        {
            String[] names = new String[] { "s1", "s2", "s3--0", "s3", "s3" };
            String[] expected = new String[] { "s1", "s2", "s3--0", "s3", "s3--1" };
            EPStatement[] stmts = CreateStmts(names);
            for (int i = 0; i < stmts.Length; i++)
            {
                Assert.AreSame(stmts[i], _epService.EPAdministrator.GetStatement(expected[i]), "failed for " + names[i]);
                Assert.AreEqual(expected[i], _epService.EPAdministrator.GetStatement(expected[i]).Name, "failed for " + names[i]);
            }

            // test statement name trim
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>("S0");
            _epService.EPAdministrator.CreateEPL("@Name(' stmt0  ') select * from S0");
            _epService.EPAdministrator.CreateEPL("select * from S0", "  stmt1  ");
            foreach (String name in new string[]{ "stmt0", "stmt1" }) {
                EPStatement stmt = _epService.EPAdministrator.GetStatement(name);
                Assert.AreEqual(name, stmt.Name);
            }
        }

        [Test]
        public void TestStatementArray()
        {
            Assert.AreEqual(0, _epService.EPAdministrator.StatementNames.Count);

            String[] names = new String[] { "s1" };
            EPStatement[] stmtsSetOne = CreateStmts(names);
            EPAssertionUtil.AssertEqualsAnyOrder(names, _epService.EPAdministrator.StatementNames);

            names = new String[] { "s1", "s2" };
            EPStatement[] stmtsSetTwo = CreateStmts(names);
            EPAssertionUtil.AssertEqualsAnyOrder(new String[] { "s1", "s1--0", "s2" }, _epService.EPAdministrator.StatementNames);
        }

        [Test]
        public void TestCreateEPLByName()
        {
            String stmt = "select * from " + typeof(SupportBean).FullName;
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(stmt, "s1");
            stmtOne.Events += _testListener.Update;
            Assert.AreEqual("s1", stmtOne.Name);
            Assert.AreEqual(stmt, stmtOne.Text);

            // check working
            SendEvent();
            _testListener.AssertOneGetNewAndReset();

            // create a second with the same name
            stmt = "select IntPrimitive from " + typeof(SupportBean).FullName;
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(stmt, "s1");
            Assert.AreEqual("s1--0", stmtTwo.Name);
            Assert.AreEqual(stmt, stmtTwo.Text);

            // create a third invalid statement with the same name
            stmt = "select xxx from " + typeof(SupportBean).FullName;
            try
            {
                _epService.EPAdministrator.CreateEPL(stmt, "s1");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                // expected
            }

            // create a forth statement with the same name
            stmt = "select TheString from " + typeof(SupportBean).FullName;
            EPStatement stmtFour = _epService.EPAdministrator.CreateEPL(stmt, "s1");
            Assert.AreEqual("s1--1", stmtFour.Name);
            Assert.AreEqual(stmt, stmtFour.Text);

            // create a fifth pattern statement with the same name
            stmt = typeof(SupportBean).FullName;
            EPStatement stmtFive = _epService.EPAdministrator.CreatePattern(stmt, "s1");
            Assert.AreEqual("s1--2", stmtFive.Name);
            Assert.AreEqual(stmt, stmtFive.Text);

            // should allow null statement name
            _epService.EPAdministrator.CreatePattern(stmt, null);
        }

        [Test]
        public void TestCreatePatternByName()
        {
            String stmt = typeof(SupportBean).FullName;
            EPStatement stmtOne = _epService.EPAdministrator.CreatePattern(stmt, "s1");
            stmtOne.Events += _testListener.Update;
            Assert.AreEqual("s1", stmtOne.Name);
            Assert.AreEqual(stmt, stmtOne.Text);

            // check working
            SendEvent();
            _testListener.AssertOneGetNewAndReset();

            // create a second with the same name
            stmt = typeof(SupportMarketDataBean).FullName;
            EPStatement stmtTwo = _epService.EPAdministrator.CreatePattern(stmt, "s1");
            Assert.AreEqual("s1--0", stmtTwo.Name);
            Assert.AreEqual(stmt, stmtTwo.Text);

            // create a third invalid statement with the same name
            stmt = "xxx" + typeof(SupportBean).FullName;
            try
            {
                _epService.EPAdministrator.CreatePattern(stmt, "s1");
                Assert.Fail();
            }
            catch (Exception ex)
            {
                // expected
            }

            // create a forth statement with the same name
            stmt = typeof(SupportBean).FullName;
            EPStatement stmtFour = _epService.EPAdministrator.CreatePattern(stmt, "s1");
            Assert.AreEqual("s1--1", stmtFour.Name);
            Assert.AreEqual(stmt, stmtFour.Text);

            // create a fifth pattern statement with the same name
            stmt = "select * from " + typeof(SupportBean).FullName;
            EPStatement stmtFive = _epService.EPAdministrator.CreateEPL(stmt, "s1");
            Assert.AreEqual("s1--2", stmtFive.Name);
            Assert.AreEqual(stmt, stmtFive.Text);

            // Null statement names should be allowed
            _epService.EPAdministrator.CreatePattern("every " + typeof(SupportBean).FullName, null);
            _epService.EPAdministrator.DestroyAllStatements();
        }

        [Test]
        public void TestDestroyAll()
        {
            EPStatement[] stmts = CreateStmts(new String[] { "s1", "s2", "s3" });
            stmts[0].Events += _testListener.Update;
            stmts[1].Events += _testListener.Update;
            stmts[2].Events += _testListener.Update;
            SendEvent();
            Assert.AreEqual(3, _testListener.NewDataList.Count);
            _testListener.Reset();

            _epService.EPAdministrator.DestroyAllStatements();
            AssertDestroyed(stmts);
        }

        [Test]
        public void TestStopStartAll()
        {
            EPStatement[] stmts = CreateStmts(new String[] { "s1", "s2", "s3" });
            stmts[0].Events += _testListener.Update;
            stmts[1].Events += _testListener.Update;
            stmts[2].Events += _testListener.Update;

            AssertStarted(stmts);

            _epService.EPAdministrator.StopAllStatements();
            AssertStopped(stmts);

            _epService.EPAdministrator.StartAllStatements();
            AssertStarted(stmts);

            _epService.EPAdministrator.DestroyAllStatements();
            AssertDestroyed(stmts);
        }

        [Test]
        public void TestStopStartSome()
        {
            EPStatement[] stmts = CreateStmts(new String[] { "s1", "s2", "s3" });
            stmts[0].Events += _testListener.Update;
            stmts[1].Events += _testListener.Update;
            stmts[2].Events += _testListener.Update;
            AssertStarted(stmts);

            stmts[0].Stop();
            SendEvent();
            Assert.AreEqual(2, _testListener.NewDataList.Count);
            _testListener.Reset();

            _epService.EPAdministrator.StopAllStatements();
            AssertStopped(stmts);

            stmts[1].Start();
            SendEvent();
            Assert.AreEqual(1, _testListener.NewDataList.Count);
            _testListener.Reset();

            _epService.EPAdministrator.StartAllStatements();
            AssertStarted(stmts);

            _epService.EPAdministrator.DestroyAllStatements();
            AssertDestroyed(stmts);
        }

        [Test]
        public void TestSPI()
        {
            EPAdministratorSPI spi = (EPAdministratorSPI)_epService.EPAdministrator;

            ExprDotNode funcnode = (ExprDotNode)spi.CompileExpression("func()");
            Assert.IsFalse(funcnode.ChainSpec[0].IsProperty);

            ExprNode node = spi.CompileExpression("value=5 and /* comment */ True");
            Assert.AreEqual("value=5 and true", node.ToExpressionStringMinPrecedenceSafe());

            Expression expr = spi.CompileExpressionToSODA("value=5 and True");
            StringWriter buf = new StringWriter();
            expr.ToEPL(buf, ExpressionPrecedenceEnum.MINIMUM);
            Assert.AreEqual("value=5 and true", buf.ToString());

            expr = spi.CompileExpressionToSODA("5 sec");
            buf = new StringWriter();
            expr.ToEPL(buf, ExpressionPrecedenceEnum.MINIMUM);
            Assert.AreEqual("5 seconds", buf.ToString());

            EvalFactoryNode pattern = spi.CompilePatternToNode("every A -> B");
            Assert.That(pattern, Is.InstanceOf<EvalFollowedByFactoryNode>());

            PatternExpr patternExpr = spi.CompilePatternToSODA("every A -> B");
            Assert.AreEqual(typeof(PatternFollowedByExpr), patternExpr.GetType());

            EPStatementObjectModel modelPattern = spi.CompilePatternToSODAModel("@Name('test') every A -> B");
            Assert.AreEqual("Name", modelPattern.Annotations[0].Name);
            Assert.AreEqual(typeof(PatternFollowedByExpr), ((PatternStream)modelPattern.FromClause.Streams[0]).Expression.GetType());

            AnnotationPart part = spi.CompileAnnotationToSODA("@somevalue(a='test', b=5)");
            Assert.AreEqual("somevalue", part.Name);
            Assert.AreEqual(2, part.Attributes.Count);
            Assert.AreEqual("a", part.Attributes[0].Name);
            Assert.AreEqual("test", part.Attributes[0].Value);
            Assert.AreEqual("b", part.Attributes[1].Name);
            Assert.AreEqual(5, part.Attributes[1].Value);

            MatchRecognizeRegEx regex = spi.CompileMatchRecognizePatternToSODA("a b* c+ d? e?");
            Assert.AreEqual(5, regex.Children.Count);

            // test fail cases
            string expected = "Incorrect syntax near 'in' (a reserved keyword) at line 1 column 46 [goofy in in]";
            String compiled = "goofy in in";
            try
            {
                spi.CompileExpression(compiled);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }

            try
            {
                spi.CompileExpressionToSODA(compiled);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }

            expected = "Incorrect syntax near 'in' (a reserved keyword) at line 1 column 6 [goofy in in]";
            try
            {
                spi.CompilePatternToNode(compiled);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }

            try
            {
                spi.CompilePatternToSODA(compiled);
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual(expected, ex.Message);
            }

            try
            {
                spi.CompileAnnotationToSODA("not an annotation");
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual("Incorrect syntax near 'not' (a reserved keyword) [not an annotation]", ex.Message);
            }

            try
            {
                spi.CompileMatchRecognizePatternToSODA("a b???");
                Assert.Fail();
            }
            catch (EPException ex)
            {
                Assert.AreEqual("Incorrect syntax near '?' expecting a closing parenthesis ')' but found a questionmark '?' at line 1 column 76 [a b???]", ex.Message);
            }

            StatementSpecRaw raw = spi.CompileEPLToRaw("select * from System.Object");
            Assert.NotNull(raw);
            EPStatementObjectModel model = spi.MapRawToSODA(raw);
            Assert.NotNull(model);

            // try control characters
            TryInvalidControlCharacters();
        }

        private void TryInvalidControlCharacters()
        {
            String epl = "select * \u008F from " + typeof(SupportBean).FullName;
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                SupportMessageAssertUtil.AssertMessage(ex, "Unrecognized control characters found in text at line 1 column 8 [");
            }
        }

        private void AssertStopped(EPStatement[] stmts)
        {
            for (int i = 0; i < stmts.Length; i++)
            {
                Assert.AreEqual(EPStatementState.STOPPED, stmts[i].State);
            }
            SendEvent();
            Assert.AreEqual(0, _testListener.NewDataList.Count);
            _testListener.Reset();
        }

        private void AssertStarted(EPStatement[] stmts)
        {
            for (int i = 0; i < stmts.Length; i++)
            {
                Assert.AreEqual(EPStatementState.STARTED, stmts[i].State);
            }
            SendEvent();
            Assert.AreEqual(stmts.Length, _testListener.NewDataList.Count);
            _testListener.Reset();
        }

        private void AssertDestroyed(EPStatement[] stmts)
        {
            for (int i = 0; i < stmts.Length; i++)
            {
                Assert.AreEqual(EPStatementState.DESTROYED, stmts[i].State);
            }
            SendEvent();
            Assert.AreEqual(0, _testListener.NewDataList.Count);
            _testListener.Reset();
        }

        private EPStatement[] CreateStmts(String[] statementNames)
        {
            EPStatement[] statements = new EPStatement[statementNames.Length];
            for (int i = 0; i < statementNames.Length; i++)
            {
                statements[i] = _epService.EPAdministrator.CreateEPL("select * from " + typeof(SupportBean).FullName, statementNames[i]);
            }
            return statements;
        }

        private void SendEvent()
        {
            SupportBean bean = new SupportBean();
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}
