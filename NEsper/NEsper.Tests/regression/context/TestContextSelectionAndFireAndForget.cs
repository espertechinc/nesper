///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextSelectionAndFireAndForget
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType<SupportBean_S1>();
            configuration.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestInvalid() {
    
            _epService.EPAdministrator.CreateEPL("create context SegmentedSB as partition by TheString from SupportBean");
            _epService.EPAdministrator.CreateEPL("create context SegmentedS0 as partition by p00 from SupportBean_S0");
            _epService.EPAdministrator.CreateEPL("context SegmentedSB create window WinSB.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("context SegmentedS0 create window WinS0.win:keepall() as SupportBean_S0");
            _epService.EPAdministrator.CreateEPL("create window WinS1.win:keepall() as SupportBean_S1");
    
            // when a context is declared, it must be the same context that applies to all named windows
            TryInvalidRuntimeQuery("context SegmentedSB select * from WinSB, WinS0",
                    "Error executing statement: Joins in runtime queries for context partitions are not supported [context SegmentedSB select * from WinSB, WinS0]");
            
            TryInvalidRuntimeQuery(null, "select * from WinSB, WinS1",
                    "No context partition selectors provided");
    
            TryInvalidRuntimeQuery(new ContextPartitionSelector[1], "select * from WinSB, WinS1",
                    "Error executing statement: Number of context partition selectors does not match the number of named windows in the from-clause [select * from WinSB, WinS1]");
    
            // test join
            _epService.EPAdministrator.CreateEPL("create context PartitionedByString partition by TheString from SupportBean");
            _epService.EPAdministrator.CreateEPL("context PartitionedByString create window MyWindowOne.win:keepall() as SupportBean");
    
            _epService.EPAdministrator.CreateEPL("create context PartitionedByP00 partition by p00 from SupportBean_S0");
            _epService.EPAdministrator.CreateEPL("context PartitionedByP00 create window MyWindowTwo.win:keepall() as SupportBean_S0");
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 11));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "G2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "G1"));
    
            try {
                RunQueryAll("select mw1.IntPrimitive as c1, mw2.id as c2 from MyWindowOne mw1, MyWindowTwo mw2 where mw1.TheString = mw2.p00", "c1,c2",
                    new Object[][]{new Object[] {10, 2}, new Object[] {11, 1}}, 2);
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(ex.Message, "Error executing statement: Joins against named windows that are under context are not supported [select mw1.IntPrimitive as c1, mw2.id as c2 from MyWindowOne mw1, MyWindowTwo mw2 where mw1.TheString = mw2.p00]");
            }
        }
    
        [Test]
        public void TestJoin() {
        }
    
        [Test]
        public void TestContextNamedWindowQuery()
        {
    
            _epService.EPAdministrator.CreateEPL("create context PartitionedByString partition by TheString from SupportBean");
            _epService.EPAdministrator.CreateEPL("context PartitionedByString create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
    
            // test no context
            RunQueryAll("select Sum(IntPrimitive) as c1 from MyWindow", "c1", new Object[][]{new Object[] {51}}, 1);
            RunQueryAll("select Sum(IntPrimitive) as c1 from MyWindow where IntPrimitive > 15", "c1", new Object[][]{new Object[] {41}}, 1);
            RunQuery("select Sum(IntPrimitive) as c1 from MyWindow", "c1", new Object[][]{new Object[] {41}}, new ContextPartitionSelector[] {new SupportSelectorPartitioned(Collections.SingletonList(new Object[]{"E2"}))});
            RunQuery("select Sum(IntPrimitive) as c1 from MyWindow", "c1", new Object[][]{new Object[] {41}}, new ContextPartitionSelector[] {new SupportSelectorById(Collections.SingletonList<int>(1))});
    
            // test with context props
            RunQueryAll("context PartitionedByString select context.key1 as c0, IntPrimitive as c1 from MyWindow",
                    "c0,c1", new Object[][]{new Object[] {"E1", 10}, new Object[] {"E2", 20}, new Object[] {"E2", 21}}, 1);
            RunQueryAll("context PartitionedByString select context.key1 as c0, IntPrimitive as c1 from MyWindow where IntPrimitive > 15",
                    "c0,c1", new Object[][]{new Object[] {"E2", 20}, new Object[] {"E2", 21}}, 1);
    
            // test targeted context partition
            RunQuery("context PartitionedByString select context.key1 as c0, IntPrimitive as c1 from MyWindow where IntPrimitive > 15",
                    "c0,c1", new Object[][]{new Object[] {"E2", 20}, new Object[] {"E2", 21}}, new SupportSelectorPartitioned[]{new SupportSelectorPartitioned(Collections.SingletonList(new Object[]{"E2"}))});
            
            try
            {
                _epService.EPRuntime.ExecuteQuery("context PartitionedByString select * from MyWindow", new ContextPartitionSelector[] {
                    new ProxyContextPartitionSelectorCategory
                    {
                        ProcLabels = () => null,
                    }
                });
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.IsTrue(ex.Message.StartsWith("Error executing statement: Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorSegmented] interfaces but received com"), "message: " + ex.Message);
            }
        }
    
        [Test]
        public void TestNestedContextNamedWindowQuery()
        {
            _epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context ACtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(id=s0.id), " +
                    "context BCtx group by IntPrimitive < 0 as grp1, group by IntPrimitive = 0 as grp2, group by IntPrimitive > 0 as grp3 from SupportBean");
            _epService.EPAdministrator.CreateEPL("context NestedContext create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
    
            RunQueryAll("select TheString as c1, Sum(IntPrimitive) as c2 from MyWindow group by TheString", "c1,c2", new Object[][]{new Object[] {"E1", 5}, new Object[] {"E2", -2}, new Object[] {"E3", 10}}, 1);
            RunQuery("select TheString as c1, Sum(IntPrimitive) as c2 from MyWindow group by TheString", "c1,c2", new Object[][]{new Object[] {"E1", 3}, new Object[] {"E3", 5}},
                    new ContextPartitionSelector[] {new SupportSelectorById(Collections.SingletonList(2))});
    
            RunQuery("context NestedContext select context.ACtx.s0.p00 as c1, context.BCtx.label as c2, TheString as c3, Sum(IntPrimitive) as c4 from MyWindow group by TheString", "c1,c2,c3,c4", new Object[][]{new Object[] {"S0_1", "grp3", "E1", 3}, new Object[] {"S0_1", "grp3", "E3", 5}},
                    new ContextPartitionSelector[] {new SupportSelectorById(Collections.SingletonList(2))});

            // extract path
            if (GetSpi(_epService).IsSupportsExtract)
            {
                GetSpi(_epService).ExtractPaths("NestedContext", new ContextPartitionSelectorAll());
            }
        }
    
        [Test]
        public void TestIterateStatement()
        {
            _epService.EPAdministrator.CreateEPL("create context PartitionedByString partition by TheString from SupportBean");
            String[] fields = "c0,c1".Split(',');
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("@Name('StmtOne') context PartitionedByString select context.key1 as c0, Sum(IntPrimitive) as c1 from SupportBean.win:length(5)");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
    
            Object[][] expectedAll = new Object[][] { new Object[] {"E1", 10},new Object[] {"E2", 41}};
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, expectedAll);
    
            // test iterator ALL
            ContextPartitionSelector selector = ContextPartitionSelectorAll.INSTANCE;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, expectedAll);
    
            // test iterator by context partition id
            selector = new SupportSelectorById(new HashSet<int>(Collections.List(0, 1, 2)));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, expectedAll);
    
            selector = new SupportSelectorById(new HashSet<int>(Collections.List(1)));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, new Object[][] { new Object[] {"E2", 41 } });
    
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorById(Collections.GetEmptySet<int>())).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorById(null)).MoveNext());
            
            try {
                stmt.GetEnumerator(null);
                Assert.Fail();
            }
            catch (ArgumentException ex) {
                Assert.AreEqual(ex.Message, "No selector provided");
            }
    
            try {
                stmt.GetSafeEnumerator(null);
                Assert.Fail();
            }
            catch (ArgumentException ex) {
                Assert.AreEqual(ex.Message, "No selector provided");
            }
    
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("select * from System.Object");
            try {
                stmtTwo.GetEnumerator(null);
                Assert.Fail();
            }
            catch (UnsupportedOperationException ex) {
                Assert.AreEqual(ex.Message, "Enumerator with context selector is only supported for statements under context");
            }
    
            try {
                stmtTwo.GetSafeEnumerator(null);
                Assert.Fail();
            }
            catch (UnsupportedOperationException ex) {
                Assert.AreEqual(ex.Message, "Enumerator with context selector is only supported for statements under context");
            }
        }
    
        private void RunQueryAll(String epl, String fields, Object[][] expected, int numStreams)
        {
            ContextPartitionSelector[] selectors = new ContextPartitionSelector[numStreams];
            for (int i = 0; i < numStreams; i++) {
                selectors[i] = ContextPartitionSelectorAll.INSTANCE;
            }
    
            RunQuery(epl, fields, expected, selectors);
    
            // run same query without selector
            EPOnDemandQueryResult result = _epService.EPRuntime.ExecuteQuery(epl);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields.Split(','), expected);
        }
    
        private void RunQuery(String epl, String fields, Object[][] expected, ContextPartitionSelector[] selectors)
        {
            // try FAF without prepare
            EPOnDemandQueryResult result = _epService.EPRuntime.ExecuteQuery(epl, selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields.Split(','), expected);
    
            // test unparameterized prepare and execute
            EPOnDemandPreparedQuery preparedQuery = _epService.EPRuntime.PrepareQuery(epl);
            EPOnDemandQueryResult resultPrepared = preparedQuery.Execute(selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(resultPrepared.Array, fields.Split(','), expected);
    
            // test unparameterized prepare and execute
            EPOnDemandPreparedQueryParameterized preparedParameterizedQuery = _epService.EPRuntime.PrepareQueryWithParameters(epl);
            EPOnDemandQueryResult resultPreparedParameterized = _epService.EPRuntime.ExecuteQuery(preparedParameterizedQuery, selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(resultPreparedParameterized.Array, fields.Split(','), expected);
    
            // test SODA prepare and execute
            EPStatementObjectModel modelForPrepare = _epService.EPAdministrator.CompileEPL(epl);
            EPOnDemandPreparedQuery preparedQueryModel = _epService.EPRuntime.PrepareQuery(modelForPrepare);
            EPOnDemandQueryResult resultPreparedModel = preparedQueryModel.Execute(selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(resultPreparedModel.Array, fields.Split(','), expected);
    
            // test model query
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
            result = _epService.EPRuntime.ExecuteQuery(model, selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields.Split(','), expected);
        }
    
        private void TryInvalidRuntimeQuery(ContextPartitionSelector[] selectors, String epl, String expected)
        {
            try {
                _epService.EPRuntime.ExecuteQuery(epl, selectors);
                Assert.Fail();
            }
            catch (Exception ex) {
                Assert.AreEqual(ex.Message, expected);
            }
        }
    
        private void TryInvalidRuntimeQuery(String epl, String expected)
        {
            try {
                _epService.EPRuntime.ExecuteQuery(epl);
                Assert.Fail();
            }
            catch (Exception ex) {
                Assert.AreEqual(expected, ex.Message);
            }
        }

        private static EPContextPartitionAdminSPI GetSpi(EPServiceProvider epService)
        {
            return ((EPContextPartitionAdminSPI)epService.EPAdministrator.ContextPartitionAdmin);
        }
    }
}
