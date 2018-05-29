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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraCreateIndex : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_A));
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));

            RunAssertionMultiRangeAndKey(epService, true);
            RunAssertionMultiRangeAndKey(epService, false);

            RunAssertionHashBTreeWidening(epService, true);
            RunAssertionHashBTreeWidening(epService, false);

            RunAssertionWidening(epService, true);
            RunAssertionWidening(epService, false);

            RunAssertionCompositeIndex(epService, true);
            RunAssertionCompositeIndex(epService, false);

            RunAssertionIndexReferences(epService, true);
            RunAssertionIndexReferences(epService, false);

            RunAssertionIndexStaleness(epService, true);
            RunAssertionIndexStaleness(epService, false);

            RunAssertionLateCreate(epService, true);
            RunAssertionLateCreate(epService, false);

            RunAssertionMultipleColumnMultipleIndex(epService, true);
            RunAssertionMultipleColumnMultipleIndex(epService, false);

            RunAssertionDropCreate(epService, true);
            RunAssertionDropCreate(epService, false);

            RunAssertionOnSelectReUse(epService, true);
            RunAssertionOnSelectReUse(epService, false);

            RunAssertionInvalid(epService, true);
            RunAssertionInvalid(epService, false);
        }

        private void RunAssertionInvalid(EPServiceProvider epService, bool namedWindow)
        {
            var eplCreate = namedWindow
                ? "create window MyInfraOne#keepall as (f1 string, f2 int)"
                : "create table MyInfraOne as (f1 string primary key, f2 int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("create index MyInfraIndex on MyInfraOne(f1)");

            epService.EPAdministrator.CreateEPL(
                "create context ContextOne initiated by SupportBean terminated after 5 sec");
            epService.EPAdministrator.CreateEPL(
                "create context ContextTwo initiated by SupportBean terminated after 5 sec");
            var eplCreateWContext = namedWindow
                ? "context ContextOne create window MyInfraCtx#keepall as (f1 string, f2 int)"
                : "context ContextOne create table MyInfraCtx as (f1 string primary key, f2 int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreateWContext);

            SupportMessageAssertUtil.TryInvalid(
                epService, "create index MyInfraIndex on MyInfraOne(f1)",
                "Error starting statement: An index by name 'MyInfraIndex' already exists [");

            SupportMessageAssertUtil.TryInvalid(
                epService, "create index IndexTwo on MyInfraOne(fx)",
                "Error starting statement: Property named 'fx' not found");

            SupportMessageAssertUtil.TryInvalid(
                epService, "create index IndexTwo on MyInfraOne(f1, f1)",
                "Error starting statement: Property named 'f1' has been declared more then once [create index IndexTwo on MyInfraOne(f1, f1)]");

            SupportMessageAssertUtil.TryInvalid(
                epService, "create index IndexTwo on MyWindowX(f1, f1)",
                "Error starting statement: A named window or table by name 'MyWindowX' does not exist [create index IndexTwo on MyWindowX(f1, f1)]");

            SupportMessageAssertUtil.TryInvalid(
                epService, "create index IndexTwo on MyInfraOne(f1 bubu, f2)",
                "Error starting statement: Unrecognized advanced-type index 'bubu'");

            SupportMessageAssertUtil.TryInvalid(
                epService, "create gugu index IndexTwo on MyInfraOne(f2)",
                "Invalid keyword 'gugu' in create-index encountered, expected 'unique' [create gugu index IndexTwo on MyInfraOne(f2)]");

            SupportMessageAssertUtil.TryInvalid(
                epService, "create unique index IndexTwo on MyInfraOne(f2 btree)",
                "Error starting statement: Combination of unique index with btree (range) is not supported [create unique index IndexTwo on MyInfraOne(f2 btree)]");

            // invalid context
            SupportMessageAssertUtil.TryInvalid(
                epService, "create unique index IndexTwo on MyInfraCtx(f1)",
                "Error starting statement: " + (namedWindow ? "Named window" : "Table") +
                " by name 'MyInfraCtx' has been declared for context 'ContextOne' and can only be used within the same context");
            SupportMessageAssertUtil.TryInvalid(
                epService, "context ContextTwo create unique index IndexTwo on MyInfraCtx(f1)",
                "Error starting statement: " + (namedWindow ? "Named window" : "Table") +
                " by name 'MyInfraCtx' has been declared for context 'ContextOne' and can only be used within the same context");

            // invalid insert-into unique index
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            var eplCreateTwo = namedWindow
                ? "@Name('create') create window MyInfraTwo#keepall as SupportBean"
                : "@Name('create') create table MyInfraTwo(TheString string primary key, IntPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreateTwo);
            epService.EPAdministrator.CreateEPL(
                "@Name('insert') insert into MyInfraTwo select TheString, IntPrimitive from SupportBean");
            epService.EPAdministrator.CreateEPL("create unique index I1 on MyInfraTwo(TheString)");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            try
            {
                epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
                Assert.Fail();
            }
            catch (Exception ex)
            {
                var text = namedWindow
                    ? "Unexpected exception in statement 'create': Unique index violation, index 'I1' is a unique index and key 'E1' already exists"
                    : "com.espertech.esper.client.EPException: Unexpected exception in statement 'insert': Unique index violation, index 'I1' is a unique index and key 'E1' already exists";
                Assert.AreEqual(text, ex.Message);
            }

            if (!namedWindow)
            {
                epService.EPAdministrator.CreateEPL("create table MyTable (p0 string, sumint sum(int))");
                SupportMessageAssertUtil.TryInvalid(
                    epService, "create index MyIndex on MyTable(p0)",
                    "Error starting statement: Tables without primary key column(s) do not allow creating an index [");
            }

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraOne", false);
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraTwo", false);
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraCtx", false);
        }

        private void RunAssertionOnSelectReUse(EPServiceProvider epService, bool namedWindow)
        {
            var stmtTextCreateOne = namedWindow
                ? "create window MyInfraONR#keepall as (f1 string, f2 int)"
                : "create table MyInfraONR as (f1 string primary key, f2 int primary key)";
            epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            epService.EPAdministrator.CreateEPL(
                "insert into MyInfraONR(f1, f2) select TheString, IntPrimitive from SupportBean");
            var indexOne = epService.EPAdministrator.CreateEPL("create index MyInfraONRIndex1 on MyInfraONR(f2)");
            var fields = "f1,f2".Split(',');

            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));

            var stmtOnSelect = epService.EPAdministrator.CreateEPL(
                "on SupportBean_S0 s0 select nw.f1 as f1, nw.f2 as f2 from MyInfraONR nw where nw.f2 = s0.id");
            var listener = new SupportUpdateListener();
            stmtOnSelect.Events += listener.Update;
            Assert.AreEqual(namedWindow ? 1 : 2, GetIndexCount(epService, namedWindow, "MyInfraONR"));

            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 1});

            indexOne.Dispose();

            epService.EPRuntime.SendEvent(new SupportBean_S0(1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 1});

            // create second identical statement
            var stmtTwo = epService.EPAdministrator.CreateEPL(
                "on SupportBean_S0 s0 select nw.f1 as f1, nw.f2 as f2 from MyInfraONR nw where nw.f2 = s0.id");
            Assert.AreEqual(namedWindow ? 1 : 2, GetIndexCount(epService, namedWindow, "MyInfraONR"));

            stmtOnSelect.Dispose();
            Assert.AreEqual(namedWindow ? 1 : 2, GetIndexCount(epService, namedWindow, "MyInfraONR"));

            stmtTwo.Dispose();
            Assert.AreEqual(namedWindow ? 0 : 1, GetIndexCount(epService, namedWindow, "MyInfraONR"));

            // two-key index order test
            epService.EPAdministrator.CreateEPL("create window MyInfraFour#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("create index idx1 on MyInfraFour (TheString, IntPrimitive)");
            epService.EPAdministrator.CreateEPL(
                "on SupportBean sb select * from MyInfraFour w where w.TheString = sb.TheString and w.IntPrimitive = sb.IntPrimitive");
            epService.EPAdministrator.CreateEPL(
                "on SupportBean sb select * from MyInfraFour w where w.IntPrimitive = sb.IntPrimitive and w.TheString = sb.TheString");
            Assert.AreEqual(1, GetNamedWindowMgmtService(epService).GetNamedWindowIndexes("MyInfraFour").Length);

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraONR", false);
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraFour", false);
        }

        private void RunAssertionDropCreate(EPServiceProvider epService, bool namedWindow)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));

            var stmtTextCreateOne = namedWindow
                ? "create window MyInfraDC#keepall as (f1 string, f2 int, f3 string, f4 string)"
                : "create table MyInfraDC as (f1 string primary key, f2 int primary key, f3 string primary key, f4 string primary key)";
            epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            epService.EPAdministrator.CreateEPL(
                "insert into MyInfraDC(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean");
            var indexOne = epService.EPAdministrator.CreateEPL("create index MyInfraDCIndex1 on MyInfraDC(f1)");
            var indexTwo = epService.EPAdministrator.CreateEPL("create index MyInfraDCIndex2 on MyInfraDC(f4)");
            var fields = "f1,f2".Split(',');

            epService.EPRuntime.SendEvent(new SupportBean("E1", -2));

            indexOne.Dispose();

            var result = epService.EPRuntime.ExecuteQuery("select * from MyInfraDC where f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2}});

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraDC where f4='?E1?'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2}});

            indexTwo.Dispose();

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraDC where f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2}});

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraDC where f4='?E1?'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2}});

            indexTwo = epService.EPAdministrator.CreateEPL("create index MyInfraDCIndex2 on MyInfraDC(f4)");

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraDC where f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2}});

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraDC where f4='?E1?'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2}});

            indexTwo.Dispose();
            Assert.AreEqual(namedWindow ? 0 : 1, GetIndexCount(epService, namedWindow, "MyInfraDC"));

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraDC", false);
        }

        private int GetIndexCount(EPServiceProvider epService, bool namedWindow, string name)
        {
            var repo = GetIndexInstanceRepo(epService, namedWindow, name);
            return repo.IndexDescriptors.Length;
        }

        private void RunAssertionMultipleColumnMultipleIndex(EPServiceProvider epService, bool namedWindow)
        {
            var stmtTextCreateOne = namedWindow
                ? "create window MyInfraMCMI#keepall as (f1 string, f2 int, f3 string, f4 string)"
                : "create table MyInfraMCMI as (f1 string primary key, f2 int, f3 string, f4 string)";
            epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            epService.EPAdministrator.CreateEPL(
                "insert into MyInfraMCMI(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean");
            epService.EPAdministrator.CreateEPL("create index MyInfraMCMIIndex1 on MyInfraMCMI(f2, f3, f1)");
            epService.EPAdministrator.CreateEPL("create index MyInfraMCMIIndex2 on MyInfraMCMI(f2, f3)");
            epService.EPAdministrator.CreateEPL("create index MyInfraMCMIIndex3 on MyInfraMCMI(f2)");
            var fields = "f1,f2,f3,f4".Split(',');

            epService.EPRuntime.SendEvent(new SupportBean("E1", -2));
            epService.EPRuntime.SendEvent(new SupportBean("E2", -4));
            epService.EPRuntime.SendEvent(new SupportBean("E3", -3));

            var result = epService.EPRuntime.ExecuteQuery("select * from MyInfraMCMI where f3='>E1<'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraMCMI where f3='>E1<' and f2=-2");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

            result = epService.EPRuntime.ExecuteQuery(
                "select * from MyInfraMCMI where f3='>E1<' and f2=-2 and f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraMCMI where f2=-2");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraMCMI where f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

            result = epService.EPRuntime.ExecuteQuery(
                "select * from MyInfraMCMI where f3='>E1<' and f2=-2 and f1='E1' and f4='?E1?'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraMCMI", false);
        }

        private void RunAssertionLateCreate(EPServiceProvider epService, bool namedWindow)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));

            var stmtTextCreateOne = namedWindow
                ? "create window MyInfraLC#keepall as (f1 string, f2 int, f3 string, f4 string)"
                : "create table MyInfraLC as (f1 string primary key, f2 int primary key, f3 string primary key, f4 string primary key)";
            epService.EPAdministrator.CreateEPL(stmtTextCreateOne);
            epService.EPAdministrator.CreateEPL(
                "insert into MyInfraLC(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean");

            epService.EPRuntime.SendEvent(new SupportBean("E1", -4));
            epService.EPRuntime.SendEvent(new SupportBean("E1", -2));
            epService.EPRuntime.SendEvent(new SupportBean("E1", -3));

            epService.EPAdministrator.CreateEPL("create index MyInfraLCIndex on MyInfraLC(f2, f3, f1)");
            var fields = "f1,f2,f3,f4".Split(',');

            var result = epService.EPRuntime.ExecuteQuery("select * from MyInfraLC where f3='>E1<' order by f2 asc");
            EPAssertionUtil.AssertPropsPerRow(
                result.Array, fields, new object[][]
                {
                    new object[] {"E1", -4, ">E1<", "?E1?"},
                    new object[] {"E1", -3, ">E1<", "?E1?"},
                    new object[] {"E1", -2, ">E1<", "?E1?"}
                });

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraLC", false);
        }

        private void RunAssertionIndexStaleness(EPServiceProvider epService, bool isNamedWindow)
        {
            var eplCreate = isNamedWindow
                ? "@Hint('enable_window_subquery_indexshare') create window MyInfraIS#keepall (pkey string, col0 int, col1 long)"
                : "create table MyInfraIS (pkey string primary key, col0 int, col1 long)";
            epService.EPAdministrator.CreateEPL(eplCreate);

            epService.EPAdministrator.CreateEPL("@Name('idx') create index MyIndex on MyInfraIS (pkey, col0)");
            epService.EPAdministrator.CreateEPL(
                "on SupportBean merge MyInfraIS where TheString = pkey " +
                "when not matched then insert select TheString as pkey, IntPrimitive as col0, LongPrimitive as col1");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("on SupportBean_S0 select col0,col1 from MyInfraIS where pkey=p00")
                .Events += listener.Update;

            MakeSendSupportBean(epService, "E1", 10, 100L);
            AssertCols(epService, listener, "E1,E2", new[] {new object[] {10, 100L}, null});

            MakeSendSupportBean(epService, "E2", 11, 101L);
            AssertCols(epService, listener, "E1,E2", new[] {new object[] {10, 100L}, new object[] {11, 101L}});

            epService.EPAdministrator.GetStatement("idx").Dispose();

            MakeSendSupportBean(epService, "E3", 12, 102L);
            AssertCols(
                epService, listener, "E1,E2,E3",
                new[] {new object[] {10, 100L}, new object[] {11, 101L}, new object[] {12, 102L}});

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraIS", false);
        }

        private void RunAssertionIndexReferences(EPServiceProvider epService, bool isNamedWindow)
        {
            var eplCreate = isNamedWindow
                ? "@Hint('enable_window_subquery_indexshare') create window MyInfraIR#keepall (col0 string, pkey int)"
                : "create table MyInfraIR (col0 string, pkey int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL(
                "insert into MyInfraIR select TheString as col0, IntPrimitive as pkey from SupportBean");

            epService.EPAdministrator.CreateEPL("@Name('idx') create index MyIndex on MyInfraIR (col0)");
            epService.EPAdministrator.CreateEPL(
                "@Name('merge') on SupportBean_S1 merge MyInfraIR where col0 = p10 when matched then delete");
            epService.EPAdministrator.CreateEPL(
                "@Name('subq') select (select col0 from MyInfraIR where col0 = s1.p10) from SupportBean_S1 s1");
            epService.EPAdministrator.CreateEPL(
                "@Name('join') select col0 from MyInfraIR, SupportBean_S1#lastevent where col0 = p10");
            AssertIndexesRef(
                epService, isNamedWindow, "MyInfraIR", isNamedWindow ? "idx,merge,subq" : "idx,merge,subq,join");

            epService.EPAdministrator.GetStatement("idx").Dispose();
            AssertIndexesRef(epService, isNamedWindow, "MyInfraIR", isNamedWindow ? "merge,subq" : "merge,subq,join");

            epService.EPAdministrator.GetStatement("merge").Dispose();
            AssertIndexesRef(epService, isNamedWindow, "MyInfraIR", isNamedWindow ? "subq" : "subq,join");

            epService.EPAdministrator.GetStatement("subq").Dispose();
            AssertIndexesRef(epService, isNamedWindow, "MyInfraIR", isNamedWindow ? "" : "join");

            epService.EPAdministrator.GetStatement("join").Dispose();
            epService.EPRuntime.ExecuteQuery("select * from MyInfraIR where col0 = 'a'");
            epService.EPRuntime.ExecuteQuery("select * from MyInfraIR mt1, MyInfraIR mt2 where mt1.col0 = 'a'");
            Assert.IsNull(GetIndexEntry(epService, isNamedWindow, "MyInfraIR"));
            AssertIndexCountInstance(epService, isNamedWindow, "MyInfraIR", isNamedWindow ? 0 : 1);

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraIR", false);
        }

        private void AssertIndexCountInstance(EPServiceProvider epService, bool namedWindow, string name, int count)
        {
            var repo = GetIndexInstanceRepo(epService, namedWindow, name);
            Assert.AreEqual(count, repo.Tables.Count);
        }

        private EventTableIndexRepository GetIndexInstanceRepo(
            EPServiceProvider epService, bool namedWindow, string name)
        {
            if (namedWindow)
            {
                var instance = GetNamedWindowMgmtService(epService).GetProcessor(name).ProcessorInstanceNoContext;
                return instance.RootViewInstance.IndexRepository;
            }

            var metadata = GetTableService(epService).GetTableMetadata(name);
            return metadata.GetState(-1).IndexRepository;
        }

        private void AssertIndexesRef(EPServiceProvider epService, bool isNamedWindow, string name, string csvNames)
        {
            var entry = GetIndexEntry(epService, isNamedWindow, name);
            if (string.IsNullOrEmpty(csvNames))
            {
                Assert.IsNull(entry);
            }
            else
            {
                EPAssertionUtil.AssertEqualsAnyOrder(csvNames.Split(','), entry.ReferringStatements);
            }
        }

        private EventTableIndexMetadataEntry GetIndexEntry(EPServiceProvider epService, bool namedWindow, string name)
        {
            var descOne = new IndexedPropDesc("col0", typeof(string));
            var index = new IndexMultiKey(false, Collections.List(descOne), Collections. GetEmptyList<IndexedPropDesc>(), null);
            var meta = GetIndexMetaRepo(epService, namedWindow, name);
            return meta.Indexes.Get(index);
        }

        private EventTableIndexMetadata GetIndexMetaRepo(EPServiceProvider epService, bool namedWindow, string name)
        {
            if (namedWindow)
            {
                var processor = GetNamedWindowMgmtService(epService).GetProcessor(name);
                return processor.EventTableIndexMetadataRepo;
            }

            var metadata = GetTableService(epService).GetTableMetadata(name);
            return metadata.EventTableIndexMetadataRepo;
        }

        private TableService GetTableService(EPServiceProvider epService)
        {
            return ((EPServiceProviderSPI) epService).ServicesContext.TableService;
        }

        private NamedWindowMgmtService GetNamedWindowMgmtService(EPServiceProvider epService)
        {
            return ((EPServiceProviderSPI) epService).ServicesContext.NamedWindowMgmtService;
        }

        private void RunAssertionCompositeIndex(EPServiceProvider epService, bool isNamedWindow)
        {
            var stmtTextCreate = isNamedWindow
                ? "create window MyInfraCI#keepall as (f1 string, f2 int, f3 string, f4 string)"
                : "create table MyInfraCI as (f1 string primary key, f2 int, f3 string, f4 string)";
            epService.EPAdministrator.CreateEPL(stmtTextCreate);
            epService.EPAdministrator.CreateEPL(
                "insert into MyInfraCI(f1, f2, f3, f4) select TheString, IntPrimitive, '>'||TheString||'<', '?'||TheString||'?' from SupportBean");
            var indexOne = epService.EPAdministrator.CreateEPL("create index MyInfraCIIndex on MyInfraCI(f2, f3, f1)");
            var fields = "f1,f2,f3,f4".Split(',');

            epService.EPRuntime.SendEvent(new SupportBean("E1", -2));

            var result = epService.EPRuntime.ExecuteQuery("select * from MyInfraCI where f3='>E1<'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraCI where f3='>E1<' and f2=-2");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraCI where f3='>E1<' and f2=-2 and f1='E1'");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {"E1", -2, ">E1<", "?E1?"}});

            indexOne.Dispose();

            // test SODA
            var create = "create index MyInfraCIIndex on MyInfraCI(f2, f3, f1)";
            var model = epService.EPAdministrator.CompileEPL(create);
            Assert.AreEqual(create, model.ToEPL());

            var stmt = epService.EPAdministrator.Create(model);
            Assert.AreEqual(create, stmt.Text);

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraCI", false);
        }

        private void RunAssertionWidening(EPServiceProvider epService, bool isNamedWindow)
        {
            // widen to long
            var stmtTextCreate = isNamedWindow
                ? "create window MyInfraW#keepall as (f1 long, f2 string)"
                : "create table MyInfraW as (f1 long primary key, f2 string primary key)";
            epService.EPAdministrator.CreateEPL(stmtTextCreate);
            epService.EPAdministrator.CreateEPL(
                "insert into MyInfraW(f1, f2) select LongPrimitive, TheString from SupportBean");
            epService.EPAdministrator.CreateEPL("create index MyInfraWIndex1 on MyInfraW(f1)");
            var fields = "f1,f2".Split(',');

            SendEventLong(epService, "E1", 10L);

            var result = epService.EPRuntime.ExecuteQuery("select * from MyInfraW where f1=10");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {10L, "E1"}});

            // coerce to short
            stmtTextCreate = isNamedWindow
                ? "create window MyInfraWTwo#keepall as (f1 short, f2 string)"
                : "create table MyInfraWTwo as (f1 short primary key, f2 string primary key)";
            epService.EPAdministrator.CreateEPL(stmtTextCreate);
            epService.EPAdministrator.CreateEPL(
                "insert into MyInfraWTwo(f1, f2) select ShortPrimitive, TheString from SupportBean");
            epService.EPAdministrator.CreateEPL("create index MyInfraWTwoIndex1 on MyInfraWTwo(f1)");

            SendEventShort(epService, "E1", 2);

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraWTwo where f1=2");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {(short) 2, "E1"}});

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraW", false);
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraWTwo", false);
        }

        private void RunAssertionHashBTreeWidening(EPServiceProvider epService, bool isNamedWindow)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));

            // widen to long
            var eplCreate = isNamedWindow
                ? "create window MyInfraHBTW#keepall as (f1 long, f2 string)"
                : "create table MyInfraHBTW as (f1 long primary key, f2 string primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);

            var eplInsert = "insert into MyInfraHBTW(f1, f2) select LongPrimitive, TheString from SupportBean";
            epService.EPAdministrator.CreateEPL(eplInsert);

            epService.EPAdministrator.CreateEPL("create index MyInfraHBTWIndex1 on MyInfraHBTW(f1 btree)");
            var fields = "f1,f2".Split(',');

            SendEventLong(epService, "E1", 10L);
            var result = epService.EPRuntime.ExecuteQuery("select * from MyInfraHBTW where f1>9");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {10L, "E1"}});

            // SODA
            var epl = "create index IX1 on MyInfraHBTW(f1, f2 btree)";
            var model = epService.EPAdministrator.CompileEPL(epl);
            Assert.AreEqual(model.ToEPL(), epl);
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            Assert.AreEqual(epl, stmt.Text);

            // SODA with unique
            var eplUnique = "create unique index IX2 on MyInfraHBTW(f1)";
            var modelUnique = epService.EPAdministrator.CompileEPL(eplUnique);
            Assert.AreEqual(eplUnique, modelUnique.ToEPL());
            var stmtUnique = epService.EPAdministrator.CreateEPL(eplUnique);
            Assert.AreEqual(eplUnique, stmtUnique.Text);

            // coerce to short
            var eplCreateTwo = isNamedWindow
                ? "create window MyInfraHBTWTwo#keepall as (f1 short, f2 string)"
                : "create table MyInfraHBTWTwo as (f1 short primary key, f2 string primary key)";
            epService.EPAdministrator.CreateEPL(eplCreateTwo);

            var eplInsertTwo = "insert into MyInfraHBTWTwo(f1, f2) select ShortPrimitive, TheString from SupportBean";
            epService.EPAdministrator.CreateEPL(eplInsertTwo);
            epService.EPAdministrator.CreateEPL("create index MyInfraHBTWTwoIndex1 on MyInfraHBTWTwo(f1 btree)");

            SendEventShort(epService, "E1", 2);

            result = epService.EPRuntime.ExecuteQuery("select * from MyInfraHBTWTwo where f1>=2");
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, new[] {new object[] {(short) 2, "E1"}});

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraHBTW", false);
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraHBTWTwo", false);
        }

        private void RunAssertionMultiRangeAndKey(EPServiceProvider epService, bool isNamedWindow)
        {
            epService.EPAdministrator.Configuration.AddEventType("SupportBeanRange", typeof(SupportBeanRange));

            var eplCreate = isNamedWindow
                ? "create window MyInfraMRAK#keepall as SupportBeanRange"
                : "create table MyInfraMRAK(id string primary key, key string, keyLong long, rangeStartLong long primary key, rangeEndLong long primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);

            var eplInsert = isNamedWindow
                ? "insert into MyInfraMRAK select * from SupportBeanRange"
                : "on SupportBeanRange t0 merge MyInfraMRAK t1 where t0.id = t1.id when not matched then insert select id, key, keyLong, rangeStartLong, rangeEndLong";
            epService.EPAdministrator.CreateEPL(eplInsert);

            epService.EPAdministrator.CreateEPL(
                "create index idx1 on MyInfraMRAK(key hash, keyLong hash, rangeStartLong btree, rangeEndLong btree)");
            var fields = "id".Split(',');

            var query1 =
                "select * from MyInfraMRAK where rangeStartLong > 1 and rangeEndLong > 2 and keyLong=1 and key='K1' order by id asc";
            RunQueryAssertion(epService, query1, fields, null);

            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("E1", "K1", 1L, 2L, 3L));
            RunQueryAssertion(epService, query1, fields, new[] {new object[] {"E1"}});

            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("E2", "K1", 1L, 2L, 4L));
            RunQueryAssertion(epService, query1, fields, new[] {new object[] {"E1"}, new object[] {"E2"}});

            epService.EPRuntime.SendEvent(SupportBeanRange.MakeLong("E3", "K1", 1L, 3L, 3L));
            RunQueryAssertion(
                epService, query1, fields, new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

            var query2 =
                "select * from MyInfraMRAK where rangeStartLong > 1 and rangeEndLong > 2 and keyLong=1 order by id asc";
            RunQueryAssertion(
                epService, query2, fields, new[] {new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});

            Assert.AreEqual(isNamedWindow ? 1 : 2, GetIndexCount(epService, isNamedWindow, "MyInfraMRAK"));

            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfraMRAK", false);
        }

        private void RunQueryAssertion(EPServiceProvider epService, string epl, string[] fields, object[][] expected)
        {
            var result = epService.EPRuntime.ExecuteQuery(epl);
            EPAssertionUtil.AssertPropsPerRow(result.Array, fields, expected);
        }

        private void SendEventLong(EPServiceProvider epService, string theString, long longPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.TheString = theString;
            theEvent.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendEventShort(EPServiceProvider epService, string theString, short shortPrimitive)
        {
            var theEvent = new SupportBean();
            theEvent.TheString = theString;
            theEvent.ShortPrimitive = shortPrimitive;
            epService.EPRuntime.SendEvent(theEvent);
        }

        private void MakeSendSupportBean(
            EPServiceProvider epService, string theString, int intPrimitive, long longPrimitive)
        {
            var b = new SupportBean(theString, intPrimitive);
            b.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(b);
        }

        private void AssertCols(
            EPServiceProvider epService, SupportUpdateListener listener, string listOfP00, object[][] expected)
        {
            var p00s = listOfP00.Split(',');
            Assert.AreEqual(p00s.Length, expected.Length);
            for (var i = 0; i < p00s.Length; i++)
            {
                epService.EPRuntime.SendEvent(new SupportBean_S0(0, p00s[i]));
                if (expected[i] == null)
                {
                    Assert.IsFalse(listener.IsInvoked);
                }
                else
                {
                    EPAssertionUtil.AssertProps(
                        listener.AssertOneGetNewAndReset(), "col0,col1".Split(','), expected[i]);
                }
            }
        }
    }
} // end of namespace