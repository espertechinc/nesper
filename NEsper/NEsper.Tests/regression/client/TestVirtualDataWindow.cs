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
using com.espertech.esper.client.hook;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.support.util;
using com.espertech.esper.support.virtualdw;
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestVirtualDataWindow
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();

            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            configuration.AddPlugInVirtualDataWindow("test", "vdw", typeof(SupportVirtualDWFactory).FullName);
            configuration.AddPlugInVirtualDataWindow("invalid", "invalid", typeof(InvalidTypeForTest).FullName);
            configuration.AddPlugInVirtualDataWindow("test", "testnoindex", typeof(SupportVirtualDWInvalidFactory).FullName);
            configuration.AddPlugInVirtualDataWindow("test", "exceptionvdw", typeof(SupportVirtualDWExceptionFactory).FullName);
            configuration.AddEventType("SupportBean", typeof(SupportBean));
            configuration.AddEventType("SupportBean_ST0", typeof(SupportBean_ST0));
            configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            SupportQueryPlanIndexHook.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            SupportVirtualDWFactory.UniqueKeys = null;
        }

        [Test]
        public void TestInsertConsume()
        {
            _epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as SupportBean");
            var window = (SupportVirtualDW)GetFromContext("/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = supportBean.AsSingleton();
            _epService.EPAdministrator.CreateEPL("insert into MyVDW select * from SupportBean");

            // test straight consume
            var fields = "TheString,IntPrimitive".Split(',');
            var stmtConsume = _epService.EPAdministrator.CreateEPL("select irstream * from MyVDW");
            stmtConsume.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 200));
            Assert.IsNull(_listener.LastOldData);
            EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[0], fields, new Object[] { "E1", 200 });
            stmtConsume.Dispose();

            // test aggregated consumer - wherein the virtual data window does not return an iterator that prefills the aggregation state
            fields = "val0".Split(',');
            var stmtAggregate = _epService.EPAdministrator.CreateEPL("select sum(IntPrimitive) as val0 from MyVDW");
            stmtAggregate.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 100 });

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 50));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { 150 });
            stmtAggregate.Dispose();
        }

        [Test]
        public void TestOnMerge()
        {
            // defined test type
            IDictionary<String, Object> mapType = new Dictionary<String, Object>();
            mapType["col1"] = "string";
            mapType["col2"] = "string";
            _epService.EPAdministrator.Configuration.AddEventType("MapType", mapType);

            _epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as MapType");

            // define some test data to return, via lookup
            var window = (SupportVirtualDW)GetFromContext("/virtualdw/MyVDW");
            IDictionary<String, Object> mapData = new Dictionary<String, Object>();
            mapData["col1"] = "key1";
            mapData["col2"] = "key2";
            window.Data = mapData.AsSingleton();

            var fieldsMerge = "col1,col2".Split(',');
            var stmtMerge = _epService.EPAdministrator.CreateEPL(
                "on SupportBean sb merge MyVDW vdw " +
                "where col1 = TheString " +
                "when matched then Update set col2 = 'xxx'" +
                "when not matched then insert select TheString as col1, 'abc' as col2");
            stmtMerge.Events += _listener.Update;
            var listenerConsume = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("select * from MyVDW").Events += listenerConsume.Update;

            // try yes-matched case
            _epService.EPRuntime.SendEvent(new SupportBean("key1", 2));
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fieldsMerge, new Object[] { "key1", "key2" });
            EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[0], fieldsMerge, new Object[] { "key1", "xxx" });
            EPAssertionUtil.AssertProps(window.LastUpdateOld[0], fieldsMerge, new Object[] { "key1", "key2" });
            EPAssertionUtil.AssertProps(window.LastUpdateNew[0], fieldsMerge, new Object[] { "key1", "xxx" });
            EPAssertionUtil.AssertProps(listenerConsume.AssertOneGetNewAndReset(), fieldsMerge, new Object[] { "key1", "xxx" });

            // try not-matched case
            _epService.EPRuntime.SendEvent(new SupportBean("key2", 3));
            Assert.IsNull(_listener.LastOldData);
            EPAssertionUtil.AssertProps(_listener.GetAndResetLastNewData()[0], fieldsMerge, new Object[] { "key2", "abc" });
            EPAssertionUtil.AssertProps(listenerConsume.AssertOneGetNewAndReset(), fieldsMerge, new Object[] { "key2", "abc" });
            Assert.IsNull(window.LastUpdateOld);
            EPAssertionUtil.AssertProps(window.LastUpdateNew[0], fieldsMerge, new Object[] { "key2", "abc" });
        }

        [Test]
        public void TestLimitation()
        {
            var stmtWindow = _epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as SupportBean");
            var window = (SupportVirtualDW)GetFromContext("/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            window.Data = supportBean.AsSingleton();
            _epService.EPAdministrator.CreateEPL("insert into MyVDW select * from SupportBean");

            // cannot iterate named window
            Assert.IsFalse(stmtWindow.HasFirst());

            // test data window aggregation (rows not included in aggregation)
            var stmtAggregate = _epService.EPAdministrator.CreateEPL("select Window(TheString) as val0 from MyVDW");
            stmtAggregate.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 100));
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E1" }, (String[])_listener.AssertOneGetNewAndReset().Get("val0"));
        }

        [Test]
        public void TestJoinAndLifecyle()
        {
            var stmt = _epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw(1, 'abc') as SupportBean");

            // define some test data to return, via lookup
            var window = (SupportVirtualDW)GetFromContext("/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 100);
            supportBean.LongPrimitive = 50;
            window.Data = supportBean.AsSingleton();

            Assert.NotNull(window.Context.EventFactory);
            Assert.AreEqual("MyVDW", window.Context.EventType.Name);
            Assert.NotNull(window.Context.StatementContext);
            Assert.AreEqual(2, window.Context.Parameters.Length);
            Assert.AreEqual(1, window.Context.Parameters[0]);
            Assert.AreEqual("abc", window.Context.Parameters[1]);
            Assert.AreEqual("MyVDW", window.Context.NamedWindowName);

            // test no-criteria join
            var fields = "st0.id,vdw.TheString,vdw.IntPrimitive".Split(',');
            var stmtJoinAll = _epService.EPAdministrator.CreateEPL("select * from MyVDW vdw, SupportBean_ST0.std:lastevent() st0");
            stmtJoinAll.Events += _listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "", "");

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "E1", "S1", 100 });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { }, window.LastAccessKeys);
            stmtJoinAll.Dispose();

            // test single-criteria join
            var stmtJoinSingle = _epService.EPAdministrator.CreateEPL("select * from MyVDW vdw, SupportBean_ST0.std:lastevent() st0 where vdw.TheString = st0.id");
            stmtJoinSingle.Events += _listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "TheString=(String)", "");

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E1" }, window.LastAccessKeys);
            Assert.IsFalse(_listener.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("S1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] { "S1", "S1", 100 });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "S1" }, window.LastAccessKeys);
            stmtJoinSingle.Dispose();

            // test multi-criteria join
            var stmtJoinMulti = _epService.EPAdministrator.CreateEPL("select vdw.TheString from MyVDW vdw, SupportBeanRange.std:lastevent() st0 " +
                    "where vdw.TheString = st0.id and LongPrimitive = keyLong and IntPrimitive between rangeStart and rangeEnd");
            stmtJoinMulti.Events += _listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "TheString=(String)|LongPrimitive=(Nullable<Int64>)", "IntPrimitive[,](Nullable<Int32>)");

            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeKeyLong("S1", 50L, 80, 120));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "vdw.TheString".Split(','), new Object[] { "S1" });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "S1", 50L, new VirtualDataWindowKeyRange(80, 120) }, window.LastAccessKeys);

            // destroy
            stmt.Dispose();
            Assert.IsNull(GetFromContext("/virtualdw/MyVDW"));
            Assert.IsTrue(window.IsDestroyed);
        }

        [Test]
        public void TestSubquery()
        {
            var window = RegisterTypeSetMapData();

            // test no-criteria subquery
            var stmtSubqueryAll = _epService.EPAdministrator.CreateEPL("select (select col1 from MyVDW vdw) from SupportBean_ST0");
            stmtSubqueryAll.Events += _listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "", "");

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "col1".Split(','), new Object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { }, window.LastAccessKeys);
            stmtSubqueryAll.Dispose();

            // test single-criteria subquery
            var stmtSubqSingleKey = _epService.EPAdministrator.CreateEPL("select (select col1 from MyVDW vdw where col1=st0.id) as val0 from SupportBean_ST0 st0");
            stmtSubqSingleKey.Events += _listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "col1=(String)", "");

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0".Split(','), new Object[] { null });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E1" }, window.LastAccessKeys);
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("key1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0".Split(','), new Object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "key1" }, window.LastAccessKeys);
            stmtSubqSingleKey.Dispose();

            // test multi-criteria subquery
            var stmtSubqMultiKey = _epService.EPAdministrator.CreateEPL("select " +
                    "(select col1 from MyVDW vdw where col1=r.id and col2=r.key and col3 between r.rangeStart and r.rangeEnd) as val0 " +
                    "from SupportBeanRange r");
            stmtSubqMultiKey.Events += _listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "col1=(String)|col2=(String)", "col3[,](Nullable<Int32>)");

            _epService.EPRuntime.SendEvent(new SupportBeanRange("key1", "key2", 5, 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "val0".Split(','), new Object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "key1", "key2", new VirtualDataWindowKeyRange(5, 10) }, window.LastAccessKeys);
            stmtSubqMultiKey.Dispose();

            // test aggregation
            _epService.EPAdministrator.CreateEPL("create schema SampleEvent as (id string)");
            _epService.EPAdministrator.CreateEPL("create window MySampleWindow.test:vdw() as SampleEvent");
            var stmt = _epService.EPAdministrator.CreateEPL("select (select count(*) as cnt from MySampleWindow) as c0 "
                    + "from SupportBean ste");
            stmt.Events += _listener.Update;

            var thewindow = (SupportVirtualDW) GetFromContext("/virtualdw/MySampleWindow");
            var row1 = Collections.SingletonDataMap("id", "V1");
            thewindow.Data = Collections.SingletonList<object>(row1);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual(1L, _listener.AssertOneGetNewAndReset().Get("c0"));

            var rows = new HashSet<object>();
            rows.Add(row1);
            rows.Add(Collections.SingletonDataMap("id", "V2"));
            thewindow.Data = rows;

            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.AreEqual(2L, _listener.AssertOneGetNewAndReset().Get("c0"));
        }

        [Test]
        public void TestContextWJoin()
        {
            SupportVirtualDW.InitializationData = new SupportBean("E1", 1).AsSet<object>();

            // prepare
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            _epService.EPAdministrator.CreateEPL("create context MyContext coalesce by " +
                    "consistent_hash_crc32(TheString) from SupportBean, " +
                    "consistent_hash_crc32(p00) from SupportBean_S0 " +
                    "granularity 4 preallocate");
            _epService.EPAdministrator.CreateEPL("context MyContext create window MyWindow.test:vdw() as SupportBean");

            // join
            var eplSubquerySameCtx = "context MyContext "
                    + "select * from SupportBean_S0 as s0 unidirectional, MyWindow as mw where mw.TheString = s0.p00";
            var stmtSameCtx = _epService.EPAdministrator.CreateEPL(eplSubquerySameCtx);
            stmtSameCtx.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "E1"));
            Assert.IsTrue(_listener.IsInvoked);
        }

        [Test]
        public void TestContextWSubquery()
        {
            SupportVirtualDW.InitializationData = new SupportBean("E1", 1).AsSet<object>();

            // prepare
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            _epService.EPAdministrator.CreateEPL("create context MyContext coalesce by " +
                    "consistent_hash_crc32(TheString) from SupportBean, " +
                    "consistent_hash_crc32(p00) from SupportBean_S0 " +
                    "granularity 4 preallocate");
            _epService.EPAdministrator.CreateEPL("context MyContext create window MyWindow.test:vdw() as SupportBean");

            // subquery - same context
            var eplSubquerySameCtx = "context MyContext "
                    + "select (select IntPrimitive from MyWindow mw where mw.TheString = s0.p00) as c0 "
                    + "from SupportBean_S0 s0";
            var stmtSameCtx = _epService.EPAdministrator.CreateEPL(eplSubquerySameCtx);
            stmtSameCtx.Events += _listener.Update;
            _epService.EPAdministrator.CreateEPL("@Hint('disable_window_subquery_indexshare') " + eplSubquerySameCtx);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            Assert.AreEqual(1, _listener.AssertOneGetNewAndReset().Get("c0"));
            stmtSameCtx.Dispose();

            // subquery - no context
            var eplSubqueryNoCtx = "select (select IntPrimitive from MyWindow mw where mw.TheString = s0.p00) as c0 "
                    + "from SupportBean_S0 s0";
            try {
                _epService.EPAdministrator.CreateEPL(eplSubqueryNoCtx);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Failed to plan subquery number 1 querying MyWindow: Mismatch in context specification, the context for the named window 'MyWindow' is 'MyContext' and the query specifies no context  [select (select IntPrimitive from MyWindow mw where mw.TheString = s0.p00) as c0 from SupportBean_S0 s0]", ex.Message);
            }

            SupportVirtualDW.InitializationData = null;
        }

        [Test]
        public void TestFireAndForget()
        {
            var window = RegisterTypeSetMapData();

            // test no-criteria FAF
            var result = _epService.EPRuntime.ExecuteQuery("select col1 from MyVDW vdw");
            AssertIndexSpec(window.LastRequestedIndex, "", "");
            Assert.AreEqual("MyVDW", window.LastRequestedIndex.NamedWindowName);
            Assert.IsNull(window.LastRequestedIndex.StatementId);
            Assert.IsNull(window.LastRequestedIndex.StatementName);
            Assert.NotNull(window.LastRequestedIndex.StatementAnnotations);
            Assert.IsTrue(window.LastRequestedIndex.IsFireAndForget);
            EPAssertionUtil.AssertProps(result.Array[0], "col1".Split(','), new Object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[0], window.LastAccessKeys);

            // test single-criteria FAF
            result = _epService.EPRuntime.ExecuteQuery("select col1 from MyVDW vdw where col1='key1'");
            AssertIndexSpec(window.LastRequestedIndex, "col1=(String)", "");
            EPAssertionUtil.AssertProps(result.Array[0], "col1".Split(','), new Object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "key1" }, window.LastAccessKeys);

            // test multi-criteria subquery
            result = _epService.EPRuntime.ExecuteQuery("select col1 from MyVDW vdw where col1='key1' and col2='key2' and col3 between 5 and 15");
            AssertIndexSpec(window.LastRequestedIndex, "col1=(String)|col2=(String)", "col3[,](Double)");
            EPAssertionUtil.AssertProps(result.Array[0], "col1".Split(','), new Object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "key1", "key2", new VirtualDataWindowKeyRange(5d, 15d) }, window.LastAccessKeys);

            // test multi-criteria subquery
            result = _epService.EPRuntime.ExecuteQuery("select col1 from MyVDW vdw where col1='key1' and col2>'key0' and col3 between 5 and 15");
            AssertIndexSpec(window.LastRequestedIndex, "col1=(String)", "col3[,](Double)|col2>(String)");
            EPAssertionUtil.AssertProps(result.Array[0], "col1".Split(','), new Object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "key1", new VirtualDataWindowKeyRange(5d, 15d), "key0" }, window.LastAccessKeys);
        }

        [Test]
        public void TestOnDelete()
        {
            var window = RegisterTypeSetMapData();

            // test no-criteria on-delete
            var stmtOnDeleteAll = _epService.EPAdministrator.CreateEPL("on SupportBean_ST0 delete from MyVDW vdw");
            stmtOnDeleteAll.Events += _listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "", "");

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "col1".Split(','), new Object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { }, window.LastAccessKeys);
            stmtOnDeleteAll.Dispose();

            // test single-criteria on-delete
            var stmtOnDeleteSingleKey = _epService.EPAdministrator.CreateEPL("on SupportBean_ST0 st0 delete from MyVDW vdw where col1=st0.id");
            stmtOnDeleteSingleKey.Events += _listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "col1=(String)", "");

            _epService.EPRuntime.SendEvent(new SupportBean_ST0("E1", 0));
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "E1" }, window.LastAccessKeys);
            Assert.IsFalse(_listener.IsInvoked);
            _epService.EPRuntime.SendEvent(new SupportBean_ST0("key1", 0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "col1".Split(','), new Object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "key1" }, window.LastAccessKeys);
            stmtOnDeleteSingleKey.Dispose();

            // test multie-criteria on-delete
            var stmtOnDeleteMultiKey = _epService.EPAdministrator.CreateEPL("@Name('ABC') on SupportBeanRange r delete " +
                    "from MyVDW vdw where col1=r.id and col2=r.key and col3 between r.rangeStart and r.rangeEnd");
            stmtOnDeleteMultiKey.Events += _listener.Update;
            AssertIndexSpec(window.LastRequestedIndex, "col1=(String)|col2=(String)", "col3[,](Nullable<Int32>)");
            Assert.AreEqual("MyVDW", window.LastRequestedIndex.NamedWindowName);
            Assert.NotNull(window.LastRequestedIndex.StatementId);
            Assert.AreEqual("ABC", window.LastRequestedIndex.StatementName);
            Assert.AreEqual(1, window.LastRequestedIndex.StatementAnnotations.Length);
            Assert.IsFalse(window.LastRequestedIndex.IsFireAndForget);

            _epService.EPRuntime.SendEvent(new SupportBeanRange("key1", "key2", 5, 10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "col1".Split(','), new Object[] { "key1" });
            EPAssertionUtil.AssertEqualsExactOrder(new Object[] { "key1", "key2", new VirtualDataWindowKeyRange(5, 10) }, window.LastAccessKeys);
            stmtOnDeleteMultiKey.Dispose();
        }

        [Test]
        public void TestInvalid()
        {
            String epl;

            epl = "create window ABC.invalid:invalid() as SupportBean";
            TryInvalid(epl, "Error starting statement: Virtual data window factory class com.espertech.esper.regression.client.TestVirtualDataWindow+InvalidTypeForTest does not implement the interface com.espertech.esper.client.hook.VirtualDataWindowFactory [create window ABC.invalid:invalid() as SupportBean]");

            epl = "select * from SupportBean.test:vdw()";
            TryInvalid(epl, "Error starting statement: Virtual data window requires use with a named window in the create-window syntax [select * from SupportBean.test:vdw()]");

            _epService.EPAdministrator.CreateEPL("create window ABC.test:testnoindex() as SupportBean");
            epl = "select (select * from ABC) from SupportBean";
            TryInvalid(epl, "Unexpected exception starting statement: Exception obtaining index lookup from virtual data window, the implementation has returned a null index [select (select * from ABC) from SupportBean]");

            try
            {
                _epService.EPAdministrator.CreateEPL("create window ABC.test:exceptionvdw() as SupportBean");
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual("Error starting statement: Error attaching view to event stream: Validation exception initializing virtual data window 'ABC': This is a test exception [create window ABC.test:exceptionvdw() as SupportBean]", ex.Message);
            }
        }

        [Test]
        public void TestManagementEvents()
        {
            var vdw = RegisterTypeSetMapData();

            // create-index event
            vdw.Events.Clear();
            var stmtIndex = _epService.EPAdministrator.CreateEPL("create index IndexOne on MyVDW (col3, col2 btree)");
            var startEvent = (VirtualDataWindowEventStartIndex)vdw.Events[0];
            Assert.AreEqual("MyVDW", startEvent.NamedWindowName);
            Assert.AreEqual("IndexOne", startEvent.IndexName);
            Assert.AreEqual(2, startEvent.Fields.Count);
            Assert.AreEqual("col3", startEvent.Fields[0].Name);
            Assert.AreEqual(true, startEvent.Fields[0].IsHash);
            Assert.AreEqual("col2", startEvent.Fields[1].Name);
            Assert.AreEqual(false, startEvent.Fields[1].IsHash);
            Assert.IsFalse(startEvent.IsUnique);

            // stop-index event
            vdw.Events.Clear();
            stmtIndex.Stop();
            var stopEvent = (VirtualDataWindowEventStopIndex)vdw.Events[0];
            Assert.AreEqual("MyVDW", stopEvent.NamedWindowName);
            Assert.AreEqual("IndexOne", stopEvent.IndexName);

            // stop named window
            vdw.Events.Clear();
            _epService.EPAdministrator.GetStatement("create-nw").Stop();
            var stopWindow = (VirtualDataWindowEventStopWindow)vdw.Events[0];
            Assert.AreEqual("MyVDW", stopWindow.NamedWindowName);

            // start named window (not an event but a new factory call)
            SupportVirtualDWFactory.Windows.Clear();
            SupportVirtualDWFactory.Initializations.Clear();
            _epService.EPAdministrator.GetStatement("create-nw").Start();
            Assert.AreEqual(1, SupportVirtualDWFactory.Windows.Count);
            Assert.AreEqual(1, SupportVirtualDWFactory.Initializations.Count);
        }

        [Test]
        public void TestIndexChoicesJoinUniqueVirtualDW()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportSimpleBeanOne>("SSB1");

            // test no where clause with unique on multiple props, exact specification of where-clause
            IndexAssertionEventSend assertSendEvents = () =>
            {
                var fields = "vdw.TheString,vdw.IntPrimitive,ssb1.i1".Split(',');
                _epService.EPRuntime.SendEvent(new SupportSimpleBeanOne("S1", 1, 102, 103));
                EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"S1", 101, 1});
            };

            var testCases = EnumHelper.GetValues<CaseEnum>();
            foreach (var caseEnum in testCases) {
                RunAssertionVirtualDw(caseEnum, "TheString", "where vdw.TheString = ssb1.s1", true, assertSendEvents);
                RunAssertionVirtualDw(caseEnum, "i1", "where vdw.TheString = ssb1.s1", false, assertSendEvents);
                RunAssertionVirtualDw(caseEnum, "IntPrimitive", "where vdw.TheString = ssb1.s1", false, assertSendEvents);
                RunAssertionVirtualDw(caseEnum, "LongPrimitive", "where vdw.LongPrimitive = ssb1.l1", true, assertSendEvents);
                RunAssertionVirtualDw(caseEnum, "LongPrimitive,TheString", "where vdw.TheString = ssb1.s1 and vdw.LongPrimitive = ssb1.l1", true, assertSendEvents);
            }
        }

        private void RunAssertionVirtualDw(CaseEnum caseEnum, String uniqueFields, String whereClause, bool unique, IndexAssertionEventSend assertion)
        {
            SupportVirtualDWFactory.UniqueKeys = new HashSet<String>(uniqueFields.SplitCsv());
            _epService.EPAdministrator.CreateEPL("create window MyVDW.test:vdw() as SupportBean");
            var window = (SupportVirtualDW) GetFromContext("/virtualdw/MyVDW");
            var supportBean = new SupportBean("S1", 101);
            supportBean.DoublePrimitive = 102;
            supportBean.LongPrimitive = 103;
            window.Data = supportBean.AsSingleton();

            var eplUnique = IndexBackingTableInfo.INDEX_CALLBACK_HOOK + "select * from ";

            if (caseEnum == CaseEnum.UNIDIRECTIONAL) {
                eplUnique += "SSB1 as ssb1 unidirectional ";
            }
            else {
                eplUnique += "SSB1.std:lastevent() as ssb1 ";
            }
            eplUnique += ", MyVDW as vdw ";
            eplUnique += whereClause;

            var stmtUnique = _epService.EPAdministrator.CreateEPL(eplUnique);
            stmtUnique.Events += _listener.Update;

            // assert query plan
            SupportQueryPlanIndexHook.AssertJoinOneStreamAndReset(unique);

            // run assertion
            assertion.Invoke();

            _epService.EPAdministrator.DestroyAllStatements();
        }

        internal enum CaseEnum
        {
            UNIDIRECTIONAL,
            MULTIDIRECTIONAL,
        }

        private void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }

        private SupportVirtualDW RegisterTypeSetMapData()
        {
            IDictionary<String, Object> mapType = new Dictionary<String, Object>();
            mapType["col1"] = "string";
            mapType["col2"] = "string";
            mapType["col3"] = "int";
            _epService.EPAdministrator.Configuration.AddEventType("MapType", mapType);

            SupportVirtualDWFactory.Initializations.Clear();
            _epService.EPAdministrator.CreateEPL("@Name('create-nw') create window MyVDW.test:vdw() as MapType");

            Assert.AreEqual(1, SupportVirtualDWFactory.Initializations.Count);
            var factoryContext = SupportVirtualDWFactory.Initializations[0];
            Assert.NotNull(factoryContext.EventFactory);
            Assert.AreEqual("MyVDW", factoryContext.EventType.Name);
            Assert.NotNull("MyVDW", factoryContext.NamedWindowName);
            Assert.AreEqual(0, factoryContext.Parameters.Length);
            Assert.AreEqual(0, factoryContext.ParameterExpressions.Length);
            Assert.NotNull(factoryContext.ViewFactoryContext);

            // define some test data to return, via lookup
            var window = (SupportVirtualDW)GetFromContext("/virtualdw/MyVDW");
            IDictionary<String, Object> mapData = new Dictionary<String, Object>();
            mapData["col1"] = "key1";
            mapData["col2"] = "key2";
            mapData["col3"] = 10;
            window.Data = mapData.AsSingleton();

            return window;
        }

        private void AssertIndexSpec(VirtualDataWindowLookupContext indexSpec, String hashfields, String btreefields)
        {
            AssertIndexFields(hashfields, indexSpec.HashFields);
            AssertIndexFields(btreefields, indexSpec.BtreeFields);
        }

        private void AssertIndexFields(String hashfields, IList<VirtualDataWindowLookupFieldDesc> fields)
        {
            if (string.IsNullOrEmpty(hashfields) && fields.IsEmpty())
            {
                return;
            }
            var split = hashfields.Split('|');
            for (var i = 0; i < split.Length; i++)
            {
                var expected = split[i];
                var field = fields[i];
                var found = field.PropertyName + field.Operator.Value.GetOp() + "(" + field.LookupValueType.GetCleanName(false) + ")";
                Assert.AreEqual(expected, found);
            }
        }

        private VirtualDataWindow GetFromContext(String name)
        {
            return (VirtualDataWindow)_epService.Directory.Lookup(name);
        }

        public class InvalidTypeForTest {}
    }
}
