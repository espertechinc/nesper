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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.util.support;

using static com.espertech.esper.supportregression.util.SupportMessageAssertUtil;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl.insertinto
{
    public class ExecInsertIntoPopulateEventTypeColumn : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBean_S0>();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
    
            RunAssertionTypableSubquery(epService);
            RunAssertionEnumerationSubquery(epService);
            RunAssertionTypableNewOperatorDocSample(epService);
            RunAssertionTypableAndCaseNew(epService);
            RunAssertionInvalid(epService);
        }
    
        private void RunAssertionTypableSubquery(EPServiceProvider epService) {
            TryAssertionTypableSubqueryMulti(epService, "objectarray");
            TryAssertionTypableSubqueryMulti(epService, "map");
    
            TryAssertionTypableSubquerySingleMayFilter(epService, "objectarray", true);
            TryAssertionTypableSubquerySingleMayFilter(epService, "map", true);
    
            TryAssertionTypableSubquerySingleMayFilter(epService, "objectarray", false);
            TryAssertionTypableSubquerySingleMayFilter(epService, "map", false);
    
            TryAssertionTypableSubqueryMultiFilter(epService, "objectarray");
            TryAssertionTypableSubqueryMultiFilter(epService, "map");
        }
    
        private void RunAssertionEnumerationSubquery(EPServiceProvider epService) {
            TryAssertionEnumerationSubqueryMultiMayFilter(epService, "objectarray", true);
            TryAssertionEnumerationSubqueryMultiMayFilter(epService, "map", true);
    
            TryAssertionEnumerationSubqueryMultiMayFilter(epService, "objectarray", false);
            TryAssertionEnumerationSubqueryMultiMayFilter(epService, "map", false);
    
            TryAssertionEnumerationSubquerySingleMayFilter(epService, "objectarray", true);
            TryAssertionEnumerationSubquerySingleMayFilter(epService, "map", true);
    
            TryAssertionEnumerationSubquerySingleMayFilter(epService, "objectarray", false);
            TryAssertionEnumerationSubquerySingleMayFilter(epService, "map", false);
    
            TryAssertionFragmentSingeColNamedWindow(epService);
        }
    
        private void RunAssertionTypableNewOperatorDocSample(EPServiceProvider epService) {
            TryAssertionTypableNewOperatorDocSample(epService, "objectarray");
            TryAssertionTypableNewOperatorDocSample(epService, "map");
        }
    
        private void RunAssertionTypableAndCaseNew(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create objectarray schema Nested(p0 string, p1 int)");
            epService.EPAdministrator.CreateEPL("create objectarray schema OuterType(n0 Nested)");
    
            string[] fields = "n0.p0,n0.p1".Split(',');
            epService.EPAdministrator.CreateEPL("@Name('out') " +
                    "expression computeNested {\n" +
                    "  sb => case\n" +
                    "  when IntPrimitive = 1 \n" +
                    "    then new { p0 = 'a', p1 = 1}\n" +
                    "  else new { p0 = 'b', p1 = 2 }\n" +
                    "  end\n" +
                    "}\n" +
                    "insert into OuterType select computeNested(sb) as n0 from SupportBean as sb");
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.GetStatement("out").Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"b", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"a", 1});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionTypableNewOperatorDocSample(EPServiceProvider epService, string typeType) {
            epService.EPAdministrator.CreateEPL("create " + typeType + " schema Item(name string, price double)");
            epService.EPAdministrator.CreateEPL("create " + typeType + " schema PurchaseOrder(orderId string, items Item[])");
            epService.EPAdministrator.CreateEPL("create schema TriggerEvent()");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("insert into PurchaseOrder select '001' as orderId, new {name= 'i1', price=10} as items from TriggerEvent");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(Collections.EmptyDataMap, "TriggerEvent");
            EventBean @event = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@event, "orderId,items[0].name,items[0].price".Split(','), new object[]{"001", "i1", 10d});
            EventBean[] underlying = (EventBean[]) @event.Get("items");
            Assert.AreEqual(1, underlying.Length);
            Assert.AreEqual("i1", underlying[0].Get("name"));
            Assert.AreEqual(10d, underlying[0].Get("price"));
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("Item", true);
            epService.EPAdministrator.Configuration.RemoveEventType("PurchaseOrder", true);
        }
    
        private void RunAssertionInvalid(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema N1_1(p0 int)");
            epService.EPAdministrator.CreateEPL("create schema N1_2(p1 N1_1)");
    
            // enumeration type is incompatible
            epService.EPAdministrator.CreateEPL("create schema TypeOne(sbs SupportBean[])");
            TryInvalid(epService, "insert into TypeOne select (select * from SupportBean_S0#keepall) as sbs from SupportBean_S1",
                    "Error starting statement: Incompatible type detected attempting to insert into column 'sbs' type 'SupportBean' compared to selected type 'SupportBean_S0' [insert into TypeOne select (select * from SupportBean_S0#keepall) as sbs from SupportBean_S1]");
    
            epService.EPAdministrator.CreateEPL("create schema TypeTwo(sbs SupportBean)");
            TryInvalid(epService, "insert into TypeTwo select (select * from SupportBean_S0#keepall) as sbs from SupportBean_S1",
                    "Error starting statement: Incompatible type detected attempting to insert into column 'sbs' type 'SupportBean' compared to selected type 'SupportBean_S0' [insert into TypeTwo select (select * from SupportBean_S0#keepall) as sbs from SupportBean_S1]");
    
            // typable - selected column type is incompatible
            TryInvalid(epService, "insert into N1_2 select new {p0='a'} as p1 from SupportBean",
                    "Error starting statement: Invalid assignment of column 'p0' of type 'System.String' to event property 'p0' typed as '" + Name.Clean<int>(false) + "', column and parameter types mismatch [insert into N1_2 select new {p0='a'} as p1 from SupportBean]");
    
            // typable - selected column type is not matching anything
            TryInvalid(epService, "insert into N1_2 select new {xxx='a'} as p1 from SupportBean",
                    "Error starting statement: Failed to find property 'xxx' among properties for target event type 'N1_1' [insert into N1_2 select new {xxx='a'} as p1 from SupportBean]");
        }
    
        private void TryAssertionFragmentSingeColNamedWindow(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("create schema AEvent (symbol string)");
            epService.EPAdministrator.CreateEPL("create window MyEventWindow#lastevent (e AEvent)");
            epService.EPAdministrator.CreateEPL("insert into MyEventWindow select (select * from AEvent#lastevent) as e from SupportBean(TheString = 'A')");
            epService.EPAdministrator.CreateEPL("create schema BEvent (e AEvent)");
            EPStatement stmt = epService.EPAdministrator.CreateEPL("insert into BEvent select (select e from MyEventWindow) as e from SupportBean(TheString = 'B')");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(Collections.SingletonDataMap("symbol", "GE"), "AEvent");
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            epService.EPRuntime.SendEvent(new SupportBean("B", 2));
            EventBean result = listener.AssertOneGetNewAndReset();
            EventBean fragment = (EventBean) result.Get("e");
            Assert.AreEqual("AEvent", fragment.EventType.Name);
            Assert.AreEqual("GE", fragment.Get("symbol"));
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void TryAssertionTypableSubquerySingleMayFilter(EPServiceProvider epService, string typeType, bool filter) {
            epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventZero(e0_0 string, e0_1 string)");
            epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventOne(ez EventZero)");
    
            string[] fields = "ez.e0_0,ez.e0_1".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("insert into EventOne select " +
                    "(select p00 as e0_0, p01 as e0_1 from SupportBean_S0#lastevent" +
                    (filter ? " where id >= 100" : "") + ") as ez " +
                    "from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x1", "y1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            object[] expected = filter ? new object[]{null, null} : new object[]{"x1", "y1"};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "x2", "y2"));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"x2", "y2"});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "x3", "y3"));
            epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            expected = filter ? new object[]{null, null} : new object[]{"x3", "y3"};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("EventZero", true);
            epService.EPAdministrator.Configuration.RemoveEventType("EventOne", true);
        }
    
        private void TryAssertionTypableSubqueryMulti(EPServiceProvider epService, string typeType) {
            epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventZero(e0_0 string, e0_1 string)");
            epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventOne(e1_0 string, ez EventZero[])");
    
            string[] fields = "e1_0,ez[0].e0_0,ez[0].e0_1,ez[1].e0_0,ez[1].e0_1".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("" +
                    "expression thequery {" +
                    "  (select p00 as e0_0, p01 as e0_1 from SupportBean_S0#keepall)" +
                    "} " +
                    "insert into EventOne select " +
                    "TheString as e1_0, " +
                    "thequery() as ez " +
                    "from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x1", "y1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EventBean @event = listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@event, fields, new object[]{"E1", "x1", "y1", null, null});
            SupportEventTypeAssertionUtil.AssertConsistency(@event);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "x2", "y2"));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"E2", "x1", "y1", "x2", "y2"});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("EventZero", true);
            epService.EPAdministrator.Configuration.RemoveEventType("EventOne", true);
        }
    
        private void TryAssertionTypableSubqueryMultiFilter(EPServiceProvider epService, string typeType) {
            epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventZero(e0_0 string, e0_1 string)");
            epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventOne(ez EventZero[])");
    
            string[] fields = "e0_0".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("insert into EventOne select " +
                    "(select p00 as e0_0, p01 as e0_1 from SupportBean_S0#keepall where id between 10 and 20) as ez " +
                    "from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x1", "y1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow((EventBean[]) listener.AssertOneGetNewAndReset().Get("ez"), fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(10, "x2"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(20, "x3"));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow((EventBean[]) listener.AssertOneGetNewAndReset().Get("ez"), fields, new object[][]{new object[] {"x2"}, new object[] {"x3"}});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("EventZero", true);
            epService.EPAdministrator.Configuration.RemoveEventType("EventOne", true);
        }
    
        private void TryAssertionEnumerationSubqueryMultiMayFilter(EPServiceProvider epService, string typeType, bool filter) {
            epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventOne(sbarr SupportBean_S0[])");
    
            string[] fields = "p00".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("insert into EventOne select " +
                    "(select * from SupportBean_S0#keepall " +
                    (filter ? "where 1=1" : "") + ") as sbarr " +
                    "from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EventBean[] inner = (EventBean[]) listener.AssertOneGetNewAndReset().Get("sbarr");
            EPAssertionUtil.AssertPropsPerRow(inner, fields, new object[][]{new object[] {"x1"}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "x2", "y2"));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            inner = (EventBean[]) listener.AssertOneGetNewAndReset().Get("sbarr");
            EPAssertionUtil.AssertPropsPerRow(inner, fields, new object[][]{new object[] {"x1"}, new object[] {"x2"}});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("EventOne", true);
        }
    
        private void TryAssertionEnumerationSubquerySingleMayFilter(EPServiceProvider epService, string typeType, bool filter) {
            epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventOne(sb SupportBean_S0)");
    
            string[] fields = "sb.p00".Split(',');
            EPStatement stmt = epService.EPAdministrator.CreateEPL("insert into EventOne select " +
                    "(select * from SupportBean_S0#length(2) " +
                    (filter ? "where id >= 100" : "") + ") as sb " +
                    "from SupportBean");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x1"));
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            object[] expected = filter ? new object[]{null} : new object[]{"x1"};
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, expected);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(100, "x2"));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            string received = (string) listener.AssertOneGetNewAndReset().Get(fields[0]);
            if (filter) {
                Assert.AreEqual("x2", received);
            } else {
                if (!received.Equals("x1") && !received.Equals("x2")) {
                    Assert.Fail();
                }
            }
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("EventOne", true);
        }
    }
} // end of namespace
