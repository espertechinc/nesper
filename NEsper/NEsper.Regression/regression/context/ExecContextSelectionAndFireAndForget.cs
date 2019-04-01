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
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.context;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextSelectionAndFireAndForget : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInvalid(epService);
            RunAssertionContextNamedWindowQuery(epService);
            RunAssertionNestedContextNamedWindowQuery(epService);
            RunAssertionIterateStatement(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
    
            epService.EPAdministrator.CreateEPL("create context SegmentedSB as partition by TheString from SupportBean");
            epService.EPAdministrator.CreateEPL("create context SegmentedS0 as partition by p00 from SupportBean_S0");
            epService.EPAdministrator.CreateEPL("context SegmentedSB create window WinSB#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("context SegmentedS0 create window WinS0#keepall as SupportBean_S0");
            epService.EPAdministrator.CreateEPL("create window WinS1#keepall as SupportBean_S1");
    
            // when a context is declared, it must be the same context that applies to all named windows
            TryInvalidRuntimeQuery(epService, "context SegmentedSB select * from WinSB, WinS0",
                    "Error executing statement: Joins in runtime queries for context partitions are not supported [context SegmentedSB select * from WinSB, WinS0]");
    
            TryInvalidRuntimeQuery(epService, null, "select * from WinSB, WinS1",
                    "No context partition selectors provided");
    
            TryInvalidRuntimeQuery(epService, new ContextPartitionSelector[1], "select * from WinSB, WinS1",
                    "Error executing statement: Number of context partition selectors does not match the number of named windows in the from-clause [select * from WinSB, WinS1]");
    
            // test join
            epService.EPAdministrator.CreateEPL("create context PartitionedByString partition by TheString from SupportBean");
            epService.EPAdministrator.CreateEPL("context PartitionedByString create window MyWindowOne#keepall as SupportBean");
    
            epService.EPAdministrator.CreateEPL("create context PartitionedByP00 partition by p00 from SupportBean_S0");
            epService.EPAdministrator.CreateEPL("context PartitionedByP00 create window MyWindowTwo#keepall as SupportBean_S0");
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 11));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "G2"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "G1"));
    
            try {
                RunQueryAll(epService, "select mw1.IntPrimitive as c1, mw2.id as c2 from MyWindowOne mw1, MyWindowTwo mw2 where mw1.TheString = mw2.p00", "c1,c2",
                        new[] {new object[] {10, 2}, new object[] {11, 1}}, 2);
            } catch (EPStatementException ex) {
                Assert.AreEqual(ex.Message, "Error executing statement: Joins against named windows that are under context are not supported [select mw1.IntPrimitive as c1, mw2.id as c2 from MyWindowOne mw1, MyWindowTwo mw2 where mw1.TheString = mw2.p00]");
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionContextNamedWindowQuery(EPServiceProvider epService) {
    
            epService.EPAdministrator.CreateEPL("create context PartitionedByString partition by TheString from SupportBean");
            epService.EPAdministrator.CreateEPL("context PartitionedByString create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
    
            // test no context
            RunQueryAll(epService, "select sum(IntPrimitive) as c1 from MyWindow", "c1", new[] {new object[] {51}}, 1);
            RunQueryAll(epService, "select sum(IntPrimitive) as c1 from MyWindow where IntPrimitive > 15", "c1", new[] {new object[] {41}}, 1);
            RunQuery(epService, "select sum(IntPrimitive) as c1 from MyWindow", "c1", new[] {new object[] {41}}, new ContextPartitionSelector[] {new SupportSelectorPartitioned(Collections.SingletonSet(new object[] {"E2"}))});
            RunQuery(epService, "select sum(IntPrimitive) as c1 from MyWindow", "c1", new[] {new object[] {41}}, new ContextPartitionSelector[] {new SupportSelectorById(Collections.SingletonSet(1))});
    
            // test with context props
            RunQueryAll(epService, "context PartitionedByString select context.key1 as c0, IntPrimitive as c1 from MyWindow",
                    "c0,c1", new[] {new object[] {"E1", 10}, new object[] {"E2", 20}, new object[] {"E2", 21}}, 1);
            RunQueryAll(epService, "context PartitionedByString select context.key1 as c0, IntPrimitive as c1 from MyWindow where IntPrimitive > 15",
                    "c0,c1", new[] {new object[] {"E2", 20}, new object[] {"E2", 21}}, 1);
    
            // test targeted context partition
            RunQuery(epService, "context PartitionedByString select context.key1 as c0, IntPrimitive as c1 from MyWindow where IntPrimitive > 15",
                    "c0,c1", new[] {new object[] {"E2", 20}, new object[] {"E2", 21}}, new[] {new SupportSelectorPartitioned(Collections.SingletonList(new object[] {"E2"}))});
    
            try
            {
                epService.EPRuntime.ExecuteQuery(
                    "context PartitionedByString select * from MyWindow", new ContextPartitionSelector[]
                    {
                        new ProxyContextPartitionSelectorCategory()
                        {
                            ProcLabels = () => null
                        }
                    });

                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.IsTrue(ex.Message.StartsWith("Error executing statement: Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorSegmented] interfaces but received com"),
                    "message: " + ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNestedContextNamedWindowQuery(EPServiceProvider epService) {
    
            epService.EPAdministrator.CreateEPL("create context NestedContext " +
                    "context ACtx initiated by SupportBean_S0 as s0 terminated by SupportBean_S1(id=s0.id), " +
                    "context BCtx group by IntPrimitive < 0 as grp1, group by IntPrimitive = 0 as grp2, group by IntPrimitive > 0 as grp3 from SupportBean");
            epService.EPAdministrator.CreateEPL("context NestedContext create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "S0_1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "S0_2"));
            epService.EPRuntime.SendEvent(new SupportBean("E2", -1));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 5));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
    
            RunQueryAll(epService, "select TheString as c1, sum(IntPrimitive) as c2 from MyWindow group by TheString", "c1,c2", new[] {new object[] {"E1", 5}, new object[] {"E2", -2}, new object[] {"E3", 10}}, 1);
            RunQuery(epService, "select TheString as c1, sum(IntPrimitive) as c2 from MyWindow group by TheString", "c1,c2", new[] {new object[] {"E1", 3}, new object[] {"E3", 5}},
                    new ContextPartitionSelector[]{new SupportSelectorById(Collections.SingletonSet(2))});
    
            RunQuery(epService, "context NestedContext select context.ACtx.s0.p00 as c1, context.BCtx.label as c2, TheString as c3, sum(IntPrimitive) as c4 from MyWindow group by TheString", "c1,c2,c3,c4", new[] {new object[] {"S0_1", "grp3", "E1", 3}, new object[] {"S0_1", "grp3", "E3", 5}},
                    new ContextPartitionSelector[]{new SupportSelectorById(Collections.SingletonSet(2))});
    
            // extract path
            if (GetSpi(epService).IsSupportsExtract) {
                GetSpi(epService).ExtractPaths("NestedContext", new ContextPartitionSelectorAll());
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionIterateStatement(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create context PartitionedByString partition by TheString from SupportBean");
            string[] fields = "c0,c1".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Name('StmtOne') context PartitionedByString select context.key1 as c0, sum(IntPrimitive) as c1 from SupportBean#length(5)");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 21));
    
            var expectedAll = new[] {new object[] {"E1", 10}, new object[] {"E2", 41}};
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, expectedAll);
    
            // test iterator ALL
            ContextPartitionSelector selector = ContextPartitionSelectorAll.INSTANCE;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, expectedAll);
    
            // test iterator by context partition id
            selector = new SupportSelectorById(new HashSet<int>(Collections.List(0, 1, 2)));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, expectedAll);
    
            selector = new SupportSelectorById(new HashSet<int>(Collections.List(1)));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, new[] {new object[] {"E2", 41}});
    
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorById(Collections.GetEmptySet<int>())).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorById(null)).MoveNext());
    
            try {
                stmt.GetEnumerator(null);
                Assert.Fail();
            } catch (ArgumentException ex) {
                Assert.AreEqual(ex.Message, "No selector provided");
            }
    
            try {
                stmt.GetSafeEnumerator(null);
                Assert.Fail();
            } catch (ArgumentException ex) {
                Assert.AreEqual(ex.Message, "No selector provided");
            }
    
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL("select * from System.Object");
            try {
                stmtTwo.GetEnumerator(null);
                Assert.Fail();
            } catch (UnsupportedOperationException ex) {
                Assert.AreEqual(ex.Message, "Enumerator with context selector is only supported for statements under context");
            }
    
            try {
                stmtTwo.GetSafeEnumerator(null);
                Assert.Fail();
            } catch (UnsupportedOperationException ex) {
                Assert.AreEqual(ex.Message, "Enumerator with context selector is only supported for statements under context");
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunQueryAll(EPServiceProvider epService, string epl, string fields, object[][] expected, int numStreams) {
            var selectors = new ContextPartitionSelector[numStreams];
            for (int i = 0; i < numStreams; i++) {
                selectors[i] = ContextPartitionSelectorAll.INSTANCE;
            }
    
            RunQuery(epService, epl, fields, expected, selectors);
    
            // run same query without selector
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery(epl);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields.Split(','), expected);
        }
    
        private void RunQuery(EPServiceProvider epService, string epl, string fields, object[][] expected, ContextPartitionSelector[] selectors) {
            // try FAF without prepare
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery(epl, selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields.Split(','), expected);
    
            // test unparameterized prepare and execute
            EPOnDemandPreparedQuery preparedQuery = epService.EPRuntime.PrepareQuery(epl);
            EPOnDemandQueryResult resultPrepared = preparedQuery.Execute(selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(resultPrepared.Array, fields.Split(','), expected);
    
            // test unparameterized prepare and execute
            EPOnDemandPreparedQueryParameterized preparedParameterizedQuery = epService.EPRuntime.PrepareQueryWithParameters(epl);
            EPOnDemandQueryResult resultPreparedParameterized = epService.EPRuntime.ExecuteQuery(preparedParameterizedQuery, selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(resultPreparedParameterized.Array, fields.Split(','), expected);
    
            // test SODA prepare and execute
            EPStatementObjectModel modelForPrepare = epService.EPAdministrator.CompileEPL(epl);
            EPOnDemandPreparedQuery preparedQueryModel = epService.EPRuntime.PrepareQuery(modelForPrepare);
            EPOnDemandQueryResult resultPreparedModel = preparedQueryModel.Execute(selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(resultPreparedModel.Array, fields.Split(','), expected);
    
            // test model query
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            result = epService.EPRuntime.ExecuteQuery(model, selectors);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(result.Array, fields.Split(','), expected);
        }
    
        private void TryInvalidRuntimeQuery(EPServiceProvider epService, ContextPartitionSelector[] selectors, string epl, string expected) {
            try {
                epService.EPRuntime.ExecuteQuery(epl, selectors);
                Assert.Fail();
            } catch (Exception ex) {
                Assert.AreEqual(ex.Message, expected);
            }
        }
    
        private void TryInvalidRuntimeQuery(EPServiceProvider epService, string epl, string expected) {
            try {
                epService.EPRuntime.ExecuteQuery(epl);
                Assert.Fail();
            } catch (Exception ex) {
                Assert.AreEqual(expected, ex.Message);
            }
        }
    
        private static EPContextPartitionAdminSPI GetSpi(EPServiceProvider epService) {
            return (EPContextPartitionAdminSPI) epService.EPAdministrator.ContextPartitionAdmin;
        }
    }
} // end of namespace
