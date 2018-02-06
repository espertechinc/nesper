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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.events;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableOnMerge
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportUpdateListener _listenerMerge;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            foreach (var clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1)}) {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            _listener = new SupportUpdateListener();
            _listenerMerge = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
            _listenerMerge = null;
        }
    
        [Test]
        public void TestMergeWhereWithMethodRead()
        {
            _epService.EPAdministrator.CreateEPL("create table varagg (KeyOne string primary key, cnt count(*))");
            _epService.EPAdministrator.CreateEPL("into table varagg select count(*) as cnt " +
                    "from SupportBean#lastevent group by TheString");
    
            _epService.EPAdministrator.CreateEPL("select varagg[p00].KeyOne as c0 from SupportBean_S0").AddListener(_listener);
            _epService.EPAdministrator.CreateEPL("on SupportBean_S1 merge varagg where cnt = 0 when matched then delete");
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 0));
            AssertKeyFound("G1,G2,G3", new bool[]{true, true, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0)); // delete
            AssertKeyFound("G1,G2,G3", new bool[] {false, true, false});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 0));
            AssertKeyFound("G1,G2,G3", new bool[]{false, true, true});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0));  // delete
            AssertKeyFound("G1,G2,G3", new bool[] {false, false, true});
        }
    
        [Test]
        public void TestMergeSelectWithAggReadAndEnum()
        {
            _epService.EPAdministrator.CreateEPL("create table varagg (" +
                    "eventset window(*) @type(SupportBean), total sum(int))");
            _epService.EPAdministrator.CreateEPL("into table varagg select window(*) as eventset, " +
                    "sum(IntPrimitive) as total from SupportBean#length(2)");
            _epService.EPAdministrator.CreateEPL("on SupportBean_S0 merge varagg " +
                    "when matched then insert into ResultStream select eventset, total, eventset.takeLast(1) as c0");
            _epService.EPAdministrator.CreateEPL("select * from ResultStream").AddListener(_listener);
    
            var e1 = new SupportBean("E1", 15);
            _epService.EPRuntime.SendEvent(e1);
    
            AssertResultAggRead(new object[] {e1}, 15);
    
            var e2 = new SupportBean("E2", 20);
            _epService.EPRuntime.SendEvent(e2);
    
            AssertResultAggRead(new object[]{e1, e2}, 35);
    
            var e3 = new SupportBean("E3", 30);
            _epService.EPRuntime.SendEvent(e3);
    
            AssertResultAggRead(new object[]{e2, e3}, 50);
        }
    
        private void AssertResultAggRead(object[] objects, int total)
        {
            var fields = "eventset,total".Split(',');
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            var @event = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@event, fields, new object[] {objects, total});
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {objects[objects.Length-1]}, @event.Get("c0").Unwrap<object>());
        }
    
        private void AssertKeyFound(string keyCsv, bool[] expected)
        {
            var split = keyCsv.Split(',');
            for (var i = 0; i < split.Length; i++) {
                var key = split[i];
                _epService.EPRuntime.SendEvent(new SupportBean_S0(0, key));
                var expectedString = expected[i] ? key : null;
                Assert.AreEqual(expectedString, _listener.AssertOneGetNewAndReset().Get("c0"), "failed for key '" + key + "'");
            }
        }
    
        [Test]
        public void TestOnMergePlainPropsAnyKeyed()
        {
            RunOnMergeInsertUpdDeleteSingleKey(true);
            RunOnMergeInsertUpdDeleteSingleKey(false);
    
            RunOnMergeInsertUpdDeleteTwoKey(true);
            RunOnMergeInsertUpdDeleteTwoKey(false);
    
            RunOnMergeInsertUpdDeleteUngrouped(true);
        }
    
        public void RunOnMergeInsertUpdDeleteUngrouped(bool soda)
        {
            var eplDeclare = "create table varagg (p0 string, sumint sum(int))";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplDeclare);
    
            var fields = "c0,c1".Split(',');
            var eplRead = "select varagg.p0 as c0, varagg.sumint as c1, varagg as c2 from SupportBean_S0";
            EPStatement stmtRead = SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplRead);
            stmtRead.AddListener(_listener);
    
            // assert selected column types
            var expectedAggType = new object[][] { new object[] { "c0", typeof(string) }, new object[] { "c1", typeof(int?) } };
             SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedAggType, stmtRead.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
    
            // assert no row
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});
    
            // create merge
            var eplMerge = "on SupportBean merge varagg" +
                    " when not matched then" +
                    " insert select TheString as p0" +
                    " when matched and TheString like \"U%\" then" +
                    " update set p0=\"updated\"" +
                    " when matched and TheString like \"D%\" then" +
                    " delete";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplMerge);
    
            // merge for varagg
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
    
            // assert
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", null});
    
            // also aggregate-into the same key
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, "into table varagg select sum(50) as sumint from SupportBean_S1");
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"E1", 50});
    
            // update for varagg
            _epService.EPRuntime.SendEvent(new SupportBean("U2", 10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            var received = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(received, fields, new object[] {"updated", 50});
            EPAssertionUtil.AssertPropsMap((IDictionary<string, object>) received.Get("c2"), "p0,sumint".Split(','), new object[] {"updated", 50});
    
            // delete for varagg
            _epService.EPRuntime.SendEvent(new SupportBean("D3", 0));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {null, null});
    
            _epService.EPAdministrator.DestroyAllStatements();
        }
    
        public void RunOnMergeInsertUpdDeleteSingleKey(bool soda) {
            var fieldsTable = "key,p0,p1,p2,sumint".Split(',');
            var eplDeclare = "create table varagg (key int primary key, p0 string, p1 int, p2 int[], sumint sum(int))";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplDeclare);
    
            var fields = "c0,c1,c2,c3".Split(',');
            var eplRead = "select varagg[id].p0 as c0, varagg[id].p1 as c1, varagg[id].p2 as c2, varagg[id].sumint as c3 from SupportBean_S0";
            EPStatement stmtRead = SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplRead);
            stmtRead.AddListener(_listener);
    
            // assert selected column types
            var expectedAggType = new object[][] { new object[] { "c0", typeof(string) }, new object[] { "c1", typeof(int) }, new object[] { "c2", typeof(int[]) }, new object[] { "c3", typeof(int?) } };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedAggType, stmtRead.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
    
            // assert no row
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {null, null, null, null});
    
            // create merge
            var eplMerge = "on SupportBean merge varagg" +
                    " where IntPrimitive=key" +
                    " when not matched then" +
                    " insert select IntPrimitive as key, \"v1\" as p0, 1000 as p1, {1,2} as p2" +
                    " when matched and TheString like \"U%\" then" +
                    " update set p0=\"v2\", p1=2000, p2={3,4}" +
                    " when matched and TheString like \"D%\" then" +
                    " delete";
            EPStatement stmtMerge = SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplMerge);
            stmtMerge.AddListener(_listenerMerge);
    
            // merge for varagg[10]
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            EPAssertionUtil.AssertProps(_listenerMerge.AssertOneGetNewAndReset(), fieldsTable, new object[] {10, "v1", 1000, new int[] {1, 2}, null});
    
            // assert key "10"
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"v1", 1000, new int?[] {1, 2}, null});
    
            // also aggregate-into the same key
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, "into table varagg select sum(50) as sumint from SupportBean_S1 group by id");
            _epService.EPRuntime.SendEvent(new SupportBean_S1(10));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"v1", 1000, new int?[] {1, 2}, 50});
    
            // update for varagg[10]
            _epService.EPRuntime.SendEvent(new SupportBean("U2", 10));
            EPAssertionUtil.AssertProps(_listenerMerge.LastNewData[0], fieldsTable, new object[] {10, "v2", 2000, new int[] {3, 4}, 50});
            EPAssertionUtil.AssertProps(_listenerMerge.GetAndResetLastOldData()[0], fieldsTable, new object[] {10, "v1", 1000, new int[] {1, 2}, 50});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {"v2", 2000, new int?[] {3, 4}, 50});
    
            // delete for varagg[10]
            _epService.EPRuntime.SendEvent(new SupportBean("D3", 10));
            EPAssertionUtil.AssertProps(_listenerMerge.AssertOneGetOldAndReset(), fieldsTable, new object[] {10, "v2", 2000, new int[] {3, 4}, 50});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {null, null, null, null});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }
    
        public void RunOnMergeInsertUpdDeleteTwoKey(bool soda)
        {
            var eplDeclare = "create table varagg (KeyOne int primary key, KeyTwo string primary key, Prop string)";
            SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplDeclare);
    
            var fields = "c0,c1,c2".Split(',');
            var eplRead = "select varagg[id,p00].KeyOne as c0, varagg[id,p00].KeyTwo as c1, varagg[id,p00].Prop as c2 from SupportBean_S0";
            EPStatement stmtRead = SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplRead);
            stmtRead.AddListener(_listener);
    
            // assert selected column types
            var expectedAggType = new object[][] { new object[] { "c0", typeof(int) }, new object[] { "c1", typeof(string) }, new object[] { "c2", typeof(string) } };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedAggType, stmtRead.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
    
            // assert no row
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "A"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {null, null, null});
    
            // create merge
            var eplMerge = "on SupportBean merge varagg" +
                    " where IntPrimitive=KeyOne and TheString=KeyTwo" +
                    " when not matched then" +
                    " insert select IntPrimitive as KeyOne, TheString as KeyTwo, \"inserted\" as Prop" +
                    " when matched and LongPrimitive>0 then" +
                    " update set Prop=\"updated\"" +
                    " when matched and LongPrimitive<0 then" +
                    " delete";
            EPStatement stmtMerge = SupportModelHelper.CreateByCompileOrParse(_epService, soda, eplMerge);
            var expectedType = new object[][] { new object[] { "KeyOne", typeof(int) }, new object[] { "KeyTwo", typeof(string) }, new object[] { "Prop", typeof(string) } };
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(expectedType, stmtMerge.EventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
    
            // merge for varagg[10, "A"]
            _epService.EPRuntime.SendEvent(new SupportBean("A", 10));
    
            // assert key {"10", "A"}
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "A"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {10, "A", "inserted"});
    
            // update for varagg[10, "A"]
            _epService.EPRuntime.SendEvent(MakeSupportBean("A", 10, 1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "A"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[] {10, "A", "updated"});
    
            // test typable output
            _epService.EPAdministrator.Configuration.AddEventType(typeof(LocalBean));
            var stmtConvert = _epService.EPAdministrator.CreateEPL("insert into LocalBean select varagg[10, 'A'] as Val0 from SupportBean_S1");
            stmtConvert.AddListener(_listener);
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), "Val0.KeyOne".Split(','), new object[]{10});
    
            // delete for varagg[10, "A"]
            _epService.EPRuntime.SendEvent(MakeSupportBean("A", 10, -1));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "A"));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new object[]{null, null, null});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__internal", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("table_varagg__public", false);
        }
    
        private SupportBean MakeSupportBean(string theString, int intPrimitive, long longPrimitive)
        {
            var bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            return bean;
        }
    
        public class LocalSubBean
        {
            public int KeyOne { get; set; }

            public string KeyTwo { get; set; }

            public string Prop { get; set; }
        }
    
        public class LocalBean
        {
            public LocalSubBean Val0 { get; set; }
        }
    }
}
