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
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestEPStatementObjectModel 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        // This is a simple EPL only.
        // Each OM/SODA Api is tested in it's respective unit test (i.e. TestInsertInto), including ToEPL()
        // 
        [Test]
        public void TestCreateFromOM()
        {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            SerializableObjectCopier.Copy(model);
    
            EPStatement stmt = _epService.EPAdministrator.Create(model, "s1");
            stmt.Events += _listener.Update;
    
            Object theEvent = new SupportBean();
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(theEvent, _listener.AssertOneGetNewAndReset().Underlying);
        }
    
        // This is a simple EPL only.
        // Each OM/SODA Api is tested in it's respective unit test (i.e. TestInsertInto), including ToEPL()
        //
        [Test]
        public void TestCreateFromOMComplete()
        {
            var model = new EPStatementObjectModel();
            model.InsertInto = InsertIntoClause.Create("ReadyStreamAvg", "line", "avgAge");
            model.SelectClause = SelectClause.Create()
                .Add("line")
                .Add(Expressions.Avg("age"), "avgAge");
            var filter = Filter.Create(typeof(SupportBean).FullName, Expressions.In("line", 1, 8, 10));
            model.FromClause = FromClause.Create(FilterStream.Create(filter, "RS").AddView("time", Expressions.Constant(10)));
            model.WhereClause = Expressions.IsNotNull("waverId");
            model.GroupByClause = GroupByClause.Create("line");
            model.HavingClause = Expressions.Lt(Expressions.Avg("age"), Expressions.Constant(0));
            model.OutputLimitClause = OutputLimitClause.Create(Expressions.TimePeriod(null, null, null, 10, null));
            model.OrderByClause = OrderByClause.Create("line");                
    
            Assert.AreEqual("insert into ReadyStreamAvg(line, avgAge) select line, avg(age) as avgAge from " + Name.Of<SupportBean>() + "(line in (1,8,10))#time(10) as RS where waverId is not null group by line having avg(age)<0 output every 10 seconds order by line", model.ToEPL());
            SerializableObjectCopier.Copy(model);
        }
    
        [Test]
        public void TestCompileToOM()
        {
            var stmtText = "select * from " + typeof(SupportBean).FullName;
            var model = _epService.EPAdministrator.CompileEPL(stmtText);
            SerializableObjectCopier.Copy(model);
            Assert.NotNull(model);
        }
        
        [Test]
        public void TestEPLtoOMtoStmt()
        {
            var stmtText = "select * from " + typeof(SupportBean).FullName;
            var model = _epService.EPAdministrator.CompileEPL(stmtText);
            SerializableObjectCopier.Copy(model);
    
            EPStatement stmt = _epService.EPAdministrator.Create(model, "s1");
            stmt.Events += _listener.Update;
    
            Object theEvent = new SupportBean();
            _epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(theEvent, _listener.AssertOneGetNewAndReset().Underlying);
            Assert.AreEqual(stmtText, stmt.Text);
            Assert.AreEqual("s1", stmt.Name);
        }
    
        [Test]
        public void TestPrecedenceExpressions()
        {
            String[][] testdata = {
                new String[] {"1+2*3", null, "ArithmaticExpression"},
                new String[] {"1+(2*3)", "1+2*3", "ArithmaticExpression"},
                new String[] {"2-2/3-4", null, "ArithmaticExpression"},
                new String[] {"2-(2/3)-4", "2-2/3-4", "ArithmaticExpression"},
                new String[] {"1+2 in (4,5)", null, "InExpression"},
                new String[] {"(1+2) in (4,5)", "1+2 in (4,5)", "InExpression"},
                new String[] {"true and false or true", "true and false or true", "Disjunction"},
                new String[] {"(true and false) or true", "true and false or true", "Disjunction"},
                new String[] {"true and (false or true)", "true and (false or true)", "Conjunction"},
                new String[] {"true and (((false or true)))", "true and (false or true)", "Conjunction"},
                new String[] {"true and (((false or true)))", "true and (false or true)", "Conjunction"},
                new String[] {"false or false and true or false", "false or false and true or false", "Disjunction"},
                new String[] {"false or (false and true) or false", "false or false and true or false", "Disjunction"},
                new String[] {"\"a\"||\"b\"=\"ab\"", null, "RelationalOpExpression"},
                new String[] {"(\"a\"||\"b\")=\"ab\"", "\"a\"||\"b\"=\"ab\"", "RelationalOpExpression"},
                };
            
            for (int i = 0; i < testdata.Length; i++) {
    
                String epl = "select * from System.Object where " + testdata[i][0];
                String expected = testdata[i][1];
                String expressionLowestPrecedenceClass = testdata[i][2];
    
                EPStatementObjectModel modelBefore = _epService.EPAdministrator.CompileEPL(epl);
                String eplAfter = modelBefore.ToEPL();
    
                if (expected == null) {
                    Assert.AreEqual(epl, eplAfter);
                }
                else {
                    String expectedEPL = "select * from System.Object where " + expected;
                    Assert.AreEqual(expectedEPL, eplAfter);
                }
    
                // get where clause root expression of both models
                EPStatementObjectModel modelAfter = _epService.EPAdministrator.CompileEPL(eplAfter);
                Assert.AreEqual(modelAfter.WhereClause.GetType(), modelBefore.WhereClause.GetType());
                Assert.AreEqual(expressionLowestPrecedenceClass, modelAfter.WhereClause.GetType().Name);
            }
        }
    
        [Test]
        public void TestPrecedencePatterns()
        {
            _epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            _epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportBean_B));
            _epService.EPAdministrator.Configuration.AddEventType("C", typeof(SupportBean_C));
            _epService.EPAdministrator.Configuration.AddEventType("D", typeof(SupportBean_D));
    
            String[][] testdata = {
                    new String[] {"A or B and C", null, "PatternOrExpr"},
                    new String[] {"(A or B) and C", null, "PatternAndExpr"},
                    new String[] {"(A or B) and C", null, "PatternAndExpr"},
                    new String[] {"every A or every B", null, "PatternOrExpr"},
                    new String[] {"B -> D or A", null, "PatternFollowedByExpr"},
                    new String[] {"every A and not B", null, "PatternAndExpr"},
                    new String[] {"every A and not B", null, "PatternAndExpr"},
                    new String[] {"every A -> B", null, "PatternFollowedByExpr"},
                    new String[] {"A where timer:within(10)", null, "PatternGuardExpr"},
                    new String[] {"every (A and B)", null, "PatternEveryExpr"},
                    new String[] {"every A where timer:within(10)", null, "PatternEveryExpr"},
                    new String[] {"A or B until C", null, "PatternOrExpr"},
                    new String[] {"A or (B until C)", "A or B until C", "PatternOrExpr"},
                    new String[] {"every (every A)", null, "PatternEveryExpr"},
                    new String[] {"(A until B) until C", null, "PatternMatchUntilExpr"},
                };
    
            for (int i = 0; i < testdata.Length; i++) {
    
                String epl = "select * from pattern [" + testdata[i][0] + "]";
                String expected = testdata[i][1];
                String expressionLowestPrecedenceClass = testdata[i][2];
                String failText = "Failed for [" +  testdata[i][0] + "]";
    
                EPStatementObjectModel modelBefore = _epService.EPAdministrator.CompileEPL(epl);
                String eplAfter = modelBefore.ToEPL();
    
                if (expected == null) {
                    if (epl != eplAfter)
                    {
                        Assert.Fail();
                    }
                    Assert.AreEqual(epl, eplAfter, failText);
                }
                else {
                    String expectedEPL = "select * from pattern [" + expected + "]";
                    Assert.AreEqual(expectedEPL, eplAfter, failText);
                }
    
                // get where clause root expression of both models
                EPStatementObjectModel modelAfter = _epService.EPAdministrator.CompileEPL(eplAfter);
                Assert.AreEqual(GetPatternRootExpr(modelAfter).GetType(), GetPatternRootExpr(modelBefore).GetType(),
                                failText);
                Assert.AreEqual(expressionLowestPrecedenceClass, GetPatternRootExpr(modelAfter).GetType().Name,
                                failText);
            }
        }
    
        private PatternExpr GetPatternRootExpr(EPStatementObjectModel model) {
            PatternStream patternStrema = (PatternStream) model.FromClause.Streams[0];
            return patternStrema.Expression;
        }
    }
}
