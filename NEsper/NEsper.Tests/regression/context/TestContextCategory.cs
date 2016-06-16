///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextCategory
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private EPServiceProviderSPI _spi;
    
        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _spi = (EPServiceProviderSPI) _epService;
    
            _listener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestBooleanExprFilter()
        {
            var eplCtx = "create context Ctx600a group by TheString like 'A%' as agroup, group by TheString like 'B%' as bgroup, group by TheString like 'C%' as cgroup from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplCtx);
            var eplSum = "context Ctx600a select context.label as c0, count(*) as c1 from SupportBean";
            var stmt = _epService.EPAdministrator.CreateEPL(eplSum);
            stmt.Events += _listener.Update;

            SendAssertBooleanExprFilter("B1", "bgroup", 1);
            SendAssertBooleanExprFilter("A1", "agroup", 1);
            SendAssertBooleanExprFilter("B171771", "bgroup", 2);
            SendAssertBooleanExprFilter("A  x", "agroup", 2);
        }
    
        [Test]
        public void TestContextPartitionSelection()
        {
            var fields = "c0,c1,c2,c3".Split(',');
            _epService.EPAdministrator.CreateEPL("create context MyCtx as group by IntPrimitive < -5 as grp1, group by IntPrimitive between -5 and +5 as grp2, group by IntPrimitive > 5 as grp3 from SupportBean");
            var stmt = _epService.EPAdministrator.CreateEPL("context MyCtx select context.id as c0, context.label as c1, TheString as c2, Sum(IntPrimitive) as c3 from SupportBean.win:keepall() group by TheString");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", -5));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", -100));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", -8));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 60));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, new Object[][]{new Object[] {0, "grp1", "E3", -108}, new Object[] {1, "grp2", "E1", 3}, new Object[] {1, "grp2", "E2", -5}, new Object[] {2, "grp3", "E1", 60}});
    
            // test iterator targeted by context partition id
            var selectorById = new SupportSelectorById(Collections.SingletonList(1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selectorById), stmt.GetSafeEnumerator(selectorById), fields, new Object[][]{new Object[] {1, "grp2", "E1", 3}, new Object[] {1, "grp2", "E2", -5}});
    
            // test iterator targeted for a given category
            var selector = new SupportSelectorCategory(new HashSet<String>{"grp1", "grp3"});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, new Object[][]{new Object[] {0, "grp1", "E3", -108}, new Object[] {2, "grp3", "E1", 60}});
    
            // test iterator targeted for a given filtered category
            var filtered = new MySelectorFilteredCategory("grp1");
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, new Object[][]{new Object[] {0, "grp1", "E3", -108}});
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorCategory((ICollection<String>)null)).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorCategory(Collections.GetEmptySet<String>())).MoveNext());
    
            // test always-false filter - compare context partition info
            filtered = new MySelectorFilteredCategory(null);
            Assert.IsFalse(stmt.GetEnumerator(filtered).MoveNext());
            EPAssertionUtil.AssertEqualsAnyOrder(new Object[]{"grp1", "grp2", "grp3"}, filtered.Categories);
    
            try
            {
                stmt.GetEnumerator(new ProxyContextPartitionSelectorSegmented
                {
                    ProcPartitionKeys = () => null
                });
                Assert.Fail();
            }
            catch (InvalidContextPartitionSelector ex)
            {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorCategory] interfaces but received com."), "message: " + ex.Message);
            }
        }
    
        [Test]
        public void TestInvalid()
        {
            String epl;
    
            // invalid filter spec
            epl = "create context ACtx group TheString is not null as cat1 from SupportBean(dummy = 1)";
            TryInvalid(epl, "Error starting statement: Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");
    
            // not a bool expression
            epl = "create context ACtx group IntPrimitive as grp1 from SupportBean";
            TryInvalid(epl, "Error starting statement: Filter expression not returning a boolean value: 'IntPrimitive' [");
    
            // validate statement not applicable filters
            _epService.EPAdministrator.CreateEPL("create context ACtx group IntPrimitive < 10 as cat1 from SupportBean");
            epl = "context ACtx select * from SupportBean_S0";
            TryInvalid(epl, "Error starting statement: Category context 'ACtx' requires that any of the events types that are listed in the category context also appear in any of the filter expressions of the statement [");
        }
    
        private void TryInvalid(String epl, String expected)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                if (!ex.Message.StartsWith(expected))
                {
                    throw new Exception("Expected/Received:\n" + expected + "\n" + ex.Message + "\n");
                }
                Assert.IsTrue(expected.Trim().Length != 0);
            }
        }
    
        [Test]
        public void TestCategory()
        {
            var filterSPI = (FilterServiceSPI) _spi.FilterService;
            var ctx = "CategorizedContext";
            _epService.EPAdministrator.CreateEPL("@Name('context') create context " + ctx + " " +
                    "group IntPrimitive < 10 as cat1, " +
                    "group IntPrimitive between 10 and 20 as cat2, " +
                    "group IntPrimitive > 20 as cat3 " +
                    "from SupportBean");
    
            var fields = "c0,c1,c2".Split(',');
            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context CategorizedContext " +
                    "select context.name as c0, context.label as c1, Sum(IntPrimitive) as c2 from SupportBean");
            statement.Events += _listener.Update;
            Assert.AreEqual(3, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 3, 0, 0, 0);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "cat1", 5});
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, new Object[][]{new Object[] {ctx, "cat1", 5}, new Object[] {ctx, "cat2", null}, new Object[] {ctx, "cat3", null}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 4));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "cat1", 9});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "cat2", 11});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 25));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "cat3", 25});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 25));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "cat3", 50});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "cat1", 12});
    
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), statement.GetSafeEnumerator(), fields, new Object[][]{new Object[] {ctx, "cat1", 12}, new Object[] {ctx, "cat2", 11}, new Object[] {ctx, "cat3", 50}});
    
            statement.Stop();
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
    
            Assert.AreEqual(1, _spi.ContextManagementService.ContextCount);
            _epService.EPAdministrator.GetStatement("context").Dispose();
            Assert.AreEqual(1, _spi.ContextManagementService.ContextCount);
    
            statement.Dispose();
            Assert.AreEqual(0, _spi.ContextManagementService.ContextCount);
        }
    
        [Test]
        public void TestSingleCategorySODAPrior()
        {
            var ctx = "CategorizedContext";
            var eplCtx = "@Name('context') create context " + ctx + " as " +
                    "group IntPrimitive<10 as cat1 " +
                    "from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplCtx);
    
            var eplStmt = "context CategorizedContext select context.name as c0, context.label as c1, prior(1,IntPrimitive) as c2 from SupportBean";
            var statementOne = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplStmt);
    
            RunAssertion(ctx, statementOne);
    
            // test SODA
            var modelContext = _epService.EPAdministrator.CompileEPL(eplCtx);
            Assert.AreEqual(eplCtx, modelContext.ToEPL());
            var stmt = _epService.EPAdministrator.Create(modelContext);
            Assert.AreEqual(eplCtx, stmt.Text);
    
            var modelStmt = _epService.EPAdministrator.CompileEPL(eplStmt);
            Assert.AreEqual(eplStmt, modelStmt.ToEPL());
            var statementTwo = (EPStatementSPI) _epService.EPAdministrator.Create(modelStmt);
            Assert.AreEqual(eplStmt, statementTwo.Text);
    
            RunAssertion(ctx, statementTwo);
        }
    
        private void RunAssertion(String ctx, EPStatementSPI statement)
        {
            statement.Events += _listener.Update;
    
            var fields = "c0,c1,c2".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "cat1", null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 4));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "cat1", 5});
    
            _epService.EPAdministrator.GetStatement("context").Dispose();
            Assert.AreEqual(1, _spi.ContextManagementService.ContextCount);
    
            _epService.EPAdministrator.DestroyAllStatements();
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
            Assert.AreEqual(0, _spi.ContextManagementService.ContextCount);
        }

        private void SendAssertBooleanExprFilter(String theString, String groupExpected, long countExpected)
        {
            String[] fields = "c0,c1".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean(theString, 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { groupExpected, countExpected });
        }

        private class MySelectorFilteredCategory : ContextPartitionSelectorFiltered
        {
            private readonly String _matchCategory;
    
            private readonly IList<Object> _categories = new List<Object>();
            private readonly LinkedHashSet<int> _cpids = new LinkedHashSet<int>();

            internal MySelectorFilteredCategory(String matchCategory)
            {
                _matchCategory = matchCategory;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
            {
                var id = (ContextPartitionIdentifierCategory) contextPartitionIdentifier;
                if (_matchCategory == null && _cpids.Contains(id.ContextPartitionId.Value))
                {
                    throw new Exception("Already exists context id: " + id.ContextPartitionId);
                }
                _cpids.Add(id.ContextPartitionId.Value);
                _categories.Add(id.Label);
                return _matchCategory != null && _matchCategory.Equals(id.Label);
            }

            public object[] Categories
            {
                get { return _categories.ToArray(); }
            }
        }
    }
}
