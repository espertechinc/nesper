///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.regression.epl
{
    [TestFixture]
    public class TestInsertIntoPopulateEventTypeColumn 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _epService = EPServiceProviderManager.GetDefaultProvider(SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
            _listener = new SupportUpdateListener();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S0));
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean_S1));
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestTypableSubquery()
        {
            RunAssertionTypableSubqueryMulti("objectarray");
            RunAssertionTypableSubqueryMulti("map");
    
            RunAssertionTypableSubquerySingleMayFilter("objectarray", true);
            RunAssertionTypableSubquerySingleMayFilter("map", true);
    
            RunAssertionTypableSubquerySingleMayFilter("objectarray", false);
            RunAssertionTypableSubquerySingleMayFilter("map", false);
    
            RunAssertionTypableSubqueryMultiFilter("objectarray");
            RunAssertionTypableSubqueryMultiFilter("map");
        }
    
        [Test]
        public void TestEnumerationSubquery()
        {
            RunAssertionEnumerationSubqueryMultiMayFilter("objectarray", true);
            RunAssertionEnumerationSubqueryMultiMayFilter("map", true);
    
            RunAssertionEnumerationSubqueryMultiMayFilter("objectarray", false);
            RunAssertionEnumerationSubqueryMultiMayFilter("map", false);
    
            RunAssertionEnumerationSubquerySingleMayFilter("objectarray", true);
            RunAssertionEnumerationSubquerySingleMayFilter("map", true);
    
            RunAssertionEnumerationSubquerySingleMayFilter("objectarray", false);
            RunAssertionEnumerationSubquerySingleMayFilter("map", false);

            RunAssertionFragmentSingeColNamedWindow();
        }
    
        [Test]
        public void TestTypableNewOperatorDocSample()
        {
            RunAssertionTypableNewOperatorDocSample("objectarray");
            RunAssertionTypableNewOperatorDocSample("map");
        }
    
        [Test]
        public void TestTypableAndCaseNew()
        {
            _epService.EPAdministrator.CreateEPL("create objectarray schema Nested(p0 string, p1 int)");
            _epService.EPAdministrator.CreateEPL("create objectarray schema OuterType(n0 Nested)");
    
            var fields = "n0.p0,n0.p1".Split(',');
            _epService.EPAdministrator.CreateEPL("@Name('out') " +
                    "expression computeNested {\n" +
                    "  sb => case\n" +
                    "  when IntPrimitive = 1 \n" +
                    "    then new { p0 = 'a', p1 = 1}\n" +
                    "  else new { p0 = 'b', p1 = 2 }\n" +
                    "  end\n" +
                    "}\n" +
                    "insert into OuterType select computeNested(sb) as n0 from SupportBean as sb");
            _epService.EPAdministrator.GetStatement("out").Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"b", 2});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"a", 1});
        }
    
        public void RunAssertionTypableNewOperatorDocSample(String typeType)
        {
            _epService.EPAdministrator.CreateEPL("create " + typeType + " schema Item(name string, price double)");
            _epService.EPAdministrator.CreateEPL("create " + typeType + " schema PurchaseOrder(orderId string, items Item[])");
            _epService.EPAdministrator.CreateEPL("create schema TriggerEvent()");
            var stmt = _epService.EPAdministrator.CreateEPL("insert into PurchaseOrder select '001' as orderId, new {name= 'i1', price=10} as items from TriggerEvent");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(Collections.GetEmptyMap<string,object>(), "TriggerEvent");
            var @event = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@event, "orderId,items[0].name,items[0].price".Split(','), new Object[] {"001", "i1", 10d});
            var underlying = (EventBean[]) @event.Get("items");
            Assert.AreEqual(1, underlying.Length);
            Assert.AreEqual("i1", underlying[0].Get("name"));
            Assert.AreEqual(10d, underlying[0].Get("price"));
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("Item", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("PurchaseOrder", true);
        }
    
        [Test]
        public void TestInvalid()
        {
            _epService.EPAdministrator.CreateEPL("create schema N1_1(p0 int)");
            _epService.EPAdministrator.CreateEPL("create schema N1_2(p1 N1_1)");
    
            // enumeration type is incompatible
            _epService.EPAdministrator.CreateEPL("create schema TypeOne(sbs SupportBean[])");
            TryInvalid("insert into TypeOne select (select * from SupportBean_S0.win:keepall()) as sbs from SupportBean_S1",
                    "Error starting statement: Incompatible type detected attempting to insert into column 'sbs' type 'SupportBean' compared to selected type 'SupportBean_S0' [insert into TypeOne select (select * from SupportBean_S0.win:keepall()) as sbs from SupportBean_S1]");
    
            _epService.EPAdministrator.CreateEPL("create schema TypeTwo(sbs SupportBean)");
            TryInvalid("insert into TypeTwo select (select * from SupportBean_S0.win:keepall()) as sbs from SupportBean_S1",
                    "Error starting statement: Incompatible type detected attempting to insert into column 'sbs' type 'SupportBean' compared to selected type 'SupportBean_S0' [insert into TypeTwo select (select * from SupportBean_S0.win:keepall()) as sbs from SupportBean_S1]");
    
            // typable - selected column type is incompatible
            TryInvalid("insert into N1_2 select new {p0='a'} as p1 from SupportBean",
                    "Error starting statement: Invalid assignment of column 'p0' of type 'System.String' to event property 'p0' typed as '" + typeof(int).FullName + "', column and parameter types mismatch [insert into N1_2 select new {p0='a'} as p1 from SupportBean]");
    
            // typable - selected column type is not matching anything
            TryInvalid("insert into N1_2 select new {xxx='a'} as p1 from SupportBean",
                    "Error starting statement: Failed to find property 'xxx' among properties for target event type 'N1_1' [insert into N1_2 select new {xxx='a'} as p1 from SupportBean]");
        }

        private void RunAssertionFragmentSingeColNamedWindow()
        {
            _epService.EPAdministrator.CreateEPL("create schema AEvent (symbol string)");
            _epService.EPAdministrator.CreateEPL("create window MyEventWindow.std:lastevent() (e AEvent)");
            _epService.EPAdministrator.CreateEPL("insert into MyEventWindow select (select * from AEvent.std:lastevent()) as e from SupportBean(TheString = 'A')");
            _epService.EPAdministrator.CreateEPL("create schema BEvent (e AEvent)");
            var stmt = _epService.EPAdministrator.CreateEPL("insert into BEvent select (select e from MyEventWindow) as e from SupportBean(TheString = 'B')");
            stmt.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(Collections.SingletonDataMap("symbol", "GE"), "AEvent");
            _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("B", 2));
            var result = _listener.AssertOneGetNewAndReset();
            var fragment = (EventBean)result.Get("e");
            Assert.AreEqual("AEvent", fragment.EventType.Name);
            Assert.AreEqual("GE", fragment.Get("symbol"));
        }
    
        private void TryInvalid(String epl, String message)
        {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        private void RunAssertionTypableSubquerySingleMayFilter(String typeType, bool filter)
        {
            _epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventZero(e0_0 string, e0_1 string)");
            _epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventOne(ez EventZero)");
    
            var fields = "ez.e0_0,ez.e0_1".Split(',');
            var stmt = _epService.EPAdministrator.CreateEPL("insert into EventOne select " +
                    "(select p00 as e0_0, p01 as e0_1 from SupportBean_S0.std:lastevent()" +
                    (filter ? " where id >= 100" : "") + ") as ez " +
                    "from SupportBean");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x1", "y1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            var expected = filter ? new Object[] {null, null} : new Object[] {"x1", "y1"};
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, expected);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "x2", "y2"));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"x2", "y2"});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "x3", "y3"));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            expected = filter ? new Object[] {null, null} : new Object[] {"x3", "y3"};
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, expected);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("EventZero", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("EventOne", true);
        }
    
        private void RunAssertionTypableSubqueryMulti(String typeType)
        {
            _epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventZero(e0_0 string, e0_1 string)");
            _epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventOne(e1_0 string, ez EventZero[])");
    
            var fields = "e1_0,ez[0].e0_0,ez[0].e0_1,ez[1].e0_0,ez[1].e0_1".Split(',');
            var stmt = _epService.EPAdministrator.CreateEPL("" +
                    "expression thequery {" +
                    "  (select p00 as e0_0, p01 as e0_1 from SupportBean_S0.win:keepall())" +
                    "} " +
                    "insert into EventOne select " +
                    "TheString as e1_0, " +
                    "thequery() as ez " +
                    "from SupportBean");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x1", "y1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            var @event = _listener.AssertOneGetNewAndReset();
            EPAssertionUtil.AssertProps(@event, fields, new Object[] {"E1", "x1", "y1", null, null});
            EventTypeAssertionUtil.AssertConsistency(@event);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "x2", "y2"));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E2", "x1", "y1", "x2", "y2"});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("EventZero", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("EventOne", true);
        }
    
        private void RunAssertionTypableSubqueryMultiFilter(String typeType)
        {
            _epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventZero(e0_0 string, e0_1 string)");
            _epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventOne(ez EventZero[])");
    
            var fields = "e0_0".Split(',');
            var stmt = _epService.EPAdministrator.CreateEPL("insert into EventOne select " +
                    "(select p00 as e0_0, p01 as e0_1 from SupportBean_S0.win:keepall() where id between 10 and 20) as ez " +
                    "from SupportBean");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x1", "y1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow((EventBean[]) _listener.AssertOneGetNewAndReset().Get("ez"), fields, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(10, "x2"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(20, "x3"));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow((EventBean[]) _listener.AssertOneGetNewAndReset().Get("ez"), fields, new Object[][] { new Object[] {"x2"}, new Object[] {"x3"}});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("EventZero", true);
            _epService.EPAdministrator.Configuration.RemoveEventType("EventOne", true);
        }
    
        private void RunAssertionEnumerationSubqueryMultiMayFilter(String typeType, bool filter)
        {
            _epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventOne(sbarr SupportBean_S0[])");
    
            var fields = "p00".Split(',');
            var stmt = _epService.EPAdministrator.CreateEPL("insert into EventOne select " +
                    "(select * from SupportBean_S0.win:keepall()" +
                    (filter ? "where 1=1" : "") + ") as sbarr " +
                    "from SupportBean");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            var inner = (EventBean[]) _listener.AssertOneGetNewAndReset().Get("sbarr");
            EPAssertionUtil.AssertPropsPerRow(inner, fields, new Object[][] { new Object[] {"x1"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "x2", "y2"));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            inner = (EventBean[]) _listener.AssertOneGetNewAndReset().Get("sbarr");
            EPAssertionUtil.AssertPropsPerRow(inner, fields, new Object[][] { new Object[] {"x1"}, new Object[] {"x2"}});
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("EventOne", true);
        }
    
        private void RunAssertionEnumerationSubquerySingleMayFilter(String typeType, bool filter)
        {
            _epService.EPAdministrator.CreateEPL("create " + typeType + " schema EventOne(sb SupportBean_S0)");
    
            var fields = "sb.p00".Split(',');
            
            _epService.EPAdministrator
                .CreateEPL(string.Format("insert into EventOne select (select * from SupportBean_S0.win:length(2) {0}) as sb " + "from SupportBean", (filter ? "where id >= 100" : "")))
                .AddListener(_listener);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "x1"));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            var expected = filter ? new Object[] {null} : new Object[] {"x1"};
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, expected);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(100, "x2"));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));

            var received = (string) _listener.AssertOneGetNewAndReset().Get(fields[0]);
            if (filter)
            {
                Assert.AreEqual("x2", received);
            }
            else
            {
                if ((received != "x1") && (received != "x2"))
                {
                    Assert.Fail();
                }
            }

            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("EventOne", true);
        }
    }
}
