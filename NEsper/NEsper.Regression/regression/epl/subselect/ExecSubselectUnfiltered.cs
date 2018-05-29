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
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;


using NUnit.Framework;

namespace com.espertech.esper.regression.epl.subselect
{
    public class ExecSubselectUnfiltered : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("S0", typeof(SupportBean_S0));
            configuration.AddEventType("S1", typeof(SupportBean_S1));
            configuration.AddEventType("S2", typeof(SupportBean_S2));
            configuration.AddEventType("S3", typeof(SupportBean_S3));
            configuration.AddEventType("S4", typeof(SupportBean_S4));
            configuration.AddEventType("S5", typeof(SupportBean_S5));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionSelfSubselect(epService);
            RunAssertionStartStopStatement(epService);
            RunAssertionWhereClauseReturningTrue(epService);
            RunAssertionWhereClauseWithExpression(epService);
            RunAssertionJoinUnfiltered(epService);
            RunAssertionInvalidSubselect(epService);
            RunAssertionUnfilteredStreamPrior_OM(epService);
            RunAssertionUnfilteredStreamPrior_Compile(epService);
            RunAssertionCustomFunction(epService);
            RunAssertionComputedResult(epService);
            RunAssertionFilterInside(epService);
            RunAssertionUnfilteredUnlimitedStream(epService);
            RunAssertionUnfilteredLengthWindow(epService);
            RunAssertionUnfilteredAsAfterSubselect(epService);
            RunAssertionUnfilteredWithAsWithinSubselect(epService);
            RunAssertionUnfilteredNoAs(epService);
            RunAssertionUnfilteredExpression(epService);
            RunAssertionMultiColumnSelect(epService);
        }
    
        private void RunAssertionSelfSubselect(EPServiceProvider epService) {
            string stmtTextOne = "insert into MyCount select count(*) as cnt from S0";
            epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            string stmtTextTwo = "select (select cnt from MyCount#lastevent) as value from S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtTextTwo);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("value"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("value"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionStartStopStatement(EPServiceProvider epService) {
            string stmtText = "select id from S0 where (select true from S1#length(1000))";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("id"));
    
            stmt.Stop();
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Start();
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(3, listener.AssertOneGetNewAndReset().Get("id"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionWhereClauseReturningTrue(EPServiceProvider epService) {
            string stmtText = "select id from S0 where (select true from S1#length(1000))";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2, listener.AssertOneGetNewAndReset().Get("id"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionWhereClauseWithExpression(EPServiceProvider epService) {
            string stmtText = "select id from S0 where (select p10='X' from S1#length(1000))";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(10, "X"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(0, listener.AssertOneGetNewAndReset().Get("id"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinUnfiltered(EPServiceProvider epService) {
            string stmtText = "select (select id from S3#length(1000)) as idS3, (select id from S4#length(1000)) as idS4 from S0#keepall as s0, S1#keepall as s1 where s0.id = s1.id";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS3"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS4"));
    
            // test no event, should return null
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            epService.EPRuntime.SendEvent(new SupportBean_S1(0));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("idS3"));
            Assert.AreEqual(null, theEvent.Get("idS4"));
    
            // send one event
            epService.EPRuntime.SendEvent(new SupportBean_S3(-1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, theEvent.Get("idS3"));
            Assert.AreEqual(null, theEvent.Get("idS4"));
    
            // send one event
            epService.EPRuntime.SendEvent(new SupportBean_S4(-2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, theEvent.Get("idS3"));
            Assert.AreEqual(-2, theEvent.Get("idS4"));
    
            // send second event
            epService.EPRuntime.SendEvent(new SupportBean_S4(-2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            epService.EPRuntime.SendEvent(new SupportBean_S1(3));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, theEvent.Get("idS3"));
            Assert.AreEqual(null, theEvent.Get("idS4"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S3(-2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            epService.EPRuntime.SendEvent(new SupportBean_S1(3));
            EventBean[] events = listener.GetNewDataListFlattened();
            Assert.AreEqual(3, events.Length);
            for (int i = 0; i < events.Length; i++) {
                Assert.AreEqual(null, events[i].Get("idS3"));
                Assert.AreEqual(null, events[i].Get("idS4"));
            }
    
            stmt.Dispose();
        }
    
        private void RunAssertionInvalidSubselect(EPServiceProvider epService) {
            TryInvalid(epService, "select (select id from S1) from S0",
                    "Error starting statement: Failed to plan subquery number 1 querying S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window (applies to correlated or non-fully-aggregated subqueries) [");
    
            TryInvalid(epService, "select (select dummy from S1#lastevent) as idS1 from S0",
                    "Error starting statement: Failed to plan subquery number 1 querying S1: Failed to validate select-clause expression 'dummy': Property named 'dummy' is not valid in any stream [select (select dummy from S1#lastevent) as idS1 from S0]");
    
            TryInvalid(epService, "select (select (select id from S1#lastevent) id from S1#lastevent) as idS1 from S0",
                    "Invalid nested subquery, subquery-within-subquery is not supported [select (select (select id from S1#lastevent) id from S1#lastevent) as idS1 from S0]");
    
            TryInvalid(epService, "select (select id from S1#lastevent where (sum(id) = 5)) as idS1 from S0",
                    "Error starting statement: Failed to plan subquery number 1 querying S1: Aggregation functions are not supported within subquery filters, consider using a having-clause or insert-into instead [select (select id from S1#lastevent where (sum(id) = 5)) as idS1 from S0]");
    
            TryInvalid(epService, "select * from S0(id=5 and (select id from S1))",
                    "Failed to validate subquery number 1 querying S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window [select * from S0(id=5 and (select id from S1))]");
    
            TryInvalid(epService, "select * from S0 group by id + (select id from S1)",
                    "Error starting statement: Subselects not allowed within group-by [select * from S0 group by id + (select id from S1)]");
    
            TryInvalid(epService, "select * from S0 order by (select id from S1) asc",
                    "Error starting statement: Subselects not allowed within order-by clause [select * from S0 order by (select id from S1) asc]");
    
            TryInvalid(epService, "select (select id from S1#lastevent where 'a') from S0",
                    "Error starting statement: Failed to plan subquery number 1 querying S1: Subselect filter expression must return a boolean value [select (select id from S1#lastevent where 'a') from S0]");
    
            TryInvalid(epService, "select (select id from S1#lastevent where id = p00) from S0",
                    "Error starting statement: Failed to plan subquery number 1 querying S1: Failed to validate filter expression 'id=p00': Property named 'p00' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\" [select (select id from S1#lastevent where id = p00) from S0]");
    
            TryInvalid(epService, "select id in (select * from S1#length(1000)) as value from S0",
                    "Error starting statement: Failed to validate select-clause expression subquery number 1 querying S1: Implicit conversion from datatype '" + Name.Clean<SupportBean_S1>() + "' to '" + Name.Clean<int>() + "' is not allowed [select id in (select * from S1#length(1000)) as value from S0]");
        }
    
        private void RunAssertionUnfilteredStreamPrior_OM(EPServiceProvider epService) {
            var subquery = new EPStatementObjectModel();
            subquery.SelectClause = SelectClause.Create().Add(Expressions.Prior(0, "id"));
            subquery.FromClause = FromClause.Create(
                FilterStream.Create("S1").AddView("length", Expressions.Constant(1000)));
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "idS1");
            model.FromClause = FromClause.Create(FilterStream.Create("S0"));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
    
            string stmtText = "select (select prior(0,id) from S1#length(1000)) as idS1 from S0";
            Assert.AreEqual(stmtText, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            RunUnfilteredStreamPrior(epService, stmt);
            stmt.Dispose();
        }
    
        private void RunAssertionUnfilteredStreamPrior_Compile(EPServiceProvider epService) {
            string stmtText = "select (select prior(0,id) from S1#length(1000)) as idS1 from S0";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(epService.Container, model);
            Assert.AreEqual(stmtText, model.ToEPL());
            EPStatement stmt = epService.EPAdministrator.Create(model);
            RunUnfilteredStreamPrior(epService, stmt);
            stmt.Dispose();
        }
    
        private void RunUnfilteredStreamPrior(EPServiceProvider epService, EPStatement stmt) {
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS1"));
    
            // test no event, should return null
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // test one event
            epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // resend event
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // test second event
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get("idS1"));
        }
    
        private void RunAssertionCustomFunction(EPServiceProvider epService) {
            string stmtText = "select (select " + typeof(SupportStaticMethodLib).FullName + ".MinusOne(id) from S1#length(1000)) as idS1 from S0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // check type
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("idS1"));
    
            // test no event, should return null
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // test one event
            epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(9d, listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // resend event
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(9d, listener.AssertOneGetNewAndReset().Get("idS1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionComputedResult(EPServiceProvider epService) {
            string stmtText = "select 100*(select id from S1#length(1000)) as idS1 from S0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS1"));
    
            // test no event, should return null
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // test one event
            epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(1000, listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // resend event
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(1000, listener.AssertOneGetNewAndReset().Get("idS1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionFilterInside(EPServiceProvider epService) {
            string stmtText = "select (select id from S1(p10='A')#length(1000)) as idS1 from S0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "X"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get("idS1"));
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(1, "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("idS1"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionUnfilteredUnlimitedStream(EPServiceProvider epService) {
            string stmtText = "select (select id from S1#length(1000)) as idS1 from S0";
            TryAssertMultiRowUnfiltered(epService, stmtText, "idS1");
        }
    
        private void RunAssertionUnfilteredLengthWindow(EPServiceProvider epService) {
            string stmtText = "select (select id from S1#length(2)) as idS1 from S0";
            TryAssertMultiRowUnfiltered(epService, stmtText, "idS1");
        }
    
        private void RunAssertionUnfilteredAsAfterSubselect(EPServiceProvider epService) {
            string stmtText = "select (select id from S1#lastevent) as idS1 from S0";
            TryAssertSingleRowUnfiltered(epService, stmtText, "idS1");
        }
    
        private void RunAssertionUnfilteredWithAsWithinSubselect(EPServiceProvider epService) {
            string stmtText = "select (select id as myId from S1#lastevent) from S0";
            TryAssertSingleRowUnfiltered(epService, stmtText, "myId");
        }
    
        private void RunAssertionUnfilteredNoAs(EPServiceProvider epService) {
            string stmtText = "select (select id from S1#lastevent) from S0";
            TryAssertSingleRowUnfiltered(epService, stmtText, "id");
        }
    
        private void RunAssertionUnfilteredExpression(EPServiceProvider epService) {
            string stmtText = "select (select p10 || p11 from S1#lastevent) as value from S0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // check type
            Assert.AreEqual(typeof(string), stmt.EventType.GetPropertyType("value"));
    
            // test no event, should return null
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("value"));
    
            // test one event
            epService.EPRuntime.SendEvent(new SupportBean_S1(-1, "a", "b"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual("ab", theEvent.Get("value"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionMultiColumnSelect(EPServiceProvider epService) {
            string stmtText = "select (select id+1 as myId from S1#lastevent) as idS1_0, " +
                    "(select id+2 as myId from S1#lastevent) as idS1_1 from S0";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS1_0"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS1_1"));
    
            // test no event, should return null
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("idS1_0"));
            Assert.AreEqual(null, theEvent.Get("idS1_1"));
    
            // test one event
            epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(11, theEvent.Get("idS1_0"));
            Assert.AreEqual(12, theEvent.Get("idS1_1"));
    
            // resend event
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(11, theEvent.Get("idS1_0"));
            Assert.AreEqual(12, theEvent.Get("idS1_1"));
    
            // test second event
            epService.EPRuntime.SendEvent(new SupportBean_S1(999));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            theEvent = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1000, theEvent.Get("idS1_0"));
            Assert.AreEqual(1001, theEvent.Get("idS1_1"));
    
            stmt.Dispose();
        }
    
        private void TryAssertSingleRowUnfiltered(EPServiceProvider epService, string stmtText, string columnName) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(columnName));
    
            // test no event, should return null
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get(columnName));
    
            // test one event
            epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get(columnName));
    
            // resend event
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get(columnName));
    
            // test second event
            epService.EPRuntime.SendEvent(new SupportBean_S1(999));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(999, listener.AssertOneGetNewAndReset().Get(columnName));
    
            stmt.Dispose();
        }
    
        private void TryAssertMultiRowUnfiltered(EPServiceProvider epService, string stmtText, string columnName) {
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(columnName));
    
            // test no event, should return null
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get(columnName));
    
            // test one event
            epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get(columnName));
    
            // resend event
            epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(10, listener.AssertOneGetNewAndReset().Get(columnName));
    
            // test second event
            epService.EPRuntime.SendEvent(new SupportBean_S1(999));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(null, listener.AssertOneGetNewAndReset().Get(columnName));
    
            stmt.Dispose();
        }
    }
} // end of namespace
