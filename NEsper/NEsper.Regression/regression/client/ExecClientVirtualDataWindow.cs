///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.supportregression.virtualdw;
using com.espertech.esper.util;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;
using static com.espertech.esper.supportregression.util.IndexBackingTableInfo;

namespace com.espertech.esper.regression.client
{
    public class ExecClientVirtualDataWindow : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            configuration.AddPlugInVirtualDataWindow("test", "vdw", typeof(SupportVirtualDWFactory).FullName);
            configuration.AddPlugInVirtualDataWindow("invalid", "invalid", typeof(InvalidTypeForTest));
            configuration.AddPlugInVirtualDataWindow("test", "testnoindex", typeof(SupportVirtualDWInvalidFactory));
            configuration.AddPlugInVirtualDataWindow("test", "exceptionvdw", typeof(SupportVirtualDWExceptionFactory));
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
            SupportQueryPlanIndexHook.Reset();
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionInsertConsume(epService);
            RunAssertionOnMerge(epService);
            RunAssertionLimitation(epService);
            RunAssertionJoinAndLifecyle(epService);
            RunAssertionSubquery(epService);
            RunAssertionContextWJoin(epService);
            RunAssertionContextWSubquery(epService);
            RunAssertionFireAndForget(epService);
            RunAssertionOnDelete(epService);
            RunAssertionInvalid(epService);
            RunAssertionManagementEvents(epService);
            RunAssertionIndexChoicesJoinUniqueVirtualDW(epService);
        }
    
        private void RunAssertionInsertConsume(EPServiceProvider epService) {
    
            epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as SupportBean");
            SupportVirtualDW window = (SupportVirtualDW) GetFromContext(epService, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = Collections.SingletonList<object>(supportBean);
            epService.EPAdministrator.CreateEPL("insert into MyVDW select * from SupportBean");
    
            // test straight consume
            string[] fields = "TheString,IntPrimitive".Split(',');
            EPStatement stmtConsume = epService.EPAdministrator.CreateEPL("select irstream * from MyVDW");
            var listener = new SupportUpdateListener();
            stmtConsume.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 200));
            Assert.IsNull(listener.LastOldData);
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[0], fields, new object[]{"E1", 200});
            stmtConsume.Dispose();
    
            // test aggregated consumer - wherein the virtual data window does not return an iterator that prefills the aggregation state
            fields = "val0".Split(',');
            EPStatement stmtAggregate = epService.EPAdministrator.CreateEPL("select sum(IntPrimitive) as val0 from MyVDW");
            stmtAggregate.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{100});
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{150});
    
            DestroyStmtsRemoveTypes(epService);
        }
    
        private void RunAssertionOnMerge(EPServiceProvider epService) {
            // defined test type
            var mapType = new Dictionary<string, object>();
            mapType.Put("col1", "string");
            mapType.Put("col2", "string");
            epService.EPAdministrator.Configuration.AddEventType("MapType", mapType);
    
            epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as MapType");
    
            // define some test data to return, via lookup
            SupportVirtualDW window = (SupportVirtualDW) GetFromContext(epService, "/virtualdw/MyVDW");
            var mapData = new Dictionary<string, object>();
            mapData.Put("col1", "key1");
            mapData.Put("col2", "key2");
            window.Data = Collections.SingletonList<object>(mapData);
    
            string[] fieldsMerge = "col1,col2".Split(',');
            EPStatement stmtMerge = epService.EPAdministrator.CreateEPL(
                "on SupportBean sb merge MyVDW vdw " +
                "where col1 = TheString " +
                "when matched then update set col2 = 'xxx'" +
                "when not matched then insert select TheString as col1, 'abc' as col2");
            var listener = new SupportUpdateListener();
            stmtMerge.Events += listener.Update;
            var listenerConsume = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from MyVDW").Events += listenerConsume.Update;
    
            // try yes-matched case
            epService.EPRuntime.SendEvent(new SupportBean("key1", 2));
            EPAssertionUtil.AssertProps(listener.LastOldData[0], fieldsMerge, new object[]{"key1", "key2"});
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[0], fieldsMerge, new object[]{"key1", "xxx"});
            EPAssertionUtil.AssertProps(window.LastUpdateOld[0], fieldsMerge, new object[]{"key1", "key2"});
            EPAssertionUtil.AssertProps(window.LastUpdateNew[0], fieldsMerge, new object[]{"key1", "xxx"});
            EPAssertionUtil.AssertProps(listenerConsume.AssertOneGetNewAndReset(), fieldsMerge, new object[]{"key1", "xxx"});
    
            // try not-matched case
            epService.EPRuntime.SendEvent(new SupportBean("key2", 3));
            Assert.IsNull(listener.LastOldData);
            EPAssertionUtil.AssertProps(listener.GetAndResetLastNewData()[0], fieldsMerge, new object[]{"key2", "abc"});
            EPAssertionUtil.AssertProps(listenerConsume.AssertOneGetNewAndReset(), fieldsMerge, new object[]{"key2", "abc"});
            Assert.IsNull(window.LastUpdateOld);
            EPAssertionUtil.AssertProps(window.LastUpdateNew[0], fieldsMerge, new object[]{"key2", "abc"});
    
            DestroyStmtsRemoveTypes(epService);
        }
    
        private void RunAssertionLimitation(EPServiceProvider epService) {
            EPStatement stmtWindow = epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as SupportBean");
            SupportVirtualDW window = (SupportVirtualDW) GetFromContext(epService, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = Collections.SingletonList<object>(supportBean);
            epService.EPAdministrator.CreateEPL("insert into MyVDW select * from SupportBean");
    
            // cannot iterate named window
            Assert.IsFalse(stmtWindow.HasFirst());
    
            // test data window aggregation (rows not included in aggregation)
            EPStatement stmtAggregate = epService.EPAdministrator.CreateEPL("select window(TheString) as val0 from MyVDW");
            var listener = new SupportUpdateListener();
            stmtAggregate.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E1"}, (string[]) listener.AssertOneGetNewAndReset().Get("val0"));
    
            DestroyStmtsRemoveTypes(epService);
        }
    
        private void RunAssertionJoinAndLifecyle(EPServiceProvider epService) {
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw(1, 'abc') as SupportBean");
    
            // define some test data to return, via lookup
            SupportVirtualDW window = (SupportVirtualDW) GetFromContext(epService, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            supportBean.LongPrimitive = 50;
            window.Data = Collections.SingletonList<object>(supportBean);
    
            Assert.IsNotNull(window.Context.EventFactory);
            Assert.AreEqual("MyVDW", window.Context.EventType.Name);
            Assert.IsNotNull(window.Context.StatementContext);
            Assert.AreEqual(2, window.Context.Parameters.Length);
            Assert.AreEqual(1, window.Context.Parameters[0]);
            Assert.AreEqual("abc", window.Context.Parameters[1]);
            Assert.AreEqual("MyVDW", window.Context.NamedWindowName);
    
            // test no-criteria join
            string[] fields = "st0.id,vdw.TheString,vdw.IntPrimitive".Split(',');
            EPStatement stmtJoinAll = epService.EPAdministrator.CreateEPL("select * from MyVDW vdw, SupportBean_ST0#lastevent st0");
            var listener = new SupportUpdateListener();
            stmtJoinAll.Events += listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "", "");
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", "S1", 100});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{}, window.LastAccessKeys);
            stmtJoinAll.Dispose();
    
            // test single-criteria join
            EPStatement stmtJoinSingle = epService.EPAdministrator.CreateEPL("select * from MyVDW vdw, SupportBean_ST0#lastevent st0 where vdw.TheString = st0.Id");
            stmtJoinSingle.Events += listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "TheString=(System.String)", "");
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E1"}, window.LastAccessKeys);
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean_ST0("S1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S1", "S1", 100});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"S1"}, window.LastAccessKeys);
            stmtJoinSingle.Dispose();
    
            // test multi-criteria join
            EPStatement stmtJoinMulti = epService.EPAdministrator.CreateEPL("select vdw.TheString from MyVDW vdw, SupportBeanRange#lastevent st0 " +
                    "where vdw.TheString = st0.id and LongPrimitive = keyLong and IntPrimitive between rangeStart and rangeEnd");
            stmtJoinMulti.Events += listener.Update;
            AssertIndexSpec(window.LastRequestedIndex,
                "TheString=(System.String)|" +
                "LongPrimitive=(" + TypeHelper.GetCleanName<long?>() + ")",
                "IntPrimitive[,](" + TypeHelper.GetCleanName<int?>() + ")");
    
            epService.EPRuntime.SendEvent(SupportBeanRange.MakeKeyLong("S1", 50L, 80, 120));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "vdw.TheString".Split(','), new object[]{"S1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"S1", 50L, new VirtualDataWindowKeyRange(80, 120)}, window.LastAccessKeys);
    
            // destroy
            stmt.Dispose();
            Assert.IsNull(GetFromContext(epService, "/virtualdw/MyVDW"));
            Assert.IsTrue(window.IsDestroyed);
    
            DestroyStmtsRemoveTypes(epService);
        }
    
        private void RunAssertionSubquery(EPServiceProvider epService) {
    
            SupportVirtualDW window = RegisterTypeSetMapData(epService);
    
            // test no-criteria subquery
            EPStatement stmtSubqueryAll = epService.EPAdministrator.CreateEPL("select (select col1 from MyVDW vdw) from SupportBean_ST0");
            var listener = new SupportUpdateListener();
            stmtSubqueryAll.Events += listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "", "");
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "col1".Split(','), new object[]{"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{}, window.LastAccessKeys);
            stmtSubqueryAll.Dispose();
    
            // test single-criteria subquery
            EPStatement stmtSubqSingleKey = epService.EPAdministrator.CreateEPL("select (select col1 from MyVDW vdw where col1=st0.id) as val0 from SupportBean_ST0 st0");
            stmtSubqSingleKey.Events += listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "col1=(System.String)", "");
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0".Split(','), new object[]{null});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E1"}, window.LastAccessKeys);
            epService.EPRuntime.SendEvent(new SupportBean_ST0("key1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0".Split(','), new object[]{"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"key1"}, window.LastAccessKeys);
            stmtSubqSingleKey.Dispose();
    
            // test multi-criteria subquery
            EPStatement stmtSubqMultiKey = epService.EPAdministrator.CreateEPL("select " +
                    "(select col1 from MyVDW vdw where col1=r.id and col2=r.key and col3 between r.rangeStart and r.rangeEnd) as val0 " +
                    "from SupportBeanRange r");
            stmtSubqMultiKey.Events += listener.Update;
            AssertIndexSpec(window.LastRequestedIndex,
                "col1=(System.String)|col2=(System.String)",
                "col3[,](" + TypeHelper.GetCleanName<int?>() + ")");
    
            epService.EPRuntime.SendEvent(new SupportBeanRange("key1", "key2", 5, 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "val0".Split(','), new object[]{"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"key1", "key2", new VirtualDataWindowKeyRange(5, 10)}, window.LastAccessKeys);
            stmtSubqMultiKey.Dispose();
    
            // test aggregation
            epService.EPAdministrator.CreateEPL("create schema SampleEvent as (id string)");
            epService.EPAdministrator.CreateEPL("create window MySampleWindow.test:vdw() as SampleEvent");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select (select count(*) as cnt from MySampleWindow) as c0 "
                    + "from SupportBean ste");
            stmt.Events += listener.Update;
    
            SupportVirtualDW thewindow = (SupportVirtualDW) GetFromContext(epService, "/virtualdw/MySampleWindow");
            IDictionary<string, Object> row1 = Collections.SingletonDataMap("id", "V1");
            thewindow.Data = Collections.SingletonList<object>(row1);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(1L, listener.AssertOneGetNewAndReset().Get("c0"));
    
            var rows = new HashSet<object>();
            rows.Add(row1);
            rows.Add(Collections.SingletonDataMap("id", "V2"));
            thewindow.Data = rows;
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(2L, listener.AssertOneGetNewAndReset().Get("c0"));
    
            DestroyStmtsRemoveTypes(epService);
        }
    
        private void RunAssertionContextWJoin(EPServiceProvider epService) {
            SupportVirtualDW.InitializationData = Collections.SingletonSet<object>(new SupportBean("E1", 1));
    
            // prepare
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.CreateEPL("create context MyContext coalesce by " +
                    "Consistent_hash_crc32(TheString) from SupportBean, " +
                    "Consistent_hash_crc32(p00) from SupportBean_S0 " +
                    "granularity 4 preallocate");
            epService.EPAdministrator.CreateEPL("context MyContext create window MyWindow.test:vdw() as SupportBean");
    
            // join
            string eplSubquerySameCtx = "context MyContext "
                    + "select * from SupportBean_S0 as s0 unidirectional, MyWindow as mw where mw.TheString = s0.p00";
            EPStatement stmtSameCtx = epService.EPAdministrator.CreateEPL(eplSubquerySameCtx);
            var listener = new SupportUpdateListener();
            stmtSameCtx.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsTrue(listener.IsInvoked);
    
            DestroyStmtsRemoveTypes(epService);
        }
    
        private void RunAssertionContextWSubquery(EPServiceProvider epService) {
            SupportVirtualDW.InitializationData = Collections.SingletonSet<object>(new SupportBean("E1", 1));
    
            // prepare
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.CreateEPL("create context MyContext coalesce by " +
                    "Consistent_hash_crc32(TheString) from SupportBean, " +
                    "Consistent_hash_crc32(p00) from SupportBean_S0 " +
                    "granularity 4 preallocate");
            epService.EPAdministrator.CreateEPL("context MyContext create window MyWindow.test:vdw() as SupportBean");
    
            // subquery - same context
            string eplSubquerySameCtx = "context MyContext "
                    + "select (select IntPrimitive from MyWindow mw where mw.TheString = s0.p00) as c0 "
                    + "from SupportBean_S0 s0";
            EPStatement stmtSameCtx = epService.EPAdministrator.CreateEPL(eplSubquerySameCtx);
            var listener = new SupportUpdateListener();
            stmtSameCtx.Events += listener.Update;
            epService.EPAdministrator.CreateEPL("@Hint('disable_window_subquery_indexshare') " + eplSubquerySameCtx);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            Assert.AreEqual(1, listener.AssertOneGetNewAndReset().Get("c0"));
            stmtSameCtx.Dispose();
    
            // subquery - no context
            string eplSubqueryNoCtx = "select (select IntPrimitive from MyWindow mw where mw.TheString = s0.p00) as c0 "
                    + "from SupportBean_S0 s0";
            try {
                epService.EPAdministrator.CreateEPL(eplSubqueryNoCtx);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to plan subquery number 1 querying MyWindow: Mismatch in context specification, the context for the named window 'MyWindow' is 'MyContext' and the query specifies no context  [select (select IntPrimitive from MyWindow mw where mw.TheString = s0.p00) as c0 from SupportBean_S0 s0]", ex.Message);
            }
    
            SupportVirtualDW.InitializationData = null;
            DestroyStmtsRemoveTypes(epService);
        }
    
        private void RunAssertionFireAndForget(EPServiceProvider epService) {
    
            SupportVirtualDW window = RegisterTypeSetMapData(epService);
    
            // test no-criteria FAF
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery("select col1 from MyVDW vdw");
            AssertIndexSpec(window.LastRequestedIndex, "", "");
            Assert.AreEqual("MyVDW", window.LastRequestedIndex.NamedWindowName);
            Assert.AreEqual(-1, window.LastRequestedIndex.StatementId);
            Assert.IsNull(window.LastRequestedIndex.StatementName);
            Assert.IsNotNull(window.LastRequestedIndex.StatementAnnotations);
            Assert.IsTrue(window.LastRequestedIndex.IsFireAndForget);
            EPAssertionUtil.AssertProps(result.Array[0], "col1".Split(','), new object[]{"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new Object[0], window.LastAccessKeys);
    
            // test single-criteria FAF
            result = epService.EPRuntime.ExecuteQuery("select col1 from MyVDW vdw where col1='key1'");
            AssertIndexSpec(window.LastRequestedIndex, "col1=(System.String)", "");
            EPAssertionUtil.AssertProps(result.Array[0], "col1".Split(','), new object[]{"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"key1"}, window.LastAccessKeys);
    
            // test multi-criteria subquery
            result = epService.EPRuntime.ExecuteQuery("select col1 from MyVDW vdw where col1='key1' and col2='key2' and col3 between 5 and 15");
            AssertIndexSpec(window.LastRequestedIndex,
                "col1=(System.String)|col2=(System.String)",
                "col3[,](" + Name.Clean<double>(false) + ")");
            EPAssertionUtil.AssertProps(result.Array[0], "col1".Split(','), new object[]{"key1"});
            EPAssertionUtil.AssertEqualsAnyOrder(new object[]{"key1", "key2", new VirtualDataWindowKeyRange(5d, 15d)}, window.LastAccessKeys);
    
            // test multi-criteria subquery
            result = epService.EPRuntime.ExecuteQuery("select col1 from MyVDW vdw where col1='key1' and col2>'key0' and col3 between 5 and 15");
            AssertIndexSpec(window.LastRequestedIndex,
                "col1=(System.String)",
                "col3[,](" + Name.Clean<double>(false) + ")|col2>(System.String)");
            EPAssertionUtil.AssertProps(result.Array[0], "col1".Split(','), new object[]{"key1"});
            EPAssertionUtil.AssertEqualsAnyOrder(new object[]{"key1", new VirtualDataWindowKeyRange(5d, 15d), "key0"}, window.LastAccessKeys);
    
            DestroyStmtsRemoveTypes(epService);
        }
    
        private void RunAssertionOnDelete(EPServiceProvider epService) {
            SupportVirtualDW window = RegisterTypeSetMapData(epService);
    
            // test no-criteria on-delete
            EPStatement stmtOnDeleteAll = epService.EPAdministrator.CreateEPL("on SupportBean_ST0 delete from MyVDW vdw");
            var listener = new SupportUpdateListener();
            stmtOnDeleteAll.Events += listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "", "");
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "col1".Split(','), new object[]{"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{}, window.LastAccessKeys);
            stmtOnDeleteAll.Dispose();
    
            // test single-criteria on-delete
            EPStatement stmtOnDeleteSingleKey = epService.EPAdministrator.CreateEPL("on SupportBean_ST0 st0 delete from MyVDW vdw where col1=st0.id");
            stmtOnDeleteSingleKey.Events += listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "col1=(System.String)", "");
    
            epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"E1"}, window.LastAccessKeys);
            Assert.IsFalse(listener.IsInvoked);
            epService.EPRuntime.SendEvent(new SupportBean_ST0("key1", 0));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "col1".Split(','), new object[]{"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"key1"}, window.LastAccessKeys);
            stmtOnDeleteSingleKey.Dispose();
    
            // test multie-criteria on-delete
            EPStatement stmtOnDeleteMultiKey = epService.EPAdministrator.CreateEPL("@Name('ABC') on SupportBeanRange r delete " +
                    "from MyVDW vdw where col1=r.id and col2=r.key and col3 between r.rangeStart and r.rangeEnd");
            stmtOnDeleteMultiKey.Events += listener.Update;
            AssertIndexSpec(window.LastRequestedIndex,
                "col1=(System.String)|col2=(System.String)",
                "col3[,](" + TypeHelper.GetCleanName<int?>() + ")");
            Assert.AreEqual("MyVDW", window.LastRequestedIndex.NamedWindowName);
            Assert.IsNotNull(window.LastRequestedIndex.StatementId);
            Assert.AreEqual("ABC", window.LastRequestedIndex.StatementName);
            Assert.AreEqual(1, window.LastRequestedIndex.StatementAnnotations.Length);
            Assert.IsFalse(window.LastRequestedIndex.IsFireAndForget);
    
            epService.EPRuntime.SendEvent(new SupportBeanRange("key1", "key2", 5, 10));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "col1".Split(','), new object[]{"key1"});
            EPAssertionUtil.AssertEqualsExactOrder(new object[]{"key1", "key2", new VirtualDataWindowKeyRange(5, 10)}, window.LastAccessKeys);
    
            DestroyStmtsRemoveTypes(epService);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            string epl;
    
            epl = "create window ABC.invalid:invalid() as SupportBean";
            TryInvalid(epService, epl, "Error starting statement: Virtual data window factory class " + Name.Clean<InvalidTypeForTest>() + " does not implement the interface com.espertech.esper.client.hook.VirtualDataWindowFactory [create window ABC.invalid:invalid() as SupportBean]");
    
            epl = "select * from SupportBean.test:vdw()";
            TryInvalid(epService, epl, "Error starting statement: Virtual data window requires use with a named window in the create-window syntax [select * from SupportBean.test:vdw()]");
    
            epService.EPAdministrator.CreateEPL("create window ABC.test:testnoindex() as SupportBean");
            epl = "select (select * from ABC) from SupportBean";
            TryInvalid(epService, epl, "Unexpected exception starting statement: Exception obtaining index lookup from virtual data window, the implementation has returned a null index [select (select * from ABC) from SupportBean]");
    
            try {
                epService.EPAdministrator.CreateEPL("create window ABC.test:exceptionvdw() as SupportBean");
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Error attaching view to event stream: Validation exception initializing virtual data window 'ABC': This is a test exception [create window ABC.test:exceptionvdw() as SupportBean]", ex.Message);
            }
        }
    
        private void RunAssertionManagementEvents(EPServiceProvider epService) {
            SupportVirtualDW vdw = RegisterTypeSetMapData(epService);
    
            // create-index event
            vdw.Events.Clear();
            EPStatement stmtIndex = epService.EPAdministrator.CreateEPL("create index IndexOne on MyVDW (col3, col2 btree)");
            VirtualDataWindowEventStartIndex startEvent = (VirtualDataWindowEventStartIndex) vdw.Events[0];
            Assert.AreEqual("MyVDW", startEvent.NamedWindowName);
            Assert.AreEqual("IndexOne", startEvent.IndexName);
            Assert.AreEqual(2, startEvent.Fields.Count);
            Assert.AreEqual("col3", ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(startEvent.Fields[0].Expressions[0]));
            Assert.AreEqual("hash", startEvent.Fields[0].Type);
            Assert.AreEqual("col2", ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(startEvent.Fields[1].Expressions[0]));
            Assert.AreEqual("btree", startEvent.Fields[1].Type);
            Assert.IsFalse(startEvent.IsUnique);
    
            // stop-index event
            vdw.Events.Clear();
            stmtIndex.Stop();
            VirtualDataWindowEventStopIndex stopEvent = (VirtualDataWindowEventStopIndex) vdw.Events[0];
            Assert.AreEqual("MyVDW", stopEvent.NamedWindowName);
            Assert.AreEqual("IndexOne", stopEvent.IndexName);
    
            // stop named window
            vdw.Events.Clear();
            epService.EPAdministrator.GetStatement("create-nw").Stop();
            VirtualDataWindowEventStopWindow stopWindow = (VirtualDataWindowEventStopWindow) vdw.Events[0];
            Assert.AreEqual("MyVDW", stopWindow.NamedWindowName);
    
            // start named window (not an event but a new factory call)
            SupportVirtualDWFactory.Windows.Clear();
            SupportVirtualDWFactory.Initializations.Clear();
            epService.EPAdministrator.GetStatement("create-nw").Start();
            Assert.AreEqual(1, SupportVirtualDWFactory.Windows.Count);
            Assert.AreEqual(1, SupportVirtualDWFactory.Initializations.Count);
    
            DestroyStmtsRemoveTypes(epService);
        }
    
        private void RunAssertionIndexChoicesJoinUniqueVirtualDW(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("SSB1", typeof(SupportSimpleBeanOne));
            var listener = new SupportUpdateListener();
    
            // test no where clause with unique on multiple props, exact specification of where-clause
            var assertSendEvents = new IndexAssertionEventSend(() => {
                string[] fields = "vdw.TheString,vdw.IntPrimitive,ssb1.i1".Split(',');
                epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("S1", 1, 102, 103));
                EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"S1", 101, 1});
            });
    
            var testCases = EnumHelper.GetValues<CaseEnum>();
            foreach (CaseEnum caseEnum in testCases) {
                TryAssertionVirtualDW(epService, listener, caseEnum, "TheString", "where vdw.TheString = ssb1.s1", true, assertSendEvents);
                TryAssertionVirtualDW(epService, listener, caseEnum, "i1", "where vdw.TheString = ssb1.s1", false, assertSendEvents);
                TryAssertionVirtualDW(epService, listener, caseEnum, "IntPrimitive", "where vdw.TheString = ssb1.s1", false, assertSendEvents);
                TryAssertionVirtualDW(epService, listener, caseEnum, "LongPrimitive", "where vdw.LongPrimitive = ssb1.l1", true, assertSendEvents);
                TryAssertionVirtualDW(epService, listener, caseEnum, "LongPrimitive,TheString", "where vdw.TheString = ssb1.s1 and vdw.LongPrimitive = ssb1.l1", true, assertSendEvents);
            }
        }
    
        private void TryAssertionVirtualDW(EPServiceProvider epService, SupportUpdateListener listener, CaseEnum caseEnum, string uniqueFields, string whereClause, bool unique, IndexAssertionEventSend assertion) {
            SupportQueryPlanIndexHook.Reset();
            SupportVirtualDWFactory.UniqueKeys = new HashSet<string>(uniqueFields.Split(','));
            epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as SupportBean");
            SupportVirtualDW window = (SupportVirtualDW) GetFromContext(epService, "/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 101);
            supportBean.DoublePrimitive = 102;
            supportBean.LongPrimitive = 103;
            window.Data = Collections.SingletonList<object>(supportBean);
    
            string eplUnique = INDEX_CALLBACK_HOOK +
                    "select * from ";
    
            if (caseEnum == CaseEnum.UNIDIRECTIONAL) {
                eplUnique += "SSB1 as ssb1 unidirectional ";
            } else {
                eplUnique += "SSB1#lastevent as ssb1 ";
            }
            eplUnique += ", MyVDW as vdw ";
            eplUnique += whereClause;
    
            EPStatement stmtUnique = epService.EPAdministrator.CreateEPL(eplUnique);
            stmtUnique.Events += listener.Update;
    
            // assert query plan
            SupportQueryPlanIndexHook.AssertJoinOneStreamAndReset(unique);
    
            // run assertion
            assertion.Invoke();
    
            epService.EPAdministrator.DestroyAllStatements();
            DestroyStmtsRemoveTypes(epService);
        }
    
        private enum CaseEnum {
            UNIDIRECTIONAL,
            MULTIDIRECTIONAL,
        }
    
        private SupportVirtualDW RegisterTypeSetMapData(EPServiceProvider epService) {
            var mapType = new Dictionary<string, object>();
            mapType.Put("col1", "string");
            mapType.Put("col2", "string");
            mapType.Put("col3", "int");
            epService.EPAdministrator.Configuration.AddEventType("MapType", mapType);
    
            SupportVirtualDWFactory.Initializations.Clear();
            epService.EPAdministrator.CreateEPL("@Name('create-nw') create window MyVDW.test:vdw() as MapType");
    
            Assert.AreEqual(1, SupportVirtualDWFactory.Initializations.Count);
            VirtualDataWindowFactoryContext factoryContext = SupportVirtualDWFactory.Initializations[0];
            Assert.IsNotNull(factoryContext.EventFactory);
            Assert.AreEqual("MyVDW", factoryContext.EventType.Name);
            Assert.IsNotNull("MyVDW", factoryContext.NamedWindowName);
            Assert.AreEqual(0, factoryContext.Parameters.Length);
            Assert.AreEqual(0, factoryContext.ParameterExpressions.Length);
            Assert.IsNotNull(factoryContext.ViewFactoryContext);
    
            // define some test data to return, via lookup
            SupportVirtualDW window = (SupportVirtualDW) GetFromContext(epService, "/virtualdw/MyVDW");
            var mapData = new Dictionary<string, object>();
            mapData.Put("col1", "key1");
            mapData.Put("col2", "key2");
            mapData.Put("col3", 10);
            window.Data = Collections.SingletonList<object>(mapData);
    
            return window;
        }
    
        private void AssertIndexSpec(VirtualDataWindowLookupContext indexSpec, string hashfields, string btreefields) {
            AssertIndexFields(hashfields, indexSpec.HashFields);
            AssertIndexFields(btreefields, indexSpec.BtreeFields);
        }
    
        private void AssertIndexFields(string hashfields, IList<VirtualDataWindowLookupFieldDesc> fields) {
            if (string.IsNullOrEmpty(hashfields) && fields.IsEmpty()) {
                return;
            }
            string[] split = hashfields.RegexSplit("\\|");
            var found = new List<string>();
            for (int i = 0; i < split.Length; i++) {
                if (i >= fields.Count)
                    Console.Write("break");
                VirtualDataWindowLookupFieldDesc field = fields[i];
                string result = field.PropertyName + field.Operator.Value.GetOp() + "(" + field.LookupValueType.GetCleanName() + ")";
                found.Add(result);
            }
            EPAssertionUtil.AssertEqualsAnyOrder(split, found.ToArray());
        }
    
        private void DestroyStmtsRemoveTypes(EPServiceProvider epService) {
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyVDW", true);
            epService.EPAdministrator.Configuration.RemoveEventType("MapType", true);
        }
    
        private VirtualDataWindow GetFromContext(EPServiceProvider epService, string name) {
            return (VirtualDataWindow) epService.Directory.Lookup(name);
        }

        public class InvalidTypeForTest { }
    }
} // end of namespace
