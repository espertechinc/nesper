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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraCreateIndex 
    {
        private EPServiceProviderSPI _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            _epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _listener = new SupportUpdateListener();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
        }
    
        [Test]
        public void TestMultiRangeAndKey()
        {
            RunAssertionMultiRangeAndKey(true);
            RunAssertionMultiRangeAndKey(false);
        }
    
        [Test]
        public void TestHashBTreeWidening()
        {
            RunAssertionHashBTreeWidening(true);
            RunAssertionHashBTreeWidening(false);
        }
    
        [Test]
        public void TestWidening()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_A));
            RunAssertionWidening(true);
            RunAssertionWidening(false);
        }
    
        [Test]
        public void TestCompositeIndex()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_A));
            RunAssertionCompositeIndex(true);
            RunAssertionCompositeIndex(false);
        }
    
        [Test]
        public void TestIndexReferences()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
            RunAssertionIndexReferences(true);
            RunAssertionIndexReferences(false);
        }
    
        [Test]
        public void TestIndexStaleness()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            RunAssertionIndexStaleness(true);
            RunAssertionIndexStaleness(false);
        }
    
        [Test]
        public void TestLateCreate()
        {
            RunAssertionLateCreate(true);
            RunAssertionLateCreate(false);
        }
    
        [Test]
        public void TestMultipleColumnMultipleIndex()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
            RunAssertionMultipleColumnMultipleIndex(true);
            RunAssertionMultipleColumnMultipleIndex(false);
        }
    
        [Test]
        public void TestDropCreate()
        {
            RunAssertionDropCreate(true);
            RunAssertionDropCreate(false);
        }
    
        [Test]
        public void TestOnSelectReUse()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
    
            RunAssertionOnSelectReUse(true);
            RunAssertionOnSelectReUse(false);
        }
    
        [Test]
        public void TestInvalid()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            RunAssertionInvalid(true);
            RunAssertionInvalid(false);
        }
    
        private void RunAssertionInvalid(bool namedWindow)
        {
            var eplCreate = namedWindow ?
                    "create window MyInfra#keepall as (f1 string, f2 int)" :
                    "create table MyInfra as (f1 string primary key, f2 int primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("create index MyInfraIndex on MyInfra(f1)");
    
            _epService.EPAdministrator.CreateEPL("create context ContextOne initiated by SupportBean terminated after 5 sec");
            _epService.EPAdministrator.CreateEPL("create context ContextTwo initiated by SupportBean terminated after 5 sec");
            var eplCreateWContext = namedWindow ?
                    "context ContextOne create window MyInfraCtx#keepall as (f1 string, f2 int)" :
                    "context ContextOne create table MyInfraCtx as (f1 string primary key, f2 int primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreateWContext);
    
            SupportMessageAssertUtil.TryInvalid(_epService, "create index MyInfraIndex on MyInfra(f1)",
                    "Error starting statement: An index by name 'MyInfraIndex' already exists [");
    
            SupportMessageAssertUtil.TryInvalid(_epService, "create index IndexTwo on MyInfra(fx)",
                    "Error starting statement: Property named 'fx' not found");
    
            SupportMessageAssertUtil.TryInvalid(_epService, "create index IndexTwo on MyInfra(f1, f1)",
                    "Error starting statement: Property named 'f1' has been declared more then once [create index IndexTwo on MyInfra(f1, f1)]");
    
            SupportMessageAssertUtil.TryInvalid(_epService, "create index IndexTwo on MyWindowX(f1, f1)",
                    "Error starting statement: A named window or table by name 'MyWindowX' does not exist [create index IndexTwo on MyWindowX(f1, f1)]");
    
            SupportMessageAssertUtil.TryInvalid(_epService, "create index IndexTwo on MyWindowX(f1 bubu, f2)",
                    "Invalid column index type 'bubu' encountered, please use any of the following index type names [BTREE, HASH] [create index IndexTwo on MyWindowX(f1 bubu, f2)]");
    
            SupportMessageAssertUtil.TryInvalid(_epService, "create gugu index IndexTwo on MyInfra(f2)",
                    "Invalid keyword 'gugu' in create-index encountered, expected 'unique' [create gugu index IndexTwo on MyInfra(f2)]");
    
            SupportMessageAssertUtil.TryInvalid(_epService, "create unique index IndexTwo on MyInfra(f2 btree)",
                    "Error starting statement: Combination of unique index with btree (range) is not supported [create unique index IndexTwo on MyInfra(f2 btree)]");
    
            // invalid context
            SupportMessageAssertUtil.TryInvalid(_epService, "create unique index IndexTwo on MyInfraCtx(f1)",
                    "Error starting statement: " + (namedWindow ? "Named window" : "Table") + " by name 'MyInfraCtx' has been declared for context 'ContextOne' and can only be used within the same context");
            SupportMessageAssertUtil.TryInvalid(_epService, "context ContextTwo create unique index IndexTwo on MyInfraCtx(f1)",
                    "Error starting statement: " + (namedWindow ? "Named window" : "Table") + " by name 'MyInfraCtx' has been declared for context 'ContextOne' and can only be used within the same context");
    
            // invalid insert-into unique index
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            var eplCreateTwo = namedWindow ?
                    "@Name('create') create window MyInfraTwo#keepall as SupportBean" :
                    "@Name('create') create table MyInfraTwo(TheString string primary key, IntPrimitive int primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreateTwo);
            _epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyInfraTwo select TheString, IntPrimitive from SupportBean");
            _epService.EPAdministrator.CreateEPL("create unique index I1 on MyInfraTwo(TheString)");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            try {
                _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
                Assert.Fail();
            }
            catch (Exception ex) {
                var text = namedWindow ?
                    "Unexpected exception in statement 'create': Unique index violation, index 'I1' is a unique index and key 'E1' already exists" :
                    "com.espertech.esper.client.EPException: Unexpected exception in statement 'insert': Unique index violation, index 'I1' is a unique index and key 'E1' already exists";
                Assert.AreEqual(text, ex.Message);
            }
    
            if (!namedWindow) {
                _epService.EPAdministrator.CreateEPL("create table MyTable (p0 string, sumint sum(int))");
                SupportMessageAssertUtil.TryInvalid(_epService, "create index MyIndex on MyTable(p0)",
                        "Error starting statement: Tables without primary key column(s) do not allow creating an index [");
            }
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfraTwo", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfraCtx", false);
        }
    
        private void RunAssertionOnSelectReUse(bool namedWindow)
        {
            var stmtTextCreateOne = namedWindow ?
                    "create window MyInfra#keepall as (f1 string, f2 int)" :
                    "create table MyInfra as (f1 string primary key, f2 int primary key)";
            _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra(f1, f2) select TheString, IntPrimitive from SupportBean");
            var indexOne = _epService.EPAdministrator.CreateEPL("create index MyInfraIndex1 on MyInfra(f2)");
            var fields = "f1,f2".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            var stmtOnSelect = _epService.EPAdministrator.CreateEPL("on SupportBean_S0 s0 select nw.f1 as f1, nw.f2 as f2 from MyInfra nw where nw.f2 = s0.id");
            stmtOnSelect.AddListener(_listener);
            Assert.AreEqual(namedWindow ? 1 : 2, GetIndexCount(namedWindow));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            indexOne.Dispose();
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
    
            // create second identical statement
            var stmtTwo = _epService.EPAdministrator.CreateEPL("on SupportBean_S0 s0 select nw.f1 as f1, nw.f2 as f2 from MyInfra nw where nw.f2 = s0.id");
            Assert.AreEqual(namedWindow ? 1 : 2, GetIndexCount(namedWindow));

            stmtOnSelect.Dispose();
            Assert.AreEqual(namedWindow ? 1 : 2, GetIndexCount(namedWindow));

            stmtTwo.Dispose();
            Assert.AreEqual(namedWindow ? 0 : 1, GetIndexCount(namedWindow));
    
            // two-key index order test
            _epService.EPAdministrator.CreateEPL("create window MyInfraTwo#keepall as SupportBean");
            _epService.EPAdministrator.CreateEPL("create index idx1 on MyInfraTwo (TheString, IntPrimitive)");
            _epService.EPAdministrator.CreateEPL("on SupportBean sb select * from MyInfraTwo w where w.TheString = sb.TheString and w.IntPrimitive = sb.IntPrimitive");
            _epService.EPAdministrator.CreateEPL("on SupportBean sb select * from MyInfraTwo w where w.IntPrimitive = sb.IntPrimitive and w.TheString = sb.TheString");
            Assert.AreEqual(1, _epService.NamedWindowMgmtService.GetNamedWindowIndexes("MyInfraTwo").Length);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionDropCreate(bool namedWindow)
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
    
            var stmtTextCreateOne = namedWindow ?
                    "create window MyInfra#keepall as (f1 string, f2 int, f3 string, f4 string)" :
                    "create table MyInfra as (f1 string primary key, f2 int primary key, f3 string primary key, f4 string primary key)";
            _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean");
            var indexOne = _epService.EPAdministrator.CreateEPL("create index MyInfraIndex1 on MyInfra(f1)");
            var indexTwo = _epService.EPAdministrator.CreateEPL("create index MyInfraIndex2 on MyInfra(f4)");
            var fields = "f1,f2".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -2));

            indexOne.Dispose();
    
            var result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2 } });
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f4='?E1?'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2 } });

            indexTwo.Dispose();
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2 } });
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f4='?E1?'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2 } });
    
            indexTwo = _epService.EPAdministrator.CreateEPL("create index MyInfraIndex2 on MyInfra(f4)");
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2 } });
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f4='?E1?'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2 } });

            indexTwo.Dispose();
            Assert.AreEqual(namedWindow ? 0 : 1, GetIndexCount(namedWindow));
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private int GetIndexCount(bool namedWindow)
        {
            var repo = GetIndexInstanceRepo(namedWindow);
            return repo.GetIndexDescriptors().Length;
        }
    
        private void RunAssertionMultipleColumnMultipleIndex(bool namedWindow)
        {
            var stmtTextCreateOne = namedWindow ?
                    "create window MyInfra#keepall as (f1 string, f2 int, f3 string, f4 string)" :
                    "create table MyInfra as (f1 string primary key, f2 int, f3 string, f4 string)";
            _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean");
            _epService.EPAdministrator.CreateEPL("create index MyInfraIndex1 on MyInfra(f2, f3, f1)");
            _epService.EPAdministrator.CreateEPL("create index MyInfraIndex2 on MyInfra(f2, f3)");
            _epService.EPAdministrator.CreateEPL("create index MyInfraIndex3 on MyInfra(f2)");
            var fields = "f1,f2,f3,f4".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -2));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", -4));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", -3));
    
            var result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f3='>E1<'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2, ">E1<", "?E1?" } });
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f3='>E1<' and f2=-2");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2, ">E1<", "?E1?" } });
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f3='>E1<' and f2=-2 and f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2, ">E1<", "?E1?" } });
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f2=-2");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2, ">E1<", "?E1?" } });
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2, ">E1<", "?E1?" } });
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f3='>E1<' and f2=-2 and f1='E1' and f4='?E1?'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2, ">E1<", "?E1?" } });
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionLateCreate(bool namedWindow)
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
    
            var stmtTextCreateOne = namedWindow ?
                    "create window MyInfra#keepall as (f1 string, f2 int, f3 string, f4 string)" :
                    "create table MyInfra as (f1 string primary key, f2 int primary key, f3 string primary key, f4 string primary key)";
            _epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -4));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -2));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -3));
    
            _epService.EPAdministrator.CreateEPL("create index MyInfraIndex on MyInfra(f2, f3, f1)");
            var fields = "f1,f2,f3,f4".Split(',');
    
            var result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f3='>E1<' order by f2 asc");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][]{
                    new object[]{"E1", -4, ">E1<", "?E1?"}, new object[]{"E1", -3, ">E1<", "?E1?"}, new object[]{"E1", -2, ">E1<", "?E1?"}});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionIndexStaleness(bool isNamedWindow)
        {
            var eplCreate = isNamedWindow ?
                    "@Hint('enable_window_subquery_indexshare') create window MyInfra#keepall (pkey string, col0 int, col1 long)" :
                    "create table MyInfra (pkey string primary key, col0 int, col1 long)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
    
            _epService.EPAdministrator.CreateEPL("@Name('idx') create index MyIndex on MyInfra (pkey, col0)");
            _epService.EPAdministrator.CreateEPL("on SupportBean merge MyInfra where TheString = pkey " +
                    "when not matched then insert select TheString as pkey, IntPrimitive as col0, LongPrimitive as col1");
            _epService.EPAdministrator.CreateEPL("on SupportBean_S0 select col0,col1 from MyInfra where pkey=p00").AddListener(_listener);
    
            MakeSendSupportBean("E1", 10, 100L);
            AssertCols("E1,E2", new object[][] { new object[] { 10, 100L }, null });
    
            MakeSendSupportBean("E2", 11, 101L);
            AssertCols("E1,E2", new object[][] { new object[] { 10, 100L }, new object[] { 11, 101L } });
    
            _epService.EPAdministrator.GetStatement("idx").Dispose();
    
            MakeSendSupportBean("E3", 12, 102L);
            AssertCols("E1,E2,E3", new object[][] { new object[] { 10, 100L }, new object[] { 11, 101L }, new object[] { 12, 102L } });
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionIndexReferences(bool isNamedWindow)
        {
            var eplCreate = isNamedWindow ?
                    "@Hint('enable_window_subquery_indexshare') create window MyInfra#keepall (col0 string, pkey int)" :
                    "create table MyInfra (col0 string, pkey int primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra select TheString as col0, IntPrimitive as pkey from SupportBean");
    
            _epService.EPAdministrator.CreateEPL("@Name('idx') create index MyIndex on MyInfra (col0)");
            _epService.EPAdministrator.CreateEPL("@Name('merge') on SupportBean_S1 merge MyInfra where col0 = p10 when matched then delete");
            _epService.EPAdministrator.CreateEPL("@Name('subq') select (select col0 from MyInfra where col0 = s1.p10) from SupportBean_S1 s1");
            _epService.EPAdministrator.CreateEPL("@Name('join') select col0 from MyInfra, SupportBean_S1#lastevent where col0 = p10");
            AssertIndexesRef(isNamedWindow, isNamedWindow ? "idx,merge,subq" : "idx,merge,subq,join");
    
            _epService.EPAdministrator.GetStatement("idx").Dispose();
            AssertIndexesRef(isNamedWindow, isNamedWindow ? "merge,subq" : "merge,subq,join");
    
            _epService.EPAdministrator.GetStatement("merge").Dispose();
            AssertIndexesRef(isNamedWindow, isNamedWindow ? "subq" : "subq,join");
    
            _epService.EPAdministrator.GetStatement("subq").Dispose();
            AssertIndexesRef(isNamedWindow, isNamedWindow ? "" : "join");
    
            _epService.EPAdministrator.GetStatement("join").Dispose();
            _epService.EPRuntime.ExecuteQuery("select * from MyInfra where col0 = 'a'");
            _epService.EPRuntime.ExecuteQuery("select * from MyInfra mt1, MyInfra mt2 where mt1.col0 = 'a'");
            Assert.IsNull(GetIndexEntry(isNamedWindow));
            AssertIndexCountInstance(isNamedWindow, isNamedWindow ? 0 : 1);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void AssertIndexCountInstance(bool namedWindow, int count) {
            var repo = GetIndexInstanceRepo(namedWindow);
            Assert.AreEqual(count, repo.GetTables().Count);
        }
    
        private EventTableIndexRepository GetIndexInstanceRepo(bool namedWindow) {
            if (namedWindow) {
                var instance = NamedWindowMgmtService.GetProcessor("MyInfra").ProcessorInstanceNoContext;
                return instance.RootViewInstance.IndexRepository;
            }
            var metadata = TableService.GetTableMetadata("MyInfra");
            return metadata.GetState(-1).IndexRepository;
        }
    
        private void AssertIndexesRef(bool isNamedWindow, string csvNames) {
            var entry = GetIndexEntry(isNamedWindow);
            if (string.IsNullOrWhiteSpace(csvNames)) {
                Assert.IsNull(entry);
            }
            else {
                EPAssertionUtil.AssertEqualsAnyOrder(csvNames.Split(','), entry.ReferringStatements);
            }
        }
    
        private EventTableIndexMetadataEntry GetIndexEntry(bool namedWindow) {
            var descOne = new IndexedPropDesc("col0", typeof(string));
            var index = new IndexMultiKey(false, Collections.SingletonList(descOne), Collections.GetEmptyList<IndexedPropDesc>());
            var meta = GetIndexMetaRepo(namedWindow);
            return meta.Indexes.Get(index);
        }
    
        private EventTableIndexMetadata GetIndexMetaRepo(bool namedWindow) {
            if (namedWindow) {
                var processor = NamedWindowMgmtService.GetProcessor("MyInfra");
                return processor.EventTableIndexMetadataRepo;
            }
            else {
                var metadata = TableService.GetTableMetadata("MyInfra");
                return metadata.EventTableIndexMetadataRepo;
            }
        }

        private TableService TableService
        {
            get { return _epService.ServicesContext.TableService; }
        }

        private NamedWindowMgmtService NamedWindowMgmtService
        {
            get { return _epService.ServicesContext.NamedWindowMgmtService; }
        }

        private void RunAssertionCompositeIndex(bool isNamedWindow)
        {
            var stmtTextCreate = isNamedWindow ?
                    "create window MyInfra#keepall as (f1 string, f2 int, f3 string, f4 string)" :
                    "create table MyInfra as (f1 string primary key, f2 int, f3 string, f4 string)";
            _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean");
            var indexOne = _epService.EPAdministrator.CreateEPL("create index MyInfraIndex on MyInfra(f2, f3, f1)");
            var fields = "f1,f2,f3,f4".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", -2));
    
            var result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f3='>E1<'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2, ">E1<", "?E1?" } });
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f3='>E1<' and f2=-2");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2, ">E1<", "?E1?" } });
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f3='>E1<' and f2=-2 and f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { "E1", -2, ">E1<", "?E1?" } });
    
            indexOne.Dispose();
    
            // test SODA
            var create = "create index MyInfraIndex on MyInfra(f2, f3, f1)";
            var model = _epService.EPAdministrator.CompileEPL(create);
            Assert.AreEqual(create, model.ToEPL());
    
            var stmt = _epService.EPAdministrator.Create(model);
            Assert.AreEqual(create, stmt.Text);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionWidening(bool isNamedWindow)
        {
            // widen to long
            var stmtTextCreate = isNamedWindow ?
                    "create window MyInfra#keepall as (f1 long, f2 string)" :
                    "create table MyInfra as (f1 long primary key, f2 string primary key)";
            _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            _epService.EPAdministrator.CreateEPL("insert into MyInfra(f1, f2) select LongPrimitive, TheString from SupportBean");
            _epService.EPAdministrator.CreateEPL("create index MyInfraIndex1 on MyInfra(f1)");
            var fields = "f1,f2".Split(',');
    
            SendEventLong("E1", 10L);
    
            var result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f1=10");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { 10L, "E1" } });
    
            // coerce to short
            stmtTextCreate = isNamedWindow ?
                    "create window MyInfraTwo#keepall as (f1 short, f2 string)" :
                    "create table MyInfraTwo as (f1 short primary key, f2 string primary key)";
            _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            _epService.EPAdministrator.CreateEPL("insert into MyInfraTwo(f1, f2) select ShortPrimitive, TheString from SupportBean");
            _epService.EPAdministrator.CreateEPL("create index MyInfraTwoIndex1 on MyInfraTwo(f1)");
    
            SendEventShort("E1", (short) 2);
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfraTwo where f1=2");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { (short)2, "E1" } });
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfraTwo", false);
        }
    
        public void RunAssertionHashBTreeWidening(bool isNamedWindow) {
    
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean_A>();
    
            // widen to long
            var eplCreate = isNamedWindow ?
                    "create window MyInfra#keepall as (f1 long, f2 string)" :
                    "create table MyInfra as (f1 long primary key, f2 string primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
    
            var eplInsert = "insert into MyInfra(f1, f2) select LongPrimitive, TheString from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplInsert);
    
            _epService.EPAdministrator.CreateEPL("create index MyInfraIndex1 on MyInfra(f1 btree)");
            var fields = "f1,f2".Split(',');
    
            SendEventLong("E1", 10L);
            var result = _epService.EPRuntime.ExecuteQuery("select * from MyInfra where f1>9");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { 10L, "E1" } });
    
            // SODA
            var epl = "create index IX1 on MyInfra(f1, f2 btree)";
            var model = _epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(model.ToEPL(), epl);
            var stmt = _epService.EPAdministrator.CreateEPL(epl);
            Assert.AreEqual(epl, stmt.Text);
    
            // SODA with unique
            var eplUnique = "create unique index IX2 on MyInfra(f1)";
            var modelUnique = _epService.EPAdministrator.CompileEPL(eplUnique);
            Assert.AreEqual(eplUnique, modelUnique.ToEPL());
            var stmtUnique = _epService.EPAdministrator.CreateEPL(eplUnique);
            Assert.AreEqual(eplUnique, stmtUnique.Text);
    
            // coerce to short
            var eplCreateTwo = isNamedWindow ?
                    "create window MyInfraTwo#keepall as (f1 short, f2 string)" :
                    "create table MyInfraTwo as (f1 short primary key, f2 string primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreateTwo);
    
            var eplInsertTwo = "insert into MyInfraTwo(f1, f2) select ShortPrimitive, TheString from SupportBean";
            _epService.EPAdministrator.CreateEPL(eplInsertTwo);
            _epService.EPAdministrator.CreateEPL("create index MyInfraTwoIndex1 on MyInfraTwo(f1 btree)");
    
            SendEventShort("E1", (short) 2);
    
            result = _epService.EPRuntime.ExecuteQuery("select * from MyInfraTwo where f1>=2");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new object[][] { new object[] { (short)2, "E1" } });
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfraTwo", false);
        }
    
        private void RunAssertionMultiRangeAndKey(bool isNamedWindow) {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));
    
            var eplCreate = isNamedWindow ?
                    "create window MyInfra#keepall as SupportBeanRange" :
                    "create table MyInfra(id string primary key, key string, keyLong long, rangeStartLong long primary key, rangeEndLong long primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreate);
    
            var eplInsert = isNamedWindow ?
                    "insert into MyInfra select * from SupportBeanRange" :
                    "on SupportBeanRange t0 merge MyInfra t1 where t0.id = t1.id when not matched then insert select id, key, keyLong, rangeStartLong, rangeEndLong";
            _epService.EPAdministrator.CreateEPL(eplInsert);
    
            _epService.EPAdministrator.CreateEPL("create index idx1 on MyInfra(key hash, keyLong hash, rangeStartLong btree, rangeEndLong btree)");
            var fields = "id".Split(',');
    
            var query1 = "select * from MyInfra where rangeStartLong > 1 and rangeEndLong > 2 and keyLong=1 and key='K1' order by id asc";
            RunQueryAssertion(query1, fields, null);
            
            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("E1", "K1", 1L, 2L, 3L));
            RunQueryAssertion(query1, fields, new object[][] { new object[] { "E1" } });
    
            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("E2", "K1", 1L, 2L, 4L));
            RunQueryAssertion(query1, fields, new object[][] { new object[] { "E1" }, new object[] { "E2" } });
    
            _epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("E3", "K1", 1L, 3L, 3L));
            RunQueryAssertion(query1, fields, new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });
    
            var query2 = "select * from MyInfra where rangeStartLong > 1 and rangeEndLong > 2 and keyLong=1 order by id asc";
            RunQueryAssertion(query2, fields, new object[][] { new object[] { "E1" }, new object[] { "E2" }, new object[] { "E3" } });
    
            Assert.AreEqual(isNamedWindow ? 1 : 2, GetIndexCount(isNamedWindow));
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunQueryAssertion(string epl, string[] fields, object[][] expected) {
            var result = _epService.EPRuntime.ExecuteQuery(epl);
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, expected);
        }
    
        private void SendEventLong(string theString, long longPrimitive) {
            var theEvent = new SupportBean();
            theEvent.TheString = theString;
            theEvent.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEventShort(string theString, short shortPrimitive) {
            var theEvent = new SupportBean();
            theEvent.TheString = theString;
            theEvent.ShortPrimitive = shortPrimitive;
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void MakeSendSupportBean(string theString, int intPrimitive, long longPrimitive) {
            var b = new SupportBean(theString, intPrimitive);
            b.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(b);
        }
    
        private void AssertCols(string listOfP00, object[][] expected) {
            var p00s = listOfP00.Split(',');
            Assert.AreEqual(p00s.Length, expected.Length);
            for (var i = 0; i < p00s.Length; i++) {
                _epService.EPRuntime.SendEvent(new SupportBean_S0(0, p00s[i]));
                if (expected[i] == null) {
                    Assert.IsFalse(_listener.IsInvoked);
                }
                else {
                    EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "col0,col1".Split(','), expected[i]);
                }
            }
        }
    }
}
