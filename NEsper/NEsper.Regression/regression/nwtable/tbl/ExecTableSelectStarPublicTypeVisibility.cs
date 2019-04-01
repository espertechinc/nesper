///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.subscriber;
using com.espertech.esper.util.support;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableSelectStarPublicTypeVisibility : RegressionExecution
    {
        public override void Run(EPServiceProvider epService)
        {
            foreach (var clazz in new[]
                {typeof(SupportBean), typeof(SupportBean_S0), typeof(SupportBean_S1), typeof(SupportBean_S2)})
            {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }

            var currentTime = new AtomicLong(0);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime.Get()));
            epService.EPAdministrator.CreateEPL(
                "@Name('create') create table MyTable as (\n" +
                "key string primary key,\n" +
                "totalInt sum(int),\n" +
                "p0 string,\n" +
                "winsb window(*) @Type(SupportBean),\n" +
                "totalLong sum(long),\n" +
                "p1 string,\n" +
                "winsb0 window(*) @Type(SupportBean_S0)\n" +
                ")");
            var expectedType = new object[][]
            {
                new object[]{"key", typeof(string)},
                new object[]{"totalInt", typeof(int)},
                new object[]{"p0", typeof(string)},
                new object[]{"winsb", typeof(SupportBean[])},
                new object[]{"totalLong", typeof(long)},
                new object[]{"p1", typeof(string)},
                new object[]{"winsb0", typeof(SupportBean_S0[])}
            };

            epService.EPAdministrator.CreateEPL(
                "into table MyTable " +
                "select sum(IntPrimitive) as totalInt, sum(LongPrimitive) as totalLong," +
                "window(*) as winsb from SupportBean#keepall group by TheString");
            epService.EPAdministrator.CreateEPL(
                "into table MyTable " +
                "select window(*) as winsb0 from SupportBean_S0#keepall group by p00");
            epService.EPAdministrator.CreateEPL(
                "on SupportBean_S1 " +
                "merge MyTable where p10 = key when matched then " +
                "update set p0 = p11, p1 = p12");

            var e1Sb = MakeSupportBean("G1", 10, 100);
            epService.EPRuntime.SendEvent(e1Sb); // update some aggs

            var e2Sb0 = new SupportBean_S0(5, "G1");
            epService.EPRuntime.SendEvent(e2Sb0); // update more aggs

            epService.EPRuntime.SendEvent(new SupportBean_S1(6, "G1", "a", "b")); // merge more values

            object[] rowValues = {"G1", 10, "a", new[] {e1Sb}, 100L, "b", new[] {e2Sb0}};
            RunAssertionSubqueryWindowAgg(epService, rowValues);
            RunAssertionOnSelectWindowAgg(epService, expectedType, rowValues);
            RunAssertionSubquerySelectStar(epService, rowValues);
            RunAssertionSubquerySelectWEnumMethod(epService, rowValues);
            RunAssertionIterateCreateTable(
                epService, expectedType, rowValues, epService.EPAdministrator.GetStatement("create"));
            RunAssertionJoinSelectStar(epService, expectedType, rowValues);
            RunAssertionJoinSelectStreamName(epService, expectedType, rowValues);
            RunAssertionJoinSelectStreamStarNamed(epService, expectedType, rowValues);
            RunAssertionJoinSelectStreamStarUnnamed(epService, expectedType, rowValues);
            RunAssertionInsertIntoBean(epService, rowValues);
            RunAssertionSingleRowFunc(epService, rowValues);
            RunAssertionOutputSnapshot(epService, expectedType, rowValues, currentTime);
            RunAssertionFireAndForgetSelectStar(epService, expectedType, rowValues);
            RunAssertionFireAndForgetInsertUpdateDelete(epService, expectedType);
        }

        private void RunAssertionSubqueryWindowAgg(EPServiceProvider epService, object[] rowValues)
        {
            var stmt = epService.EPAdministrator.CreateEPL(
                "select " +
                "(select window(mt.*) from MyTable as mt) as c0," +
                "(select first(mt.*) from MyTable as mt) as c1" +
                " from SupportBean_S2");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            var @event = listener.AssertOneGetNewAndReset();
            AssertEventUnd(((object[][]) @event.Get("c0"))[0], rowValues);
            AssertEventUnd(@event.Get("c1"), rowValues);
            stmt.Dispose();
        }

        private void RunAssertionOnSelectWindowAgg(
            EPServiceProvider epService, object[][] expectedType, object[] rowValues)
        {
            var stmt = epService.EPAdministrator.CreateEPL(
                "on SupportBean_S2 select " +
                "window(win.*) as c0," +
                "last(win.*) as c1, " +
                "first(win.*) as c2, " +
                "first(p1) as c3," +
                "window(p1) as c4," +
                "sorted(p1) as c5," +
                "minby(p1) as c6" +
                " from MyTable as win");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            var @event = listener.AssertOneGetNewAndReset();
            foreach (var col in "c1,c2,c6".Split(','))
            {
                AssertEventUnd(@event.Get(col), rowValues);
            }

            foreach (var col in "c0,c5".Split(','))
            {
                AssertEventUnd(((object[][]) @event.Get(col))[0], rowValues);
            }

            Assert.AreEqual("b", @event.Get("c3"));
            EPAssertionUtil.AssertEqualsExactOrder(new[] {"b"}, (string[]) @event.Get("c4"));

            stmt.Dispose();
        }

        private void RunAssertionOutputSnapshot(
            EPServiceProvider epService, object[][] expectedType, object[] rowValues, AtomicLong currentTime)
        {
            var stmt = epService.EPAdministrator.CreateEPL("select * from MyTable output snapshot every 1 second");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            AssertEventType(stmt.EventType, expectedType);

            currentTime.Set(currentTime.Get() + 1000L);
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(currentTime.Get()));
            var @event = listener.AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(@event.EventType, expectedType, @event.Underlying, rowValues);
        }

        private void RunAssertionFireAndForgetInsertUpdateDelete(EPServiceProvider epService, object[][] expectedType)
        {
            var result = epService.EPRuntime.ExecuteQuery("insert into MyTable(key) values ('dummy')");
            AssertEventType(result.EventType, expectedType);

            result = epService.EPRuntime.ExecuteQuery("delete from MyTable where key = 'dummy'");
            AssertEventType(result.EventType, expectedType);

            result = epService.EPRuntime.ExecuteQuery("update MyTable set key='dummy' where key='dummy'");
            AssertEventType(result.EventType, expectedType);
        }

        private void RunAssertionIterateCreateTable(
            EPServiceProvider epService, object[][] expectedType, object[] rowValues, EPStatement stmtCreate)
        {
            AssertEventTypeAndEvent(stmtCreate.EventType, expectedType, stmtCreate.First().Underlying, rowValues);
        }

        private void RunAssertionSingleRowFunc(EPServiceProvider epService, object[] rowValues)
        {
            // try join passing of params
            var eplJoin = "select " +
                          GetType().FullName + ".MyServiceEventBean(mt) as c0, " +
                          GetType().FullName + ".MyServiceObjectArray(mt) as c1 " +
                          "from SupportBean_S2, MyTable as mt";
            var stmtJoin = epService.EPAdministrator.CreateEPL(eplJoin);
            var listener = new SupportUpdateListener();
            stmtJoin.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            var result = listener.AssertOneGetNewAndReset();
            AssertEventUnd(result.Get("c0"), rowValues);
            AssertEventUnd(result.Get("c1"), rowValues);
            stmtJoin.Dispose();

            // try subquery
            epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction(
                "pluginServiceEventBean", GetType(), "MyServiceEventBean");
            var eplSubquery = "select (select pluginServiceEventBean(mt) from MyTable as mt) as c0 " +
                              "from SupportBean_S2";
            var stmtSubquery = epService.EPAdministrator.CreateEPL(eplSubquery);
            stmtSubquery.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            result = listener.AssertOneGetNewAndReset();
            AssertEventUnd(result.Get("c0"), rowValues);
            stmtSubquery.Dispose();
        }

        private void RunAssertionInsertIntoBean(EPServiceProvider epService, object[] rowValues)
        {
            epService.EPAdministrator.Configuration.AddEventType(typeof(MyBeanCtor));
            var epl = "insert into MyBeanCtor select * from SupportBean_S2, MyTable";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            AssertEventUnd(listener.AssertOneGetNewAndReset().Get("arr"), rowValues);

            stmt.Dispose();
        }

        private void RunAssertionSubquerySelectWEnumMethod(EPServiceProvider epService, object[] rowValues)
        {
            var epl = "select (select * from MyTable).where(v=>v.key = 'G1') as mt from SupportBean_S2";
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            Assert.AreEqual(typeof(ICollection<object[]>), stmt.EventType.GetPropertyType("mt"));

            epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            var coll = listener.AssertOneGetNewAndReset().Get("mt").Unwrap<object[]>();
            AssertEventUnd(coll.First(), rowValues);

            stmt.Dispose();
        }

        private void RunAssertionSubquerySelectStar(EPServiceProvider epService, object[] rowValues)
        {
            var eplFiltered = "select (select * from MyTable where key = 'G1') as mt from SupportBean_S2";
            RunAssertionSubquerySelectStar(epService, rowValues, eplFiltered);

            var eplUnfiltered = "select (select * from MyTable) as mt from SupportBean_S2";
            RunAssertionSubquerySelectStar(epService, rowValues, eplUnfiltered);

            // With @eventbean
            var eplEventBean = "select (select * from MyTable) @eventbean as mt from SupportBean_S2";
            var stmt = epService.EPAdministrator.CreateEPL(eplEventBean);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            Assert.AreEqual(typeof(object[][]), stmt.EventType.GetPropertyType("mt"));
            Assert.AreSame(GetTablePublicType(epService, "MyTable"), stmt.EventType.GetFragmentType("mt").FragmentType);

            epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            var @event = listener.AssertOneGetNewAndReset();
            var value = (object[][]) @event.Get("mt");
            AssertEventUnd(value[0], rowValues);
            Assert.AreSame(
                GetTablePublicType(epService, "MyTable"), ((EventBean[]) @event.GetFragment("mt"))[0].EventType);

            stmt.Dispose();
        }

        private void RunAssertionSubquerySelectStar(EPServiceProvider epService, object[] rowValues, string epl)
        {
            var stmt = epService.EPAdministrator.CreateEPL(epl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            Assert.AreEqual(typeof(object[]), stmt.EventType.GetPropertyType("mt"));

            epService.EPRuntime.SendEvent(new SupportBean_S2(0));
            var @event = listener.AssertOneGetNewAndReset();
            AssertEventUnd(@event.Get("mt"), rowValues);

            stmt.Dispose();
        }

        private void RunAssertionJoinSelectStreamStarUnnamed(
            EPServiceProvider epService, object[][] expectedType, object[] rowValues)
        {
            var joinEpl = "select mt.* from MyTable as mt, SupportBean_S2 where key = p20";
            var stmt = epService.EPAdministrator.CreateEPL(joinEpl);
            var listener = new SupportUpdateListener();
            var subscriber = new SupportSubscriberMultirowObjectArrayNStmt();
            stmt.Events += listener.Update;
            stmt.Subscriber = subscriber;

            AssertEventType(stmt.EventType, expectedType);

            // listener assertion
            epService.EPRuntime.SendEvent(new SupportBean_S2(0, "G1"));
            var @event = listener.AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(@event.EventType, expectedType, @event.Underlying, rowValues);

            // subscriber assertion
            var newData = subscriber.GetAndResetIndicateArr()[0].First;
            AssertEventUnd(newData[0][0], rowValues);

            stmt.Dispose();
        }

        private void RunAssertionJoinSelectStreamStarNamed(
            EPServiceProvider epService, object[][] expectedType, object[] rowValues)
        {
            var joinEpl = "select mt.* as mymt from MyTable as mt, SupportBean_S2 where key = p20";
            var stmt = epService.EPAdministrator.CreateEPL(joinEpl);
            var listener = new SupportUpdateListener();
            var subscriber = new SupportSubscriberMultirowObjectArrayNStmt();
            stmt.Events += listener.Update;
            stmt.Subscriber = subscriber;

            AssertEventType(stmt.EventType.GetFragmentType("mymt").FragmentType, expectedType);

            // listener assertion
            epService.EPRuntime.SendEvent(new SupportBean_S2(0, "G1"));
            var @event = listener.AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(
                @event.EventType.GetFragmentType("mymt").FragmentType,
                expectedType, @event.Get("mymt"), rowValues);

            // subscriber assertion
            var newData = subscriber.GetAndResetIndicateArr()[0].First;
            AssertEventUnd(newData[0][0], rowValues);

            stmt.Dispose();
        }

        private void RunAssertionJoinSelectStreamName(
            EPServiceProvider epService, object[][] expectedType, object[] rowValues)
        {
            var joinEpl = "select mt from MyTable as mt, SupportBean_S2 where key = p20";
            var stmt = epService.EPAdministrator.CreateEPL(joinEpl);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;

            AssertEventType(stmt.EventType.GetFragmentType("mt").FragmentType, expectedType);

            epService.EPRuntime.SendEvent(new SupportBean_S2(0, "G1"));
            var @event = listener.AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(
                @event.EventType.GetFragmentType("mt").FragmentType,
                expectedType, @event.Get("mt"), rowValues);

            stmt.Dispose();
        }

        private void RunAssertionJoinSelectStar(
            EPServiceProvider epService, object[][] expectedType, object[] rowValues)
        {
            var joinEpl = "select * from MyTable, SupportBean_S2 where key = p20";
            var stmt = epService.EPAdministrator.CreateEPL(joinEpl);
            var listener = new SupportUpdateListener();
            var subscriber = new SupportSubscriberMultirowObjectArrayNStmt();
            stmt.Events += listener.Update;
            stmt.Subscriber = subscriber;

            AssertEventType(stmt.EventType.GetFragmentType("stream_0").FragmentType, expectedType);

            // listener assertion
            epService.EPRuntime.SendEvent(new SupportBean_S2(0, "G1"));
            var @event = listener.AssertOneGetNewAndReset();
            AssertEventTypeAndEvent(
                @event.EventType.GetFragmentType("stream_0").FragmentType,
                expectedType, @event.Get("stream_0"), rowValues);

            // subscriber assertion
            var newData = subscriber.GetAndResetIndicateArr()[0].First;
            AssertEventUnd(newData[0][0], rowValues);

            stmt.Dispose();
        }

        private void RunAssertionFireAndForgetSelectStar(
            EPServiceProvider epService, object[][] expectedType, object[] rowValues)
        {
            var result = epService.EPRuntime.ExecuteQuery("select * from MyTable where key = 'G1'");
            AssertEventTypeAndEvent(result.EventType, expectedType, result.Array[0].Underlying, rowValues);
        }

        private void AssertEventTypeAndEvent(
            EventType eventType, object[][] expectedType, object underlying, object[] expectedValues)
        {
            AssertEventType(eventType, expectedType);
            AssertEventUnd(underlying, expectedValues);
        }

        private void AssertEventUnd(object underlying, object[] expectedValues)
        {
            var und = (object[]) underlying;
            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, und);
        }

        private void AssertEventType(EventType eventType, object[][] expectedType)
        {
            SupportEventTypeAssertionUtil.AssertEventTypeProperties(
                expectedType, eventType, SupportEventTypeAssertionEnum.NAME, SupportEventTypeAssertionEnum.TYPE);
        }

        private SupportBean MakeSupportBean(string theString, int intPrimitive, int longPrimitive)
        {
            var supportBean = new SupportBean(theString, intPrimitive);
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

        public EventType GetTablePublicType(EPServiceProvider epService, string tableName)
        {
            return ((EPServiceProviderSPI) epService).ServicesContext.TableService.GetTableMetadata(tableName)
                .PublicEventType;
        }

        public sealed class MyBeanCtor
        {
            public MyBeanCtor(SupportBean_S2 sb, object[] arr)
            {
                Sb = sb;
                Arr = arr;
            }

            public SupportBean_S2 Sb { get; }

            public object[] Arr { get; }
        }
    }
} // end of namespace