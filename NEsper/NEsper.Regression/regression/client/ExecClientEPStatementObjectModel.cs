///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientEPStatementObjectModel : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionCreateFromOM(epService);
            RunAssertionCreateFromOMComplete(epService);
            RunAssertionCompileToOM(epService);
            RunAssertionEPLtoOMtoStmt(epService);
            RunAssertionPrecedenceExpressions(epService);
            RunAssertionPrecedencePatterns(epService);
        }
    
        // This is a simple EPL only.
        // Each OM/SODA Api is tested in it's respective unit test (i.e. TestInsertInto), including ToEPL()
        //
        private void RunAssertionCreateFromOM(EPServiceProvider epService) {
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.CreateWildcard();
            model.FromClause = FromClause.Create(FilterStream.Create(typeof(SupportBean).FullName));
            SerializableObjectCopier.Copy(epService.Container, model);
    
            var stmt = epService.EPAdministrator.Create(model, "s1");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var theEvent = new SupportBean();
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(theEvent, listener.AssertOneGetNewAndReset().Underlying);
    
            stmt.Dispose();
        }
    
        // This is a simple EPL only.
        // Each OM/SODA Api is tested in it's respective unit test (i.e. TestInsertInto), including ToEPL()
        //
        private void RunAssertionCreateFromOMComplete(EPServiceProvider epService) {
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
    
            Assert.AreEqual("insert into ReadyStreamAvg(line, avgAge) select line, avg(age) as avgAge from " + typeof(SupportBean).FullName + "(line in (1,8,10))#time(10) as RS where waverId is not null group by line having avg(age)<0 output every 10 seconds order by line", model.ToEPL());
            SerializableObjectCopier.Copy(epService.Container, model);
        }
    
        private void RunAssertionCompileToOM(EPServiceProvider epService) {
            var stmtText = "select * from " + typeof(SupportBean).FullName;
            var model = epService.EPAdministrator.CompileEPL(stmtText);
            SerializableObjectCopier.Copy(epService.Container, model);
            Assert.IsNotNull(model);
        }
    
        private void RunAssertionEPLtoOMtoStmt(EPServiceProvider epService) {
            var stmtText = "select * from " + typeof(SupportBean).FullName;
            var model = epService.EPAdministrator.CompileEPL(stmtText);
            SerializableObjectCopier.Copy(epService.Container, model);
    
            var stmt = epService.EPAdministrator.Create(model, "s1");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            var theEvent = new SupportBean();
            epService.EPRuntime.SendEvent(theEvent);
            Assert.AreEqual(theEvent, listener.AssertOneGetNewAndReset().Underlying);
            Assert.AreEqual(stmtText, stmt.Text);
            Assert.AreEqual("s1", stmt.Name);
    
            stmt.Dispose();
        }
    
        private void RunAssertionPrecedenceExpressions(EPServiceProvider epService) {
            string[][] testdata = {
                    new[] {"1+2*3", null, "ArithmaticExpression"},
                    new[] {"1+(2*3)", "1+2*3", "ArithmaticExpression"},
                    new[] {"2-2/3-4", null, "ArithmaticExpression"},
                    new[] {"2-(2/3)-4", "2-2/3-4", "ArithmaticExpression"},
                    new[] {"1+2 in (4,5)", null, "InExpression"},
                    new[] {"(1+2) in (4,5)", "1+2 in (4,5)", "InExpression"},
                    new[] {"true and false or true", null, "Disjunction"},
                    new[] {"(true and false) or true", "true and false or true", "Disjunction"},
                    new[] {"true and (false or true)", null, "Conjunction"},
                    new[] {"true and (((false or true)))", "true and (false or true)", "Conjunction"},
                    new[] {"true and (((false or true)))", "true and (false or true)", "Conjunction"},
                    new[] {"false or false and true or false", null, "Disjunction"},
                    new[] {"false or (false and true) or false", "false or false and true or false", "Disjunction"},
                    new[] {"\"a\"||\"b\"=\"ab\"", null, "RelationalOpExpression"},
                    new[] {"(\"a\"||\"b\")=\"ab\"", "\"a\"||\"b\"=\"ab\"", "RelationalOpExpression"},
            };
    
            foreach (var aTestdata in testdata) {
    
                var epl = "select * from System.Object where " + aTestdata[0];
                var expected = aTestdata[1];
                var expressionLowestPrecedenceClass = aTestdata[2];
    
                var modelBefore = epService.EPAdministrator.CompileEPL(epl);
                var eplAfter = modelBefore.ToEPL();
    
                if (expected == null) {
                    Assert.AreEqual(epl, eplAfter);
                } else {
                    var expectedEPL = "select * from System.Object where " + expected;
                    Assert.AreEqual(expectedEPL, eplAfter);
                }
    
                // get where clause root expression of both models
                var modelAfter = epService.EPAdministrator.CompileEPL(eplAfter);
                Assert.AreEqual(modelAfter.WhereClause.GetType(), modelBefore.WhereClause.GetType());
                Assert.AreEqual(expressionLowestPrecedenceClass, modelAfter.WhereClause.GetType().Name);
            }
        }
    
        private void RunAssertionPrecedencePatterns(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType("B", typeof(SupportBean_B));
            epService.EPAdministrator.Configuration.AddEventType("C", typeof(SupportBean_C));
            epService.EPAdministrator.Configuration.AddEventType("D", typeof(SupportBean_D));
    
            string[][] testdata = {
                    new[] {"A or B and C", null, "PatternOrExpr"},
                    new[] {"(A or B) and C", null, "PatternAndExpr"},
                    new[] {"(A or B) and C", null, "PatternAndExpr"},
                    new[] {"every A or every B", null, "PatternOrExpr"},
                    new[] {"B -> D or A", null, "PatternFollowedByExpr"},
                    new[] {"every A and not B", null, "PatternAndExpr"},
                    new[] {"every A and not B", null, "PatternAndExpr"},
                    new[] {"every A -> B", null, "PatternFollowedByExpr"},
                    new[] {"A where timer:within(10)", null, "PatternGuardExpr"},
                    new[] {"every (A and B)", null, "PatternEveryExpr"},
                    new[] {"every A where timer:within(10)", null, "PatternEveryExpr"},
                    new[] {"A or B until C", null, "PatternOrExpr"},
                    new[] {"A or (B until C)", "A or B until C", "PatternOrExpr"},
                    new[] {"every (every A)", null, "PatternEveryExpr"},
                    new[] {"(A until B) until C", null, "PatternMatchUntilExpr"},
            };
    
            foreach (var aTestdata in testdata) {
    
                var epl = "select * from pattern [" + aTestdata[0] + "]";
                var expected = aTestdata[1];
                var expressionLowestPrecedenceClass = aTestdata[2];
                var failText = "Failed for [" + aTestdata[0] + "]";
    
                var modelBefore = epService.EPAdministrator.CompileEPL(epl);
                var eplAfter = modelBefore.ToEPL();
    
                if (expected == null) {
                    Assert.AreEqual(epl, eplAfter, failText);
                } else {
                    var expectedEPL = "select * from pattern [" + expected + "]";
                    Assert.AreEqual(expectedEPL, eplAfter, failText);
                }
    
                // get where clause root expression of both models
                var modelAfter = epService.EPAdministrator.CompileEPL(eplAfter);
                Assert.AreEqual(GetPatternRootExpr(modelAfter).GetType(), GetPatternRootExpr(modelBefore).GetType(), failText);
                Assert.AreEqual(expressionLowestPrecedenceClass, GetPatternRootExpr(modelAfter).GetType().Name, failText);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private PatternExpr GetPatternRootExpr(EPStatementObjectModel model) {
            var patternStream = (PatternStream) model.FromClause.Streams[0];
            return patternStream.Expression;
        }
    }
} // end of namespace
