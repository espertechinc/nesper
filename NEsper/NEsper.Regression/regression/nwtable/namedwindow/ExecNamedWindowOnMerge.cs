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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.bean.bookexample;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util;
using NEsper.Avro.Extensions;
using NEsper.Avro.Util.Support;
using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.namedwindow
{
    public class ExecNamedWindowOnMerge : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionUpdateNonPropertySet(epService);
            RunAssertionMergeTriggeredByAnotherWindow(epService);
            RunAssertionDocExample(epService);
            RunAssertionTypeReference(epService);
            RunAssertionPropertyEval(epService);
            RunAssertionPropertyInsertBean(epService);
            RunAssertionSubselect(epService);
        }
    
        private void RunAssertionUpdateNonPropertySet(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("increaseIntCopyDouble", GetType(), "IncreaseIntCopyDouble");
            epService.EPAdministrator.CreateEPL("create window MyWindowUNP#keepall as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into MyWindowUNP select * from SupportBean");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("on SupportBean_S0 as sb " +
                    "merge MyWindowUNP as mywin when matched then " +
                    "update set mywin.DoublePrimitive = id, IncreaseIntCopyDouble(initial, mywin)");
            var mergeListener = new SupportUpdateListener();
            stmt.Events += mergeListener.Update;
            string[] fields = "IntPrimitive,DoublePrimitive,DoubleBoxed".Split(',');
    
            epService.EPRuntime.SendEvent(MakeSupportBean("E1", 10, 2));
            epService.EPRuntime.SendEvent(new SupportBean_S0(5, "E1"));
            EPAssertionUtil.AssertProps(mergeListener.GetAndResetLastNewData()[0], fields, new object[]{11, 5d, 5d});
    
            // try a case-statement
            string eplCase = "on SupportBean_S0 merge MyWindowUNP " +
                    "when matched then update set TheString = " +
                    "case IntPrimitive when 1 then 'a' else 'b' end";
            epService.EPAdministrator.CreateEPL(eplCase);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionMergeTriggeredByAnotherWindow(EPServiceProvider epService) {
    
            // test dispatch between named windows
            epService.EPAdministrator.CreateEPL("@Name('A') create window A#unique(id) as (id int)");
            epService.EPAdministrator.CreateEPL("@Name('B') create window B#unique(id) as (id int)");
            epService.EPAdministrator.CreateEPL("@Name('C') on A merge B when not matched then insert select 1 as id when matched then insert select 1 as id");
    
            var nwListener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('D') select * from B").Events += nwListener.Update;
            epService.EPAdministrator.CreateEPL("@Name('E') insert into A select IntPrimitive as id FROM SupportBean");
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(nwListener.IsInvoked);
            epService.EPAdministrator.DestroyAllStatements();
    
            // test insert-stream only, no remove stream
            string[] fields = "c0,c1".Split(',');
            epService.EPAdministrator.CreateEPL("create window W1#lastevent as SupportBean");
            epService.EPAdministrator.CreateEPL("insert into W1 select * from SupportBean");
            epService.EPAdministrator.CreateEPL("create window W2#lastevent as SupportBean");
            epService.EPAdministrator.CreateEPL("on W1 as a merge W2 as b when not matched then insert into OutStream " +
                    "select a.TheString as c0, istream() as c1");
            var mergeListener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from OutStream").Events += mergeListener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"E1", true});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(mergeListener.AssertOneGetNewAndReset(), fields, new object[]{"E2", true});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionDocExample(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionDocExample(epService, rep);
            }
        }
    
        private void TryAssertionDocExample(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
    
            string baseModule = eventRepresentationEnum.GetAnnotationText() + " create schema OrderEvent as (orderId string, productId string, price double, quantity int, deletedFlag bool)";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(baseModule, null, null, null);
    
            string appModuleOne = eventRepresentationEnum.GetAnnotationText() + " create schema ProductTotalRec as (productId string, totalPrice double);" +
                    "" +
                    eventRepresentationEnum.GetAnnotationText() + " @Name('nwProd') create window ProductWindow#unique(productId) as ProductTotalRec;" +
                    "" +
                    "on OrderEvent oe\n" +
                    "merge ProductWindow pw\n" +
                    "where pw.productId = oe.productId\n" +
                    "when matched\n" +
                    "then update set totalPrice = totalPrice + oe.price\n" +
                    "when not matched\n" +
                    "then insert select productId, price as totalPrice;";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(appModuleOne, null, null, null);
    
            string appModuleTwo = eventRepresentationEnum.GetAnnotationText() + " @Name('nwOrd') create window OrderWindow#keepall as OrderEvent;" +
                    "" +
                    "on OrderEvent oe\n" +
                    "  merge OrderWindow pw\n" +
                    "  where pw.orderId = oe.orderId\n" +
                    "  when not matched \n" +
                    "    then insert select *\n" +
                    "  when matched and oe.deletedFlag=true\n" +
                    "    then delete\n" +
                    "  when matched\n" +
                    "    then update set pw.quantity = oe.quantity, pw.price = oe.price";
    
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(appModuleTwo, null, null, null);
    
            SendOrderEvent(epService, eventRepresentationEnum, "O1", "P1", 10, 100, false);
            SendOrderEvent(epService, eventRepresentationEnum, "O1", "P1", 11, 200, false);
            SendOrderEvent(epService, eventRepresentationEnum, "O2", "P2", 3, 300, false);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(epService.EPAdministrator.GetStatement("nwProd").GetEnumerator(), "productId,totalPrice".Split(','), new object[][]{new object[] {"P1", 21d}, new object[] {"P2", 3d}});
            EPAssertionUtil.AssertPropsPerRowAnyOrder(epService.EPAdministrator.GetStatement("nwOrd").GetEnumerator(), "orderId,quantity".Split(','), new object[][]{new object[] {"O1", 200}, new object[] {"O2", 300}});
    
            string module = "create schema StreetCarCountSchema (streetid string, carcount int);" +
                    "    create schema StreetChangeEvent (streetid string, action string);" +
                    "    create window StreetCarCountWindow#unique(streetid) as StreetCarCountSchema;" +
                    "    on StreetChangeEvent ce merge StreetCarCountWindow w where ce.streetid = w.streetid\n" +
                    "    when not matched and ce.action = 'ENTER' then insert select streetid, 1 as carcount\n" +
                    "    when matched and ce.action = 'ENTER' then update set StreetCarCountWindow.carcount = carcount + 1\n" +
                    "    when matched and ce.action = 'LEAVE' then update set StreetCarCountWindow.carcount = carcount - 1;" +
                    "    select * from StreetCarCountWindow;";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(module, null, null, null);
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "OrderEvent,OrderWindow,StreetCarCountSchema,StreetCarCountWindow,StreetChangeEvent,ProductWindow,ProductTotalRec".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void SendOrderEvent(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string orderId, string productId, double price, int quantity, bool deletedFlag) {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{orderId, productId, price, quantity, deletedFlag}, "OrderEvent");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new LinkedHashMap<string, Object>();
                theEvent.Put("orderId", orderId);
                theEvent.Put("productId", productId);
                theEvent.Put("price", price);
                theEvent.Put("quantity", quantity);
                theEvent.Put("deletedFlag", deletedFlag);
                epService.EPRuntime.SendEvent(theEvent, "OrderEvent");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "OrderEvent").AsRecordSchema());
                theEvent.Put("orderId", orderId);
                theEvent.Put("productId", productId);
                theEvent.Put("price", price);
                theEvent.Put("quantity", quantity);
                theEvent.Put("deletedFlag", deletedFlag);
                epService.EPRuntime.SendEventAvro(theEvent, "OrderEvent");
            } else {
                Assert.Fail();
            }
        }
    
        private void RunAssertionTypeReference(EPServiceProvider epService) {
            ConfigurationOperations configOps = epService.EPAdministrator.Configuration;
    
            epService.EPAdministrator.CreateEPL("@Name('ces') create schema EventSchema(in1 string, in2 int)");
            epService.EPAdministrator.CreateEPL("@Name('cnws') create schema WindowSchema(in1 string, in2 int)");
    
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"ces"}, configOps.GetEventTypeNameUsedBy("EventSchema").ToArray());
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"cnws"}, configOps.GetEventTypeNameUsedBy("WindowSchema").ToArray());
    
            epService.EPAdministrator.CreateEPL("@Name('cnw') create window MyWindowATR#keepall as WindowSchema");
            EPAssertionUtil.AssertEqualsAnyOrder("cnws,cnw".Split(','), configOps.GetEventTypeNameUsedBy("WindowSchema").ToArray());
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"cnw"}, configOps.GetEventTypeNameUsedBy("MyWindowATR").ToArray());
    
            epService.EPAdministrator.CreateEPL("@Name('om') on EventSchema merge into MyWindowATR " +
                    "when not matched then insert select in1, in2");
            EPAssertionUtil.AssertEqualsAnyOrder("ces,om".Split(','), configOps.GetEventTypeNameUsedBy("EventSchema").ToArray());
            EPAssertionUtil.AssertEqualsAnyOrder("cnws,cnw".Split(','), configOps.GetEventTypeNameUsedBy("WindowSchema").ToArray());
        }
    
        private void RunAssertionPropertyEval(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType("OrderBean", typeof(OrderBean));
    
            string[] fields = "c1,c2".Split(',');
            epService.EPAdministrator.CreateEPL("create window MyWindowPE#keepall as (c1 string, c2 string)");
    
            string epl = "on OrderBean[books] " +
                    "merge MyWindowPE mw " +
                    "when not matched then " +
                    "insert select bookId as c1, title as c2 ";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            var mergeListener = new SupportUpdateListener();
            stmt.Events += mergeListener.Update;
    
            epService.EPRuntime.SendEvent(OrderBeanFactory.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(mergeListener.LastNewData, fields, new object[][]{
                new object[] {"10020", "Enders Game"},
                new object[] {"10021", "Foundation 1"},
                new object[] {"10022", "Stranger in a Strange Land"}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPropertyInsertBean(EPServiceProvider epService) {
            EPStatement stmtWindow = epService.EPAdministrator.CreateEPL("create window MergeWindow#unique(TheString) as SupportBean");
    
            string epl = "on SupportBean as up merge MergeWindow as mv where mv.TheString=up.TheString when not matched then insert select IntPrimitive";
            EPStatement stmtMerge = epService.EPAdministrator.CreateEPL(epl);
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
    
            EventBean theEvent = stmtWindow.First();
            EPAssertionUtil.AssertProps(theEvent, "TheString,IntPrimitive".Split(','), new object[]{null, 10});
            stmtMerge.Dispose();
    
            epl = "on SupportBean as up merge MergeWindow as mv where mv.TheString=up.TheString when not matched then insert select TheString, IntPrimitive";
            epService.EPAdministrator.CreateEPL(epl);
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
    
            EPAssertionUtil.AssertPropsPerRow(stmtWindow.GetEnumerator(), "TheString,IntPrimitive".Split(','), new object[][]{new object[] {null, 10}, new object[] {"E2", 20}});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSubselect(EPServiceProvider epService) {
            foreach (EventRepresentationChoice rep in EnumHelper.GetValues<EventRepresentationChoice>()) {
                TryAssertionSubselect(epService, rep);
            }
        }
    
        private void TryAssertionSubselect(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum) {
            string[] fields = "col1,col2".Split(',');
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEvent as (in1 string, in2 int)");
            epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MySchema as (col1 string, col2 int)");
            EPStatement namedWindowStmt = epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window MyWindowSS#lastevent as MySchema");
            epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyWindowSS");
    
            string epl = "on MyEvent me " +
                    "merge MyWindowSS mw " +
                    "when not matched and (select IntPrimitive>0 from SupportBean(TheString like 'A%')#lastevent) then " +
                    "insert(col1, col2) select (select TheString from SupportBean(TheString like 'A%')#lastevent), (select IntPrimitive from SupportBean(TheString like 'A%')#lastevent) " +
                    "when matched and (select IntPrimitive>0 from SupportBean(TheString like 'B%')#lastevent) then " +
                    "update set col1=(select TheString from SupportBean(TheString like 'B%')#lastevent), col2=(select IntPrimitive from SupportBean(TheString like 'B%')#lastevent) " +
                    "when matched and (select IntPrimitive>0 from SupportBean(TheString like 'C%')#lastevent) then " +
                    "delete";
            epService.EPAdministrator.CreateEPL(epl);
    
            // no action tests
            SendMyEvent(epService, eventRepresentationEnum, "X1", 1);
            epService.EPRuntime.SendEvent(new SupportBean("A1", 0));   // ignored
            SendMyEvent(epService, eventRepresentationEnum, "X2", 2);
            epService.EPRuntime.SendEvent(new SupportBean("A2", 20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, null);
    
            SendMyEvent(epService, eventRepresentationEnum, "X3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"A2", 20}});
    
            epService.EPRuntime.SendEvent(new SupportBean_A("Y1"));
            epService.EPRuntime.SendEvent(new SupportBean("A3", 30));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, null);
    
            SendMyEvent(epService, eventRepresentationEnum, "X4", 4);
            epService.EPRuntime.SendEvent(new SupportBean("A4", 40));
            SendMyEvent(epService, eventRepresentationEnum, "X5", 5);   // ignored as matched (no where clause, no B event)
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"A3", 30}});
    
            epService.EPRuntime.SendEvent(new SupportBean("B1", 50));
            SendMyEvent(epService, eventRepresentationEnum, "X6", 6);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"B1", 50}});
    
            epService.EPRuntime.SendEvent(new SupportBean("B2", 60));
            SendMyEvent(epService, eventRepresentationEnum, "X7", 7);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"B2", 60}});
    
            epService.EPRuntime.SendEvent(new SupportBean("B2", 0));
            SendMyEvent(epService, eventRepresentationEnum, "X8", 8);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"B2", 60}});
    
            epService.EPRuntime.SendEvent(new SupportBean("C1", 1));
            SendMyEvent(epService, eventRepresentationEnum, "X9", 9);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("C1", 0));
            SendMyEvent(epService, eventRepresentationEnum, "X10", 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[] {"A4", 40}});
    
            epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "MyEvent,MySchema,MyWindowSS".Split(',')) {
                epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private SupportBean MakeSupportBean(string theString, int intPrimitive, double doublePrimitive) {
            var sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    
        private void SendMyEvent(EPServiceProvider epService, EventRepresentationChoice eventRepresentationEnum, string in1, int in2) {
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                epService.EPRuntime.SendEvent(new object[]{in1, in2}, "MyEvent");
            } else if (eventRepresentationEnum.IsMapEvent()) {
                var theEvent = new LinkedHashMap<string, object>();
                theEvent.Put("in1", in1);
                theEvent.Put("in2", in2);
                epService.EPRuntime.SendEvent(theEvent, "MyEvent");
            } else if (eventRepresentationEnum.IsAvroEvent()) {
                var theEvent = new GenericRecord(SupportAvroUtil.GetAvroSchema(epService, "MyEvent").AsRecordSchema());
                theEvent.Put("in1", in1);
                theEvent.Put("in2", in2);
                epService.EPRuntime.SendEventAvro(theEvent, "MyEvent");
            } else {
                Assert.Fail();
            }
        }
    
        public static void IncreaseIntCopyDouble(SupportBean initialBean, SupportBean updatedBean) {
            updatedBean.IntPrimitive = initialBean.IntPrimitive + 1;
            updatedBean.DoubleBoxed = updatedBean.DoublePrimitive;
        }
    }
} // end of namespace
