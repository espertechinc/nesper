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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableInsertInto
    {
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            foreach (Type clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1), typeof(SupportBean_S2)}) {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
        }
    
        [Test]
        public void TestInsertIntoSelfAccess()
        {
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL("create table MyTable(pkey string primary key)");
            _epService.EPAdministrator.CreateEPL("insert into MyTable select TheString as pkey from SupportBean where MyTable[TheString] is null");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey".Split(','), new object[][] { new object[] { "E1" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey".Split(','), new object[][] { new object[] { "E1" }, new object[] { "E2" } });
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
        }
    
        [Test]
        public void TestNamedWindowMergeInsertIntoTable()
        {
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL("create table MyTable(pkey string)");
            _epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            _epService.EPAdministrator.CreateEPL("on SupportBean as sb merge MyWindow when not matched " +
                    "then insert into MyTable select sb.TheString as pkey");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey".Split(','), new object[][] { new object[] { "E1" } });
        }
    
        [Test]
        public void TestSplitStream()
        {
            EPStatement stmtCreateOne = _epService.EPAdministrator.CreateEPL(
                    "create table MyTableOne(pkey string primary key, col int)");
            EPStatement stmtCreateTwo = _epService.EPAdministrator.CreateEPL(
                    "create table MyTableTwo(pkey string primary key, col int)");
    
            string eplSplit = "on SupportBean \n" +
                    "  insert into MyTableOne select TheString as pkey, IntPrimitive as col where IntPrimitive > 0\n" +
                    "  insert into MyTableTwo select TheString as pkey, IntPrimitive as col where IntPrimitive < 0\n" +
                    "  insert into OtherStream select TheString as pkey, IntPrimitive as col where IntPrimitive = 0\n";
            _epService.EPAdministrator.CreateEPL(eplSplit);
    
            SupportUpdateListener otherStreamListener = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("select * from OtherStream").AddListener(otherStreamListener);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            AssertSplitStream(stmtCreateOne, stmtCreateTwo, new object[][] { new object[] { "E1", 1 } }, new object[0][]);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", -2));
            AssertSplitStream(stmtCreateOne, stmtCreateTwo, new object[][] { new object[] { "E1", 1 } }, new object[][] { new object[] { "E2", -2 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", -3));
            AssertSplitStream(stmtCreateOne, stmtCreateTwo, new object[][] { new object[] { "E1", 1 } }, new object[][] { new object[] { "E2", -2 }, new object[] { "E3", -3 } });
            Assert.IsFalse(otherStreamListener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            AssertSplitStream(stmtCreateOne, stmtCreateTwo, new object[][] { new object[] { "E1", 1 } }, new object[][] { new object[] { "E2", -2 }, new object[] { "E3", -3 } });
            EPAssertionUtil.AssertProps(otherStreamListener.AssertOneGetNewAndReset(), "pkey,col".Split(','), new object[]{"E4", 0});
        }
    
        private void AssertSplitStream(EPStatement stmtCreateOne, EPStatement stmtCreateTwo, object[][] tableOneRows, object[][] tableTwoRows)
        {
            string[] fields = "pkey,col".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreateOne.GetEnumerator(), fields, tableOneRows);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreateTwo.GetEnumerator(), fields, tableTwoRows);
        }
    
        [Test]
        public void TestInsertIntoFromNamedWindow()
        {
            _epService.EPAdministrator.CreateEPL("create window MyWindow#unique(TheString) as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL("create table MyTable(pkey0 string primary key, pkey1 int primary key)");
            _epService.EPAdministrator.CreateEPL("on SupportBean_S1 insert into MyTable select TheString as pkey0, IntPrimitive as pkey1 from MyWindow");
            string[] fields = "pkey0,pkey1".Split(',');
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 10 } });
    
            _epService.EPRuntime.ExecuteQuery("delete from MyTable");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            _epService.EPRuntime.SendEvent(new SupportBean_S1(2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 10 }, new object[] { "E2", 20 } });
        }
    
        [Test]
        public void TestInsertInto()
        {
            RunInsertIntoKeyed();
            RunInsertIntoUnKeyed();
        }
    
        private void RunInsertIntoUnKeyed()
        {
            string[] fields = "TheString".Split(',');
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL("create table MyTable(TheString string)");
            _epService.EPAdministrator.CreateEPL("@Name('tbl-insert') insert into MyTable select TheString from SupportBean");
    
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[0][]);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1" } });
    
            try {
                _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
                Assert.Fail();
            }
            catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "com.espertech.esper.client.EPException: Unexpected exception in statement 'tbl-insert': Unique index violation, table 'MyTable' is a declared to hold a single un-keyed row");
            }
        }
    
        private void RunInsertIntoKeyed()
        {
            string[] fields = "pkey,thesum".Split(',');
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL("create table MyTable(" +
                    "pkey string primary key," +
                    "thesum sum(int))");
            _epService.EPAdministrator.CreateEPL("insert into MyTable select TheString as pkey from SupportBean");
            _epService.EPAdministrator.CreateEPL("into table MyTable select sum(id) as thesum from SupportBean_S0 group by p00");
            _epService.EPAdministrator.CreateEPL("on SupportBean_S1 insert into MyTable select p10 as pkey");
            _epService.EPAdministrator.CreateEPL("on SupportBean_S2 merge MyTable where p20 = pkey when not matched then insert into MyTable select p20 as pkey");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", null } });
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1"));
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 10 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 10 }, new object[] { "E2", null } });
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(20, "E2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 21 }, new object[] { "E2", 20 } });
    
            // assert on-insert and on-merge
            _epService.EPRuntime.SendEvent(new SupportBean_S1(0, "E3"));
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0, "E4"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(3, "E3"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(4, "E4"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 21 }, new object[] { "E2", 20 }, new object[] { "E3", 3 }, new object[] { "E4", 4 } });
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("table_MyTable__internal", false);
            _epService.EPAdministrator.Configuration.RemoveEventType("table_MyTable__public", false);
        }
    
        [Test]
        public void TestInsertIntoWildcard()
        {
            RunAssertionWildcard(true, null);
            EnumHelper.ForEach<EventRepresentationChoice>(rep => RunAssertionWildcard(false, rep));
        }
    
        private void RunAssertionWildcard(bool bean, EventRepresentationChoice? rep)
        {
            if (bean) {
                _epService.EPAdministrator.CreateEPL("create schema MySchema as " + typeof(MyP0P1Event).MaskTypeName());
            }
            else {
                _epService.EPAdministrator.CreateEPL("create " + rep.Value.GetOutputTypeCreateSchemaName() + " schema MySchema (P0 string, P1 string)");
            }
    
            EPStatement stmtTheTable = _epService.EPAdministrator.CreateEPL("create table TheTable (P0 string, P1 string)");
            _epService.EPAdministrator.CreateEPL("insert into TheTable select * from MySchema");
    
            if (bean) {
                _epService.EPRuntime.SendEvent(new MyP0P1Event("a", "b"));
            }
            else if (rep.Value.IsMapEvent()) {
                IDictionary<String, object> map = new Dictionary<string, object>();
                map.Put("P0", "a");
                map.Put("P1", "b");
                _epService.EPRuntime.SendEvent(map, "MySchema");
            } else if (rep.Value.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(
                    new object[] { "a", "b" }, "MySchema");
            } else if (rep.Value.IsAvroEvent()) {
                var theEvent = SupportAvroUtil.GetAvroRecord(_epService, "MySchema");
                theEvent.Put("P0", "a");
                theEvent.Put("P1", "b");
                _epService.EPRuntime.SendEventAvro(theEvent, "MySchema");
            }
            else
            {
            }
            EPAssertionUtil.AssertProps(stmtTheTable.First(), "P0,P1".Split(','), new object[] {"a", "b"});
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MySchema", false);
        }

        internal class MyP0P1Event
        {
            internal MyP0P1Event(string p0, string p1) {
                this.P0 = p0;
                this.P1 = p1;
            }

            public string P0 { get; private set; }

            public string P1 { get; private set; }
        }
    }
}
