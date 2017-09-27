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
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestSubselectUnfiltered
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("S0", typeof(SupportBean_S0));
            config.AddEventType("S1", typeof(SupportBean_S1));
            config.AddEventType("S2", typeof(SupportBean_S2));
            config.AddEventType("S3", typeof(SupportBean_S3));
            config.AddEventType("S4", typeof(SupportBean_S4));
            config.AddEventType("S5", typeof(SupportBean_S5));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestSelfSubselect()
        {
            var stmtTextOne = "insert into MyCount select Count(*) as cnt from S0";
            _epService.EPAdministrator.CreateEPL(stmtTextOne);
    
            var stmtTextTwo = "select (select cnt from MyCount#lastevent) as value from S0";
            var stmt = _epService.EPAdministrator.CreateEPL(stmtTextTwo);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("value"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(1L, _listener.AssertOneGetNewAndReset().Get("value"));
        }
    
        [Test]
        public void TestStartStopStatement()
        {
            var stmtText = "select id from S0 where (select true from S1#length(1000))";
    
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("id"));
    
            stmt.Stop();
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(_listener.IsInvoked);
    
            stmt.Start();
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(3, _listener.AssertOneGetNewAndReset().Get("id"));
        }
    
        [Test]
        public void TestWhereClauseReturningTrue()
        {
            var stmtText = "select id from S0 where (select true from S1#length(1000))";
    
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(2, _listener.AssertOneGetNewAndReset().Get("id"));
        }
    
        [Test]
        public void TestWhereClauseWithExpression()
        {
            var stmtText = "select id from S0 where (select p10='X' from S1#length(1000))";
    
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10, "X"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(0, _listener.AssertOneGetNewAndReset().Get("id"));
        }
    
        [Test]
        public void TestJoinUnfiltered()
        {
            var stmtText = "select (select id from S3#length(1000)) as idS3, (select id from S4#length(1000)) as idS4 from S0#keepall as s0, S1#keepall as s1 where s0.id = s1.id";
    
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS3"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS4"));
    
            // test no event, should return null
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0));
            var theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("idS3"));
            Assert.AreEqual(null, theEvent.Get("idS4"));
    
            // send one event
            _epService.EPRuntime.SendEvent(new SupportBean_S3(-1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, theEvent.Get("idS3"));
            Assert.AreEqual(null, theEvent.Get("idS4"));
    
            // send one event
            _epService.EPRuntime.SendEvent(new SupportBean_S4(-2));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, theEvent.Get("idS3"));
            Assert.AreEqual(-2, theEvent.Get("idS4"));
    
            // send second event
            _epService.EPRuntime.SendEvent(new SupportBean_S4(-2));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(3));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, theEvent.Get("idS3"));
            Assert.AreEqual(null, theEvent.Get("idS4"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S3(-2));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(3));
            var events = _listener.GetNewDataListFlattened();
            Assert.AreEqual(3, events.Length);
            for (var i = 0; i < events.Length; i++)
            {
                Assert.AreEqual(null, events[i].Get("idS3"));
                Assert.AreEqual(null, events[i].Get("idS4"));
            }
        }
    
        [Test]
        public void TestInvalidSubselect()
        {
            TryInvalid("select (select id from S1) from S0",
                       "Error starting statement: Failed to plan subquery number 1 querying S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window (applies to correlated or non-fully-aggregated subqueries) [");
    
            TryInvalid("select (select dummy from S1#lastevent) as idS1 from S0",
                       "Error starting statement: Failed to plan subquery number 1 querying S1: Failed to validate select-clause expression 'dummy': Property named 'dummy' is not valid in any stream [select (select dummy from S1#lastevent) as idS1 from S0]");
    
            TryInvalid("select (select (select id from S1#lastevent) id from S1#lastevent) as idS1 from S0",
                       "Invalid nested subquery, subquery-within-subquery is not supported [select (select (select id from S1#lastevent) id from S1#lastevent) as idS1 from S0]");
    
            TryInvalid("select (select id from S1#lastevent where (sum(id) = 5)) as idS1 from S0",
                       "Error starting statement: Failed to plan subquery number 1 querying S1: Aggregation functions are not supported within subquery filters, consider using a having-clause or insert-into instead [select (select id from S1#lastevent where (sum(id) = 5)) as idS1 from S0]");
    
            TryInvalid("select * from S0(id=5 and (select id from S1))",
                       "Failed to validate subquery number 1 querying S1: Subqueries require one or more views to limit the stream, consider declaring a length or time window [select * from S0(id=5 and (select id from S1))]");
    
            TryInvalid("select * from S0 group by id + (select id from S1)",
                       "Error starting statement: Subselects not allowed within group-by [select * from S0 group by id + (select id from S1)]");
    
            TryInvalid("select * from S0 order by (select id from S1) asc",
                       "Error starting statement: Subselects not allowed within order-by clause [select * from S0 order by (select id from S1) asc]");
    
            TryInvalid("select (select id from S1#lastevent where 'a') from S0",
                       "Error starting statement: Failed to plan subquery number 1 querying S1: Subselect filter expression must return a boolean value [select (select id from S1#lastevent where 'a') from S0]");
    
            TryInvalid("select (select id from S1#lastevent where id = p00) from S0",
                       "Error starting statement: Failed to plan subquery number 1 querying S1: Failed to validate filter expression 'id=p00': Property named 'p00' must be prefixed by a stream name, use the stream name itself or use the as-clause to name the stream with the property in the format \"stream.property\" [select (select id from S1#lastevent where id = p00) from S0]");
    
            TryInvalid("select id in (select * from S1#length(1000)) as value from S0",
                       "Error starting statement: Failed to validate select-clause expression subquery number 1 querying S1: Implicit conversion from datatype '" + Name.Of<SupportBean_S1>() + "' to '" + Name.Of<int>() + "' is not allowed [select id in (select * from S1#length(1000)) as value from S0]");
        }
    
        [Test]
        public void TestUnfilteredStreamPrior_OM()
        {
            var subquery = new EPStatementObjectModel();
            subquery.SelectClause = SelectClause.Create().Add(Expressions.Prior(0, "id"));
            subquery.FromClause = FromClause.Create(FilterStream.Create("S1").AddView("length", Expressions.Constant(1000)));
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create().Add(Expressions.Subquery(subquery), "idS1");
            model.FromClause = FromClause.Create(FilterStream.Create("S0"));
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
    
            var stmtText = "select (select prior(0,id) from S1#length(1000)) as idS1 from S0";
            Assert.AreEqual(stmtText, model.ToEPL());
            var stmt = _epService.EPAdministrator.Create(model);
            RunUnfilteredStreamPrior(stmt);
        }
    
        [Test]
        public void TestUnfilteredStreamPrior_Compile()
        {
            var stmtText = "select (select prior(0,id) from S1#length(1000)) as idS1 from S0";
            var model = _epService.EPAdministrator.CompileEPL(stmtText);
            model = (EPStatementObjectModel) SerializableObjectCopier.Copy(model);
            Assert.AreEqual(stmtText, model.ToEPL());
            var stmt = _epService.EPAdministrator.Create(model);
            RunUnfilteredStreamPrior(stmt);
        }
    
        private void RunUnfilteredStreamPrior(EPStatement stmt)
        {
            stmt.Events += _listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS1"));
    
            // test no event, should return null
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // test one event
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(10, _listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // resend event
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(10, _listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // test second event
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(10, _listener.AssertOneGetNewAndReset().Get("idS1"));
        }
    
        [Test]
        public void TestCustomFunction()
        {
            var stmtText = "select (select " + typeof(SupportStaticMethodLib).FullName + ".MinusOne(id) from S1#length(1000)) as idS1 from S0";
    
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // check type
            Assert.AreEqual(typeof(double?), stmt.EventType.GetPropertyType("idS1"));
    
            // test no event, should return null
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // test one event
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(9d, _listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // resend event
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(9d, _listener.AssertOneGetNewAndReset().Get("idS1"));
        }
    
        [Test]
        public void TestComputedResult()
        {
            var stmtText = "select 100*(select id from S1#length(1000)) as idS1 from S0";
    
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS1"));
    
            // test no event, should return null
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // test one event
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(1000, _listener.AssertOneGetNewAndReset().Get("idS1"));
    
            // resend event
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(1000, _listener.AssertOneGetNewAndReset().Get("idS1"));
        }
    
        [Test]
        public void TestFilterInside()
        {
            var stmtText = "select (select id from S1(p10='A')#length(1000)) as idS1 from S0";
    
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "X"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get("idS1"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1, "A"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("idS1"));
        }
    
        [Test]
        public void TestUnfilteredUnlimitedStream()
        {
            var stmtText = "select (select id from S1#length(1000)) as idS1 from S0";
            RunAssertMultiRowUnfiltered(stmtText, "idS1");
        }
    
        [Test]
        public void TestUnfilteredLengthWindow()
        {
            var stmtText = "select (select id from S1#length(2)) as idS1 from S0";
            RunAssertMultiRowUnfiltered(stmtText, "idS1");
        }
    
        [Test]
        public void TestUnfilteredAsAfterSubselect()
        {
            var stmtText = "select (select id from S1#lastevent) as idS1 from S0";
            RunAssertSingleRowUnfiltered(stmtText, "idS1");
        }
    
        [Test]
        public void TestUnfilteredWithAsWithinSubselect()
        {
            var stmtText = "select (select id as myId from S1#lastevent) from S0";
            RunAssertSingleRowUnfiltered(stmtText, "myId");
        }
    
        [Test]
        public void TestUnfilteredNoAs()
        {
            var stmtText = "select (select id from S1#lastevent) from S0";
            RunAssertSingleRowUnfiltered(stmtText, "id");
        }
    
        [Test]
        public void TestUnfilteredExpression()
        {
            var stmtText = "select (select p10 || p11 from S1#lastevent) as value from S0";
    
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // check type
            Assert.AreEqual(typeof(String), stmt.EventType.GetPropertyType("value"));
    
            // test no event, should return null
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            var theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("value"));
    
            // test one event
            _epService.EPRuntime.SendEvent(new SupportBean_S1(-1, "a", "b"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("ab", theEvent.Get("value"));
        }
    
        [Test]
        public void TestMultiColumnSelect()
        {
            var stmtText = "select (select id+1 as myId from S1#lastevent) as idS1_0, " +
                    "(select id+2 as myId from S1#lastevent) as idS1_1 from S0";
    
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS1_0"));
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType("idS1_1"));
    
            // test no event, should return null
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            var theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(null, theEvent.Get("idS1_0"));
            Assert.AreEqual(null, theEvent.Get("idS1_1"));
    
            // test one event
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(11, theEvent.Get("idS1_0"));
            Assert.AreEqual(12, theEvent.Get("idS1_1"));
    
            // resend event
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(11, theEvent.Get("idS1_0"));
            Assert.AreEqual(12, theEvent.Get("idS1_1"));
    
            // test second event
            _epService.EPRuntime.SendEvent(new SupportBean_S1(999));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1000, theEvent.Get("idS1_0"));
            Assert.AreEqual(1001, theEvent.Get("idS1_1"));
        }
    
        private void RunAssertSingleRowUnfiltered(String stmtText, String columnName)
        {
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(columnName));
    
            // test no event, should return null
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get(columnName));
    
            // test one event
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(10, _listener.AssertOneGetNewAndReset().Get(columnName));
    
            // resend event
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(10, _listener.AssertOneGetNewAndReset().Get(columnName));
    
            // test second event
            _epService.EPRuntime.SendEvent(new SupportBean_S1(999));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(999, _listener.AssertOneGetNewAndReset().Get(columnName));
        }
    
        private void RunAssertMultiRowUnfiltered(String stmtText, String columnName)
        {
            var stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            stmt.Events += _listener.Update;
    
            // check type
            Assert.AreEqual(typeof(int?), stmt.EventType.GetPropertyType(columnName));
    
            // test no event, should return null
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get(columnName));
    
            // test one event
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            Assert.AreEqual(10, _listener.AssertOneGetNewAndReset().Get(columnName));
    
            // resend event
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2));
            Assert.AreEqual(10, _listener.AssertOneGetNewAndReset().Get(columnName));
    
            // test second event
            _epService.EPRuntime.SendEvent(new SupportBean_S1(999));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3));
            Assert.AreEqual(null, _listener.AssertOneGetNewAndReset().Get(columnName));
        }
    
        private void TryInvalid(String stmtText, String expectedMsg)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                SupportMessageAssertUtil.AssertMessage(ex, expectedMsg);
            }
        }
    }
}
