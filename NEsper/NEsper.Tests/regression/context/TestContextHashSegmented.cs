///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextHashSegmented
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
        public void TestScoringUseCase()
        {
            RunAssertionScoringUseCase(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionScoringUseCase(EventRepresentationEnum.MAP);
            RunAssertionScoringUseCase(EventRepresentationEnum.DEFAULT);
        }
    
        private void RunAssertionScoringUseCase(EventRepresentationEnum eventRepresentationEnum)
        {
            var fields = "userId,keyword,sumScore".Split(',');
            var epl =
                    eventRepresentationEnum.GetAnnotationText() + " create schema ScoreCycle (userId string, keyword string, productId string, score long);\n" +
                    eventRepresentationEnum.GetAnnotationText() + " create schema UserKeywordTotalStream (userId string, keyword string, sumScore long);\n" +
                    "\n" +
                    eventRepresentationEnum.GetAnnotationText() + " create context HashByUserCtx as " +
                            "coalesce by Consistent_hash_crc32(userId) from ScoreCycle, " +
                            "consistent_hash_crc32(userId) from UserKeywordTotalStream " +
                            "granularity 1000000;\n" +
                    "\n" +
                    "context HashByUserCtx create window ScoreCycleWindow.std:unique(productId, keyword) as ScoreCycle;\n" +
                    "\n" +
                    "context HashByUserCtx insert into ScoreCycleWindow select * from ScoreCycle;\n" +
                    "\n" +
                    "@Name('outOne') context HashByUserCtx insert into UserKeywordTotalStream \n" +
                    "select userId, keyword, Sum(score) as sumScore from ScoreCycleWindow group by keyword;\n" +
                    "\n" +
                    "@Name('outTwo') context HashByUserCtx on UserKeywordTotalStream(sumScore > 10000) delete from ScoreCycleWindow;\n";
    
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            _epService.EPAdministrator.GetStatement("outOne").Events += _listener.Update;
    
            MakeSendScoreEvent("ScoreCycle", eventRepresentationEnum, "Pete", "K1", "P1", 100);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"Pete", "K1", 100L});
    
            MakeSendScoreEvent("ScoreCycle", eventRepresentationEnum, "Pete", "K1", "P2", 15);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"Pete", "K1", 115L});
    
            MakeSendScoreEvent("ScoreCycle", eventRepresentationEnum, "Joe", "K1", "P2", 30);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"Joe", "K1", 30L});
    
            MakeSendScoreEvent("ScoreCycle", eventRepresentationEnum, "Joe", "K2", "P1", 40);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"Joe", "K2", 40L});
    
            MakeSendScoreEvent("ScoreCycle", eventRepresentationEnum, "Joe", "K1", "P1", 20);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"Joe", "K1", 50L});
    
            _epService.Initialize();
        }
    
        [Test]
        public void TestContextPartitionSelection()
        {
            var fields = "c0,c1,c2".Split(',');
            _epService.EPAdministrator.CreateEPL("create context MyCtx as coalesce Consistent_hash_crc32(TheString) from SupportBean granularity 16 preallocate");
            var stmt = _epService.EPAdministrator.CreateEPL("context MyCtx select context.id as c0, TheString as c1, Sum(IntPrimitive) as c2 from SupportBean.win:keepall() group by TheString");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, new Object[][]{new Object[] {5, "E1", 1}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 100));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 101));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, new Object[][]{new Object[] {5, "E1", 6}, new Object[] {15, "E2", 10}, new Object[] {9, "E3", 201}});
    
            // test iterator targeted hash
            var selector = new SupportSelectorByHashCode(Collections.SingletonList(15));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, new Object[][]{new Object[] {15, "E2", 10}});
            selector = new SupportSelectorByHashCode(new HashSet<int>{ 1, 9, 5 });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, new Object[][]{new Object[] {5, "E1", 6}, new Object[] {9, "E3", 201}});
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorByHashCode(Collections.SingletonList(99))).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorByHashCode(Collections.GetEmptySet<int>())).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorByHashCode(null)).MoveNext());
    
            // test iterator filtered
            var filtered = new MySelectorFilteredHash(Collections.SingletonList(15));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, new Object[][]{new Object[] {15, "E2", 10}});
            filtered = new MySelectorFilteredHash(new HashSet<int>{1, 9, 5});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, new Object[][]{new Object[] {5, "E1", 6}, new Object[] {9, "E3", 201}});
    
            // test always-false filter - compare context partition info
            filtered = new MySelectorFilteredHash(Collections.GetEmptySet<int>());
            Assert.IsFalse(stmt.GetEnumerator(filtered).MoveNext());
            Assert.AreEqual(16, filtered.Contexts.Count);
    
            try {
                stmt.GetEnumerator(new ProxyContextPartitionSelectorSegmented
                {
                    ProcPartitionKeys = () => null
                });
                Assert.Fail();
            }
            catch (InvalidContextPartitionSelector ex) {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorHash] interfaces but received com."), "message: " + ex.Message);
            }
        }
    
        [Test]
        public void TestInvalid()
        {
            String epl;
    
            // invalid filter spec
            epl = "create context ACtx coalesce hash_code(IntPrimitive) from SupportBean(dummy = 1) granularity 10";
            TryInvalid(epl, "Error starting statement: Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");
    
            // invalid hash code function
            epl = "create context ACtx coalesce hash_code_xyz(IntPrimitive) from SupportBean granularity 10";
            TryInvalid(epl, "Error starting statement: For context 'ACtx' expected a hash function that is any of {consistent_hash_crc32, hash_code} or a plug-in single-row function or script but received 'hash_code_xyz' [");
    
            // invalid no-param hash code function
            epl = "create context ACtx coalesce hash_code() from SupportBean granularity 10";
            TryInvalid(epl, "Error starting statement: For context 'ACtx' expected one or more parameters to the hash function, but found no parameter list [");
    
            // validate statement not applicable filters
            _epService.EPAdministrator.CreateEPL("create context ACtx coalesce hash_code(IntPrimitive) from SupportBean granularity 10");
            epl = "context ACtx select * from SupportBean_S0";
            TryInvalid(epl, "Error starting statement: Segmented context 'ACtx' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");

            // invalid attempt to partition a named window's streams
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
            epl = "create context SegmentedByWhat partition by TheString from MyWindow";
            TryInvalid(epl, "Error starting statement: Partition criteria may not include named windows [create context SegmentedByWhat partition by TheString from MyWindow]");
 
        }
    
        private void TryInvalid(String epl, String expected) {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                if (!ex.Message.StartsWith(expected)) {
                    throw new Exception("Expected/Received:\n" + expected + "\n" + ex.Message + "\n");
                }
                Assert.IsTrue(expected.Trim().Length != 0);
            }
        }
    
        [Test]
        public void TestHashSegmentedFilter() {
    
            var ctx = "HashSegmentedContext";
            var eplCtx = "@Name('context') create context " + ctx + " as " +
                    "coalesce " +
                    " Consistent_hash_crc32(TheString) from SupportBean(IntPrimitive > 10) " +
                    "granularity 4 " +
                    "preallocate";
            _epService.EPAdministrator.CreateEPL(eplCtx);

            var eplStmt = "context " + ctx + " " + "select context.name as c0, IntPrimitive as c1 from SupportBean.std:lastevent()";
            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplStmt);
            statement.Events += _listener.Update;
    
            var fields = "c0,c1".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, 12});
            AssertIterator(statement, fields, new Object[][]{new Object[] {ctx, 12}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            AssertIterator(statement, fields, new Object[][]{new Object[] {ctx, 12}});
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 15));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, 15});
        }
    
        [Test]
        public void TestHashSegmentedManyArg() {
            TryHash("consistent_hash_crc32(TheString, IntPrimitive)");
            TryHash("hash_code(TheString, IntPrimitive)");
        }
    
        private void TryHash(String hashFunc) {
            var eplCtxCRC32 = "@Name('context') create context Ctx1 as coalesce " +
                    hashFunc + " from SupportBean " +
                    "granularity 1000000";
            _epService.EPAdministrator.CreateEPL(eplCtxCRC32);
    
            var fields = "c1,c2,c3,c4,c5".Split(',');
            var eplStmt = "context Ctx1 select IntPrimitive as c1, " +
                    "sum(LongPrimitive) as c2, Prev(1, LongPrimitive) as c3, Prior(1, LongPrimitive) as c4," +
                    "(select p00 from SupportBean_S0.win:length(2)) as c5 " +
                    "from SupportBean.win:length(3)";
            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplStmt);
            statement.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(MakeBean("E1", 100, 20L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{100, 20L, null, null, null});

            _epService.EPRuntime.SendEvent(MakeBean("E1", 100, 21L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{100, 41L, 20L, 20L, null});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1000, "S0"));
            _epService.EPRuntime.SendEvent(MakeBean("E1", 100, 22L));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{100, 63L, 21L, 21L, "S0"});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        [Test]
        public void TestHashSegmentedMulti() {
    
            var ctx = "HashSegmentedContext";
            var eplCtx = "@Name('context') create context " + ctx + " as " +
                    "coalesce " +
                    " Consistent_hash_crc32(TheString) from SupportBean, " +
                    " Consistent_hash_crc32(p00) from SupportBean_S0 " +
                    "granularity 4 " +
                    "preallocate";
            _epService.EPAdministrator.CreateEPL(eplCtx);
            var codeFunc = new SupportHashCodeFuncGranularCRC32(4);
    
            var eplStmt = "context " + ctx + " " +
                    "select context.name as c0, IntPrimitive as c1, id as c2 from SupportBean.win:keepall() as t1, SupportBean_S0.win:keepall() as t2 where t1.TheString = t2.p00";
            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplStmt);
            statement.Events += _listener.Update;
    
            var fields = "c0,c1,c2".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E2"));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 11));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E4"));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "E1"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, 10, 3});
            AssertIterator(statement, fields, new Object[][]{new Object[] {ctx, 10, 3}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "E4"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "E5"));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 12));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, 12, 1});
            AssertIterator(statement, fields, new Object[][]{new Object[] {ctx, 10, 3}, new Object[] {ctx, 12, 1}});
        }
    
        [Test]
        public void TestHashSegmented() {
    
            // Comment-in to see CRC32 code.
            for (var i = 0; i < 10; i++) {
                var key = "E" + i;
                var code = SupportHashCodeFuncGranularCRC32.ComputeCRC32(key) % 4;
                var hashCode = i.GetHashCode() % 4;
                //Console.WriteLine(key + " code " + code + " hashCode " + hashCode);
            }
    
            // test CRC32 Hash
            var filterSPI = (FilterServiceSPI) _spi.FilterService;
            var ctx = "HashSegmentedContext";
            var eplCtx = "@Name('context') create context " + ctx + " as " +
                    "coalesce Consistent_hash_crc32(TheString) from SupportBean " +
                    "granularity 4 " +
                    "preallocate";
            _epService.EPAdministrator.CreateEPL(eplCtx);
    
            var eplStmt = "context " + ctx + " " +
                    "select context.name as c0, TheString as c1, Sum(IntPrimitive) as c2 from SupportBean.win:keepall() group by TheString";
            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplStmt);
            statement.Events += _listener.Update;
            Assert.AreEqual(4, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 4, 0, 0, 0);
    
            RunAssertionHash(ctx, statement, new SupportHashCodeFuncGranularCRC32(4));
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            // test same with SODA
            var modelCtx = _epService.EPAdministrator.CompileEPL(eplCtx);
            Assert.AreEqual(eplCtx, modelCtx.ToEPL());
            var stmtCtx = _epService.EPAdministrator.Create(modelCtx);
            Assert.AreEqual(eplCtx, stmtCtx.Text);
            
            statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplStmt);
            statement.Events += _listener.Update;
            RunAssertionHash(ctx, statement, new SupportHashCodeFuncGranularCRC32(4));
    
            // test with GetHashCode String hash
            _epService.EPAdministrator.CreateEPL("@Name('context') create context " + ctx + " " +
                    "coalesce Hash_code(TheString) from SupportBean " +
                    "granularity 6 " +
                    "preallocate");
    
            statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context " + ctx + " " +
                    "select context.name as c0, TheString as c1, Sum(IntPrimitive) as c2 from SupportBean.win:keepall() group by TheString");
            statement.Events += _listener.Update;
            Assert.AreEqual(6, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 6, 0, 0, 0);
    
            RunAssertionHash(ctx, statement, new HashCodeFuncGranularInternalHash(6));
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            // test no pre-allocate
            _epService.EPAdministrator.CreateEPL("@Name('context') create context " + ctx + " " +
                    "coalesce Hash_code(TheString) from SupportBean " +
                    "granularity 16 ");
    
            statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("context " + ctx + " " +
                    "select context.name as c0, TheString as c1, Sum(IntPrimitive) as c2 from SupportBean.win:keepall() group by TheString");
            statement.Events += _listener.Update;
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
    
            RunAssertionHash(ctx, statement, new HashCodeFuncGranularInternalHash(16));
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
        }
    
        private void RunAssertionHash(String ctx, EPStatementSPI statement, HashCodeFunc codeFunc) {
    
            var fields = "c0,c1,c2".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "E1", 5});
            AssertIterator(statement, fields, new Object[][]{new Object[] {ctx, "E1", 5}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 6));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "E2", 6});
            AssertIterator(statement, fields, new Object[][]{new Object[] {ctx, "E1", 5}, new Object[] {ctx, "E2", 6}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 7));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "E3", 7});
            AssertIterator(statement, fields, new Object[][]{new Object[] {ctx, "E1", 5}, new Object[] {ctx, "E3", 7}, new Object[] {ctx, "E2", 6}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 8));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "E4", 8});
            AssertIterator(statement, fields, new Object[][]{new Object[] {ctx, "E1", 5}, new Object[] {ctx, "E3", 7}, new Object[] {ctx, "E4", 8}, new Object[] {ctx, "E2", 6}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 9));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "E5", 9});
            AssertIterator(statement, fields, new Object[][]{new Object[] {ctx, "E5", 9}, new Object[] {ctx, "E1", 5}, new Object[] {ctx, "E3", 7}, new Object[] {ctx, "E4", 8}, new Object[] {ctx, "E2", 6}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "E1", 15});
            AssertIterator(statement, fields, new Object[][]{new Object[] {ctx, "E5", 9}, new Object[] {ctx, "E1", 15}, new Object[] {ctx, "E3", 7}, new Object[] {ctx, "E4", 8}, new Object[] {ctx, "E2", 6}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 11));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{ctx, "E4", 19});
            AssertIterator(statement, fields, new Object[][]{new Object[] {ctx, "E5", 9}, new Object[] {ctx, "E1", 15}, new Object[] {ctx, "E3", 7}, new Object[] {ctx, "E4", 19}, new Object[] {ctx, "E2", 6}});
    
            statement.Stop();
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
    
            Assert.AreEqual(1, _spi.ContextManagementService.ContextCount);
            _epService.EPAdministrator.GetStatement("context").Dispose();
            Assert.AreEqual(1, _spi.ContextManagementService.ContextCount);
    
            statement.Dispose();
            Assert.AreEqual(0, _spi.ContextManagementService.ContextCount);
        }
    
        private void AssertIterator(EPStatementSPI statement, String[] fields, Object[][] expected) {
            EventBean[] rows = EPAssertionUtil.EnumeratorToArray(statement.GetEnumerator());
            AssertIterator(rows, fields, expected);

            rows = EPAssertionUtil.EnumeratorToArray(statement.GetSafeEnumerator());
            AssertIterator(rows, fields, expected);
        }
    
        private void AssertIterator(EventBean[] events, String[] fields, Object[][] expected) {
            var result = EPAssertionUtil.EventsToObjectArr(events, fields);
            EPAssertionUtil.AssertEqualsAnyOrder(expected, result);
        }
    
        private SupportBean MakeBean(String theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    
        [Test]
        public void TestHashSegmentedBySingleRowFunc()
        {
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("MyHash", GetType().FullName, "MyHashFunc");
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("MySecond", GetType().FullName, "MySecondFunc");
            _epService.EPAdministrator.Configuration.AddImport(GetType().FullName);
    
            var eplCtx = "@Name('context') create context HashSegmentedContext as " +
                    "coalesce MyHash(*) from SupportBean " +
                    "granularity 4 " +
                    "preallocate";
            _epService.EPAdministrator.CreateEPL(eplCtx);
    
            var eplStmt = string.Format("context HashSegmentedContext select context.id as c1, MyHash(*) as c2, MySecond(*, TheString) as c3, {0}.MySecondFunc(*, TheString) as c4 from SupportBean", GetType().Name);
            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(eplStmt);
            statement.Events += _listener.Update;
    
            var fields = "c1,c2,c3, c4".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{3, 3, "E1", "E1"});    // context id matches the number returned by myHashFunc
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{0, 0, "E2", "E2"});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 7));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{3, 7, "E3", "E3"});
        }
    
        public static int MyHashFunc(SupportBean sb) {
            return sb.IntPrimitive;
        }
    
        public static String MySecondFunc(SupportBean sb, String text) {
            return text;
        }
    
        private void MakeSendScoreEvent(String typeName, EventRepresentationEnum eventRepresentationEnum, String userId, String keyword, String productId, long score) {
            IDictionary<String, Object> theEvent = new LinkedHashMap<String, Object>();
            theEvent.Put("userId", userId);
            theEvent.Put("keyword", keyword);
            theEvent.Put("productId", productId);
            theEvent.Put("score", score);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), typeName);
            }
            else {
                _epService.EPRuntime.SendEvent(theEvent, typeName);
            }
        }
    
        public interface HashCodeFunc {
            int CodeFor(String key);
        }
    
        public class HashCodeFuncGranularInternalHash : HashCodeFunc {
            private int granularity;
    
            public HashCodeFuncGranularInternalHash(int granularity) {
                this.granularity = granularity;
            }
    
            public int CodeFor(String key) {
                return key.GetHashCode() % granularity;
            }
        }

        internal class MySelectorFilteredHash : ContextPartitionSelectorFiltered
        {
            private readonly ICollection<int> _match;
    
            private readonly IList<int> _contexts = new List<int>();
            private readonly LinkedHashSet<int> _cpids = new LinkedHashSet<int>();
    
            internal MySelectorFilteredHash(ICollection<int> match)
            {
                _match = match;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier)
            {
                var id = (ContextPartitionIdentifierHash) contextPartitionIdentifier;
                if (_match == null && _cpids.Contains(id.ContextPartitionId.Value))
                {
                    throw new Exception("Already exists context id: " + id.ContextPartitionId);
                }
                _cpids.Add(id.ContextPartitionId.Value);
                _contexts.Add(id.Hash);
                return _match.Contains(id.Hash);
            }

            public IList<int> Contexts
            {
                get { return _contexts; }
            }
        }
    }
}
