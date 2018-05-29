///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using Avro.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.filter;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.context;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextHashSegmented : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.EngineDefaults.Logging.IsEnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            //RunAssertionScoringUseCase(epService);
            //RunAssertionContextPartitionSelection(epService);
            //RunAssertionHashSegmentedFilter(epService);
            RunAssertionHashSegmentedManyArg(epService);
            RunAssertionHashSegmentedMulti(epService);
            RunAssertionHashSegmented(epService);
            RunAssertionHashSegmentedBySingleRowFunc(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionScoringUseCase(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionScoringUseCase(epService, rep);
            }
        }
    
        private void TryAssertionScoringUseCase(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            string[] fields = "userId,keyword,sumScore".Split(',');
            string epl =
                    eventRepresentationEnum.GetAnnotationText() + " create schema ScoreCycle (userId string, keyword string, productId string, score long);\n" +
                            eventRepresentationEnum.GetAnnotationText() + " create schema UserKeywordTotalStream (userId string, keyword string, sumScore long);\n" +
                            "\n" +
                            eventRepresentationEnum.GetAnnotationText() + " create context HashByUserCtx as " +
                            "coalesce by consistent_hash_crc32(userId) from ScoreCycle, " +
                            "consistent_hash_crc32(userId) from UserKeywordTotalStream " +
                            "granularity 1000000;\n" +
                            "\n" +
                            "context HashByUserCtx create window ScoreCycleWindow#unique(productId, keyword) as ScoreCycle;\n" +
                            "\n" +
                            "context HashByUserCtx insert into ScoreCycleWindow select * from ScoreCycle;\n" +
                            "\n" +
                            "@Name('outOne') context HashByUserCtx insert into UserKeywordTotalStream \n" +
                            "select userId, keyword, sum(score) as sumScore from ScoreCycleWindow group by keyword;\n" +
                            "\n" +
                            "@Name('outTwo') context HashByUserCtx on UserKeywordTotalStream(sumScore > 10000) delete from ScoreCycleWindow;\n";
    
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("outOne").Events += listener.Update;
    
            MakeSendScoreEvent(epService, "ScoreCycle", eventRepresentationEnum, "Pete", "K1", "P1", 100);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"Pete", "K1", 100L});
    
            MakeSendScoreEvent(epService, "ScoreCycle", eventRepresentationEnum, "Pete", "K1", "P2", 15);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"Pete", "K1", 115L});
    
            MakeSendScoreEvent(epService, "ScoreCycle", eventRepresentationEnum, "Joe", "K1", "P2", 30);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"Joe", "K1", 30L});
    
            MakeSendScoreEvent(epService, "ScoreCycle", eventRepresentationEnum, "Joe", "K2", "P1", 40);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"Joe", "K2", 40L});
    
            MakeSendScoreEvent(epService, "ScoreCycle", eventRepresentationEnum, "Joe", "K1", "P1", 20);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"Joe", "K1", 50L});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("ScoreCycle", false);
            epService.EPAdministrator.Configuration.RemoveEventType("ScoreCycleWindow", false);
            epService.EPAdministrator.Configuration.RemoveEventType("UserKeywordTotalStream", false);
        }
    
        private void RunAssertionContextPartitionSelection(EPServiceProvider epService) {
            string[] fields = "c0,c1,c2".Split(',');
            epService.EPAdministrator.CreateEPL("create context MyCtx as coalesce consistent_hash_crc32(TheString) from SupportBean granularity 16 preallocate");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("context MyCtx select context.id as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, new[] {new object[] {5, "E1", 1}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 100));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 101));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(), stmt.GetSafeEnumerator(), fields, new[] {new object[] {5, "E1", 6}, new object[] {15, "E2", 10}, new object[] {9, "E3", 201}});
    
            // test iterator targeted hash
            var selector = new SupportSelectorByHashCode(Collections.SingletonSet(15));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, new[] {new object[] {15, "E2", 10}});
            selector = new SupportSelectorByHashCode(Collections.Set(1, 9, 5));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(selector), stmt.GetSafeEnumerator(selector), fields, new[] {new object[] {5, "E1", 6}, new object[] {9, "E3", 201}});
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorByHashCode(Collections.SingletonSet(99))).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorByHashCode(Collections.GetEmptySet<int>())).MoveNext());
            Assert.IsFalse(stmt.GetEnumerator(new SupportSelectorByHashCode(null)).MoveNext());
    
            // test iterator filtered
            var filtered = new MySelectorFilteredHash(Collections.Set<int>(15));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, new[] {new object[] {15, "E2", 10}});
            filtered = new MySelectorFilteredHash(Collections.Set(1, 9, 5));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmt.GetEnumerator(filtered), stmt.GetSafeEnumerator(filtered), fields, new[] {new object[] {5, "E1", 6}, new object[] {9, "E3", 201}});
    
            // test always-false filter - compare context partition info
            filtered = new MySelectorFilteredHash(Collections.GetEmptySet<int>());
            Assert.IsFalse(stmt.GetEnumerator(filtered).MoveNext());
            Assert.AreEqual(16, filtered.Contexts.Count);
    
            try {
                stmt.GetEnumerator(new ProxyContextPartitionSelectorSegmented() {
                    ProcPartitionKeys = () => null
                });
                Assert.Fail();
            } catch (InvalidContextPartitionSelector ex) {
                Assert.IsTrue(ex.Message.StartsWith("Invalid context partition selector, expected an implementation class of any of [ContextPartitionSelectorAll, ContextPartitionSelectorFiltered, ContextPartitionSelectorById, ContextPartitionSelectorHash] interfaces but received com."),
                    "message: " + ex.Message);
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;
    
            // invalid filter spec
            epl = "create context ACtx coalesce hash_code(IntPrimitive) from SupportBean(dummy = 1) granularity 10";
            TryInvalid(epService, epl, "Error starting statement: Failed to validate filter expression 'dummy=1': Property named 'dummy' is not valid in any stream [");
    
            // invalid hash code function
            epl = "create context ACtx coalesce hash_code_xyz(IntPrimitive) from SupportBean granularity 10";
            TryInvalid(epService, epl, "Error starting statement: For context 'ACtx' expected a hash function that is any of {consistent_hash_crc32, hash_code} or a plug-in single-row function or script but received 'hash_code_xyz' [");
    
            // invalid no-param hash code function
            epl = "create context ACtx coalesce hash_code() from SupportBean granularity 10";
            TryInvalid(epService, epl, "Error starting statement: For context 'ACtx' expected one or more parameters to the hash function, but found no parameter list [");
    
            // validate statement not applicable filters
            epService.EPAdministrator.CreateEPL("create context ACtx coalesce hash_code(IntPrimitive) from SupportBean granularity 10");
            epl = "context ACtx select * from SupportBean_S0";
            TryInvalid(epService, epl, "Error starting statement: Segmented context 'ACtx' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [");
    
            // invalid attempt to partition a named window's streams
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epl = "create context SegmentedByWhat partition by TheString from MyWindow";
            TryInvalid(epService, epl, "Error starting statement: Partition criteria may not include named windows [create context SegmentedByWhat partition by TheString from MyWindow]");
        }
    
        private void RunAssertionHashSegmentedFilter(EPServiceProvider epService) {
    
            string ctx = "HashSegmentedContext";
            string eplCtx = "@Name('context') create context " + ctx + " as " +
                    "coalesce " +
                    " consistent_hash_crc32(TheString) from SupportBean(IntPrimitive > 10) " +
                    "granularity 4 " +
                    "preallocate";
            epService.EPAdministrator.CreateEPL(eplCtx);
    
            string eplStmt = "context " + ctx + " " + "select context.name as c0, IntPrimitive as c1 from SupportBean#lastevent";
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplStmt);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            string[] fields = "c0,c1".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, 12});
            AssertIterator(statement, fields, new[] {new object[] {ctx, 12}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            AssertIterator(statement, fields, new[] {new object[] {ctx, 12}});
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E6", 15));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, 15});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionHashSegmentedManyArg(EPServiceProvider epService) {
            TryHash(epService, "consistent_hash_crc32(TheString, IntPrimitive)");
            TryHash(epService, "hash_code(TheString, IntPrimitive)");
        }
    
        private void TryHash(EPServiceProvider epService, string hashFunc) {
            string eplCtxCRC32 = "@Name('context') create context Ctx1 as coalesce " +
                    hashFunc + " from SupportBean " +
                    "granularity 1000000";
            epService.EPAdministrator.CreateEPL(eplCtxCRC32);
    
            string[] fields = "c1,c2,c3,c4,c5".Split(',');
            string eplStmt = "context Ctx1 select IntPrimitive as c1, " +
                    "sum(LongPrimitive) as c2, prev(1, LongPrimitive) as c3, prior(1, LongPrimitive) as c4," +
                    "(select p00 from SupportBean_S0#length(2)) as c5 " +
                    "from SupportBean#length(3)";
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplStmt);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(MakeBean("E1", 100, 20L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100, 20L, null, null, null});
    
            epService.EPRuntime.SendEvent(MakeBean("E1", 100, 21L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100, 41L, 20L, 20L, null});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1000, "S0"));
            epService.EPRuntime.SendEvent(MakeBean("E1", 100, 22L));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100, 63L, 21L, 21L, "S0"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionHashSegmentedMulti(EPServiceProvider epService) {
    
            string ctx = "HashSegmentedContext";
            string eplCtx = "@Name('context') create context " + ctx + " as " +
                    "coalesce " +
                    " consistent_hash_crc32(TheString) from SupportBean, " +
                    " consistent_hash_crc32(p00) from SupportBean_S0 " +
                    "granularity 4 " +
                    "preallocate";
            epService.EPAdministrator.CreateEPL(eplCtx);
            // comment-me-in: var codeFunc = new SupportHashCodeFuncGranularCRC32(4);
    
            string eplStmt = "context " + ctx + " " +
                    "select context.name as c0, IntPrimitive as c1, id as c2 from SupportBean#keepall as t1, SupportBean_S0#keepall as t2 where t1.TheString = t2.p00";
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplStmt);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            string[] fields = "c0,c1,c2".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E2"));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 11));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "E4"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "E1"));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, 10, 3});
            AssertIterator(statement, fields, new[] {new object[] {ctx, 10, 3}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "E4"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "E5"));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 12));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, 12, 1});
            AssertIterator(statement, fields, new[] {new object[] {ctx, 10, 3}, new object[] {ctx, 12, 1}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionHashSegmented(EPServiceProvider epService) {
    
            // Comment-in to see CRC32 code.
            for (int i = 0; i < 10; i++) {
                string key = "E" + i;
                long code = SupportHashCodeFuncGranularCRC32.ComputeCrc32(key) % 4;
                int hashCode = i.GetHashCode() % 4;
                //Log.Info(key + " code " + code + " hashCode " + hashCode);
            }
    
            // test CRC32 Hash
            FilterServiceSPI filterSPI = (FilterServiceSPI) ((EPServiceProviderSPI) epService).FilterService;
            string ctx = "HashSegmentedContext";
            string eplCtx = "@Name('context') create context " + ctx + " as " +
                    "coalesce consistent_hash_crc32(TheString) from SupportBean " +
                    "granularity 4 " +
                    "preallocate";
            epService.EPAdministrator.CreateEPL(eplCtx);
    
            string eplStmt = "context " + ctx + " " +
                    "select context.name as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString";
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplStmt);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            Assert.AreEqual(4, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 4, 0, 0, 0);
    
            TryAssertionHash(epService, listener, ctx, statement, new SupportHashCodeFuncGranularCRC32(4).CodeFor);
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            // test same with SODA
            EPStatementObjectModel modelCtx = epService.EPAdministrator.CompileEPL(eplCtx);
            Assert.AreEqual(eplCtx, modelCtx.ToEPL());
            EPStatement stmtCtx = epService.EPAdministrator.Create(modelCtx);
            Assert.AreEqual(eplCtx, stmtCtx.Text);
    
            statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplStmt);
            statement.Events += listener.Update;
            TryAssertionHash(epService, listener, ctx, statement, new SupportHashCodeFuncGranularCRC32(4).CodeFor);
    
            // test with Java-hashCode string hash
            epService.EPAdministrator.CreateEPL("@Name('context') create context " + ctx + " " +
                    "coalesce hash_code(TheString) from SupportBean " +
                    "granularity 6 " +
                    "preallocate");
    
            statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context " + ctx + " " +
                    "select context.name as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString");
            statement.Events += listener.Update;
            Assert.AreEqual(6, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 6, 0, 0, 0);
    
            TryAssertionHash(epService, listener, ctx, statement, HashCodeFuncGranularInternalHash(6));
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
    
            // test no pre-allocate
            epService.EPAdministrator.CreateEPL("@Name('context') create context " + ctx + " " +
                    "coalesce hash_code(TheString) from SupportBean " +
                    "granularity 16 ");
    
            statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL("context " + ctx + " " +
                    "select context.name as c0, TheString as c1, sum(IntPrimitive) as c2 from SupportBean#keepall group by TheString");
            statement.Events += listener.Update;
            Assert.AreEqual(1, filterSPI.FilterCountApprox);
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
    
            TryAssertionHash(epService, listener, ctx, statement, HashCodeFuncGranularInternalHash(16));
            Assert.AreEqual(0, filterSPI.FilterCountApprox);
        }
    
        private void TryAssertionHash(EPServiceProvider epService, SupportUpdateListener listener, string ctx, EPStatementSPI statement, HashCodeFunc codeFunc) {
    
            string[] fields = "c0,c1,c2".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 5));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "E1", 5});
            AssertIterator(statement, fields, new[] {new object[] {ctx, "E1", 5}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 6));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "E2", 6});
            AssertIterator(statement, fields, new[] {new object[] {ctx, "E1", 5}, new object[] {ctx, "E2", 6}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 7));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "E3", 7});
            AssertIterator(statement, fields, new[] {new object[] {ctx, "E1", 5}, new object[] {ctx, "E3", 7}, new object[] {ctx, "E2", 6}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 8));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "E4", 8});
            AssertIterator(statement, fields, new[] {new object[] {ctx, "E1", 5}, new object[] {ctx, "E3", 7}, new object[] {ctx, "E4", 8}, new object[] {ctx, "E2", 6}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E5", 9));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "E5", 9});
            AssertIterator(statement, fields, new[] {new object[] {ctx, "E5", 9}, new object[] {ctx, "E1", 5}, new object[] {ctx, "E3", 7}, new object[] {ctx, "E4", 8}, new object[] {ctx, "E2", 6}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "E1", 15});
            AssertIterator(statement, fields, new[] {new object[] {ctx, "E5", 9}, new object[] {ctx, "E1", 15}, new object[] {ctx, "E3", 7}, new object[] {ctx, "E4", 8}, new object[] {ctx, "E2", 6}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 11));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{ctx, "E4", 19});
            AssertIterator(statement, fields, new[] {new object[] {ctx, "E5", 9}, new object[] {ctx, "E1", 15}, new object[] {ctx, "E3", 7}, new object[] {ctx, "E4", 19}, new object[] {ctx, "E2", 6}});
    
            statement.Stop();
            AgentInstanceAssertionUtil.AssertInstanceCounts(statement.StatementContext, 0, 0, 0, 0);
    
            EPServiceProviderSPI spi = (EPServiceProviderSPI) epService;
            Assert.AreEqual(1, spi.ContextManagementService.ContextCount);
            epService.EPAdministrator.GetStatement("context").Dispose();
            Assert.AreEqual(1, spi.ContextManagementService.ContextCount);
    
            statement.Dispose();
            Assert.AreEqual(0, spi.ContextManagementService.ContextCount);
        }
    
        private void AssertIterator(EPStatementSPI statement, string[] fields, object[][] expected) {
            EventBean[] rows = EPAssertionUtil.EnumeratorToArray(statement.GetEnumerator());
            AssertIterator(rows, fields, expected);
    
            rows = EPAssertionUtil.EnumeratorToArray(statement.GetSafeEnumerator());
            AssertIterator(rows, fields, expected);
        }
    
        private void AssertIterator(EventBean[] events, string[] fields, object[][] expected) {
            object[][] result = EPAssertionUtil.EventsToObjectArr(events, fields);
            EPAssertionUtil.AssertEqualsAnyOrder(expected, result);
        }
    
        private SupportBean MakeBean(string theString, int intPrimitive, long longPrimitive) {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    
        private void RunAssertionHashSegmentedBySingleRowFunc(EPServiceProvider epService) {
    
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("myHash", GetType(), "MyHashFunc");
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("mySecond", GetType(), "MySecondFunc");
            epService.EPAdministrator.Configuration.AddImport(GetType().FullName);
    
            string eplCtx = "@Name('context') create context HashSegmentedContext as " +
                    "coalesce MyHash(*) from SupportBean " +
                    "granularity 4 " +
                    "preallocate";
            epService.EPAdministrator.CreateEPL(eplCtx);
    
            string eplStmt = "context HashSegmentedContext select context.id as c1, MyHash(*) as c2, MySecond(*, TheString) as c3, "
                    + this.GetType().Name + ".MySecondFunc(*, TheString) as c4 from SupportBean";
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL(eplStmt);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            string[] fields = "c1,c2,c3, c4".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, 3, "E1", "E1"});    // context id matches the number returned by myHashFunc
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{0, 0, "E2", "E2"});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", 7));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{3, 7, "E3", "E3"});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        public static int MyHashFunc(SupportBean sb) {
            return sb.IntPrimitive;
        }
    
        public static string MySecondFunc(SupportBean sb, string text) {
            return text;
        }
    
        private void MakeSendScoreEvent(EPServiceProvider epService, string typeName, EventRepresentationChoice eventRepresentationEnum, string userId, string keyword, string productId, long score) {
            if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new LinkedHashMap<string, object>();
                theEvent.Put("userId", userId);
                theEvent.Put("keyword", keyword);
                theEvent.Put("productId", productId);
                theEvent.Put("score", score);
                epService.EPRuntime.SendEvent(theEvent, typeName);
            } else if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{userId, keyword, productId, score}, typeName);
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var record = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, typeName).AsRecordSchema());
                record.Put("userId", userId);
                record.Put("keyword", keyword);
                record.Put("productId", productId);
                record.Put("score", score);
                epService.EPRuntime.SendEventAvro(record, typeName);
            } else {
                Assert.Fail();
            }
        }
    
        public delegate int HashCodeFunc(string key);

        public static HashCodeFunc HashCodeFuncGranularInternalHash(int granularity) {
            return key => key.GetHashCode() % granularity;
        }

        internal class MySelectorFilteredHash : ContextPartitionSelectorFiltered
        {
            private readonly ISet<int> _match;
    
            private readonly List<int?> _contexts = new List<int?>();
            private readonly LinkedHashSet<int?> _cpids = new LinkedHashSet<int?>();
    
            internal MySelectorFilteredHash(ISet<int> match) {
                this._match = match;
            }
    
            public bool Filter(ContextPartitionIdentifier contextPartitionIdentifier) {
                var id = (ContextPartitionIdentifierHash) contextPartitionIdentifier;
                if (_match == null && _cpids.Contains(id.ContextPartitionId)) {
                    throw new EPRuntimeException("Already exists context id: " + id.ContextPartitionId);
                }
                _cpids.Add(id.ContextPartitionId);
                _contexts.Add(id.Hash);
                return _match.Contains(id.Hash);
            }

            public IList<int?> Contexts => _contexts;
        }
    }
} // end of namespace
