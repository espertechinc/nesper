///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.regression.view;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.bean.bookexample;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestNamedWindowOnMerge
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _nwListener;
        private SupportUpdateListener _mergeListener;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("SupportBean", typeof(SupportBean));
            config.AddEventType("SupportBean_A", typeof(SupportBean_A));
            config.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
    
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
            _nwListener = new SupportUpdateListener();
            _mergeListener = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _nwListener = null;
            _mergeListener = null;
        }
    
        [Test]
        public void TestUpdateNonPropertySet() {
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("increaseIntCopyDouble", this.GetType().FullName, "IncreaseIntCopyDouble");
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("on SupportBean_S0 as sb " +
                    "merge MyWindow as mywin when matched then " +
                    "update set mywin.set_DoublePrimitive(id), increaseIntCopyDouble(initial, mywin)");
            stmt.AddListener(_mergeListener);
            string[] fields = "IntPrimitive,DoublePrimitive,DoubleBoxed".Split(',');
    
            _epService.EPRuntime.SendEvent(MakeSupportBean("E1", 10, 2));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(5, "E1"));
            EPAssertionUtil.AssertProps(_mergeListener.GetAndResetLastNewData()[0], fields, new object[]{11, 5d, 5d});
    
            // try a case-statement
            string eplCase = "on SupportBean_S0 merge MyWindow " +
                    "when matched then update set TheString = " +
                    "case IntPrimitive when 1 then 'a' else 'b' end";
            _epService.EPAdministrator.CreateEPL(eplCase);
        }
    
        [Test]
        public void TestMergeTriggeredByAnotherWindow() {
    
            // test dispatch between named windows
            _epService.EPAdministrator.CreateEPL("@Name('A') create window A.std:unique(id) as (id int)");
            _epService.EPAdministrator.CreateEPL("@Name('B') create window B.std:unique(id) as (id int)");
            _epService.EPAdministrator.CreateEPL("@Name('C') on A merge B when not matched then insert select 1 as id when matched then insert select 1 as id");
    
            _epService.EPAdministrator.CreateEPL("@Name('D') select * from B").AddListener(_nwListener);
            _epService.EPAdministrator.CreateEPL("@Name('E') insert into A select IntPrimitive as id FROM SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsTrue(_nwListener.IsInvoked);
            _epService.EPAdministrator.DestroyAllStatements();
    
            // test insert-stream only, no remove stream
            string[] fields = "c0,c1".Split(',');
            _epService.EPAdministrator.CreateEPL("create window W1.std:lastevent() as SupportBean");
            _epService.EPAdministrator.CreateEPL("insert into W1 select * from SupportBean");
            _epService.EPAdministrator.CreateEPL("create window W2.std:lastevent() as SupportBean");
            _epService.EPAdministrator.CreateEPL("on W1 as a merge W2 as b when not matched then insert into OutStream " +
                    "select a.TheString as c0, istream() as c1");
            _epService.EPAdministrator.CreateEPL("select * from OutStream").AddListener(_mergeListener);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), fields, new object[] {"E1", true});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_mergeListener.AssertOneGetNewAndReset(), fields, new object[] {"E2", true});
        }
    
        [Test]
        public void TestDocExample() {
            RunAssertionDocExample(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionDocExample(EventRepresentationEnum.MAP);
            RunAssertionDocExample(EventRepresentationEnum.DEFAULT);
        }
    
        public void RunAssertionDocExample(EventRepresentationEnum eventRepresentationEnum) {
    
            string baseModule = eventRepresentationEnum.GetAnnotationText() + " create schema OrderEvent as (orderId string, productId string, price double, quantity int, deletedFlag boolean)";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(baseModule, null, null, null);
    
            string appModuleOne = eventRepresentationEnum.GetAnnotationText() + " create schema ProductTotalRec as (productId string, totalPrice double);" +
                    "" +
                    eventRepresentationEnum.GetAnnotationText() + " @Name('nwProd') create window ProductWindow.std:unique(productId) as ProductTotalRec;" +
                    "" +
                    "on OrderEvent oe\n" +
                    "merge ProductWindow pw\n" +
                    "where pw.productId = oe.productId\n" +
                    "when matched\n" +
                    "then update set totalPrice = totalPrice + oe.price\n" +
                    "when not matched\n" +
                    "then insert select productId, price as totalPrice;";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(appModuleOne, null, null, null);
    
            string appModuleTwo = eventRepresentationEnum.GetAnnotationText() + " @Name('nwOrd') create window OrderWindow.win:keepall() as OrderEvent;" +
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
    
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(appModuleTwo, null, null, null);
    
            SendOrderEvent(eventRepresentationEnum, "O1", "P1", 10, 100, false);
            SendOrderEvent(eventRepresentationEnum, "O1", "P1", 11, 200, false);
            SendOrderEvent(eventRepresentationEnum, "O2", "P2", 3, 300, false);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_epService.EPAdministrator.GetStatement("nwProd").GetEnumerator(), "productId,totalPrice".Split(','), new object[][] { new object[] { "P1", 21d }, new object[] { "P2", 3d } });
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_epService.EPAdministrator.GetStatement("nwOrd").GetEnumerator(), "orderId,quantity".Split(','), new object[][] { new object[] { "O1", 200 }, new object[] { "O2", 300 } });
    
            string module = "create schema StreetCarCountSchema (streetid string, carcount int);" +
                    "    create schema StreetChangeEvent (streetid string, action string);" +
                    "    create window StreetCarCountWindow.std:unique(streetid) as StreetCarCountSchema;" +
                    "    on StreetChangeEvent ce merge StreetCarCountWindow w where ce.streetid = w.streetid\n" +
                    "    when not matched and ce.action = 'ENTER' then insert select streetid, 1 as carcount\n" +
                    "    when matched and ce.action = 'ENTER' then update set StreetCarCountWindow.carcount = carcount + 1\n" +
                    "    when matched and ce.action = 'LEAVE' then update set StreetCarCountWindow.carcount = carcount - 1;" +
                    "    select * from StreetCarCountWindow;";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(module, null, null, null);
    
            _epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "OrderEvent,OrderWindow,StreetCarCountSchema,StreetCarCountWindow,StreetChangeEvent,ProductWindow,ProductTotalRec".Split(',')) {
                _epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private void SendOrderEvent(EventRepresentationEnum eventRepresentationEnum, string orderId, string productId, double price, int quantity, bool deletedFlag) {
            IDictionary<String, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("orderId", orderId);
            theEvent.Put("productId", productId);
            theEvent.Put("price", price);
            theEvent.Put("quantity", quantity);
            theEvent.Put("deletedFlag", deletedFlag);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "OrderEvent");
            }
            else {
                _epService.EPRuntime.SendEvent(theEvent, "OrderEvent");
            }
        }
    
        [Test]
        public void TestTypeReference() {
            ConfigurationOperations configOps = _epService.EPAdministrator.Configuration;
    
            _epService.EPAdministrator.CreateEPL("@Name('ces') create schema EventSchema(in1 string, in2 int)");
            _epService.EPAdministrator.CreateEPL("@Name('cnws') create schema WindowSchema(in1 string, in2 int)");
    
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"ces"}, configOps.GetEventTypeNameUsedBy("EventSchema").ToArray());
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"cnws"}, configOps.GetEventTypeNameUsedBy("WindowSchema").ToArray());
    
            _epService.EPAdministrator.CreateEPL("@Name('cnw') create window MyWindow.win:keepall() as WindowSchema");
            EPAssertionUtil.AssertEqualsAnyOrder("cnws,cnw".Split(','), configOps.GetEventTypeNameUsedBy("WindowSchema").ToArray());
            EPAssertionUtil.AssertEqualsAnyOrder(new string[]{"cnw"}, configOps.GetEventTypeNameUsedBy("MyWindow").ToArray());
    
            _epService.EPAdministrator.CreateEPL("@Name('om') on EventSchema merge into MyWindow " +
                    "when not matched then insert select in1, in2");
            EPAssertionUtil.AssertEqualsAnyOrder("ces,om".Split(','), configOps.GetEventTypeNameUsedBy("EventSchema").ToArray());
            EPAssertionUtil.AssertEqualsAnyOrder("cnws,cnw".Split(','), configOps.GetEventTypeNameUsedBy("WindowSchema").ToArray());
        }
    
        [Test]
        public void TestPropertyEval() {
            _epService.EPAdministrator.Configuration.AddEventType("OrderBean", typeof(OrderBean));
    
            string[] fields = "c1,c2".Split(',');
            _epService.EPAdministrator.CreateEPL("create window MyWindow.win:keepall() as (c1 string, c2 string)");
    
            string epl =  "on OrderBean[books] " +
                          "merge MyWindow mw " +
                          "when not matched then " +
                          "insert select bookId as c1, title as c2 ";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_mergeListener);

            _epService.EPRuntime.SendEvent(TestContainedEventSimple.MakeEventOne());
            EPAssertionUtil.AssertPropsPerRow(_mergeListener.LastNewData, fields, new object[][]{
                new object[]{"10020", "Enders Game"},
                new object[]{"10021", "Foundation 1"},
                new object[]{"10022", "Stranger in a Strange Land"}});
        }
    
        [Test]
        public void TestPropertyInsertBean() {
            EPStatement stmtWindow = _epService.EPAdministrator.CreateEPL("create window MergeWindow.std:unique(TheString) as SupportBean");
    
            string epl = "on SupportBean as up merge MergeWindow as mv where mv.TheString=up.TheString when not matched then insert select IntPrimitive";
            EPStatement stmtMerge = _epService.EPAdministrator.CreateEPL(epl);
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
    
            EventBean theEvent = stmtWindow.First();
            EPAssertionUtil.AssertProps(theEvent, "TheString,IntPrimitive".Split(','), new object[]{null, 10});
            stmtMerge.Dispose();
    
            epl = "on SupportBean as up merge MergeWindow as mv where mv.TheString=up.TheString when not matched then insert select TheString, IntPrimitive";
            _epService.EPAdministrator.CreateEPL(epl);
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));

            EPAssertionUtil.AssertPropsPerRow(stmtWindow.GetEnumerator(), "TheString,IntPrimitive".Split(','), new object[][] { new object[] { null, 10 }, new object[] { "E2", 20 } });
        }
    
        [Test]
        public void TestSubselect() {
            RunAssertionSubselect(EventRepresentationEnum.OBJECTARRAY);
            RunAssertionSubselect(EventRepresentationEnum.MAP);
            RunAssertionSubselect(EventRepresentationEnum.DEFAULT);
        }
    
        private void RunAssertionSubselect(EventRepresentationEnum eventRepresentationEnum) {
            string[] fields = "col1,col2".Split(',');
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MyEvent as (in1 string, in2 int)");
            _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create schema MySchema as (col1 string, col2 int)");
            EPStatement namedWindowStmt = _epService.EPAdministrator.CreateEPL(eventRepresentationEnum.GetAnnotationText() + " create window MyWindow.std:lastevent() as MySchema");
            _epService.EPAdministrator.CreateEPL("on SupportBean_A delete from MyWindow");
    
            string epl =  "on MyEvent me " +
                          "merge MyWindow mw " +
                          "when not matched and (select IntPrimitive>0 from SupportBean(TheString like 'A%').std:lastevent()) then " +
                          "insert(col1, col2) select (select TheString from SupportBean(TheString like 'A%').std:lastevent()), (select IntPrimitive from SupportBean(TheString like 'A%').std:lastevent()) " +
                          "when matched and (select IntPrimitive>0 from SupportBean(TheString like 'B%').std:lastevent()) then " +
                          "update set col1=(select TheString from SupportBean(TheString like 'B%').std:lastevent()), col2=(select IntPrimitive from SupportBean(TheString like 'B%').std:lastevent()) " +
                          "when matched and (select IntPrimitive>0 from SupportBean(TheString like 'C%').std:lastevent()) then " +
                          "delete";
            _epService.EPAdministrator.CreateEPL(epl);
    
            // no action tests
            SendMyEvent(eventRepresentationEnum, "X1", 1);
            _epService.EPRuntime.SendEvent(new SupportBean("A1", 0));   // ignored
            SendMyEvent(eventRepresentationEnum, "X2", 2);
            _epService.EPRuntime.SendEvent(new SupportBean("A2", 20));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, null);
    
            SendMyEvent(eventRepresentationEnum, "X3", 3);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "A2", 20 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean_A("Y1"));
            _epService.EPRuntime.SendEvent(new SupportBean("A3", 30));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, null);
    
            SendMyEvent(eventRepresentationEnum, "X4", 4);
            _epService.EPRuntime.SendEvent(new SupportBean("A4", 40));
            SendMyEvent(eventRepresentationEnum, "X5", 5);   // ignored as matched (no where clause, no B event)
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "A3", 30 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("B1", 50));
            SendMyEvent(eventRepresentationEnum, "X6", 6);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "B1", 50 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("B2", 60));
            SendMyEvent(eventRepresentationEnum, "X7", 7);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "B2", 60 } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("B2", 0));
            SendMyEvent(eventRepresentationEnum, "X8", 8);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][]{new object[]{"B2", 60}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("C1", 1));
            SendMyEvent(eventRepresentationEnum, "X9", 9);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("C1", 0));
            SendMyEvent(eventRepresentationEnum, "X10", 10);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(namedWindowStmt.GetEnumerator(), fields, new object[][] { new object[] { "A4", 40 } });
    
            _epService.EPAdministrator.DestroyAllStatements();
            foreach (string name in "MyEvent,MySchema,MyWindow".Split(',')) {
                _epService.EPAdministrator.Configuration.RemoveEventType(name, true);
            }
        }
    
        private SupportBean MakeSupportBean(string theString, int intPrimitive, double doublePrimitive) {
            SupportBean sb = new SupportBean(theString, intPrimitive);
            sb.DoublePrimitive = doublePrimitive;
            return sb;
        }
    
        private void SendMyEvent(EventRepresentationEnum eventRepresentationEnum, string in1, int in2) {
            IDictionary<String, object> theEvent = new LinkedHashMap<string, object>();
            theEvent.Put("in1", in1);
            theEvent.Put("in2", in2);
            if (eventRepresentationEnum.IsObjectArrayEvent()) {
                _epService.EPRuntime.SendEvent(theEvent.Values.ToArray(), "MyEvent");
            }
            else {
                _epService.EPRuntime.SendEvent(theEvent, "MyEvent");
            }
        }
    
        public static void IncreaseIntCopyDouble(SupportBean initialBean, SupportBean updatedBean) {
            updatedBean.IntPrimitive = initialBean.IntPrimitive + 1;
            updatedBean.DoubleBoxed = updatedBean.DoublePrimitive;
        }
    }
}
