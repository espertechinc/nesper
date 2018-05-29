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
using Avro.Generic;
using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;
using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableInsertInto : RegressionExecution {
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new Type[] {
                typeof(SupportBean),
                typeof(SupportBean_S0),
                typeof(SupportBean_S1),
                typeof(SupportBean_S2)
            }) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionInsertIntoSelfAccess(epService);
            RunAssertionNamedWindowMergeInsertIntoTable(epService);
            RunAssertionSplitStream(epService);
            RunAssertionInsertIntoFromNamedWindow(epService);
            RunAssertionInsertInto(epService);
            RunAssertionInsertIntoWildcard(epService);
        }
    
        private void RunAssertionInsertIntoSelfAccess(EPServiceProvider epService) {
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("create table MyTableIISA(pkey string primary key)");
            epService.EPAdministrator.CreateEPL("insert into MyTableIISA select TheString as pkey from SupportBean where MyTableIISA[TheString] is null");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey".Split(','), new object[][]{new object[] {"E1"}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey".Split(','), new object[][]{new object[] {"E1"}, new object[] {"E2"}});
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionNamedWindowMergeInsertIntoTable(EPServiceProvider epService) {
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("create table MyTableNWM(pkey string)");
            epService.EPAdministrator.CreateEPL("create window MyWindow#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("on SupportBean as sb merge MyWindow when not matched " +
                    "then insert into MyTableNWM select sb.TheString as pkey");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), "pkey".Split(','), new object[][]{new object[] {"E1"}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSplitStream(EPServiceProvider epService) {
            EPStatement stmtCreateOne = epService.EPAdministrator.CreateEPL(
                    "create table MyTableOne(pkey string primary key, col int)");
            EPStatement stmtCreateTwo = epService.EPAdministrator.CreateEPL(
                    "create table MyTableTwo(pkey string primary key, col int)");
    
            string eplSplit = "on SupportBean \n" +
                    "  insert into MyTableOne select TheString as pkey, IntPrimitive as col where IntPrimitive > 0\n" +
                    "  insert into MyTableTwo select TheString as pkey, IntPrimitive as col where IntPrimitive < 0\n" +
                    "  insert into OtherStream select TheString as pkey, IntPrimitive as col where IntPrimitive = 0\n";
            epService.EPAdministrator.CreateEPL(eplSplit);
    
            var otherStreamListener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from OtherStream").Events += otherStreamListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            AssertSplitStream(stmtCreateOne, stmtCreateTwo, new object[][]{new object[] {"E1", 1}}, new Object[0][]);
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", -2));
            AssertSplitStream(stmtCreateOne, stmtCreateTwo, new object[][]{new object[] {"E1", 1}}, new object[][] {new object[] {"E2", -2}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E3", -3));
            AssertSplitStream(stmtCreateOne, stmtCreateTwo, new object[][]{new object[] {"E1", 1}}, new object[][] {new object[] {"E2", -2}, new object[] {"E3", -3}});
            Assert.IsFalse(otherStreamListener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            AssertSplitStream(stmtCreateOne, stmtCreateTwo, new object[][]{new object[] {"E1", 1}}, new object[][] {new object[] {"E2", -2}, new object[] {"E3", -3}});
            EPAssertionUtil.AssertProps(otherStreamListener.AssertOneGetNewAndReset(), "pkey,col".Split(','), new object[]{"E4", 0});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertSplitStream(EPStatement stmtCreateOne, EPStatement stmtCreateTwo, object[][] tableOneRows, object[][] tableTwoRows) {
            string[] fields = "pkey,col".Split(',');
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreateOne.GetEnumerator(), fields, tableOneRows);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreateTwo.GetEnumerator(), fields, tableTwoRows);
        }
    
        private void RunAssertionInsertIntoFromNamedWindow(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create window MyWindow#unique(TheString) as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("create table MyTableIIF(pkey0 string primary key, pkey1 int primary key)");
            epService.EPAdministrator.CreateEPL("on SupportBean_S1 insert into MyTableIIF select TheString as pkey0, IntPrimitive as pkey1 from MyWindow");
            string[] fields = "pkey0,pkey1".Split(',');
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean_S1(1));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 10}});
    
            epService.EPRuntime.ExecuteQuery("delete from MyTableIIF");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
            epService.EPRuntime.SendEvent(new SupportBean_S1(2));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 10}, new object[] {"E2", 20}});
        }
    
        private void RunAssertionInsertInto(EPServiceProvider epService) {
            RunInsertIntoKeyed(epService);
    
            RunInsertIntoUnKeyed(epService);
        }
    
        private void RunInsertIntoUnKeyed(EPServiceProvider epService) {
            string[] fields = "TheString".Split(',');
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("create table MyTableIIU(TheString string)");
            epService.EPAdministrator.CreateEPL("@Name('tbl-insert') insert into MyTableIIU select TheString from SupportBean");
    
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new Object[0][]);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1"}});
    
            try {
                epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
                Assert.Fail();
            } catch (EPException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "com.espertech.esper.client.EPException: Unexpected exception in statement 'tbl-insert': Unique index violation, table 'MyTableIIU' is a declared to hold a single un-keyed row");
            }
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunInsertIntoKeyed(EPServiceProvider epService) {
            string[] fields = "pkey,thesum".Split(',');
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL("create table MyTableIIK(" +
                    "pkey string primary key," +
                    "thesum sum(int))");
            epService.EPAdministrator.CreateEPL("insert into MyTableIIK select TheString as pkey from SupportBean");
            epService.EPAdministrator.CreateEPL("into table MyTableIIK select sum(id) as thesum from SupportBean_S0 group by p00");
            epService.EPAdministrator.CreateEPL("on SupportBean_S1 insert into MyTableIIK select p10 as pkey");
            epService.EPAdministrator.CreateEPL("on SupportBean_S2 merge MyTableIIK where p20 = pkey when not matched then insert into MyTableIIK select p20 as pkey");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", null}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "E1"));
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 10}});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 10}, new object[] {"E2", null}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "E2"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(11, "E1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 21}, new object[] {"E2", 20}});
    
            // assert on-insert and on-merge
            epService.EPRuntime.SendEvent(new SupportBean_S1(0, "E3"));
            epService.EPRuntime.SendEvent(new SupportBean_S2(0, "E4"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(3, "E3"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(4, "E4"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 21}, new object[] {"E2", 20}, new object[] {"E3", 3}, new object[] {"E4", 4}});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_MyTableIIK__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_MyTableIIK__public", false);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionInsertIntoWildcard(EPServiceProvider epService) {
            TryAssertionWildcard(epService, true, null);
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionWildcard(epService, false, rep);
            }
        }
    
        private void TryAssertionWildcard(EPServiceProvider epService, bool bean, EventRepresentationChoice? rep) {
            if (bean) {
                epService.EPAdministrator.CreateEPL("create schema MySchema as " + typeof(MyP0P1Event).FullName);
            } else {
                epService.EPAdministrator.CreateEPL("create " + rep.Value.GetOutputTypeCreateSchemaName() + " schema MySchema (p0 string, p1 string)");
            }
    
            EPStatement stmtTheTable = epService.EPAdministrator.CreateEPL("create table TheTable (p0 string, p1 string)");
            epService.EPAdministrator.CreateEPL("insert into TheTable select * from MySchema");
    
            if (bean) {
                epService.EPRuntime.SendEvent(new MyP0P1Event("a", "b"));
            } else if (rep.Value.IsMapEvent()) {
                var map = new Dictionary<string, Object>();
                map.Put("p0", "a");
                map.Put("p1", "b");
                epService.EPRuntime.SendEvent(map, "MySchema");
            } else if (rep.Value.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{"a", "b"}, "MySchema");
            } else if (rep.Value.IsAvroEvent()) {
                var theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "MySchema").AsRecordSchema());
                theEvent.Put("p0", "a");
                theEvent.Put("p1", "b");
                epService.EPRuntime.SendEventAvro(theEvent, "MySchema");
            }
            EPAssertionUtil.AssertProps(stmtTheTable.First(), "p0,p1".Split(','), new object[]{"a", "b"});
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MySchema", false);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    }

    public class MyP0P1Event
    {
        private readonly string _p0;
        private readonly string _p1;

        public MyP0P1Event(string p0, string p1)
        {
            _p0 = p0;
            _p1 = p1;
        }

        [PropertyName("p0")]
        public string P0 => _p0;
        [PropertyName("p1")]
        public string P1 => _p1;
    }
} // end of namespace
