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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.regression.client;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableSelectStarPublicTypeVisibility
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private MySubscriberMultirowObjectArr _subscriber;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            foreach (Type clazz in new Type[] {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1), typeof(SupportBean_S2)}) {
                _epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
            _listener = new SupportUpdateListener();
            _subscriber = new MySubscriberMultirowObjectArr();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listener = null;
            _subscriber = null;
        }
    
        [Test]
        public void TestSelectPublicTypeAndUnderlying()
        {
            AtomicLong currentTime = new AtomicLong(0);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime.Get()));
            _epService.EPAdministrator.CreateEPL("@Name('create') create table MyTable as (\n" +
                    "key string primary key,\n" +
                    "totalInt sum(int),\n" +
                    "p0 string,\n" +
                    "winsb window(*) @type(SupportBean),\n" +
                    "totalLong sum(long),\n" +
                    "p1 string,\n" +
                    "winsb0 window(*) @type(SupportBean_S0)\n" +
                    ")");
            object[][] expectedType = new object[][]{
                    new object[] {"key", typeof(string)},
                    new object[] {"totalInt", typeof(int?)},
                    new object[] {"p0", typeof(string)},
                    new object[] {"winsb", typeof(SupportBean[])},
                    new object[] {"totalLong", typeof(long?)},
                    new object[] {"p1", typeof(string)},
                    new object[] {"winsb0", typeof(SupportBean_S0[])},
            };
    
            _epService.EPAdministrator.CreateEPL("into table MyTable " +
                    "select sum(IntPrimitive) as totalInt, sum(LongPrimitive) as totalLong," +
                    "window(*) as winsb from SupportBean.win:keepall() group by TheString");
            _epService.EPAdministrator.CreateEPL("into table MyTable " +
                    "select window(*) as winsb0 from SupportBean_S0.win:keepall() group by p00");
            _epService.EPAdministrator.CreateEPL("on SupportBean_S1 " +
                    "merge MyTable where p10 = key when matched then " +
                    "update set p0 = p11, p1 = p12");
    
            SupportBean e1_sb = MakeSupportBean("G1", 10, 100);
            _epService.EPRuntime.SendEvent(e1_sb); // update some aggs
    
            SupportBean_S0 e2_sb0 = new SupportBean_S0(5, "G1");
            _epService.EPRuntime.SendEvent(e2_sb0); // update more aggs
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(6, "G1", "a", "b")); // merge more values
    
            object[] rowValues = {"G1", 10, "a", new SupportBean[] {e1_sb}, 100L, "b", new SupportBean_S0[] {e2_sb0}};
            RunAssertionSubqueryWindowAgg(rowValues);
            RunAssertionOnSelectWindowAgg(expectedType, rowValues);
            RunAssertionSubquerySelectStar(rowValues);
            RunAssertionSubquerySelectWEnumMethod(rowValues);
            RunAssertionIterateCreateTable(expectedType, rowValues, _epService.EPAdministrator.GetStatement("create"));
            RunAssertionJoinSelectStar(expectedType, rowValues);
            RunAssertionJoinSelectStreamName(expectedType, rowValues);
            RunAssertionJoinSelectStreamStarNamed(expectedType, rowValues);
            RunAssertionJoinSelectStreamStarUnnamed(expectedType, rowValues);
            RunAssertionInsertIntoBean(rowValues);
            RunAssertionSingleRowFunc(rowValues);
            RunAssertionOutputSnapshot(expectedType, rowValues, currentTime);
            RunAssertionFireAndForgetSelectStar(expectedType, rowValues);
            RunAssertionFireAndForgetInsertUpdateDelete(expectedType);
        }
    
        private void RunAssertionSubqueryWindowAgg(object[] rowValues)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select " +
                    "(select window(mt.*) from MyTable as mt) as c0," +
                    "(select first(mt.*) from MyTable as mt) as c1" +
                    " from SupportBean_S2");
            stmt.AddListener(_listener);
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            EventBean @event = _listener.AssertOneGetNewAndReset();
            AssertEventUnd(((object[][])@event.Get("c0"))[0], rowValues);
            AssertEventUnd(@event.Get("c1"), rowValues);
            stmt.Dispose();
        }
    
        private void RunAssertionOnSelectWindowAgg(object[][] expectedType, object[] rowValues)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("on SupportBean_S2 select " +
                    "window(win.*) as c0," +
                    "last(win.*) as c1, " +
                    "first(win.*) as c2, " +
                    "first(p1) as c3," +
                    "window(p1) as c4," +
                    "sorted(p1) as c5," +
                    "minby(p1) as c6" +
                    " from MyTable as win");
            stmt.AddListener(_listener);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            EventBean @event = _listener.AssertOneGetNewAndReset();
            foreach (string col in "c1,c2,c6".Split(',')) {
                AssertEventUnd(@event.Get(col), rowValues);
            }
            foreach (string col in "c0,c5".Split(',')) {
                AssertEventUnd(((object[][])@event.Get(col))[0], rowValues);
            }
            Assert.AreEqual("b", @event.Get("c3"));
            EPAssertionUtil.AssertEqualsExactOrder(new string[]{"b"}, (string[]) @event.Get("c4"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionOutputSnapshot(object[][] expectedType, object[] rowValues, AtomicLong currentTime)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from MyTable output snapshot every 1 second");
            stmt.AddListener(_listener);
            AssertEventType(stmt.EventType, expectedType);
    
            currentTime.Set(currentTime.Get() + 1000L);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime.Get()));
            EventBean @event = _listener.AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(@event.EventType, expectedType, @event.Underlying, rowValues);
        }
    
        private void RunAssertionFireAndForgetInsertUpdateDelete(object[][] expectedType)
        {
            EPOnDemandQueryResult result = _epService.EPRuntime.ExecuteQuery("insert into MyTable(key) values ('dummy')");
            AssertEventType(result.EventType, expectedType);
    
            result = _epService.EPRuntime.ExecuteQuery("delete from MyTable where key = 'dummy'");
            AssertEventType(result.EventType, expectedType);
    
            result = _epService.EPRuntime.ExecuteQuery("update MyTable set key='dummy' where key='dummy'");
            AssertEventType(result.EventType, expectedType);
        }
    
        private void RunAssertionIterateCreateTable(object[][] expectedType, object[] rowValues, EPStatement stmtCreate)
        {
            AssertEventTypeAndEvent(stmtCreate.EventType, expectedType, stmtCreate.First().Underlying, rowValues);
        }
    
        private void RunAssertionSingleRowFunc(object[] rowValues)
        {
            // try join passing of params
            string eplJoin = "select " +
                    this.GetType().FullName + ".MyServiceEventBean(mt) as c0, " +
                    this.GetType().FullName + ".MyServiceObjectArray(mt) as c1 " +
                    "from SupportBean_S2, MyTable as mt";
            EPStatement stmtJoin = _epService.EPAdministrator.CreateEPL(eplJoin);
            stmtJoin.AddListener(_listener);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            EventBean result = _listener.AssertOneGetNewAndReset();
            AssertEventUnd(result.Get("c0"), rowValues);
            AssertEventUnd(result.Get("c1"), rowValues);
            stmtJoin.Dispose();
    
            // try subquery
            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("pluginServiceEventBean", this.GetType().FullName, "MyServiceEventBean");
            string eplSubquery = "select (select pluginServiceEventBean(mt) from MyTable as mt) as c0 " +
                    "from SupportBean_S2";
            EPStatement stmtSubquery = _epService.EPAdministrator.CreateEPL(eplSubquery);
            stmtSubquery.AddListener(_listener);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            result = _listener.AssertOneGetNewAndReset();
            AssertEventUnd(result.Get("c0"), rowValues);
            stmtSubquery.Dispose();
        }
    
        private void RunAssertionInsertIntoBean(object[] rowValues)
        {
            _epService.EPAdministrator.Configuration.AddEventType(typeof(MyBeanCtor));
            string epl = "insert into MyBeanCtor select * from SupportBean_S2, MyTable";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_listener);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            AssertEventUnd(_listener.AssertOneGetNewAndReset().Get("arr"), rowValues);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSubquerySelectWEnumMethod(object[] rowValues)
        {
            string epl = "select (select * from MyTable).where(v=>v.key = 'G1') as mt from SupportBean_S2";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_listener);

            Assert.AreEqual(typeof(ICollection<object>), stmt.EventType.GetPropertyType("mt"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            ICollection<object> coll = (ICollection<object>)_listener.AssertOneGetNewAndReset().Get("mt");
            AssertEventUnd(coll.First(), rowValues);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSubquerySelectStar(object[] rowValues)
        {
            string eplFiltered = "select (select * from MyTable where key = 'G1') as mt from SupportBean_S2";
            RunAssertionSubquerySelectStar(rowValues, eplFiltered);
    
            string eplUnfiltered = "select (select * from MyTable) as mt from SupportBean_S2";
            RunAssertionSubquerySelectStar(rowValues, eplUnfiltered);
    
            // With @eventbean
            string eplEventBean = "select (select * from MyTable) @eventbean as mt from SupportBean_S2";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(eplEventBean);
            stmt.AddListener(_listener);
            Assert.AreEqual(typeof(object[][]), stmt.EventType.GetPropertyType("mt"));
            Assert.AreSame(GetTablePublicType("MyTable"), stmt.EventType.GetFragmentType("mt").FragmentType);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            EventBean @event = _listener.AssertOneGetNewAndReset();
            object[][] value = (object[][]) @event.Get("mt");
            AssertEventUnd(value[0], rowValues);
            Assert.AreSame(GetTablePublicType("MyTable"), ((EventBean[]) @event.GetFragment("mt"))[0].EventType);
    
            stmt.Dispose();
        }
    
        private void RunAssertionSubquerySelectStar(object[] rowValues, string epl)
        {
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(epl);
            stmt.AddListener(_listener);
    
            Assert.AreEqual(typeof(object[]), stmt.EventType.GetPropertyType("mt"));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            EventBean @event = _listener.AssertOneGetNewAndReset();
            AssertEventUnd(@event.Get("mt"), rowValues);
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinSelectStreamStarUnnamed(object[][] expectedType, object[] rowValues)
        {
            string joinEpl = "select mt.* from MyTable as mt, SupportBean_S2 where key = p20";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinEpl);
            stmt.AddListener(_listener);
            stmt.Subscriber = _subscriber;
    
            AssertEventType(stmt.EventType, expectedType);
    
            // listener assertion
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0, "G1"));
            EventBean @event = _listener.AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(@event.EventType, expectedType, @event.Underlying, rowValues);
    
            // subscriber assertion
            object[][] newData = _subscriber.GetAndResetIndicateArr()[0].First;
            AssertEventUnd(newData[0][0], rowValues);
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinSelectStreamStarNamed(object[][] expectedType, object[] rowValues)
        {
            string joinEpl = "select mt.* as mymt from MyTable as mt, SupportBean_S2 where key = p20";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinEpl);
            stmt.AddListener(_listener);
            stmt.Subscriber = _subscriber;
    
            AssertEventType(stmt.EventType.GetFragmentType("mymt").FragmentType, expectedType);
    
            // listener assertion
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0, "G1"));
            EventBean @event = _listener.AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(@event.EventType.GetFragmentType("mymt").FragmentType,
                    expectedType, @event.Get("mymt"), rowValues);
    
            // subscriber assertion
            object[][] newData = _subscriber.GetAndResetIndicateArr()[0].First;
            AssertEventUnd(newData[0][0], rowValues);
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinSelectStreamName(object[][] expectedType, object[] rowValues)
        {
            string joinEpl = "select mt from MyTable as mt, SupportBean_S2 where key = p20";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinEpl);
            stmt.AddListener(_listener);
    
            AssertEventType(stmt.EventType.GetFragmentType("mt").FragmentType, expectedType);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0, "G1"));
            EventBean @event = _listener.AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(@event.EventType.GetFragmentType("mt").FragmentType,
                    expectedType, @event.Get("mt"), rowValues);
    
            stmt.Dispose();
        }
    
        private void RunAssertionJoinSelectStar(object[][] expectedType, object[] rowValues)
        {
            string joinEpl = "select * from MyTable, SupportBean_S2 where key = p20";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(joinEpl);
            stmt.AddListener(_listener);
            stmt.Subscriber = _subscriber;
    
            AssertEventType(stmt.EventType.GetFragmentType("stream_0").FragmentType, expectedType);
    
            // listener assertion
            _epService.EPRuntime.SendEvent(new SupportBean_S2(0, "G1"));
            EventBean @event = _listener.AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(@event.EventType.GetFragmentType("stream_0").FragmentType,
                    expectedType, @event.Get("stream_0"), rowValues);
    
            // subscriber assertion
            object[][] newData = _subscriber.GetAndResetIndicateArr()[0].First;
            AssertEventUnd(newData[0][0], rowValues);
    
            stmt.Dispose();
        }
    
        private void RunAssertionFireAndForgetSelectStar(object[][] expectedType, object[] rowValues)
        {
            EPOnDemandQueryResult result = _epService.EPRuntime.ExecuteQuery("select * from MyTable where key = 'G1'");
            AssertEventTypeAndEvent(result.EventType, expectedType, result.Array[0].Underlying, rowValues);
        }
    
        private void AssertEventTypeAndEvent(EventType eventType, object[][] expectedType, object underlying, object[] expectedValues)
        {
            AssertEventType(eventType, expectedType);
            AssertEventUnd(underlying, expectedValues);
        }
    
        private void AssertEventUnd(object underlying, object[] expectedValues)
        {
            object[] und = (object[]) underlying;
            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, und);
        }
    
        private void AssertEventType(EventType eventType, object[][] expectedType)
        {
            EventTypeAssertionUtil.AssertEventTypeProperties(expectedType, eventType, EventTypeAssertionEnum.NAME, EventTypeAssertionEnum.TYPE);
        }
    
        private SupportBean MakeSupportBean(string theString, int intPrimitive, int longPrimitive)
        {
            SupportBean supportBean = new SupportBean(theString, intPrimitive);
            supportBean.LongPrimitive = longPrimitive;
            return supportBean;
        }
    
        public static object[] MyServiceEventBean(EventBean @event)
        {
            return (object[]) @event.Underlying;
        }
    
        public static object[] MyServiceObjectArray(object[] data)
        {
            return data;
        }
    
        public EventType GetTablePublicType(string tableName)
        {
            return ((EPServiceProviderSPI) _epService).ServicesContext.TableService.GetTableMetadata(tableName).PublicEventType;
        }
    
        private class MyBeanCtor
        {
            public MyBeanCtor(SupportBean_S2 sb, object[] arr)
            {
                Sb = sb;
                Arr = arr;
            }

            public SupportBean_S2 Sb { get; private set; }

            public object[] Arr { get; private set; }
        }
    }
}
